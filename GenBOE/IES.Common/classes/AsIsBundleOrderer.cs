// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.Collections.Generic;
    using System.Web.Optimization;

    /// <summary>
    /// This class forces the bundle to be ordered as it appears instead of Microsoft re-ordering it as they please.
    /// </summary>
    /// <seealso cref="IBundleOrderer" />
    public class AsIsBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}
