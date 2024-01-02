using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    //// This class is used within BOEExportTaskElement to keep track of exported
    // labor types/spread data
    public class BOEExportTaskElementLabor
    {
        public BOEExportTaskElementLabor()
            : base()
        {
            Cost = null;
            Hours = null;
            ExportFields = new Dictionary<string, string>();
            ElementType = BOEExportTaskElementType.None;
            StartDate = null;
            EndDate = null;
        }

        public BOEExportTaskElementLabor(BOEExportTaskElement boeExportTaskElement)
            : this()
        {
            if (boeExportTaskElement == null)
            {
                throw new ArgumentNullException(nameof(boeExportTaskElement));
            }

            ElementType = boeExportTaskElement.ElementType;
        }

        public decimal? Cost { get; set; }
        public decimal? Hours { get; set; }
        
        public Dictionary<string, string> ExportFields { get; set; }

        public BOEExportTaskElementType ElementType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Collection<ExportCustomField> CustomFields { get; set; }
        public decimal? TieredPercent { get; set; }

        /// <summary>
        /// The value of the order in which the resource will appear in the task listing
        /// </summary>
        public int LaborTypeOrder { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ExportCustomField
    {
        public string CustomFieldName { get; set; }
        public string CustomFieldValueName { get; set; }
        public string CustomFieldValueDescription { get; set; }
        public bool IsOpenEnded { get; set; }
    }
}
