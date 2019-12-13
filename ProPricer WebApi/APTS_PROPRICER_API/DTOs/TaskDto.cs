/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System.Collections.Generic;

namespace APTSPropricerApi.DTOs
{
    /// <summary>
    /// Incomplete Data Transfer Object for Tasks
    /// </summary>
    public class TaskDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ActualFee { get; set; }
        public int Quantity { get; set; }
        public IEnumerable<ResourceAssignmentDto> ResourceAssignments { get; set; }
        public IEnumerable<MaterialAssignmentDto> MaterialAssignments { get; set; }
        public IEnumerable<SummaryFieldsDto> SummaryFields { get; set; }
        public IEnumerable<TravelsDto> Travels { get; set; }
    }
}