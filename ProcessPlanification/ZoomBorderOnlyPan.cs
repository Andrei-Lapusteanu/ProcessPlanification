using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProcessPlanification
{
    /// <summary>
    /// See 'PanArea', 'PanAreaStaticData' and 'PanAreaDynamicData' inside main project for implementation
    /// </summary>
    class ZoomBorderOnlyPan : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;
        private Point topOrigin;
        private bool gotTopOrigin = false;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point startpoint = Mouse.GetPosition(this);
            var clickedElement = e.OriginalSource as FrameworkElement;

            if (child != null && clickedElement.Uid == "" || clickedElement.Uid == "PCTitle")
            {
                if (child != null)
                {
                    var tt = GetTranslateTransform(child);
                    if (e.Delta > 0)
                    {
                        if (tt.Y < 0.0)
                            tt.Y += 15;
                    }
                    else if (e.Delta < 0)
                        tt.Y -= 15;
                        
                }
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Uncomment this to enable pan. It is commented because it is buggy.

            /*
            Point startpoint = Mouse.GetPosition(this);
            var clickedElement = e.OriginalSource as FrameworkElement;

            if (child != null && clickedElement.Uid == "")
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
            */
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point startpoint = Mouse.GetPosition(this);
            var clickedElement = e.OriginalSource as FrameworkElement;

            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

         
        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    if (gotTopOrigin == false)
                    {
                        topOrigin = origin;
                        gotTopOrigin = true;
                    }

                    if (e.RightButton != MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Pressed)
                    {
                        var tt = GetTranslateTransform(child);
                        Vector v = start - e.GetPosition(this);
                        tt.Y = origin.Y - v.Y;

                        // Don't allow further up scolling
                        if (tt.Y > topOrigin.Y)
                            tt.Y = topOrigin.Y;
                    }
                }
            }
        }

        #endregion
    }
}

