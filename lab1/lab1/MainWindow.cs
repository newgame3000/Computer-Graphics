using System;
using Gtk;
using Cairo;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;
using System.Collections.Generic;
using Gdk;
using GLib;
using Application = Gtk.Application;

namespace lab1
{
    class Point
    {
        public
        Point()
        {
            x = 0;
            y = 0;
        }
        public double x = 0;
        public double y = 0;
    }
    class MainWindow : Window
    {
        [UI] private Button _button = null;
        [UI] private CheckButton _auto = null;
        [UI] private DrawingArea _drawingArea = null;
        [UI] private Adjustment _adj = null;
        [UI] private Adjustment _param = null;
        [UI] private Adjustment _phi = null;
        [UI] private Adjustment _rotation = null;
        [UI] private Adjustment _rotationX = null;
        [UI] private Adjustment _rotationY = null;
        

        private double Globalgo = 15;
        private int paramA = 10;
        int paramPhi = 20;
        private bool motion = false;
        static private int dx = 0;
        static private int dy = 0;
        private int Oldx = -1;
        private int Oldy = -1;
        private double Rotation = 0;
        private double rx = 0;
        private double ry = 0;
        private double maxFuncX;
        private double maxFuncY;

        private List<Point> points = new List<Point>();
        private List<Point> RotationPoints = new List<Point>();

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            
            double k = 2 * Math.PI / paramPhi;
            for (double i = 0; i <= 2 * Math.PI; i += k)
            {
                Point point = new Point();
                point.x = paramA * Math.Cos(i) * Math.Cos(i) * Math.Cos(i) * Globalgo;
                point.y = paramA * Math.Sin(i) * Math.Sin(i) * Math.Sin(i) * Globalgo;
                points.Add(point);
            }
            points.Add(points[0]);

            DeleteEvent += Window_DeleteEvent;
            _drawingArea.Drawn += Coordinates;

            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
                
            _drawingArea.ScrollEvent += (o, args) =>
            {
                if (args.Event.Direction == ScrollDirection.Down)
                {
                    _adj.Value -= 1;
                }
                else if (args.Event.Direction == ScrollDirection.Up)
                {
                    _adj.Value += 1;
                }
            };
            
            _drawingArea.ButtonPressEvent += DrawingAreaOnButtonPressEvent;
            _drawingArea.MotionNotifyEvent += DrawingAreaOnMotionNotifyEvent;
            _drawingArea.ButtonReleaseEvent += DrawingAreaOnButtonReleaseEvent;
            _adj.ValueChanged += AdjOnValueChanged;
            _param.ValueChanged += ParamOnValueChanged;
            _phi.ValueChanged += Redro;
            _rotation.ValueChanged += RotationOnValueChanged;
            _rotationX.ValueChanged += RotationXOnValueChanged;
            _rotationY.ValueChanged += RotationYOnValueChanged;
            _button.Clicked += ButtonOnClicked;
        }

        private void ButtonOnClicked(object? sender, EventArgs e)
        {
           Globalgo = 15;
           paramA = 10;
           paramPhi = 20;
           _phi.Value = 20;
           _param.Value = 10;
           _adj.Value = 15;
           _rotationX.Value = 0;
           _rotationY.Value = 0;
           _rotation.Value = 0;
           dx = 0;
           dy = 0;
           Oldx = -1;
           Oldy = -1;
           Rotation = 0;
           rx = 0;
           ry = 0;
           points.Clear();
           double k = 2 * Math.PI / paramPhi;
           for (double i = 0; i <= 2 * Math.PI; i += k)
           {
               Point point = new Point();
               point.x = paramA * Math.Cos(i) * Math.Cos(i) * Math.Cos(i) * Globalgo;
               point.y = paramA * Math.Sin(i) * Math.Sin(i) * Math.Sin(i) * Globalgo;
               points.Add(point);
           }
           points.Add(points[0]);
           _drawingArea.QueueDraw();
        }

        private void RotateGraph()
        {
            RotationPoints = new List<Point>();
            maxFuncX = -1e18;
            maxFuncY = -1e18;
            for (int i = 0; i < points.Count - 1; ++i)
            {
                RotationPoints.Add(new Point());
                RotationPoints[i].x = (points[i].x - rx) * Math.Cos(Rotation) - (points[i].y - ry) * Math.Sin(Rotation) + rx;
                RotationPoints[i].y = (points[i].x - rx) * Math.Sin(Rotation) + (points[i].y - ry) * Math.Cos(Rotation) + ry;
            }

            for (int i = 0; i < points.Count - 1; ++i)
            {

                if (Math.Abs(RotationPoints[i].x) > maxFuncX)
                {
                    maxFuncX = Math.Abs(RotationPoints[i].x);
                }

                if (Math.Abs(RotationPoints[i].y) > maxFuncY)
                {
                    maxFuncY = Math.Abs(RotationPoints[i].y);
                }
            }

            RotationPoints.Add(RotationPoints[0]);
        }
        
        private void RotationYOnValueChanged(object? sender, EventArgs e)
        {
            ry = - _rotationY.Value * Globalgo;
            _drawingArea.QueueDraw();
        }

        private void RotationXOnValueChanged(object? sender, EventArgs e)
        {
            rx = _rotationX.Value * Globalgo;
            _drawingArea.QueueDraw();
        }
        
        private void RotationOnValueChanged(object? sender, EventArgs e)
        {
            Rotation = (int)_rotation.Value * (Math.PI / 180);
            _drawingArea.QueueDraw();
        }

        private void DrawingAreaOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (motion)
            {
                dx += (int) args.Event.X - Oldx;
                dy += (int) args.Event.Y - Oldy;
                _drawingArea.QueueDraw();
               
                Oldx = (int) args.Event.X;
                Oldy = (int) args.Event.Y;
            }
        }

        private void DrawingAreaOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            motion = false;
        }

        private void DrawingAreaOnButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            motion = true;
            Oldx = (int) args.Event.X;
            Oldy = (int) args.Event.Y;
        }

        private void Redro(object? sender, EventArgs e)
        {
            points.Clear();
            paramPhi = (int)_phi.Value;
            double k = 2 * Math.PI / paramPhi;
            for (double i = 0; i <= 2 * Math.PI; i += k)
            {
                Point point = new Point();
                point.x = paramA * Math.Cos(i) * Math.Cos(i) * Math.Cos(i) * Globalgo;
                point.y = paramA * Math.Sin(i) * Math.Sin(i) * Math.Sin(i) * Globalgo;
                points.Add(point);
            }
            points.Add(points[0]);
            _drawingArea.QueueDraw();
        }

        private void ParamOnValueChanged(object? sender, EventArgs e)
        {
            for (int i = 0; i < points.Count - 1; ++i)
            {
                points[i].x =  points[i].x / paramA * (int)_param.Value;
                points[i].y = points[i].y / paramA * (int)_param.Value;
            }
            paramA = (int) _param.Value;
            _drawingArea.QueueDraw();
        }

        private void AdjOnValueChanged(object? sender, EventArgs e)
        {
            rx /= Globalgo;
            ry /= Globalgo;
            for (int i = 0; i < points.Count - 1; ++i)
            {
                points[i].x =  points[i].x / Globalgo * _adj.Value;
                points[i].y = points[i].y / Globalgo * _adj.Value;
            }

            Globalgo = _adj.Value;
            rx *= Globalgo;
            ry *= Globalgo;
            _drawingArea.QueueDraw();
        }

        public void DrawCoordinates(Context context, int point1, int point2, double go)
        {
            context.MoveTo(0, point2 / 2 + dy);
            context.LineTo(point1, point2 / 2 + dy);
            context.Stroke();

            context.MoveTo(point1 / 2 + dx, 0);
            context.LineTo(point1 / 2 + dx, point2);
            context.Stroke();

            for (double i = point1 / 2 + dx; i < point1; i += go)
            {
                context.MoveTo(i, point2 / 2  + dy - 3);
                context.LineTo(i, point2 / 2  + dy + 3);
                context.Stroke();
            }
            
            for (double i = point1 / 2 + dx; i > 0; i -= go)
            {
                context.MoveTo(i, point2 / 2 + dy - 3);
                context.LineTo(i, point2 / 2  + dy + 3);
                context.Stroke();
            }
            
            for (double i = point2 / 2 + dy; i < point2; i += go)
            {
                context.MoveTo(point1 / 2 + dx - 3, i);
                context.LineTo(point1 / 2 + dx + 3, i);
                context.Stroke();
            }
            
            for (double i = point2 / 2 + dy; i > 0; i -= go)
            {
                context.MoveTo(point1 / 2 + dx - 3, i);
                context.LineTo(point1 / 2 + dx + 3, i);
                context.Stroke();
            }

            RotateGraph();

            if (_auto.Active)
            {
                _adj.Value *= (0.5 * Math.Min(point1 / (2 * maxFuncX), point2 / (2 * maxFuncY)));
                _drawingArea.QueueDraw();
            }
            
            for (int i = 0; i < RotationPoints.Count - 1; ++i)
            {
                context.MoveTo(RotationPoints[i].x + point1 / 2 + dx, RotationPoints[i].y + point2 / 2 + dy);
                context.LineTo(RotationPoints[i + 1].x + point1 / 2 + dx, RotationPoints[i + 1].y + point2 / 2 + dy);
                context.Stroke();
            }
        }
        
        private void Coordinates(object o, DrawnArgs args)
        {
            var cr = args.Cr;
            cr.SetSourceRGB(0.9, 0.9, 0.9);
            cr.Paint();
            cr.Antialias = Antialias.Subpixel;
            cr.LineWidth = 2d;
            
            cr.SetSourceRGB(.0, .0, .0);
            
            DrawCoordinates(cr, _drawingArea.Window.Width, _drawingArea.Window.Height, Globalgo);
            
            cr.SetSourceRGB(0.0, 0.5, 0.9);
            cr.Arc(rx + _drawingArea.Window.Width / 2 + dx,ry + _drawingArea.Window.Height / 2 + dy,3,0,360);
            cr.Fill();
            cr.Stroke();
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}
