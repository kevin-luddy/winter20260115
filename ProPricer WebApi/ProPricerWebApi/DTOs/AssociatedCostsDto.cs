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
    public class AssociatedCostsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ResourcesDto Resource { get; set; }
        public IEnumerable<ResourceFieldsDto> ResourceFields { get; set; }
        public double Amount { get; set; }
        public double? TotalAmount { get; set; }
        public bool LinkQty { get; set; }
        public bool LinkSpread { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SpreadCurve { get; set; }
        public IEnumerable<SpreadDto> Spread { get; set; }
    }
}