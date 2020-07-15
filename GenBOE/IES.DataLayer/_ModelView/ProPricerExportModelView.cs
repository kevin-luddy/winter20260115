// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Text;
    
    /// <summary>
    /// The Model View used for a ProPricer Export Row.
    /// </summary>
    public abstract class ProPricerExportModelView : IProPricerExportModelView
    {
        #region Constants

        /// <summary>
        /// double quotes are needed around certain fields for the export
        /// </summary>
        protected const string DOUBLE_QUOTE = "\u0022";

        /// <summary>
        /// a comma has to follow each field in the file row
        /// </summary>
        protected const string END_FIELD = ",";

        /// <summary>
        /// Double quotes surrounding a comma (end-field terminator)
        /// </summary>
        protected const string DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE = DOUBLE_QUOTE + END_FIELD + DOUBLE_QUOTE;

        #endregion Constants

        /// <summary>
        /// Generates string representation of this ModelView object to attach to the StringBuilder.
        /// </summary>
        /// <param name="stringBuilder">A stringbuilder to append this objects representation into.</param>
        public abstract void ExportString(StringBuilder stringBuilder);
    }
}