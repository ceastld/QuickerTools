using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickerTools.Domain.Windows
{
    public class WindowConfig
    {
        public WindowConfig() { }
        public WindowConfig(double l, double t, double w, double h)
        {
            _left = l; _top = t; _width = w; _height = h;
        }
        public WindowConfig(Window window)
        {
            _left = window.Left; _top = window.Top;
            _height = window.Height; _width = window.Width;
        }
        public void SetWithWindow(Window window)
        {
            _left = window.Left; _top = window.Top;
            _height = window.Height; _width = window.Width;
        }
        public void SetToWindow(Window window)
        {
            window.Left = _left; window.Top = _top;
            window.Height = _height; window.Width = _width;
        }
        public double _left { get; set; }
        public double _top { get; set; }
        public double _width { get; set; }
        public double _height { get; set; }
    }

}
