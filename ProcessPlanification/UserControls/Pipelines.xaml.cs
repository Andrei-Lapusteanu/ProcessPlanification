using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessPlanification.UserControls
{
    /// <summary>
    /// Interaction logic for Pipelines.xaml
    /// </summary>
    public partial class Pipelines : UserControl
    {
        int CPU_Count;
        public Pipelines(int cpu_c)
        {
            InitializeComponent();

            CPU_Count = cpu_c;
            TestGeneration();
        }

        public void TestGeneration()
        {
            Rectangle r = new Rectangle();
            r.Height = 30;
            r.Width = 500;
            r.Fill = new SolidColorBrush(Colors.Tomato);

            if(CPU_Count == 1)
            {
                r.HorizontalAlignment = HorizontalAlignment.Center;
                r.VerticalAlignment = VerticalAlignment.Center;
                GraphArea.Children.Add(r);
            }
            else if (CPU_Count == 2)
            {

            }
            else if (CPU_Count == 3)
            {

            }
            else if (CPU_Count == 4)
            {

            }
        }
    }
}
