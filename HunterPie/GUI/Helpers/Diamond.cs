using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HunterPie.GUI.Helpers
{
    public class Diamond : Shape
    {

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        // Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(Diamond), new PropertyMetadata(0.0, PercentageChanged));

        protected override Geometry DefiningGeometry => GetGeometry();

        private static void PercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var diamond = d as Diamond;
            diamond?.InvalidateVisual();
        }

        private Geometry GetGeometry()
        {
            Point start = new Point(Width, 0);
            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext context = geom.Open())
            {
                context.BeginFigure(start, false, false);
                for (int lIndex = 0; lIndex < 4; lIndex++)
                {
                    Point? nextPoint = CalculatePoint(lIndex);
                    if (nextPoint == null) break;
                    context.LineTo((Point)nextPoint, true, true);
                }

            }
            return geom;
        }

        private Point? CalculatePoint(int line)
        {
            double p = (100 / 4 * ((double)line)) / 100;
            if (Percentage < p) return null;
            else
            {
                double percentageOfLine = Math.Min(1, (Percentage * 100 - (25 * line)) / 25);
                switch (line)
                {
                    case 0:
                        return new Point(Width, Height * percentageOfLine);
                    case 1:
                        return new Point(Width - (Width * percentageOfLine), Height);
                    case 2:
                        return new Point(0, Height - (Height * percentageOfLine));
                    case 3:
                        return new Point(Width * percentageOfLine, 0);
                    default:
                        return null;
                }
            }
        }
    }
}
