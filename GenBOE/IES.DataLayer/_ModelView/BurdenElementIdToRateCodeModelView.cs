// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using IES.Common;

    /// <summary>
    /// The Model View is used in the BurdenPoolLoader to load the BurdenElementID to RateCode to avoid an additional DB call.
    /// </summary>
    [Serializable]
    public class BurdenElementIdToRateCodeModelView : UpdateableDTO
    {
        /// <summary>
        /// Gets or sets the burden element Id.
        /// </summary>
        public int BurdenElementId { get; set; }

        /// <summary>
        /// Gets or sets the rate code Id.
        /// </summary>
        public int RateCodeId { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        public string RateCode { get; set; }
    }
}
