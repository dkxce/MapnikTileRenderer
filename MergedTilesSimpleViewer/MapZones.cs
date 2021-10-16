using System;
using System.Collections.Generic;
using System.Text;

namespace NaviMapNet
{
    public class Zone
    {
        public Zone() { }
        private static double MaxError = 1E-09;

        // Возврат величины ошибки MaxError
        public static double EPS
        {
            get { return MaxError; }
        }


        // Точка
        public class TPoint
        {
            public TPoint() { this.X = 0; this.Y = 0; }
            public double X;
            public double Y;
        }

        // Вектор
        public class TVector : TPoint
        {
            public TVector() { this.X = 0; this.Y = 0; this.Z = 0; }
            public double Z;
        }

        // Полигон
        public class TPolygon
        {
            public TPoint[] Dots;
            public TPolygon(ushort corners)
            {
                if (corners == 0) { Dots = null; }
                else
                {
                    this.Dots = new TPoint[corners];
                    for (ushort x = 0; x < corners; x++) this.Dots[x] = new TPoint();
                };
            }
        }

        // Эллипс
        public class TEllipse
        {
            public TPoint Center;
            public float XRadius;
            public float YRadius;
            public TEllipse() { Center = new TPoint(); XRadius = 1; YRadius = 1; }
        }

        // Круг
        public class TCircle
        {
            public TPoint Center;
            public float Radius;
            public TCircle() { Center = new TPoint(); Radius = 1; }
        }

        // Треугольник
        public class TTriangle
        {
            public TPoint APoint;
            public TPoint BPoint;
            public TPoint CPoint;
            private TPoint[] dots;
            public TTriangle()
            {
                this.dots = new TPoint[3];
                for (ushort x = 0; x < 3; x++) this.dots[x] = new TPoint();
                APoint = dots[0];
                BPoint = dots[1];
                CPoint = dots[2];
            }
        }


        // квадрат числа
        private static double sqr(double val)
        {
            return val * val;
        }


        // вхождение точки в круг
        public static bool PointInCircle(TPoint point, TCircle circle, double EPS)
        {
            return (bool)(((sqr(point.X - circle.Center.X) + sqr(point.Y - circle.Center.Y)) / sqr(circle.Radius)) <= (1 + EPS));
        }

        // вхождение точки в круг
        public static bool PointInCircle(TPoint point, TCircle circle)
        {
            return PointInCircle(point, circle, MaxError);
        }


        // вхождение точки в эллипс
        public static bool PointInEllipse(TPoint point, TEllipse ellipse, double EPS)
        {
            return (bool)((sqr(point.X - ellipse.Center.X) / sqr(ellipse.XRadius) + sqr(point.Y - ellipse.Center.Y) / sqr(ellipse.YRadius)) <= (1 + EPS));
        }

        // вхождение точки в эллипс
        public static bool PointInEllipse(TPoint point, TEllipse ellipse)
        {
            return (bool)PointInEllipse(point, ellipse, MaxError);
        }


        // число пересечений точки c отрезком
        private static int CRS(TPoint P, TPoint A1, TPoint A2, double EPS)
        {
            double x;
            int res = 0;
            if (Math.Abs(A1.Y - A2.Y) < EPS)
            {
                if ((Math.Abs(P.Y - A1.Y) < EPS) && ((P.X - A1.X) * (P.X - A2.X) < 0.0)) res = -1;
                return res;
            };
            if ((A1.Y - P.Y) * (A2.Y - P.Y) > 0.0) return res;
            x = A2.X - (A2.Y - P.Y) / (A2.Y - A1.Y) * (A2.X - A1.X);
            if (Math.Abs(x - P.X) < EPS)
            {
                res = -1;
            }
            else
            {
                if (x < P.X)
                {
                    res = 1;
                    if ((Math.Abs(A1.Y - P.Y) < EPS) && (A1.Y < A2.Y)) res = 0;
                    else
                        if ((Math.Abs(A2.Y - P.Y) < EPS) && (A2.Y < A1.Y)) res = 0;
                };
            };
            return res;
        }

        // вхождение точки в полигон
        public static bool PointInPolygon(TPoint point, TPolygon polygon, double EPS)
        {
            int count, up;
            count = 0;
            for (int i = 0; i < polygon.Dots.Length - 1; i++)
            {
                up = CRS(point, polygon.Dots[i], polygon.Dots[i + 1], EPS);
                if (up >= 0) count += up; else break;
            };
            up = CRS(point, polygon.Dots[polygon.Dots.Length - 1], polygon.Dots[0], EPS);
            if (up >= 0) return Convert.ToBoolean((count + up) & 1); else return false;
        }

        // вхождение точки в полигон
        public static bool PointInPolygon(TPoint point, TPolygon polygon)
        {
            return PointInPolygon(point, polygon, MaxError);
        }

        // пересечение полигонов
        public static bool PolygonInPolygon(TPolygon pol1, TPolygon pol2)
        {
            for(int i=0;i<pol1.Dots.Length;i++) if(PointInPolygon(pol1.Dots[i],pol2)) return true;
            for(int i=0;i<pol2.Dots.Length;i++) if(PointInPolygon(pol2.Dots[i],pol1)) return true;
            return false;
        }
        
        // пересечение прямоугольников
        private bool InRectangle(System.Drawing.Rectangle r1, System.Drawing.Rectangle r2)
        {
            return (r1.Left < r2.Right) && (r2.Left < r1.Right) && (r1.Top < r2.Bottom) && (r2.Top < r1.Bottom);
        }

        // вхождение точки в треугольник
        public static bool PointInTriangle(TPoint point, TTriangle triangle, double EPS)
        {
            TPolygon poly = new TPolygon(3);
            poly.Dots[0].X = triangle.APoint.X;
            poly.Dots[0].Y = triangle.APoint.Y;
            poly.Dots[1].X = triangle.BPoint.X;
            poly.Dots[1].Y = triangle.BPoint.Y;
            poly.Dots[2].X = triangle.CPoint.X;
            poly.Dots[2].Y = triangle.CPoint.Y;
            return PointInPolygon(point, poly, EPS);
        }

        // вхождение точки в треугольник
        public static bool PointInTriangle(TPoint point, TTriangle triangle)
        {
            return PointInTriangle(point, triangle, MaxError);
        }


        // попадание точки в линию
        public static bool PointInLine(TPoint point, TPolygon line)
        {
            return PointInLine(point, line, 3, 1);
        }

        // попадание точки в линию
        public static bool PointInLine(TPoint point, TPolygon line, int deltaX, double zoom)
        {
            for(int i=1;i<line.Dots.Length;i++)
            {
                double dx = (line.Dots[i].Y - line.Dots[i-1].Y) / (line.Dots[i].X - line.Dots[i-1].X);
                if (Math.Abs((point.X - line.Dots[i - 1].X) * dx - (point.Y - line.Dots[i - 1].Y)) < ((5 + deltaX) * zoom))
                    return true;
            };			
			return false;
        }

        // вхождение в составные зоны
        public static bool PointInDoubleZone(bool in_parentzone, bool not_in_childzone)
        { return in_parentzone & (!not_in_childzone); }

        // вхождение в составные зоны - Исключающий полигон в полигоне
        public static bool PointInDoubleZone(TPoint point, TPolygon in_parent_zone, TPolygon not_in_childzone, double EPS)
        { return PointInPolygon(point, in_parent_zone, EPS) & (!PointInPolygon(point, not_in_childzone, EPS)); }

        // вхождение в составные зоны - Исключающий полигон в полигоне
        public static bool PointInDoubleZone(TPoint point, TPolygon in_parent_zone, TPolygon not_in_childzone)
        { return PointInPolygon(point, in_parent_zone, MaxError) & (!PointInPolygon(point, not_in_childzone, MaxError)); }

        // вхождение в составные зоны - Исключающий эллипс в полигоне
        public static bool PointInDoubleZone(TPoint point, TPolygon in_parent_zone, TEllipse not_in_childzone, double EPS)
        { return PointInPolygon(point, in_parent_zone, EPS) & (!PointInEllipse(point, not_in_childzone, EPS)); }

        // вхождение в составные зоны - Исключающий эллипс в полигоне
        public static bool PointInDoubleZone(TPoint point, TPolygon in_parent_zone, TEllipse not_in_childzone)
        { return PointInPolygon(point, in_parent_zone, MaxError) & (!PointInEllipse(point, not_in_childzone, MaxError)); }

        // вхождение в составные зоны - Исключающий полигон в эллипсе
        public static bool PointInDoubleZone(TPoint point, TEllipse in_parent_zone, TPolygon not_in_childzone, double EPS)
        { return PointInEllipse(point, in_parent_zone, EPS) & (!PointInPolygon(point, not_in_childzone, EPS)); }

        // вхождение в составные зоны - Исключающий полигон в эллипсе
        public static bool PointInDoubleZone(TPoint point, TEllipse in_parent_zone, TPolygon not_in_childzone)
        { return PointInEllipse(point, in_parent_zone, MaxError) & (!PointInPolygon(point, not_in_childzone, MaxError)); }

        // вхождение в составные зоны - Исключающий эллипс в эллипсе
        public static bool PointInDoubleZone(TPoint point, TEllipse in_parent_zone, TEllipse not_in_childzone, double EPS)
        { return PointInEllipse(point, in_parent_zone, EPS) & (!PointInEllipse(point, not_in_childzone, EPS)); }

        // вхождение в составные зоны - Исключающий эллипс в эллипсе
        public static bool PointInDoubleZone(TPoint point, TEllipse in_parent_zone, TEllipse not_in_childzone)
        { return PointInEllipse(point, in_parent_zone, MaxError) & (!PointInEllipse(point, not_in_childzone, MaxError)); }

        public static double DegToRad(double deg)
        {
            return (deg / 180.0 * Math.PI);
        }

        // Рассчет расстояния       
        #region LENGTH
        public static uint GetLengthMeters(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            return GetLengthMetersC(StartLat, StartLong, EndLat, EndLong, radians);
        }
        public static uint GetLengthMetersA(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;     // Преобразование градусов в радианы

            double a = 6378137.0000;     // WGS-84 Equatorial Radius (a)
            double f = 1 / 298.257223563;  // WGS-84 Flattening (f)
            double b = (1 - f) * a;      // WGS-84 Polar Radius
            double e2 = (2 - f) * f;      // WGS-84 Квадрат эксцентричности эллипсоида  // 1-(b/a)^2

            // Переменные, используемые для вычисления смещения и расстояния
            double fPhimean;                           // Средняя широта
            double fdLambda;                           // Разница между двумя значениями долготы
            double fdPhi;                           // Разница между двумя значениями широты
            double fAlpha;                           // Смещение
            double fRho;                           // Меридианский радиус кривизны
            double fNu;                           // Поперечный радиус кривизны
            double fR;                           // Радиус сферы Земли
            double fz;                           // Угловое расстояние от центра сфероида
            double fTemp;                           // Временная переменная, использующаяся в вычислениях

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            // предположительно что расстояние между точками << радиуса земли
            if (!radians)
            {
                fdLambda = (StartLong - EndLong) * D2R;
                fdPhi = (StartLat - EndLat) * D2R;
                fPhimean = ((StartLat + EndLat) / 2) * D2R;
            }
            else
            {
                fdLambda = StartLong - EndLong;
                fdPhi = StartLat - EndLat;
                fPhimean = (StartLat + EndLat) / 2;
            };

            // Вычисляем меридианные и поперечные радиусы кривизны средней широты
            fTemp = 1 - e2 * (sqr(Math.Sin(fPhimean)));
            fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние
            if (!radians)
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * sqr(Math.Sin(fdLambda / 2.0)));
            }
            else
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat) * Math.Cos(StartLat) * sqr(Math.Sin(fdLambda / 2.0)));
            };
            fz = 2 * Math.Asin(fz);

            // Вычисляем смещение
            if (!radians)
            {
                fAlpha = Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            }
            else
            {
                fAlpha = Math.Cos(EndLat) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            };
            fAlpha = Math.Asin(fAlpha);

            // Вычисляем радиус Земли
            fR = (fRho * fNu) / (fRho * sqr(Math.Sin(fAlpha)) + fNu * sqr(Math.Cos(fAlpha)));
            // Получаем расстояние
            return (uint)Math.Round(Math.Abs(fz * fR));
        }
        public static uint GetLengthMetersB(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double fPhimean, fdLambda, fdPhi, fAlpha, fRho, fNu, fR, fz, fTemp, Distance,
                D2R = Math.PI / 180,
                a = 6378137.0,
                e2 = 0.006739496742337;
            if (radians) D2R = 1;

            fdLambda = (StartLong - EndLong) * D2R;
            fdPhi = (StartLat - EndLat) * D2R;
            fPhimean = (StartLat + EndLat) / 2.0 * D2R;

            fTemp = 1 - e2 * Math.Pow(Math.Sin(fPhimean), 2);
            fRho = a * (1 - e2) / Math.Pow(fTemp, 1.5);
            fNu = a / Math.Sqrt(1 - e2 * Math.Sin(fPhimean) * Math.Sin(fPhimean));

            fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
              Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));
            fAlpha = Math.Asin(Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) / Math.Sin(fz));
            fR = fRho * fNu / (fRho * Math.Pow(Math.Sin(fAlpha), 2) + fNu * Math.Pow(Math.Cos(fAlpha), 2));
            Distance = fz * fR;

            return (uint)Math.Round(Distance);
        }
        public static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;
            if (radians) D2R = 1;
            double dDistance = Double.MinValue;
            double dLat1InRad = StartLat * D2R;
            double dLong1InRad = StartLong * D2R;
            double dLat2InRad = EndLat * D2R;
            double dLong2InRad = EndLong * D2R;

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            const double kEarthRadiusKms = 6378137.0000;
            dDistance = kEarthRadiusKms * c;

            return (uint)Math.Round(dDistance);
        }
        public static double GetLengthMetersD(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            return EarthRadius * (Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2)));
        }
        public static double GetLengthMetersE(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            /* This algorithm is called Sinnott's Formula */
            double dlon = (lon2) - (lon1);
            double dlat = (lat2) - (lat1);
            double a = Math.Pow(Math.Sin(dlat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2.0);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadius * c;
        }
        #endregion LENGTH

        //{ Перевод из географической в декартовую систему координат }
        public static TVector GeographicToDecVector(double Lat, double Long, double radius, bool radians)
        {
            double D2R = Math.PI / 180;
            double conv = 1;
            TVector result = new TVector();
            if (!radians) conv = D2R;
            try
            {
                result.X = radius * Math.Cos(Long * conv) * Math.Sin(Lat * conv);
                result.Y = radius * Math.Sin(Long * conv) * Math.Sin(Lat * conv);
                result.Z = radius * Math.Cos(Lat * conv);
                return result;
            }
            catch { return result; }
        }

        //{ Перевод из географической в декартовую систему координат }
        public static TVector GeographicToDecVector(double Lat, double Long, bool radians)
        {
            return GeographicToDecVector(Lat, Long, 1, radians);
        }

        //{ Нахождение нормали к поверхности, заданной двумя векторами}
        public static TVector GetNormalVectorFrom2DecVectors(TVector A, TVector B)
        {
            TVector C = new TVector();
            try
            {
                C.X = A.Y * B.Z - B.Y * A.Z;
                C.Y = A.Z * B.X - B.Z * A.X;
                C.Z = A.X * B.Y - B.X * A.Y;
                return C;
            }
            catch { return C; };
        }
        //{ Нахождение нормали к поверхности, заданной двумя точками трека }
        public static TVector GetNormalVectorFromGeo(double StartLat, double StartLong, double EndLat, double EndLong, bool Radians)
        {
            try { return GetNormalVectorFrom2DecVectors(GeographicToDecVector(StartLat, StartLong, Radians), GeographicToDecVector(EndLat, EndLong, Radians)); }
            catch { return null; };
        }
        // Нахождение угла между векторами
        public static double GetAngleBetween2NormalVectors(TVector A, TVector B, bool Radians)
        {
            double R2D = 180 / Math.PI;     // Преобразование радиан в градусы
            double dev, result;
            try
            {
                dev = Math.Sqrt(sqr(A.X) + sqr(A.Y) + sqr(A.Z)) * Math.Sqrt(sqr(B.X) + sqr(B.Y) + sqr(B.Z));
                if (dev > EPS) result = (A.X * B.X + A.Y * B.Y + A.Z * B.Z) / dev; else result = 0;
                result = Math.Acos(result);
                if (!Radians) return result * R2D; else return result;
            }
            catch { return 0; };
        }
    }
}
