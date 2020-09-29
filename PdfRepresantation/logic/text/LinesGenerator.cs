﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfRepresantation
{
    class LinesGenerator
    {
        private readonly PageContext pageContext;

        public LinesGenerator(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }


        public IList<PdfTextLineDetails> CreateLines(IList<PdfTextBlock> texts)
        {
            AssignStartEnd(texts);
            return texts
                .OrderBy(t => t.Rotation)
                .ThenBy(t => t.Start)
                .GroupBy(t => (int) Math.Round(t.Bottom * 2))
                .OrderByDescending(g => g.Key)
                .SelectMany(g => new LineGenarator(pageContext, g).Lines)
                .ToList();
        }

        private void AssignStartEnd(IList<PdfTextBlock> texts)
        {
            foreach (var t in texts)
            {
                if (pageContext.PageRTL)
                {
                    if (t.Value == " " && t.Width < t.SpaceWidth)
                    {
                        t.Left = t.Right - t.SpaceWidth;
                        t.Width = t.SpaceWidth;
                    }

                    t.Start = pageContext.PageWidth - t.Right;
                    t.End = pageContext.PageWidth - t.Left;
                }
                else
                {
                    if (t.Value == " " && t.Width < t.SpaceWidth)
                        t.Width = t.SpaceWidth;
                    t.Start = t.Left;
                    t.End = t.Right;
                }
            }
        }
    }
}