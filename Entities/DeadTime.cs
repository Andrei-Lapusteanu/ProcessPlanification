using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// This data type represents dead time inside the pipeline
    /// </summary>
    public class DeadTime : ICloneable
    {
        private int delayValue;

        public DeadTime(int deadTime)
        {
            DelayValue = deadTime;
        }

        public DeadTime()
        {
            DelayValue = 0;
        }

        public int DelayValue
        {
            get { return delayValue; }
            set { delayValue = value; }
        }

        public object Clone()
        {
            return new DeadTime(this.delayValue);
        }
    }
}
