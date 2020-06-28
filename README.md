PdfRepresantation is a simple tool to get the details of a pdf, and convert it to html, text or image. 

It is writen in C# based on itext7 package 

usage:
```cs
      string path = "file.pdf";
      PdfDetails pdf = PdfDetailsFactory.Create(path);
      string text = pdf.ToString();
      string html = new PdfHtmlWriter().ConvertPdf(pdf);
      Bitmap bitmap = new PdfImageWriter().ConvertToImage(pdf);           
```

PdfRepresantation is licensed as <a href="/License.md">AGPL</a>
