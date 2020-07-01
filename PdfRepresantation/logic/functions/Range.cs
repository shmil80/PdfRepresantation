using System;

namespace PdfRepresantation
{
    public class Range<T> where T : IComparable<T>
    {
        public T Min, Max;
        public T Cut(T value)
        {
            if (Min.CompareTo(value) >= 0)
                return Min;
            if (Max.CompareTo(value) <= 0)
                return Max;
            return value;
        }
    }
    public class Range : Range<float>
    {
        public float Length => Max - Min;

        public static Range[] CreateArray(float[] source)
        {
            var result = new Range[source.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Range {Min = source[i * 2], Max = source[i * 2 + 1]};
            }

            return result;
        }
        public static float Interpolate(float input, Range source, Range target)
        {
            return target.Min + (input - source.Min) * target.Length / source.Length;
        }

    }

}