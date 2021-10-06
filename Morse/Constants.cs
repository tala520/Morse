namespace Morse
{
    public static class Constants
    {
        public const int DataRowCount = 32;
        public const int DataColCount = 64;
        
        public static int DataBlockByteSize => (Constants.DataColCount * Constants.DataRowCount) / 2;
        public const int FrameRate = 4;
        public const int Eof = 16;

        public const string Rev = "Rev";
        public const string Sed = "Sed";
    }
}