// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System.Collections.Generic;
	using IES.Common.Core.Loaders;
	using IES.DataBridge.ModelViews;

	/// <summary>
	/// Interface for Banner Loader
	/// </summary>
	public interface IRateCodeReplicationLoader : IDataLoader<RateCodeModelView>
    {
        /// <summary>
        /// Get all Rate Code Replications.
        /// </summary>
        /// <returns>All Rate Code Replications.</returns>
        ICollection<RateCodeModelView> GetAll();
    }
}
