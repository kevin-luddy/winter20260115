// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using IES.Common.Core.Models;

	/// <summary>
	/// The Model View used for Burden Element to Rate Code mapping values.
	/// </summary>
	public class BurdenElementRateCodeMappingModelView : UpdateableDTO
    {
        /// <summary>
        /// Gets or sets the burden element.
        /// </summary>
        public BurdenElementModelView BurdenElement { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        public string RateCode { get; set; }
    }
}
