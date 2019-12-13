using System.Collections.Generic;
using System.Web.Mvc;
using System;
using GenBOE.Dtos;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Paramters and filters used when searching for metrics.
    /// </summary>
    public class MetricsSearchDialogParametersModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public MetricsSearchDialogParametersModelView()
        {
            this.SearchFor = string.Empty;
            this.DialogTitle = string.Empty;
            this.DataSourceFilter = new List<SelectListItem>();
            this.MeasureFunctionFilter = new List<SelectListItem>();
            this.MeasureNameFilter = new List<SelectListItem>();
            this.MeasureQualifierFilter = new List<SelectListItem>();
            this.ProgramNameFilter = new List<SelectListItem>();
            this.MSTSearchHelpLink = string.Empty;
            this.MetricStoreConnected = true;
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="dto">MSTMetricSearchCriteriaDTO</param>
        public MetricsSearchDialogParametersModelView(MSTMetricSearchCriteriaDTO dto)
            : this()
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            this.SearchFor = string.Empty;

            this.DataSourceFilter.Add(new SelectListItem() { Selected = true, Text = "Select All", Value = "0" });
            this.SelectedDataSourceId = 0;
            foreach (DataSource dataSource in dto.DataSources)
            {
                this.DataSourceFilter.Add(new SelectListItem() { Selected = false, Text = dataSource.DataSourceName, Value = dataSource.Id.ToString() });
            }

            this.SelectedMeasureFunctionId = "ALL";
            this.MeasureFunctionFilter.Add(new SelectListItem() { Selected = true, Text = "Select All", Value = "All" });
            foreach (MeasureFunction measureFunction in dto.MeasureFunctions)
            {
                this.MeasureFunctionFilter.Add(new SelectListItem() { Selected = false, Text = measureFunction.MeasureFunctionName, Value = measureFunction.Id });
            }

            this.SelectedMeasureNameId = 0;
            this.MeasureNameFilter.Add(new SelectListItem() { Selected = true, Text = "Select All", Value = "0" });
            foreach (Measure measure in dto.MeasureNames)
            {
                this.MeasureNameFilter.Add(new SelectListItem() { Selected = false, Text = measure.MeasureName, Value = measure.Id.ToString() });
            }

            this.SelectedMeasureQualifierId = "ALL";
            this.MeasureQualifierFilter.Add(new SelectListItem() { Selected = true, Text = "Select All", Value = "ALL" });
            foreach (MeasureQualifier measureQualifier in dto.MeasureQualifiers)
            {
                this.MeasureQualifierFilter.Add(new SelectListItem() { Selected = false, Text = measureQualifier.MeasureQualifierDescription, Value = measureQualifier.Id.ToString() });
            }

            this.ProgramNameFilter = new List<SelectListItem>();
            this.SelectedProgramId = 0;
            this.ProgramNameFilter.Add(new SelectListItem() { Selected = true, Text = "Select All", Value = "0" });
            foreach (Program program in dto.Programs)
            {
                this.ProgramNameFilter.Add(new SelectListItem() { Selected = false, Text = program.ProgramName, Value = program.Id.ToString() });
            }
        }

        /// <summary>
        /// Search string to used against all search criteria.
        /// </summary>
        public string SearchFor { get; set; }

        /// <summary>
        /// Returns a company specific dialog suffix Id used for searching metrics. This allows using different dialogs identified by id in the
        /// UI for each company to search the metrics database.
        /// </summary>
        /// <returns></returns>
        public string SearchMetricsDialogIdSuffix { get; set; }

        /// <summary>
        /// Gets the search dialog title specific to the current company configuration.
        /// </summary>
        public string DialogTitle { get; set; }

        /// <summary>
        /// Select list of program names.
        /// </summary>
        public ICollection<SelectListItem> ProgramNameFilter { get; set; }

        /// <summary>
        /// The currently selected program name id.
        /// </summary>
        public int? SelectedProgramId { get; set; }

        /// <summary>
        /// Select list of measure names.
        /// </summary>
        public ICollection<SelectListItem> MeasureNameFilter { get; set; }

        /// <summary>
        /// The currently selected measure name id.
        /// </summary>
        public int? SelectedMeasureNameId { get; set; }

        /// <summary>
        /// Select list of measure qualifiers.
        /// </summary>
        public ICollection<SelectListItem> MeasureQualifierFilter { get; set; }

        /// <summary>
        /// The currently selected measure qualifier id.
        /// </summary>
        public string SelectedMeasureQualifierId { get; set; }

        /// <summary>
        /// Select list of measure functions.
        /// </summary>
        public ICollection<SelectListItem> MeasureFunctionFilter { get; set; }

        /// <summary>
        /// The currently selected measure name id.
        /// </summary>
        public string SelectedMeasureFunctionId { get; set; }

        /// <summary>
        /// Select list of data sources.
        /// </summary>
        public ICollection<SelectListItem> DataSourceFilter { get; set; }

        /// <summary>
        /// The currently selected measure name id.
        /// </summary>
        public int? SelectedDataSourceId { get; set; }

        /// <summary>
        /// A link to the MST search help document.
        /// </summary>
        public string MSTSearchHelpLink { get; set; }

        /// <summary>
        /// Will be set to false if unable to connect to Metrics database
        /// </summary>
        public bool MetricStoreConnected { get; set; }
    }
}
