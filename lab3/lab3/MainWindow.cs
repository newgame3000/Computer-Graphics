using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using System.Numerics;
using Cairo;
using Gdk;
using Window = Gtk.Window;

namespace lab3
{
    class MainWindow : Window
    {
        [UI] private DrawingArea _drawingArea = null;
        [UI] private Adjustment _shiftX = null;
        [UI] private Adjustment _shiftY = null;
        [UI] private Adjustment _shiftZ = null;
        [UI] private Adjustment _scaleX = null;
        [UI] private Adjustment _scaleY = null;
        [UI] private Adjustment _scaleZ = null;
        [UI] private Adjustment _rotationX = null;
        [UI] private Adjustment _rotationY = null;
        [UI] private Adjustment _rotationZ = null;
        [UI] private CheckButton _all = null;
        [UI] private CheckButton _normal = null;
        [UI] private CheckButton _edges = null;
        [UI] private CheckButton _lightlines = null;
        [UI] private CheckButton _vertexNormal = null;
        [UI] private ComboBoxText _light = null;
        [UI] private ComboBoxText _proj = null;
        [UI] private ComboBoxText _mouse = null;
        [UI] private ComboBoxText _paint = null;
        
        [UI] private Adjustment _m11 = null;
        [UI] private Adjustment _m12 = null;
        [UI] private Adjustment _m13 = null;
        [UI] private Adjustment _m14 = null;
        [UI] private Adjustment _m21 = null;
        [UI] private Adjustment _m22 = null;
        [UI] private Adjustment _m23 = null;
        [UI] private Adjustment _m24 = null;
        [UI] private Adjustment _m31 = null;
        [UI] private Adjustment _m32 = null;
        [UI] private Adjustment _m33 = null;
        [UI] private Adjustment _m34 = null;
        [UI] private Adjustment _m41 = null;
        [UI] private Adjustment _m42 = null;
        [UI] private Adjustment _m43 = null;
        [UI] private Adjustment _m44 = null;
        
        [UI] private Adjustment _a = null;
        [UI] private Adjustment _b = null;
        [UI] private Adjustment _c = null;
        [UI] private Adjustment _phi = null;
        [UI] private Adjustment _theta = null;
        
        [UI] private Adjustment _r = null;
        [UI] private Adjustment _g = null;
        [UI] private Adjustment _bl = null;
        
        [UI] private Adjustment _kar = null;
        [UI] private Adjustment _kag = null;
        [UI] private Adjustment _kab = null;
        [UI] private Adjustment _kdr = null;
        [UI] private Adjustment _kdg = null;
        [UI] private Adjustment _kdb = null;
        [UI] private Adjustment _ksr = null;
        [UI] private Adjustment _ksg = null;
        [UI] private Adjustment _ksb = null;
        
        [UI] private Adjustment _lx = null;
        [UI] private Adjustment _ly = null;
        [UI] private Adjustment _lz = null;
        
        [UI] private Adjustment _iar = null;
        [UI] private Adjustment _iag = null;
        [UI] private Adjustment _iab = null;
        [UI] private Adjustment _ilr = null;
        [UI] private Adjustment _ilg = null;
        [UI] private Adjustment _ilb = null;
        
        [UI] private Adjustment _p = null;
        [UI] private Adjustment _d = null;
        [UI] private Adjustment _k = null;
        class Vertex
        {
            public Vector4 PointInLocalSpace;
            public Vector4 PointInWorldSpace;
            public Vector3 NormalInWorldSpace;

            public Vector3 Int;
            public List<Polygon> polygons= new List<Polygon>();
            public Vertex(float x, float y, float z, float w)
            {
                PointInLocalSpace = new Vector4(x, y, z, w);
            }
        }
        
        class Polygon
        {
            public List<Vertex> points;
            public Vector3 Color;

            public Vector3 NormalInWorldSpace;
            public Polygon()
            {
                points = new List<Vertex>();
            }
        }

        private List<Polygon> Polygons = new List<Polygon>();
        private List<List<Vertex>> Verticies = new List<List<Vertex>>();
        private bool motion = false;
        private float Oldx = -1;
        private float Oldy = -1;
        
        private Matrix4x4 WorldMatrix = new Matrix4x4
            (1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        
        private Matrix4x4 ViewMatrix = new Matrix4x4
            (1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        
        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
        }

        CairoSurface surface;
        
        void Figure()
        {
            Verticies = new List<List<Vertex>>();
            
            Verticies.Add(new List<Vertex>());
            Verticies[Verticies.Count - 1].Add(new Vertex(0, 0, (int)_c.Value, 1));
            Verticies[Verticies.Count - 1].Add(new Vertex(0, 0, -(int)_c.Value, 1));
            
            double phi = 2 * Math.PI / _phi.Value;
            double theta = 2 * Math.PI / _theta.Value;

            double sumphi = 0;
            double sumtheta = 0;
            
            for (int i = 0; i < (int)_phi.Value; ++i)
            {
                sumphi += phi;
                Verticies.Add(new List<Vertex>());
                for (int j = 1; j < (int)_theta.Value / 2; ++j)
                {
                    sumtheta += theta;
                    Vertex A = new((float)((int)_a.Value * Math.Sin(sumtheta) * Math.Cos(sumphi)), (float)((int)_b.Value * Math.Sin(sumtheta) * Math.Sin(sumphi)),
                        (float)((int)_c.Value * Math.Cos(sumtheta)), 1);
                    Verticies[Verticies.Count - 1].Add(A);
                }
                sumtheta = 0;
            }

            Polygons = new List<Polygon>();


            //Крышка
            for (int i = 1; i < Verticies.Count - 1; ++i)
            {
                Polygons.Add(new Polygon());
                Polygons[Polygons.Count - 1].points.Add(Verticies[0][0]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[i + 1][0]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[i][0]);

                Verticies[0][0].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[i + 1][0].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[i][0].polygons.Add(Polygons[Polygons.Count - 1]);
            }
            
            Polygons.Add(new Polygon());
            Polygons[Polygons.Count - 1].points.Add(Verticies[0][0]);
            Polygons[Polygons.Count - 1].points.Add(Verticies[1][0]);
            Polygons[Polygons.Count - 1].points.Add(Verticies[Verticies.Count - 1][0]);
            
            Verticies[0][0].polygons.Add(Polygons[Polygons.Count - 1]);
            Verticies[1][0].polygons.Add(Polygons[Polygons.Count - 1]);
            Verticies[Verticies.Count - 1][0].polygons.Add(Polygons[Polygons.Count - 1]);
                    
            //Середина
            for (int i = 1; i < Verticies.Count - 1; ++i)
            {
                for (int j = 0; j < Verticies[i].Count - 1; ++j)
                {
                    Polygons.Add(new Polygon());
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i][j + 1]);
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i][j]);
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i + 1][j + 1]);
                    
                    Verticies[i][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                    Verticies[i][j].polygons.Add(Polygons[Polygons.Count - 1]);
                    Verticies[i + 1][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                    
                    Polygons.Add(new Polygon());
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i + 1][j + 1]);
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i][j]);
                    Polygons[Polygons.Count - 1].points.Add(Verticies[i + 1][j]);
                    
                    Verticies[i + 1][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                    Verticies[i][j].polygons.Add(Polygons[Polygons.Count - 1]);
                    Verticies[i + 1][j].polygons.Add(Polygons[Polygons.Count - 1]);
                }
            }
            
            for (int j = 0; j < Verticies[Verticies.Count - 1].Count - 1; ++j)
            {
                Polygons.Add(new Polygon());
                Polygons[Polygons.Count - 1].points.Add(Verticies[Verticies.Count - 1][j]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[1][j + 1]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[Verticies.Count - 1][j + 1]);
                
                Verticies[Verticies.Count - 1][j].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[1][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[Verticies.Count - 1][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                
                Polygons.Add(new Polygon());
                Polygons[Polygons.Count - 1].points.Add(Verticies[1][j + 1]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[Verticies.Count - 1][j]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[1][j]);
                
                Verticies[1][j + 1].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[Verticies.Count - 1][j].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[1][j].polygons.Add(Polygons[Polygons.Count - 1]);
            }

            //Нижняя крышка
            for (int i = 1; i < Verticies.Count - 1; ++i)
            {
                Polygons.Add(new Polygon());
                Polygons[Polygons.Count - 1].points.Add(Verticies[0][1]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[i][Verticies[i].Count - 1]);
                Polygons[Polygons.Count - 1].points.Add(Verticies[i + 1][Verticies[i + 1].Count - 1]);
                
                Verticies[0][1].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[i][Verticies[i].Count - 1].polygons.Add(Polygons[Polygons.Count - 1]);
                Verticies[i + 1][Verticies[i + 1].Count - 1].polygons.Add(Polygons[Polygons.Count - 1]);
            }
            
            Polygons.Add(new Polygon());
            Polygons[Polygons.Count - 1].points.Add(Verticies[0][1]);
            Polygons[Polygons.Count - 1].points.Add(Verticies[Verticies.Count - 1][Verticies[Verticies.Count - 1].Count - 1]);
            Polygons[Polygons.Count - 1].points.Add(Verticies[1][Verticies[1].Count - 1]);
            
            Verticies[0][1].polygons.Add(Polygons[Polygons.Count - 1]);
            Verticies[Verticies.Count - 1][Verticies[Verticies.Count - 1].Count - 1].polygons.Add(Polygons[Polygons.Count - 1]);
            Verticies[1][Verticies[1].Count - 1].polygons.Add(Polygons[Polygons.Count - 1]);
                    
            if (_paint.ActiveText == "Случайные цвета")
            {
                RandomColor();
            }
            
            if (_paint.ActiveText == "Выбранный цвет")
            {
                Color();
            }
        }
        
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            surface = new CairoSurface(_drawingArea);
            
            Figure();
            CalculateMatrix();
            
            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
            
            _drawingArea.SizeAllocated += DrawingAreaOnSizeAllocated;
            _drawingArea.Drawn += Drawing;
            
            _shiftX.ValueChanged += ValueChanged2;
            _shiftY.ValueChanged += ValueChanged2;
            _shiftZ.ValueChanged += ValueChanged2;
            
            _scaleX.ValueChanged += ValueChanged2;
            _scaleY.ValueChanged += ValueChanged2;
            _scaleZ.ValueChanged += ValueChanged2;
            
            _rotationX.ValueChanged += ValueChanged2;
            _rotationY.ValueChanged += ValueChanged2;
            _rotationZ.ValueChanged += ValueChanged2;
            
            _normal.Toggled += ValueChanged2;
            _all.Toggled += ValueChanged2;
            _edges.Toggled += ValueChanged2;
            
            _drawingArea.MotionNotifyEvent += DrawingAreaOnMotionNotifyEvent;
            _drawingArea.ButtonReleaseEvent += DrawingAreaOnButtonReleaseEvent;
            _drawingArea.ButtonPressEvent += DrawingAreaOnButtonPressEvent;

            _phi.ValueChanged += ValueChanged;
            _theta.ValueChanged += ValueChanged;

            _a.ValueChanged += ValueChanged;
            _b.ValueChanged += ValueChanged;
            _c.ValueChanged += ValueChanged;
            
            _r.ValueChanged += ValueChanged;
            _g.ValueChanged += ValueChanged;
            _bl.ValueChanged += ValueChanged;

            _kag.ValueChanged += ValueChanged2;
            _kar.ValueChanged += ValueChanged2;
            _kab.ValueChanged += ValueChanged2;
            _kdr.ValueChanged += ValueChanged2;
            _kdg.ValueChanged += ValueChanged2;
            _kdb.ValueChanged += ValueChanged2;
            _ksr.ValueChanged += ValueChanged2;
            _ksg.ValueChanged += ValueChanged2;
            _ksb.ValueChanged += ValueChanged2;

            _iab.ValueChanged += ValueChanged2;
            _iag.ValueChanged += ValueChanged2;
            _iar.ValueChanged += ValueChanged2;
            _ilb.ValueChanged += ValueChanged2;
            _ilg.ValueChanged += ValueChanged2;
            _ilr.ValueChanged += ValueChanged2;

            _lx.ValueChanged += ValueChanged2;
            _ly.ValueChanged += ValueChanged2;
            _lz.ValueChanged += ValueChanged2;

            _p.ValueChanged += ValueChanged2;
            _d.ValueChanged += ValueChanged2;
            _k.ValueChanged += ValueChanged2;
            _lightlines.Clicked += ValueChanged2;
            
            _drawingArea.ScrollEvent += (o, args) =>
            {
                if (args.Event.Direction == ScrollDirection.Down)
                {
                    _scaleX.Value -= 0.1;
                    _scaleY.Value -= 0.1;
                    _scaleZ.Value -= 0.1;
                    
                }
                else if (args.Event.Direction == ScrollDirection.Up)
                {
                    _scaleX.Value += 0.1;
                    _scaleY.Value += 0.1;
                    _scaleZ.Value += 0.1;
                }
            };
             
            _proj.Append("None", "Не задана");
            _proj.Append("Side", "Вид сбоку");
            _proj.Append("Top", "Вид сверху");
            _proj.Append("Front", "Вид спереди");
            _proj.Append("Izom", "Изометрия");
            _proj.Changed += ValueChanged;

            _mouse.Append("shift", "Перемещение");
            _mouse.Append("rotation", "Вращение");
            
            _light.Append("None", "Нет");
            _light.Append("Flat", "Плоское затенение");
            _light.Append("Guro", "Затенение Гуро");
            _light.Changed += ValueChanged;
            
            _paint.Append("Nones", "Нет");
            _paint.Append("Rand", "Случайные цвета");
            _paint.Append("P", "Выбранный цвет");
            _paint.Changed += ValueChanged;
            
            DeleteEvent += Window_DeleteEvent;
        }

        private void ValueChanged(object? sender, EventArgs e)
        {
            Figure();
            CalculateMatrix();
            _drawingArea.QueueDraw();
        }

        private void ValueChanged2(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void RandomColor()
        {
            for (int i = 0; i < Polygons.Count; ++i)
            {
                Random gen = new((int) DateTime.Now.Ticks & 0x0000FFFF);
                HSV.ToRgb(gen.NextDouble(), 1, 1, out var r, out var g, out var b);
                Polygons[i].Color = new Vector3((float)r, (float)g, (float)b);
            }
        }

        void Color()
        {
            for (int i = 0; i < Polygons.Count; ++i)
            {
                Polygons[i].Color = new Vector3((float)_r.Value, (float)_g.Value, (float)_bl.Value);
            }
        }
        
        private void PaintOnToggled(object? sender, EventArgs e)
        {
            RandomColor();
            _drawingArea.QueueDraw();
        }

        private void DrawingAreaOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (motion)
            {
                if (_mouse.ActiveText == "Перемещение")
                {
                    _shiftX.Value += ((float) args.Event.X - Oldx) / 30;
                    _shiftY.Value += ((float) args.Event.Y - Oldy) / 30;
                    _drawingArea.QueueDraw();

                    Oldx = (float) args.Event.X;
                    Oldy = (float) args.Event.Y;
                }

                if (_mouse.ActiveText == "Вращение")
                {
                    _rotationY.Value += -((float) args.Event.X - Oldx) / 10;
                    _rotationX.Value += ((float) args.Event.Y - Oldy) / 10;
                    
                    _drawingArea.QueueDraw();

                    Oldx = (float) args.Event.X;
                    Oldy = (float) args.Event.Y;
                }

            }
        }
        
        private void DrawingAreaOnButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            motion = true;
            Oldx = (float) args.Event.X;
            Oldy = (float) args.Event.Y;
        }
        
        private void DrawingAreaOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            motion = false;
        }

        private void DrawingAreaOnSizeAllocated(object o, SizeAllocatedArgs args)
        {

            float maxX = -1;
            float maxY = -1;

            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    if (Verticies[i][j].PointInWorldSpace.X > maxX)
                    {
                        maxX = Verticies[i][j].PointInWorldSpace.X;
                    }

                    if (Verticies[i][j].PointInWorldSpace.Y > maxY)
                    {
                        maxY = Verticies[i][j].PointInWorldSpace.Y;
                    }
                }
            }

            float m = Math.Min(args.Allocation.Width / (4 * maxX), args.Allocation.Height / (4 * maxY));
            
            ViewMatrix.M11 = m;
            ViewMatrix.M22 = m;
            
            ViewMatrix.M41 = args.Allocation.Width / 2;
            ViewMatrix.M42 = args.Allocation.Height / 2;
            _drawingArea.QueueDraw();
        }
        
        void CalculateMatrix()
        {

            WorldMatrix = Matrix4x4.Identity;
            
            WorldMatrix *= Matrix4x4.CreateScale((float)_scaleX.Value, (float)_scaleY.Value, (float)_scaleZ.Value);
            
            if (_proj.ActiveText == "Изометрия")
            {
                WorldMatrix *= Matrix4x4.CreateRotationY((float) (-45 * Math.PI / 180)) *
                               Matrix4x4.CreateRotationX((float) (35 * Math.PI / 180));
            }
            else
            {
                WorldMatrix *= Matrix4x4.CreateRotationX((float) (_rotationX.Value * Math.PI / 180) );
                WorldMatrix *= Matrix4x4.CreateRotationY((float) (_rotationY.Value * Math.PI / 180));
                WorldMatrix *= Matrix4x4.CreateRotationZ((float) (_rotationZ.Value * Math.PI / 180));
            }

            WorldMatrix *= Matrix4x4.CreateTranslation((float) _shiftX.Value, (float) _shiftY.Value, (float)_shiftZ.Value);
            
            Matrix4x4 projectionMatrix = Matrix4x4.Identity;
            
            if (_proj.ActiveText == "Вид сверху")
            {
                projectionMatrix.M22 = 0;
            }
            
            if (_proj.ActiveText == "Вид сбоку")
            {
                projectionMatrix.M11 = 0;
            }
            
            if (_proj.ActiveText == "Вид спереди")
            {
                projectionMatrix.M33 = 0;
            }

            WorldMatrix = projectionMatrix * WorldMatrix;

            _m11.Value = WorldMatrix.M11;
            _m12.Value = WorldMatrix.M12;
            _m13.Value = WorldMatrix.M13;
            _m14.Value = WorldMatrix.M14;
            _m21.Value = WorldMatrix.M21;
            _m22.Value = WorldMatrix.M22;
            _m23.Value = WorldMatrix.M23;
            _m24.Value = WorldMatrix.M24;
            _m31.Value = WorldMatrix.M31;
            _m32.Value = WorldMatrix.M32;
            _m33.Value = WorldMatrix.M33;
            _m34.Value = WorldMatrix.M34;
            _m41.Value = WorldMatrix.M41;
            _m42.Value = WorldMatrix.M42;
            _m43.Value = WorldMatrix.M43;
            _m44.Value = WorldMatrix.M44;
            
            WMatrix();
            _drawingArea.QueueDraw();
        }

        void WMatrix()
        {
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Verticies[i][j].PointInWorldSpace =
                        Vector4.Transform(Verticies[i][j].PointInLocalSpace, WorldMatrix);
                }
            }
        }

        Vector4 View(Vector4 a)
        {
            Vector4 ans = Vector4.Transform(a, ViewMatrix);
            return ans;
        }

        bool Hide(Polygon a)
        {
            Vector2 start = new Vector2((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X) / 3, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y) / 3 );
            
            start = Vector2.Transform(start, ViewMatrix);

            Vector3 vector3 = new Vector3(a.NormalInWorldSpace.X, a.NormalInWorldSpace.Y, a.NormalInWorldSpace.Z);
            
            vector3.X = vector3.X + start.X;
            vector3.Y = vector3.Y + start.Y;
            
            vector3 = Vector3.Transform(vector3, ViewMatrix);
            
            if (vector3.Z > 0)
            {
                return true;
            }
            return false;
        }

        Vector3 Normalize(Vector3 vector1)
        {
            double len = Math.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
            if (len == 0)
            {
                vector1.X = 0;
                vector1.Y = 0;
                vector1.Z = 0;
            }
            else
            {
                vector1.X = (float) (_scaleX.Value  * (vector1.X / len) / 2);
                vector1.Y = (float) (_scaleY.Value  * (vector1.Y / len) / 2);
                vector1.Z = (float) (_scaleZ.Value  * (vector1.Z / len) / 2);
            }

            return vector1;
        }

        Vector3 DifferenceVector3(Vector4 a, Vector4 b)
        {
            Vector3 ans = new Vector3();
            ans.X = a.X - b.X;
            ans.Y = a.Y - b.Y;
            ans.Z = a.Z - b.Z;
            return ans;
        }

        void DrawNormals(Context cr, Polygon a)
        {
            Vector2 start = new Vector2((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X) / 3, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y) / 3 );

            Vector3 vector3 = new Vector3(a.NormalInWorldSpace.X, a.NormalInWorldSpace.Y, a.NormalInWorldSpace.Z);

            vector3.X = vector3.X + start.X;
            vector3.Y = vector3.Y + start.Y;
            
            vector3 = Vector3.Transform(vector3, ViewMatrix);
            start = Vector2.Transform(start, ViewMatrix);
            
            cr.SetSourceRGB(.0, 0, 1);
            cr.MoveTo(start.X, start.Y);
            cr.LineTo(vector3.X, vector3.Y);
            cr.Stroke();
        }

        void Normals(Polygon a)
        {
            Vector3 vector1 =  DifferenceVector3(a.points[0].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            Vector3 vector2 =  DifferenceVector3(a.points[2].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            

            Vector3 vector3 = new Vector3(vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);

            vector3 = Normalize(vector3);
            
            a.NormalInWorldSpace = vector3;
        }

        Vector3 Multiplication(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        
        Vector3 Background()
        {
            Vector3 ka = new Vector3((float)_kar.Value, (float)_kag.Value, (float)_kab.Value);
            Vector3 ia = new Vector3((float)_iar.Value, (float)_iag.Value, (float)_iab.Value);
            return Multiplication(ka, ia);
        }

        Vector3 Diffuse( Polygon a)
        {
            Vector3 ans = new Vector3();
            Vector3 kd = new Vector3((float)_kdr.Value, (float)_kdg.Value, (float)_kdb.Value);
            Vector3 il = new Vector3((float)_ilr.Value, (float)_ilg.Value, (float)_ilb.Value);

            ans = Multiplication(kd, il);

             Vector3 start = new Vector3((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X) / 3, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y) / 3, (a.points[0].PointInWorldSpace.Z + a.points[1].PointInWorldSpace.Z + a.points[2].PointInWorldSpace.Z) / 3 );
            Vector3 dot = new Vector3((float)_lx.Value - start.X, (float)_ly.Value - start.Y,(float)_lz.Value - start.Z);
            
            dot = Normalize(dot);
            ans *= Vector3.Dot(dot, a.NormalInWorldSpace);
            ans.X = Math.Max(0, ans.X);
            ans.Y = Math.Max(0, ans.Y);
            ans.Z = Math.Max(0, ans.Z);
            
            dot.X = (float)_lx.Value - a.NormalInWorldSpace.X - start.X;
            dot.Y = (float)_ly.Value - a.NormalInWorldSpace.Y - start.Y;
            dot.Z = (float)_lz.Value - a.NormalInWorldSpace.Z - start.Z;
            return ans/ (float)(dot.Length() / 50 + _k.Value);
        }

        Vector3 Specular(Polygon a)
        {
            Vector3 ans = new Vector3();
            Vector3 ks = new Vector3((float)_ksr.Value, (float)_ksg.Value, (float)_ksb.Value);
            Vector3 il = new Vector3((float)_ilr.Value, (float)_ilg.Value, (float)_ilb.Value);
            
            ans = Multiplication(ks, il);
            
            Vector3 start = new Vector3((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X) / 3, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y) / 3, (a.points[0].PointInWorldSpace.Z + a.points[1].PointInWorldSpace.Z + a.points[2].PointInWorldSpace.Z) / 3 );
            Vector3 l = new Vector3((float)_lx.Value - start.X, (float)_ly.Value - start.Y,(float)_lz.Value - start.Z);
            l = Normalize(l);

            if (Vector3.Dot(l, a.NormalInWorldSpace) < 1e-6)
            {
                return new Vector3(0, 0, 0);
            }
            
            Vector3 r = new Vector3();

            r = 2 * a.NormalInWorldSpace * (Vector3.Dot(l, a.NormalInWorldSpace) / Vector3.Dot(a.NormalInWorldSpace, a.NormalInWorldSpace));
            r = r - l;
            
            r = Normalize(r);

            float cosRL = 1;
            
            for (int i = 0; i < _p.Value; ++i)
            {
                double t = Vector3.Dot(r, l);
                if (t > -1e-6)
                {
                    cosRL *= Vector3.Dot(r, l);
                }
                else
                {
                    return new Vector3(0, 0, 0);
                }
            }

            ans *= cosRL;
            ans.X = Math.Max(0, ans.X);
            ans.Y = Math.Max(0, ans.Y);
            ans.Z = Math.Max(0, ans.Z);
            
            l.X = (float)_lx.Value - a.NormalInWorldSpace.X - start.X;
            l.Y = (float)_ly.Value - a.NormalInWorldSpace.Y - start.Y;
            l.Z = (float)_lz.Value - a.NormalInWorldSpace.Z - start.Z;
            return ans/ (float)(l.Length()/ 50+ _k.Value);
        }

        void VertexNormals()
        {
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Vector3 n = new Vector3(0, 0, 0);
                    for (int k = 0; k < Verticies[i][j].polygons.Count; ++k)
                    {
                        n += Verticies[i][j].polygons[k].NormalInWorldSpace;
                    }

                    n.X = n.X / Verticies[i][j].polygons.Count;
                    n.Y = n.Y / Verticies[i][j].polygons.Count;
                    n.Z = n.Z / Verticies[i][j].polygons.Count;
                    n = Normalize(n);
                    Verticies[i][j].NormalInWorldSpace = n;
                }
            }
        }

        void Diffuse()
        {
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Vector3 ans = new Vector3();
                    Vector3 kd = new Vector3((float)_kdr.Value, (float)_kdg.Value, (float)_kdb.Value);
                    Vector3 il = new Vector3((float)_ilr.Value, (float)_ilg.Value, (float)_ilb.Value);
                    
                    ans = Multiplication(kd, il);

                    Vector3 start = new Vector3(Verticies[i][j].PointInWorldSpace.X, Verticies[i][j].PointInWorldSpace.Y, Verticies[i][j].PointInWorldSpace.Z);
            
                    Vector3 dot = new Vector3((float)_lx.Value - start.X, (float)_ly.Value - start.Y,(float)_lz.Value - start.Z);
            
                    dot = Normalize(dot);
                    ans *= Vector3.Dot(dot, Verticies[i][j].NormalInWorldSpace);
                    ans.X = Math.Max(0, ans.X);
                    ans.Y = Math.Max(0, ans.Y);
                    ans.Z = Math.Max(0, ans.Z);

                    dot.X = (float)_lx.Value - Verticies[i][j].PointInWorldSpace.X;
                    dot.Y = (float)_ly.Value - Verticies[i][j].PointInWorldSpace.Y;
                    dot.Z = (float)_lz.Value - Verticies[i][j].PointInWorldSpace.Z;
                    Verticies[i][j].Int += (ans / (float)(dot.Length() / 30  + _k.Value));
                }
            }
        }

        void Specular()
        {
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Vector3 ans = new Vector3();
                    Vector3 ks = new Vector3((float)_ksr.Value, (float)_ksg.Value, (float)_ksb.Value);
                    Vector3 il = new Vector3((float)_ilr.Value, (float)_ilg.Value, (float)_ilb.Value);
                    
                    ans = Multiplication(ks, il);
                    
                    Vector3 start = new Vector3(Verticies[i][j].PointInWorldSpace.X, Verticies[i][j].PointInWorldSpace.Y, Verticies[i][j].PointInWorldSpace.Z);
                    Vector3 l = new Vector3((float)_lx.Value - start.X, (float)_ly.Value - start.Y,(float)_lz.Value - start.Z);
                    l = Normalize(l);

                    if (Vector3.Dot(l, Verticies[i][j].NormalInWorldSpace) < 1e-6)
                    {
                        continue;
                    }
                                        
                    Vector3 r = new Vector3();

                    r = 2 * Verticies[i][j].NormalInWorldSpace * (Vector3.Dot(l, Verticies[i][j].NormalInWorldSpace) / Vector3.Dot(Verticies[i][j].NormalInWorldSpace, Verticies[i][j].NormalInWorldSpace));
                    r = r - l;
                    
                    r = Normalize(r);

                    float cosRL = 1;

                    bool end = false;
                    for (int k = 0; k < _p.Value; ++k)
                    {
                        double t = Vector3.Dot(r, l);
                        if (t > -1e-6)
                        {
                            cosRL *= Vector3.Dot(r, l);
                        }
                        else
                        {
                            end = true;
                            break;
                        }
                    }

                    if (end)
                    {
                        continue;
                    }
                    
                    ans *= cosRL;
                    ans.X = Math.Max(0, ans.X);
                    ans.Y = Math.Max(0, ans.Y);
                    ans.Z = Math.Max(0, ans.Z);

                    l.X = (float)_lx.Value - start.X;
                    l.Y = (float)_ly.Value - start.Y;
                    l.Z = (float)_lz.Value - start.Z;
                    Verticies[i][j].Int += (ans / (float)(l.Length() / 30  + _k.Value));
                }
            }
        }

        void DrawVertexNormals(Context cr)
        {
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    cr.SetSourceRGB(.0, .0, .0);

                    Vector2 start = new Vector2(Verticies[i][j].PointInWorldSpace.X,
                        Verticies[i][j].PointInWorldSpace.Y);
                    Vector2 end= new Vector2(Verticies[i][j].NormalInWorldSpace.X, Verticies[i][j].NormalInWorldSpace.Y);
                    
                    end.X = end.X + start.X;
                    end.Y = end.Y + start.Y;
                    
                    start = Vector2.Transform(start, ViewMatrix);
                    end = Vector2.Transform(end, ViewMatrix);
                    
                    cr.MoveTo(start.X, start.Y);
                    cr.LineTo(end.X, end.Y);
                    cr.Stroke();
                }
            }
        }
        private void Drawing(object o, DrawnArgs args)
        {
            var cr = args.Cr;
            cr.SetSourceRGB(0.9, 0.9, 0.9);
            cr.Paint();
            cr.Antialias = Antialias.Subpixel;
            cr.LineWidth = 2d;
            cr.SetSourceRGB(.0, .0, .0);
            cr.LineJoin = LineJoin.Round;

            for (int i = 0; i < Polygons.Count; ++i)
            {
                Normals(Polygons[i]);
            }

            if (_light.ActiveText == "Затенение Гуро")
            {
                Vector3 I = new Vector3();
                I = Background();
                                
                VertexNormals();

                for (int k = 0; k < Verticies.Count; ++k)
                {
                    for (int t = 0; t < Verticies[k].Count; ++t)
                    {
                        Verticies[k][t].Int = new Vector3(0, 0, 0);
                        Verticies[k][t].Int += I;
                    }
                }

                Diffuse();
                Specular();
            }
            
            
            for (int i = 0; i < Polygons.Count; ++i)
            {

                if (!_all.Active && Hide(Polygons[i])) 
                {
                    continue;
                }
                List<Vector2> Triangle = new List<Vector2>();

                Vector4 picture = View(Polygons[i].points[0].PointInWorldSpace); 
                Triangle.Add(new Vector2(picture.X, picture.Y));
                cr.MoveTo(picture.X, picture.Y);
                for (int j = 1; j < Polygons[i].points.Count; ++j)
                {
                    picture =  View(Polygons[i].points[j].PointInWorldSpace); 
                    Triangle.Add(new Vector2(picture.X, picture.Y));
                    cr.LineTo(picture.X, picture.Y);
                }
                
                picture = View(Polygons[i].points[0].PointInWorldSpace);
                cr.LineTo(picture.X, picture.Y);

                if (_paint.ActiveText == "Случайные цвета" || _paint.ActiveText == "Выбранный цвет" && _light.ActiveText != "Затенение Гуро")
                {

                    Vector3 colors = new Vector3(Polygons[i].Color.X, Polygons[i].Color.Y, Polygons[i].Color.Z);

                    if (_light.ActiveText == "Плоское затенение")
                    {
                        Vector3 I = new Vector3();
                        I = Background();

                        I += Diffuse(Polygons[i]);
                        I += Specular(Polygons[i]);

                        colors = Multiplication(I, colors);
                    }
                    
                    
                    cr.SetSourceRGB(colors.X, colors.Y, colors.Z);
                    if (_edges.Active)
                    { 
                        cr.FillPreserve();
                    }
                    else
                    {
                        cr.Antialias = Antialias.None;
                        cr.Fill();
                    }
                }

                if (_edges.Active)
                {
                    cr.SetSourceRGB(.0, .0, .0);
                    cr.Stroke(); 
                }
                else
                {
                    if (_paint.ActiveText != "Нет")
                    {
                        cr.SetSourceRGB(0.9, 0.9, 0.9);
                    }
                    cr.Stroke();  
                }
                
                if (_lightlines.Active)
                {
                    Vector3 l = new Vector3((float) _lx.Value, (float) _ly.Value, (float) _lz.Value);
                    Vector2 start = new Vector2((Polygons[i].points[0].PointInWorldSpace.X + Polygons[i].points[1].PointInWorldSpace.X + Polygons[i].points[2].PointInWorldSpace.X) / 3, (Polygons[i].points[0].PointInWorldSpace.Y + Polygons[i].points[1].PointInWorldSpace.Y + Polygons[i].points[2].PointInWorldSpace.Y) / 3 );
                    cr.SetSourceRGB(1, 0, 0);
                    
                    Vector3 r = new Vector3();
                    
                    r = 2 * Polygons[i].NormalInWorldSpace * Vector3.Dot(l, Polygons[i].NormalInWorldSpace) / Vector3.Dot(Polygons[i].NormalInWorldSpace, Polygons[i].NormalInWorldSpace);
                    r = r - l;
            
                    r = Normalize(r);
                    
                    start = Vector2.Transform(start, ViewMatrix);
                    l = Vector3.Transform(l, ViewMatrix);
                    
                    cr.MoveTo(start.X, start.Y);
                    cr.LineTo(l.X , l.Y );
                    
                    r = Vector3.Transform(r, ViewMatrix);
                    
                    cr.MoveTo(start.X, start.Y);
                    cr.LineTo(r.X , r.Y );
                    cr.Stroke();
                } 
                
            }
            
            if (_light.ActiveText == "Затенение Гуро")
            {
                surface.BeginUpdate(cr);
                for (int i = 0; i < Polygons.Count; ++i)
                {

                    if (!_all.Active && Hide(Polygons[i]))
                    {
                        continue;
                    }

                    List<Vector2> Triangle = new List<Vector2>();

                    Vector4 picture = View(Polygons[i].points[0].PointInWorldSpace);
                    Triangle.Add(new Vector2(picture.X, picture.Y));
                    for (int j = 1; j < Polygons[i].points.Count; ++j)
                    {
                        picture = View(Polygons[i].points[j].PointInWorldSpace);
                        Triangle.Add(new Vector2(picture.X, picture.Y));
                    }
                    
                    surface.DrawTriangle(Multiplication(Polygons[i].Color, Polygons[i].points[0].Int), Triangle[0], Multiplication(Polygons[i].Color, Polygons[i].points[1].Int), Triangle[1], Multiplication(Polygons[i].Color, Polygons[i].points[2].Int), Triangle[2]);

                }
                surface.EndUpdate();
            }
            
            
            
            if (_light.ActiveText != "Нет" && _paint.ActiveText != "Нет")
            {
                 cr.SetSourceRGB(1, 0.0, 0.0);
                  Vector3 l = new Vector3((float) _lx.Value, (float) _ly.Value, (float) _lz.Value);
                  l = Vector3.Transform(l, ViewMatrix);
                 cr.Arc( l.X,   l.Y,3,0,360);
                 cr.Fill();
                 cr.SetSourceRGB(.0, .0, .0);
            }
            
            if (_normal.Active)
            {
                for (int i = 0; i < Polygons.Count; ++i)
                {
                    if (!_all.Active && Hide(Polygons[i])) 
                    {
                        continue;
                    }
                    DrawNormals(cr, Polygons[i]);
                }
            }
            
            if (_vertexNormal.Active)
            {
                DrawVertexNormals(cr);
            }
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}