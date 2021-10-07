namespace Morse
{
    public static class Constants
    {
        public const int DataRowCount = 16;
        public const int DataColCount = 32;
        public static int DataBlockByteSize => (Constants.DataColCount * Constants.DataRowCount);
        public const int FrameRate = 1;

        public const string Rev = "Rev";
        public const string Sed = "Sed";
    }
}