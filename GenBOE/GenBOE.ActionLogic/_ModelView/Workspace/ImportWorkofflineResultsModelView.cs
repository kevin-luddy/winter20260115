using System;
using System.Collections.ObjectModel;
using IES.Common.classes;

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    public class ImportWorkofflineResultsModelView
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportWorkofflineResultsModelView()
        {
            this.Boes = new Collection<ImportWorkofflineBOEResultsModelView>();
            this.ImportTypes = new Collection<int>();
        }

        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public Collection<int> ImportTypes { get; set; }
        public int BoesToUpdate { get; set; }
        public int BoesToNotUpdate { get; set; }
        public int TaskElementsUpdate { get; set; }
        public int TaskElementsNotUpdate { get; set; }
        public int ResourceTypeUpdate { get; set; }
        public int ResourceTypeNotUpdate { get; set; }
        public int ResourceSpreadUpdate { get; set; }
        public int ResourceSpreadNotUpdate { get; set; }
        public Collection<ImportWorkofflineBOEResultsModelView> Boes { get; set; } 
    }

    public class ImportWorkofflineBOEResultsModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportWorkofflineBOEResultsModelView()
        {
            this.TaskElements = new Collection<ImportWorkofflineTaskElementResultsModelView>();
            this.CustomFields = new Collection<ImportWorkofflineCustomFieldModelView>();
            this.WBSNumber = CommonConstants.Unassigned_WBS_Display_Text;
            this.WBSTitle = String.Empty;
            this.CLINNumber = CommonConstants.Unassigned_CLIN_Display_Text;
            this.CLINTitle = String.Empty;
            this.Title = "No Title";
            this.Description = String.Empty;
            this.SourcesOfData = String.Empty;
        }

        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public int ImportType { get; set; }
        public int BOEID { get; set; }
        public String WBSNumber { get; set; }
        public String WBSTitle { get; set; }
        public String CLINNumber { get; set; }
        public String CLINTitle { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String SourcesOfData { get; set; }
        public int WBSID { get; set; }
        public int CLINID { get; set; }
        public Collection<ImportWorkofflineCustomFieldModelView> CustomFields { get; set; }
        public Collection<ImportWorkofflineTaskElementResultsModelView> TaskElements { get; set; }
    }

    public class ImportWorkofflineTaskElementResultsModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportWorkofflineTaskElementResultsModelView()
        {
            this.ResourceTypes = new Collection<ImportWorkofflineResourceTypeResultsModelView>();
            this.CustomFields = new Collection<ImportWorkofflineCustomFieldModelView>();
        }

        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public int ImportType { get; set; }
        public String TaskID { get; set; }
        public String TaskTitle { get; set; }
        public String BOEWBSNumber { get; set; }
        public String BOEWBSTitle { get; set; }
        public String BOECLINNumber { get; set; }
        public String BOECLINTitle { get; set; }
        public String BOETitle { get; set; }
        public int BOETaskElementID { get; set; }
        public String Description { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String MOQHoursEquation { get; set; }
        public Collection<int> WorkspaceVariableIDs { get; set; }
        public int MOQTypeID { get; set; }
        public string MOQText { get; set; }
        public Collection<ImportWorkofflineResourceTypeResultsModelView> ResourceTypes { get; set; }
        public Collection<ImportWorkofflineCustomFieldModelView> CustomFields { get; set; }
    }

    public class ImportWorkofflineResourceTypeResultsModelView
    {
        public ImportWorkofflineResourceTypeResultsModelView()
        {
            this.ResourceSpreads = new ImportWorkofflineResourceSpreadResultsModelViewCollection();
            this.CustomFields = new Collection<ImportWorkofflineCustomFieldModelView>();
        }
        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public int ImportType { get; set; }
        public String Resource { get; set; }
        public String PerformingOrg { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String SpreadCurve { get; set; }
        public String TaskID { get; set; }
        public String TaskTitle { get; set; }
        public String BOETitle { get; set; }
        public String BOEWBSNumber { get; set; }
        public String BOEWBSTitle { get; set; }
        public String BOECLINNumber { get; set; }
        public String BOECLINTitle { get; set; }
        public int BOELaborTypeID { get; set; }
        public int ResourceID { get; set; }
        public int? ClinID { get; set; }
        public int? WbsID { get; set; }
        public int PerformingOrgID { get; set; }
        public int SpreadCurveID { get; set; }
        public Decimal PercentSpread { get; set; }
        public decimal ValueSpread { get; set; }
        public ImportWorkofflineResourceSpreadResultsModelViewCollection ResourceSpreads { get; set; }
        public Collection<ImportWorkofflineCustomFieldModelView> CustomFields { get; set; }
        public bool PercentSpreadLocked { get; set; }
        public bool HourSpreadLocked { get; set; }
    }

    public class ImportWorkofflineResourceSpreadResultsModelView
    {
        public ImportWorkofflineResourceSpreadResultsModelView()
        {
            this.ImportTypes = new Collection<int>();
    
        }
        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public Collection<int> ImportTypes { get; set; }
        public int BOELaborSpreadID { get; set; }
        public String LaborSpreadDate { get; set; }
        public Decimal LaborSpreadValue { get; set; }
    }

    public class ImportWorkofflineResourceSpreadResultsModelViewCollection : Collection<ImportWorkofflineResourceSpreadResultsModelView>
    {
        public ImportWorkofflineResourceSpreadResultsModelViewCollection()
        {
            this.ImportTypes = new Collection<int>();
        }

        public Collection<int> ImportTypes { get; set; }
        public String ElementofCost { get; set; }
        public String Resource { get; set; }
        public String PerformingOrg { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String SpreadCurve { get; set; }
        public String TaskID { get; set; }
        public String TaskTitle { get; set; }
        public String BOETitle { get; set; }
        public String BOEWBSNumber { get; set; }
        public String BOEWBSTitle { get; set; }
        public String BOECLINNumber { get; set; }
        public String BOECLINTitle { get; set; }
    }

    public class ImportWorkofflineCustomFieldModelView
    {
        public ImportWorkofflineCustomFieldModelView()
        {
            this.CustomFieldName = String.Empty;
            this.CustomFieldIDDecription = String.Empty;
            this.CustomFieldID = 0;
            this.CustomFieldValueID = 0;
            this.IsOpenEnded = false;
        }

        public String CustomFieldName { get; set; }
        public String CustomFieldIDDecription { get; set; }
        public int CustomFieldID { get; set; }
        public int CustomFieldValueID { get; set; }
        public bool IsOpenEnded { get; set; }
    }

}
