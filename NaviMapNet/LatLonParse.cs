using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace NaviMapNet
{
    internal class PointD
    {
        public double X;
        public double Y;
        public byte Type;

        public PointD() { }

        public PointD(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(int X, int Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(float X, float Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(double X, double Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public PointD(Point point, byte Type)
        {
            this.X = point.X;
            this.Y = point.Y;
            this.Type = Type;
        }

        public PointD(PointF point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public PointD(PointF point, byte Type)
        {
            this.X = point.X;
            this.Y = point.Y;
            this.Type = Type;
        }

        public PointF PointF
        {
            get
            {
                return new PointF((float)X, (float)Y);
            }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (X == 0) && (Y == 0);
            }
        }

        public static PointF ToPointF(PointD point)
        {
            return point.PointF;
        }

        public static PointF[] ToPointF(PointD[] points)
        {
            PointF[] result = new PointF[points.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = points[i].PointF;
            return result;
        }
    }

    internal class LatLonParser
    {
        public enum FFormat : byte
        {
            None = 0,
            DDDDDD = 1,
            DDMMMM = 2,
            DDMMSS = 3
        }

        public enum DFormat : byte
        {
            ENG_NS = 0,
            ENG_EW = 1,
            RUS_NS = 2,
            RUS_EW = 3,
            MINUS = 4,
            DEFAULT = 4,
            NONE = 4
        }

        public static double ToLat(string line_in)
        {
            return Parse(line_in, true);
        }

        public static double ToLon(string line_in)
        {
            return Parse(line_in, false);
        }

        public static double Parse(string line_in, bool true_lat_false_lon)
        {
            int nn = 1;
            string full = GetCorrectString(line_in, true_lat_false_lon, out nn);
            if (String.IsNullOrEmpty(full)) return 0f;
            string mm = "0";
            string ss = "0";
            string dd = "0";
            if (full.IndexOf("°") > 0)
            {
                int dms = 0;
                int from = 0;
                int next = 0;
                dd = full.Substring(from, (next = full.IndexOf("°", from)) - from);
                from = next + 1;
                if (full.IndexOf("'") > 0)
                {
                    dms = 1;
                    mm = full.Substring(from, (next = full.IndexOf("'", from)) - from);
                    from = next + 1;
                };
                if (full.IndexOf("\"") > 0)
                {
                    dms = 2;
                    ss = full.Substring(from, (next = full.IndexOf("\"", from)) - from);
                    from = next + 1;
                };
                if (from < full.Length)
                {
                    if (dms == 1)
                        ss = full.Substring(from);
                    else if (dms == 0)
                        mm = full.Substring(from);
                };
            }
            else
            {
                bool loop = true;
                double num3 = 0.0;
                int num4 = 1;
                if (full[0] == '-') num4++;
                while (loop)
                {
                    try
                    {
                        num3 = Convert.ToDouble(full.Substring(0, num4++), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        loop = false;
                    };
                    if (num4 > full.Length)
                    {
                        loop = false;
                    };
                }
                dd = num3.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            double d = ((Convert.ToDouble(dd, System.Globalization.CultureInfo.InvariantCulture) + Convert.ToDouble(mm, System.Globalization.CultureInfo.InvariantCulture) / 60.0 + Convert.ToDouble(ss, System.Globalization.CultureInfo.InvariantCulture) / 60.0 / 60.0) * (double)nn);
            return d;
        }

        public static PointD Parse(string line_in)
        {
            return new PointD(ToLon(line_in), ToLat(line_in));
        }

        private static string GetCorrectString(string str, bool lat, out int digit)
        {
            digit = 1;
            if (String.IsNullOrEmpty(str)) return null;

            string text = str.Trim();
            if (String.IsNullOrEmpty(text)) return null;

            text = text.ToLower().Replace("``", "\"").Replace("`", "'").Replace("%20", " ").Trim();
            while (text.IndexOf("  ") >= 0) text = text.Replace("  ", " ");
            text = text.Replace("° ", "°").Replace("' ", "'").Replace("\" ", "\"");
            if (String.IsNullOrEmpty(text)) return null;

            bool hasDigits = false;
            bool noletters = true;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                    hasDigits = true;
                if (char.IsLetter(text[i]))
                    noletters = false;
            };
            if (!hasDigits) return null;

            if (noletters)
            {
                string[] lalo = text.Split(new char[] { '+', ' ', '=', ';', ',', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lalo.Length == 0)
                    return null;
                if (lalo.Length == 2)
                {
                    if (lat)
                        text = lalo[0];
                    else
                        text = lalo[1];
                };
            };

            text = text.Replace("+", "").Replace(" ", "").Replace("=", "").Replace(";", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
            if (String.IsNullOrEmpty(text)) return null;

            double d;
            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
            {
                if (d < 0) digit = -1;
                return text.Replace("-", "");
            };

            int copyl = text.Length;
            int find = 0;
            int start = 0;
            bool endsWithLetter = (char.IsLetter(text[text.Length - 1]));

            if (lat)
            {
                if ((find = text.IndexOf("lat")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("latitude")) >= 0) start = find + (endsWithLetter ? 0 : 8);
                if ((find = text.IndexOf("ш")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("шир")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("широта")) >= 0) start = find + (endsWithLetter ? 0 : 6);
                if ((find = text.IndexOf("n")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("с")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("сш")) >= 0) start = find + (endsWithLetter ? 0 : 2);
                if ((find = text.IndexOf("с.ш")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("с.ш.")) >= 0) start = find + (endsWithLetter ? 0 : 4);
                if ((find = text.IndexOf("s")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("ю")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("юш")) >= 0) { start = find + (endsWithLetter ? 0 : 2); digit = -1; };
                if ((find = text.IndexOf("ю.ш")) >= 0) { start = find + (endsWithLetter ? 0 : 3); digit = -1; };
                if ((find = text.IndexOf("ю.ш.")) >= 0) { start = find + (endsWithLetter ? 0 : 4); digit = -1; };
            }
            else
            {
                if ((find = text.IndexOf("lon")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("longitude")) >= 0) start = find + (endsWithLetter ? 0 : 9);
                if ((find = text.IndexOf("д")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("дол")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("долгота")) >= 0) start = find + (endsWithLetter ? 0 : 7);
                if ((find = text.IndexOf("e")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("в")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("вд")) >= 0) start = find + (endsWithLetter ? 0 : 2);
                if ((find = text.IndexOf("в.д")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("в.д.")) >= 0) start = find + (endsWithLetter ? 0 : 4);
                if ((find = text.IndexOf("w")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("з")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("зд")) >= 0) { start = find + (endsWithLetter ? 0 : 2); digit = -1; };
                if ((find = text.IndexOf("з.д")) >= 0) { start = find + (endsWithLetter ? 0 : 3); digit = -1; };
                if ((find = text.IndexOf("з.д.")) >= 0) { start = find + (endsWithLetter ? 0 : 4); digit = -1; };
            };

            if (endsWithLetter)
            {
                copyl = start;
                start = 0;
                for (int i = copyl - 1; i >= start; i--)
                    if (char.IsLetter(text[i]))
                        copyl = copyl - (start = i + 1);
            }
            else
            {
                for (int i = start; i < copyl; i++)
                    if (char.IsLetter(text[i]))
                        copyl = i - start;
            };

            if (copyl > (text.Length - start)) copyl -= start;

            text = text.Substring(start, copyl);
            text = text.Replace(",", ".");
            return text;
        }

        public static string ToString(double fvalue)
        {
            return DoubleToString(fvalue, -1);
        }

        public static string ToString(double fvalue, int digitsAfterDelimiter)
        {
            return DoubleToString(fvalue, digitsAfterDelimiter);
        }

        public static string ToString(double lat, double lon)
        {
            return String.Format("{0},{1}", ToString(lat), ToString(lon));
        }

        public static string ToString(double lat, double lon, int digitsAfterDelimiter)
        {
            return String.Format("{0},{1}", DoubleToString(lat, digitsAfterDelimiter), DoubleToString(lon, digitsAfterDelimiter));
        }

        public static string ToString(double lat, double lon, FFormat fformat)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(lat, fformat), ToString(lon, fformat));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(lat, DFormat.ENG_NS), ToString(lat, fformat), GetLinePrefix(lat, DFormat.ENG_EW), ToString(lon, fformat) });
        }

        public static string ToString(double lat, double lon, FFormat fformat, int digitsAfterDelimiter)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(lat, fformat, digitsAfterDelimiter), ToString(lon, fformat, digitsAfterDelimiter));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(lat, DFormat.ENG_NS), ToString(lat, fformat, digitsAfterDelimiter), GetLinePrefix(lat, DFormat.ENG_EW), ToString(lon, fformat, digitsAfterDelimiter) });
        }

        public static string ToString(PointD latlon)
        {
            return String.Format("{0},{1}", ToString(latlon.Y), ToString(latlon.X));
        }

        public static string ToString(PointD latlon, int digitsAfterDelimiter)
        {
            return String.Format("{0},{1}", DoubleToString(latlon.Y, digitsAfterDelimiter), DoubleToString(latlon.X, digitsAfterDelimiter));
        }

        public static string ToString(PointD latlon, FFormat fformat)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(latlon.Y, fformat), ToString(latlon.X, fformat));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(latlon.Y, DFormat.ENG_NS), ToString(latlon.Y, fformat), GetLinePrefix(latlon.X, DFormat.ENG_EW), ToString(latlon.X, fformat) });
        }

        public static string ToString(PointD latlon, FFormat fformat, int digitsAfterDelimiter)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(latlon.Y, fformat, digitsAfterDelimiter), ToString(latlon.X, fformat, digitsAfterDelimiter));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(latlon.Y, DFormat.ENG_NS), ToString(latlon.Y, fformat, digitsAfterDelimiter), GetLinePrefix(latlon.X, DFormat.ENG_EW), ToString(latlon.X, fformat, digitsAfterDelimiter) });
        }

        public static string ToString(double fvalue, FFormat format)
        {
            return ToString(fvalue, format, 6);
        }

        public static string ToString(double fvalue, FFormat format, int digitsAfterDelimiter)
        {
            double num = Math.Abs(fvalue);
            string result;
            if (format == FFormat.None)
            {
                result = DoubleToString(num, digitsAfterDelimiter);
            }
            else if (format == FFormat.DDDDDD)
            {
                result = DoubleToString(num, digitsAfterDelimiter) + "°";
            }
            else
            {
                string text = "";
                text = text + DoubleToString(Math.Truncate(num), 0) + "° ";
                double num2 = (num - Math.Truncate(num)) * 60.0;
                if (format == FFormat.DDMMMM)
                {
                    text = text + DoubleToString(num2, 4) + "'";
                }
                else
                {
                    text = text + string.Format("{0}", (int)Math.Truncate(num2)) + "' ";
                    num2 = (num2 - Math.Truncate(num2)) * 60.0;
                    text = text + DoubleToString(num2, 3) + "\"";
                }
                result = text;
            }
            return result;
        }

        public static string GetLinePrefix(double fvalue, DFormat format)
        {
            string result;
            switch ((byte)format)
            {
                case 0:
                    result = ((fvalue >= 0.0) ? "N" : "S");
                    break;
                case 1:
                    result = ((fvalue >= 0.0) ? "E" : "W");
                    break;
                case 2:
                    result = ((fvalue >= 0.0) ? "С" : "Ю");
                    break;
                case 3:
                    result = ((fvalue >= 0.0) ? "В" : "З");
                    break;
                default:
                    result = ((fvalue >= 0.0) ? "" : "-");
                    break;
            }
            return result;
        }

        public static string DoubleToString(double val, int digitsAfterDelimiter)
        {
            if (digitsAfterDelimiter < 0)
                return val.ToString(System.Globalization.CultureInfo.InvariantCulture);

            string daf = "";
            for (int i = 0; i < digitsAfterDelimiter; i++) daf += "0";
            return val.ToString("0." + daf, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string DoubleToStringMax(double val, int maxDigitsAfterDelimiter)
        {
            string res = val.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (maxDigitsAfterDelimiter < 0) return res;
            if (res.IndexOf(".") < 0) return res;
            if ((res.Length - res.IndexOf(".")) <= maxDigitsAfterDelimiter) return res;

            string daf = "";
            for (int i = 0; i < maxDigitsAfterDelimiter; i++) daf += "0";
            return val.ToString("0." + daf, System.Globalization.CultureInfo.InvariantCulture);
        }
    }

}
