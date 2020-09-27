
using System.Threading.Tasks;
using System.Drawing;

namespace ZPL
{
  public interface IZPLPrintingService
  {
    Task<string> GetImageZPLEncoded(Bitmap bitmap);
    Task<bool> SendDataToPrinter(string printerIPAddress, int printerPort, string zplString);

  }
  
}