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
    public class ResourceAssignmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InfoDescription { get; set; }
        public IEnumerable<ResourceFieldsDto> ResourceFields { get; set; }
        public string SourceType { get; set; }
        public string Amount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SpreadCurve { get; set; }
        public IEnumerable<SpreadDto> Spread { get; set; }
        public string DirectCost { get; set; }
        public string Price { get; set; }
        public IEnumerable<BurdenCostDto> BurdenCost { get; set; }
    }
}