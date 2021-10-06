using System;
using System.Windows.Media;


namespace Morse
{
    public static class BlockColors
    {
        private static readonly byte _opacity100 = 255;
        private static readonly byte _opacity75 = 192;
        private static readonly byte _opacity50 = 128;
        private static readonly byte _opacity25 = 64;

        private static Color _black = Colors.Black;
        private static Color _red = Colors.Red;
        private static Color _green = Colors.Green;
        private static Color _blue = Colors.Blue;
        private static Color _white = Colors.White;

        private static Color Black100 => Color.FromArgb(_opacity100, _black.R, _black.G, _black.B);
        private static Color Black75 => Color.FromArgb(_opacity75, _black.R, _black.G, _black.B);
        private static Color Black50 => Color.FromArgb(_opacity50, _black.R, _black.G, _black.B);
        private static Color Black25 => Color.FromArgb(_opacity25, _black.R, _black.G, _black.B);

        private static Color Red100 => Color.FromArgb(_opacity100, _red.R, _red.G, _red.B);
        private static Color Red75 => Color.FromArgb(_opacity75, _red.R, _red.G, _red.B);
        private static Color Red50 => Color.FromArgb(_opacity50, _red.R, _red.G, _red.B);
        private static Color Red25 => Color.FromArgb(_opacity25, _red.R, _red.G, _red.B);

        private static Color Green100 => Color.FromArgb(_opacity100, _green.R, _green.G, _green.B);
        private static Color Green75 => Color.FromArgb(_opacity75, _green.R, _green.G, _green.B);
        private static Color Green50 => Color.FromArgb(_opacity50, _green.R, _green.G, _green.B);
        private static Color Green25 => Color.FromArgb(_opacity25, _green.R, _green.G, _green.B);

        private static Color Blue100 => Color.FromArgb(_opacity100, _blue.R, _blue.G, _blue.B);
        private static Color Blue75 => Color.FromArgb(_opacity75, _blue.R, _blue.G, _blue.B);
        private static Color Blue50 => Color.FromArgb(_opacity50, _blue.R, _blue.G, _blue.B);
        private static Color Blue25 => Color.FromArgb(_opacity25, _blue.R, _blue.G, _blue.B);

        private static Color White => Color.FromArgb(_opacity100, _white.R, _white.G, _white.B);

        public static Color Covert(byte value)
        {
            if (value > 16)
            {
                throw new ArgumentException("the value is not in range");
            }

            switch (value)
            {
                case 0: return Black100;
                case 1: return Black75;
                case 2: return Black50;
                case 3: return Black25;

                case 4: return Red100;
                case 5: return Red75;
                case 6: return Red50;
                case 7: return Red25;

                case 8: return Green100;
                case 9: return Green75;
                case 10: return Green50;
                case 11: return Green25;

                case 12: return Blue100;
                case 13: return Blue75;
                case 14: return Blue50;
                case 15: return Blue25;
                
                case 16: return White;
                default: throw new ArgumentException("the value is not in range");
            }
        }


        public static byte Covert(Color value)
        {
            if (IsBlack(value))
            {
                if (IsOpacity100(value))
                {
                    return 0;
                }

                if (IsOpacity75(value))
                {
                    return 1;
                }

                if (IsOpacity50(value))
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }

            if (IsRed(value))
            {
                if (IsOpacity100(value))
                {
                    return 4;
                }

                if (IsOpacity75(value))
                {
                    return 5;
                }

                if (IsOpacity50(value))
                {
                    return 6;
                }
                else
                {
                    return 7;
                }
            }

            if (IsGreen(value))
            {
                if (IsOpacity100(value))
                {
                    return 8;
                }

                if (IsOpacity75(value))
                {
                    return 9;
                }

                if (IsOpacity50(value))
                {
                    return 10;
                }
                else
                {
                    return 11;
                }
            }

            if (IsBlue(value))
            {
                if (IsOpacity100(value))
                {
                    return 12;
                }

                if (IsOpacity75(value))
                {
                    return 13;
                }

                if (IsOpacity50(value))
                {
                    return 14;
                }
                else
                {
                    return 15;
                }
            }

            if (IsWhite(value))
            {
                return 16;
            }

            throw new ArgumentException("the value is not in range");
        }

        private static bool IsBlack(Color value)
        {
            return value.R <10 && value.G <10 && value.B <10;
        }

        private static bool IsRed(Color value)
        {
            return value.R > 250 && value.G < 10 && value.B < 10;
        }

        private static bool IsGreen(Color value)
        {
            return value.R < 10 && value.G > 250 && value.B < 10;
        }

        private static bool IsBlue(Color value)
        {
            return value.R < 10 && value.G < 10 && value.B > 250;
        }

        private static bool IsWhite(Color value)
        {
            return value.R > 250 && value.G > 250 && value.B > 250;
        }

        private static bool IsOpacity100(Color value)
        {
            return value.A > 250;
        }

        private static bool IsOpacity75(Color value)
        {
            return value.A > 185 && value.A < 250;
        }

        private static bool IsOpacity50(Color value)
        {
            return value.A > 100 && value.A < 185;
        }
    }
}