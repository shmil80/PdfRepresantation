using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfRepresantation
{
    class LinesGenerator
    {
        private readonly PageContext pageContext;

        public LinesGenerator(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }


        public IList<PdfTextLineDetails> CreateLines(IList<PdfTextBlock> texts)
        {
            AssignStartEnd(texts);
            var textGroups = texts
                .OrderBy(t => t.Rotation)
                .ThenBy(t => t.Start)
                .GroupBy(t => (int) Math.Round(t.Bottom * 2))
                .OrderByDescending(g => g.Key)
                .ToArray();
            FindSpace(textGroups);
            return textGroups
                .SelectMany(g => new LineGenarator(pageContext, g).Lines)
                .ToList();
        }

        private void FindSpace(IGrouping<int, PdfTextBlock>[] textGroups)
        {
            var result = FindDistanceAfter(textGroups);
            if (result.Count == 0)
                return;
            var fontHead = FindHead(result);
            var regulars = new List<PdfTextBlock>();
            var heads = new List<PdfTextBlock>();
            foreach (var text in result)
            {
                if (text.FontSize >= fontHead)
                    heads.Add(text);
                else
                    regulars.Add(text);
            }

            FindSpace(regulars,350);
            FindSpace(heads,100);
        }

        private void FindSpace(List<PdfTextBlock> result,int minStart)
        {
            if (result.Count == 0)
                return;
            float? val = null;
            float? last = null;
            float? lastDiff = null;
            int start;
            if (result.Count <= minStart)
                start = 0;
            else// if (result.Count <= minStart * 2)
                start = result.Count - minStart;
            //else
                //start = result.Count / 2;
            for (var index = start; index < result.Count; index++)
            {
                var current = result[index];
                if (current.DistanceAfter < -0.5)
                    continue;
                if (last != null)
                {
                    var diff = current.DistanceAfter - last;
                    if (diff == 0)
                        continue;
                    if (lastDiff != null)
                    {
                        if (diff > 0.3 && diff / lastDiff > 1.5)
                        {
                            var porportion = current.DistanceAfter / last.Value;
                            if (porportion > 2.5 || porportion < 0)
                            {
                                val = current.DistanceAfter;
                                break;
                            }
                        }
                    }

                    lastDiff = diff;
                }

                if (index > start + 3)
                    last = result[index - 3].DistanceAfter;
            }

            //val = 5F;
            if (val == null)
                val = result[result.Count - 1].DistanceAfter + 1;
            foreach (var block in result)
            {
                block.BigSpace = val.Value;
            }
        }

        internal double FindHead(List<PdfTextBlock> blocks)
        {
            var groups = blocks
                .GroupBy(c => Math.Round(c.FontSize, 1))
                .OrderByDescending(g => g.Key)
                .Select(g => new
                {
                    FontSize = g.Key,
                    Count = g.Count()
                })
                .ToArray();
            var sum = 0;
            double result = 0;
            foreach (var group in groups)
            {
                sum += group.Count;
                if (sum < blocks.Count / 2.5)
                {
                    result = group.FontSize;
                }
                else break;
            }

            return result - 0.05;
        }

        private static List<PdfTextBlock> FindDistanceAfter(IGrouping<int, PdfTextBlock>[] textGroups)
        {
            List<PdfTextBlock> result = new List<PdfTextBlock>();
            foreach (var group in textGroups)
            {
                var list = @group.ToArray();
                if (list.Length == 0)
                    continue;
                var last = list[0];
                for (int i = 1; i < list.Length; i++)
                {
                    var current = list[i];
                    if (string.IsNullOrWhiteSpace(current.Value))
                        continue;
                    last.DistanceAfter = current.Start - last.End;
                    result.Add(last);
                    last = current;
                }
            }


            result.Sort(PdfTextBlock.DistanceAfterComparer);
            return result;
        }

        private void AssignStartEnd(IList<PdfTextBlock> texts)
        {
            foreach (var t in texts)
            {
                if (pageContext.PageRTL)
                {
                    if (t.Value == " " && t.Width < t.SpaceWidth)
                    {
                        t.Left = t.Right - t.SpaceWidth;
                        t.Width = t.SpaceWidth;
                    }

                    t.Start = pageContext.PageWidth - t.Right;
                    t.End = pageContext.PageWidth - t.Left;
                }
                else
                {
                    if (t.Value == " " && t.Width < t.SpaceWidth)
                        t.Width = t.SpaceWidth;
                    t.Start = t.Left;
                    t.End = t.Right;
                }
            }
        }
    }
}