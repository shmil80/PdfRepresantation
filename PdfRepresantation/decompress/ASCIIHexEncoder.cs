namespace PdfRepresantation
{
    public class ASCIIHexEncoder
    {
        public static byte[] ASCIIHexEncode(byte[] @in)
        {
            var @out = new byte[@in.Length * 2 + 1];
            for (int index = 0; index < @in.Length; ++index)
            {
                @out[index * 2] = (byte) (ToCharHex(@in[index] >> 4));
                @out[index * 2 + 1] = (byte) (ToCharHex(@in[index] & 0b00001111));
            }
            @out[@in.Length * 2] = (byte) '>';
            return @out;
        }

        private static int ToCharHex(int v)
        {
            return v <= 9 ? v + '0' : v - 10+ 'A' ;
        }
    }
}