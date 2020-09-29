using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PdfRepresantation
{
    class ImageWrapper
    {
        private Bitmap bitmap;
        private MemoryStream stream;
        private ImageFormat format;

        public ImageFormat Format
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = (Bitmap) Bitmap.FromStream(stream);
                    format = bitmap.RawFormat;
                }
                return format;
            }
            set
            {
                format = value;
                stream = new MemoryStream();
                bitmap.Save(stream, format);
                stream.Position = 0;
                bitmap = null;
            }
        }

        public byte[] Buffer
        {
            get
            {
                if (stream == null)
                {
                    stream = new MemoryStream();
                    bitmap.Save(stream, format);
                    stream.Position=0;
                }

                return stream.CanWrite ? stream.ToArray() : stream.GetBuffer();
            }
            set
            {
                stream = new MemoryStream(value);
                stream.Position = 0;
                bitmap = null;
            }
        }

        public Bitmap Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = (Bitmap) Bitmap.FromStream(stream);
                    format = bitmap.RawFormat;
                }

                return bitmap;
            }
            set
            {
                stream = null;
                bitmap = value;
            }
        }
    }
}