using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Model
{
    public class FileIO : Control
    {

        #region Constructor

        static FileIO()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileIO), new FrameworkPropertyMetadata(typeof(FileIO)));
        }

        #endregion

        #region Functions

        // This function reads data from a file and return it as 'List<Process>'
        public List<Process> ReadFromFile(string filePath)
        {
            try {
                List<Process> processList = new List<Process>();

                using (StreamReader sr = new StreamReader(filePath))
                {
                    String line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] tokenizedLine = new string[4];
                        Entities.Process proc = new Entities.Process();

                        tokenizedLine = line.Split();

                        proc.ID = Int32.Parse(tokenizedLine[0]);
                        proc.ArrivalTime = int.Parse(tokenizedLine[1]);
                        proc.ProccesingTime = int.Parse(tokenizedLine[2]);
                        proc.Priority = int.Parse(tokenizedLine[3]);

                        processList.Add(proc);
                    }
                }
                return processList;
            }
            catch (Exception ex) { MessageBox.Show("Error reading file. File formating or parsing to integer error. \n\n" + ex); return null; }
        }

        // This function saves data to a file
        public void SaveToFile(List<Process> processList, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                String line;

                foreach (Process proc in processList)
                {
                    line = proc.ID + " " + proc.ArrivalTime + " " + proc.ProccesingTime + " " + proc.Priority + "\n";
                    sw.Write(line);
                }
            }
        }

        #endregion

    }
}
