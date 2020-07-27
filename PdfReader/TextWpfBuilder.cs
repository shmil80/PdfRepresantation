using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using PdfRepresantation;
using Color = System.Drawing.Color;

namespace PdfReader
{
    class TextWpfBuilder

    {
        public void AddLine(PdfTextLineDetails line, PageContext pageContext)
        {
            var lineBox = new SelectableTextBlock
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Thickness()
                //Width = line.Width,
                //Height = line.Height,
                //Background =Brushes.Transparent,
                //ClipToBounds = false
            };
            if (pageContext.page.RightToLeft)
            {
                lineBox.FlowDirection = FlowDirection.RightToLeft;
                lineBox.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                lineBox.FlowDirection = FlowDirection.LeftToRight;
                lineBox.HorizontalAlignment = HorizontalAlignment.Left;
            }

            PdfLinkResult link = null;
            foreach (var text in line.Texts)
            {
                if (text.LinkParent != null)
                {
                    if (text.LinkParent != link)
                    {
                        link = text.LinkParent;
                        lineBox.Inlines.Add(CreateLink(link));
                    }

                    continue;
                }

                lineBox.Inlines.Add(CreateRun(text));
            }

            lineBox.Inlines.Add(new Run(""));

            SetPosition(line, pageContext, lineBox);
            if (line.Rotation.HasValue)
            {
                lineBox.RenderTransformOrigin = new Point(0,1);
                lineBox.RenderTransform = new RotateTransform {Angle = line.Rotation.Value,};
            }

             pageContext.pagePanel.Children.Add(lineBox);
        }

        private static void SetPosition(PdfTextLineDetails line, PageContext pageContext, DependencyObject lineBox)
        {
            lineBox.SetValue(Canvas.BottomProperty, (double) (pageContext.page.Height - line.Bottom));
            if (pageContext.page.RightToLeft)
            {
                lineBox.SetValue(Canvas.RightProperty, (double) line.Right);
            }
            else
            {
                lineBox.SetValue(Canvas.LeftProperty, (double) line.Left);
            }
        }

        private Span CreateLink(PdfLinkResult link)
        {
            Span span;
            if (Uri.IsWellFormedUriString(link.Link, UriKind.RelativeOrAbsolute))
            {
                span = new Hyperlink
                {
                    NavigateUri = new Uri(link.Link),
                    Cursor = Cursors.Hand,
                    ForceCursor = true,
                };
                span.MouseDown += LinkMouseDown;
                //span.RequestNavigate += NavigateToLink;
            }
            else
                span = new Span();

            foreach (var text in link.Children)
            {
                span.Inlines.Add(CreateRun(text));
            }

            return span;
        }

        private void LinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink) sender).NavigateUri.AbsoluteUri);
        }

        private void NavigateToLink(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        private Run CreateRun(PdfTextResult text)
        {
            var run = new Run(text.Value)
            {
                BaselineAlignment = BaselineAlignment.Baseline,
                FontSize = text.FontSize,
                FontWeight = text.Font.Bold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = text.Font.Italic ? FontStyles.Italic : FontStyles.Normal,
                FontFamily = new FontFamily(text.Font.BasicFontFamily+",Times New Roman"),
            };
            if (text.Stroke.MainColor.HasValue)
            {
                var color = PdfWpfBuilder.ConvertColor(text.Stroke.MainColor.Value);
                run.Foreground = new SolidColorBrush(color);
            }

            return run;
        }
    }
}