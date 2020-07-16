// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using IES.Common;
    
    /// <summary>
    /// Mile Reimbursement Dto
    /// </summary>
    [Serializable()]
    public class MileReimbursementRateDTO : UpdateableDTO
    {

        public MileReimbursementRateDTO()
        {
            MileReimbursementRate = 0; ;
        }

        /// <summary>
        /// The rate to be updated
        /// </summary>
        public decimal MileReimbursementRate { get; set; }

    }
}