// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GenBOE.Dtos
{
    /// <summary>
    /// DTO used to display search criteria for MST metrics.
    /// </summary>
    public class MSTMetricSearchCriteriaDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTMetricSearchCriteriaDTO()
        {
            this.Programs = new Collection<Program>();
            this.MeasureFunctions = new Collection<MeasureFunction>();
            this.MeasureNames = new Collection<Measure>();
            this.MeasureQualifiers = new Collection<MeasureQualifier>();
            this.DataSources = new Collection<DataSource>();
        }

        /// <summary>
        /// Complete list of MST programs.
        /// </summary>
        public ICollection<Program> Programs { get; set; }

        /// <summary>
        /// Complete list of MST measure functions.
        /// </summary>
        public ICollection<MeasureFunction> MeasureFunctions { get; set; }

        /// <summary>
        /// Complete list of MST measure names.
        /// </summary>
        public ICollection<Measure> MeasureNames { get; set; }

        /// <summary>
        /// Complete list of MST measure qualifiers.
        /// </summary>
        public ICollection<MeasureQualifier> MeasureQualifiers { get; set; }

        /// <summary>
        /// Complete list of Data Sources.
        /// </summary>
        public ICollection<DataSource> DataSources { get; set; }     
    }

    /// <summary>
    /// Represents MST programs.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Program Name
        /// </summary>
        public string ProgramName { get; set; }
    }

    /// <summary>
    /// Represents MST measures functions.
    /// </summary>
    public class MeasureFunction
    {
        /// <summary>
        /// Measure Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Measure Name
        /// </summary>
        public string MeasureFunctionName { get; set; }
    }

    /// <summary>
    /// Represents MST measures names.
    /// </summary>
    public class Measure
    {
        /// <summary>
        /// Measure Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Measure Name
        /// </summary>
        public string MeasureName { get; set; }
    }

    /// <summary>
    /// Represents MST Data Sources.
    /// </summary>
    public class DataSource
    {
        /// <summary>
        /// Measure Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Measure Name
        /// </summary>
        public string DataSourceName { get; set; }
    }

    /// <summary>
    /// Represents MST Measure qualifiers (e.g. CDR, PDR, SRR, etc).
    /// </summary>
    public class MeasureQualifier
    {
        /// <summary>
        /// Measure Id. Is the same as the measure qualifier.
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// Measure Name
        /// </summary>
        public string MeasureQualifierDescription { get; set; }
    }    
}
