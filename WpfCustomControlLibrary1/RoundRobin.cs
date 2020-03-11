using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using WpfCustomControlLibrary1;

namespace Controller
{
    /// <summary>
    /// This represents the implementation of the "Round Robin" processes scheduling algorithm
    /// This is the worst implementation of all of the algorithms, so I deem it unsafe for use (especially for 3 & 4 cpus)
    /// </summary>
    public class RoundRobin
    {

        #region Variables

        //NOTE: Not all vars are in use, they were declared when first designing application, but currently have no display function on the UI 
        private int cpu_count;
        private int pipelineTime;
        private int pipelinesTotalTime;
        private int timeQuantum;

        private List<int> processingTimePerPipeline;
        private List<int> totalTimePerPipeline;

        private List<Process> processList;
        private List<List<Object>> multiCorePipeline;
        private List<Process> processListCopy;

        public SortingAlgorithms alg = new SortingAlgorithms();
        public SharedFunctions sharedFunc;

        #endregion

        #region Constructor

        public RoundRobin(List<Process> processList, int cpu_count, int timeQuantum)
        {
            CPU_Count = cpu_count;
            ProcessList = processList;
            TimeQuantum = timeQuantum;

            sharedFunc = new SharedFunctions(CPU_Count);

            InitVars();

            MultiCorePipeline = sharedFunc.InitPipelines();
        }

        #endregion

        #region Functions

        void InitVars()
        {
            processListCopy = new List<Process>();

            foreach (Process proc in ProcessList)
                processListCopy.Add(proc.Clone() as Process);

           PipelineTime = 0;
        }

        public void AlgorithmStart()
        {
            alg.SortByArrivalTime(ProcessList);

            AllCoresAlgorithm();
        }

        public void AllCoresAlgorithm()
        {
            while (sharedFunc.HaveAllJobsFinished(processListCopy) == false)
            {
                if (PipelineTime > 0 && PipelineTime % TimeQuantum == 0)
                    ExpellProcesses();

                PutInPipelines();

                sharedFunc.InsertDeadTime(MultiCorePipeline);

                sharedFunc.FinishExecutedProcesses(MultiCorePipeline);

                IncrementTime(1);
            }

            CalculateProcessingTime();
        }

        public void ExpellProcesses()
        {
            for (int i = 0; i < CPU_Count; i++)
                if (MultiCorePipeline[i].OfType<Process>().Any<Process>() == true)
                {
                    int procTimeMinusExecTime = MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime;

                    if (MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing == true)
                    {
                        if (procTimeMinusExecTime == 0)
                            MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;

                        MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;

                        Process expelledProcess = MultiCorePipeline[i].OfType<Process>().Last<Process>().Clone() as Process;
                        //expelledProcess.Expelled = true;

                        MultiCorePipeline[i].Remove(MultiCorePipeline[i].OfType<Process>().Last<Process>());
                        MultiCorePipeline[i].Add(expelledProcess);

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////
                        if (procTimeMinusExecTime != 0)
                            for (int k = 0; k < ProcessList.Count; k++)
                                if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ID == processListCopy[k].ID) // Copy neaparat
                                {
                                    processListCopy[k].ProccesingTime = processListCopy[k].ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime;
                                    processListCopy[k].ExecutedTime = 0;
                                    processListCopy[k].Executing = false;

                                    processListCopy[k].Stalled = true;
                                    processListCopy[k].StallTime = pipelineTime + 1;
                                }
                    }
                }
        }

        public int GetIndexOfNextProcess(int index)
        {
            int stop = ProcessList.Count;

            if (index == processListCopy.Count - 1)
                index = 0;
            else
                index++;

            while (stop > 0 && (processListCopy[index].Executing == true || processListCopy[index].FinishedExecuting == true || processListCopy[index].ArrivalTime > 0))
            {
                if (index == processListCopy.Count - 1)
                    index = 0;

                index++;
                stop--;
            }

            return index;
        }

        // Needs some working on 
        // The function is an extension of FCSF algorithm
        public void PutInPipelines()
        {
            // For each core
            for (int i = 0; i < CPU_Count; i++)
            {
                // If the pipeline ever contained a process
                if (MultiCorePipeline[i].OfType<Process>().Any<Process>() == true)

                    //If the process has finished executing
                    if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime < 1)
                    {
                        MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;
                        MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;
                    }

                for (int j = 0; j < ProcessList.Count; j++)
                    if (MultiCorePipeline[i].OfType<Process>().Any<Process>() == true)
                    {
                        if (MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing == false)
                            if (processListCopy[j].ArrivalTime < 1 && processListCopy[j].Executing == false && processListCopy[j].FinishedExecuting == false)
                                if (processListCopy[j].Stalled == false || processListCopy[j].ID == MultiCorePipeline[i].OfType<Process>().Last<Process>().ID)
                                    if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime < 1)
                                    {
                                        MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;
                                        MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;

                                        processListCopy[j].Executing = true;

                                        MultiCorePipeline[i].Add(processListCopy[j]);
                                    }
                                    else if (PipelineTime > 0 && PipelineTime % TimeQuantum == 0)
                                    {
                                        int idx = -1;

                                        for (int k = 0; k < processListCopy.Count; k++)
                                            if (processListCopy[k].ID == MultiCorePipeline[i].OfType<Process>().Last<Process>().ID)
                                            {
                                                idx = k;
                                                k = processListCopy.Count;
                                            }

                                        int nextIndex = GetIndexOfNextProcess(idx);

                                        if (nextIndex == -1)
                                            break;

                                        else if (processListCopy[nextIndex].ArrivalTime < 1 && processListCopy[nextIndex].Executing == false && processListCopy[nextIndex].FinishedExecuting == false)
                                        {
                                            // Put next in
                                            processListCopy[nextIndex].Executing = true;
                                            MultiCorePipeline[i].Add(processListCopy[nextIndex]);
                                            break;
                                        }

                                        else
                                        {
                                            // Put last back
                                            processListCopy[idx].Executing = true;
                                            MultiCorePipeline[i].Add(processListCopy[idx]);
                                            break;
                                        }
                                    }
                    }
                    else if (processListCopy[j].ArrivalTime < 1 && processListCopy[j].Executing == false && processListCopy[j].FinishedExecuting == false && processListCopy[j].Stalled == false)
                    {
                        processListCopy[j].Executing = true;

                        MultiCorePipeline[i].Add(processListCopy[j]);
                    }
            }
        }

        public void UnstallProcess()
        {
            foreach (Process proc in processListCopy)
                if (proc.Stalled == true)
                {
                    if (proc.StallTime == pipelineTime)
                    {
                        proc.Stalled = false;
                        proc.StallTime = 0;
                    }
                    else
                        proc.StallTime++;
                }
        }

        public void IncrementTime(int time)
        {
            PipelineTime += time;

            for (int i = 0; i < processListCopy.Count; i++)
            {
                processListCopy[i].ArrivalTime -= time;

                if (processListCopy[i].Executing == true)
                {
                    processList[i].ExecutedTime += time;
                    processListCopy[i].ExecutedTime += time;
                }
            }

            UnstallProcess();
        }

        public void CalculateProcessingTime()
        {
            //ProcessingTimePerPipeline = new List<int>();
            totalTimePerPipeline = new List<int>();

            for (int i = 0; i < cpu_count; i++)
            {
                int tempTotalTime = 0;
                int tempProcTime = 0;

                foreach (DeadTime dt in MultiCorePipeline[i].OfType<DeadTime>())
                    tempTotalTime += dt.DelayValue;

                foreach (Process proc in MultiCorePipeline[i].OfType<Process>())
                {
                    tempTotalTime += proc.ExecutedTime;
                    tempProcTime += proc.ExecutedTime;
                }

                totalTimePerPipeline.Add(tempTotalTime);
                //ProcessingTimePerPipeline.Add(tempProcTime);
            }

            PipelinesTotalTime = totalTimePerPipeline.Max();
        }

        #endregion

        #region Properties

        public int CPU_Count
        {
            get { return cpu_count; }
            set { cpu_count = value; }
        }

        public List<Process> ProcessList
        {
            get { return processList; }
            set { processList = value; }
        }

        public int PipelineTime
        {
            get { return pipelineTime; }
            set { pipelineTime = value; }
        }

        public int PipelinesTotalTime
        {
            get { return pipelinesTotalTime; }
            set { pipelinesTotalTime = value; }
        }

        public List<List<object>> MultiCorePipeline
        {
            get { return multiCorePipeline; }
            set { multiCorePipeline = value; }
        }

        public List<int> ProcessingTimePerPipeline
        {
            get { return processingTimePerPipeline; }
            set { processingTimePerPipeline = value; }
        }

        public int TimeQuantum
        {
            get { return timeQuantum; }
            set { timeQuantum = value; }
        }

        #endregion

    }
}
