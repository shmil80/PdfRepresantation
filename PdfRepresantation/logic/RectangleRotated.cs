using System;
using iText.Kernel.Geom;

namespace PdfRepresantation
{
    class RectangleRotated
    {
        public float Left, Bottom, Width, Height, Angle, BottomInOwnPlane;

        public RectangleRotated(Vector leftBottom, Vector rightBottom,
            Vector leftTop)
        {
            Left = leftBottom.Get(Vector.I1);
            Bottom = leftBottom.Get(Vector.I2);

            var cosLine = rightBottom.Get(Vector.I1) - leftBottom.Get(Vector.I1);
            var sinLine = rightBottom.Get(Vector.I2) - leftBottom.Get(Vector.I2);
            var heightRectLeftLine = leftTop.Get(Vector.I2) - leftBottom.Get(Vector.I2);
            if (Math.Abs(sinLine) < 0.0001)
            {
                Height = heightRectLeftLine;
                if (cosLine < 0)
                {
                    Angle = 180;
                    Width = -cosLine;
                    BottomInOwnPlane = -Bottom;
                }
                else
                {
                    Angle = 0;
                    Width = cosLine;
                    BottomInOwnPlane = Bottom;
                }

                return;
            }

            if (Math.Abs(cosLine) < 0.0001)
            {
                var widthRectLeftLine = leftTop.Get(Vector.I1) - leftBottom.Get(Vector.I1);
                if (sinLine < 0)
                {
                    Angle = 90;
                    Width = -sinLine;
                    Height = widthRectLeftLine;
                    BottomInOwnPlane = Left;
                }
                else
                {
                    Angle = 270;
                    Width = sinLine;
                    Height = -widthRectLeftLine;
                    BottomInOwnPlane = -Left;
                }

                return;
            }


            var radians = Math.Atan2(cosLine, sinLine);
            Angle = (float) (radians * 180 / Math.PI);

            var ration = Math.Cos(radians);
            Width = (float) (cosLine / ration);
            ration = Math.Sin(radians);
            Height = (float) (heightRectLeftLine / ration);

            var startPageRadian = Math.Atan2(Left, Bottom);
            var lineFromStartPage = Math.Sqrt(Left * Left + Bottom * Bottom);
            BottomInOwnPlane = (float) (Math.Sin(startPageRadian + radians) * lineFromStartPage);
        }

           const float Tolerance = 0.0000001F;
        public RectangleRotated(Matrix ctm)
        {
            Left = ctm.Get(Matrix.I31);
            Bottom = ctm.Get(Matrix.I32);
            
            var xToX = ctm.Get(Matrix.I11);
            var yToY = ctm.Get(Matrix.I22);
            var yToX = ctm.Get(Matrix.I21);
            var xToY = ctm.Get(Matrix.I12);
            if (Math.Abs(xToY) < Tolerance && Math.Abs(yToX) < Tolerance)
            {
                Width = Math.Abs(xToX);
                Height = Math.Abs(yToY);
                if (xToX < 0)
                    Left += xToX; //reverse rtl
               if (yToY < 0)
                   Bottom += xToX;//reverse utb 
                Angle =0;
                BottomInOwnPlane = Bottom;
            }
            else if(Math.Abs(xToX)<1&&Math.Abs(yToY)<1)
            {
                //Todo rotation in image is not completed at all
                Width = Math.Abs(yToX);
                Height = Math.Abs(xToY);
                if (yToX < 0)
                    Left += yToX; 
                if (xToY < 0)
                    Bottom += xToY;
                Angle = 90;

            }
        }
    }
}