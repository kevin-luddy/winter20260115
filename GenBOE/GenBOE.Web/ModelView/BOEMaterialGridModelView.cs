// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.DataBridge.DTO;

    public class BOEMaterialGridModelView
    {
        public string DisplayEvent { get; set; }
        public string DeleteAction { get; set; }

        public Collection<BOEMaterialGridRow> TaskElements { get; set; }
    }
    public class BOEMaterialGridRow : PersistedDataModelView
    {
        public BOEMaterialGridRow()
        {
            MaterialID = -1;
            TaskTitle = string.Empty;
            TaskDescription = string.Empty;
            Deleted = false;
        }

        public BOEMaterialGridRow(MaterialDTO inMaterial)
            : this()
        {
            if (inMaterial != null)
            {

                MaterialID = inMaterial.Id;
                TaskID = inMaterial.TaskID;
                TaskTitle = inMaterial.TaskTitle;
                Deleted = false;
            }
        }
        public int MaterialID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string TaskID { get; set; }
        public bool Deleted { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        public decimal TotalCost { get; set; }
    }
}
