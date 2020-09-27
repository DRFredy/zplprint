
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPL
{
  public class ZPLPrintingService : IZPLPrintingService
  {
    /// <summary>
    /// Obtains the ZPL code for printing the parameterized image
    /// </summary>
    /// <param name="bitmap">The image to be ZPL encoded</param>
    /// <returns>A string containing ZPL code</returns>
    public async Task<string> GetImageZPLEncoded(Bitmap bitmap)
    {
        try
        {
            StringBuilder sbZPLs = new StringBuilder();

            int posX = 0;
            int posY = 0;
            string zplEncodedString = ZPLConv.GetZPLImage(bitmap, posX, posY);

            sbZPLs.AppendLine();

            sbZPLs.AppendLine(await Task.FromResult(zplEncodedString));

            return sbZPLs.ToString();
        }
        catch //(Exception ex) 
        {
            //handle
        }

        return null;
    }

    /// <summary>
    /// Sends the ZPL code to the specified Zebra printer
    /// </summary>        
    /// <param name="printerIPAddress">The IP address to contact the printer</param>
    /// <param name="printerPort">The port to connect with the printer</param>
    /// <param name="zplString">Raw zpl string</param>
    /// <returns>A boolean indicatig true the sending has been executed successfully.  Otherwise it returns false.</returns>
    public async Task<bool> SendDataToPrinter(string printerIPAddress, int printerPort, string zplString)
    {
        try
        {
            using (System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient())
            {
                await client.ConnectAsync(printerIPAddress, printerPort);

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(client.GetStream()))
                {
                    writer.Write(zplString); writer.Flush();

                    writer.Close();
                    client.Close();
                }
            }

            return true;
        }
        catch //(Exception ex)
        {
            throw;
        }
    }
  }

  /// <summary>
  /// ZPLII image conversion (code from git Labelary)
  /// </summary>
  internal class ZPLConv
    {
        public ZPLConv()
        { }

        /// <summary>
        /// Builds the string for the converted ZB64 to be used in tha ZPLII command string.
        /// </summary>
        /// <param name="srcbitmap">Provided Image</param>
        /// <param name="posx">Horizontal posistion</param>
        /// <param name="posy">Vertical posistion</param>
        /// <returns>A string containing the zpl encoded data</returns>
        internal static string GetZPLImage(Bitmap srcbitmap, int posx, int posy)
        {
            try
            {
                Rectangle dim = new Rectangle(Point.Empty, srcbitmap.Size);
                int rowdata = ((dim.Width + 7) / 8);
                int bytes = rowdata * dim.Height;

                using (Bitmap bmpCompressed = srcbitmap.Clone(dim, PixelFormat.Format1bppIndexed))
                {
                    StringBuilder result = new StringBuilder();
                    result.AppendLine("^XA");
                    result.AppendFormat("^FO{0},{1}^GFA,{2},{2},{3},", posx, posy, rowdata * dim.Height, rowdata);
                    byte[][] imageData = ConvertImageBinary(dim, rowdata, bmpCompressed);

                    byte[] previousRow = null;
                    foreach (byte[] row in imageData)
                    {
                        AppendLine(row, previousRow, result);
                        previousRow = row;
                    }
                    result.Append(@"^FS");
                    result.AppendLine("^XZ");

                    // NOTE: You can save the encoded contents into a file to copy/paste its contents into http://labelary.com/viewer.html
                    // In order to do that, just uncomment the following line.
                    //bmpCompressed.Save(@"c:\temp\archivo.png", ImageFormat.Png);

                    return result.ToString();
                }
            }
            catch //(Exception ex)
            {
                //handle
            }

            return null;
        }

        /// <summary>
        /// Converts the image into a byte array (pointer) and converts the image byte-by-byte while inverting the color of the image color for printing.
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="stride"></param>
        /// <param name="bmpimage"></param>
        /// <returns>A matrix</returns>
        private static byte[][] ConvertImageBinary(Rectangle dim, int stride, Bitmap bmpimage)
        {
            byte[][] imagebytes;
            var data = bmpimage.LockBits(dim, ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);

            try
            {
                // This is required to perform operations with pointers. This is only working with a locked bitmap in memory so it is "safe".
                unsafe
                {
                    byte* pixelData = (byte*)data.Scan0.ToPointer();
                    byte mask = (byte)(0xff << (data.Stride * 8 - dim.Width));
                    imagebytes = new byte[dim.Height][];

                    for (int x = 0; x < dim.Height; x++)
                    {
                        byte* rowStart = pixelData + x * data.Stride;
                        imagebytes[x] = new byte[stride];

                        for (int y = 0; y < stride; y++)
                        {
                            byte invert = (byte)(0xff ^ rowStart[y]);
                            invert = (y == stride - 1) ? (byte)(invert & mask) : invert;
                            imagebytes[x][y] = invert;
                        }
                    }
                }
            
                return imagebytes;
            }
            catch //(Exception ex)
            {
                //handle
            }
            finally
            {
                bmpimage.UnlockBits(data);
            }
            
            return new byte[][] {};
        }

        /// <summary>
        /// Converts byte to ZB64 and appends to current string.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="previousRow"></param>
        /// <param name="zb64stream"></param>
        private static void AppendLine(byte[] row, byte[] previousRow, StringBuilder zb64stream)
        {
            try
            {
                if (row.All(r => r == 0))
                {
                    zb64stream.Append(",");
                    return;
                }

                if (row.All(r => r == 0xff))
                {
                    zb64stream.Append("!");
                    return;
                }

                if (previousRow != null && MatchByteArray(row, previousRow))
                {
                    zb64stream.Append(":");
                    return;
                }

                byte[] nibbles = new byte[row.Length * 2];
                for (int i = 0; i < row.Length; i++)
                {
                    nibbles[i * 2] = (byte)(row[i] >> 4);
                    nibbles[i * 2 + 1] = (byte)(row[i] & 0x0f);
                }

                for (int i = 0; i < nibbles.Length; i++)
                {
                    byte pixel = nibbles[i];

                    int repcount = 0;
                    for (int j = i; j < nibbles.Length && repcount <= 400; j++)
                    {
                        if (pixel == nibbles[j])
                        {
                            repcount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (repcount > 2)
                    {
                        if (repcount == nibbles.Length - i
                            && (pixel == 0 || pixel == 0xf))
                        {
                            if (pixel == 0)
                            {
                                if (i % 2 == 1)
                                {
                                    zb64stream.Append("0");
                                }
                                zb64stream.Append(",");
                                return;
                            }
                            else if (pixel == 0xf)
                            {
                                if (i % 2 == 1)
                                {
                                    zb64stream.Append("F");
                                }
                                zb64stream.Append("!");
                                return;
                            }
                        }
                        else
                        {
                            zb64stream.Append(ConvertZB64(repcount));
                            i += repcount - 1;
                        }
                    }
                    zb64stream.Append(pixel.ToString("X"));
                }
            }
            catch //(Exception ex)
            {
                //handle
            }
        }

        /// <summary>
        /// Converts to ZB64 format specified in the ZPLII Programming document.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string ConvertZB64(int count)
        {
            try
            {
                if (count > 419)
                    throw new ArgumentOutOfRangeException();

                int high = count / 20;
                int low = count % 20;

                const string lowString = " GHIJKLMNOPQRSTUVWXY";
                const string highString = " ghijklmnopqrstuvwxyz";

                string repeatSequence = "";
                if (high > 0)
                {
                    repeatSequence += highString[high];
                }
                if (low > 0)
                {
                    repeatSequence += lowString[low];
                }

                return repeatSequence;
            }
            catch// (Exception ex)
            {
                //handle
            }

            return null;
        }

        private static bool MatchByteArray(byte[] row, byte[] lastrow)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] != lastrow[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

}

