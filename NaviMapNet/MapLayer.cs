using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace NaviMapNet
{
    /// <summary>
    ///     Класс слоя карты
    /// </summary>
    public class MapLayer
    {
        /// <summary>
        ///     Слои
        /// </summary>
        internal MapLayers _Layers = null;

        /// <summary>
        ///     Имя слоя
        /// </summary>
        private string _name;

        /// <summary>
        ///     Имя слоя
        /// </summary>
        public string Name { set { _name = value; } get { return _name; } }

        /// <summary>
        ///     Создание слоя
        /// </summary>
        /// <param name="Name">Имя слоя</param>
        public MapLayer(string Name)
        {
            _name = Name;
        }

        /// <summary>
        ///     Объекты слоя
        /// </summary>
        private List<MapObject> _objects = new List<MapObject>();

        /// <summary>
        ///     Объект слоя по индексу
        /// </summary>
        /// <param name="index">Индекс объекта</param>
        /// <returns>Объект</returns>
        public MapObject this[int index]
        {
            set
            {
                _objects[index] = value;
            }
            get
            {
                return _objects[index];
            }
        }

        /// <summary>
        ///     Объект слоя по имени
        /// </summary>
        /// <param name="Name">Имя объекта</param>
        /// <returns>Объект</returns>
        public MapObject this[string Name]
        {
            set
            {
                for (int i = 0; i < _objects.Count; i++)
                    if (_objects[i].Name == Name)
                    {
                        _objects[i] = value;
                        return;
                    };
                _objects.Add(value);
                value._layer = this;
                if (value._id == String.Empty) value._id = "MOID_" + System.DateTime.UtcNow.Ticks.ToString() + "_" + this._Layers._ttl_obj_counter.ToString();
                if (value.Name == String.Empty) value.Name = value._id;
                this._Layers._ttl_obj_counter++;
            }
            get
            {
                foreach (MapObject obj in _objects)
                    if (obj.Name == Name) return obj;
                return null;
            }
        }

        /// <summary>
        ///     Индекс объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>Индекс</returns>
        public int this[MapObject obj]
        {
            get
            {
                return _objects.IndexOf(obj);
            }
        }

        /// <summary>
        ///     Видимость слоя
        /// </summary>
        private bool _visible = true;

        /// <summary>
        ///     Видимость слоя
        /// </summary>
        public bool Visible { set { _visible = value; } get { return _visible; } }

        /// <summary>
        ///     Видимые объекты слоя
        /// </summary>
        public MapObject[] VisibleObjects
        {
            get
            {
                ArrayList objs = new ArrayList();
                foreach (MapObject obj in _objects)
                    if (obj.Visible) objs.Add(objs);
                return (MapObject[])objs.ToArray();
            }
        }

        /// <summary>
        ///     Число видимых объектов карты
        /// </summary>
        public int VisibleObjectsCount
        {
            get
            {
                int objs = 0;
                foreach (MapObject obj in _objects)
                    if (obj.Visible) objs++;
                return objs;
            }
        }

        /// <summary>
        ///     Невидимые объекты слоя
        /// </summary>
        public MapObject[] HiddenObjects
        {
            get
            {
                ArrayList objs = new ArrayList();
                foreach (MapObject obj in _objects)
                    if (!obj.Visible) objs.Add(objs);
                return (MapObject[])objs.ToArray();
            }
        }

        /// <summary>
        ///     Число невидимых объектов карты
        /// </summary>
        public int HiddenObjectsCount
        {
            get
            {
                int objs = 0;
                foreach (MapObject obj in _objects)
                    if (!obj.Visible) objs++;
                return objs;
            }
        }

        /// <summary>
        ///     Число объектов слоя
        /// </summary>
        public int ObjectsCount
        {
            get { return _objects.Count; }
        }

        /// <summary>
        ///     Границы всех объектов слоя
        /// </summary>
        public RectangleF Bounds
        {
            get
            {
                float xl = 0, yt = 0, xr = 0, yb = 0;
                if (_objects.Count > 0)
                {
                    xl = _objects[0].Bounds.Left;
                    yt = _objects[0].Bounds.Bottom; // Remember Reverse Load
                    xr = _objects[0].Bounds.Right;
                    yb = _objects[0].Bounds.Top; // Remember Reverse Load
                };
                foreach (MapObject obj in _objects)
                {
                    if (obj.Bounds.Left < xl) xl = obj.Bounds.Left;
                    if (obj.Bounds.Bottom > yt) yt = obj.Bounds.Bottom; // Remember Reverse Load
                    if (obj.Bounds.Right > xr) xr = obj.Bounds.Right;
                    if (obj.Bounds.Top < yb) yb = obj.Bounds.Top; // Remember Reverse Load
                };
                return new RectangleF(xl, yb, xr - xl, yt - yb);  // Remember Reverse Save
            }
        }

        /// <summary>
        ///     Границы видимых объектов слоя
        /// </summary>
        public RectangleF BoundsVisible
        {
            get
            {
                float xl = 0, yt = 0, xr = 0, yb = 0;
                int s_o = -1;
                for (int i = 0; i < _objects.Count; i++) if (_objects[i].Visible) { s_o = i; break; };
                if (_objects.Count > 0 && s_o > -1)
                {
                    xl = _objects[s_o].Bounds.Left;
                    yt = _objects[s_o].Bounds.Bottom; // Remember Reverse Load
                    xr = _objects[s_o].Bounds.Right;
                    yb = _objects[s_o].Bounds.Top; // Remember Reverse Load
                };
                foreach (MapObject obj in _objects)
                if (obj.Visible)
                {
                    if (obj.Bounds.Left < xl) xl = obj.Bounds.Left;
                    if (obj.Bounds.Bottom > yt) yt = obj.Bounds.Bottom; // Remember Reverse Load
                    if (obj.Bounds.Right > xr) xr = obj.Bounds.Right;
                    if (obj.Bounds.Top < yb) yb = obj.Bounds.Top; // Remember Reverse Load
                };
                return new RectangleF(xl, yb, xr - xl, yt - yb);  // Remember Reverse Save
            }
        }

        /// <summary>
        ///     Индекс слоя
        /// </summary>
        public int Index
        {
            get
            {
                if (_Layers == null) return -1;
                return _Layers[this];
            }
        }

        /// <summary>
        ///     Добавление объекта
        /// </summary>
        /// <param name="obj"></param>
        public void Add(MapObject obj)
        {
            _objects.Add(obj);
            obj._layer = this;
            if (obj._id == String.Empty) obj._id = "MOID_" + System.DateTime.UtcNow.Ticks.ToString() + "_" + this._Layers._ttl_obj_counter.ToString();
            if (obj.Name == String.Empty) obj.Name = obj._id;
            this._Layers._ttl_obj_counter++;
        }

        /// <summary>
        ///     Вставка объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="index">Индекс</param>
        public void Add(MapObject obj, int index)
        {
            _objects.Insert(index, obj);
            obj._layer = this;
            if (obj._id == String.Empty) obj._id = "MOID_" + System.DateTime.UtcNow.Ticks.ToString() + "_" + this._Layers._ttl_obj_counter.ToString();
            if (obj.Name == String.Empty) obj.Name = obj._id;
            this._Layers._ttl_obj_counter++;
        }

        /// <summary>
        ///     Удаление объекта
        /// </summary>
        /// <param name="obj">Слой</param>
        public void Remove(MapObject obj)
        {
            _objects.Remove(obj);
            obj._layer = null;
        }

        /// <summary>
        ///     Удаление объекта
        /// </summary>
        /// <param name="index">Индекс объекта</param>
        public void Remove(int index)
        {
            _objects[index]._layer = null;
            _objects.RemoveAt(index);
        }

        /// <summary>
        ///     Существование объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>Существует</returns>
        public bool Contains(MapObject obj)
        {
            return _objects.Contains(obj);
        }

        /// <summary>
        ///     Существование объекта
        /// </summary>
        /// <param name="Name">Имя объекта</param>
        /// <returns>Существует</returns>
        public bool Contains(string Name)
        {
            foreach (MapObject obj in _objects)
                if (obj.Name == Name) return true;
            return false;
        }

        /// <summary>
        ///     Удаление объектов слоя
        /// </summary>
        public void Clear()
        {
            _objects.Clear();
        }

        /// <summary>
        ///     Изменение очередности объектов
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="newIndex">Новый индекс</param>
        public void Move(MapObject obj, int newIndex)
        {
            _objects.Remove(obj);
            _objects.Insert(newIndex, obj);
        }

        /// <summary>
        ///     Изменение очередности объектов
        /// </summary>
        /// <param name="index">Текущий индекс</param>
        /// <param name="newIndex">Новый индекс</param>
        public void Move(int index, int newIndex)
        {
            MapObject obj = this[index];
            _objects.Remove(obj);
            _objects.Insert(newIndex, obj);
        }

        /// <summary>
        ///     Перестановка объектов
        /// </summary>
        /// <param name="objA">объект A</param>
        /// <param name="objB">объект B</param>
        public void Change(MapObject objA, MapObject objB)
        {
            int _a = this[objA];
            int _b = this[objB];
            _objects.Remove(objA);
            _objects.Remove(objB);
            if (_a < _b)
            {
                _objects.Insert(_a, objA);
                _objects.Insert(_b, objB);
            }
            else
            {
                _objects.Insert(_b, objB);
                _objects.Insert(_a, objA);
            };
        }

        /// <summary>
        ///     Перестановка объектов
        /// </summary>
        /// <param name="indexA">Индекс объекта A для перестановки</param>
        /// <param name="indexB">Индекс объекта B для перестановки</param>
        public void Change(int indexA, int indexB)
        {
            MapObject layA = this[indexA];
            MapObject layB = this[indexB];
            _objects.Remove(layA);
            _objects.Remove(layB);
            if (indexA < indexB)
            {
                _objects.Insert(indexA, layA);
                _objects.Insert(indexB, layB);
            }
            else
            {
                _objects.Insert(indexB, layB);
                _objects.Insert(indexA, layA);
            };
        }

        /// <summary>
        ///     Объекты в указанной зоне
        /// </summary>
        /// <param name="bounds">Зона в метрах</param>
        /// <returns>Объекты</returns>
        public MapObject[] Select(RectangleF bounds)
        {
            List<MapObject> objs = new List<MapObject>();
            foreach (MapObject mo in _objects)
            {
                if (InRectangle(mo.Bounds, bounds)) objs.Add(mo);
            };
            return objs.ToArray();
        }

        /// <summary>
        ///     Объекты в указанной зоне
        /// </summary>
        /// <param name="bounds">Зона в метрах</param>
        /// <param name="objTypes">Какие объекты выбирать</param>
        /// <returns>Объекты</returns>
        public MapObject[] Select(PointF[] _bounds, MapObjectType objTypes)
        {
            RectangleF bounds = new RectangleF((float)(_bounds[0].X), (float)(_bounds[1].Y), (float)(_bounds[1].X - _bounds[0].X), (float)(_bounds[0].Y - _bounds[1].Y));

            List<MapObject> objs = new List<MapObject>();
            foreach (MapObject mo in _objects)
            {
                if ((objTypes & mo.ObjectType) != 0)
                    if (InRectangle(mo.Bounds, bounds))
                        objs.Add(mo);
            };
            return objs.ToArray();
        }

        /// <summary>
        ///     Объекты в указанной зоне
        /// </summary>
        /// <param name="objType">Какие объекты выбирать</param>
        /// <returns></returns>
        public MapObject[] Select(MapObjectType objTypes)
        {
            List<MapObject> objs = new List<MapObject>();
            foreach (MapObject mo in _objects)
            {
                if ((objTypes & mo.ObjectType) != 0)
                    objs.Add(mo);
            };
            return objs.ToArray();
        }

        /// <summary>
        ///     Объекты в указанной зоне
        /// </summary>
        /// <param name="bounds">Зона в метрах</param>
        /// <param name="objTypes">Какие объекты выбирать</param>
        /// <param name="visible">Видимые объекты</param>
        /// <param name="hidden">Невидимые объекты</param>
        /// <returns></returns>
        public MapObject[] Select(RectangleF bounds, MapObjectType objTypes, bool visible, bool hidden)
        {
            List<MapObject> objs = new List<MapObject>();
            foreach (MapObject mo in _objects)
            {
                if ((visible && mo.Visible) || (hidden && (!mo.Visible)))
                    if ((objTypes & mo.ObjectType) != 0)
                        if (InRectangle(mo.Bounds, bounds)) objs.Add(mo);
            };
            return objs.ToArray();
        }

        /// <summary>
        ///     Проверка на пересечение прямоугольников
        /// </summary>
        /// <param name="r1">Прямоугольник</param>
        /// <param name="r2">Прямоугольник</param>
        /// <returns></returns>
        private bool InRectangle(RectangleF r1, RectangleF r2)
        {
            return (r1.Left < r2.Right) && (r2.Left < r1.Right) && (r1.Top < r2.Bottom) && (r2.Top < r1.Bottom);
        }
    }

}
