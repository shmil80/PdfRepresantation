﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfRepresantation
{
    public class PageParser : IEventListener
    {
        protected readonly ImageParser imageParser;
        protected readonly ShapeParser shapeParser;
        protected readonly TextParser textParser;
        private readonly LinesGenerator linesGenerator;

        public readonly PageContext pageContext;
        private int orderIndex = 0;

        public PageParser(PdfPage page, int pageNumber)
        {
            var pageSize = page.GetPageSize();
            pageContext = new PageContext
            {
                Page = page,
                PageNumber = pageNumber,
                PageHeight = pageSize.GetHeight(),
                PageWidth = pageSize.GetWidth()
            };
            pageContext.LinkManager = new LinkManager(pageContext);
            linesGenerator = new LinesGenerator(pageContext);
            imageParser = new ImageParser(pageContext);
            shapeParser = new ShapeParser(pageContext);
            textParser = new TextParser(pageContext);
            pageContext.LinkManager.FindLinks();
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            switch (type)
            {
                case EventType.BEGIN_TEXT:
                case EventType.CLIP_PATH_CHANGED: break;
                case EventType.END_TEXT:
                    textParser.MarkAsEnd();
                    break;
                case EventType.RENDER_PATH:
                    var pathInfo = (PathRenderInfo) data;
                    shapeParser.ParsePath(pathInfo, orderIndex++);
                    break;
                case EventType.RENDER_TEXT:
                    textParser.ParseText((TextRenderInfo) data);
                    break;
                case EventType.RENDER_IMAGE:
                    imageParser.ParseImage((ImageRenderInfo) data, orderIndex++);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        public ICollection<EventType> GetSupportedEvents()
        {
            return new[]
            {
                EventType.RENDER_TEXT, EventType.RENDER_IMAGE,
                EventType.RENDER_PATH, EventType.END_TEXT
            };
        }

        public virtual PdfPageDetails CreatePageDetails()
        {
            textParser.MarkAsEnd();
            pageContext.PageRTL = RightToLeftManager.Instance.FindRightToLeft(textParser.texts);
            var lines = linesGenerator.CreateLines(textParser.texts);

            return new PdfPageDetails
            {
                Lines = lines,
                RightToLeft = pageContext.PageRTL,
                Images = imageParser.images,
                PageNumber = pageContext.PageNumber,
                Shapes = shapeParser.shapes,
                Fonts = textParser.fonts.Values.ToList(),
                Height = pageContext.PageHeight,
                Width = pageContext.PageWidth
            };
        }
    }
}