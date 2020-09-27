using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ZPL
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0 || args[0].Length < 3) 
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("zplprin file.bmp <printer ip addr> <printer port>");
                Console.WriteLine("(the order of the parameters cannot be altered)");
            }
            else
            {
                string fileName = args[0].Split(" ")[0];
                string ip = args[0].Split(" ")[1];
                int port = Int32.Parse(args[0].Split(" ")[2]);

                Task.Run(async() => {
                    Image image = Image.FromStream(null, true);
                    Bitmap bmp = new Bitmap(image);
                    
                    ZPLPrintingService prnSvc = new ZPLPrintingService();
                    string zpl = await prnSvc.GetImageZPLEncoded(bmp);

                    await prnSvc.SendDataToPrinter(ip, port, zpl);
                });
            }
        }
    }
}
