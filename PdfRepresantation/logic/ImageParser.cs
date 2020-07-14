using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Wmf;
using iText.Kernel.Pdf.Xobject;
using Org.BouncyCastle.Crypto.Modes;
using Color = System.Drawing.Color;
using Image = iText.Layout.Element.Image;
using IOException = iText.IO.IOException;
using Matrix = iText.Kernel.Geom.Matrix;
using Point = iText.Kernel.Geom.Point;
using Rectangle = System.Drawing.Rectangle;

namespace PdfRepresantation
{
    public class ImageParser
    {
        public readonly IList<PdfImageDetails> images = new List<PdfImageDetails>();
        private readonly PageContext pageContext;

        internal ImageParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }


        public virtual void ParseImage(ImageRenderInfo data, int orderIndex)
        {
            ImageWrapper imageWrapper;
            try
            {
                imageWrapper = new ImageWrapper {Buffer = data.GetImage().GetImageBytes()};
            }
            catch (IOException e)
            {
                Log.Info("Wrong format of image:" + e.Message);
                //wrong format of image
                return;
            }
            var ctm = data.GetImageCtm();
            var position=new RectangleRotated(ctm);
            var image = new PdfImageDetails
            {
                Left = position.Left,
                Bottom = pageContext.PageHeight - position.Bottom,
                Width = position.Width,
                Height = position.Height,
                Rotation=position.Angle==0?(float?) null:position.Angle,
                Top = pageContext.PageHeight - position.Bottom- position.Height,
                Right = pageContext.PageWidth - position.Left - position.Width,
                Order = orderIndex,
            };
            if (!MergeSoftMask(data, imageWrapper))
                ApplyMask(data, imageWrapper);
            if (pageContext.Processor.CurrentClipping != null)
            {
                var clippingMask = CreateClippingMask(image.Left,image.Top,
                    imageWrapper.Bitmap.Width/image.Width,
                    imageWrapper.Bitmap.Height/image.Height,
                    imageWrapper.Bitmap.Width, imageWrapper.Bitmap.Height,
                    pageContext.Processor.CurrentClipping);
                if(clippingMask!=null)
                    MergeMaskImage(imageWrapper, clippingMask);
            }


            image.Buffer = imageWrapper.Buffer;
            images.Add(image);
        }

        private Bitmap CreateClippingMask(float x,float y,
            float scaleH,float scaleV,
            int width, int height, ClippingPath shape)
        {
            if (shape.Lines.All(l => l.AllPoints.All(p => Math.Abs(p.X) > 100000 || Math.Abs(p.Y) > 100000)))
                return null;
            Bitmap result = new Bitmap(width, height);
            var graphics = Graphics.FromImage(result);
            graphics.FillRectangle(Brushes.Black, 0f, 0f, pageContext.PageWidth, pageContext.PageHeight);
            GraphicsPath path = new GraphicsPath();

            PointF Point(ShapePoint p)
            {
                return new PointF(scaleH*(p.X-x),scaleV*(p.Y-y));
            }
            foreach (var line in shape.Lines)
            {
                var start = Point(line.Start);
                var end = Point(line.End);
                if (line.CurveControlPoint1 == null)
                {
                    path.AddLine(start, end);
                    continue;
                }

                var controlPoint1 =  Point(line.CurveControlPoint1);
                if (line.CurveControlPoint2 == null)
                {
                    path.AddBeziers(new[] {start, controlPoint1, end});
                    continue;
                }

                var controlPoint2 = Point(line.CurveControlPoint2);
                path.AddBezier(start, controlPoint1, controlPoint2, end);
            }

            path.FillMode = shape.EvenOddRule ? FillMode.Alternate : FillMode.Winding;
            path.CloseFigure();
            try
            {
                graphics.FillPath(Brushes.White, path);
            }
            catch (OverflowException e)
            {
                Log.Error(e.ToString());
              
            }
            return result;
        }

        private bool ApplyMask(ImageRenderInfo data, ImageWrapper imageWrapper)
        {
            var image = data.GetImage();
            var dict = image.GetPdfObject();
            var maskObject = dict.Get(PdfName.Mask);
            switch (maskObject)
            {
                case null: return false;
                case PdfStream maskStream:
                    MergeMaskObject(imageWrapper, maskStream);
                    return true;
                case PdfArray array:
                    var spaceName = dict.GetAsName(PdfName.ColorSpace) ??
                                    dict.GetAsArray(PdfName.ColorSpace).GetAsName(0);
                    MergeMaskArray(imageWrapper, array, spaceName);
                    return true;
            }


            return false;
        }

        private void MergeMaskArray(ImageWrapper imageWrapper, PdfArray array, PdfName spaceName)
        {
            var colors = array.ToIntArray();
            var colorManager = (NormalColorManager) ColorManager.GetManagerByName(spaceName);
            MakeTransparentPossible(imageWrapper);

            var firstHalf = new int[colors.Length / 2];
            var secondHalf = new int[colors.Length / 2];
            for (var i = 0; i < colors.Length; i++)
            {
                if (i % 2 == 0)
                    firstHalf[i / 2] = colors[i];
                else
                    secondHalf[i / 2] = colors[i];
            }

            var start = colorManager.Color(firstHalf, 1);
            if (start == null)
                return;
            var end = colorManager.Color(secondHalf, 1);
            if (end == null)
                return;

            imageWrapper.Bitmap = ApplyRangeTransparent(imageWrapper.Bitmap, start.Value, end.Value);
        }

        private static void MakeTransparentPossible(ImageWrapper imageWrapper)
        {
            if (!Equals(imageWrapper.Format, ImageFormat.Bmp) &&
                !Equals(imageWrapper.Format, ImageFormat.MemoryBmp) &&
                !Equals(imageWrapper.Format, ImageFormat.Jpeg))
                return;
            imageWrapper.Format = ImageFormat.Png;
        }

        private bool MergeSoftMask(ImageRenderInfo data, ImageWrapper imageWrapper)
        {
            var image = data.GetImage();
            var maskStream = image.GetPdfObject().GetAsStream(PdfName.SMask);
            if (maskStream == null)
                return false;
            MergeMaskObject(imageWrapper, maskStream);

            return true;
        }

        private void MergeMaskObject(ImageWrapper imageWrapper, PdfStream maskStream)
        {
            var maskImage = new PdfImageXObject(maskStream);
            var bytesMask = maskImage.GetImageBytes();
            var bitmapMask = Bitmap.FromStream(new MemoryStream(bytesMask)) as Bitmap;

            MergeMaskImage(imageWrapper, bitmapMask);
        }

        private void MergeMaskImage(ImageWrapper imageWrapper, Bitmap bitmapMask)
        {
            MakeTransparentPossible(imageWrapper);
            imageWrapper.Bitmap = ApplyMask(imageWrapper.Bitmap, bitmapMask);
        }

        private Bitmap ApplyRangeTransparent(Bitmap input, Color start, Color end)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            var rect = new Rectangle(0, 0, input.Width, input.Height);

            var bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrInput = (byte*) bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*) bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        var index = 4 * x;
                        var b = ptrInput[index]; // blue
                        var g = ptrInput[index + 1]; // green
                        var r = ptrInput[index + 2]; // red
                        if (start.R <= r && end.R >= r || start.G <= g && end.G >= g || start.B <= b && end.B >= b)
                        {
                            ptrOutput[index + 3] = 0; //alpha
                        }
                        else
                        {
                            ptrOutput[index] = b;
                            ptrOutput[index + 1] = g;
                            ptrOutput[index + 2] = r;
                            ptrOutput[index + 3] = 255; //alpha
                        }
                    }
                }
            }

            input.UnlockBits(bitsInput);
            output.UnlockBits(bitsOutput);

            return output;
        }

        private Bitmap ApplyMask(Bitmap input, Bitmap mask)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            var rect = new Rectangle(0, 0, input.Width, input.Height);
            var bitsMask = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrMask = (byte*) bitsMask.Scan0 + y * bitsMask.Stride;
                    byte* ptrInput = (byte*) bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*) bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        var index = 4 * x;
                        ptrOutput[index] = ptrInput[index]; // blue
                        ptrOutput[index + 1] = ptrInput[index + 1]; // green
                        ptrOutput[index + 2] = ptrInput[index + 2]; // red

                        ptrOutput[index + 3] = ptrMask[index]; //alpha
                    }
                }
            }

            mask.UnlockBits(bitsMask);
            input.UnlockBits(bitsInput);
            output.UnlockBits(bitsOutput);

            return output;
        }
    }
}