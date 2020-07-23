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
    /// <summary>
    /// Data Transfer Object for General Proposal Data
    /// </summary>
    public class ProposalDto
    {
        //General proposal screen fields 
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string CreatorName { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string ParentFolderName { get; set; }
        public string Manager { get; set; }
        public string BusinessUnit { get; set; }
        public string Rfq { get; set; }
        public string StartDate { get; set; } //TimeFrame in PROPRICER
        public string EndDate { get; set; } //TimeFrame in PROPRICER
        public int NumberMonths { get; set; }
        public string DueDate { get; set; } //TimeFrame in PROPRICER 
        public byte? ResourceDecimalPercision { get; set; }
        public string AwardProbability { get; set; }
        public string TargetPrice { get; set; }

        public string GlobalProfitFactor { get; set; }

        //Pricing screens fields
        public string DirectRateTable { get; set; }
        public string BurdenRateTable { get; set; }
        public string TravelRateTable { get; set; }

        public string FactorRateTable { get; set; }

        //Summary fields screen
        public string TaskIdLabel { get; set; }

        public IEnumerable<SummaryFieldDefinitionsDto> SumFieldDefs { get; set; }

        //Notes screen 
        public string Notes { get; set; }

        //Calendar screen  
        public int FiscalYearStartMonthOffset { get; set; }

        //Reports – footer screen
        public string RptFooter { get; set; }

        // Array of tasks – only used to collect all tasks (for TaskDto breakout see Task Controller)
        public IEnumerable<TaskDto> Tasks { get; set; }
    }
}