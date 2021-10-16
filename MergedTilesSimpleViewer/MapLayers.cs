using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace NaviMapNet
{
        /// <summary>
        ///     ����� ����� �����
        /// </summary>
        public class MapLayers
        {
            public MapLayers(NaviMapNetViewer map)
            {
                this._Map = map;
            }

            /// <summary>
            ///     ����� ������� ��������
            /// </summary>
            internal int _ttl_obj_counter = 0;

            /// <summary>
            ///     ���� �����
            /// </summary>
            private List<MapLayer> _layers = new List<MapLayer>();

            /// <summary>
            ///     ���� �� �������
            /// </summary>
            /// <param name="index">������ ����</param>
            /// <returns>����</returns>
            public MapLayer this[int index]
            {
                set
                {
                    _layers[index] = value;
                }
                get
                {
                    return _layers[index];
                }
            }

            /// <summary>
            ///     ���� �� �����
            /// </summary>
            /// <param name="Name">��� ����</param>
            /// <returns>����</returns>
            public MapLayer this[string Name]
            {
                set
                {
                    for (int i = 0; i < _layers.Count; i++)
                        if (_layers[i].Name == Name)
                        {
                            _layers[i] = value;
                            return;
                        };
                    _layers.Add(value);
                    value._Layers = this;
                }
                get
                {
                    foreach (MapLayer lay in _layers)
                        if (lay.Name == Name) return lay;
                    return null;
                }
            }

            /// <summary>
            ///     ������ ����
            /// </summary>
            /// <param name="Layer">����</param>
            /// <returns>������</returns>
            public int this[MapLayer Layer]
            {
                get
                {
                    return _layers.IndexOf(Layer);
                }
            }

            /// <summary>
            ///     ����� �����
            /// </summary>
            public int Count { get { return _layers.Count; } }

            /// <summary>
            ///     ������� ����
            /// </summary>
            public MapLayer[] VisibleLayers
            {
                get
                {
                    List<MapLayer> lays = new List<MapLayer>();
                    foreach (MapLayer lay in _layers)
                        if (lay.Visible) lays.Add(lay);
                    return lays.ToArray();
                }
            }

            /// <summary>
            ///     ����� ������� �����
            /// </summary>
            public int VisibleLayersCount
            {
                get
                {
                    int lays = 0;
                    foreach (MapLayer lay in _layers)
                        if (lay.Visible) lays++;
                    return lays;
                }
            }

            /// <summary>
            ///     ��������� ����
            /// </summary>
            public MapLayer[] HiddenLayers
            {
                get
                {
                    ArrayList lays = new ArrayList();
                    foreach (MapLayer lay in _layers)
                        if (!lay.Visible) lays.Add(lay);
                    return (MapLayer[])lays.ToArray();
                }
            }

            /// <summary>
            ///     ����� ��������� �����
            /// </summary>
            public int HiddenLayersCount
            {
                get
                {
                    int lays = 0;
                    foreach (MapLayer lay in _layers)
                        if (!lay.Visible) lays++;
                    return lays;
                }
            }

            /// <summary>
            ///     ���������� ����
            /// </summary>
            /// <param name="Layer">����</param>
            public void Add(MapLayer Layer)
            {
                _layers.Add(Layer);
                Layer._Layers = this;
            }

            /// <summary>
            ///     ������� ����
            /// </summary>
            /// <param name="Layer">����</param>
            /// <param name="index">������</param>
            public void Add(MapLayer Layer, int index)
            {
                _layers.Insert(index, Layer);
                Layer._Layers = this;
            }

            public bool Exists(string LayerName)
            {
                if (Count == 0) return false;
                for (int i = 0; i < Count; i++)
                    if (LayerName.ToUpper() == _layers[i].Name.ToUpper())
                        return true;
                return false;
            }

            /// <summary>
            ///     �������� ����
            /// </summary>
            /// <param name="Layer">����</param>
            public void Remove(MapLayer Layer)
            {
                _layers.Remove(Layer);
                Layer._Layers = null;
            }

            /// <summary>
            ///     �������� ����
            /// </summary>
            /// <param name="index">������ ����</param>
            public void Remove(int index)
            {
                _layers[index]._Layers = null;
                _layers.RemoveAt(index);
            }

            /// <summary>
            ///     �������� ����
            /// </summary>
            /// <param name="name">name ����</param>
            public void Remove(string name)
            {
                for(int i=Count-1;i>=0;i++)
                    if (_layers[i].Name.ToUpper() == name.ToUpper())
                    {
                        Remove(i);
                        return;
                    };
            }

            /// <summary>
            ///     ������������� ����
            /// </summary>
            /// <param name="Layer">����</param>
            /// <returns>����������</returns>
            public bool Contains(MapLayer Layer)
            {
                return _layers.Contains(Layer);
            }

            /// <summary>
            ///     ������������� ����
            /// </summary>
            /// <param name="Name">��� ����</param>
            /// <returns>����������</returns>
            public bool Contains(string Name)
            {
                foreach (MapLayer lay in _layers)
                    if (lay.Name == Name) return true;
                return false;
            }

            /// <summary>
            ///     �������� �����
            /// </summary>
            public void Clear()
            {
                _layers.Clear();
            }

            /// <summary>
            ///     ��������� ����������� �����
            /// </summary>
            /// <param name="Layer">����</param>
            /// <param name="newIndex">����� ������</param>
            public void Move(MapLayer Layer, int newIndex)
            {
                _layers.Remove(Layer);
                _layers.Insert(newIndex, Layer);
            }

            /// <summary>
            ///     ��������� ����������� �����
            /// </summary>
            /// <param name="index">������� ������</param>
            /// <param name="newIndex">����� ������</param>
            public void Move(int index, int newIndex)
            {
                MapLayer lay = this[index];
                _layers.Remove(lay);
                _layers.Insert(newIndex, lay);
            }

            /// <summary>
            ///     ������������ �����
            /// </summary>
            /// <param name="layerA">���� A</param>
            /// <param name="layerB">���� B</param>
            public void Change(MapLayer layerA, MapLayer layerB)
            {
                int _a = this[layerA];
                int _b = this[layerB];
                _layers.Remove(layerA);
                _layers.Remove(layerB);
                if (_a < _b)
                {
                    _layers.Insert(_a, layerA);
                    _layers.Insert(_b, layerB);
                }
                else
                {
                    _layers.Insert(_b, layerB);
                    _layers.Insert(_a, layerA);
                };
            }

            /// <summary>
            ///     ������������ �����
            /// </summary>
            /// <param name="indexA">������ ���� A ��� ������������</param>
            /// <param name="indexB">������ ���� B ��� ������������</param>
            public void Change(int indexA, int indexB)
            {
                MapLayer layA = this[indexA];
                MapLayer layB = this[indexB];
                _layers.Remove(layA);
                _layers.Remove(layB);
                if (indexA < indexB)
                {
                    _layers.Insert(indexA, layA);
                    _layers.Insert(indexB, layB);
                }
                else
                {
                    _layers.Insert(indexB, layB);
                    _layers.Insert(indexA, layA);
                };
            }

            /// <summary>
            ///     �����-��������
            /// </summary>
            internal NaviMapNetViewer _Map;
        }


        //internal class TEST
        //{
        //    void test()
        //    {
        //        MapEngine map = new MapEngine();
        //        MapLayer ml = new MapLayer("AAA");
        //        map.Layers.Add(ml);
                
        //        MapPoint mp = new MapPoint(0, 0);
        //        mp.SizePix = new Size(16, 16);
        //        mp.Visible = true;
        //        ml.Add(mp);

        //        MapMultiPoint mmp = new MapMultiPoint();
        //        mmp.Name = "Test";

        //        MapEllipse me = new MapEllipse(0, 0, 40, 100);
        //        me.BodyColor = Color.Yellow;

        //        MapLine ln = new MapLine(0, 0, 0, 0, 1);
        //        ln.Width = 3;
        //        ln.BorderColor = Color.Tomato;
        //        ln.BodyColor = Color.Red;

        //        MapPolyLine pln = new MapPolyLine(10, 3);

        //        MapPolygon pol = new MapPolygon(3);
        //        MapLayer lll = map.Layers[0];
        //        lll.Select(new Rectangle(0, 0, 0, 0), MapObjectType.mPoint | MapObjectType.mEllipse, true, false);
        //    }
        //}
}
