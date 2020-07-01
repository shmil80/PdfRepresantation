using System;

namespace PdfRepresantation
{
    class SampleBitConverter
    {
        public static int[] ConvertBits(byte[] sampleData, int bitPerSample)
        {
            int[] result = new int[sampleData.Length * bitPerSample / 8];
            switch (bitPerSample)
            {
                case 1:
                    for (var i = 0; i < sampleData.Length; i++)
                    {
                        byte b = sampleData[i];
                        int start = i * 8;
                        result[start + 0] = (b & 0b00000001);
                        result[start + 1] = (b & 0b00000010) >> 1;
                        result[start + 2] = (b & 0b00000100) >> 2;
                        result[start + 3] = (b & 0b00001000) >> 3;
                        result[start + 4] = (b & 0b00010000) >> 4;
                        result[start + 5] = (b & 0b00100000) >> 5;
                        result[start + 6] = (b & 0b01000000) >> 6;
                        result[start + 7] = b >> 7;
                    }

                    break;
                case 2:
                    for (var i = 0; i < sampleData.Length; i++)
                    {
                        byte b = sampleData[i];
                        int start = i * 4;
                        result[start + 0] = (b & 0b00000011);
                        result[start + 1] = (b & 0b00001100) >> 2;
                        result[start + 2] = (b & 0b00110000) >> 4;
                        result[start + 3] = b >> 6;
                    }

                    break;
                case 4:
                    for (var i = 0; i < sampleData.Length; i++)
                    {
                        byte b = sampleData[i];
                        int start = i * 2;
                        result[start + 0] = (b & 0b00001111);
                        result[start + 1] = b >> 4;
                    }

                    break;
                case 8: break;
                case 12:
                    for (var i = 0; i < sampleData.Length; i += 3)
                    {
                        byte b1 = sampleData[i];
                        byte b2 = sampleData[i + 1];
                        byte b3 = sampleData[i + 2];
                        int start = i * 2 / 3;
                        result[start] = (b1 << 4) | (b2 >> 4);
                        result[start + 1] = ((b2 & 0b00001111) << 8) | b3;
                    }

                    break;
                case 16:
                    for (var i = 0; i < sampleData.Length; i += 2)
                    {
                        result[i / 2] = BitConverter.ToInt16(sampleData, i);
                    }

                    break;
                case 24:
                    for (var i = 0; i < sampleData.Length; i += 3)
                    {
                        byte b1 = sampleData[i];
                        byte b2 = sampleData[i + 1];
                        byte b3 = sampleData[i + 2];
                        result[i / 3] = (b1 << 16) | (b2 << 8) | b3;
                    }

                    break;
                case 32:
                    for (var i = 0; i < sampleData.Length; i += 4)
                    {
                        result[i / 4] = BitConverter.ToInt32(sampleData, i);
                    }

                    break;
            }

            return result;
        }

    }
}