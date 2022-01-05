using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Numerics;
using Cairo;
using Gdk;
using SharpGL;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Window = Gtk.Window;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using CGPlatform;

namespace CP
{
    class MainWindow : Window
    {
        [UI] private GLArea _drawingArea = null;
        
        [UI] private Adjustment _shiftX = null;
        [UI] private Adjustment _shiftY = null;
        [UI] private Adjustment _shiftZ = null;
        [UI] private Adjustment _scaleX = null;
        [UI] private Adjustment _scaleY = null;
        [UI] private Adjustment _scaleZ = null;
        [UI] private Adjustment _rotationX = null;
        [UI] private Adjustment _rotationY = null;
        [UI] private Adjustment _rotationZ = null;
        
        [UI] private CheckButton _lines = null;
        [UI] private CheckButton _fill = null;
        [UI] private CheckButton _light = null;
        [UI] private CheckButton _showLight = null;
        [UI] private CheckButton _animation = null;
        [UI] private CheckButton _points = null;
        [UI] private CheckButton _norm = null;
        [UI] private CheckButton _genp = null;
        
        [UI] private Adjustment _u = null;
        [UI] private Adjustment _v = null;
        [UI] private Adjustment _cposX = null;
        [UI] private Adjustment _cposY = null;
        [UI] private Adjustment _cposZ = null;
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
        [UI] private Adjustment _k = null;
        [UI] private Adjustment _beta1 = null;
        [UI] private Adjustment _beta2 = null;
        [UI] private Adjustment _np = null;
        [UI] private Adjustment _fp = null;
        [UI] private Adjustment _pointx = null;
        [UI] private Adjustment _pointy = null;
        [UI] private Adjustment _pointz = null;
        
        [UI] private ComboBoxText _mouse = null;

        [UI] private Button _addr = null;
        [UI] private Button _addl = null;
        [UI] private Button _adddown = null;
        [UI] private Button _addup = null;
        
        [UI] private Button _remr = null;
        [UI] private Button _reml = null;
        [UI] private Button _remdown = null;
        [UI] private Button _remup = null;
        
        [UI] private FileChooserButton _load = null;
        
        class Vertex
        {
            public Vector3 Point;
            public Vector3 Color;
            public uint Id = 0;
            public Vector3 NormalInWorldSpace;
            
            public List<Polygon> polygons= new List<Polygon>();
            public Vertex(float x, float y, float z)
            {
                Point = new Vector3(x, y, z);
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


        class Camera
        {
            public Vector3 Position;
            public Vector3 Target;
            public Vector3 Up;

            public Camera(Vector3 position, Vector3 target, Vector3 u)
            {
                Position = position;
                Target = target;
                Up = u;
            }
        }

        List<Vertex> dr = new List<Vertex>();

        private List<Polygon> Polygons = new List<Polygon>();
        private List<List<Vertex>> Verticies = new List<List<Vertex>>();
        static OpenGL gl = new SharpGL.OpenGL();
        private float[] masver;
        private uint[] masid;
        private uint VCount = 0;
        private Camera camera = new Camera(new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        private bool motion = false;
        private float Oldx = -1;
        private float Oldy = -1;
        private float dX = 0;
        private float dY = 0;
        private double width = 0;
        private double height = 0;
        private float startTime;
        private int angle = 120;
        private int pointid1 = -1;
        private int pointid2 = -1;

        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
        }

        public static string ReadFromRes(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            var c = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))

            using (StreamReader reader = new StreamReader(stream)) 
            {
                return reader.ReadToEnd();
            }
        }
         Vector3 DifferenceVector3(Vector3 a, Vector3 b)
        {
            Vector3 ans = new Vector3();
            ans.X = a.X - b.X;
            ans.Y = a.Y - b.Y;
            ans.Z = a.Z - b.Z;
            return ans;
        }
        void Normals(Polygon a)
        {
            Vector3 vector1 =  DifferenceVector3(a.points[0].Point,a.points[1].Point) ;
            Vector3 vector2 =  DifferenceVector3(a.points[2].Point,a.points[1].Point) ;
            

            Vector3 vector3 = new Vector3(vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);

            vector3 = Vector3.Normalize(vector3);
            
            a.NormalInWorldSpace = vector3;
        }

        float coeffB(int i, float u, float delta)
        {
            float b_i = 0;
            i = i % 4;
            if (i == 0)
            {
                b_i = 2 * (float) _beta1.Value * (float) _beta1.Value * (float) _beta1.Value * (1 - u) *
                    (1 - u) * (1 - u) / delta;
            }

            if (i == 1)
            {
                b_i = (2 * (float) _beta1.Value * (float) _beta1.Value * (float) _beta1.Value * u * (u * u - 3 * u + 3) + 
                       2 * (float) _beta1.Value * (float) _beta1.Value * (u * u * u - 3 * u * u + 2) + 
                       2 * (float) _beta1.Value * (float) _beta1.Value * (u * u * u - 3 * u * u + 2) + 
                       (float) _beta2.Value * (2 * u * u * u - 3 * u * u + 1)) / delta;
            }

            if (i == 2)
            {
                b_i = (2 * (float) _beta1.Value * (float) _beta1.Value * u * u * (-u + 3) +
                       2 * (float) _beta1.Value * u * (-u * u + 3) +
                       (float) _beta2.Value * u * u * (-2 * u + 3) +
                       2 * (-u * u * u + 1)) / delta;
            }

            if (i == 3)
            {
                b_i = 2 * u * u * u / delta;
            }

            return b_i;
        }

        void Elementary_beta_spline(float du, float dv, float delta, int k1, int k2, int p)
        {
            float u = 0;
            float v = 0;
            
            for (int t1 = 0; t1 <= (int)_u.Value; ++t1)
            {
                for (int t2 = 0; t2 <= (int)_v.Value; ++t2)
                {
                    Vector3 res = new Vector3(0, 0, 0);
                    for (int i = k1; i < k1 + 4; ++i)
                    {
                        float b_i = coeffB(i - k1, u, delta);
                        for (int j = k2; j < k2 + 4; ++j)
                        {
                            float b_j = coeffB(j - k2, v, delta);
                            
                            res.X += b_i * b_j * Verticies[i][j].Point.X;
                            res.Y += b_i * b_j * Verticies[i][j].Point.Y;
                            res.Z += b_i * b_j * Verticies[i][j].Point.Z;
                        }
                    }
                    dr.Add(new Vertex(res.X, res.Y, res.Z));

                    dr[dr.Count - 1].Id = (uint) dr.Count - 1;
                    
                     v += dv;
                }

                v = 0;
                u += du;
            }

            p = p * ((int)_u.Value + 1) * ((int)_v.Value + 1);
            
            for (int i = 0; i < (int) _u.Value; ++i)
            {
                for (int j = 0; j < (int) _v.Value; ++j)
                {
                    Polygons.Add(new Polygon());
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j + p]);
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j + 1 + p]);
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j + (int)_v.Value + 1 + p]);
                    
                    dr[i * ((int)_v.Value + 1) + j + p].polygons.Add(Polygons[Polygons.Count - 1]);
                    dr[i * ((int)_v.Value + 1) + j + 1 + p].polygons.Add(Polygons[Polygons.Count - 1]);
                    dr[i * ((int)_v.Value + 1) + j + ((int)_v.Value + 1) + p].polygons.Add(Polygons[Polygons.Count - 1]);
                    
                    Polygons.Add(new Polygon());
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j + 1 + p]);
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j + 1 + (int)_v.Value + 1 + p]);
                    Polygons[Polygons.Count - 1].points.Add(dr[i * ((int)_v.Value + 1) + j +  (int)_v.Value + 1 + p]);
               
                    dr[i * ((int)_v.Value + 1) + j + 1 + p].polygons.Add(Polygons[Polygons.Count - 1]);
                    dr[i * ((int)_v.Value + 1) + 1 + j +((int)_v.Value + 1) + p].polygons.Add(Polygons[Polygons.Count - 1]);
                    dr[i * ((int)_v.Value + 1) + j + ((int)_v.Value + 1) + p].polygons.Add(Polygons[Polygons.Count - 1]);
                }
            }
        }


        void Figure()
        {
            dr = new List<Vertex>();
            
            float du = (float) (1f / _u.Value);
            float dv = (float) (1f / _v.Value);
            
            float delta = 2 * (float) _beta1.Value * (float) _beta1.Value * (float) _beta1.Value +
                          4 * (float) _beta1.Value * (float) _beta1.Value + 4 * (float) _beta1.Value +
                          (float) _beta2.Value + 2;

            
            Polygons = new List<Polygon>();

            int p = 0;
            for (int i = 0; i < Verticies.Count - 3; ++i)
            {
                for (int j = 0; j < Verticies[i].Count - 3; ++j)
                { Elementary_beta_spline(du, dv, delta, i, j, p);
                    p += 1;
                }
            }
            
            
            foreach (var pol in Polygons)
            { 
                Normals(pol);
            }

            for (int i = 0; i < dr.Count; ++i)
            {
                Vector3 n = new Vector3(0, 0, 0);
                for (int l = 0; l < dr[i].polygons.Count; ++l)
                {
                    n += dr[i].polygons[l].NormalInWorldSpace;
                }

                n.X = n.X / dr[i].polygons.Count;
                n.Y = n.Y / dr[i].polygons.Count;
                n.Z = n.Z / dr[i].polygons.Count;
                n = Vector3.Normalize(n);
                dr[i].NormalInWorldSpace = n;
            }

            masver = new float[dr.Count * 6];
            int k = 0;
            for (int i = 0; i < dr.Count; ++i)
            {
                masver[k] = dr[i].Point.X;
                k += 1;
                masver[k] = dr[i].Point.Y;
                k += 1;
                masver[k] = dr[i].Point.Z;
                k += 1;
            }

            for (int i = 0; i < dr.Count; ++i)
            {
                masver[k] = dr[i].NormalInWorldSpace.X;
                k += 1;
                masver[k] = dr[i].NormalInWorldSpace.Y;
                k += 1;
                masver[k] = dr[i].NormalInWorldSpace.Z;
                k += 1;
            }
            
            masid = new uint[Polygons.Count * 3];
            k = 0;

            foreach (var pol in Polygons)
            {
                for (int i = 0; i < pol.points.Count; ++i)
                {
                    masid[k] = pol.points[i].Id;
                    k += 1;
                }
            }
        }


        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            width = _drawingArea.Allocation.Width;
            height = _drawingArea.Allocation.Height;
            _drawingArea.Realized += DrawingAreaOnRealized; 
            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
            _drawingArea.Resize += DrawingAreaResizeEvent;
            _load.SetCurrentFolder("./");
            
            _shiftX.ValueChanged += ValueChanged;
            _shiftY.ValueChanged += ValueChanged;
            _shiftZ.ValueChanged += ValueChanged;

            _scaleX.ValueChanged += ValueChanged;
            _scaleY.ValueChanged += ValueChanged;
            _scaleZ.ValueChanged += ValueChanged;
            _rotationX.ValueChanged += ValueChanged;
            _rotationY.ValueChanged += ValueChanged;
            _rotationZ.ValueChanged += ValueChanged;

            _lines.Toggled += ValueChanged;
            _fill.Toggled += ValueChanged;
            _light.Toggled += ValueChanged;
            
            _drawingArea.MotionNotifyEvent += DrawingAreaOnMotionNotifyEvent;
            _drawingArea.ButtonReleaseEvent += DrawingAreaOnButtonReleaseEvent;
            _drawingArea.ButtonPressEvent += DrawingAreaOnButtonPressEvent;
            _u.ValueChanged += ValueChanged2;
            _v.ValueChanged += ValueChanged2;
            _beta1.ValueChanged += ValueChanged2;
            _beta2.ValueChanged += ValueChanged2;

            _r.ValueChanged += ValueChanged;
            _g.ValueChanged += ValueChanged;
            _bl.ValueChanged += ValueChanged;

            _cposX.ValueChanged += CposXValueChanged;
            _cposY.ValueChanged += CposYValueChanged;
            _cposZ.ValueChanged += CposZValueChanged;
            
            _kag.ValueChanged += ValueChanged;
            _kar.ValueChanged += ValueChanged;
            _kab.ValueChanged += ValueChanged;
            _kdr.ValueChanged += ValueChanged;
            _kdg.ValueChanged += ValueChanged;
            _kdb.ValueChanged += ValueChanged;
            _ksr.ValueChanged += ValueChanged;
            _ksg.ValueChanged += ValueChanged;
            _ksb.ValueChanged += ValueChanged;
            
            _iab.ValueChanged += ValueChanged;
            _iag.ValueChanged += ValueChanged;
            _iar.ValueChanged += ValueChanged;
            _ilb.ValueChanged += ValueChanged;
            _ilg.ValueChanged += ValueChanged;
            _ilr.ValueChanged += ValueChanged;
            
            _lx.ValueChanged += ValueChanged;
            _ly.ValueChanged += ValueChanged;
            _lz.ValueChanged += ValueChanged;

            _p.ValueChanged += ValueChanged;
            _k.ValueChanged += ValueChanged;
            _showLight.Toggled += ValueChanged;
            _animation.Toggled += ValueChanged;
            _np.ValueChanged += ValueChanged;
            _fp.ValueChanged += ValueChanged;

            _pointx.ValueChanged +=  MotionPointX;
            _pointy.ValueChanged +=  MotionPointY;
            _pointz.ValueChanged +=  MotionPointZ;
            
            _addr.Clicked += delegate(object? sender, EventArgs args)
            {
                for (int i = 0; i < Verticies.Count; ++i)
                {
                    Verticies[i].Add(new Vertex(Verticies[i][Verticies[i].Count - 1].Point.X + 1, Verticies[i][Verticies[i].Count - 1].Point.Y + 1, Verticies[i][Verticies[i].Count - 1].Point.Z + 1));
                }

                VCount += (uint)Verticies.Count;
                Figure();
                _drawingArea.QueueRender();
            };
            
            _addl.Clicked += delegate(object? sender, EventArgs args)
            {
                for (int i = 0; i < Verticies.Count; ++i)
                {
                    Verticies[i].Insert(0, new Vertex(Verticies[i][0].Point.X - 1, Verticies[i][0].Point.Y - 1, Verticies[i][0].Point.Z - 1));
                }

                VCount += (uint)Verticies.Count;
                Figure();
                _drawingArea.QueueRender();
            };
            
            _adddown.Clicked += delegate(object? sender, EventArgs args)
            {
                List<Vertex> a = new List<Vertex>();

                for (int i = 0; i < Verticies[Verticies.Count - 1].Count; ++i)
                {
                    a.Add(new Vertex(Verticies[Verticies.Count - 1][i].Point.X - 1, Verticies[Verticies.Count - 1][i].Point.Y - 1, Verticies[Verticies.Count - 1][i].Point.Z - 1));
                }
                
                Verticies.Add(a);
                VCount += (uint) Verticies[Verticies.Count - 1].Count;
                Figure();
                _drawingArea.QueueRender();

            };
            
            _addup.Clicked += delegate(object? sender, EventArgs args)
            {
                List<Vertex> a = new List<Vertex>();
                for (int i = 0; i < Verticies[0].Count; ++i)
                {
                    a.Add( new Vertex(Verticies[0][i].Point.X + 1, Verticies[0][i].Point.Y + 1, Verticies[0][i].Point.Z + 1));
                }
                
                Verticies.Insert(0, a);
                VCount += (uint) Verticies[0].Count;
                Figure();
                _drawingArea.QueueRender();
                
            };
            
            _remr.Clicked += delegate(object? sender, EventArgs args)
            {

                if (Verticies[0].Count > 4)
                {
                    for (int i = 0; i < Verticies.Count; ++i)
                    {
                        Verticies[i].RemoveAt(Verticies[i].Count - 1);
                    }

                    VCount -= (uint) Verticies.Count;
                    Figure();
                    _drawingArea.QueueRender();
                }
            };
            
            _reml.Clicked += delegate(object? sender, EventArgs args)
            {
                if (Verticies[0].Count > 4)
                {
                    for (int i = 0; i < Verticies.Count; ++i)
                    {
                        Verticies[i].RemoveAt(0);
                    }

                    VCount -= (uint) Verticies.Count;
                    Figure();
                    _drawingArea.QueueRender();
                }
            };
            
            _remdown.Clicked += delegate(object? sender, EventArgs args)
            {
                if (Verticies.Count > 4)
                {
                    Verticies.RemoveAt(Verticies.Count - 1);
                    VCount -= (uint) Verticies[Verticies.Count - 1].Count;
                    Figure();
                    _drawingArea.QueueRender();
                }
            };
            
            _remup.Clicked += delegate(object? sender, EventArgs args)
            {
                if (Verticies.Count > 4)
                {
                    Verticies.RemoveAt(0);
                    VCount -= (uint) Verticies[0].Count;
                    Figure();
                    _drawingArea.QueueRender();
                }
            };

            _drawingArea.ScrollEvent += (o, args) =>
           {
               if (args.Event.Direction == ScrollDirection.Down)
               {
                   // _scaleX.Value -= 0.1;
                   // _scaleY.Value -= 0.1;
                   // _scaleZ.Value -= 0.1;
                   angle += 1;
                   angle = Math.Min(180, angle);

               }
               else if (args.Event.Direction == ScrollDirection.Up)
               {
                   // _scaleX.Value += 0.1;
                   // _scaleY.Value += 0.1;
                   // _scaleZ.Value += 0.1;
                   angle -= 1;
                   angle = Math.Max(0, angle);
               }
           };
            
            _load.FileSet += LoadFile;
            _load.FileActivated += LoadFile;
            
            _mouse.Append("rotation", "Вращение камерой");
            _mouse.Append("shift", "Перемещение точек");
            _mouse.Active = 0;

            DeleteEvent += Window_DeleteEvent;
        }

        private void  MotionPointX(object? sender, EventArgs e)
        {
            if (pointid1 != -1 && pointid2 != -1)
            {
                Verticies[pointid1][pointid2].Point.X = (float)_pointx.Value;
                Figure();
                _drawingArea.QueueRender();
            }
        }
        
        private void  MotionPointY(object? sender, EventArgs e)
        {
            if (pointid1 != -1 && pointid2 != -1)
            {
                Verticies[pointid1][pointid2].Point.Y = (float)_pointy.Value;
                Figure();
                _drawingArea.QueueRender();
            }
        }
        
        private void  MotionPointZ(object? sender, EventArgs e)
        {
            if (pointid1 != -1 && pointid2 != -1)
            {
                Verticies[pointid1][pointid2].Point.Z = (float)_pointz.Value;
                Figure();
                _drawingArea.QueueRender();
            }
        }

        private void LoadFile(object? sender, EventArgs e)
        {
            pointid1 = -1;
            pointid2 = -1;
            StreamReader file = new StreamReader(_load.Filename);
            string s = "";
            VCount = 0;
            int k = 0;
            float u = 0, v = 0, beta1 = 0, beta2 = 0;
            Verticies = new List<List<Vertex>>();
            while ((s = file.ReadLine()) != null)
            {
                string[] num = s.Split(" ");
                if (k == 0)
                {
                    beta1 = (float)Convert.ToDouble(num[0], CultureInfo.InvariantCulture);
                    beta2 = (float)Convert.ToDouble(num[1], CultureInfo.InvariantCulture);
                    k += 1;
                    continue;
                }

                if (k == 1)
                {
                    u = (float)Convert.ToDouble(num[0], CultureInfo.InvariantCulture);
                    v = (float)Convert.ToDouble(num[1], CultureInfo.InvariantCulture);
                    k += 1;
                    continue;
                }

                Verticies.Add(new List<Vertex>());
                for (int i = 0; i < num.Length; i += 3)
                {
                    float x = (float)Convert.ToDouble(num[i], CultureInfo.InvariantCulture);
                    float y = (float)Convert.ToDouble(num[i + 1], CultureInfo.InvariantCulture);
                    float z = (float)Convert.ToDouble(num[i + 2], CultureInfo.InvariantCulture);
                    Verticies[Verticies.Count - 1].Add(new Vertex(x, y, z));
                    VCount += 1;
                }
            }
            _u.Value = u;
            _v.Value = v;
            _beta1.Value = beta1;
            _beta2.Value = beta2;
            Figure();
            _drawingArea.QueueRender();
        }
        
        private Matrix4f Perspective(double verticalAngle, double aspectRatio, double nearPlane, double farPlane) {
            var radians = (verticalAngle / 2) * Math.PI / 180;
            var sine = Math.Sin(radians);

            var cotan = Math.Cos(radians) / sine;
            var clip = farPlane - nearPlane;
            Matrix4f t1 = new Matrix4f
            (
                (float)cotan / (float)aspectRatio, 0, 0, 0,
                0, (float)cotan, 0, 0,
                0, 0, -((float)nearPlane + (float)farPlane) / (float)clip, -(float)(2.0 * (float)nearPlane * (float)farPlane) / (float)clip,
                0, 0, (float)-1.0, (float)1.0
            );
            return t1;
        }

        private Matrix4f Model() {
           var model = Matrix4f.Identity;
            model[1, 1] = (float)_scaleX.Value;;
            model[2, 2] = (float)_scaleY.Value;
            model[3, 3] = (float)_scaleZ.Value;
            model[4, 4] = 1;

            var rx = Matrix4f.Identity;
            rx[1, 1] = 1;
            rx[2, 2] = (float)Math.Cos((float)(_rotationX.Value * Math.PI / 180));
            rx[3, 3] = (float)Math.Cos((float)(_rotationX.Value * Math.PI / 180));
            rx[2, 3] = -(float)Math.Sin((float)(_rotationX.Value * Math.PI / 180));
            rx[3, 2] = (float)Math.Sin((float)(_rotationX.Value * Math.PI / 180));
            rx[4, 4] = 1;
                
            var ry = Matrix4f.Identity;
            ry[2, 2] = 1;
            ry[1, 1] = (float)Math.Cos((float)(_rotationY.Value * Math.PI / 180));
            ry[3, 3] = (float)Math.Cos((float)(_rotationY.Value * Math.PI / 180));
            ry[1, 3] = (float)Math.Sin((float)(_rotationY.Value * Math.PI / 180));
            ry[3, 1] = -(float)Math.Sin((float)(_rotationY.Value * Math.PI / 180));
            ry[4, 4] = 1;
                
            var rz = Matrix4f.Identity;

            rz[3, 3] = 1;
            rz[4, 4] = 1;
            rz[1, 1] = (float)Math.Cos((float)(_rotationZ.Value * Math.PI / 180));
            rz[2, 2] = (float)Math.Cos((float)(_rotationZ.Value * Math.PI / 180));
            rz[1, 2] = -(float)Math.Sin((float)(_rotationZ.Value * Math.PI / 180));
            rz[2, 1] = (float)Math.Sin((float)(_rotationZ.Value * Math.PI / 180));

            model = rx * ry * rz * model;
                
            model[4, 1] = -(float)_shiftX.Value;
            model[4, 2] = (float)_shiftY.Value;
            model[4, 3] = (float) _shiftZ.Value;

            return model;
        }

        private Matrix4f View()
        {
            var view = Matrix4f.Identity;
            Vector3 cameraDirection = (camera.Position - camera.Target);
            cameraDirection = Vector3.Normalize(cameraDirection);
            Vector3 cameraRight = Vector3.Cross(camera.Up, cameraDirection);
            cameraRight = Vector3.Normalize(cameraRight);

            view[1, 1] = cameraRight.X;
            view[1, 2] = cameraRight.Y;
            view[1, 3] = cameraRight.Z;

            view[2, 1] = camera.Up.X;
            view[2, 2] = camera.Up.Y;
            view[2, 3] = camera.Up.Z;

            view[3, 1] = cameraDirection.X;
            view[3, 2] = cameraDirection.Y;
            view[3, 3] = cameraDirection.Z;
            view[4, 4] = 1;

            var view2 = Matrix4f.Identity;
            view2[1, 4] = -camera.Position.X;
            view2[2, 4] = -camera.Position.Y;
            view2[3, 4] = -camera.Position.Z;

            view = view * view2;
            return view;
        }

        private void CposXValueChanged(object? sender, EventArgs e)
        {
            camera.Position.X = (float)_cposX.Value;
            _drawingArea.QueueRender();
        }
        
        private void CposYValueChanged(object? sender, EventArgs e)
        {
            camera.Position.Y = (float)_cposY.Value;
            _drawingArea.QueueRender();
        }
        
        private void CposZValueChanged(object? sender, EventArgs e)
        {
            camera.Position.Z = (float)_cposZ.Value;
            _drawingArea.QueueRender();
        }
        private void DrawingAreaOnRealized(object? sender, EventArgs e)
        {
            _drawingArea.MakeCurrent();
            
            var frame_clock = _drawingArea.Context.Window.FrameClock;
            frame_clock.Update += (_, _) => _drawingArea.QueueRender();
            frame_clock.BeginUpdating();
            
            uint vertexShader;

            vertexShader = gl.CreateShader(OpenGL.GL_VERTEX_SHADER);
            string s = ReadFromRes("CP.VertexShader.glsl");
            gl.ShaderSource(vertexShader, s);
            gl.CompileShader(vertexShader);
                
            System.Text.StringBuilder txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(vertexShader, 512, (IntPtr)0, txt);
            //Console.WriteLine(txt);

            var glsl_tmp = new int[1];
            gl.GetShader(vertexShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
                
            uint fragmentShader;
            fragmentShader = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            s = ReadFromRes("CP.FragmentShader.glsl");
            gl.ShaderSource(fragmentShader, s);
            gl.CompileShader(fragmentShader);
                
            txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(fragmentShader, 512, (IntPtr)0, txt);
            // Console.WriteLine(txt);
            gl.GetShader(fragmentShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
                 
            
            uint fragmentLight;
            fragmentLight = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            s = ReadFromRes("CP.FragmentLight.glsl");
            gl.ShaderSource(fragmentLight, s);
            gl.CompileShader(fragmentLight);
                
            txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(fragmentLight, 512, (IntPtr)0, txt);
            //Console.WriteLine(txt);
            gl.GetShader(fragmentLight, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
            
            uint shaderProgram;
            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);

            gl.GetProgram(shaderProgram, OpenGL.GL_LINK_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader program link failed");

            uint lightProgram;
            lightProgram = gl.CreateProgram();
            gl.AttachShader(lightProgram, vertexShader);
            gl.AttachShader(lightProgram, fragmentLight);
            gl.LinkProgram(lightProgram);

            gl.GetProgram(lightProgram, OpenGL.GL_LINK_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader program link failed");
            
            
            Verticies.Add(new List<Vertex> {
                new(-1, 0, -1), new((float)-2 , 0, -1), new((float)2, 0, -1), new(1, 0, -1)
            });
            
            Verticies.Add(new List<Vertex> {
               new(-1, 0, (float)-2), new((float)-2, 0, (float)-2), new((float)2, 0, (float)-2), new(1, 0, (float)-2)
            });
            
            Verticies.Add(new List<Vertex> {
                new(-1, 0, (float)2), new((float)-2, 0, (float)2), new((float)2, 0, (float)2), new(1, 0, (float)2)
            });
            
            Verticies.Add(new List<Vertex> {
                 new(-1, 0, 1), new((float)-2, 0, 1), new((float)2 , 0, 1), new(1, 0, 1)
            });

            VCount = 16;
            

            Figure();

            uint[] VAO = new uint[8];
            uint[] VBO = new uint[9];
            gl.GenVertexArrays(8, VAO);
            gl.GenBuffers(9, VBO);
            _drawingArea.Render += (o, args) =>
            {
                
                if (!_animation.Active)
                {
                    startTime = (float)frame_clock.FrameTime;
                }
                
                gl.BindVertexArray(VAO[0]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[0]);
                gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, VBO[1]);
                    
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, masver, OpenGL.GL_DYNAMIC_DRAW);
                gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, masid, OpenGL.GL_DYNAMIC_DRAW);

                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false,0, (IntPtr)0);
                gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,0, (IntPtr)(sizeof(float) * dr.Count * 3));

                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
                
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0); 
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[1]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[2]);

                float[] l = new float[3];
                l[0] = (float) _lx.Value;
                l[1] = (float) _ly.Value;
                l[2] = (float) _lz.Value;
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, l, OpenGL.GL_DYNAMIC_DRAW);
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.EnableVertexAttribArray(0);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[2]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[3]);
                gl.EnableVertexAttribArray(0);
                float[] pts = new float[VCount * 3];
                int t = 0;
                for (int i = 0; i < Verticies.Count; ++i)
                {
                    for (int j = 0; j < Verticies[i].Count; ++j)
                    {
                        pts[t] = Verticies[i][j].Point.X;
                        t += 1;
                        pts[t] = Verticies[i][j].Point.Y;
                        t += 1;
                        pts[t] = Verticies[i][j].Point.Z;
                        t += 1;
                    }
                }
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, pts , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[3]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[4]);
                gl.EnableVertexAttribArray(0);
                float[] pt = new float[3];

                if (pointid1 != -1 && pointid2 != -1)
                {
                    pt[0] = Verticies[pointid1][pointid2].Point.X;
                    pt[1] = Verticies[pointid1][pointid2].Point.Y;
                    pt[2] = Verticies[pointid1][pointid2].Point.Z;
                }
                
               
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER,pt , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[4]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[5]);
                gl.EnableVertexAttribArray(0);
                float[] gen = new float[dr.Count * 3];
                t = 0;
                for (int i = 0; i < dr.Count; ++i)
                {
                        gen[t] = dr[i].Point.X;
                        t += 1;
                        gen[t] = dr[i].Point.Y;
                        t += 1;
                        gen[t] = dr[i].Point.Z;
                        t += 1;
                }
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, gen , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[5]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[6]);
                gl.EnableVertexAttribArray(0);
                float[] x = new float[]
                {
                    0, 0, 0,
                    1, 0, 0
                };
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, x , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[6]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[7]);
                gl.EnableVertexAttribArray(0);
                float[] y = new float[]
                {
                    0, 0, 0,
                    0, 1, 0
                };
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, y , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                gl.BindVertexArray(VAO[7]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[8]);
                gl.EnableVertexAttribArray(0);
                float[] z = new float[]
                {
                    0, 0, 0,
                    0, 0, 1
                };
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, IntPtr.Zero);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, z , OpenGL.GL_DYNAMIC_DRAW);
                gl.BindVertexArray(0);
                
                
                gl.FrontFace(OpenGL.GL_CW);

                gl.Enable(OpenGL.GL_DEPTH_TEST);
                gl.DepthFunc(OpenGL.GL_LESS);

                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                //gl.Enable(OpenGL.GL_CULL_FACE);

                gl.Enable(OpenGL.GL_LINE_SMOOTH);
                gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
                    
                gl.ClearColor(0.8f, 0.8f, 0.8f, 1);
                
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                
                var proj = Perspective(angle, (double)width / height, _np.Value, _fp.Value);

                int loc = gl.GetUniformLocation(shaderProgram, "proj4f");
                gl.UniformMatrix4(loc, 1, false, proj.ToArray());
                loc = gl.GetUniformLocation(shaderProgram, "axies");
                gl.Uniform1(loc, 0);
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "proj4f");
                gl.UniformMatrix4(loc, 1, false, proj.ToArray());
                loc = gl.GetUniformLocation(lightProgram, "axies");
                gl.Uniform1(loc, 0);
                
                gl.UseProgram(shaderProgram);

                var model = Model();
                
                gl.UseProgram(shaderProgram);
                loc = gl.GetUniformLocation(shaderProgram, "model4f");
                gl.UniformMatrix4(loc, 1, false, model.ToArray());
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "model4f");
                gl.UniformMatrix4(loc, 1, false, model.ToArray());

                var view = View();

                gl.UseProgram(shaderProgram);
                loc = gl.GetUniformLocation(shaderProgram, "view4f");
                gl.UniformMatrix4(loc, 1, true, view.ToArray());
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "view4f");
                gl.UniformMatrix4(loc, 1, true, view.ToArray());


                if (_animation.Active)
                {
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "animation");
                    gl.Uniform1(loc, 1);
                    loc = gl.GetUniformLocation(shaderProgram, "t");
                    gl.Uniform1(loc, (float)((frame_clock.FrameTime - startTime)/1000000));
                    
                    gl.UseProgram(lightProgram);
                    loc = gl.GetUniformLocation(lightProgram, "animation");
                    gl.Uniform1(loc, 1);
                    loc = gl.GetUniformLocation(lightProgram, "t");
                    gl.Uniform1(loc, (float)((frame_clock.FrameTime - startTime)/1000000));
                }
                else
                {
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "animation");
                    gl.Uniform1(loc, 0);
                    
                    gl.UseProgram(lightProgram);
                    loc = gl.GetUniformLocation(lightProgram, "animation");
                    gl.Uniform1(loc, 0);
                }
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "m.ka");
                gl.Uniform3(loc, (float) _kar.Value, (float) _kag.Value, (float) _kab.Value);
                loc = gl.GetUniformLocation(lightProgram, "m.kd");
                gl.Uniform3(loc, (float) _kdr.Value, (float) _kdg.Value, (float) _kdb.Value);
                loc = gl.GetUniformLocation(lightProgram, "m.ks");
                gl.Uniform3(loc, (float) _ksr.Value, (float) _ksg.Value, (float) _ksb.Value);
                loc = gl.GetUniformLocation(lightProgram, "m.p");
                gl.Uniform1(loc, (float)_p.Value);
                
                loc = gl.GetUniformLocation(lightProgram, "camera");
                gl.Uniform3(loc, (float)camera.Position.X, (float) camera.Position.Y, (float) camera.Position.Y);
                
                loc = gl.GetUniformLocation(lightProgram, "l.ia");
                gl.Uniform3(loc, (float) _iar.Value, (float) _iag.Value, (float) _iab.Value);
                loc = gl.GetUniformLocation(lightProgram, "l.il");
                gl.Uniform3(loc, (float) _ilr.Value, (float) _ilg.Value, (float) _ilb.Value);
                loc = gl.GetUniformLocation(lightProgram, "l.position");
                gl.Uniform3(loc, (float) _lx.Value, (float) _ly.Value, (float) _lz.Value);

                loc = gl.GetUniformLocation(lightProgram, "c");
                gl.Uniform3(loc, (float)_r.Value, (float)_g.Value, (float)_bl.Value);
                
                loc = gl.GetUniformLocation(lightProgram, "k");
                gl.Uniform1(loc, (float) _k.Value);

                loc = gl.GetUniformLocation(lightProgram, "width");
                gl.Uniform1(loc, (float) width);
                
                loc = gl.GetUniformLocation(lightProgram, "height");
                gl.Uniform1(loc, (float) height);


                gl.UseProgram(shaderProgram);
                
                 loc = gl.GetUniformLocation(shaderProgram, "width");
                gl.Uniform1(loc, (float) width);
                
                loc = gl.GetUniformLocation(shaderProgram, "height");
                gl.Uniform1(loc, (float) height);
                
                loc = gl.GetUniformLocation(shaderProgram, "c");
                gl.Uniform4(loc, (float)_r.Value, (float) _g.Value, (float) _bl.Value, 1);
                
                gl.BindVertexArray(VAO[0]);

                if (_fill.Active)
                {
                    if (!_light.Active)
                    {
                        gl.UseProgram(shaderProgram);
                        gl.CullFace(OpenGL.GL_BACK);
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr) 0);
                    }
                    else
                    {
                        if (_showLight.Active)
                        {
                             gl.BindVertexArray(VAO[1]);
                             gl.UseProgram(shaderProgram);
                             loc = gl.GetUniformLocation(shaderProgram, "model4f");
                             gl.UniformMatrix4(loc, 1, false, Matrix4f.Identity.ToArray());
                             loc = gl.GetUniformLocation(lightProgram, "c");
                             gl.Uniform4(loc, 0f, 0f, 0f, 1);
                             gl.PointSize(10);
                             gl.DrawArrays(OpenGL.GL_POINTS, 0, 1);
                             gl.BindVertexArray(0);
                             gl.BindVertexArray(VAO[0]);
                        }
                        
                        gl.UseProgram(lightProgram);
                        loc = gl.GetUniformLocation(lightProgram, "c");
                        gl.Uniform3(loc, (float)_r.Value, (float)_g.Value, (float)_bl.Value);
                        gl.CullFace(OpenGL.GL_BACK);
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr) 0);
                    }
                }

                if (_lines.Active)
                {
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "c");
                    gl.Uniform4(loc, 1f, 1f, 1f, 1);

                    gl.LineWidth(4);
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                    gl.CullFace(OpenGL.GL_BACK); 
                    gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);
                }

                if (_points.Active)
                {
                    gl.BindVertexArray(VAO[2]);
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "model4f");
                    gl.UniformMatrix4(loc, 1, false, Matrix4f.Identity.ToArray());
                    loc = gl.GetUniformLocation(shaderProgram, "c");
                    gl.Uniform4(loc, 1f, 0f, 0f, 1);
                    gl.PointSize(6);
                    gl.DrawArrays(OpenGL.GL_POINTS, 0, (int)VCount);
                }

                if (pointid1 != -1 && pointid2 != -1)
                {
                    gl.BindVertexArray(VAO[3]);
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "model4f");
                    gl.UniformMatrix4(loc, 1, false, Matrix4f.Identity.ToArray());
                    loc = gl.GetUniformLocation(shaderProgram, "c");
                    gl.Uniform4(loc, 0f, 0.5f, 0.5f, 1);
                    gl.PointSize(7);
                    gl.DrawArrays(OpenGL.GL_POINTS, 0, 1);
                }
                
                if (_genp.Active)
                {
                    gl.BindVertexArray(VAO[4]);
                    gl.UseProgram(shaderProgram);
                    loc = gl.GetUniformLocation(shaderProgram, "c");
                    gl.Uniform4(loc, 0f, 1f, 0f, 1);
                    gl.PointSize(6);
                    gl.DrawArrays(OpenGL.GL_POINTS, 0, (int)dr.Count);
                }

                gl.UseProgram(shaderProgram);
                loc = gl.GetUniformLocation(shaderProgram, "axies");
                gl.Uniform1(loc, 1);
                
                loc = gl.GetUniformLocation(shaderProgram, "shift");
                gl.Uniform1(loc, 0.8f);

                gl.BindVertexArray(VAO[5]);
                loc = gl.GetUniformLocation(shaderProgram, "c");
                gl.Uniform4(loc, 1f, 0f, 0f, 1);
                gl.LineWidth(4);
                gl.DrawArrays(OpenGL.GL_LINES, 0, 2);
                
                gl.BindVertexArray(VAO[6]);
                loc = gl.GetUniformLocation(shaderProgram, "c");
                gl.Uniform4(loc, 0f, 1f, 0f, 1);
                gl.LineWidth(4);
                gl.DrawArrays(OpenGL.GL_LINES, 0, 2);
                
                gl.BindVertexArray(VAO[7]);
                loc = gl.GetUniformLocation(shaderProgram, "c");
                gl.Uniform4(loc, 0f, 0f, 1f, 1);
                gl.LineWidth(4);
                gl.DrawArrays(OpenGL.GL_LINES, 0, 2);
                

                gl.BindVertexArray(0);
            };
             
            _drawingArea.Unrealized += delegate(object? sender, EventArgs args)
            {
                gl.UseProgram(0);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);
                gl.DeleteBuffers(2, new [] { VBO[0], VBO[1] });
                gl.DeleteVertexArrays(1, new [] { VAO[0] });
                gl.DeleteProgram(shaderProgram);
            };
        }
        

        void DrawingAreaResizeEvent(object sender, EventArgs args)
        {
            width = _drawingArea.Allocation.Width;
            height = _drawingArea.Allocation.Height;
            _drawingArea.QueueRender();
        }
        
        private void ValueChanged(object? sender, EventArgs e)
        {
            _drawingArea.QueueRender();
        }
        
        private void ValueChanged2(object? sender, EventArgs e)
        {
            Figure();
            _drawingArea.QueueRender();
        }
        
        private void DrawingAreaOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (motion)
            {
                if (_mouse.ActiveText == "Вращение камерой")
                {
                    dY = (float) args.Event.X - Oldx;
                    dX = (float) args.Event.Y - Oldy;
                    Vector3 cameraDirection = (camera.Position - camera.Target);
                    cameraDirection = Vector3.Normalize(cameraDirection);
                    Vector3 cameraRight = Vector3.Cross(camera.Up, cameraDirection);
                    cameraRight = Vector3.Normalize(cameraRight);

                    var matrix1 = Matrix4x4.CreateFromAxisAngle(cameraRight, -0.01f * dX);
                    var matrix2 = Matrix4x4.CreateFromAxisAngle(camera.Up, -0.01f * dY);

                    var npos = Vector3.Transform(camera.Position, matrix2 * matrix1);
                    camera.Up = Vector3.Normalize(Vector3.Transform(camera.Up, matrix1));
                    camera.Position = Vector3.Normalize(npos) * camera.Position.Length();
                    _drawingArea.QueueDraw();

                    Oldx = (float) args.Event.X;
                    Oldy = (float) args.Event.Y;

                    _cposX.Value = camera.Position.X;
                    _cposY.Value = camera.Position.Y;
                    _cposZ.Value = camera.Position.Z;
                }

                if (_mouse.ActiveText == "Перемещение точек")
                {
                    dX = (float)(args.Event.X / width * 2 - 1) - Oldx;
                    dY = (float) (1 - (float)args.Event.Y / height * 2) - Oldy;

                    Vector3 cameraDirection = (camera.Position - camera.Target);
                    cameraDirection = Vector3.Normalize(cameraDirection);
                    Vector3 cameraRight = Vector3.Cross(camera.Up, cameraDirection);
                    cameraRight = Vector3.Normalize(cameraRight);

                    dX /= 50;
                    dY /= 50;
                    
                    Vector3 v = new Vector3();

                    v = Verticies[pointid1][pointid2].Point +  camera.Up * (float)(width / height) * dY + cameraRight * dX;
                    
                    Verticies[pointid1][pointid2].Point = v;
                    
                    _pointx.Value = Verticies[pointid1][pointid2].Point.X;
                    _pointy.Value = Verticies[pointid1][pointid2].Point.Y;
                    _pointz.Value = Verticies[pointid1][pointid2].Point.Z;
                    
                     Figure();
                    _drawingArea.QueueRender();
                }
            }
        }
        private void DrawingAreaOnButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            motion = true;

            if (_mouse.ActiveText == "Вращение камерой")
            {
                Oldx = (float) args.Event.X;
                Oldy = (float) args.Event.Y;
            }

            if (_mouse.ActiveText == "Перемещение точек")
            {
                var proj = Perspective(angle, width / height, _np.Value, _fp.Value);
                var model = Model();
                var view = View();
                
                Oldx = (float)(args.Event.X / width * 2 - 1);
                Oldy = (float)(1 - (float)args.Event.Y / height * 2);
            
                float min = 999999;
                
                for (int i = 0; i < Verticies.Count; ++i)
                {
                    for (int j = 0; j < Verticies[i].Count; ++j)
                    {
                        Vector4f vec = new Vector4f(Verticies[i][j].Point.X, Verticies[i][j].Point.Y,
                            Verticies[i][j].Point.Z, 1);

                        vec = proj * view * model * vec;

                        vec.X /= vec.W;
                        vec.Y /= vec.W;
                        vec.Z /= vec.W;
                        vec.W /= vec.W;

                        float dist = (float) Math.Sqrt((Oldx - vec.X) * (Oldx - vec.X) +
                                                       (Oldy - vec.Y) * (Oldy - vec.Y));
            
                        if (dist < min)
                        {
                            pointid1 = i;
                            pointid2 = j;
                            min = dist;
                        }
                    }
                }

                _pointx.Value = Verticies[pointid1][pointid2].Point.X;
                _pointy.Value = Verticies[pointid1][pointid2].Point.Y;
                _pointz.Value = Verticies[pointid1][pointid2].Point.Z;

            }
        }
        private void DrawingAreaOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            motion = false;
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}