using Controller;
using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Entities;
using System.Windows.Media.Effects;

namespace ProcessPlanification
{
    public partial class MainWindow : Window
    {

        #region Variables

        int CPU_Count = 3;
        int timeQuantum = 1;

        List<Process> originalProcesses = new List<Process>();
        List<Process> processesAtSpecificTime = new List<Process>();

        List<Process> processesFCFS = new List<Process>();
        List<Process> processesSJN = new List<Process>();
        List<Process> processesPS = new List<Process>();
        List<Process> processesRR = new List<Process>();

        List<List<Object>> pipelinesFCFS = new List<List<Object>>();
        List<List<Object>> pipelinesSJN = new List<List<Object>>();
        List<List<Object>> pipelinesPS = new List<List<Object>>();
        List<List<Object>> pipelinesRR = new List<List<Object>>();

        FileIO fio = new FileIO();
        PipelineControl lastCreated = new PipelineControl(0, null, null, null, 0);

        int totalTimeFCFS = 0;
        int totalTimeSJN = 0;
        int totalTimePS = 0;
        int totalTimeRR = 0;

        int minPriority = Int32.MaxValue;
        int maxPriority = Int32.MinValue;
        int priorityDifference = -1;
        double colorIncrement = -1;

        Boolean isTimeQuantumValid = true;

        BlurEffect blurEffect = new BlurEffect();
        BlurEffect blurReset = new BlurEffect();

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            InitLocalProcesses();

            comboBoxCPU.SelectedIndex = 1;
            comboBoxAlg.SelectedIndex = 2;

            InitBlurEffect();
        }

        #endregion

        #region Events

        // This events generates a PipelineControl dependent on the CPU_Count and algorithm selected
        // In this case PipelineControl will be rendered completed
        private void buttonRunAll_Click(object sender, RoutedEventArgs e)
        {
            List<List<Object>> pipelineToBeSent = new List<List<Object>>();
            string strToBeSent = null;
            int timeToBeSent = -1;
            bool stopFlag = false;
            CPU_Count = comboBoxCPU.SelectedIndex + 1;

            ClearPipelines();

            if (comboBoxAlg.SelectedIndex == 0)
            {
                RunFCFS();
                pipelineToBeSent = pipelinesFCFS;
                strToBeSent = "First Come First Serve";
                timeToBeSent = totalTimeFCFS;
            }
            else if (comboBoxAlg.SelectedIndex == 1)
            {
                RunSJN();
                pipelineToBeSent = pipelinesSJN;
                strToBeSent = "Shortest Job First";
                timeToBeSent = totalTimeSJN;
            }
            else if (comboBoxAlg.SelectedIndex == 2)
            {
                RunPS();
                pipelineToBeSent = pipelinesPS;
                strToBeSent = "Priority Scheduling";
                timeToBeSent = totalTimePS;
            }
            else if (comboBoxAlg.SelectedIndex == 3)
            {
                if (isTimeQuantumValid)
                {
                    RunRR();
                    pipelineToBeSent = pipelinesRR;
                    strToBeSent = "Round Robin";
                    timeToBeSent = totalTimeRR;
                }
                else
                    stopFlag = true;
            }

            if (stopFlag == false)
            {
                PipelineControl pc = new PipelineControl(CPU_Count, pipelineToBeSent, originalProcesses, strToBeSent, timeToBeSent);
                pc.VerticalAlignment = VerticalAlignment.Top;
                pc.HorizontalAlignment = HorizontalAlignment.Left;
                sPanel.Children.Add(pc);
                AdjustPipelineControlWidth();
                lastCreated = pc;
            }
        }

        // This events generates a PipelineControl dependent on the CPU_Count and algorithm selected
        // In this case PipelineControl will be rendered at time = 0
        private void buttonCreatePipelineControl_Click(object sender, RoutedEventArgs e)
        {
            // Call RunAll button - this is inefficient because the PipelineControl will first be rendered fully (needs some fixing)
            buttonRunAll_Click(sender, e);

            // After the render, resize the stack panels
            if (lastCreated != null)
                foreach (UIElement child in lastCreated.CustomGrid.Children)
                    if (child is StackPanel)
                        (child as StackPanel).Width = 0;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            RemoveChildren();
        }

        private void buttonRecenterMainCanvas_Click(object sender, RoutedEventArgs e)
        {
            PanArea.Reset();
        }

        private void buttonRecenterMainCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonRecenterMainCanvas.Opacity = 1;
        }

        private void buttonRecenterMainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonRecenterMainCanvas.Opacity = 0.3;
        }

        private void buttonEditInput_Click(object sender, RoutedEventArgs e)
        {
            EditLoadData eld = new EditLoadData(originalProcesses);

            eld.ShowDialog();
        }

        private void buttonIncrement_Click(object sender, RoutedEventArgs e)
        {
            HandleIncrement(true);
        }

        private void buttonDecrement_Click(object sender, RoutedEventArgs e)
        {
            HandleIncrement(false);
        }

        private void sPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HandlePipelineControlSelection(e);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustPipelineControlWidth();
        }

        private void buttonInfoPopUpOK_Click(object sender, RoutedEventArgs e)
        {
            HidePopUp();
        }

        private void textBoxTimeQuantum_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckTimeQuantumTextBoxValidity();
        }

        #endregion

        #region Functions

        // This function empties all lists (pipelines and processes)
        public void ClearPipelines()
        {
            pipelinesFCFS = new List<List<Object>>();
            pipelinesSJN = new List<List<Object>>();
            pipelinesPS = new List<List<Object>>();
            pipelinesRR = new List<List<Object>>();

            processesFCFS = new List<Process>();
            processesSJN = new List<Process>();
            processesPS = new List<Process>();
            processesRR = new List<Process>();

            InitLocalProcesses();
        }

        // This function creates (as clones) the process lists for each algorithm
        // There's a list for each algorithm for safety (although I have not tested it with only one, so this may be inneficient)
        public void InitLocalProcesses()
        {
            foreach (Process proc in OriginalProcesses)
            {
                processesFCFS.Add(proc.Clone() as Process);
                processesSJN.Add(proc.Clone() as Process);
                processesPS.Add(proc.Clone() as Process);
                processesRR.Add(proc.Clone() as Process);
            }
        }

        public void InitBlurEffect()
        {
            blurEffect.Radius = 10;
            blurEffect.KernelType = KernelType.Gaussian;
            blurReset.Radius = 0;
        }

        public void CreateOriginalProcessesClone()
        {
            processesAtSpecificTime = new List<Process>();

            foreach (Process proc in originalProcesses)
                processesAtSpecificTime.Add(proc.Clone() as Process);
        }

        // This clears the PipelineControls from the main panel (object sPanel)
        public void RemoveChildren()
        {
            if (sPanel.Children.Count > 0)
                sPanel.Children.RemoveRange(0, sPanel.Children.Count);

            ClearPipelines();
        }

        public void RunFCFS()
        {
            FirstComeFirstServe FCFS = new FirstComeFirstServe(processesFCFS, CPU_Count);
            FCFS.AlgorithmStart();
            pipelinesFCFS = FCFS.MultiCorePipeline;
            totalTimeFCFS = FCFS.PipelinesTotalTime;
        }
        public void RunSJN()
        {
            ShortestJobNext SJN = new ShortestJobNext(processesSJN, CPU_Count);
            SJN.AlgorithmStart();
            pipelinesSJN = SJN.MultiCorePipeline;
            totalTimeSJN = SJN.PipelinesTotalTime;
        }
        public void RunPS()
        {
            PriorityScheduling PS = new PriorityScheduling(processesPS, CPU_Count);
            PS.AlgorithmStart();
            pipelinesPS = PS.MultiCorePipeline;
            totalTimePS = PS.PipelinesTotalTime;
        }
        public void RunRR()
        {
            timeQuantum = int.Parse(textBoxTimeQuantum.Text);
            RoundRobin RR = new RoundRobin(processesRR, CPU_Count, timeQuantum);
            RR.AlgorithmStart();
            pipelinesRR = RR.MultiCorePipeline;
            totalTimeRR = RR.PipelinesTotalTime;
        }

        // This is the main function used to fill 'gridStaticDataDisplay' and 'gridStaticDataDisplay'
        public void CreateAndFillDataGrid(Grid myGrid, List<Process> myProcList, bool dynamic)
        {
            List<RowDefinition> rdList = CreateDataGridRows(myProcList);

            ClearDataGrid(false);

            // Add as many rows to grid as there are processes
            foreach (RowDefinition rd in rdList)
                myGrid.RowDefinitions.Add(rd);

            // Iterate through created rows
            for (int rowIndex = 0; rowIndex < myProcList.Count; rowIndex++)
            {
                int priority = myProcList[rowIndex].Priority;
                int colStop = -1;

                // Object 'gridStaticDataDisplay' needs 4 columns, object  'gridDynamicDataDisplay' needs 6 columns (see UI)
                // Boolean 'dynamic' determines number of columns needed
                if (dynamic == true)
                    colStop = 6;
                else
                    colStop = 4;

                // Iterate through columns
                for (int colIndex = 0; colIndex < colStop; colIndex++)
                {
                    TextBlock newTB = CreateStaticGridTextBlock(colIndex, priority);

                    // It is easier to create a Rectangle and set it's background (with Fill property) rather than using BitmapImage class
                    Rectangle img = new Rectangle();

                    if (colIndex == 0)
                        newTB.Text = myProcList[rowIndex].ID.ToString();

                    else if (colIndex == 1)
                        newTB.Text = myProcList[rowIndex].ArrivalTime.ToString();

                    else if (colIndex == 2)
                        newTB.Text = myProcList[rowIndex].ProccesingTime.ToString();

                    else if (colIndex == 3)
                        newTB.Text = myProcList[rowIndex].Priority.ToString();

                    else if (colIndex == 4)
                    {
                        // If the process is currently executing 
                        if (myProcList[rowIndex].Executing == true)
                            img = StyleFinishedColumnRectangle("../../img/processing_3_Downloaded.png");

                        // If the process has stalled (been cut short by another process)
                        else if (myProcList[rowIndex].Stalled == true && myProcList[rowIndex].Executing == false)
                            img = StyleFinishedColumnRectangle("../../img/stop2.png");

                        // If the process hasn't yet started executing
                        else if (myProcList[rowIndex].Executing == false && myProcList[rowIndex].FinishedExecuting == false)
                            img = StyleFinishedColumnRectangle("../../img/alert.png");

                        // If the process has finished executing
                        else if (myProcList[rowIndex].FinishedExecuting == true)
                            img = StyleFinishedColumnRectangle("../../img/tick.png");
                    }
                    else
                        newTB.Text = myProcList[rowIndex].CurrentCPU;

                    // Add created text blocks
                    if (colIndex < 4)
                        myGrid.Children.Add(SetDataGridColsRows(newTB, rowIndex, colIndex));

                    // Add created Rectangle
                    else if (colIndex == 4)
                        myGrid.Children.Add(SetDataGridColsRowsImage(img, rowIndex, colIndex));

                    // Add remaining text block
                    else
                        myGrid.Children.Add(SetDataGridColsRows(newTB, rowIndex, colIndex));
                }

            }

        }

        // This function styles the Rectangle that will be added to the grid, it also sets its background
        public Rectangle StyleFinishedColumnRectangle(string uri)
        {
            Rectangle img = new Rectangle();

            //  The 'uri' is the relative path to the image
            img.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri(@uri, UriKind.Relative)) };
            img.Height = 18;
            img.Width = 18;
            img.VerticalAlignment = VerticalAlignment.Center;
            img.HorizontalAlignment = HorizontalAlignment.Center;

            return img;
        }

        //This function creates the rows for objects 'gridStaticDataDisplay' and 'gridStaticDataDisplay'
        public List<RowDefinition> CreateDataGridRows(List<Process> myProcList)
        {
            List<RowDefinition> rowDefinitionsList = new List<RowDefinition>();
            int rowHeight = 30;

            // Create as many rows as there are processes
            for (int i = 0; i < myProcList.Count; i++)
            {
                RowDefinition newRow = new RowDefinition();
                newRow.Height = new GridLength(rowHeight);
                rowDefinitionsList.Add(newRow);
            }

            return rowDefinitionsList;
        }

        // This function sets the created text block in function 'CreateAndFillDataGrid' to the appropiate grid cell
        public TextBlock SetDataGridColsRows(TextBlock tb, int rowIndex, int colIndex)
        {
            Grid.SetColumn(tb, colIndex);
            Grid.SetRow(tb, rowIndex + 1);

            return tb;
        }

        // This function sets the created Rectangle in function 'CreateAndFillDataGrid' to the appropiate grid cell 
        public Rectangle SetDataGridColsRowsImage(Rectangle img, int rowIndex, int colIndex)
        {
            Grid.SetColumn(img, colIndex);
            Grid.SetRow(img, rowIndex + 1);

            return img;
        }

        // This function creates and styles the Text Block that will be inserted in objects 'gridStaticDataDisplay' or 'gridStaticDataDisplay'
        public TextBlock CreateStaticGridTextBlock(int colIndex, int priority)
        {
            TextBlock newTB = new TextBlock();
            newTB.FontSize = 20;
            newTB.FontFamily = new FontFamily("Source Sans Pro");
            newTB.VerticalAlignment = VerticalAlignment.Center;
            newTB.HorizontalAlignment = HorizontalAlignment.Center;

            if (colIndex == 0)
                newTB.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF19E4BF"));

            // This if statement represents the Priority column in 'gridStaticDataDisplay' and 'gridStaticDataDisplay'. The text block is styled dynamically (from red to green) depending on the Priority values
            // Low priorities (of high value) are tinted towards red, high priorities (of low values) are tinted towards green (see formula below)
            else if (colIndex == 3)
                newTB.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)((priority - minPriority) * colorIncrement), (byte)(255 - ((priority - minPriority) * colorIncrement)), 30));

            else
                newTB.Foreground = new SolidColorBrush(Colors.White);

            return newTB;
        }

        // This function empties 'gridStaticDataDisplay' or 'gridStaticDataDisplay'
        public void ClearDataGrid(bool staticGrid)
        {
            // Boolean 'staticGrid' determines which grid to clear
            if (staticGrid == true)
            {
                if (gridStaticDataDisplay.Children.Count > 1)
                    gridStaticDataDisplay.Children.RemoveRange(4, gridStaticDataDisplay.Children.Count);
            }
            else
            {
                if (gridDynamicDataDisplay.Children.Count > 1)
                    gridDynamicDataDisplay.Children.RemoveRange(4, gridDynamicDataDisplay.Children.Count);
            }
        }

        // This is the main function that deals with changes in time (increment, decrement, timescale clicks) for a selected PipelineControl
        // Boolean 'increment' - if true, advance in time else regress
        // Boolean 'isIncremet' - false if time is NOT changed by Increment and Decrement buttons, but by clicking on the timescale, true otherwise
        public void ChangePipelineControlTime(Boolean increment, Boolean isIncrement, int extTime)
        {
            foreach (PipelineControl pc in sPanel.Children)
                if (pc.IsSelected == true)
                {
                    if (isIncrement == true)
                        if (increment == true)
                            pc.IncrementPipelineTime();
                        else pc.DecrementPipelineTime();

                    ExecuteTimeChanges(pc, extTime);

                    // Depending on the current time, resize visual elements in the PipelineControl
                    foreach (UIElement child in pc.CustomGrid.Children)
                        if (child is StackPanel)
                            (child as StackPanel).Width = pc.TimeCounter * pc.TimeScaleUnitValue;
                }
        }

        // Function that handles multiple calls needed to update process lists and grid values
        public void ExecuteTimeChanges(PipelineControl pc, int time)
        {
            ClearDataGrid(false);
            CreateOriginalProcessesClone();
            processesAtSpecificTime = pc.GetDataFromSpecificTime(time);
            CreateAndFillDataGrid(gridDynamicDataDisplay, processesAtSpecificTime, true);
            textBlockDynamicDataSetTimeValue.Text = pc.TimeCounter.ToString();
        }

        // This function handles the logic needed to select or deselect a PipelineControl
        public void SelectAndHighlightPC(PipelineControl pc, Grid mainGrid, FrameworkElement clickedElement)
        {
            if (pc.IsSelected == false)
            {
                DeselectAllPipelineControls();

                pc.IsSelected = true;

                // Add a glow effect
                AddPipelineControlGlow(mainGrid);

                // Update lists and grids on UI
                ExecuteTimeChanges(pc, pc.TimeCounter);
            }
            else
            {
                // Remove effect if PipelineControl is deselected
                mainGrid.Effect = null;
                pc.IsSelected = false;
            }
        }

        public void AddPipelineControlGlow(Grid mainGrid)
        {
            mainGrid.Effect = new DropShadowEffect
            {
                BlurRadius = 15,
                Color = new Color { A = 255, R = 255, G = 255, B = 255 },
                Direction = 320,
                ShadowDepth = 0,
                Opacity = 1
            };
        }

        // This is called before selecting a PipelineControl, it deselects all of the others (lazy but it works)
        public void DeselectAllPipelineControls()
        {
            foreach (PipelineControl pc in sPanel.Children)
            {
                pc.mainControl.Effect = null;
                pc.IsSelected = false;
            }
        }

        // After loading a data set, this function gets information regarding priorities
        // The information is used the color the Priority column ('gridStaticDataDisplay' and 'gridStaticDataDisplay') accordingly
        public void FindProcessPriorityInfo()
        {
            ResetPriorityVars();

            for (int i = 0; i < originalProcesses.Count; i++)
            {
                if (minPriority > originalProcesses[i].Priority)
                    minPriority = originalProcesses[i].Priority;

                if (maxPriority < originalProcesses[i].Priority)
                    maxPriority = originalProcesses[i].Priority;
            }

            priorityDifference = maxPriority - minPriority;
            if (priorityDifference > 0)
                colorIncrement = 255 / priorityDifference;
        }

        public void ResetPriorityVars()
        {
            minPriority = Int32.MaxValue;
            maxPriority = Int32.MinValue;
            priorityDifference = -1;
            colorIncrement = -1;
        }

        // Maybe to rewrite
        // This is used the get the elements under the mouse click down event on the object 'sPanel' 
        // NOTE: PipelineControl can only be selected by clicking on the title bar
        //       It also uses multiple parent calls to determine which PipelineControl instance was selected (it's written ugly)
        public void HandlePipelineControlSelection(MouseButtonEventArgs e)
        {
            Point startpoint = Mouse.GetPosition(this);
            var clickedElement = e.OriginalSource as FrameworkElement;

            if (clickedElement.Uid == "PCTitle")
            {
                var parent = new DependencyObject();

                if (clickedElement is Grid)
                    parent = VisualTreeHelper.GetParent(clickedElement);
                else if (clickedElement is TextBlock)
                    parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(clickedElement));

                var grandpa = VisualTreeHelper.GetParent(parent);
                var greatGrandpa = VisualTreeHelper.GetParent(grandpa);
                var ancestor = VisualTreeHelper.GetParent(greatGrandpa);

                SelectAndHighlightPC(ancestor as PipelineControl, parent as Grid, clickedElement);
            }
        }

        //This function resizes the PipelineControls with regard to their container
        public void AdjustPipelineControlWidth()
        {
            foreach (PipelineControl pc in sPanel.Children)
            {
                if (mainCanvas.ActualWidth > 10)
                {
                    sPanel.Width = mainCanvas.ActualWidth - 10;
                    pc.mainControl.Width = sPanel.Width;
                }
            }
        }

        // This function enables the main controls, after data has been loaded
        public void EnableMainControls()
        {
            stackPanelMainControls.IsEnabled = true;
            stackPanelMainControls.Opacity = 1;
        }

        // This function shows a PopUp
        public void ShowPopUp()
        {
            gridLeftPanel.IsEnabled = false;
            gridRightPanel.IsEnabled = false;

            gridLeftPanel.Effect = blurEffect;
            gridRightPanel.Effect = blurEffect;

            gridInfoPopUp.Visibility = Visibility.Visible;
        }

        // This function hides the aforementioned PopUp
        public void HidePopUp()
        {
            gridLeftPanel.IsEnabled = true;
            gridRightPanel.IsEnabled = true;

            gridLeftPanel.Effect = blurReset;
            gridRightPanel.Effect = blurReset;

            gridInfoPopUp.Visibility = Visibility.Hidden;
        }

        public void HandleIncrement(bool increment)
        {
            bool flag = false;
            foreach (PipelineControl pc in sPanel.Children)
                if (pc.IsSelected == true)
                {
                    flag = true;
                    break;
                }

            if (flag == true)
                ChangePipelineControlTime(increment, true, -1);
            else
                ShowPopUp();
        }

        public void CheckTimeQuantumTextBoxValidity()
        {
            int parsedVal;

            if (textBoxTimeQuantum.Text != "")
            {
                if (int.TryParse(textBoxTimeQuantum.Text, out parsedVal) == false)
                {
                    isTimeQuantumValid = false;
                    textBlockInvalidTimeQuantum.Visibility = Visibility.Visible;
                }
                else
                {
                    if (parsedVal > 0)
                    {
                        if (textBlockInvalidTimeQuantum != null)
                            textBlockInvalidTimeQuantum.Visibility = Visibility.Hidden;

                        isTimeQuantumValid = true;
                    }
                    else
                    {
                        textBlockInvalidTimeQuantum.Visibility = Visibility.Visible;
                        isTimeQuantumValid = false;
                    }
                }
            }
            else
            {
                isTimeQuantumValid = false;
                textBlockInvalidTimeQuantum.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Properties 

        public List<Process> OriginalProcesses
        {
            get { return originalProcesses; }
            set { originalProcesses = value; }
        }

        #endregion

    }
}
