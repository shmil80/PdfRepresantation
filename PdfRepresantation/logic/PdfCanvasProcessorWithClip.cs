﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfRepresantation
{
    class ClippingGroup
    {
        public readonly List<ClippingPath> Clipings = new List<ClippingPath>();
    }

    class PdfCanvasProcessorWithClip : PdfCanvasProcessor
    {
        private readonly PageContext pageContext;
        public ClippingGroup CurrentClipping => clipings.Count == 0 ? null : clipings.Peek();
        private readonly Stack<ClippingGroup> clipings = new Stack<ClippingGroup>();

        private Dictionary<string, IContentOperator> clipOperators = new Dictionary<string, IContentOperator>();

        public PdfCanvasProcessorWithClip(IEventListener eventListener, PageContext pageContext) : base(eventListener)
        {
            this.pageContext = pageContext;
            pageContext.Processor = this;
            clipOperators.Add("Q", new RestoreClipingOperator());
            clipOperators.Add("q", new NewClipingOperator());
        }
        protected override PdfFont GetFont(PdfDictionary fontDict)
        {
            var pdfFont = base.GetFont(fontDict);
            pageContext.FontManager.CreateFont(pdfFont);    
            return pdfFont;
        }

        protected override void InvokeOperator(PdfLiteral @operator, IList<PdfObject> operands)
        {
            base.InvokeOperator(@operator, operands);
            var key = @operator.ToString();
            if (clipOperators.TryGetValue(key, out var op))
                op.Invoke(this, @operator, operands);
        }

        class NewClipingOperator : IContentOperator
        {
            public virtual void Invoke(PdfCanvasProcessor processor, PdfLiteral @operator, IList<PdfObject> operands)
            {
                ((PdfCanvasProcessorWithClip) processor).Push();
            }
        }

        class RestoreClipingOperator : IContentOperator
        {
            public virtual void Invoke(PdfCanvasProcessor processor, PdfLiteral @operator, IList<PdfObject> operands)
            {
                ((PdfCanvasProcessorWithClip) processor).Pop();
            }
        }


        public void Clip(ClippingPath clip)
        {
            if (clipings.Count == 0)
                clipings.Push(new ClippingGroup {Clipings = {clip}});
            else
                CurrentClipping.Clipings.Add(clip);
        }


        private void Pop()
        {
            if (clipings.Count > 0)
                clipings.Pop();
        }

        private void Push()
        {
            if (clipings.Count > 0)
            {
                var item = new ClippingGroup();
                item.Clipings.AddRange(clipings.Reverse().SelectMany(l => l.Clipings));
                clipings.Push(item);
            }
        }
    }
}