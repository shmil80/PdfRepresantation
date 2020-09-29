using System;
using System.Reflection;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filters;

namespace PdfRepresantation
{
    public class PdfDecompressor
    {
        private static PdfWriter CreateWriter(string dest)
        {
            return new PdfWriter(dest,
                new WriterProperties().SetCompressionLevel(CompressionConstants.NO_COMPRESSION));
        }
         
        public void Decompress(string pathOrigin,string pathResult)
        {
            var writer = CreateWriter(pathResult);
            PdfDocument srcDoc = new PdfDocument(new PdfReader(pathOrigin), writer);
            // Creating a PdfDocument       
//            PdfDocument pdf = new PdfDocument(writer);
            int numberOfPdfObjects = srcDoc.GetNumberOfPdfObjects();
            var fieldOutputStream =
                typeof(PdfStream).GetField("outputStream", BindingFlags.Instance | BindingFlags.NonPublic);
            for (int i = 1; i <= numberOfPdfObjects; i++)
            {
                PdfObject obj = srcDoc.GetPdfObject(i);
                if (obj != null && obj.IsStream())
                {
                    PdfStream stream = (PdfStream) obj;
                    try
                    {
                        var decode = stream.GetAsName(PdfName.Filter);
                        byte[] bytes;
                        if (Equals(decode, PdfName.DCTDecode))
                        {
                            bytes = stream.GetBytes(false);
                            bytes = ASCIIHexEncoder.ASCIIHexEncode(bytes);
                            stream.Put(PdfName.Filter,new PdfArray(new[]{ PdfName.ASCIIHexDecode,PdfName.DCTDecode}));
                        }

                        else
                        {
                            IFilterHandler g;
                            
                            bytes = stream.GetBytes();
                            if (Equals(stream.GetAsName(PdfName.Type), PdfName.Stream)
                                ||Equals(stream.GetAsName(PdfName.Subtype), PdfName.Image)
                                ||stream.ContainsKey(PdfName.Length1)
                            )
                            {
                                bytes = ASCIIHexEncoder.ASCIIHexEncode(bytes);
                                stream.Put(PdfName.Filter, PdfName.ASCIIHexDecode);
                            }
                            else
                                stream.Remove(PdfName.Filter);
                        }

                        stream.Put(PdfName.Length,new PdfNumber(bytes.Length));
                        var outputStream = new PdfOutputStream(new ByteArrayOutputStream(bytes.Length));
                        outputStream.WriteBytes(bytes);
                        fieldOutputStream.SetValue(stream, outputStream);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
//            srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), pdf);

            //            // Creating a Document        
//            Document document = new Document(pdf);
//
//                Document document = new Document(pdfDoc);
//
//                // Assume that there is a single XObject in the source document
//                // and this single object is an image.
//                PdfDictionary pageDict = pdfDoc.GetFirstPage().GetPdfObject();
//                PdfDictionary resources = pageDict.GetAsDictionary(PdfName.Resources);
//                PdfDictionary xObjects = resources.GetAsDictionary(PdfName.XObject);
//                PdfName imgRef = xObjects.KeySet().First();
//                PdfStream stream = xObjects.GetAsStream(imgRef);
//                var imageObject = new PdfImageXObject(stream);
//                imageObject.GetPdfObject().Remove(PdfName.SMask);
//            

            srcDoc.Close();
//            pdf.Close();
        }
 
    }
}