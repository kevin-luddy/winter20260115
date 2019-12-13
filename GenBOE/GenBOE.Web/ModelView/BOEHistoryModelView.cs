using System;
using System.ComponentModel.DataAnnotations;

namespace GenBOE.Web.ModelView
{
    public class BOEHistoryModelView
    {
        public BOEHistoryModelView()
        {

        }

        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PerformedBy { get; set; }

        // Display date as MM/DD/YYYY HH:MM AM/PM
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime Timestamp { get; set; }
    }
}
