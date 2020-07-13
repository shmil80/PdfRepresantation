﻿namespace PdfRepresantation
{
    public class HtmlWriterConfig
    {
        public bool DrawShapes { get; set; } = true;
        public bool EmbeddedImages { get; set; } = true;
        public string DirImages { get; set; }
        public bool UseCanvas { get; set; }
        public bool AddHeader { get; set; }
        public int RoundDigits { get; set; } = 2;
    }
}