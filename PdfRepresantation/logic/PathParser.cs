using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfRepresantation
{
    public abstract class PathParser
    {
        protected readonly PageContext pageContext;

        internal PathParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

        

   
        protected IEnumerable<ShapeLine> ConvertLines(Path path, Matrix ctm,float height)
        {
            return from subpath in path.GetSubpaths()
                from line in subpath.GetSegments()
                select ConvertLine(line, ctm,height);
        }

        protected ShapeLine ConvertLine(IShape line, Matrix ctm, float height)
        {
            var points = line.GetBasePoints()
                .Select(p => ConvertPoint(p, ctm))
                .ToArray();
            var result = new ShapeLine
            {
                Start = points[0],
                End = points[points.Length - 1]
            };
            if (points.Length > 2)
            {
                result.CurveControlPoint1 = points[1];
                if (points.Length > 3)
                    result.CurveControlPoint2 = points[2];
            }

            return result;
        }
        protected abstract ShapePoint ConvertPoint(Point p, Matrix ctm);

    }
}