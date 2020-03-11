using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using WpfCustomControlLibrary1;

namespace Controller
{
    /// <summary>
    /// This represents the implementation of the "Priority" processes scheduling algorithm
    /// </summary>
    public class PriorityScheduling
    {

        #region Variables

        //NOTE: Not all vars are in use, they were declared when first designing application, but currently have no display function on the UI 
        private int cpu_count;
        private int pipelineTime;
        private int pipelinesTotalTime;

        private List<int> processingTimePerPipeline;
        private List<int> totalTimePerPipeline;

        private List<Process> processList;
        private List<List<Object>> multiCorePipeline;
        private List<Process> processListCopy;

        public SortingAlgorithms alg = new SortingAlgorithms();
        public SharedFunctions sharedFunc;

        #endregion

        #region Constructor

        public PriorityScheduling(List<Process> processList, int cpu_count)
        {
            CPU_Count = cpu_count;
            ProcessList = processList;

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

            pipelineTime = 0;
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
                PutInPipelines();

                sharedFunc.InsertDeadTime(MultiCorePipeline);

                sharedFunc.FinishExecutedProcesses(MultiCorePipeline);

                IncrementTime(1);
            }

            CalculateProcessingTime();
        }

        public void PutInPipelines()
        {
            for (int i = 0; i < CPU_Count; i++)
            {
                // List that will contain processes that are fit to enter the pipeline (processes that have arrived)
                List<Process> pendingProcesses = new List<Process>();

                for (int j = 0; j < ProcessList.Count; j++)

                {
                    // A process is fit to enter the pipeline if it has arrived, it isn't currently executing, it hasn't finished executing and it isn't stalled
                    if (processListCopy[j].ArrivalTime < 1 && processListCopy[j].Executing == false && processListCopy[j].FinishedExecuting == false && processListCopy[j].Stalled == false)
                        pendingProcesses.Add(processListCopy[j]);
                }

                // If there are any candidate processes for entering the pipeline
                if (pendingProcesses.Count > 0)
                {
                    // Sort the pending processes by priority (because of Priority Scheduling)
                    if (pendingProcesses.Count > 1)
                        alg.SortByPriority(pendingProcesses);

                    // Check if there is any pipeline empty
                    bool flag = false;
                    for (int k = 0; k < MultiCorePipeline.Count; k++)
                    {
                        if (MultiCorePipeline[k].OfType<Process>().Any() == false)
                        {
                            // If true, add the best fit pending process to the pipeline
                            pendingProcesses[0].Executing = true;
                            MultiCorePipeline[k].Add(pendingProcesses[0]);
                            flag = true;
                            break;
                        }
                    }

                    if (flag == false)
                        if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime == 0)
                        {
                            // If true, add the best fit pending process to the pipeline and also deal with the finished process
                            MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;
                            MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;

                            pendingProcesses[0].Executing = true;
                            MultiCorePipeline[i].Add(pendingProcesses[0]);
                        }
                        // If not, check if the best fit pending process has higher priority than the one currently in the pipe
                        else if ((pendingProcesses[0].Priority < MultiCorePipeline[i].OfType<Process>().Last<Process>().Priority))
                        {
                            MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;

                            //Not in use anymore
                            //MultiCorePipeline[i].OfType<Process>().Last<Process>().Stalled = true;

                            // If true, eliminate process from pipeline, but keep a clone of it so it can be added later on
                            Process expelledProcess = MultiCorePipeline[i].OfType<Process>().Last<Process>().Clone() as Process;
                            MultiCorePipeline[i].Remove(MultiCorePipeline[i].OfType<Process>().Last<Process>());
                            MultiCorePipeline[i].Add(expelledProcess);

                            for (int j = 0; j < ProcessList.Count; j++)
                                if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ID == processListCopy[j].ID) // Copy neaparat
                                {
                                    processListCopy[j].ProccesingTime = processListCopy[j].ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime;
                                    processListCopy[j].ExecutedTime = 0;
                                    processListCopy[j].Executing = false;

                                    processListCopy[j].Stalled = true;
                                    processListCopy[j].StallTime = pipelineTime + 1;
                                }

                            pendingProcesses[0].Executing = true;
                            MultiCorePipeline[i].Add(pendingProcesses[0]);
                        }
                }
                // If there are no candidate processes
                else
                    for (int j = 0; j < CPU_Count; j++)
                        // Finish processes that have executed fully
                        try
                        {////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (MultiCorePipeline[j].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[j].OfType<Process>().Last<Process>().ExecutedTime == 0)
                            {
                                MultiCorePipeline[j].OfType<Process>().Last<Process>().Executing = false;
                                MultiCorePipeline[j].OfType<Process>().Last<Process>().FinishedExecuting = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            // handle
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

        #endregion

    }
}
