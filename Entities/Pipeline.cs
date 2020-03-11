using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pipeline
    {
        private List<Object> objList;
        private int count;
        private int procCount;
        private int deadTimeCount;
        private Boolean isEmpty;

        public Pipeline()
        {
            ObjList = new List<object>();
            Count = 0;
            ProcCount = 0;
            DeadTimeCount = 0;
            IsEmpty = true;
        }

        // Not safe
        public Pipeline(List<Object> oList)
        {
            this.ObjList = oList;
            Count = ObjList.Count;
            ProcCount = ObjList.OfType<Process>().Count<Process>();
            DeadTimeCount = ObjList.OfType<DeadTime>().Count<DeadTime>();

            if (Count > 0)
                IsEmpty = false;
            else IsEmpty = true;
        }

        public void Add(Object obj)
        {
            objList.Add(obj);

            if (obj.GetType() == typeof(Process))
                procCount++;
            else if (obj.GetType() == typeof(DeadTime))
                deadTimeCount++;

            count++;
        }

        public void Remove()
        {
            //Object 
        }

        public Object getLastElement()
        {
            return objList.Last<Object>();
        }

        public Process getLastProcess()
        {
            return objList.OfType<Process>().Last<Process>();
        }

        public List<object> ObjList
        {
            get
            {
                return objList;
            }

            set
            {
                objList = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return isEmpty;
            }

            set
            {
                isEmpty = value;
            }
        }

        public int ProcCount
        {
            get
            {
                return procCount;
            }

            set
            {
                procCount = value;
            }
        }

        public int DeadTimeCount
        {
            get
            {
                return deadTimeCount;
            }

            set
            {
                deadTimeCount = value;
            }
        }
    }
}
