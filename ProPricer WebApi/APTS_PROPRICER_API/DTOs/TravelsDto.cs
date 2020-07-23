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
    public class TravelsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Destination { get; set; }
        public string DestinationDescription { get; set; }
        public string Comments { get; set; }
        public int People { get; set; }
        public string Days { get; set; }
        public int Trips { get; set; }
        public string TripCost { get; set; }
        public string TotalCost { get; set; }
        public ResourceAssignmentDto ResourceAssignment { get; set; }
        public IEnumerable<TravelExpenseDto> Expenses { get; set; }
    }
}