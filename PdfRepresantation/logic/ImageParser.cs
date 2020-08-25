using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Xobject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Color = System.Drawing.Color;
using IOException = iText.IO.IOException;
using Matrix = iText.Kernel.Geom.Matrix;
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
                imageWrapper = new ImageWrapper { Buffer = data.GetImage().GetImageBytes() };
            }
            catch (IOException e)
            {
                Log.Info("Wrong format of image:" + e.Message);
                //wrong format of image
                return;
            }
            try
            {
                Bitmap bitmap = imageWrapper.Bitmap;
            }
            catch (ArgumentException e)
            {
                Log.Info("Wrong format of image:" + e.Message);
                //wrong format of image
                return;
            }
            Matrix ctm = data.GetImageCtm();
            RectangleRotated position = new RectangleRotated(ctm);
            var image = new PdfImageDetails
            {
                Left = position.Left,
                Bottom = pageContext.PageHeight - position.Bottom,
                Width = position.Width,
                Height = position.Height,
                Rotation = position.Angle == 0 ? (float?)null : position.Angle,
                Top = pageContext.PageHeight - position.Bottom - position.Height,
                Right = pageContext.PageWidth - position.Left - position.Width,
                Order = orderIndex,
            };
            Log.Debug($"image:{{{image.Left},{image.Top},{image.Width},{image.Height}}}");
            if (!MergeSoftMask(data, imageWrapper))
            {
                ApplyMask(data, imageWrapper);
            }

            if (pageContext.Processor.CurrentClipping != null)
            {
                Log.Debug($"building clipping mask");
                Bitmap clippingMask = CreateClippingMask(image.Left, image.Top,
                    imageWrapper.Bitmap.Width / image.Width,
                    imageWrapper.Bitmap.Height / image.Height,
                    imageWrapper.Bitmap.Width, imageWrapper.Bitmap.Height,
                    pageContext.Processor.CurrentClipping);
                if (clippingMask != null)
                {
                    MergeMaskImage(imageWrapper, clippingMask);
                    Log.Debug($"applying clipping mask");
                }
            }

            if (IsEmpty(imageWrapper.Bitmap))
            {
                Log.Debug($"image is empty. ignore it!");
                return;
            }

            image.Buffer = imageWrapper.Buffer;
            images.Add(image);
        }

        private Bitmap CreateClippingMask(float x, float y,
            float scaleH, float scaleV,
            int width, int height, ClippingGroup @group)
        {
            if (@group.Clipings.All(s =>
                s.Lines.All(l => l.AllPoints.All(p => Math.Abs(p.X) > 100000 || Math.Abs(p.Y) > 100000))))
            {
                return null;
            }

            Bitmap result = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(result);
            graphics.FillRectangle(Brushes.Black, 0f, 0f, pageContext.PageWidth, pageContext.PageHeight);

            PointF Point(ShapePoint p)
            {
                return new PointF(scaleH * (p.X - x), scaleV * (p.Y - y));
            }

            Region region = null;
            foreach (ClippingPath shape in @group.Clipings)
            {
                GraphicsPath path = new GraphicsPath();
                foreach (ShapeLine line in shape.Lines)
                {
                    PointF start = Point(line.Start);
                    PointF end = Point(line.End);
                    if (line.CurveControlPoint1 == null)
                    {
                        path.AddLine(start, end);
                        continue;
                    }

                    PointF controlPoint1 = Point(line.CurveControlPoint1);
                    if (line.CurveControlPoint2 == null)
                    {
                        path.AddBeziers(new[] { start, controlPoint1, end });
                        continue;
                    }

                    PointF controlPoint2 = Point(line.CurveControlPoint2);
                    path.AddBezier(start, controlPoint1, controlPoint2, end);
                }

                path.FillMode = shape.EvenOddRule ? FillMode.Alternate : FillMode.Winding;
                path.CloseFigure();

                if (region == null)
                {
                    region = new Region(path);
                }
                else
                {
                    region.Intersect(path);
                }
            }

            try
            {
                graphics.FillRegion(Brushes.White, region);
            }
            catch (OverflowException e)
            {
                Log.Error(e.ToString());
            }

            return result;
        }

        private bool ApplyMask(ImageRenderInfo data, ImageWrapper imageWrapper)
        {
            PdfImageXObject image = data.GetImage();
            PdfStream dict = image.GetPdfObject();
            PdfObject maskObject = dict.Get(PdfName.Mask);
            switch (maskObject)
            {
                case null: return false;
                case PdfStream maskStream:
                    MergeMaskObject(imageWrapper, maskStream);
                    Log.Debug($"applying mask");
                    return true;
                case PdfArray array:
                    PdfName spaceName = dict.GetAsName(PdfName.ColorSpace) ??
                                    dict.GetAsArray(PdfName.ColorSpace).GetAsName(0);
                    MergeMaskArray(imageWrapper, array, spaceName);
                    Log.Debug($"applying array mask");
                    return true;
            }


            return false;
        }

        private void MergeMaskArray(ImageWrapper imageWrapper, PdfArray array, PdfName spaceName)
        {
            int[] colors = array.ToIntArray();
            NormalColorManager colorManager = (NormalColorManager)ColorManager.GetManagerByName(spaceName);
            MakeTransparentPossible(imageWrapper);

            int[] firstHalf = new int[colors.Length / 2];
            int[] secondHalf = new int[colors.Length / 2];
            for (int i = 0; i < colors.Length; i++)
            {
                if (i % 2 == 0)
                {
                    firstHalf[i / 2] = colors[i];
                }
                else
                {
                    secondHalf[i / 2] = colors[i];
                }
            }

            Color? start = colorManager.Color(firstHalf, 1);
            if (start == null)
            {
                return;
            }

            Color? end = colorManager.Color(secondHalf, 1);
            if (end == null)
            {
                return;
            }

            imageWrapper.Bitmap = ApplyRangeTransparent(imageWrapper.Bitmap, start.Value, end.Value);
        }

        private static void MakeTransparentPossible(ImageWrapper imageWrapper)
        {
            if (!Equals(imageWrapper.Format, ImageFormat.Bmp) &&
                !Equals(imageWrapper.Format, ImageFormat.MemoryBmp) &&
                !Equals(imageWrapper.Format, ImageFormat.Jpeg))
            {
                return;
            }

            imageWrapper.Format = ImageFormat.Png;
        }

        private bool MergeSoftMask(ImageRenderInfo data, ImageWrapper imageWrapper)
        {
            PdfImageXObject image = data.GetImage();
            PdfStream maskStream = image.GetPdfObject().GetAsStream(PdfName.SMask);
            if (maskStream == null)
            {
                return false;
            }

            MergeMaskObject(imageWrapper, maskStream);
            Log.Debug($"applying soft mask");

            return true;
        }

        private void MergeMaskObject(ImageWrapper imageWrapper, PdfStream maskStream)
        {
            PdfImageXObject maskImage = new PdfImageXObject(maskStream);
            byte[] bytesMask = maskImage.GetImageBytes();
            Bitmap bitmapMask = Bitmap.FromStream(new MemoryStream(bytesMask)) as Bitmap;

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
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);

            BitmapData bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        int index = 4 * x;
                        byte b = ptrInput[index]; // blue
                        byte g = ptrInput[index + 1]; // green
                        byte r = ptrInput[index + 2]; // red
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
        private bool IsEmpty(Bitmap input)
        {
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            BitmapData bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            bool IsEmpty()
            {
                unsafe
                {
                    for (int y = 0; y < input.Height; y++)
                    {
                        byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                        for (int x = 0; x < input.Width; x++)
                        {
                            if (ptrInput[4 * x + 3] > 0)// alpha
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            bool result = IsEmpty();
            input.UnlockBits(bitsInput);
            return result;
        }
        private Bitmap ApplyMask(Bitmap input, Bitmap mask)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            BitmapData bitsMask = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrMask = (byte*)bitsMask.Scan0 + y * bitsMask.Stride;
                    byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        int index = 4 * x;
                        ptrOutput[index] = ptrInput[index]; // blue
                        ptrOutput[index + 1] = ptrInput[index + 1]; // green
                        ptrOutput[index + 2] = ptrInput[index + 2]; // red
                        if (ptrInput[index + 3] > ptrMask[index])
                        {
                            ptrOutput[index + 3] = ptrMask[index]; //alpha
                        }
                        else
                        {
                            ptrOutput[index + 3] = ptrInput[index + 3]; //previous alpha
                        }
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