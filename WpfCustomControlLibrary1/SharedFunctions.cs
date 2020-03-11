using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCustomControlLibrary1
{
    public class SharedFunctions
    {
        private int cpu_count;

        public SharedFunctions(int cpus)
        {
            this.cpu_count = cpus;
        }

        #region Functions

        public void InitVars(List<Process> procList, List<Process> procListCopy, int pipelineTime)
        {
            procListCopy = new List<Process>();

            foreach (Process proc in procList)
                procListCopy.Add(proc.Clone() as Process);

            pipelineTime = 0;
        }

        public List<List<Object>> InitPipelines()
        {
            List<List<Object>> MultiCorePipeline = new List<List<Object>>();

            for (int i = 0; i < cpu_count; i++)
                MultiCorePipeline.Add(new List<Object>());

            return MultiCorePipeline;
        }

        public Boolean HaveAllJobsFinished(List<Process> processList)
        {
            Boolean flag = true;

            for (int i = 0; i < processList.Count; i++)
                if (processList[i].FinishedExecuting == false)
                    return flag = false;

            return flag;
        }

        public void InsertDeadTime(List<List<Object>> MultiCorePipeline)
        {
            DeadTime dt = new DeadTime(1);

            for (int i = 0; i < cpu_count; i++)
                if (MultiCorePipeline[i].OfType<Process>().Any() == false)
                    MultiCorePipeline[i].Add(dt);

                else if (MultiCorePipeline[i].OfType<Process>().Last<Process>().Executing == false)
                    MultiCorePipeline[i].Add(dt);
        }

        public void FinishExecutedProcesses(List<List<Object>> MultiCorePipeline)
        {
            for (int i = 0; i < cpu_count; i++)
                for (int j = 0; j < MultiCorePipeline[i].Count; j++)
                    if (MultiCorePipeline[i][j] is Process)
                        if ((MultiCorePipeline[i][j] as Process).ProccesingTime == (MultiCorePipeline[i][j] as Process).ExecutedTime)
                        {
                            (MultiCorePipeline[i][j] as Process).FinishedExecuting = true;
                            (MultiCorePipeline[i][j] as Process).Executing = false;
                        }
        }

        #endregion

    }
}
