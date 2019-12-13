using System;
using System.ComponentModel.DataAnnotations;

namespace GenBOE.Web.ModelView
{
    public class BOEOtherDirectCostGridModelView
    {
        public string DisplayEvent { get; set; }

        public BOEOtherDirectCostGridModelView()
        {

        }

        public int ODCID { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        public string TaskID { get; set; }

        public string TaskTitle { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? EndDate { get; set; }

    }
}
