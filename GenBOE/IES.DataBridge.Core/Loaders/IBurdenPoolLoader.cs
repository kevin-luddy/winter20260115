// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System.Collections.Generic;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Models;
	using IES.DataBridge.ModelViews;

	/// <summary>
	/// Inteface for Burden Pool Loader
	/// </summary>
	/// <seealso cref="IES.Common.Core.IDataLoader{IES.DataBridge.ModelViews.BurdenPoolDetailModelView}" />
	public interface IBurdenPoolLoader : IDataLoader<BurdenPoolDetailModelView>
    {
        /// <summary>
        /// Gets all Commercial and Government Burden Pools as options lists.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="commercialBurdenPoolOptions">Out parameter for returning Commercial Burden Pool options.</param>
        /// <param name="governmentBurdenPoolsOptions">Out parameter for returning Government Burden Pool options.</param>
        void GetBurdenPoolOptions(int revisionId, out ICollection<OptionModelView> commercialBurdenPoolOptions, out ICollection<OptionModelView> governmentBurdenPoolsOptions);

        /// <summary>
        /// BurdenPoolGridModelView GetByRevision
        /// </summary>
        /// <param name="revisionId">int</param>
        /// <returns>BurdenPoolGridModelView</returns>
        BurdenPoolGridModelView GetByRevision(int revisionId);

        /// <summary>
        /// Save collection of BurdenPools. All pools are passed in, then all reference data
        /// can be retrieved in one DB call to get RateCodes and BurdenElements.
        /// </summary>
        /// <param name="dtosIn">BurdenPoolDetailModelView</param>
        /// <param name="revisionId">Should be the WIP revision.</param>
        void SaveBurdenPools(ICollection<BurdenPoolDetailModelView> dtosIn, int revisionId);
    }
}
