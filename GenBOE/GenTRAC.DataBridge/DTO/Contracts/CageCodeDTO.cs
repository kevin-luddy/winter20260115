// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Cage Codes DTO
    /// </summary>
    [Serializable]
    public class CageCodeDTO : IES.Common.UpdateableDTO
    {
        /// <summary>
        /// Cage Code
        /// </summary>
        public string CageCode { get; set; }
        
        /// <summary>
        /// Cage Code Address 1
        /// </summary>
        public string Address1 { get; set; }
        
        /// <summary>
        /// Cage Code Address 2
        /// </summary>
        public string Address2 { get; set; }
        
        /// <summary>
        /// City of Address
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// State of Address
        /// </summary>
        public string State { get; set; }
        
        /// <summary>
        /// Zip of Address
        /// </summary>
        public string Zip { get; set; }
    }
}
