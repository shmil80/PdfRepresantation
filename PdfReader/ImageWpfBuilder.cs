using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfRepresantation;

namespace PdfReader
{
    class ImageWpfBuilder
    {
        public void AddImage(PdfImageDetails imageDetails, PageContext pageContext)
        {
            var image = new Image
            {
                Width = imageDetails.Width,
                Height = imageDetails.Height,
                Stretch = Stretch.Fill,
                Source = LoadImage(imageDetails.Buffer)
            };
            image.SetValue(Canvas.TopProperty,(double) imageDetails.Top);
            if (pageContext.page.RightToLeft)
            {
                image.SetValue(Canvas.RightProperty,(double) imageDetails.Right);
            }
            else
            {
                image.SetValue(Canvas.LeftProperty, (double)imageDetails.Left);
            }
            pageContext.pagePanel.Children.Add(image);
        }
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}