// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CustomFields
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    public class RestoreOptionData
    {
        public RestoreOptionData()
        {
            this.OptionAdded = new Collection<RestoreOption>();
            this.OptionChanged = new Collection<RestoreOptionChanged>();
            this.OptionDeleted = new Collection<RestoreOption>();
            this.OptionNotChanged = new Collection<RestoreOption>();
        }

        public Collection<RestoreOption> OptionAdded { get; set; }
        public Collection<RestoreOptionChanged> OptionChanged { get; set; }
        public Collection<RestoreOption> OptionDeleted { get; set; }
        public Collection<RestoreOption> OptionNotChanged { get; set; }
    }

    public class RestoreOption : UpdateableDTO
    {
        public RestoreOption()
        {
            this.ID = -1;
            this.Name = string.Empty;
            this.Desc = string.Empty;
            this.SegRegion = string.Empty;
            this.LaborType = string.Empty;
            this.ElementOfCostName = string.Empty; 
        }

		/// <summary>
		/// Perf Org/Resource ID
		/// </summary>
        public int ID { get; set; }

		/// <summary>
		/// Perf Org/Resource Name
		/// </summary>
        public string Name { get; set; }

		/// <summary>
		/// Perf Org/Resource Description
		/// </summary>
        public string Desc { get; set; }

		/// <summary>
		/// Resource Segment Region
		/// </summary>
        public string SegRegion { get; set; }

		/// <summary>
		/// Resource Labor Type
		/// </summary>
        public string LaborType { get; set; }

		/// <summary>
		/// Resource Element of Cost Name
		/// </summary>
        public string ElementOfCostName { get; set; }

		/// <summary>
		/// Resource Element of Cost
		/// </summary>
        public ElementOfCostType ElementofCost { get; set; }

		/// <summary>
		/// Resource Rate Type
		/// </summary>
		public RateType RateType { get; set; }
	}

    public class RestoreOptionChanged : UpdateableDTO
    {
        public RestoreOptionChanged()
        {
            this.ID = -1;
            this.Name = string.Empty;
            this.Desc = string.Empty;
            this.SegRegion = string.Empty;
            this.LaborType = string.Empty;
            this.ElementOfCostName = string.Empty;
            this.ChangedToName = string.Empty;
            this.ChangedToDesc = string.Empty;
            this.ChangedToSegRegion = string.Empty;
            this.ChangedToLaborType = string.Empty;
            this.ChangedToElementOfCostName = string.Empty;
        }

		/// <summary>
		/// Perf Org/Resource ID
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Perf Org/Resource original Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Perf Org/Resource original description
		/// </summary>
		public string Desc { get; set; }

		/// <summary>
		/// Resource original segment region
		/// </summary>
        public string SegRegion { get; set; }

		/// <summary>
		/// Resource original labor type
		/// </summary>
		public string LaborType { get; set; }

		/// <summary>
		/// Resource original element of cost name
		/// </summary>
		public string ElementOfCostName { get; set; }

		/// <summary>
		/// Resource original element of cost
		/// </summary>
		public ElementOfCostType ElementOfCost { get; set; }

		/// <summary>
		/// Resource original rate type
		/// </summary>
		public RateType RateType { get; set; }

		/// <summary>
		/// Perf Org/Resource changed-to name
		/// </summary>
		public string ChangedToName { get; set; }

		/// <summary>
		/// Perf Org/Resource changed-to description
		/// </summary>
		public string ChangedToDesc { get; set; }

		/// <summary>
		/// Resource changed-to segment region
		/// </summary>
		public string ChangedToSegRegion { get; set; }

		/// <summary>
		/// Resource changed-to labor type
		/// </summary>
		public string ChangedToLaborType { get; set; }

		/// <summary>
		/// Resource changed-to element of cost name
		/// </summary>
		public string ChangedToElementOfCostName { get; set; }

		/// <summary>
		/// Resource changed-to element of cost
		/// </summary>
		public ElementOfCostType ChangedToElementOfCost { get; set; }

		/// <summary>
		/// Resource changed-to rate type
		/// </summary>
		public RateType ChangedToRateType { get; set; }
	}   
}
