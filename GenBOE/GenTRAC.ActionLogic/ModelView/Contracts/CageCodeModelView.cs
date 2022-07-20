// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Contracts
{
    /// <summary>
    /// Cage Code View Model
    /// </summary>
	public class CageCodeModelView
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
