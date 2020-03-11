using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// This is (probably) the most important data type
    /// Variable names are explicit, no need for documentation
    /// </summary>
    public class Process : IComparable<Process>, ICloneable
    {

        #region Variables

        private int id;
        private int arrivalTime;
        private int proccesingTime;
        private int executedTime;
        private int priority;
        private int stallTime;
        private Boolean executing;
        private Boolean finishedExecuting;
        private Boolean expelled;
        private Boolean altered;
        private Boolean stalled;
        private String currentCPU;

        #endregion

        #region Constructors

        public Process(int id, int arrivalTime, int proccesingTime, int priority, int execTime, bool stalled)
        {
            ID = id;
            ArrivalTime = arrivalTime;
            ProccesingTime = proccesingTime;
            ExecutedTime = execTime;
            Priority = priority;
            StallTime = 0;
            Executing = false;
            FinishedExecuting = false;
            Stalled = stalled;
            CurrentCPU = "-";
        }

        public Process()
        {
            ID = 0;
            ArrivalTime = 0;
            ProccesingTime = 0;
            ExecutedTime = 0;
            Priority = 0;
            StallTime = 0;
            Executing = false;
            FinishedExecuting = false;
            Stalled = false;
            CurrentCPU = "";
        }

        #endregion

        #region Interface Implementaions

        public static Comparison<Process> CompareByID = delegate (Process p1, Process p2)
        {
            return p1.ID.CompareTo(p2.ID);
        };

        public static Comparison<Process> CompareByArrivalTime = delegate (Process p1, Process p2)
        {
            return p1.ArrivalTime.CompareTo(p2.ArrivalTime);
        };

        public static Comparison<Process> CompareByProcessingTime = delegate (Process p1, Process p2)
        {
            return p1.ProccesingTime.CompareTo(p2.ProccesingTime);
        };

        public static Comparison<Process> CompareByPriority = delegate (Process p1, Process p2)
        {
            return p1.Priority.CompareTo(p2.Priority);
        };

        public int CompareTo(Process other)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return new Process(this.id, this.arrivalTime, this.proccesingTime, this.priority, this.executedTime, this.stalled);
        }

        #endregion

        #region Properties

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public int ArrivalTime
        {
            get { return arrivalTime; }
            set { arrivalTime = value; }
        }

        public int ProccesingTime
        {
            get { return proccesingTime; }
            set { proccesingTime = value; }
        }
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public bool Executing
        {
            get { return executing; }
            set { executing = value; }
        }

        public int ExecutedTime
        {
            get { return executedTime; }
            set { executedTime = value; }
        }

        public bool FinishedExecuting
        {
            get { return finishedExecuting; }
            set { finishedExecuting = value; }
        }

        public string CurrentCPU
        {
            get { return currentCPU; }
            set { currentCPU = value; }
        }

        public bool Stalled
        {
            get { return stalled; }
            set { stalled = value; }
        }

        public int StallTime
        {
            get { return stallTime; }
            set { stallTime = value; }
        }

        #endregion

    }
}
