# zplprint
Example about how to generate ZPL code from an image and sending it to a Zebra Printer.

# Usage:
  //Load the image, convert to .bmp and store in a Bitmap object (in this case I named it "bmp").
  
  ZPLPrintingService prnSvc = new ZPLPrintingService();
  string zpl = await prnSvc.GetImageZPLEncoded(bmp); //bmp is your bitmap object

  await prnSvc.SendDataToPrinter(ip, port, zpl);
