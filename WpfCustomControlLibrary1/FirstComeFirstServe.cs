using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using WpfCustomControlLibrary1;

namespace Controller
{
    /// <summary>
    /// This represents the implementation of the "First Come First Serve" processes scheduling algorithm
    /// </summary>
    public class FirstComeFirstServe
    {

        #region Variables

        //NOTE: Not all vars are in use, they were declared when first designing application, but currently have no display function on the UI 
        private int cpu_count;
        private int pipelinedProcesses;
        private int pipelineTime;
        private int pipelinesTotalTime;

        private List<int> processingTimePerPipeline;
        private List<int> totalTimePerPipeline;

        private List<Process> processList;
        private List<List<Object>> multiCorePipeline;

        public SortingAlgorithms alg = new SortingAlgorithms();
        public SharedFunctions sharedFunc;

        #endregion

        #region Constructor

        public FirstComeFirstServe(List<Process> processList, int cpu_count)
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
            pipelinedProcesses = 0;
            pipelineTime = 0;
            pipelinesTotalTime = 0;
        }

        public void AlgorithmStart()
        {
            alg.SortByArrivalTime(ProcessList);

            
                MultiCoreAlgorithm();
        }

        public void MultiCoreAlgorithm()
        {
            while (sharedFunc.HaveAllJobsFinished(ProcessList) == false)
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
                for (int j = 0; j < ProcessList.Count; j++)
                    if (processList[j].Executing == false && processList[j].FinishedExecuting == false && ProcessList[j].ArrivalTime < 1)
                        if (MultiCorePipeline[i].OfType<Process>().Any<Process>() == true)
                        {
                            if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime < 1)
                            {
                                MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;
                                MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;

                                ProcessList[j].Executing = true;

                                MultiCorePipeline[i].Add(ProcessList[j]);
                                PipelinedProcesses++;
                            }
                        }
                        else
                        {
                            ProcessList[j].Executing = true;

                            MultiCorePipeline[i].Add(ProcessList[j]);
                            PipelinedProcesses++;
                        }

                if (MultiCorePipeline[i].OfType<Process>().Any<Process>() == true)
                    if (MultiCorePipeline[i].OfType<Process>().Last<Process>().ProccesingTime - MultiCorePipeline[i].OfType<Process>().Last<Process>().ExecutedTime < 1)
                    {
                        MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing = false;
                        MultiCorePipeline[i].OfType<Process>().Last<Process>().FinishedExecuting = true;
                    }
            }
        }

        public void IncrementTime(int time)
        {
            PipelineTime += time;

            for (int i = 0; i < ProcessList.Count; i++)
            {
                processList[i].ArrivalTime -= time;

                if (ProcessList[i].Executing == true)
                    processList[i].ExecutedTime += time;
            }
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

        public int PipelinedProcesses
        {
            get { return pipelinedProcesses; }
            set { pipelinedProcesses = value; }
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
