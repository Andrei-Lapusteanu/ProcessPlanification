using Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation; 

namespace ProcessPlanification
{
    public partial class PipelineControl : UserControl
    {

        #region Variables

        List<List<Object>> processList;
        List<List<Object>> pipeliesAuxiliary;
        List<Process> originalProcesses;
        List<Process> originalProcessesCopy;

        List<SolidColorBrush> colorPalette;
        private Grid customGrid;
        Grid dockpanelGrid = new Grid();

        List<DependencyObject> hitTestList = null;

        public int CPU_Count;
        string processName;
        int totalPipelineTime;
        private int timeCounter = 0;

        int stackPanelTickBarMarginLeft = 100;
        private int timeScaleUnitValue = 40;
        double actualHeight;
        double thisActualHeight;

        private Boolean isSelected = false;

        #endregion

        #region Constructor
        public PipelineControl(int cpu_c, List<List<Object>> pList, List<Process> origProcesses, string pName, int totalTime)
        {
            InitializeComponent();

            CPU_Count_Prop = cpu_c;
            processList = pList;
            originalProcesses = origProcesses;
            processName = pName;
            totalPipelineTime = totalTime;
            this.Margin = new Thickness(0, 0, 0, 10);

            CreateOriginalProcessesClone();
            CreateUserControl();
        }

        #endregion

        #region Events

        // This event handles the MouseMove event in the graph area (in the 'canvasGraphArea')
        private void GraphArea_MouseMove(object sender, MouseEventArgs e)
        {
            // Get element under mouse
            HitTest(sender, e);

            DependencyObject elementToFind = null;

            foreach (DependencyObject element in hitTestList)
                // If hit object is of type grid (more precisely, the element that represents the process)
                if (element.GetType().Name.Equals("Grid"))
                    // Highlight it
                    HighlightProcessInPipeline(elementToFind, element);
                
                // Else if a textBox is hit (the textBox inside the aforementioned Grid
                else if (element.GetType().Name.Equals("TextBlock"))
                {
                    // If this textBox has this Uid
                    if ((element as TextBlock).Uid == "UCelement")
                    {
                        // Get its parent and highlight it
                        DependencyObject temp = new DependencyObject();
                        temp = (element as TextBlock).Parent;
                        HighlightProcessInPipeline(elementToFind, temp);
                    }
                }
                // Else, deselect the Grid (if mouse is not over it)
                else
                    foreach (UIElement child in CustomGrid.Children)
                        if (child.GetType().Name == "StackPanel")
                            foreach (UIElement baby in (child as StackPanel).Children)
                                if (baby.GetType().Name == "Grid")
                                {
                                    (baby as Grid).Children.RemoveRange(1, (baby as Grid).Children.Count);
                                    
                                    // Effect is not removed, only hidden
                                    (baby as Grid).Effect = new DropShadowEffect { Opacity = 0 };
                                    Panel.SetZIndex(baby, 0);
                                }
        }

        // This event handles the minimize action on the PipelineControl
        // It is a bit messy and I cannot be bothered to rewrite it :)
        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (buttonMinimize.Tag.ToString() == "Expanded")
            {
                buttonMinimize.Tag = "Minimized";

                RotateTransform rotateTransform = new RotateTransform(180, 0, 0);
                buttonMinimize.RenderTransform = rotateTransform; ;

            }
            else if (buttonMinimize.Tag.ToString() == "Minimized")
            {
                buttonMinimize.Tag = "Expanded";

                RotateTransform rotateTransform = new RotateTransform(0, 0, 0);
                buttonMinimize.RenderTransform = rotateTransform;
            }

            string tempStr = mainControl.ActualHeight.ToString();
            ActualHeight1 = double.Parse(tempStr);

            string tempStr2 = this.ActualHeight.ToString();
            thisActualHeight = double.Parse(tempStr);

            if (mainPanel.ActualHeight > 0)
            {
                mainPanel.Height = 0;
                this.Height = 50;

            }
            else if (mainPanel.ActualHeight == 0)
            {
                mainPanel.Height = ActualHeight1 - gridTitle.ActualHeight;
                this.Height = thisActualHeight;
            }
        }

        // This event handles the clicking of the 'X' button
        private void buttonCloseUC_Click(object sender, RoutedEventArgs e)
        {
            // Remove User Control instance by accessing parent
            (this.Parent as StackPanel).Children.Remove(this);
        }

        private void buttonRecenterGraph_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonRecenterGraph.Opacity = 1;
        }

        private void buttonRecenterGraph_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonRecenterGraph.Opacity = 0.3;
        }

        private void buttonRecenterGraph_Click(object sender, RoutedEventArgs e)
        {
            ZoomAndPanArea.Reset();
        }

        #endregion

        #region Functions

        // This is the main function that handles the creation of the PipelineControl
        public void CreateUserControl()
        {
            AdjustControlSize();

            FillColorPalette();

            CustomGrid = CreateGrid(CPU_Count_Prop);

            dockpanelGrid = CreateGrid(CPU_Count_Prop);
            dockpanelGrid.Width = 70;

            SetTitle();

            SetTitleColor();

            FillPipelines();

            canvasGraphArea.Children.Add(CustomGrid);
        }

        // This function resizes the PipelineControl dependent on the 'CPU_Count'
        // The larger the core count, the bigger the control 
        public void AdjustControlSize()
        {
            if (CPU_Count_Prop == 2)
            {
                mainControl.Height = 350;
                ZoomAndPanArea.Height = mainControl.Height - 50;
            }
            else if (CPU_Count_Prop == 3)
            {
                mainControl.Height = 400;
                ZoomAndPanArea.Height = mainControl.Height - 50;
            }
            else if (CPU_Count_Prop == 4)
            {
                mainControl.Height = 450;
                ZoomAndPanArea.Height = mainControl.Height - 50;
            }
        }

        // This function creates a palette of colors, used as background to the grids that represent the processes in the graph area
        // It is a bit limited and colors will be repeated if there are more processes than colors
        // At first, colors were randomized, but this caused problems if the colors were too dark or too bright
        // A function that properly randomizes the colors in a given color range can be written, but this is sufficient for now
        public void FillColorPalette()
        {
            colorPalette = new List<SolidColorBrush>();

            colorPalette.Add(new SolidColorBrush(Colors.Turquoise));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0068")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8B00FF")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF13F09F")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF23BBEE")));
            colorPalette.Add(new SolidColorBrush(Colors.DarkCyan));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF890000")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECEC27")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF7400")));
            colorPalette.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF701945")));
        }

        // This function, dependent on the 'CPU_Count', creates the grids that will hold the visual representations
        // of the processes, inside the 'canvasGraphArea'
        // The larger the 'CPU_Count', the more grid rows
        public Grid CreateGrid(int rows)
        {
            Grid newGrid = new Grid();
            Grid CPUIdentifier = new Grid();

            for (int i = 0; i < rows; i++)
            {
                // Create new row
                RowDefinition newRow = new RowDefinition();

                // Size is vertically accordingly
                newRow.Height = new GridLength(ZoomAndPanArea.Height / rows);
                newGrid.RowDefinitions.Add(newRow);
                
                // Create pipeline identifiers
                CPUIdentifier = new Grid();
                CPUIdentifier = CreateCPUIdentifiers(i);

                // Set identifier to row
                Grid.SetRow(CPUIdentifier, i);
                newGrid.Children.Add(CPUIdentifier);

                newGrid.Uid = "graphGridUID";
            }

            return newGrid;
        }

        // This function creates CPU identifiers (that read "CPU1", "CPU2") inside the 'canvasGraphArea'
        // See the left side of the 'canvasGraphArea' at runtime, after generating a PipelineControl with a valid data set
        public Grid CreateCPUIdentifiers(int CPU_Number)
        {
            Grid newGrid = new Grid();

            newGrid.Uid = "identifier";
            newGrid.Height = 50;
            newGrid.Width = 80;
            newGrid.HorizontalAlignment = HorizontalAlignment.Left;
            newGrid.VerticalAlignment = VerticalAlignment.Center;
            newGrid.Margin = new Thickness(20, 0, 0, 0);
            newGrid.Background = CreateCPUIdentifierBrush();

            TextBlock tb = CreateCPUIdentifierTextBlock(CPU_Number);

            newGrid.Children.Add(tb);

            return newGrid;
        }

        // This styles the grid that will hold the aforementioned identifier
        public LinearGradientBrush CreateCPUIdentifierBrush()
        {
            LinearGradientBrush lgb = new LinearGradientBrush(
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(80, 0, 0, 0),
                0);

            return lgb;
        }

        // This created the textblock that will hold the text
        // 'CPU_Number' sets the text accordingly
        public TextBlock CreateCPUIdentifierTextBlock(int CPU_Number)
        {
            TextBlock newTB = new TextBlock();

            newTB.Text = "CPU " + (CPU_Number + 1).ToString();
            newTB.FontFamily = new FontFamily("Segoe UI Light");
            newTB.FontSize = 22;
            newTB.VerticalAlignment = VerticalAlignment.Center;
            newTB.HorizontalAlignment = HorizontalAlignment.Center;
            newTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF19E4BF"));
            newTB.Uid = "UCelement";

            return newTB;
        }

        // This functions sets the title of the PipelineControl
        // The title is created dynamically, with regard to the used algorithm, CPU count, and total processing time
        public void SetTitle()
        {
            textBlockTitle.Text = processName + ", " + CPU_Count_Prop.ToString() + " CPU, " + "Proc. time = " + totalPipelineTime.ToString();
        }

        // This function styles the title bar of the PipelineControl
        // The title bar is colored differently for each algorithm (4 gradients in total)
        public void SetTitleColor()
        {
            LinearGradientBrush lgb = new LinearGradientBrush();

            if (processName == "First Come First Serve")
                lgb = new LinearGradientBrush(Color.FromArgb(255, 25, 228, 191), Color.FromArgb(255, 20, 178, 149), 90);
            else if (processName == "Shortest Job First")
                lgb = new LinearGradientBrush(Color.FromArgb(255, 84, 241, 120), Color.FromArgb(255, 25, 184, 119), 90);
            else if (processName == "Priority Scheduling")
                lgb = new LinearGradientBrush(Color.FromArgb(255, 0, 209, 255), Color.FromArgb(255, 0, 155, 189), 90);
            else
                lgb = new LinearGradientBrush(Color.FromArgb(255, 255, 240, 93), Color.FromArgb(255, 255, 168, 82), 90);

            gridTitle.Background = lgb;
        }

        // This is the main function that fills the graph area 
        public void FillPipelines()
        {
            // For each CPU
            for (int i = 0; i < CPU_Count_Prop; i++)
            {
                // Create a new stackPanel, which will hold the visual representations of the processes
                StackPanel sPanel = new StackPanel();
                sPanel = CreateStackPanels(i);

                // Create the timescale
                TimeScaleElement tse = CreateTimeScale(i);

                // For each process in the current pipeline (current CPU)
                for (int j = 0; j < processList[i].Count; j++)
                {
                    // Create a new grid (visual representation of the process)
                    Grid newGrid = new Grid();
                    newGrid.Height = 50;

                    // If the current element in the pipeline is a process, create and style the Grid
                    if (processList[i][j] is Process)
                    {
                        TextBlock tb = CreateProcessTextBlock((processList[i][j] as Process).ID);
                        newGrid.Children.Add(tb);
                        newGrid.Width = TimeScaleUnitValue * (processList[i][j] as Process).ExecutedTime;
                        newGrid.Background = colorPalette[((processList[i][j] as Process).ID - 1) % colorPalette.Count];
                        newGrid.Uid = i.ToString() + '-' + j.ToString();
                    }
                    // Else, if the current element in the pipeline is not a process (and is of type DeadTime), insert a grid styled accordingly
                    else
                    {
                        newGrid.Width = TimeScaleUnitValue * (processList[i][j] as DeadTime).DelayValue;
                        newGrid.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "img/diagonalLines4_GrayGradient.png")));
                        newGrid.Opacity = 0.7;
                        newGrid.Uid = "UCelement";
                    }

                    // Position the created grid accordingly
                    Grid.SetRow(newGrid, i);
                    sPanel.Children.Add(newGrid);
                }
            }
        }

        // This function creates the stackPanels, which will hold the visual representations of the processes
        public StackPanel CreateStackPanels(int index)
        {
            StackPanel sPanel = new StackPanel();

            // The stackPanel's width is set dependent on the 'totalPipelineTime', this making sure it fits
            // precisely the processes that will be drawn
            sPanel.Width = TimeScaleUnitValue * totalPipelineTime;
            sPanel.Height = 50;
            sPanel.HorizontalAlignment = HorizontalAlignment.Left;
            sPanel.Margin = new Thickness(stackPanelTickBarMarginLeft, 0, 20, 0);
            sPanel.Background = new SolidColorBrush(Colors.Transparent);
            sPanel.Orientation = Orientation.Horizontal;

            // Position the created stackPanel accordingly
            Grid.SetRow(sPanel, index);

            CustomGrid.Children.Add(sPanel);

            return sPanel;
        }

        // This function creates the timescale (or timeline), that accompanies every stackPanel inside the graph area
        public TimeScaleElement CreateTimeScale(int index)
        {
            TimeScaleElement newTSE = new TimeScaleElement();
            int tickBarMarginBottom = -1;

            for (int i = 0; i < totalPipelineTime + 1; i++)
            {
                // Position it horizontally, depending on the 'CPU_Count'
                // The values are absolute, if the PipelineControl is ever to be resized in height, these values need to be made relative
                if (CPU_Count == 1)
                    tickBarMarginBottom = 45;
                else if (CPU_Count == 2)
                    tickBarMarginBottom = 20;
                else if (CPU_Count == 3)
                    tickBarMarginBottom = 3;
                else if (CPU_Count == 4)
                    tickBarMarginBottom = -2;

                newTSE = new TimeScaleElement();
                newTSE.HorizontalAlignment = HorizontalAlignment.Left;
                newTSE.VerticalAlignment = VerticalAlignment.Bottom;
                newTSE.textBlockTickValue.Text = i.ToString();
                newTSE.rectangleTick.Uid = i.ToString();
                newTSE.textBlockTickValue.Uid = i.ToString();

                if (i == 0)
                    newTSE.Margin = new Thickness(stackPanelTickBarMarginLeft - 16, 0, 0, tickBarMarginBottom);
                else
                    newTSE.Margin = new Thickness(stackPanelTickBarMarginLeft - 16 + (TimeScaleUnitValue * i), 0, 0, tickBarMarginBottom);

                Grid.SetRow(newTSE, index);

                CustomGrid.Children.Add(newTSE);
            }

            return newTSE;
        }

        // This function creates the textBlocks that are put into the visual represenation of the processes inside the graph area
        // They represent the process ID
        // See "P1", "P2" ... inside the processes inside the graph area, after generating a PipelineControl with a valid data set
        public TextBlock CreateProcessTextBlock(int id)
        {
            TextBlock newTB = new TextBlock();

            newTB.Text = "P" + id.ToString();
            newTB.FontFamily = new FontFamily("Segoe UI Light");
            newTB.FontSize = 18;
            newTB.VerticalAlignment = VerticalAlignment.Center;
            newTB.HorizontalAlignment = HorizontalAlignment.Left;
            newTB.Foreground = new SolidColorBrush(Colors.White);
            newTB.Uid = "UCelement";

            return newTB;
        }


        // This is the main function that deals with changes in time
        // NOTE: The following functions are not documented (and will not be documented) well
        //       The reason is that changes in time are made using a really inneficient and "it works as it is" logic 
        //       The logic is functional and is tested, and probably doesn't need any major changes in the current state of the application
        public List<Process> GetDataFromSpecificTime(int extTime)
        {
            CreateOriginalProcessesClone();

            CreateAuxiliaryPipelines();

            SetToInitialTimeProcesses();

            SetProcessesAtSpecificTime(extTime);

            return originalProcessesCopy;
        }

        // This created a clone of the 'originalProcesses' (the data loaded from 'EditLoadData.xaml')
        public void CreateOriginalProcessesClone()
        {
            originalProcessesCopy = new List<Process>();

            if (originalProcesses != null)
                foreach (Process proc in originalProcesses)
                    originalProcessesCopy.Add(proc.Clone() as Process);
        }

        // This function creates an auxiliaty pipeline using 'processList'
        public void CreateAuxiliaryPipelines()
        {
            pipeliesAuxiliary = new List<List<Object>>();

            for (int i = 0; i < processList.Count; i++)
            {
                pipeliesAuxiliary.Add(new List<Object>());

                for (int j = 0; j < processList[i].Count; j++)
                {
                    if (processList[i][j] is DeadTime)
                        pipeliesAuxiliary[i].Add(new DeadTime(1));

                    else
                        for (int k = 0; k < (processList[i][j] as Process).ExecutedTime; k++)
                            pipeliesAuxiliary[i].Add(processList[i][j]);
                }
            }
        }

        public void SetToInitialTimeProcesses()
        {
            for (int i = 0; i < originalProcessesCopy.Count; i++)
            {
                originalProcessesCopy[i].ExecutedTime = 0;
                originalProcessesCopy[i].Executing = false;
                originalProcessesCopy[i].FinishedExecuting = false;
            }
        }

        // This function fills the lists created above with the data at a specific time in the pipeline
        public void SetProcessesAtSpecificTime(int extTime)
        {
            if (extTime > -1)
                TimeCounter = extTime;

            try
            {
                for (int j = 0; j < TimeCounter; j++)
                    for (int i = 0; i < pipeliesAuxiliary.Count; i++)
                        if (pipeliesAuxiliary[i][j] is Process)
                        {
                            Process auxProc = new Process();

                            foreach (Process proc in originalProcessesCopy)
                                if ((pipeliesAuxiliary[i][j] as Process).ID == proc.ID)
                                {
                                    auxProc = proc;
                                    break;
                                }

                            if (j > 0)
                            {
                                if (pipeliesAuxiliary[i][j + 1] is Process)
                                {
                                    if ((pipeliesAuxiliary[i][j] as Process).ID == (pipeliesAuxiliary[i][j + 1] as Process).ID)
                                        EnableAuxProcExecution(auxProc, i);

                                    else
                                    {
                                        if ((pipeliesAuxiliary[i][j] as Process).FinishedExecuting == false)
                                            StallAuxProc(auxProc);
                                        else
                                            EnableAuxProcExecution(auxProc, i);
                                    }
                                }
                                else
                                    EnableAuxProcExecution(auxProc, i);

                            }
                            else
                                EnableAuxProcExecution(auxProc, i);

                            foreach (Process mainProc in originalProcesses)
                                if (mainProc.ID == auxProc.ID)
                                    if (auxProc.ExecutedTime == mainProc.ProccesingTime)
                                    {
                                        auxProc.Executing = false;
                                        auxProc.FinishedExecuting = true;
                                        auxProc.CurrentCPU = "-";
                                    }
                        }

                for (int i = 0; i < originalProcessesCopy.Count; i++)
                {
                    for (int j = 0; j < TimeCounter; j++)
                        if (originalProcessesCopy[i].ArrivalTime > 0)
                            originalProcessesCopy[i].ArrivalTime--;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Timeline click event exception. Will be (sometime) fixed. Sorry :) \n\n" + ex);
            }
        }

        public void EnableAuxProcExecution(Process proc, int cpu)
        {
            proc.ExecutedTime++;
            proc.ProccesingTime--;
            proc.Executing = true;
            proc.Stalled = false;
            proc.CurrentCPU = (cpu + 1).ToString();
        }

        public void StallAuxProc(Process proc)
        {
            proc.ExecutedTime++;
            proc.ProccesingTime--;
            proc.Executing = false;
            proc.Stalled = true;
            proc.CurrentCPU = "-";
        }

        // This function highlights the hovered (with the mouse) process in the graph area
        // It also fills the data in the right of the PipelineComtrol (grid 'PropertiesArea') with the data from the hovered element
        public void HighlightProcessInPipeline(DependencyObject elementToFind, DependencyObject element)
        {
            elementToFind = element;

            if ((element as Grid).Children.Count > 0 && (element as Grid).Uid != "identifier")
            {
                Border b = new Border();
                b.BorderThickness = new Thickness(1.5);
                b.BorderBrush = new SolidColorBrush(Colors.White);

                // Add glow effect
                Panel.SetZIndex((element as Grid), 1);
                (element as Grid).Effect = new DropShadowEffect
                {
                    BlurRadius = 15,
                    Color = new Color { A = 255, R = 255, G = 255, B = 255 },
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 1
                };

                (element as Grid).Children.Add(b);

                string[] tokenizedStr = new string[2];
                int i = 0;
                int j = 0;

                tokenizedStr = (elementToFind as Grid).Uid.Split('-');

                if (tokenizedStr[0] != "" && tokenizedStr[0] != "UCelement")
                {
                    i = int.Parse(tokenizedStr[0]);
                    j = int.Parse(tokenizedStr[1]);

                    textBlockProcessID.Text = (processList[i][j] as Process).ID.ToString();
                    textBlockTotalProcessingTime.Text = (processList[i][j] as Process).ProccesingTime.ToString();
                    textBlockPriority.Text = (processList[i][j] as Process).Priority.ToString();

                    if ((processList[i][j] as Process).ProccesingTime != 0)
                        textBlockProcessingTimeLeft.Text = ((processList[i][j] as Process).ProccesingTime - (processList[i][j] as Process).ExecutedTime).ToString();
                    else
                        textBlockProcessingTimeLeft.Text = (processList[i][j] as Process).ProccesingTime.ToString();
                    textBlockExecutedTime.Text = (processList[i][j] as Process).ExecutedTime.ToString();

                    if ((processList[i][j] as Process).FinishedExecuting == true)
                    {
                        textBlockFinishedExecuting.Text = "true";
                        textBlockFinishedExecuting.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF33BD3F"));
                    }
                    else
                    {
                        textBlockFinishedExecuting.Text = "false";
                        textBlockFinishedExecuting.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3966"));
                    }
                }
            }
        }

        // Increment time in pipeline
        public void IncrementPipelineTime()
        {
            if (TimeCounter < totalPipelineTime)
                TimeCounter++;
        }

        // Decrement time in pipeline
        public void DecrementPipelineTime()
        {
            if (TimeCounter > 0)
                TimeCounter--;
        }

        // This handles hit tests from the mouse
        HitTestResultBehavior CollectAllVisuals_Callback(HitTestResult result)
        {
            if (result == null || result.VisualHit == null)
                return HitTestResultBehavior.Stop;

            hitTestList.Add(result.VisualHit);
            return HitTestResultBehavior.Continue;
        }

        // This gets and returs the hit tests
        public List<DependencyObject> HitTest(object sender, MouseEventArgs e)
        {
            hitTestList = new List<DependencyObject>();

            Point pt = e.GetPosition(sender as IInputElement);

            VisualTreeHelper.HitTest(
                sender as Visual, null,
                CollectAllVisuals_Callback,
                new PointHitTestParameters(pt));

            hitTestList.Reverse();

            return hitTestList;
        }

        #endregion

        #region Properties

        public int CPU_Count_Prop
        {
            get { return CPU_Count; }
            set { CPU_Count = value; }
        }

        public double ActualHeight1
        {
            get { return actualHeight; }
            set { actualHeight = value; }
        }

        public Grid CustomGrid
        {
            get { return customGrid; }
            set { customGrid = value; }
        }

        public int TimeScaleUnitValue
        {
            get { return timeScaleUnitValue; }
            set { timeScaleUnitValue = value; }
        }

        public int TimeCounter
        {
            get { return timeCounter; }
            set { timeCounter = value; }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        #endregion

    }
}
