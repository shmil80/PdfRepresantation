using iText.Kernel.Pdf;

namespace PdfRepresantation
{
 public   class PageContext
    {
        internal bool PageRTL { get; set; }
        internal float PageHeight { get; set; }
        internal float PageWidth { get; set; }
        internal PdfPage Page { get; set; }
        internal int PageNumber { get; set; }

        internal LinkManager LinkManager { get; set; }
        internal PdfCanvasProcessorWithClip Processor{ get; set; }
        internal FontManager FontManager{ get; set; }
    }
}