using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
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

namespace lab4_5
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
        
        // [UI] private Adjustment _kar = null;
        // [UI] private Adjustment _kag = null;
        // [UI] private Adjustment _kab = null;
        // [UI] private Adjustment _kdr = null;
        // [UI] private Adjustment _kdg = null;
        // [UI] private Adjustment _kdb = null;
        // [UI] private Adjustment _ksr = null;
        // [UI] private Adjustment _ksg = null;
        // [UI] private Adjustment _ksb = null;
        //
        // [UI] private Adjustment _lx = null;
        // [UI] private Adjustment _ly = null;
        // [UI] private Adjustment _lz = null;
        //
        // [UI] private Adjustment _iar = null;
        // [UI] private Adjustment _iag = null;
        // [UI] private Adjustment _iab = null;
        // [UI] private Adjustment _ilr = null;
        // [UI] private Adjustment _ilg = null;
        // [UI] private Adjustment _ilb = null;
        //
        // [UI] private Adjustment _p = null;
        // [UI] private Adjustment _d = null;
        // [UI] private Adjustment _k = null;
        
        class Vertex
        {
            public Vector3 Point;
            public Vector3 Color;
            public uint Id = 0;
            public Vertex(float x, float y, float z)
            {
                Point = new Vector3(x, y, z);
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

        private List<List<Vertex>> Verticies = new List<List<Vertex>>();
        static OpenGL gl = new SharpGL.OpenGL();
        private float[] masver;
        private uint[] masid;
        private Camera camera = new Camera(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        private bool motion = false;
        private float Oldx = -1;
        private float Oldy = -1;
        private float dX = 0;
        private float dY = 0;
        private double width = 0;
        private double height = 0;
        
        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
        }

        public static string ReadFromRes(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
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
                
                masver = new float[V.Count * 3];

                int size = (Verticies.Count - 2) * 3 + 3 + (V.Count - Verticies[0].Count - Verticies[Verticies.Count - 1].Count - Verticies.Count + 2) * 6 + (Verticies[Verticies.Count - 1].Count - 1) * 6 +
                           (Verticies.Count - 2) * 3 + 3;
                
                masid = new uint[size];
               
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
            
            // _kag.ValueChanged += ValueChanged2;
            // _kar.ValueChanged += ValueChanged2;
            // _kab.ValueChanged += ValueChanged2;
            // _kdr.ValueChanged += ValueChanged2;
            // _kdg.ValueChanged += ValueChanged2;
            // _kdb.ValueChanged += ValueChanged2;
            // _ksr.ValueChanged += ValueChanged2;
            // _ksg.ValueChanged += ValueChanged2;
            // _ksb.ValueChanged += ValueChanged2;
            //
            // _iab.ValueChanged += ValueChanged2;
            // _iag.ValueChanged += ValueChanged2;
            // _iar.ValueChanged += ValueChanged2;
            // _ilb.ValueChanged += ValueChanged2;
            // _ilg.ValueChanged += ValueChanged2;
            // _ilr.ValueChanged += ValueChanged2;
            //
            // _lx.ValueChanged += ValueChanged2;
            // _ly.ValueChanged += ValueChanged2;
            // _lz.ValueChanged += ValueChanged2;
            //
            // _p.ValueChanged += ValueChanged2;
            // _d.ValueChanged += ValueChanged2;
            // _k.ValueChanged += ValueChanged2;

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
             uint vertexShader;

            vertexShader = gl.CreateShader(OpenGL.GL_VERTEX_SHADER);
            string s = ReadFromRes("lab4-5.VertexShader.glsl");
            gl.ShaderSource(vertexShader, s);
            gl.CompileShader(vertexShader);
                
            System.Text.StringBuilder txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(vertexShader, 512, (IntPtr)0, txt);
            Console.WriteLine(txt);

            var glsl_tmp = new int[1];
            gl.GetShader(vertexShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
                
            uint fragmentShader;
            fragmentShader = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            s = ReadFromRes("lab4-5.FragmentShader.glsl");
            gl.ShaderSource(fragmentShader, s);
            gl.CompileShader(fragmentShader);
                
            txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(fragmentShader, 512, (IntPtr)0, txt);
            Console.WriteLine(txt);
            gl.GetShader(fragmentShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");
                 
            uint shaderProgram;
            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);

            gl.GetProgram(shaderProgram, OpenGL.GL_LINK_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader program link failed");

            Figure();

            uint[] VAO = new uint[1];
            uint[] VBO = new uint[2];
            _drawingArea.Render += (o, args) =>
            {
                VAO = new uint[1];
                
                gl.GenVertexArrays(1, VAO);
                    
                VBO = new uint[2];
                gl.GenBuffers(2, VBO); 
                    
                gl.BindVertexArray(VAO[0]);
                    
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[0]);
                gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, VBO[1]);
                    
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, masver, OpenGL.GL_DYNAMIC_DRAW);
                gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, masid, OpenGL.GL_DYNAMIC_DRAW);
                    
                gl.EnableVertexAttribArray(0);
                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false,0, (IntPtr)0);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0); 
                gl.BindVertexArray(0);
                
                
                gl.FrontFace(OpenGL.GL_CCW);

                gl.Enable(OpenGL.GL_DEPTH_TEST);
                gl.DepthFunc(OpenGL.GL_LESS);

                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                gl.Enable(OpenGL.GL_CULL_FACE);

                gl.Enable(OpenGL.GL_LINE_SMOOTH);
                gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
                    
                gl.ClearColor(0.8f, 0.8f, 0.8f, 1);
                
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                gl.UseProgram(shaderProgram);
                 
                var proj = Matrix4f.Identity;
                proj[1,1] = (float)Math.Min(height / width, 1);
                proj[2,2] = (float)Math.Min(width / height, 1);
                int loc = gl.GetUniformLocation(shaderProgram, "proj4f");
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
                loc = gl.GetUniformLocation(shaderProgram, "model4f");
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
                
                view[4, 1] = camera.Position.X;
                view[4, 2] = camera.Position.Y;
                view[4, 3] = camera.Position.Z;
                
                loc = gl.GetUniformLocation(shaderProgram, "view4f");
                gl.UniformMatrix4(loc, 1, true, view.ToArray());
                loc = gl.GetUniformLocation(shaderProgram, "c");
                
                gl.BindVertexArray(VAO[0]);

                if (_fill.Active)
                {
                    gl.CullFace(OpenGL.GL_BACK);
                    gl.Uniform4(loc, (float) _r.Value, (float) _g.Value, (float) _bl.Value, 1);
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                    gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr) 0);
                }

                if (_lines.Active)
                {
                    gl.Uniform4(loc, 1f, 1f, 1f, 1);
                    gl.LineWidth(2);
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                    gl.CullFace(OpenGL.GL_BACK);
                    gl.DrawElements(OpenGL.GL_TRIANGLES, masid.Length, OpenGL.GL_UNSIGNED_INT, (IntPtr)0);
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
                
                var npos = Vector3.Transform(camera.Position, matrix1 * matrix2);
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