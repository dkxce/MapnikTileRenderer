using System;
using System.Collections.Generic;
using System.Text;

namespace NaviMapNet
{
    public class Geometry
    {
        public Geometry() { }
                
        // Возврат величины ошибки MaxError
        private static double MaxError = 1E-09;
        public static double EPS { get { return MaxError; } }

        public enum TObjectClass
        {
            Point = 0x01,
            Vector = 0x03,
            Multipoint = 0x04,
            Line = 0x0C,
            Polygon = 0x14,
            Polyline = 0x24,
            Triangle = 0x44,
            Ellipse = 0x81,
            Circle = 0x181
        }

        public abstract class TObject
        {
            public TObjectClass Class { get { return GetClass(); } }
            public abstract TObjectClass GetClass();
            
            public long Count { get { return GetCount(); } }
            public abstract long GetCount();
            
            public double[] Bounds { get { return GetBounds(); } }
            public System.Drawing.RectangleF Rect
            { 
                get 
                {
                    double[] b = GetBounds();
                    return new System.Drawing.RectangleF((float)b[0], (float)b[1], (float)(b[2] - b[0]), (float)(b[3] - b[1]));
                } 
            }
            public System.Drawing.SizeF Size
            { 
                get 
                { 
                    double[] b = GetBounds();
                    return new System.Drawing.SizeF((float)Math.Round(b[2]-b[0]),(float)Math.Round(b[3]-b[1]));
                } 
            }
            public double Width
            {
                get
                {
                    double[] b = GetBounds();
                    return Math.Round(b[2] - b[0]);
                }
            }
            public double Height
            {
                get
                {
                    double[] b = GetBounds();
                    return Math.Round(b[3] - b[1]);
                }
            }
            public abstract double[] GetBounds();
        }

        // Точка
        public class TPoint : TObject
        {
            public override TObjectClass GetClass() { return TObjectClass.Point; }
            public override long GetCount() { return 1; }
            public override double[] GetBounds() { return new double[] { X, Y, X, Y }; }

            public double X;
            public double Y;

            public TPoint() { this.X = 0; this.Y = 0; }
            public TPoint(int X, int Y) { this.X = X; this.Y = Y; }
            public TPoint(double X, double Y) { this.X = X; this.Y = Y; }
            public TPoint(System.Drawing.Point p) { this.X = p.X; this.Y = p.Y; }
            public TPoint(System.Drawing.PointF p) { this.X = p.X; this.Y = p.Y; }

            public static TPoint FromXY(int x, int y) { return new TPoint(x, y); }
            public static TPoint FromXY(int[] xy) { return new TPoint(xy[0], xy[1]); }
            public static TPoint FromXY(double x, double y) { return new TPoint(x, y); }
            public static TPoint FromXY(double[] xy) { return new TPoint(xy[0], xy[1]); }
            public static TPoint FromXY(System.Drawing.Point p) { return new TPoint(p); }
            public static TPoint[] FromXY(System.Drawing.Point[] ps)
            {
                if (ps == null) return null;
                if (ps.Length == 0) return new TPoint[0];
                TPoint[] res = new TPoint[ps.Length];
                for (int i = 0; i < res.Length; i++) res[i] = (TPoint)ps[i];
                return res;
            }
            public static TPoint FromXY(System.Drawing.PointF p) { return new TPoint(p); }
            public static TPoint[] FromXY(System.Drawing.PointF[] ps)
            {
                if (ps == null) return null;
                if (ps.Length == 0) return new TPoint[0];
                TPoint[] res = new TPoint[ps.Length];
                for (int i = 0; i < res.Length; i++) res[i] = (TPoint)ps[i];
                return res;
            }
            public static TPoint FromLatLon(double Lat, double Lon) { return new TPoint(Lon, Lat); }                        
            
            // Point
            public static explicit operator TPoint(System.Drawing.Point p) { return new TPoint(p.X,p.Y); }
            public static explicit operator System.Drawing.Point(TPoint p) { return new System.Drawing.Point((int)p.X, (int)p.Y); }

            // PointF
            public System.Drawing.PointF AsPointF { get { return new System.Drawing.PointF((float)X, (float)Y); } set { X = value.X; Y = value.Y; } }            
            public static explicit operator TPoint(System.Drawing.PointF p) { return new TPoint(p.X, p.Y); }            
            public static explicit operator System.Drawing.PointF(TPoint p) { return new System.Drawing.PointF((float)p.X, (float)p.Y); }

            // int[]
            public static explicit operator TPoint(int[] xy) { return new TPoint(xy[0], xy[1]); }
            public static explicit operator int[](TPoint p) { return new int[] { (int)p.X, (int)p.Y }; }     

            // double[]
            public static explicit operator TPoint(double[] xy) { return new TPoint(xy[0], xy[1]); }            
            public static explicit operator double[](TPoint p) { return new double[] { p.X, p.Y }; }        
    
            // 
            public override string ToString()
            {
                return "{"+X.ToString()+";"+Y.ToString()+"}";
            }
        }

        // Мультиточечный объект
        public class TPoints : TObject 
        {
            public override TObjectClass GetClass() { return TObjectClass.Multipoint; }
            public override long GetCount() { return Dots == null ? 0 : (long)Dots.Length; }
            public override double[] GetBounds() 
            {
                double[] res = new double[] {double.MaxValue, double.MaxValue, double.MinValue, double.MinValue};
                for (int i = 0; i < Dots.Length; i++)
                {
                    res[0] = Math.Min(res[0],Dots[i].X);
                    res[1] = Math.Min(res[1],Dots[i].Y);
                    res[2] = Math.Max(res[0],Dots[i].X);
                    res[3] = Math.Max(res[1],Dots[i].Y);
                };
                return res;
            }

            public TPoint[] Dots;
            public System.Drawing.PointF[] AsPointF
            {
                get
                {
                    if (Dots == null) return null;
                    System.Drawing.PointF[] res = new System.Drawing.PointF[Dots.Length];
                    if (Dots.Length == 0) return res;
                    for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.PointF)Dots[i];
                    return res;
                }
                set
                {
                    if (value == null) Dots = null;
                    Dots = new TPoint[value.Length];
                    if (Dots.Length == 0) return;
                    for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)value[i];
                }
            }
            public TPoints() { this.Dots = new TPoint[0]; }
            public TPoints(TPoint[] Dots) { this.Dots = Dots; }
            public TPoints(System.Drawing.Point[] points) 
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }
            public TPoints(System.Drawing.PointF[] points)
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }            

            public static explicit operator TPoints(System.Drawing.Point[] ps) { return new TPoints(ps); }
            public static explicit operator TPoints(System.Drawing.PointF[] ps) { return new TPoints(ps); }
            public static explicit operator System.Drawing.Point[](TPoints p)
            {
                System.Drawing.Point[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.Point[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.Point)p.Dots[i];
                return res;
            }
            public static explicit operator System.Drawing.PointF[](TPoints p)
            {
                System.Drawing.PointF[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.PointF[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.PointF)p.Dots[i];
                return res;
            }

            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Dots.Length; i++) s += (i > 0 ? "," : "") + "{" + Dots[i].X.ToString() + ";" + Dots[i].Y.ToString() + "}";
                return s;
            }
        }

        // Вектор
        public class TVector : TPoint
        {
            public override TObjectClass GetClass() { return TObjectClass.Vector; }
            public override long GetCount() { return 1; }

            public TVector() { this.X = 0; this.Y = 0; this.Z = 0; }
            public double Z;            
            public override string ToString()
            {
                return "{" + X.ToString() + ";" + Y.ToString() + ";"+Z.ToString()+"}";
            }
        }

        // Линия
        public class TLine : TPoints
        {
            public override TObjectClass GetClass() { return TObjectClass.Line; }

            public TLine()
            {
                this.Dots = new TPoint[]{new TPoint(),new TPoint()};
            }
            public TLine(TPoint start, TPoint end)
            {
                this.Dots = new TPoint[]{start,end};
            }
            public TLine(System.Drawing.Point start, System.Drawing.Point end)
            {
                this.Dots = new TPoint[]{(TPoint)start, (TPoint)end};
            }
            public TLine(System.Drawing.PointF start, System.Drawing.PointF end)
            {
                this.Dots = new TPoint[]{(TPoint)start, (TPoint)end};
            }
            public TLine(TPoint[] Dots) { this.Dots = Dots; }
            public TLine(System.Drawing.Point[] points) 
            {
                this.Dots = new TPoint[]{(TPoint)points[0],(TPoint)points[1]};
            }
            public TLine(System.Drawing.PointF[] points)
            {
                this.Dots = new TPoint[] { (TPoint)points[0], (TPoint)points[1] };
            }
            public static explicit operator TLine(System.Drawing.Point[] ps) { return new TLine(ps); }
            public static explicit operator TLine(System.Drawing.PointF[] ps) { return new TLine(ps); }
            public static explicit operator System.Drawing.Point[](TLine p) 
            {
                return new System.Drawing.Point[] {(System.Drawing.Point)p.Dots[0],(System.Drawing.Point)p.Dots[1]};
            }
            public static explicit operator System.Drawing.PointF[](TLine p)
            {
                return new System.Drawing.PointF[] { (System.Drawing.PointF)p.Dots[0], (System.Drawing.PointF)p.Dots[1] };
            }

            public bool HasPoint(TPoint p)
            {
                return GetPointIndex(p) >= 0;
            }
            public int GetPointIndex(TPoint p)
            {
                if ((Dots == null) || (Dots.Length == 0)) return -1;
                for (int i = 0; i < Dots.Length; i++)
                    if ((Dots[i].X == p.X) && (Dots[i].Y == p.Y))
                        return i;
                return -1;
            }

            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Dots.Length; i++) s += (i > 0 ? "," : "") + "{" + Dots[i].X.ToString() + ";" + Dots[i].Y.ToString() + "}";
                return s;
            }
        }

        // Полигон
        public class TPolygon : TPoints
        {
            public override TObjectClass GetClass() { return TObjectClass.Polygon; }

            public TPolygon(ushort corners)
            {
                if (corners == 0) { Dots = null; }
                else
                {
                    this.Dots = new TPoint[corners];
                    for (ushort x = 0; x < corners; x++) this.Dots[x] = new TPoint();
                };
            }
            public TPolygon(TPoint[] Dots) { this.Dots = Dots; }
            public TPolygon(System.Drawing.Point[] points) 
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }
            public TPolygon(System.Drawing.PointF[] points)
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }
            public static explicit operator TPolygon(System.Drawing.Point[] ps) { return new TPolygon(ps); }
            public static explicit operator TPolygon(System.Drawing.PointF[] ps) { return new TPolygon(ps); }
            public static explicit operator System.Drawing.Point[](TPolygon p) 
            {
                System.Drawing.Point[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.Point[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.Point)p.Dots[i];
                return res;
            }
            public static explicit operator System.Drawing.PointF[](TPolygon p)
            {
                System.Drawing.PointF[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.PointF[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.PointF)p.Dots[i];
                return res;
            }

            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Dots.Length; i++) s += (i > 0 ? "," : "") + "{" + Dots[i].X.ToString() + ";" + Dots[i].Y.ToString() + "}";
                return s;
            }
        }

        // Линия
        public class TPolyline : TPoints
        {
            public override TObjectClass GetClass() { return TObjectClass.Polyline; }

            public TPoint[] Dots;
            public TPolyline(ushort corners)
            {
                if (corners == 0) { Dots = null; }
                else
                {
                    this.Dots = new TPoint[corners];
                    for (ushort x = 0; x < corners; x++) this.Dots[x] = new TPoint();
                };
            }
            public TPolyline(TPoint[] Dots) { this.Dots = Dots; }
            public TPolyline(System.Drawing.Point[] points) 
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }
            public TPolyline(System.Drawing.PointF[] points)
            {
                if (points == null) return;
                Dots = new TPoint[points.Length];
                if (Dots.Length == 0) return;
                for (int i = 0; i < Dots.Length; i++) Dots[i] = (TPoint)points[i];
            }
            public static explicit operator TPolyline(System.Drawing.Point[] ps) { return new TPolyline(ps); }
            public static explicit operator TPolyline(System.Drawing.PointF[] ps) { return new TPolyline(ps); }
            public static explicit operator System.Drawing.Point[](TPolyline p) 
            {
                System.Drawing.Point[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.Point[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.Point)p.Dots[i];
                return res;
            }
            public static explicit operator System.Drawing.PointF[](TPolyline p)
            {
                System.Drawing.PointF[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.PointF[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.PointF)p.Dots[i];
                return res;
            }

            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Dots.Length; i++) s += (i > 0 ? "," : "") + "{" + Dots[i].X.ToString() + ";" + Dots[i].Y.ToString() + "}";
                return s;
            }
        }

        // Эллипс
        public class TEllipse : TPoint
        {
            public override TObjectClass GetClass() { return TObjectClass.Ellipse; }
            public override double[] GetBounds() { return new double[]{X-XRadius,Y-YRadius,X+YRadius,Y+YRadius}; }

            public TPoint Center { get { return new TPoint(X, Y); } set { this.X = value.X; this.Y = value.Y; } }
            public double XRadius;
            public double YRadius;
            public TEllipse() { Center = new TPoint(); XRadius = 1; YRadius = 1; }
            public TEllipse(TPoint center) { Center = center; XRadius = 1; YRadius = 1; }
            public TEllipse(System.Drawing.Point center) { Center = (TPoint)center; XRadius = 1; YRadius = 1; }
            public TEllipse(System.Drawing.PointF center) { Center = (TPoint)center; XRadius = 1; YRadius = 1; }
            public TEllipse(TPoint center, double XRadius, double YRadius) { Center = center; this.XRadius = XRadius; this.YRadius = YRadius; }
            public TEllipse(System.Drawing.Point center, float XRadius, float YRadius) { Center = (TPoint)center; this.XRadius = XRadius; this.YRadius = YRadius; }
            public TEllipse(System.Drawing.PointF center, float XRadius, float YRadius) { Center = (TPoint)center; this.XRadius = XRadius; this.YRadius = YRadius; }

            public static implicit operator TCircle(TEllipse ell) { return new TCircle(ell.Center, Math.Max(ell.XRadius, ell.YRadius)); }
            public static implicit operator TEllipse(TCircle ell) { return new TEllipse(ell.Center, ell.Radius, ell.Radius); }

            public override string ToString()
            {
                return "{" + Center.X.ToString() + ":" + XRadius.ToString() + ";" + Center.Y.ToString() + ":"+YRadius.ToString()+"}";
            }
        }

        // Круг
        public class TCircle : TPoint
        {
            public override TObjectClass GetClass() { return TObjectClass.Circle; }
            public override double[] GetBounds() { return new double[] { X - Radius, Y - Radius, X + Radius, Y + Radius }; }

            public TPoint Center { get { return new TPoint(X, Y); } set { this.X = value.X; this.Y = value.Y; } }
            public double Radius;
            public TCircle() { Center = new TPoint(); Radius = 1; }
            public TCircle(TPoint center) { Center = center; Radius = 1; }
            public TCircle(System.Drawing.Point center) { Center = (TPoint)center; Radius = 1; }
            public TCircle(System.Drawing.PointF center) { Center = (TPoint)center; Radius = 1; }
            public TCircle(TPoint center, double radius) { Center = center; this.Radius = radius; }
            public TCircle(System.Drawing.Point center, float radius) { Center = (TPoint)center; this.Radius = radius; }
            public TCircle(System.Drawing.PointF center, float radius) { Center = (TPoint)center; this.Radius = radius; }

            public static implicit operator TCircle(TEllipse ell) { return new TCircle(ell.Center,Math.Max(ell.XRadius,ell.YRadius)); }
            public static implicit operator TEllipse(TCircle ell) { return new TEllipse(ell.Center, ell.Radius, ell.Radius); }

            public override string ToString()
            {
                return "{" + Center.X.ToString() + ":" + Radius.ToString() + ";" + Center.Y.ToString() + ":" + Radius.ToString() + "}";
            }
        }

        // Треугольник
        public class TTriangle : TPoints
        {
            public override TObjectClass GetClass() { return TObjectClass.Triangle; }

            public TPoint APoint;
            public TPoint BPoint;
            public TPoint CPoint;
            
            public TTriangle()
            {
                this.Dots = new TPoint[3];
                for (ushort x = 0; x < 3; x++) this.Dots[x] = new TPoint();
                APoint = Dots[0];
                BPoint = Dots[1];
                CPoint = Dots[2];
            }
            public TTriangle(TPoint[] ps)
            {
                this.Dots = ps;
                for (ushort x = 0; x < 3; x++) this.Dots[x] = new TPoint();
                APoint = Dots[0];
                BPoint = Dots[1];
                CPoint = Dots[2];
            }
            public TTriangle(TPolygon p)
            {
                this.Dots = p.Dots;
                for (ushort x = 0; x < 3; x++) this.Dots[x] = new TPoint();
                APoint = Dots[0];
                BPoint = Dots[1];
                CPoint = Dots[2];
            }
            public TTriangle(System.Drawing.Point[] ps)
            {
                this.Dots = new TPoint[ps.Length];
                for (ushort x = 0; x < 3; x++) this.Dots[x] = (TPoint)ps[x];
                APoint = Dots[0];
                BPoint = Dots[1];
                CPoint = Dots[2];
            }
            public TTriangle(System.Drawing.PointF[] ps)
            {
                this.Dots = new TPoint[ps.Length];
                for (ushort x = 0; x < 3; x++) this.Dots[x] = (TPoint)ps[x];
                APoint = Dots[0];
                BPoint = Dots[1];
                CPoint = Dots[2];
            }

            public static explicit operator TTriangle(System.Drawing.Point[] ps) { return new TTriangle(ps); }
            public static explicit operator TTriangle(System.Drawing.PointF[] ps) { return new TTriangle(ps); }
            public static explicit operator TTriangle(TPolygon p) { return new TTriangle(p); }
            public static explicit operator System.Drawing.Point[](TTriangle p) 
            {
                System.Drawing.Point[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.Point[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.Point)p.Dots[i];
                return res;
            }
            public static explicit operator System.Drawing.PointF[](TTriangle p)
            {
                System.Drawing.PointF[] res = null;
                if ((p == null) || (p.Dots == null)) return res;
                res = new System.Drawing.PointF[p.Dots.Length];
                if (p.Dots.Length == 0) return res;
                for (int i = 0; i < res.Length; i++) res[i] = (System.Drawing.PointF)p.Dots[i];
                return res;
            }
            public static explicit operator TPolygon(TTriangle p) { return new TPolygon(p.Dots); }

            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < Dots.Length; i++) s += (i > 0 ? "," : "") + "{" + Dots[i].X.ToString() + ";" + Dots[i].Y.ToString() + "}";
                return s;
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
        public static bool IntersectPolygonsA(TPolygon pol1, TPolygon pol2, double EPS)
        {
            for (int i = 0; i < pol1.Dots.Length; i++) if (PointInPolygon(pol1.Dots[i], pol2, EPS)) return true;
            for (int i = 0; i < pol2.Dots.Length; i++) if (PointInPolygon(pol2.Dots[i], pol1, EPS)) return true;
            return false;
        }

        // 
        // CrossPolygons IntersectPolygons
        /// <summary>
        ///     Накладываются ли или пересекаются полигоны
        ///     быстрее чем B
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IntersectPolygonsA(TPolygon pol1, TPolygon pol2)
        {
            for (int i = 0; i < pol1.Dots.Length; i++) if (PointInPolygon(pol1.Dots[i], pol2)) return true;
            for (int i = 0; i < pol2.Dots.Length; i++) if (PointInPolygon(pol2.Dots[i], pol1)) return true;
            return false;
        }    

        // 
        // CrossPolygons IntersectPolygons
        /// <summary>
        ///     Накладываются ли или пересекаются полигоны
        ///     быстрее чем B
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IntersectPolygons(TPolygon pol1, TPolygon pol2)
        {
            return IntersectPolygonsA(pol1, pol2);
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
            return PointInLine(point, line, MaxError);
        }

        // попадание точки в линию
        public static bool PointInLine(TPoint point, TPolygon line, double EPS)
        {
            for(int i=1;i<line.Dots.Length;i++)
            {
                double dx = (line.Dots[i].Y - line.Dots[i - 1].Y) / (line.Dots[i].X - line.Dots[i - 1].X);
                if (Math.Abs((point.X - line.Dots[i - 1].X) * dx - (point.Y - line.Dots[i - 1].Y)) <= EPS)
                    return true;
            };			
			return false;
        }

        // попадание точки в отрезок
        public static bool PointInLineSegment(TPoint point, TPolygon line)
        {
            return PointInLineSegment(point, line);
        }

        // попадание точки в отрезок
        public static bool PointInLineSegment(TPoint point, TPolygon line, double EPS)
        {
            for (int i = 1; i < line.Dots.Length; i++)
            {
                double dx = (line.Dots[i].Y - line.Dots[i - 1].Y) / (line.Dots[i].X - line.Dots[i - 1].X);
                if ((Math.Abs((point.X - line.Dots[i - 1].X) * dx - (point.Y - line.Dots[i - 1].Y)) <= EPS) && // удовлетворяет ли уравнению линии
                    (point.X >= Math.Min(line.Dots[i - 1].X, line.Dots[i].X)) && // попадает ли в bounds отрезка
                    (point.X <= Math.Max(line.Dots[i - 1].X, line.Dots[i].X)) &&
                    (point.Y >= Math.Min(line.Dots[i - 1].Y, line.Dots[i].Y)) &&
                    (point.Y <= Math.Max(line.Dots[i - 1].Y, line.Dots[i].Y))) return true;
            };
            return false;
        }


        /// <summary>
        ///     Проверка на пересечение прямоугольников
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool IntersectRectangles(System.Drawing.RectangleF r1, System.Drawing.RectangleF r2)
        {
            return (r1.Left < r2.Right) && (r2.Left < r1.Right) && (r1.Top < r2.Bottom) && (r2.Top < r1.Bottom);
        }

        public static bool IntersectLines(TPoint line1start, TPoint line1end, TPoint line2start, TPoint line2end)
        {
            TPoint cr = GetTwoLinesCrossPoint(line1start, line1end, line2start, line2end);
            if (cr == null) return false;
            if ((cr.X < Math.Min(line1start.X, line1end.X)) || (cr.Y < Math.Min(line1start.Y, line1end.Y)) || (cr.X > Math.Max(line1start.X, line1end.X)) || (cr.Y > Math.Max(line1start.Y, line1end.Y)) || (cr.X < Math.Min(line2start.X, line2end.X)) || (cr.Y < Math.Min(line2start.Y, line2end.Y)) || (cr.X > Math.Max(line2start.X, line2end.X)) || (cr.Y > Math.Max(line2start.Y, line2end.Y)))
                return false;
            else
                return true;
        }

        public static TPoint GetTwoLinesCrossPoint(TPoint line1start, TPoint line1end, TPoint line2start, TPoint line2end)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            double A1 = line1end.Y - line1start.Y;
            double B1 = line1start.X - line1end.X;
            double C1 = A1 * line1start.X + B1 * line1start.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            double A2 = line2end.Y - line2start.Y;
            double B2 = line2start.X - line2end.X;
            double C2 = A2 * line2start.X + B2 * line2start.Y;

            // Get delta and check if the lines are parallel
            double delta = A1 * B2 - A2 * B1;
            if (delta == 0) return null;

            // now return the Vector2 intersection point
            TPoint res = new TPoint(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );
            return res;
        }

        // CrossPolygonLines
        /// <summary>
        ///     Пересекаются ли линии полигонов
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IntersectPolygonsLines(List<TPoint> list1, List<TPoint> list2)
        {
            return IntersectPolygonsLines(list1.ToArray(), list2.ToArray());
        }

        // CrossPolygonLines 
        /// <summary>
        ///     Пересекаются ли линии полигонов
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IntersectPolygonsLines(TPoint[] list1, TPoint[] list2)
        {
            for (int m = 0; m < list1.Length; m++)
                for (int n = 0; n < list2.Length; n++)
                {
                    if ((m != list1.Length - 1) && (n != list2.Length - 1))
                        if (IntersectLines(list1[m], list1[m + 1], list2[n], list2[n + 1])) 
                            return true;
                    if ((m == list1.Length - 1) && (n != list2.Length - 1))
                        if (IntersectLines(list1[m], list1[0], list2[n], list2[n + 1])) 
                            return true;
                    if ((m != list1.Length - 1) && (n == list2.Length - 1))
                        if (IntersectLines(list1[m], list1[m + 1], list2[n], list2[0])) 
                            return true;
                    if ((m == list1.Length - 1) && (n == list2.Length - 1))
                        if (IntersectLines(list1[m], list1[0], list2[n], list2[0])) 
                            return true;
                };
            return false;
        }

        // CrossLines 
        /// <summary>
        ///     Пересекаются ли линии
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IntersectPolyLines(TPoint[] polyline1, TPoint[] polyline2)
        {
            for (int m = 1; m < polyline1.Length; m++)
                for (int n = 1; n < polyline2.Length; n++)
                    if (IntersectLines(polyline1[m - 1], polyline1[m], polyline2[n - 1], polyline2[n])) return true;
            return false;
        }

        // 
        // CrossPolygons IntersectPolygons
        /// <summary>
        ///     Накладываются ли или пересекаются полигоны
        ///     медленее чем A
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IntersectPolygonsB(TPolygon p1, TPolygon p2)
        {
            if (IntersectPolygonsLines(p1.Dots, p2.Dots)) return true; // если линии пересекаются, то и полигоны тоже
            if (PointInPolygon(p1.Dots[0], p2, MaxError)) return true; // если полигон 1 лежив в 2
            if (PointInPolygon(p2.Dots[0], p1, MaxError)) return true; // если полигон 2 лежив в 1
            return false; // если bounds пересекаются, но сами полигоны нет
        }

        public static bool PolygonAinPolygonB(TPolygon a, TPolygon b)
        {
            if (IntersectPolygonsLines(a.Dots, b.Dots)) return false;
            if (PointInPolygon(a.Dots[0], b, MaxError)) return true; // если полигон a лежив в b
            return false;
        }        

        public static double DegToRad(double deg)
        {
            return (deg / 180.0 * Math.PI);
        }

        public static double RadToDeg(double rad)
        {
            return ((rad) * 180) / Math.PI;
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
