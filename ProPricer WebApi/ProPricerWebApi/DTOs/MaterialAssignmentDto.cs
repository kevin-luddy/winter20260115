/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System.Collections.Generic;

namespace APTSPropricerApi.DTOs
{
    public class MaterialAssignmentDto
    {
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PartName { get; set; }
        public string PartDescription { get; set; }
        public string MakeBuy { get; set; }
        public string UnitQty { get; set; }
        public string ShipQty { get; set; }
        public string TotalMfgStartQty { get; set; }
        public string TotalCost { get; set; }
        public string UnitCost { get; set; }
        public ResourceAssignmentDto ResourceAssignment { get; set; }
        public IEnumerable<AssociatedCostsDto> AssociatedCosts { get; set; }
    }
}