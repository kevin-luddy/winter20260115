// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Text;

    /// <summary>
    /// Interface for ProPricer Export Model Views.
    /// </summary>
    public interface IProPricerExportModelView
    {
        /// <summary>
        /// Generates string representation of this ModelView object to attach to the StringBuilder.
        /// </summary>
        /// <param name="stringBuilder">A stringbuilder to append this objects representation into.</param>
        void ExportString(StringBuilder stringBuilder);
    }
}
