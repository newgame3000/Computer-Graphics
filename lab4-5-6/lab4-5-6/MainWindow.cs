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
using System.Text;
using Window = Gtk.Window;
using System.IO;
using System.Linq;
using System.Reflection;
using CGPlatform;

namespace lab4_5_6
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

        [UI] private Adjustment _a = null;
        [UI] private Adjustment _b = null;
        [UI] private Adjustment _c = null;
        [UI] private Adjustment _phi = null;
        [UI] private Adjustment _theta = null;
        
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

        private List<Polygon> Polygons = new List<Polygon>();
        private List<List<Vertex>> Verticies = new List<List<Vertex>>();
        static OpenGL gl = new SharpGL.OpenGL();
        private float[] masver;
        private uint[] masid;
        private uint VCount = 0;
        private Camera camera = new Camera(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        private bool motion = false;
        private float Oldx = -1;
        private float Oldy = -1;
        private float dX = 0;
        private float dY = 0;
        private double width = 0;
        private double height = 0;
        private float startTime;
        
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
        
        unsafe void Figure()
        {
            List<Vertex> V = new List<Vertex>();
            Verticies = new List<List<Vertex>>();
            Verticies.Add(new List<Vertex>());
            Verticies[Verticies.Count - 1].Add(new Vertex(0, 0, (int) _c.Value));
            Verticies[Verticies.Count - 1].Add(new Vertex(0, 0, -(int) _c.Value));
            V.Add(new Vertex(0, 0, (int) _c.Value));
            V.Add(new Vertex(0, 0, -(int) _c.Value));
                
            double phi = 2 * Math.PI / _phi.Value;
            double theta = 2 * Math.PI / _theta.Value;

            double sumphi = 0;
            double sumtheta = 0;

            for (int i = 0; i < (int) _phi.Value; ++i)
            {
                sumphi += phi;
                Verticies.Add(new List<Vertex>());
                for (int j = 1; j < (int) _theta.Value / 2; ++j)
                {
                    sumtheta += theta;
                    Vertex A = new((float) ((int) _a.Value * Math.Sin(sumtheta) * Math.Cos(sumphi)) ,
                        (float) ((int) _b.Value * Math.Sin(sumtheta) * Math.Sin(sumphi)),
                        (float) ((int) _c.Value * Math.Cos(sumtheta)));
                    Verticies[Verticies.Count - 1].Add(A);
                    V.Add(A);
                }
                sumtheta = 0;
            }

            uint p = 0;
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Verticies[i][j].Id = p;
                    p += 1;
                }
            }

            VCount = (uint)V.Count;

            int size = (Verticies.Count - 2) * 3 + 3 + (V.Count - Verticies[0].Count - Verticies[Verticies.Count - 1].Count - Verticies.Count + 2) * 6 + (Verticies[Verticies.Count - 1].Count - 1) * 6 +
                           (Verticies.Count - 2) * 3 + 3;
                
            masid = new uint[size];
               
            masver = new float[V.Count * 6];
                
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

            foreach (var pol in Polygons)
            { 
                Normals(pol);
            }
                
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    Vector3 n = new Vector3(0, 0, 0);
                    for (int l = 0; l < Verticies[i][j].polygons.Count; ++l)
                    {
                        n += Verticies[i][j].polygons[l].NormalInWorldSpace;
                    }

                    n.X = n.X / Verticies[i][j].polygons.Count;
                    n.Y = n.Y / Verticies[i][j].polygons.Count;
                    n.Z = n.Z / Verticies[i][j].polygons.Count;
                    n = Vector3.Normalize(n);
                    Verticies[i][j].NormalInWorldSpace = n;
                }
            }
                
            int k = 0;

            foreach (Vertex vert in V)
            {
                masver[k] = vert.Point.X;
                k += 1;
                masver[k] = vert.Point.Y;
                k += 1;
                masver[k] = vert.Point.Z;
                k += 1;
            }

            int k2 = k;
                
            k = 0;
            //Крышка
            for (int i = 1; i < Verticies.Count - 1; ++i)
            {
                masid[k] = Verticies[0][0].Id;
                k += 1;
                masid[k] = Verticies[i + 1][0].Id;
                k += 1;
                masid[k] = Verticies[i][0].Id;
                k += 1;
            }
                
            masid[k] = Verticies[0][0].Id;
            k += 1;
            masid[k] = Verticies[1][0].Id;
            k += 1;
            masid[k] = Verticies[Verticies.Count - 1][0].Id;
            k += 1;

            int t = 0;
                 
            //Середина
            for (int i = 1; i < Verticies.Count - 1; ++i)
            {
                for (int j = 0; j < Verticies[i].Count - 1; ++j)
                {
                    t += 1;
                    masid[k] = Verticies[i][j + 1].Id;
                    k += 1;
                    masid[k] = Verticies[i][j].Id;
                    k += 1;
                    masid[k] = Verticies[i + 1][j + 1].Id;
                    k += 1;
                 
                    masid[k] = Verticies[i + 1][j + 1].Id;
                    k += 1;
                    masid[k] = Verticies[i][j].Id;
                    k += 1; 
                    masid[k] = Verticies[i + 1][j].Id; 
                    k += 1;
                }
            }
                 
            for (int j = 0; j < Verticies[Verticies.Count - 1].Count - 1; ++j)
            {
                masid[k] = Verticies[Verticies.Count - 1][j].Id;
                k += 1;
                masid[k] = Verticies[1][j + 1].Id;
                k += 1;
                masid[k] = Verticies[Verticies.Count - 1][j + 1].Id;
                k += 1;
                 
                masid[k] = Verticies[1][j + 1].Id;
                k += 1;
                masid[k] = Verticies[Verticies.Count - 1][j].Id;
                k += 1;
                masid[k] = Verticies[1][j].Id;
                k += 1;
            }
            //Нижняя крышка
            for (int i = 1; i < Verticies.Count - 1; ++i)
            { 
                masid[k] = Verticies[0][1].Id;
                k += 1;
                masid[k] = Verticies[i][Verticies[i].Count - 1].Id;
                k += 1;
                masid[k] = Verticies[i + 1][Verticies[i + 1].Count - 1].Id;
                k += 1;
            }
                 
            masid[k] = Verticies[0][1].Id;
            k += 1;
            masid[k] = Verticies[Verticies.Count - 1][Verticies[Verticies.Count - 1].Count - 1].Id;
            k += 1;
            masid[k] = Verticies[1][Verticies[1].Count - 1].Id;
                  
                  
            for (int i = 0; i < Verticies.Count; ++i)
            {
                for (int j = 0; j < Verticies[i].Count; ++j)
                {
                    masver[k2] = (float)Verticies[i][j].NormalInWorldSpace.X;
                    k2 += 1;
                    masver[k2] = (float)Verticies[i][j].NormalInWorldSpace.Y;
                    k2 += 1;
                    masver[k2] = (float)Verticies[i][j].NormalInWorldSpace.Z;
                    k2 += 1;
                }
            }
        }


        private unsafe MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            width = _drawingArea.Allocation.Width;
            height = _drawingArea.Allocation.Height;
            _drawingArea.Realized += DrawingAreaOnRealized; 
            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
            _drawingArea.Resize += DrawingAreaResizeEvent;
            
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
            _phi.ValueChanged += ValueChanged2;
            _theta.ValueChanged += ValueChanged2;
            _a.ValueChanged += ValueChanged2;
            _b.ValueChanged += ValueChanged2;
            _c.ValueChanged += ValueChanged2;

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

            _drawingArea.ScrollEvent += (o, args) =>
           {
               if (args.Event.Direction == ScrollDirection.Down)
               {
                   _scaleX.Value -= 0.01;
                   _scaleY.Value -= 0.01;
                   _scaleZ.Value -= 0.01;
                   
               }
               else if (args.Event.Direction == ScrollDirection.Up)
               {
                   _scaleX.Value += 0.01;
                   _scaleY.Value += 0.01;
                   _scaleZ.Value += 0.01;
               }
           };
           DeleteEvent += Window_DeleteEvent;
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
            string s = ReadFromRes("lab4-5-6.VertexShader.glsl");
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
            s = ReadFromRes("lab4-5-6.FragmentShader.glsl");
            gl.ShaderSource(fragmentShader, s);
            gl.CompileShader(fragmentShader);
                
            txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(fragmentShader, 512, (IntPtr)0, txt);
            // Console.WriteLine(txt);
            gl.GetShader(fragmentShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
                 
            
            uint fragmentLight;
            fragmentLight = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            s = ReadFromRes("lab4-5-6.FragmentLight.glsl");
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
            
            Figure();

            uint[] VAO = new uint[2];
            uint[] VBO = new uint[3];
            gl.GenVertexArrays(2, VAO);
            gl.GenBuffers(3, VBO);
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
                gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,0, (IntPtr)(sizeof(float) * VCount * 3));

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
                
                gl.FrontFace(OpenGL.GL_CW);

                gl.Enable(OpenGL.GL_DEPTH_TEST);
                gl.DepthFunc(OpenGL.GL_LESS);

                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                gl.Enable(OpenGL.GL_CULL_FACE);

                gl.Enable(OpenGL.GL_LINE_SMOOTH);
                gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
                    
                gl.ClearColor(0.8f, 0.8f, 0.8f, 1);
                
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

                var proj = Matrix4f.Identity;
                proj[1,1] = (float)Math.Min(height / width, 1);
                proj[2,2] = (float)Math.Min(width / height, 1);
                
                gl.UseProgram(shaderProgram);
                int loc = gl.GetUniformLocation(shaderProgram, "proj4f");
                gl.UniformMatrix4(loc, 1, false, proj.ToArray());
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "proj4f");
                gl.UniformMatrix4(loc, 1, false, proj.ToArray());
                
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
                
                gl.UseProgram(shaderProgram);
                loc = gl.GetUniformLocation(shaderProgram, "model4f");
                gl.UniformMatrix4(loc, 1, false, model.ToArray());
                
                gl.UseProgram(lightProgram);
                loc = gl.GetUniformLocation(lightProgram, "model4f");
                gl.UniformMatrix4(loc, 1, false, model.ToArray());
                
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
                view2[4, 1] = -camera.Position.X;
                view2[4, 2] = -camera.Position.Y;
                view2[4, 3] = -camera.Position.Z;

                view = view * view2;
                
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
                        if (_lines.Active)
                        {
                            loc = gl.GetUniformLocation(shaderProgram, "c");
                            gl.Uniform4(loc, 1f, 1f, 1f, 1);
                            gl.LineWidth(4);
                            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                            gl.CullFace(OpenGL.GL_BACK);
                            gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);
                        }
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
                             gl.Uniform4(loc, 1f, 0f, 0f, 1);
                             gl.PointSize(20);
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

                        if (_lines.Active)
                        {
                            gl.BindVertexArray(VAO[0]);
                            gl.UseProgram(lightProgram);
                            loc = gl.GetUniformLocation(lightProgram, "c");
                            gl.Uniform3(loc, 1f, 1f, 1f);
                            gl.LineWidth(2);
                            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                            gl.CullFace(OpenGL.GL_BACK);
                            gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);
                        }
                    }
                }
                else
                {
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
                }
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
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}