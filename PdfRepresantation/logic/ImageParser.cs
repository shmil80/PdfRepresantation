using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Wmf;
using iText.Kernel.Pdf.Xobject;
using Org.BouncyCastle.Crypto.Modes;
using Color = System.Drawing.Color;
using Image = iText.Layout.Element.Image;
using IOException = iText.IO.IOException;
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
            byte[] bytes;
            try
            {
                bytes = data.GetImage().GetImageBytes();
            }
            catch (IOException e)
            {
                Log.Info("Wrong format of image:" + e.Message);
                //wrong format of image
                return;
            }
            
            if (!MergeSoftMask(data, ref bytes))
                ApplyMask(data, ref bytes);

            // var start = data.GetStartPoint();
            var ctm = data.GetImageCtm();
            var width = ctm.Get(Matrix.I11);
            var height = ctm.Get(Matrix.I22);
            var x = ctm.Get(Matrix.I31);
            var y = pageContext.PageHeight - ctm.Get(Matrix.I32);
            images.Add(new PdfImageDetails
            {
                Buffer = bytes,
                Bottom = y,
                Top = y - height,
                Left = x,
                Right = pageContext.PageWidth - x - width,
                Width = width,
                Height = height,
                Order = orderIndex,
            });
        }

        private bool ApplyMask(ImageRenderInfo data, ref byte[] bytes)
        {
            var image = data.GetImage();
            var dict = image.GetPdfObject();
            var maskObject = dict.Get(PdfName.Mask);
            switch (maskObject)
            {
                case null: return false;
                case PdfStream maskStream:
                    MergeMaskObject(ref bytes, maskStream);
                    return true;
                case PdfArray array:
                    var spaceName = dict.GetAsName(PdfName.ColorSpace) ??
                                    dict.GetAsArray(PdfName.ColorSpace).GetAsName(0);
                    MergeMaskArray(ref bytes, array, spaceName);
                    return true;
            }


            return false;
        }

        private void MergeMaskArray(ref byte[] bytes, PdfArray array, PdfName spaceName)
        {
            var colors = array.ToIntArray();
            var colorManager = (NormalColorManager)ColorManager.GetManagerByName(spaceName);
            var bitmapImage = Bitmap.FromStream(new MemoryStream(bytes)) as Bitmap;
            MakeTransparentPossible(ref bitmapImage);

            var firstHalf = new int[colors.Length / 2];
            var secondHalf = new int[colors.Length / 2];
            for (var i = 0; i < colors.Length; i++)
            {
                if (i % 2 == 0)
                    firstHalf[i / 2] = colors[i];
                else
                    secondHalf[i / 2] = colors[i];
            }

            var start = colorManager.Color(firstHalf,1);
            if (start == null)
                return;
            var end = colorManager.Color(secondHalf,1);
            if (end == null)
                return;

            var output = ApplyRangeTransparent(bitmapImage, start.Value, end.Value);
            using (var memoryStream = new MemoryStream())
            {
                output.Save(memoryStream, bitmapImage.RawFormat);
                bytes = memoryStream.GetBuffer();
            }
        }

        private static void MakeTransparentPossible(ref Bitmap bitmapImage)
        {
            if (!Equals(bitmapImage.RawFormat, ImageFormat.Bmp) &&
                !Equals(bitmapImage.RawFormat, ImageFormat.MemoryBmp) &&
                !Equals(bitmapImage.RawFormat, ImageFormat.Jpeg))
                return;
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                bitmapImage.Save(memoryStream, ImageFormat.Png);
                bytes = memoryStream.GetBuffer();
            }

            bitmapImage = Bitmap.FromStream(new MemoryStream(bytes)) as Bitmap;
        }

        private bool MergeSoftMask(ImageRenderInfo data, ref byte[] bytes)
        {
            var image = data.GetImage();
            var maskStream = image.GetPdfObject().GetAsStream(PdfName.SMask);
            if (maskStream == null)
                return false;
            MergeMaskObject(ref bytes, maskStream);

            return true;
        }

        private void MergeMaskObject(ref byte[] bytes, PdfStream maskStream)
        {
            var bitmapImage = Bitmap.FromStream(new MemoryStream(bytes)) as Bitmap;

            var maskImage = new PdfImageXObject(maskStream);
            var bytesMask = maskImage.GetImageBytes();
            var bitmapMask = Bitmap.FromStream(new MemoryStream(bytesMask)) as Bitmap;

            var bitmapResult = ApplyMask(bitmapImage, bitmapMask);

            using (var memoryStream = new MemoryStream())
            {
                bitmapResult.Save(memoryStream, bitmapImage.RawFormat);
                bytes = memoryStream.GetBuffer();
            }
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