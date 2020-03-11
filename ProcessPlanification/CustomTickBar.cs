using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ProcessPlanification
{
    class CustomTickBar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            string[] textArray = null;//= TickBarText.Split(',');
            FormattedText formattedText;
            Size size = new Size(base.ActualWidth, base.ActualHeight);
            double num = this.Maximum - this.Minimum;
            double num5 = this.ReservedSpace * 0.5;
            size.Width -= this.ReservedSpace;
            double tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));
            int j = 0;
            for (double i = 0; i <= num; i += this.TickFrequency)
            {
                string annotation;
                try
                {
                    annotation = textArray[j];
                }
                catch (System.IndexOutOfRangeException)
                {
                    annotation = "";
                }
                formattedText = new FormattedText(annotation, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black);
                dc.DrawText(formattedText, new Point((tickFrequencySize * i) + num5 - (formattedText.Width / 2), 10));
                j++;
            }

            base.OnRender(dc); //This is essential so that tick marks are displayed. 
        }
    }
}
