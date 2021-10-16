//#define gpc
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

#if gpc
using GpcWrapper;
#endif


namespace PolyLineBuffer
{
    public class PolyLineBufferCreator
    {
        public class PolyResult
        {
            public PointF[] polygon;
            public List<PointF[]> segments;

            public PolyResult()
            {
                this.polygon = new PointF[0];
                this.segments = new List<PointF[]>();
            }

            public PolyResult(PointF[] polygon, List<PointF[]> segments)
            {
                this.polygon = polygon;
                this.segments = segments;
            }

            public bool PointIn(PointF point)
            {
                if (segments.Count == 0) return false;
                for (int i = 0; i < segments.Count; i++)
                    if (PointInPolygon(point, segments[i]))
                        return true;
                return false;
            }

            private static bool PointInPolygon(PointF point, PointF[] polygon)
            {
                if (polygon == null) return false;
                if (polygon.Length < 2) return false;

                int i, j, nvert = polygon.Length;
                bool c = false;

                for (i = 0, j = nvert - 1; i < nvert; j = i++)
                {
                    if (((polygon[i].Y >= point.Y) != (polygon[j].Y >= point.Y)) &&
                        (point.X <= (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                      )
                        c = !c;
                }

                return c;
            }
        }

        /// <summary>
        ///     Calc distance in custom units
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <returns>distance in custom units</returns>
        public delegate float DistanceFunction(PointF a, PointF b);

        /// <summary>
        ///     return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float SampleDistFunc(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">Polyline</param>
        /// <param name="distance">Buffer radius size in custom units</param>
        /// <param name="right">include right side</param>
        /// <param name="left">include left side</param>
        /// <param name="poliSegments"></param>
        /// <param name="DistanceFunc"></param>
        /// <returns></returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, bool right, bool left, DistanceFunction DistanceFunc)
        {
            float step = 1;

            // Empty
            if (line == null) return new PolyResult();
            if (line.Length == 0) return new PolyResult();

            // Point
            if (line.Length == 1)
            {
                PointF s = line[0];
                float d = DistanceFunc == null ? SampleDistFunc(s, new PointF(s.X + step, s.Y)) : DistanceFunc(s, new PointF(s.X + step, s.Y));
                float r = distance / d * step;
                PointF[] res = GetEllipse(s, r, 0, 360);
                List<PointF[]> segments = new List<PointF[]>();
                segments.Add(res);
                return new PolyResult(res, segments);
            };

            // Line
            if (line.Length > 1)
            {
                List<PointF[]> poliSegments = new List<PointF[]>();

                List<PointF> p = new List<PointF>();
                List<PointF> v = new List<PointF>();
                v.AddRange(line);
#if gpc                
                p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
#else
                if (right)
                {
                    p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                    if (!left)
                    {
                        v.Reverse();
                        p.AddRange(v);
                    };
                };
                if (left)
                {
                    if (!right) p.AddRange(v);
                    v.Reverse();
                    if (right) p.RemoveAt(p.Count - 1);
                    p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                    if (right) p.RemoveAt(p.Count - 1);
                };
#endif
                return new PolyResult(p.ToArray(), poliSegments);
            };
            return new PolyResult();
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">Polyline</param>
        /// <param name="distance">Buffer radius size in custom units</param>
        /// <param name="DistanceFunc"></param>
        /// <returns></returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, DistanceFunction DistanceFunc)
        {
            return GetLineBufferPolygon(line, distance, true, true, DistanceFunc);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">Polyline</param>
        /// <param name="distance">Buffer radius size in custom units</param>
        /// <returns></returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance)
        {
            return GetLineBufferPolygon(line, distance, true, true, SampleDistFunc);
        }

        private static PointF[] glbp(PointF[] line, float distance, List<PointF[]> poliSegments, DistanceFunction DistanceFunc)
        {
            float step = 1;
            List<PointF> p = new List<PointF>();

#if gpc
            for (int n = 1; n < line.Length; n++)
            {
                List<PointF> polse = new List<PointF>();
                PointF s = line[n - 1];
                PointF e = line[n];

                float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                if (e.X < s.X) angle = 180 + angle;
                if (angle < 0) angle += 360;

                float c = (float)(e.Y - Math.Tan(angle * Math.PI / 180) * e.X);
                float d = DistFunc(s, new PointF(s.X + step, s.Y));
                float r = distance / d * step;
                float cCR = (float)(c - r / Math.Cos(angle * Math.PI / 180));

                PointF[] els = GetEllipse(s, r, angle - 270, angle - 90);
                PointF[] ele = GetEllipse(e, r, angle - 90, angle + 90);
                List<PointF> segments = new List<PointF>();
                segments.AddRange(els);
                segments.AddRange(ele);
                poliSegments.Add(segments.ToArray());
                p = segments;
            };
            if (poliSegments.Count != 0)
            {
                Polygon was = null;
                for (int i = 0; i < poliSegments.Count; i++)
                {
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddPolygon(poliSegments[i]);
                    Polygon cur = new Polygon(gp);
                    if (i > 0) cur = cur.Clip(GpcOperation.Union, was);
                    was = cur;
                };
                if ((was != null) && (was.Contour != null) && (was.Contour.Length > 0))
                {
                    p.Clear();
                    for (int i = 0; i < was.Contour[0].Vertex.Length; i++)
                        p.Add(new PointF((float)was.Contour[0].Vertex[i].X, (float)was.Contour[0].Vertex[i].Y));
                };
            };
                
#else

            float pAn = 0;
            float pCR = 0;            

            for (int n = 1; n < line.Length; n++)
            {
                // y = f(x)
                // y = tan(angle) * x + c;

                PointF befpP = n < 2 ? new PointF(0, 0) : line[n - 2];
                PointF prevP = line[n - 1];
                PointF currP = line[n];
                float angle = (float)((Math.Atan((currP.Y - prevP.Y) / (currP.X - prevP.X)) * 180 / Math.PI));
                if (currP.X < prevP.X) angle = 180 + angle;
                if (angle < 0) angle += 360;                

                if(Math.Abs(Math.Tan(angle * Math.PI / 180)) > 750) angle -= (float)0.05;


                float c = (float)(currP.Y - Math.Tan(angle * Math.PI / 180) * currP.X);
                float d = DistanceFunc == null ? SampleDistFunc(prevP, new PointF(prevP.X + step, prevP.Y)) : DistanceFunc(prevP, new PointF(prevP.X + step, prevP.Y));
                float r = distance / d * step;
                float cCR = (float)(c - r / Math.Cos(angle * Math.PI / 180));

                //if (poliSegments != null)
                //{
                //    PointF[] els = GetEllipse(s, r, angle - 270, angle - 90);
                //    PointF[] ele = GetEllipse(e, r, angle - 90, angle + 90);
                //    List<PointF> segments = new List<PointF>();
                //    segments.AddRange(els);
                //    segments.AddRange(ele);
                //    poliSegments.Add(segments.ToArray());
                //};

                // first point
                if (n == 1)
                {
                    PointF[] el = GetEllipse(prevP, r, angle - 180, angle - 90);
                    p.AddRange(el);
                    List<PointF> tmps = new List<PointF>();
                    tmps.AddRange(el); tmps.Add(prevP);
                    if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                };

                // lines
                {
                    // yr = f(x)
                    // yr = tan(angle) * x + c - r;

                    if (n == 1)
                    {
                        pAn = angle;
                        pCR = c - r;
                    };

                    if ((angle == pAn) && (n != 1)) // no turn
                    {
                        float xr = (float)(prevP.X + Math.Cos((angle - 90) * Math.PI / 180) * r);
                        float yr = (float)(prevP.Y + Math.Sin((angle - 90) * Math.PI / 180) * r);
                        p.Add(new PointF(xr, yr));
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                        tmps.Add(new PointF(xr, yr));
                        tmps.Add(currP);
                        if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                    };                    

                    // turn to left/right
                    if (n > 1)
                    {
                        float dA = (pAn - angle);
                        if (dA < -180) dA += 360;
                        if (dA > 180) dA = dA - 360;

                        if ((dA > 0) && (dA < 180)) // turn left
                        {
                            // tan(angle)*x+(c-r) = tan(pAn)*x+pCR
                            // x = (pCR-(c-r))/tan(angle)-tan(pAn)                            

                            float xu = (float)((pCR - cCR) / (Math.Tan(angle * Math.PI / 180) - Math.Tan(pAn * Math.PI / 180)));
                            float yu = (float)(Math.Tan(pAn * Math.PI / 180) * xu + pCR);

                            bool add = true;
                            if (befpP.X != 0)
                            {
                                float dsum = DistanceFunc == null ? SampleDistFunc(currP, befpP) : DistanceFunc(currP, befpP);
                                if (dsum < (distance * 2))
                                    add = false;
                                else
                                {
                                    float dprv = DistanceFunc == null ? SampleDistFunc(prevP, currP) : DistanceFunc(prevP, currP);
                                    float dcur = DistanceFunc == null ? SampleDistFunc(prevP, befpP) : DistanceFunc(prevP, befpP);
                                    float dadd = DistanceFunc == null ? SampleDistFunc(prevP, new PointF(xu, yu)) : DistanceFunc(prevP, new PointF(xu, yu));
                                    if ((dadd > dcur) && (dadd > dprv)) 
                                        add = false;
                                };                               
                            };


                            if (add)
                            {
                                float da = pAn - dA / 2 - 90;
                                p.Add(new PointF(xu, yu));

                                if (poliSegments != null)
                                {
                                    List<PointF> tmps = new List<PointF>();
                                    tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                                    tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                                    tmps.Add(new PointF(xu, yu));
                                    tmps.Add(prevP);
                                    poliSegments.Add(tmps.ToArray());
                                };
                            };
                        }
                        else // turn right
                        {
                            PointF[] el = GetEllipse(prevP, r, pAn - 90, angle - 90);
                            p.AddRange(el);
                            
                            if (poliSegments != null)
                            {
                                List<PointF> tmps = new List<PointF>();
                                tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                                tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                                tmps.AddRange(el);
                                tmps.Add(prevP);
                                poliSegments.Add(tmps.ToArray());
                            };
                        };
                    };
                    pAn = angle;
                    pCR = cCR;
                };

                // last point
                if (n == (line.Length - 1))
                {
                    PointF[] el = GetEllipse(currP, r, angle - 90, angle);
                    p.AddRange(el);
                    if (poliSegments != null)
                    {
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                        tmps.Add(el[0]);
                        tmps.Add(currP);
                        poliSegments.Add(tmps.ToArray());
                    };
                    {
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(currP); tmps.AddRange(el);
                        if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                    };
                };
            };
#endif
            if ((p[0].X == p[p.Count - 1].X) && (p[0].Y == p[p.Count - 1].Y)) p.RemoveAt(p.Count - 1);
            for (int i = p.Count - 2, j = p.Count - 1; i >= 0; i--, j--)
                if ((p[i].X == p[j].X) && (p[i].Y == p[j].Y)) p.RemoveAt(j);

            // optimize
            //for (int pn = p.Count - 1; pn >= 0; pn--)
            //{
            //    float maxd = float.MaxValue;
            //    for (int n = 1; n < line.Length; n++)
            //    {
            //        PointF prevP = line[n - 1];
            //        PointF currP = line[n];                    

            //        PointF pol;
            //        int side;
            //        float curd = DistanceFromPointToLine(p[pn], prevP, currP, DistanceFunc, out pol, out side);
            //        if (curd < maxd) maxd = curd;                    
            //    };
            //    if (maxd < (distance * 0.75)) p.RemoveAt(pn);
            //};

            return p.ToArray();
        }

        private static PointF[] GetEllipse(PointF center, float radius, float angle_from, float angle_to)
        {
            List<PointF> p = new List<PointF>();
            if (angle_to < angle_from) angle_to += 360;
            for (float an = angle_from; an <= angle_to; an += 90 / 5)
            {
                float x = (float)(center.X + Math.Cos(an * Math.PI / 180) * radius);
                float y = (float)(center.Y + Math.Sin(an * Math.PI / 180) * radius);
                p.Add(new PointF(x, y));
            };
            return p.ToArray();
        }

        private static float DistanceFromPointToLine(PointF pt, PointF lineStart, PointF lineEnd, DistanceFunction DistanceFunc, out PointF pointOnLine, out int side)
        {
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;

            if ((dx == 0) && (dy == 0))
            {
                // line is a point
                // ����� ����� ���� � ������� ������ ����� ������� TRA
                pointOnLine = lineStart;
                side = 0;
                //dx = pt.X - lineStart.X;
                //dy = pt.Y - lineStart.Y;                
                //return Math.Sqrt(dx * dx + dy * dy);
                float dist = DistanceFunc == null ? SampleDistFunc(pt, pointOnLine) : DistanceFunc(pt, pointOnLine);
                return dist;
            };

            side = Math.Sign((lineEnd.X - lineStart.X) * (pt.Y - lineStart.Y) - (lineEnd.Y - lineStart.Y) * (pt.X - lineStart.X));

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - lineStart.X) * dx + (pt.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                pointOnLine = new PointF(lineStart.X, lineStart.Y);
                dx = pt.X - lineStart.X;
                dy = pt.Y - lineStart.Y;
            }
            else if (t > 1)
            {
                pointOnLine = new PointF(lineEnd.X, lineEnd.Y);
                dx = pt.X - lineEnd.X;
                dy = pt.Y - lineEnd.Y;
            }
            else
            {
                pointOnLine = new PointF(lineStart.X + t * dx, lineStart.Y + t * dy);
                dx = pt.X - pointOnLine.X;
                dy = pt.Y - pointOnLine.Y;
            };

            float d = DistanceFunc == null ? SampleDistFunc(pt, pointOnLine) : DistanceFunc(pt, pointOnLine);
            return d;
        }       
    }
}
