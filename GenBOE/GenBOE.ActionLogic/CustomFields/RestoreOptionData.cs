// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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

        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string SegRegion { get; set; }
        public string LaborType { get; set; }
        public string ElementOfCostName { get; set; }
        public ElementOfCostType ElementofCost { get; set; }
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
            this.ChangedToID = string.Empty;
            this.ChangedToDesc = string.Empty;
            this.ChangedToSegRegion = string.Empty;
            this.ChangedToLaborType = string.Empty;
            this.ChangedToElementOfCostName = string.Empty;
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string SegRegion { get; set; }
        public string LaborType { get; set; }
        public string ElementOfCostName { get; set; }
        public ElementOfCostType ElementOfCost { get; set; }
        public string ChangedToID { get; set; }
        public string ChangedToDesc { get; set; }
        public string ChangedToSegRegion { get; set; }
        public string ChangedToLaborType { get; set; }
        public string ChangedToElementOfCostName { get; set; }
        public ElementOfCostType ChangedElementOfCost { get; set; }
    }

   
}
