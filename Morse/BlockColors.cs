using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;


namespace Morse
{
    public static class BlockColors
    {
        private static byte[] RgbValues = {0, 42, 84, 126, 168, 210, 255};
        private static Dictionary<byte, Color> ValueColorMap = BuildValueColorMap();
        private static Color colorOfNull = Colors.White;

        public static Color Covert(byte? value)
        {
            return value == null ? colorOfNull : ValueColorMap[value.Value];
        }

        private static Dictionary<byte, Color> BuildValueColorMap()
        {
            Dictionary<byte, Color> map = new Dictionary<byte, Color>();
            int value = 0;
            for (byte r = 0; r < RgbValues.Length; r++)
            {
                for (byte g = 0; g < RgbValues.Length; g++)
                {
                    for (byte b = 0; b < RgbValues.Length; b++)
                    {
                        map[(byte)value] = Color.FromRgb(RgbValues[r], RgbValues[g], RgbValues[b]);
                        value++;
                        if (value == 256)
                        {
                            return map;
                        }
                    }
                }
            }

            return map;
        }


        public static byte? Covert(Color value)
        {
            byte r = GetRgbValue(value.R);
            byte g = GetRgbValue(value.G);
            byte b = GetRgbValue(value.B);

            if (colorOfNull.R == r && colorOfNull.G == g && colorOfNull.B == b)
            {
                return null;
            }
            
            for (var key = 0; key < 256; key++)
            {
                var valueColor = ValueColorMap[(byte)key];
                if(valueColor.R==r && valueColor.G==g && valueColor.B==b)
                {
                    return (byte) key;
                }

            }
            throw new ArgumentException("the Color value is invalid");
        }

        private static byte GetRgbValue(byte value)
        {
            if (Array.Exists(RgbValues, e => e == value))
            {
                return value;
            }

            foreach (var rgbValue in RgbValues)
            {
                if (Math.Abs(value - rgbValue) < 20)
                {
                    return rgbValue;
                }
            }

            throw new ArgumentException("the RGB value " + value + " is invalid");
        }

        public static string Print()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var keyValuePair in ValueColorMap)
            {
                sb.AppendLine("Key: "+keyValuePair.Key+" Value:"+keyValuePair.Value.R+","+keyValuePair.Value.G+","+keyValuePair.Value.B);
            }

            return sb.ToString();
        }
    }
}