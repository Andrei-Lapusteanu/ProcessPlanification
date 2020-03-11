using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Controller
{
    /// <summary>
    /// This class contains sorting algorithms for the Process lists
    /// </summary>
    public class SortingAlgorithms : Control
    {
        Entities.Process dummyProcess = new Entities.Process();
        static SortingAlgorithms()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SortingAlgorithms), new FrameworkPropertyMetadata(typeof(SortingAlgorithms)));
        }

        public List<Entities.Process> SortByID(List<Entities.Process> processList)
        {
            processList.Sort(Entities.Process.CompareByID);

            return processList;
        }
        public List<Entities.Process> SortByArrivalTime(List<Entities.Process> processList)
        {
            processList.Sort(Entities.Process.CompareByArrivalTime);

            return processList;
        }
        public List<Entities.Process> SortByProcessingTime(List<Entities.Process> processList)
        {
            processList.Sort(Entities.Process.CompareByProcessingTime);

            return processList;
        }
        public List<Entities.Process> SortByPriority(List<Entities.Process> processList)
        {
            processList.Sort(Entities.Process.CompareByPriority);

            return processList;
        }
    }
}
