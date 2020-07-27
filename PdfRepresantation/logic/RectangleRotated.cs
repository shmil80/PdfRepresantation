using System;
using iText.Kernel.Geom;

namespace PdfRepresantation
{
    class RectangleRotated
    {
        public float Left, Bottom, Width, Height, Angle;

        public RectangleRotated(LineSegment bottom, LineSegment top)
        {
            Vector leftBottom = bottom.GetStartPoint(),
                rightBottom = bottom.GetEndPoint(),
                leftTop = top.GetStartPoint(),
                rightTop = top.GetEndPoint();
            var normalLeft = leftBottom.Get(Vector.I1);
            var normalBottom = leftBottom.Get(Vector.I2);
            var cosLine = rightBottom.Get(Vector.I1) - leftBottom.Get(Vector.I1);
            var sinLine = rightBottom.Get(Vector.I2) - leftBottom.Get(Vector.I2);
            var heightRectLeftLine = leftTop.Get(Vector.I2) - leftBottom.Get(Vector.I2);
            if (Math.Abs(sinLine) < 0.0001)
            {
                Bottom = normalBottom;
                Height = heightRectLeftLine;
                Angle = 0;
                if (cosLine < 0)
                {
                    Width = -cosLine;
                    Left = rightBottom.Get(Vector.I1);
                }
                else
                {
                    Width = cosLine;
                    Left = normalLeft;
                }

                return;
            }
            //the plane of bottom is tranformed from the left bottom of the page rotated with the angle
            //the axis is the same: (0,0)=>(0,0)

//            if (Math.Abs(cosLine) < 0.0001)
//            {
//                var widthRectLeftLine = leftTop.Get(Vector.I1) - leftBottom.Get(Vector.I1);
//                if (sinLine < 0)
//                {
//                    Angle = 90;
//                    Width = -sinLine;
//                    Height = widthRectLeftLine;
//                    Bottom = normalLeft;
//                    Left = -normalBottom;
//                }
//                else
//                {
//                    Angle = 270;
//                    Width = sinLine;
//                    Height = -widthRectLeftLine;
//                    Bottom = -normalLeft;
//                    Left = normalBottom;
//                }
//
//                return;
//            }


            float rationCos;
            float rationSin;
            if (!(Math.Abs(cosLine) < 0.0001))
            {
                var radians = Math.Atan2(cosLine, sinLine);
                Angle = (float) (radians * 180 / Math.PI);
                rationCos = (float) Math.Cos(radians);
                rationSin = (float) Math.Sin(radians);
//                Width = cosLine / rationCos;
//                Height = (heightRectLeftLine / rationSin);
//
//                var startPageRadian = Math.Atan2(normalLeft, normalBottom);
//                var lineFromStartPage = Math.Sqrt(normalLeft * normalLeft + normalBottom * normalBottom);
//                Bottom = (float) (Math.Sin(startPageRadian + radians) * lineFromStartPage);
//                Left = (float) (Math.Cos(startPageRadian + radians) * lineFromStartPage);
            }
            else
            {
                rationCos = 0;
                if (sinLine < 0)
                {
                    Angle = 90;
                    rationSin = 1;
                }
                else
                {
                    Angle = 270;
                    rationSin = -1;
                }
            }

            var matrixRotation = new Matrix(-rationCos, rationSin,
                -rationSin, -rationCos, 0, 0);
            var transformedLeftBottom = leftBottom.Cross(matrixRotation);
            var transformedRightTop = rightTop.Cross(matrixRotation);
            Left = transformedLeftBottom.Get(Vector.I1);
            Bottom = transformedLeftBottom.Get(Vector.I2);
            Width = transformedRightTop.Get(Vector.I1) - transformedLeftBottom.Get(Vector.I1);
            Height = transformedRightTop.Get(Vector.I2) - transformedLeftBottom.Get(Vector.I2);
        }

        public static Vector RotateBack(float angle, float left, float bottom)
        {
            double radians = angle / 180 * Math.PI;
            var rationCos = (float) Math.Cos(radians);
            var rationSin = (float) Math.Sin(radians);
            var matrixRotation = new Matrix(rationCos, -rationSin,
                rationSin, rationCos, 0, 0);
            var leftBottom = new Vector(left, bottom, 0);
            var transformedLeftBottom = leftBottom.Cross(matrixRotation);
            return transformedLeftBottom;
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
                    Bottom += xToX; //reverse utb 
                Angle = 0;
            }
            else if (Math.Abs(xToX) < 1 && Math.Abs(yToY) < 1)
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