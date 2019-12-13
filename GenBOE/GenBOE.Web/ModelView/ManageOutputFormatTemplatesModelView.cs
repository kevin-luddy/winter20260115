// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using GenBOE.Dtos;

    /// <summary>
    /// Class to capture the output format templates in the system
    /// </summary>
    public class ManageOutputFormatTemplatesModelView : PagedResultsModelView<int>
    {
        public ManageOutputFormatTemplatesModelView()
            : base()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            GridRows = new Collection<ManageOutputFormatTemplatesModelViewRow>();
            ResultsPerPage = 100;
        }

        /// <summary>
        /// Each element in the collection represents one export format grid
        /// </summary>
        public Collection<ManageOutputFormatTemplatesModelViewRow> GridRows { get; set; }
    }

    /// <summary>
    /// Class detailing an export format template known to the system
    /// </summary>
    public class ManageOutputFormatTemplatesModelViewRow : PersistedDataModelView
    {
        public ManageOutputFormatTemplatesModelViewRow()
            : base()
        {
            this.TemplateId = -1;
            this.TemplateName = string.Empty;
            this.TemplateDescription = string.Empty;
            this.IsAvailableToAllWorkspaces = false;
            this.IsActive = true;
        }

        public ManageOutputFormatTemplatesModelViewRow(WorkspaceExportFormatDTO inExportFormatDTO)
            : this()
        {
            if (inExportFormatDTO == null)
            {
                throw new ArgumentNullException(nameof(inExportFormatDTO));
            }

            this.TemplateId = inExportFormatDTO.ExportFormat.TemplateId;
            this.TemplateDescription = inExportFormatDTO.ExportFormatDescription;
            this.TemplateName = inExportFormatDTO.ExportFormatName;
            this.IsAvailableToAllWorkspaces = inExportFormatDTO.IsAvailableToAllWorkspaces;
        }

        /// <summary>
        /// The unique id of the document in the system
        /// </summary>
        public int TemplateId { get; set; }
        
        /// <summary>
        /// The name of the template (the file is named this as well)
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// The description of the template
        /// </summary>
        public string TemplateDescription { get; set; }

        /// <summary>
        /// Gets/sets whether the template is available to all Workspaces or if Workspaces must be selected manually
        /// </summary>
        public bool IsAvailableToAllWorkspaces { get; set; }

        /// <summary>
        /// Whether the template can be replaced with a newer version of itself
        /// </summary>
        public bool IsReplaceable
        {
            get
            {
                return TemplateId != (int)ExcelReportTemplateType.MASTER;
            }
        }

        /// <summary>
        /// Gets/Sets whether the template is active or archived
        /// </summary>
        public bool IsActive { get; set; }
    }
}