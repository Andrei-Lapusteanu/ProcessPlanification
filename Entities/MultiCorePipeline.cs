using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    class MultiCorePipeline
    {
        private List<Pipeline> multiCorePipeList;
        private int pipeCount;

        public MultiCorePipeline()
        {
            multiCorePipeList = new List<Pipeline>();
            pipeCount = 1;
            InitPipelines();
        }

        public MultiCorePipeline(int cpus)
        {
            multiCorePipeList = new List<Pipeline>();
            pipeCount = cpus;
            InitPipelines();
        }

        public void InitPipelines()
        {
            for(int i = 0; i < pipeCount; i++)
                multiCorePipeList.Add(new Pipeline());
        }
        /*
        public int getProcessesCount()
        {
            for(int i = 0; i < pipeCount; i++)
            {
                multiCorePipeList[i].
            }

            return;
        }
        */
    }
}
