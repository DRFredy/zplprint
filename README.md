# zplprint
Example about how to generate ZPL code from an image and sending it to a Zebra Printer.

# Usage:
//Load the image, convert it to Bitmap (in this case I named it "bmp") and pass it to the GetImageZPLEncoded method.
  
ZPLPrintingService prnSvc = new ZPLPrintingService();

string zplCode = await prnSvc.GetImageZPLEncoded(bmp); //bmp is your bitmap object
await prnSvc.SendDataToPrinter(ip, port, zplCode);
