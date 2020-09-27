# zplprint
### C# Example about how to generate ZPL code from an image and send it to a Zebra Printer.

This is a little (but useful) portion of code in which I was working on months ago (I can't remember if it was at the end of 2019, or starting th e2020's)..

Thing is: if you are stuck wondering how to talk with that printer and you don't know where to start, this code could be quite helpful. 

I used (a lot) this site to test the generated code => http://labelary.com/viewer.html

### Usage:
//Load the image, convert it to Bitmap (in this case I named it "bmp") and pass it to the GetImageZPLEncoded method.

ZPLPrintingService prnSvc = new ZPLPrintingService();

string zplCode = await prnSvc.GetImageZPLEncoded(bmp); //bmp is your bitmap object

await prnSvc.SendDataToPrinter(ip, port, zplCode);

### Important:
I got the code of the ZPLConv class somwhere here in github. I can't remember where exactly, so my apologies to that user (credits to that person). 
