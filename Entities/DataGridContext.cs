namespace Entities
{
    /// <summary>
    /// This data type is used to help with the filling of grids inside "EditLoadData.cs"
    /// </summary>
    public class DataGridContext
    {

        #region Variables

        private int? id;
        private int? arrivalTime;
        private int? processingTime;
        private int? priority;

        #endregion

        #region Constructors

        public DataGridContext(int id, int arrivalTime, int processingTime, int priority)
        {
            this.id = id;
            this.arrivalTime = arrivalTime;
            this.processingTime = processingTime;
            this.priority = priority;
        }

        public DataGridContext(int id)
        {
            this.id = id;
            this.arrivalTime = null;
            this.processingTime = null;
            this.priority = null;
        }

        public DataGridContext()
        {
            this.id = null;
            this.arrivalTime = null;
            this.processingTime = null;
            this.priority = null;
        }

        #endregion

        #region Properties

        public int? ID
        {
            get { return id; }
            set { id = value; }
        }

        public int? ArrivalTime
        {
            get { return arrivalTime; }
            set { arrivalTime = value; }
        }

        public int? ProcessingTime
        {
            get { return processingTime; }
            set { processingTime = value; }
        }

        public int? Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        #endregion

    }
}
