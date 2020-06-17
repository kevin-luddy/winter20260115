// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class BoeWbsReportModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BoeWbsReportModelView()
        {
            BOETitle = string.Empty;
            WBSNumber = string.Empty;
            WBSTitle = string.Empty;
            TotalHours = 0;
            TotalCost = 0;
        }

        /// <summary>
        /// BOE Title
        /// </summary>
        public string BOETitle { get; set; }

        /// <summary>
        /// WBS Number for the BOE
        /// </summary>
        public string WBSNumber { get; set; }
        
        /// <summary>
        /// Title of the WBS for the BOE
        /// </summary>
        public string WBSTitle { get; set; }

        /// <summary>
        /// Total hours for the BOE
        /// </summary>
        public decimal TotalHours { get; set; }

        /// <summary>
        /// Total cost for the BOE
        /// </summary>
        public decimal TotalCost { get; set; }
    }
}
