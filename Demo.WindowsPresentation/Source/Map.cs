using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using System.Globalization;
using System.Windows;
using System.Diagnostics;

namespace Demo.WindowsPresentation
{
    /// <summary>
    /// The custom map of GMapControl 
    /// </summary>
    public class Map : GMapControl
    {
        public long ElapsedMilliseconds;

#if DEBUG

        private int counter;
        readonly Typeface tf = new Typeface("GenericSansSerif");
        readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection();
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// any custom drawing here
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            stopwatch.Reset();
            stopwatch.Start();

            base.OnRender(drawingContext);
            stopwatch.Stop();

            FormattedText text =
                new FormattedText(
                    string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider + ", refresh: " +
                    counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + stopwatch.ElapsedMilliseconds + "ms",
                    CultureInfo.InvariantCulture, fd, tf, 20, Brushes.Blue);
            drawingContext.DrawText(text, new Point(text.Height, text.Height));
            text = null;
        }
#endif
    }
}
