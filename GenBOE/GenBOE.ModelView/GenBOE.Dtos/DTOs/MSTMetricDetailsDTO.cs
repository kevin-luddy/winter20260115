using System;
using System.Text;

namespace GenBOE.Dtos
{
    /// <summary>
    /// DTO containing all details of an MST metric.
    /// </summary>
    public class MSTMetricDetailsDTO
    {
        /// <summary>
        /// Decimal format string to add commas and 2 decimal places.
        /// </summary>
        private const string DECIMAL_FORMAT = "#,##0.00";

        /// <summary>
        /// Id of the found metric record in the BOE_Data_Table view.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id of the found metric record in the BOE_Data_Table view.
        /// </summary>
        public int PMMMeasureId { get; set; }

        /// <summary>
        /// Program name.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Scope name. Scope is a sub category of a Program. Many scopes per Program.
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// Measure data is the actual value of the measure.
        /// </summary>
        public decimal? MeasureData { get; set; }

        /// <summary>
        /// Measure Description.
        /// </summary>
        public string MeasureDescription { get; set; }

        /// <summary>
        /// Gets mesaure data in a formatted string for UI display.
        /// </summary>
        public string MeasureDataFormatted
        {
            get
            {
                string data;
                data = MeasureData != null ? Math.Round(MeasureData.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty;
                return data;
            }
        }

        /// <summary>
        /// Measure name.
        /// </summary>
        public string MeasureName { get; set; }

        /// <summary>
        /// Data source.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Measure function.
        /// </summary>
        public string MeasureFunction { get; set; }

        /// <summary>
        /// Measure qualifier.
        /// </summary>
        public string MeasureQualifier { get; set; }

        /// <summary>
        /// The measure equation detailing how the measure is calculated.
        /// </summary>
        public string Equation { get; set; }

        /// <summary>
        /// A derived measure may or may not consist of up to 5 base measures. This property concatenates the base measures and the
        /// base measure data into a user readable form. 
        /// </summary>
        public string BaseMeasures
        {
            get
            {
                // The first empty sub base measure indicates there is no more base measure sub data.
                StringBuilder baseMeasure = new StringBuilder();
                if (!string.IsNullOrEmpty(BaseMeasure1Name))
                {
                    baseMeasure.AppendFormat("{0} {1}; ", (BaseMeasure1Data != null ? Math.Round(BaseMeasure1Data.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty), BaseMeasure1Name.Trim());
                    if (!string.IsNullOrEmpty(BaseMeasure2Name))
                    {
                        baseMeasure.AppendFormat("{0} {1}; ", (BaseMeasure2Data != null ? Math.Round(BaseMeasure2Data.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty), BaseMeasure2Name.Trim());
                        if (!string.IsNullOrEmpty(BaseMeasure3Name))
                        {
                            baseMeasure.AppendFormat("{0} {1}; ", (BaseMeasure3Data != null ? Math.Round(BaseMeasure3Data.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty), BaseMeasure3Name.Trim());
                            if (!string.IsNullOrEmpty(BaseMeasure4Name))
                            {
                                baseMeasure.AppendFormat("{0} {1}; ", (BaseMeasure4Data != null ? Math.Round(BaseMeasure4Data.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty), BaseMeasure4Name.Trim());
                                if (!string.IsNullOrEmpty(BaseMeasure5Name))
                                {
                                    baseMeasure.AppendFormat("{0} {1}; ", (BaseMeasure5Data != null ? Math.Round(BaseMeasure5Data.Value, 2, MidpointRounding.AwayFromZero).ToString(DECIMAL_FORMAT) : string.Empty), BaseMeasure5Name.Trim());
                                }
                            }
                        }
                    }
                }
                return baseMeasure.ToString();
            }
        }

        /// <summary>
        /// The measure comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The measure start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The measure end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The programs contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// A truncated version of workpackages for display in a UI dialog. List of Work Packages (percentages; justification), 
        /// separated by semicolons if this is an effort measure.  Else "NA".
        /// </summary>
        public string WorkPackagesShort
        {
            get
            {
                string workPackages = WorkPackages;
                if (!string.IsNullOrEmpty(workPackages) && workPackages.Length > 100)
                {
                    return workPackages.Substring(0, 100) + "...";
                }
                return workPackages;
            }
        }

        /// <summary>
        /// List of Work Packages (percentages; justification), separated by semicolons if this is an effort measure.  Else "NA".
        /// </summary>
        public string WorkPackages { get; set; }

        /// <summary>
        /// Optional base measure 1 name.
        /// </summary>
        public string BaseMeasure1Name { get; set; }

        /// <summary>
        /// Optional base measure 1 data.
        /// </summary>
        public decimal? BaseMeasure1Data { get; set; }

        /// <summary>
        /// Program Id.
        /// </summary>
        public int ProgramId { get; set; }

        /// <summary>
        /// Function Id is an acroynm for the measure function.
        /// </summary>
        public string MeasureFunctionId { get; set; }

        /// <summary>
        /// Data source Id.
        /// </summary>
        public int DataSourceId { get; set; }

        /// <summary>
        /// Measure Id.
        /// </summary>
        public int MeasureId { get; set; }

        /// <summary>
        /// Measure qualifier Id.
        /// </summary>
        public string MeasureQualifierId { get; set; }

        /// <summary>
        /// Optional base measure 2 name.
        /// </summary>
        public string BaseMeasure2Name { get; set; }

        /// <summary>
        /// Optional base measure 2 data.
        /// </summary>
        public decimal? BaseMeasure2Data { get; set; }

        /// <summary>
        /// Optional base measure 3 name.
        /// </summary>
        public string BaseMeasure3Name { get; set; }

        /// <summary>
        /// Optional base measure 3 data.
        /// </summary>
        public decimal? BaseMeasure3Data { get; set; }

        /// <summary>
        /// Optional base measure 4 name.
        /// </summary>
        public string BaseMeasure4Name { get; set; }

        /// <summary>
        /// Optional base measure 4 data.
        /// </summary>
        public decimal? BaseMeasure4Data { get; set; }

        /// <summary>
        /// Optional base measure 5 name.
        /// </summary>
        public string BaseMeasure5Name { get; set; }

        /// <summary>
        /// Optional base measure 5 data.
        /// </summary>
        public decimal? BaseMeasure5Data { get; set; }

        /// <summary>
        /// Programs business area.
        /// </summary>
        public string BusinessArea { get; set; }
        
        /// <summary>
        /// Programs line of business.
        /// </summary>
        public string LineOfBusiness { get; set; }

        /// <summary>
        /// Measure Group Name.
        /// </summary>
        public string MeasureGroupName { get; set; }

        /// <summary>
        /// Measure Category Name.
        /// </summary>
        public string MeasureCategoryName { get; set; }

        /// <summary>
        /// URL to the measure in MST PMM product.
        /// </summary>
        public string MeasureLink { get; set; }

        /// <summary>
        /// Combination of MeasureValidationDate and MeasureValidatedBy for display in the UI.
        /// </summary>
        public string MeasureValidation
        {
            get
            {
                string validation = string.Empty;
                if (MeasureValidationDate  != null)
                {
                    validation = MeasureValidationDate.Value.ToString("d");
                    if (!string.IsNullOrEmpty(MeasureValidatedBy))
                    {
                        validation = validation + " " + MeasureValidatedBy;
                    }
                }
                return validation;
            }
        }

        /// <summary>
        /// Date the measure was validated.  No date implies it is not validated
        /// </summary>
        public DateTime? MeasureValidationDate { get; set; }

        /// <summary>
        /// User ID of person that validated this.  Or PMM if done automatically.
        /// </summary>
        public string MeasureValidatedBy { get; set; }

        /// <summary>
        /// Description of the program owning the metric.
        /// </summary>
        public string ProgramDescription { get; set; }

        /// <summary>
        /// The date a this metric was added to a task element. This field is only populated on a call to the loaders GetByTaskElementIds method.
        /// </summary>
        public DateTime? DateAddedToTaskElement { get; set; }

        /// <summary>
        /// Help link to MST measure detail.
        /// </summary>
        public string MSTMetricDetailHelpLink { get; set; } 
    }
}
