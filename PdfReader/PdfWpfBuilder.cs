using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PdfRepresantation;

namespace PdfReader
{
    class PdfWpfBuilder
    {
        private readonly TextWpfBuilder textBuilder;
        private readonly ImageWpfBuilder imageBuilder;
        private readonly ShapeWpfBuilder shapeBuilder;

        public PdfWpfBuilder()
        {
            textBuilder = CreateTextBuilder();
            imageBuilder = CreateImageBuilder();
            shapeBuilder = CreateShapeBuilder();
        }

        protected virtual TextWpfBuilder CreateTextBuilder() => new TextWpfBuilder();
        protected virtual ImageWpfBuilder CreateImageBuilder() => new ImageWpfBuilder();
        protected virtual ShapeWpfBuilder CreateShapeBuilder() => new ShapeWpfBuilder();

        public virtual void AddPdf(Panel rootContainer, PdfDetails details)
        {
            
                var pages=details.Pages
                    .Select(CreatePage)
                    .ToArray();
                rootContainer.Children.Clear();
                foreach (var page in pages)
                {
                    rootContainer.Children.Add(page);
                }
           
        }

        protected virtual Panel CreatePage(PdfPageDetails page)
        {
            const double marginLeft = 30;
            const double marginTop = 50;
            var pagePanel = new Canvas
            {
                Background = new SolidColorBrush(Colors.White),
                Width = page.Width,
                Height = page.Height,
                Margin = new Thickness(marginLeft, page.PageNumber == 1 ? marginTop : 0, marginLeft, marginTop),
                VerticalAlignment = VerticalAlignment.Top,
                ClipToBounds = true
            };
            if (page.RightToLeft)
            {
                pagePanel.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                pagePanel.HorizontalAlignment = HorizontalAlignment.Left;
            }

            PageContext pageContext = new PageContext {pagePanel = pagePanel, page = page};
            foreach (var item in page.OrderedDawings)
            {
                switch (item)
                {
                    case PdfImageDetails image:
                        imageBuilder.AddImage(image, pageContext);
                        break;
                    case ShapeDetails shape:
                        shapeBuilder.AddShape(shape, pageContext);
                        break;
                }
            }

            foreach (var line in page.Lines)
            {
                textBuilder.AddLine(line, pageContext);
            }


            return pagePanel;
        }

        public static Color ConvertColor(System.Drawing.Color color)
        {
            var result = Color.FromArgb(color.A, color.R, color.G, color.B);
            return result;
        }
    }
}