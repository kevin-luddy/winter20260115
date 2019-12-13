using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;
using IES.Common;

namespace GenBOE.Web.ModelView
{
    public class DefaultPerfOrgModelView : PersistedDataModelView
    {
        public DefaultPerfOrgModelView()
            : base()
        {
            PerfOrgListID = -1;
            PerfOrgListName = string.Empty;
            DefaultPerfOrgs = new Collection<DefaultPerfOrgMV>();
        }

        public DefaultPerfOrgModelView(PerformingOrgListDTO list)
            : this()
        {
            if (list != null)
            {
                PerfOrgListID = list.PerformingOrgListID;
                PerfOrgListName = list.PerformingOrgListName;
                UpdateDate = list.UpdateDate;
            }
        }

        public int PerfOrgListID { get; set; }

        [Required(ErrorMessage = "List Name is required.")]
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string PerfOrgListName { get; set; }

        public Collection<DefaultPerfOrgMV> DefaultPerfOrgs { get; set; }
    }

    public class DefaultPerfOrgMV : PersistedDataModelView
    {
        public DefaultPerfOrgMV()
            : base()
        {
            PerfOrgID = 0;
            PerfOrgName = string.Empty;
            PerfOrgDesc = string.Empty;
            Deleted = false;
        }

        public DefaultPerfOrgMV(PerformingOrgDTO performingOrg)
            : this()
        {
            if (performingOrg == null)
            {
                throw new ArgumentNullException(nameof(performingOrg));
            }

            PerfOrgID = performingOrg.Id;
            PerfOrgName = performingOrg.PerformingOrgName;
            PerfOrgDesc = performingOrg.PerformingOrgDesc;
            UpdateDate = performingOrg.UpdateDate;
        }

        public int PerfOrgID { get; set; }

        [Required(ErrorMessage = "ID is required.")]
        [StringLength(20, ErrorMessage = "A maximum of 20 characters are allowed")]
        public string PerfOrgName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(Constants.PERF_ORG_DESC_MAX_LENGTH)]
        public string PerfOrgDesc { get; set; }

        public bool Deleted { get; set; }
    }
}