// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System.Collections.Generic;
	using System.Linq;
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using IES.Common.Core;
	using Microsoft.Extensions.Logging;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Utilities;

	/// <summary>
	/// Rate Grid Loader
	/// </summary>
	public class RateConfigLoader : IRateConfigLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; set; }

        #region constructors
        /// <summary>
        /// Rate Config Loader Constructor
        /// </summary>
        public RateConfigLoader(ILogger<RateConfigLoader> logger)
		{
			this.Log = logger;
		}
        #endregion

        /// <summary>
        /// Gets the Rate Format Configuration data.
        /// </summary>
        /// <returns>All rate format configuration rows.</returns>
        public ICollection<RateConfigModelView> GetAll()
        {
            ICollection<RateConfigModelView> rateConfigs;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    rateConfigs = context.RateConfigs
                    .Select(r =>
                    new RateConfigModelView()
                    {
                        Id = r.ID,
                        RateTarget = r.RateTarget,
                        RateCategory = (RateCategory)r.CategoryID,
                        Prefix = r.Prefix,
                        Suffix = r.Suffix,
                        Precision = r.Precision,
                        Multiplier = r.Multiplier
                    }).ToList();
                }
            }

            return rateConfigs;
        }
    }
}