using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using System.Numerics;
using Cairo;
using Gdk;
using Window = Gtk.Window;

namespace lab2
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
        [UI] private CheckButton _paint = null;
        [UI] private CheckButton _edges = null;
        [UI] private ComboBoxText _proj = null;
        [UI] private ComboBoxText _mouse = null;
        
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
        
        
        
        class Vertex
        {
            public Vector4 PointInLocalSpace;
            public Vector4 PointInWorldSpace;
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

            public Polygon()
            {
                points = new List<Vertex>();
            }
        }

        private List<Polygon> Polygons = new List<Polygon>();
        private List<Vertex> Verticies = new List<Vertex>();
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

        void Figure()
        {
            Verticies = new List<Vertex>()
            {
                new Vertex(-25, -70, -25, 1),
                new Vertex(25, -70, -25, 1),
                new Vertex(25, -70, 25, 1),
                new Vertex(-25, -70, 25, 1),
                
                new Vertex(-50, 0, -50, 1),
                new Vertex(50, 0, -50, 1),
                new Vertex(50, 0, 50, 1),
                new Vertex(-50, 0, 50, 1),

            };
            
            Polygons.Add(new Polygon());
            for (int i = 3; i >= 0; --i)
            {
                Polygons[0].points.Add(Verticies[i]);
                Verticies[i].polygons.Add(Polygons[0]);
            }
            
            Polygons.Add(new Polygon());
            Verticies[1].polygons.Add(Polygons[1]);
            Verticies[2].polygons.Add(Polygons[1]);
            Verticies[6].polygons.Add(Polygons[1]);
            Verticies[5].polygons.Add(Polygons[1]);
            
            Polygons[1].points.Add(Verticies[1]);
            Polygons[1].points.Add(Verticies[2]);
            Polygons[1].points.Add(Verticies[6]);
            Polygons[1].points.Add(Verticies[5]);
            
            
            Polygons.Add(new Polygon());
            Verticies[2].polygons.Add(Polygons[2]);
            Verticies[3].polygons.Add(Polygons[2]);
            Verticies[7].polygons.Add(Polygons[2]);
            Verticies[6].polygons.Add(Polygons[2]);
            
            Polygons[2].points.Add(Verticies[2]);
            Polygons[2].points.Add(Verticies[3]);
            Polygons[2].points.Add(Verticies[7]);
            Polygons[2].points.Add(Verticies[6]);
            
            
            Polygons.Add(new Polygon());
            Verticies[3].polygons.Add(Polygons[3]);
            Verticies[0].polygons.Add(Polygons[3]);
            Verticies[4].polygons.Add(Polygons[3]);
            Verticies[7].polygons.Add(Polygons[3]);
            
            Polygons[3].points.Add(Verticies[3]);
            Polygons[3].points.Add(Verticies[0]);
            Polygons[3].points.Add(Verticies[4]);
            Polygons[3].points.Add(Verticies[7]);
            
            
            Polygons.Add(new Polygon());
            Verticies[0].polygons.Add(Polygons[4]);
            Verticies[1].polygons.Add(Polygons[4]);
            Verticies[5].polygons.Add(Polygons[4]);
            Verticies[4].polygons.Add(Polygons[4]);
            
            Polygons[4].points.Add(Verticies[0]);
            Polygons[4].points.Add(Verticies[1]);
            Polygons[4].points.Add(Verticies[5]);
            Polygons[4].points.Add(Verticies[4]);
            
            Polygons.Add(new Polygon());
            for (int i = 4; i < Verticies.Count; ++i)
            {
                Polygons[5].points.Add(Verticies[i]);
                Verticies[i].polygons.Add(Polygons[5]);
            }
            
        }
        
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            
            Figure();
            CalculateMatrix();
            
            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
            
            _drawingArea.SizeAllocated += DrawingAreaOnSizeAllocated;
            _drawingArea.Drawn += Drawing;
            
            _shiftX.ValueChanged += ShiftXOnValueChanged;
            _shiftY.ValueChanged += ShiftYOnValueChanged;
            _shiftZ.ValueChanged += ShiftZOnValueChanged;
            
            _scaleX.ValueChanged += ScaleXOnValueChanged;
            _scaleY.ValueChanged += ScaleYOnValueChanged;
            _scaleZ.ValueChanged += ScaleZOnValueChanged;
            
            _rotationX.ValueChanged += RotationXOnValueChanged;
            _rotationY.ValueChanged += RotationYOnValueChanged;
            _rotationZ.ValueChanged += RotationZOnValueChanged;
            
            _normal.Toggled += NormalOnToggled;
            _all.Toggled += AllOnToggled;
            _paint.Toggled += PaintOnToggled;
            _edges.Toggled += EdgesOnToggled;
            
            _drawingArea.MotionNotifyEvent += DrawingAreaOnMotionNotifyEvent;
            _drawingArea.ButtonReleaseEvent += DrawingAreaOnButtonReleaseEvent;
            _drawingArea.ButtonPressEvent += DrawingAreaOnButtonPressEvent;


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
            _proj.Changed += ProjOnChanged;

            _mouse.Append("shift", "Перемещение");
            _mouse.Append("rotation", "Вращение");
            
            DeleteEvent += Window_DeleteEvent;
        }

        private void ProjOnChanged(object? sender, EventArgs e)
        {
             CalculateMatrix();
        }

        private void EdgesOnToggled(object? sender, EventArgs e)
        {
            _drawingArea.QueueDraw();
        }

        private void PaintOnToggled(object? sender, EventArgs e)
        {
            for (int i = 0; i < Polygons.Count; ++i)
            {
                Random gen = new((int) DateTime.Now.Ticks & 0x0000FFFF);
                HSV.ToRgb(gen.NextDouble(), 1, 1, out var r, out var g, out var b);
                Polygons[i].Color = new Vector3((float)r, (float)g, (float)b);
            }
            _drawingArea.QueueDraw();
        }

        private void DrawingAreaOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (motion)
            {
                if (_mouse.ActiveText == "Перемещение")
                {
                    _shiftX.Value += ((float) args.Event.X - Oldx) / 10;
                    _shiftY.Value += ((float) args.Event.Y - Oldy) / 10;
                    _drawingArea.QueueDraw();

                    Oldx = (float) args.Event.X;
                    Oldy = (float) args.Event.Y;
                }

                if (_mouse.ActiveText == "Вращение")
                {
                    _rotationY.Value += ((float) args.Event.X - Oldx) / 10;
                    _rotationX.Value += -((float) args.Event.Y - Oldy) / 10;
                    
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
        private void AllOnToggled(object? sender, EventArgs e)
        {
             _drawingArea.QueueDraw();
        }

        private void NormalOnToggled(object? sender, EventArgs e)
        {
            _drawingArea.QueueDraw();
        }

        private void RotationXOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void RotationYOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void RotationZOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }

        private void ScaleXOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void ScaleYOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        private void ScaleZOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void ShiftXOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        } 
        
        private void ShiftYOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }
        
        private void ShiftZOnValueChanged(object? sender, EventArgs e)
        {
            CalculateMatrix();
        }

        private void DrawingAreaOnSizeAllocated(object o, SizeAllocatedArgs args)
        {

            float maxX = -1;
            float maxY = -1;

            for (int i = 0; i < Verticies.Count; ++i)
            {
                if (Verticies[i].PointInWorldSpace.X > maxX)
                {
                    maxX = Verticies[i].PointInWorldSpace.X;
                }
                
                if (Verticies[i].PointInWorldSpace.Y > maxY)
                {
                    maxY = Verticies[i].PointInWorldSpace.Y;
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
                Verticies[i].PointInWorldSpace = Vector4.Transform(Verticies[i].PointInLocalSpace, WorldMatrix);
            }
        }

        Vector4 View(Vector4 a)
        {
            Vector4 ans = Vector4.Transform(a, ViewMatrix);
            return ans;
        }

        bool Hide(Polygon a)
        {
            Vector3 vector1 =  DifferenceVector3(a.points[0].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            Vector3 vector2 =  DifferenceVector3(a.points[2].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            

            Vector3 vector3 = new Vector3(vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);

            vector3 = Normalize(vector3);

            Vector2 start = new Vector2((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X + a.points[3].PointInWorldSpace.X) / 4, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y + a.points[3].PointInWorldSpace.Y) / 4 );

            vector3.X = vector3.X + start.X;
            vector3.Y = vector3.Y + start.Y;
            
            
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
                vector1.X = (float) (_scaleX.Value * 15 * (vector1.X / len));
                vector1.Y = (float) (_scaleY.Value * 15 * (vector1.Y / len));
                vector1.Z = (float) (_scaleZ.Value * 15 * (vector1.Z / len));
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
            Vector3 vector1 =  DifferenceVector3(a.points[0].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            Vector3 vector2 =  DifferenceVector3(a.points[2].PointInWorldSpace,a.points[1].PointInWorldSpace) ;
            

            Vector3 vector3 = new Vector3(vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);

            vector3 = Normalize(vector3);

            Vector2 start = new Vector2((a.points[0].PointInWorldSpace.X + a.points[1].PointInWorldSpace.X + a.points[2].PointInWorldSpace.X + a.points[3].PointInWorldSpace.X) / 4, (a.points[0].PointInWorldSpace.Y + a.points[1].PointInWorldSpace.Y + a.points[2].PointInWorldSpace.Y + a.points[3].PointInWorldSpace.Y) / 4 );

            vector3.X = vector3.X + start.X;
            vector3.Y = vector3.Y + start.Y;
            
            start = Vector2.Transform(start, ViewMatrix);
            vector3 = Vector3.Transform(vector3, ViewMatrix);
            
            cr.SetSourceRGB(.0, 0, 1);
            cr.MoveTo(start.X, start.Y);
            cr.LineTo(vector3.X, vector3.Y);
            cr.Stroke();
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

                if (!_all.Active && Hide(Polygons[i])) 
                {
                    continue;
                }

                Vector4 picture = View(Polygons[i].points[0].PointInWorldSpace); 
                cr.MoveTo(picture.X, picture.Y);
                for (int j = 1; j < Polygons[i].points.Count; ++j)
                {
                    picture =  View(Polygons[i].points[j].PointInWorldSpace); 
                    cr.LineTo(picture.X, picture.Y);
                }
                
                picture = View(Polygons[i].points[0].PointInWorldSpace);
                cr.LineTo(picture.X, picture.Y);
                
                if (_paint.Active)
                {
                    cr.SetSourceRGB(Polygons[i].Color.X, Polygons[i].Color.Y, Polygons[i].Color.Z);
                    cr.FillPreserve();
                }

                if (_edges.Active)
                {
                    cr.SetSourceRGB(.0, .0, .0);
                    cr.Stroke(); 
                }
                else
                {
                    if (!_paint.Active)
                    {
                        cr.SetSourceRGB(0.9, 0.9, 0.9);
                    }
                    cr.Stroke();  
                }
                
                 if (_normal.Active)
                {
                    DrawNormals(cr, Polygons[i]);
                    cr.SetSourceRGB(.0, .0, .0);
                }
            }
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}