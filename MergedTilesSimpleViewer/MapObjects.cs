using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace NaviMapNet
{
    /// <summary>
    ///     Тип объекта карты
    /// </summary>
    public enum MapObjectType
    {
        /// <summary>
        ///     Точечный и мультиточечный объекты
        /// </summary>
        mPoint = 0x01,
        /// <summary>
        ///     Линия и полилиния
        /// </summary>
        mLine = 0x02,
        /// <summary>
        ///     Эллипс или круг
        /// </summary>
        mEllipse = 0x04,
        /// <summary>
        ///     Квадрат, полиональный объект
        /// </summary>
        mPolygon = 0x08,
        /// <summary>
        ///     Полилиния
        /// </summary>
        mPolyline = 0x10
    }

    #region MapObjects

    /// <summary>
    ///     Класс объекта слоя карты
    /// </summary>
    public abstract class MapObject
    {
        /// <summary>
        ///     Слой в котором лежит объект
        /// </summary>
        internal MapLayer _layer = null;

        /// <summary>
        ///     Идентификатор
        /// </summary>
        internal string _id = String.Empty;

        /// <summary>
        ///     Идентификатор объекта (get)
        /// </summary>
        public string ID { get { return _id; } }

        /// <summary>
        ///     Тип
        /// </summary>
        protected MapObjectType _type = MapObjectType.mPoint;

        /// <summary>
        ///     Тип объекта (get)
        /// </summary>
        public MapObjectType ObjectType { get { return _type; } }

        /// <summary>
        ///     Данные пользователя
        /// </summary>
        private object _userdata;

        /// <summary>
        ///     Пользовательские данные
        /// </summary>
        public object UserData { get { return _userdata; } set { _userdata = value; } }

        /// <summary>
        ///     Image
        /// </summary>
        private Image _img = null;

        /// <summary>
        ///     Картинка объекта
        /// </summary>
        public Image Img { set { _img = value; } get { return _img; } }

        /// <summary>
        ///     Имя типа
        /// </summary>
        /// <returns>Имя типа</returns>
        public string GetTypeName()
        {
            return this.GetType().ToString();
        }

        private bool DrawEvenSizeIsTooSmall_ = false;
        /// <summary>
        ///     Отрисовывать объект всегда, даже если его размеры менее пикселя
        /// </summary>
        public bool DrawEvenSizeIsTooSmall { get { return DrawEvenSizeIsTooSmall_; } set { DrawEvenSizeIsTooSmall_ = value; } }


        /// <summary>
        ///     Границы объекта в градусах
        /// </summary>
        private RectangleF _bounds = new Rectangle(0, 0, 0, 0);

        /// <summary>
        ///     Границы объекта в градусах (get)
        /// </summary>
        public RectangleF Bounds { get { this.ReBounds();  return _bounds; } }

        public Rectangle GetBoundsForZoom(int zoomId)
        {
            RectangleF b = this.Bounds;            
            double[] pA = TilesProjection.fromLLtoPixel(new double[] { b.Left, b.Top }, zoomId);
            double[] pB = TilesProjection.fromLLtoPixel(new double[] { b.Right, b.Bottom }, zoomId);
            Rectangle r = new Rectangle((int)pA[0], (int)pB[1], (int)pB[0] - (int)pA[0], (int)pA[1] - (int)pB[1]);
            return r;
        }

        /// <summary>
        ///     Размер захватываемой области в Pixels
        /// </summary>
        private Size _sizePixels = new Size(0,0);

        /// <summary>
        ///     Размер захватываемой области в Pixels
        /// </summary>
        public Size SizePixels { set {_sizePixels = value;} get{return _sizePixels;}}

        /// <summary>
        ///     Центр объекта в градусах  (get)
        /// </summary>
        public PointF Center
        {
            get
            {
                return new PointF((this.Bounds.Left+this.Bounds.Right)/2,(this.Bounds.Top+this.Bounds.Bottom)/2);
            }
        }

        /// <summary>
        ///     X-координата центра в градусах (get)
        /// </summary>
        public double X { get { return (this.Bounds.Left + this.Bounds.Right) / 2; } }

        /// <summary>
        ///     Y-координата центра в градусах (get)
        /// </summary>
        public double Y { get { return (this.Bounds.Top + this.Bounds.Bottom) / 2; } }

        /// <summary>
        ///     Координаты точек объекта в градусах
        /// </summary>
        private PointF[] _points = new PointF[0];

        /// <summary>
        ///     Координаты точек объекта в градусах (get set)
        /// </summary>
        public PointF[] Points 
        {
            set { this._points = value; }
            get { return _points; } 
        }

        /// <summary>
        ///     Пересчет Bounds
        /// </summary>
        private void ReBounds()
        {
            if (_points.Length == 0)
            {
                _bounds = new RectangleF(0, 0, 0, 0);
                return;
            };
            float xl = _points[0].X;
            float xr = _points[0].X;
            float yt = _points[0].Y;
            float yb = _points[0].Y;
            if (_points.Length == 1)
            {
                _bounds = new RectangleF(xl, yb, 0, 0);  // Remember Reverse Save
                return;
            };
            foreach (PointF pnt in _points)
            {
                if (pnt.X < xl) xl = pnt.X;
                if (pnt.X > xr) xr = pnt.X;
                if (pnt.Y > yt) yt = pnt.Y;
                if (pnt.Y < yb) yb = pnt.Y;
            };
            _bounds = new RectangleF(xl, yb, xr - xl, yt - yb);  // Remember Reverse Save
        }

        /// <summary>
        ///     Наименование объекта
        /// </summary>
        private string _name = String.Empty;

        /// <summary>
        ///     Наименование объекта (get set)
        /// </summary>
        public string Name { get { return _name;} set {_name = value;} }

        private string _Text = String.Empty;
        public string Text { get { return _Text; } set { _Text = value; } }

        /// <summary>
        ///     Цвет обводки объекта
        /// </summary>
        private Color _borderColor = Color.Black;

        /// <summary>
        ///     Цвет обводки объекта (get set)
        /// </summary>
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; } }

        /// <summary>
        ///     Цвет заливки объекта
        /// </summary>
        private Color _bodyColor = Color.Black;

        /// <summary>
        ///     Цвет заливки объекта  (get set)
        /// </summary>
        public Color BodyColor { get { return _bodyColor; } set { _bodyColor = value; } }

        /// <summary>
        ///     Видимость
        /// </summary>
        private bool _visible = true;

        /// <summary>
        ///     Видимость объекта  (get set)
        /// </summary>
        public bool Visible
        {
            set { _visible = value; }
            get { return _visible; }
        }

        /// <summary>
        ///     Индекс объекта (get)
        /// </summary>
        public int Index
        {
            get
            {
                if (_layer == null) return -1;
                return _layer[this];
            }
        }

        /// <summary>
        ///     Число точек объекта
        /// </summary>
        public int PointsCount { get { return _points.Length; } }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public abstract bool ShowObject(double zoom);

        private bool _DrawText = false;
        public bool DrawText { get { return _DrawText; } set { _DrawText = value; } }
        public Brush TextBrush = new SolidBrush(Color.Black);
        public Font TextFont = new Font("Arial", 8, FontStyle.Regular);
        public Point TextOffset = new Point(0, 0);
    }

    /// <summary>
    ///     Класс точки карты
    /// </summary>
    public class MapPoint : MapObject
    {
        /// <summary>
        ///     Создание точечного объекта
        /// </summary>
        public MapPoint()
        {
            this._type = MapObjectType.mPoint;
            this.Points = (new PointF[] { new PointF(0, 0) });
            this.SizePixels = new Size(1, 1);
        }

        public bool Squared = false;

        /// <summary>
        ///     Создание точечного объекта
        /// </summary>
        /// <param name="XY">Координаты точки WGS84</param>
        public MapPoint(PointF XY)
        {
            this._type = MapObjectType.mPoint;            
            this.Points = (new PointF[] { XY });
            this.SizePixels = new Size(1, 1);
        }

        /// <summary>
        ///     Создание точечного объекта
        /// </summary>
        /// <param name="X">Координата X в метрах</param>
        /// <param name="Y">Координата Y в метрах</param>
        public MapPoint(double Lat, double Lon)
        {
            this._type = MapObjectType.mPoint;
            this.Points = (new PointF[1] { new PointF((float)Lon, (float)Lat) });
            this.SizePixels = new Size(1, 1);
        }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Координаты точки в degrees
        /// </summary>
        public PointF XY
        {
            set 
            {
                this.Points = (new PointF[1] { value });
            }
            get { return this.Points[0]; }
        }

        /// <summary>
        ///     Цвет точки
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        public static implicit operator MapPoint(PointF p)
        {
            return new MapPoint(p);
        }

        public static implicit operator PointF(MapPoint p)
        {
            return p.Points[0];
        }
    }

    /// <summary>
    ///     Класс мультиточки карты
    /// </summary>
    public class MapMultiPoint : MapObject
    {
        /// <summary>
        ///     Создание мультиточечного объекта
        /// </summary>
        public MapMultiPoint()
        {
            this._type = MapObjectType.mPoint;
        }

        public bool Squared = false;

        /// <summary>
        ///     Создание мультиточечного объекта
        /// </summary>
        /// <param name="Points">Точки объекта в WGS84</param>
        public MapMultiPoint(PointF[] Points)
        {
            this._type = MapObjectType.mPoint;
            this.Points = (Points);
        }
      

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Число точек
        /// </summary>
        public int Count { get { return this.Points.Length; } }

        /// <summary>
        ///     Координаты точки в WGS84
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <returns>Точка</returns>
        public PointF this[int index]
        {
            set
            {
                Points[index] = value;
            }
            get { return Points[index]; }
        }

        /// <summary>
        ///     Цвет точек
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        public static implicit operator MapMultiPoint(PointF[] ps)
        {
            return new MapMultiPoint(ps);
        }

        public static implicit operator PointF[](MapMultiPoint mp)
        {
            return mp.Points;
        }
    }

    /// <summary>
    ///     Класс эллипса карты
    /// </summary>
    public class MapEllipse : MapObject
    {
        private SizeF SizeMeters_ = new SizeF();
        public SizeF SizeMeters 
        { 
            get { return SizeMeters_; }
            set { SizeMeters_ = value; Rebound(); }
        }
        private int ZoomID_ = 0;
        public int ZoomID { get { return ZoomID_;} set {ZoomID_ = value;} }

        private int LineWidth_ = 1;
        public int LineWidth { get { return LineWidth_; } set { LineWidth_ = value; } }

        /// <summary>
        ///     Создание эллипса
        /// </summary>
        public MapEllipse()
        {
            this._type = MapObjectType.mEllipse;
            this.Points = (new PointF[1] { new PointF(0, 0) });
        }

        /// <summary>
        ///     Создание эллипса
        /// </summary>
        /// <param name="XY">Центр эллипса в WGS84</param>
        /// <param name="width">Ширина в метрах</param>
        /// <param name="height">Высота в метрах</param>
        public MapEllipse(PointF XY, double width, double height)
        {
            this._type = MapObjectType.mEllipse;
            this.Points = (new PointF[1] { XY });
            this.SizeMeters = new SizeF((float)width, (float)height);
        }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Координаты центра эллипса в WGS
        /// </summary>
        public PointF XY
        {
            set { this.Points = (new PointF[1] { value }); }
            get { return this.Points[0]; }
        }

        /// <summary>
        ///     Высота эллипса в Pixels
        /// </summary>
        public int Height
        {            
            get { 
                Size sz = NaviMapNetViewer.MetersToPixels(SizeMeters_,ZoomID_);
                return sz.Height; 
            }
        }

        /// <summary>
        ///     Ширина эллипса в Pixels
        /// </summary>
        public int Width
        {
            get
            {
                Size sz = NaviMapNetViewer.MetersToPixels(SizeMeters_, ZoomID_);
                return sz.Width;
            }
        }

        /// <summary>
        ///     Размер захватываемой области в Pixels
        /// </summary>
        public Size SizePixels 
        { 
            get
            {
                return NaviMapNetViewer.MetersToPixels(SizeMeters_, ZoomID_);
            }
        }

        private void Rebound()
        {
            List<PointF> pts = new List<PointF>();
            pts.Add(this.Center);
            PointF c = NaviMapNetViewer.DegreesToMeters(Center);
            PointF p1 = new PointF((float)(c.X - SizeMeters_.Width / 2.0), (float)(c.Y - SizeMeters_.Height / 2.0));
            PointF p2 = new PointF((float)(c.X + SizeMeters_.Width / 2.0), (float)(c.Y + SizeMeters_.Height / 2.0));
            pts.Add(NaviMapNetViewer.MetersToDegrees(p1));
            pts.Add(NaviMapNetViewer.MetersToDegrees(p2));
            this.Points = pts.ToArray();
        }


        /// <summary>
        ///     Цвет эллипса
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        public static implicit operator MapEllipse(PointF p)
        {
            return new MapEllipse(p,1,1);
        }

        public static implicit operator PointF(MapEllipse p)
        {
            return p.Points[0];
        }
    }

    /// <summary>
    ///     Класс линии карты
    /// </summary>
    public class MapLine : MapObject
    {
        /// <summary>
        ///     Создание линии
        /// </summary>
        public MapLine()
        {
            this._type = MapObjectType.mLine;
            this.Points = (new PointF[2] { new PointF(0, 0), new PointF(0, 0) });
            this.SizePixels = new Size(1, 0);
        }

        /// <summary>
        ///     Создание линии
        /// </summary>
        /// <param name="fromPoint">Координаты точки в WGS</param>
        /// <param name="toPoint">Координаты точки в WGS</param>
        public MapLine(PointF fromPoint, PointF toPoint)
        {
            this._type = MapObjectType.mLine;
            this.Points = (new PointF[2] { fromPoint, toPoint });
            this.SizePixels = new Size(1, 0);
        }

        /// <summary>
        ///     Создание линии
        /// </summary>
        /// <param name="fromPoint">Координаты точки в WGS84</param>
        /// <param name="toPoint">Координаты точки в WGS84</param>
        /// <param name="width">Толщина линии в пикселях</param>
        public MapLine(PointF fromPoint, PointF toPoint, int width)
        {
            this._type = MapObjectType.mLine;
            this.Points = (new PointF[2] { fromPoint, toPoint } );
            this.SizePixels = new Size(1, 0);
            this.SizePixels = new Size(width, 0);
        }        

        /// <summary>
        ///     Цвет линии
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Толщина линии в пикселях
        /// </summary>
        public int Width
        {
            get { return this.SizePixels.Width; }
            set { this.SizePixels = new Size(value, 0); }
        }

        /// <summary>
        ///     0 точка линии в WGS
        /// </summary>
        public PointF XY0 { set { this.Points[0] = value; } get { return this.Points[0]; } }

        /// <summary>
        ///     1 точка линии в WGS
        /// </summary>
        public PointF XY1 { set { this.Points[1] = value; } get { return this.Points[1]; } }

        public static implicit operator MapLine(PointF[] ps)
        {
            return new MapLine(ps[0],ps[1]);
        }

        public static implicit operator PointF[](MapLine ml)
        {
            return ml.Points;
        }
    }

    /// <summary>
    ///     Класс полилинии карты
    /// </summary>
    public class MapPolyLine : MapObject
    {
        /// <summary>
        ///     Создание полилинии
        /// </summary>
        public MapPolyLine()
        {
            this._type = MapObjectType.mPolyline;
            this.SizePixels = new Size(1, 0);
        }

        /// <summary>
        ///     Создание полилинии
        /// </summary>
        /// <param name="pointsCount">Число точек</param>
        public MapPolyLine(int pointsCount)
        {
            this._type = MapObjectType.mPolyline;
            PointF[] tmp = new PointF[pointsCount];
            for (int i = 0; i < pointsCount; i++) tmp[i] = new PointF(0, 0);
            this.Points = (tmp);
            this.SizePixels = new Size(1, 0);
        }

        /// <summary>
        ///     Создание полилинии
        /// </summary>
        /// <param name="pointsCount">Число точек</param>
        /// <param name="width">Толщина линии в пикселях</param>
        public MapPolyLine(int pointsCount, int width)
        {
            this._type = MapObjectType.mPolyline;
            PointF[] tmp = new PointF[pointsCount];
            for (int i = 0; i < pointsCount; i++) tmp[i] = new PointF(0, 0);
            this.Points = (tmp);
            this.SizePixels = new Size(width, 0);
        }

        /// <summary>
        ///     Создание полилинии
        /// </summary>
        /// <param name="points">Коорднаты точек в WGS</param>
        public MapPolyLine(PointF[] points)
        {
            this._type = MapObjectType.mPolyline;
            this.Points = (points);
            this.SizePixels = new Size(1, 0);
        }

        /// <summary>
        ///     Создание полилинии
        /// </summary>
        /// <param name="points">Коорднаты точек в WGS</param>
        /// <param name="width">Толщина линии в пикселях</param>
        public MapPolyLine(PointF[] points, int width)
        {
            this._type = MapObjectType.mPolyline;
            this.Points = (points);
            this.SizePixels = new Size(width, 0);
        }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Толщина линии в пикселях
        /// </summary>
        public int Width
        {
            get { return this.SizePixels.Width; }
            set { this.SizePixels = new Size(value, 0); }
        }

        /// <summary>
        ///     Цвет линии
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        /// <summary>
        ///     Точки полилинии (в WGS)
        /// </summary>
        /// <param name="index">Индекс точки</param>
        /// <returns></returns>
        public PointF this[int index]
        {
            set { this.Points[index] = value; }
            get { return this.Points[index]; }
        }

        /// <summary>
        ///     Добавление точки к полилинии
        /// </summary>
        /// <param name="point">Координаты точки в метрах</param>
        public void AddPoint(PointF point)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.Add(point);
            this.Points = (pnts.ToArray());
        }

        /// <summary>
        ///     Добавление точки к полилинии
        /// </summary>
        /// <param name="point">Координаты точки в WGS</param>
        /// <param name="index">Индекс точки</param>
        public void AddPoint(Point point, int index)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.Insert(index, point);
            this.Points = (pnts.ToArray());
        }

        /// <summary>
        ///     Удаление точки
        /// </summary>
        /// <param name="index">индекс точки</param>
        public void RemovePoint(int index)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.RemoveAt(index);
            this.Points = (pnts.ToArray());
        }

        public static implicit operator MapPolyLine(PointF[] ps)
        {
            return new MapPolyLine(ps);
        }

        public static implicit operator PointF[](MapPolyLine mp)
        {
            return mp.Points;
        }
    }

    /// <summary>
    ///     Класс полигонального объекта
    /// </summary>
    public class MapPolygon : MapObject
    {
        /// <summary>
        ///     Создание полигонального объекта
        /// </summary>
        public MapPolygon()
        {
            this._type = MapObjectType.mPolygon;
        }

        /// <summary>
        ///     Создание полигонального объекта
        /// </summary>
        /// <param name="pointsCount">Число точек</param>
        public MapPolygon(int pointsCount)
        {
            this._type = MapObjectType.mPolygon;
            PointF[] tmp = new PointF[pointsCount];
            for (int i = 0; i < pointsCount; i++) tmp[i] = new PointF(0, 0);
            this.Points = (tmp);
        }

        /// <summary>
        ///     Создание полигонального объекта
        /// </summary>
        /// <param name="points">Коорднаты точек в WGS</param>
        public MapPolygon(PointF[] points)
        {
            this._type = MapObjectType.mPolygon;
            this.Points = (points);
        }

        /// <summary>
        ///     Толщина границы полигона в пикселях
        /// </summary>
        public int Width
        {
            get { return this.SizePixels.Width; }
            set { this.SizePixels = new Size(value, 0); }
        }

        /// <summary>
        ///     Цвет полигона
        /// </summary>
        public Color Color { set { this.BodyColor = value; } get { return this.BodyColor; } }

        /// <summary>
        ///     Отображать ли объект при этом зуме
        /// </summary>
        /// <param name="zoom">Зум</param>
        /// <returns>Отображать</returns>
        public override bool ShowObject(double zoom)
        {
            return true;
        }

        /// <summary>
        ///     Точки полигона (в WGS)
        /// </summary>
        /// <param name="index">Индекс точки</param>
        /// <returns>Координаты точки в WGS</returns>
        public PointF this[int index]
        {
            set { this.Points[index] = value; }
            get { return this.Points[index]; }
        }

        /// <summary>
        ///     Добавление точки к полигону
        /// </summary>
        /// <param name="point">Координаты точки в WGS</param>
        public void AddPoint(PointF point)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.Add(point);
            this.Points = (pnts.ToArray());
        }

        /// <summary>
        ///     Добавление точки к полигону
        /// </summary>
        /// <param name="point">Координаты точки в WGS</param>
        /// <param name="index">Индекс точки</param>
        public void AddPoint(PointF point, int index)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.Insert(index, point);
            this.Points = (pnts.ToArray());
        }

        /// <summary>
        ///     Удаление точки
        /// </summary>
        /// <param name="index">индекс точки</param>
        public void RemovePoint(int index)
        {
            List<PointF> pnts = new List<PointF>();
            foreach (PointF pnt in this.Points) pnts.Add(pnt);
            pnts.RemoveAt(index);
            this.Points = (pnts.ToArray());
        }

        public static implicit operator MapPolygon(PointF[] ps)
        {
            return new MapPolygon(ps);
        }

        public static implicit operator PointF[](MapPolygon mp)
        {
            return mp.Points;
        }
    }

    #endregion

}
