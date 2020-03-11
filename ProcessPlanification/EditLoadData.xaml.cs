using Entities;
using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace ProcessPlanification
{
    public partial class EditLoadData : Window
    {

        #region Variables

        List<Process> originalProcesses = new List<Process>();

        List<DataGridContext> dataContext = new List<DataGridContext>();
        List<WindowClosingEventType> eventArgsList = new List<WindowClosingEventType>();
        Random rand = new Random();
        FileIO fio = new FileIO();
        DataGridContext dgc = new DataGridContext();

        BlurEffect blurEffect = new BlurEffect();
        BlurEffect blurReset = new BlurEffect();

        int idCounter = 1;
        private bool isEditEnabled = false;
        private bool isRandomEnabled = true;
        private bool areTextBoxValuesValid = true;
        private bool windowInitialized = false;
        private bool dataGridModified = false;
        private bool submitClicked = false;

        #endregion

        #region Constructor

        public EditLoadData(List<Process> origProc)
        {
            InitializeComponent();

            this.originalProcesses = origProc;

            InitDataGrid();

            if (originalProcesses != null)
            {
                dataContext = CreateDataGridContext();
                FillDataGrid(dataContext);
                SetNotificationText();
            }

            windowInitialized = true;

            InitBlurEffect();
        }

        #endregion

        #region Events

        private void buttonLoadData_Click(object sender, RoutedEventArgs e)
        {
            // Open window and select file (path) to read
            string filePath = OpenFileDialog(".TXT Files (*.txt)|*.txt");

            // Use ReadFromFile function from Model to read the selected file
            List<Process> tempProcList = fio.ReadFromFile(filePath);

            if (tempProcList != null)
                HandleLoadActions(tempProcList);
        }

        private void buttonSaveData_Click(object sender, RoutedEventArgs e)
        {
            // Open window to select filename to save to
            SaveFileDialog svd = new SaveFileDialog();

            svd.Filter = "Text file (*.txt)|*.txt";

            // Use SaveToFile function from Model to save to the file
            if (svd.ShowDialog() == true)
                fio.SaveToFile(originalProcesses, svd.FileName);

            dataGridModified = false;

            HidePopUp();
        }

        // This event handles the closing of the window and if there is data unsaved in the 'dataGrid', it shows a PopUp, prompting the user to take actions
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            eventArgsList.Add(new WindowClosingEventType(sender, e));

            if (dataGridModified == true && submitClicked == false)
                if (dataContext.Count != 0 && CheckDataGridValidity() == true)
                {
                    e.Cancel = true;

                    ShowPopUp();
                }
                else
                {
                    sender = eventArgsList[0].Sender;
                    e = eventArgsList[0].Cea;
                }
        }

        private void buttonSubmitEdit_Click(object sender, RoutedEventArgs e)
        {
            if (originalProcesses != null)
            {
                HandleSubmitActions();

                submitClicked = true;

                this.Close();
            }
        }

        private void buttonEnableEditing_Click(object sender, RoutedEventArgs e)
        {
            if (isEditEnabled == true)
                FinishEditingTasks();
            else
                EnableEditingTasks();
        }

        private void buttonAddRow_Click(object sender, RoutedEventArgs e)
        {
            HandleAddRowActions();
        }

        private void buttonDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            HandleDeleteRowActions();
        }

        private void buttonClearDataGrid_Click(object sender, RoutedEventArgs e)
        {
            HandleClearTableActions();
        }

        private void checkBoxRandomizeValues_Click(object sender, RoutedEventArgs e)
        {
            HandleRandomizeActions();
        }

        private void textBoxRandomRangeStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowInitialized == true)
                CheckRandomTextBoxValues();
        }

        private void textBoxRandomRangeEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowInitialized == true)
                CheckRandomTextBoxValues();
        }

        private void buttonClosingWindowPopupClose_Click(object sender, RoutedEventArgs e)
        {
            dataContext = new List<DataGridContext>();

            this.Close();
        }

        private void buttonClosingWindowPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            HidePopUp();
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            dataGridModified = true;
        }

        #endregion

        #region Functions

        // This function fills the grid with the loaded data
        public void InitDataGrid()
        {
            dataGrid.ItemsSource = dataContext;

            // Refresh is necessary to update the UI (more precisely, the 'dataGrid')
            dataGrid.Items.Refresh();

            SetDataGridStyle();
        }

        // BlurEffect is used for the background when the 'gridClosingWindowPopup' appears
        public void InitBlurEffect()
        {
            blurEffect.Radius = 10;
            blurEffect.KernelType = KernelType.Gaussian;
            blurReset.Radius = 0;
        }

        // This function fills a list of objects of type 'DataGridContext'
        // This object is similar in members to the type 'Process' but it has nullable integer types
        public List<DataGridContext> CreateDataGridContext()
        {
            List<DataGridContext> dgcList = new List<DataGridContext>();
            idCounter = 1;

            for (int i = 0; i < originalProcesses.Count; i++)
            {
                DataGridContext dgc = new DataGridContext(originalProcesses[i].ID,
                                                          originalProcesses[i].ArrivalTime,
                                                          originalProcesses[i].ProccesingTime,
                                                          originalProcesses[i].Priority);
                dgcList.Add(dgc);
                idCounter++;
            }

            return dgcList;
        }

        // This function fills the table with the loaded data
        public void FillDataGrid(List<DataGridContext> dataContext)
        {
            dataGrid.ItemsSource = dataContext;
        }

        // This function styles the header and cells of the table
        public void SetDataGridStyle()
        {
            Style s = this.FindResource("HeaderStyle") as Style;
            Style sCell = this.FindResource("DataGridCellAlignmentCenter") as Style;
            dataGrid.ColumnHeaderStyle = s;
            dataGrid.CellStyle = sCell;
            dataGrid.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        // This is the main function which deals with the notification (bottom right in UI, image + textblock)
        public void SetNotificationText()
        {
            if (areTextBoxValuesValid == false)
                HandleNotificationErrorTextBox();

            else if (CheckForNegativeValues() == false)
                HandleNotificationErrorGrid();

            else if (dataContext.Count == 0 || CheckDataGridValidity() == false)
                HandleNotificationWarning();

            else if (CheckDataGridValidity() == true)
                HandleNotificationSuccess();
        }

        // This function sets the error state of the notification when the randomize textbox values are invalid
        public void HandleNotificationErrorTextBox()
        {
            imageNotification.Source = new BitmapImage(new Uri(@"../../img/alert.png", UriKind.Relative));
            textBlockNotification.Text = "Error: Text box values are invalid or unsafe";
        }

        // This function sets the error state of the notification when the table has negative values
        public void HandleNotificationErrorGrid()
        {
            imageNotification.Source = new BitmapImage(new Uri(@"../../img/alert.png", UriKind.Relative));
            textBlockNotification.Text = "Error: Some fields may have zero or negative values";

            DisableSaveSubmitButtons();
        }

        // This function sets the warning state of the notification then fields are incomplete or missing
        public void HandleNotificationWarning()
        {
            imageNotification.Source = new BitmapImage(new Uri(@"../../img/warning.png", UriKind.Relative));
            textBlockNotification.Text = "Warning: No data or fields are incomplete";

            DisableSaveSubmitButtons();
        }

        // This function sets the success state of the notification when the data (from 'dataGrid') is valid
        public void HandleNotificationSuccess()
        {
            imageNotification.Source = new BitmapImage(new Uri(@"../../img/tick.png", UriKind.Relative));
            textBlockNotification.Text = "Success: Data is valid";

            EnableSaveSubmitButtons();
        }

        public void EnableSaveSubmitButtons()
        {
            buttonSubmitEdit.IsEnabled = true;
            buttonSubmitEdit.Opacity = 1;

            buttonSaveData.IsEnabled = true;
            buttonSaveData.Opacity = 1;
        }

        public void DisableSaveSubmitButtons()
        {
            buttonSubmitEdit.IsEnabled = false;
            buttonSubmitEdit.Opacity = 0.6;

            buttonSaveData.IsEnabled = false;
            buttonSaveData.Opacity = 0.6;
        }

        //  This function opens the default Windows file dialog (used to load data)
        public string OpenFileDialog(string fileType)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            string tempPath = Path.GetDirectoryName(
                                      Path.GetDirectoryName(
                                      Path.GetDirectoryName(
                                          System.Environment.CurrentDirectory)));

            dialog.InitialDirectory = tempPath + "\\inputs";
            dialog.DefaultExt = ".txt";
            dialog.Filter = fileType;

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
                return dialog.FileName;
            else
                return null;
        }

        // This function contains multiple calls that deal with the loaded data
        public void HandleLoadActions(List<Process> tempProcList)
        {
            originalProcesses = tempProcList;
            dataContext = CreateDataGridContext();
            FillDataGrid(dataContext);
            SetNotificationText();
        }

        // This function has calls to functions in the main window that load up the main UI with the data from this window
        // The logic may be a bit 'meh', but it works
        public void HandleSubmitActions()
        {
            ((MainWindow)Application.Current.MainWindow).ClearDataGrid(true);
            ((MainWindow)Application.Current.MainWindow).OriginalProcesses = new List<Process>();
            ((MainWindow)Application.Current.MainWindow).OriginalProcesses = originalProcesses;
            ((MainWindow)Application.Current.MainWindow).FindProcessPriorityInfo();
            ((MainWindow)Application.Current.MainWindow).InitLocalProcesses();
            ((MainWindow)Application.Current.MainWindow).EnableMainControls();
            ((MainWindow)Application.Current.MainWindow).RemoveChildren();

            Grid myGrid = ((MainWindow)Application.Current.MainWindow).gridStaticDataDisplay;
            List<Process> myProcList = ((MainWindow)Application.Current.MainWindow).OriginalProcesses;

            ((MainWindow)Application.Current.MainWindow).CreateAndFillDataGrid(myGrid, myProcList, false);
        }

        // This function handles the actions needed to add a new row to the table 'dataGrid'
        public void HandleAddRowActions()
        {
            // If window is in edit mode
            if (isEditEnabled == true)
            {
                // If randomize values checkbox is checked
                if (isRandomEnabled == true)
                {
                    // Generate random values depending on the values in the textBoxes
                    int start = int.Parse(textBoxRandomRangeStart.Text);
                    int end = int.Parse(textBoxRandomRangeEnd.Text);

                    dataContext.Add(new DataGridContext(idCounter++, rand.Next(start, end),
                                                                     rand.Next(start, end),
                                                                     rand.Next(start, end)));
                }
                // Else add empty rows
                else if (isRandomEnabled == false)
                    dataContext.Add(new DataGridContext(idCounter++));

                // Refresh table
                dataGrid.Items.Refresh();

                dataGridModified = true;
            }
        }

        // This function handles the actions needed to delete a row from the table 'dataGrid'
        public void HandleDeleteRowActions()
        {
            // If there is data to be deleted and window is in edit mode
            if (dataContext.Count > 0 && isEditEnabled == true)
            {
                // Remove last element
                dataContext.RemoveAt(dataContext.Count - 1);
                dataGrid.Items.Refresh();
                idCounter--;
                dataGridModified = true;
            }
        }

        // This function handles the actions needed clear the table 'dataGrid'
        public void HandleClearTableActions()
        {
            // If window is in edit mode
            if (isEditEnabled == true)
            {
                // Reinstatiate dataContext
                dataContext = new List<DataGridContext>();
                dataGrid.ItemsSource = dataContext;
                dataGrid.Items.Refresh();
                idCounter = 1;
            }
        }

        // This function handles the actions needed to randomize the values in the table 'dataGrid'
        public void HandleRandomizeActions()
        {
            if (checkBoxRandomizeValues.IsChecked == true)
            {
                isRandomEnabled = true;
                CheckRandomTextBoxValues();
            }
            else
            {
                isRandomEnabled = false;
                CheckRandomTextBoxValues();
            }
        }

        // This is an important function, needed to validate the data loaded or edited in the table
        // Only if this function returns true the Submit button will be enabled
        // Otherwise, the data in the table is not fit to be processed in the Controller
        public Boolean CheckDataGridValidity()
        {
            Boolean tableDataValid = false;

            // If there is any data loaded to the table
            if (dataContext.Count == 0)
                return true;

            for (int i = 0; i < dataContext.Count; i++)
            {
                // If there is any cell that is null
                if (dataContext[i].ID == null || dataContext[i].ArrivalTime == null || dataContext[i].ProcessingTime == null || dataContext[i].Priority == null)
                {
                    // Invalidate the data from the table
                    tableDataValid = false;
                    break;
                }
                else
                    tableDataValid = true;
            }

            return tableDataValid;
        }

        // This function checks to see if there are any cells in the 'dataGrid' that are negative (for ArrivalTime) or zero (for ProcessingTime and Priority)
        // It return true is all data is valid
        public Boolean CheckForNegativeValues()
        {
            for (int i = 0; i < dataContext.Count; i++)
            {
                if (dataContext[i].ArrivalTime < 0)
                    return false;

                if (dataContext[i].ProcessingTime < 1)
                    return false;

                if (dataContext[i].Priority < 1)
                    return false;
            }

            return true;
        }

        // This function handles the actions when window is put into edit mode
        // It is a bit messy
        public void EnableEditingTasks()
        {
            stackPanelSaveLoadControls.IsEnabled = false;
            stackPanelSaveLoadControls.Opacity = 0.6;
            stackPanelNotification.IsEnabled = true;
            stackPanelNotification.Opacity = 1;

            dataGrid.IsReadOnly = false;
            dataGrid.IsEnabled = true;
            isEditEnabled = true;

            buttonEnableEditing.Content = "Finish Editing";

            if (areTextBoxValuesValid == true)
                buttonAddRow.IsEnabled = true;

            buttonDeleteRow.IsEnabled = true;
            buttonClearDataGrid.IsEnabled = true;
            buttonClearDataGrid.Opacity = 1;
        }

        // This function handles the actions when window is reset from the edit mode
        // It is a bit messy
        public void FinishEditingTasks()
        {
            stackPanelSaveLoadControls.IsEnabled = true;
            stackPanelSaveLoadControls.Opacity = 1;
            stackPanelNotification.IsEnabled = false;
            stackPanelNotification.Opacity = 0.6;

            dataGrid.IsReadOnly = true;
            dataGrid.IsEnabled = false;
            isEditEnabled = false;

            buttonEnableEditing.Content = "Enable Editing";

            buttonAddRow.IsEnabled = false;
            buttonDeleteRow.IsEnabled = false;
            buttonClearDataGrid.IsEnabled = false;
            buttonClearDataGrid.Opacity = 0.6;

            if (CheckDataGridValidity() == true)
                FillProcessListFromTable();

            SetNotificationText();
        }

        // This function fills a list of processes with the (valid) data from 'dataGrid'. This is the data that will be sent to the main UI if Submit is clicked
        public void FillProcessListFromTable()
        {
            originalProcesses = new List<Process>();
            dataContext = dataGrid.ItemsSource as List<DataGridContext>;
            
            for (int i = 0; i < dataContext.Count; i++)
                originalProcesses.Add(new Process((int)dataContext[i].ID, (int)dataContext[i].ArrivalTime, (int)dataContext[i].ProcessingTime, (int)dataContext[i].Priority, 0, false));
        }

        // This function checks if the values enetered in the randomize TextBoxes are valid
        public void CheckRandomTextBoxValues()
        {
            int parsedValStart;
            int parsedValEnd;

            // If the text boxes are not empty, TryParse them to int. If it fails, fields are not valid
            if (textBoxRandomRangeStart.Text != "" || textBoxRandomRangeStart.Text != "")
            {
                if (int.TryParse(textBoxRandomRangeStart.Text, out parsedValStart) == false || int.TryParse(textBoxRandomRangeEnd.Text, out parsedValEnd) == false)
                {
                    areTextBoxValuesValid = false;
                    buttonAddRow.IsEnabled = false;
                }
                // Else, is fields are valid
                else
                {
                    // If values are valid
                    if (parsedValStart > 0 && parsedValEnd > 0 && parsedValStart < parsedValEnd)
                    {
                        areTextBoxValuesValid = true;
                        buttonAddRow.IsEnabled = true;
                    }
                    else
                    {
                        // If randomize checkbox is checked, data does not count
                        if (checkBoxRandomizeValues.IsChecked == true)
                        {
                            areTextBoxValuesValid = false;
                            buttonAddRow.IsEnabled = false;
                        }
                        // Else, it does count
                        else
                        {
                            areTextBoxValuesValid = true;
                            buttonAddRow.IsEnabled = true;
                        }
                    }
                }
            }
            else
            {
                areTextBoxValuesValid = false;
                buttonAddRow.IsEnabled = false;
            }

            SetNotificationText();
        }

        // This functions shows a PopUp inside the window
        // It is triggered when there is unsaved data in the table and the user tries the close the window
        public void ShowPopUp()
        {
            gridSaveLoadControls.IsEnabled = false;
            gridEditLoadTable.IsEnabled = false;

            gridSaveLoadControls.Effect = blurEffect;
            gridEditLoadTable.Effect = blurEffect;

            gridClosingWindowPopup.Visibility = Visibility.Visible;
        }

        // This function hides the aforementioned PopUp
        public void HidePopUp()
        {
            gridSaveLoadControls.IsEnabled = true;
            gridEditLoadTable.IsEnabled = true;

            gridSaveLoadControls.Effect = blurReset;
            gridEditLoadTable.Effect = blurReset;

            gridClosingWindowPopup.Visibility = Visibility.Hidden;
        }

        #endregion

    }
}
