// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using IES.Common;

    /// <summary>
    /// Escalation Rates DTO at the Workspace Level
    /// </summary>
    public class WorkspaceRMSEscalationRatesDTO : UpdateableDTO
    {
        /// <summary>
        /// Public constructor.
        /// </summary>
        public WorkspaceRMSEscalationRatesDTO()
        {
            this.Id = -1;
            this.WorkspaceId = -1;
            this.AirfareRate = 0;
            this.PerDiemRate = 0;
            this.MiscRate = 0;
        }

        /// <summary>
        /// The workspace Id.
        /// </summary>
        public int WorkspaceId { get; set; }


        public int Year { get; set; }

        public decimal AirfareRate { get; set; }

        public decimal PerDiemRate { get; set; }

        public decimal MiscRate { get; set; }
    }
}
