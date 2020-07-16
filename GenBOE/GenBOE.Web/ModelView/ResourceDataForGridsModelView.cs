// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using IES.Common;

    public class ResourceDataForGridsModelView
    {
        public ResourceDataForGridsModelView()
        {
            ResourceID = 0;
            ElementOfCost = ElementOfCostType.NotSet;
            ResourceDescription = string.Empty;
            ResourceCode = string.Empty;
            Company = string.Empty;
            ResourceRate = string.Empty;
        }

        public int ResourceID { get; set; }
        public ElementOfCostType ElementOfCost { get; set; }
        public RateType RateType { get; set; }
        public string Company { get; set; }
        public string ResourceRate { get; set; }
        public string ResourceDescription { get; set; }
        public string ResourceCode { get; set; }
        // this gets sent to Labor Spread Grid
        public string SpreadText { get; set; }
        // this gets sent to BOE Summary
        public string BOESummaryText { get; set; }
    }

    public class ResourceDataForGridsDescriptionModelView
    {
        public ResourceDataForGridsDescriptionModelView()
        {
            ResourceDescription = string.Empty;
            ElementOfCost = ElementOfCostType.NotSet;
        }

        public string ResourceDescription { get; set; }
        public ElementOfCostType ElementOfCost { get; set; }
    }
}
