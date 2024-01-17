// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
        protected const char DOUBLE_QUOTE = '\u0022';

        /// <summary>
        /// a comma has to follow each field in the file row
        /// </summary>
        protected const char END_FIELD = ',';

		/// <summary>
		/// Double quotes surrounding a comma (end-field terminator)
		/// </summary>
		protected const string DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE = "\u0022,\u0022";

        #endregion Constants

        /// <summary>
        /// Generates string representation of this ModelView object to attach to the StringBuilder.
        /// </summary>
        /// <param name="stringBuilder">A stringbuilder to append this objects representation into.</param>
        public abstract void ExportString(StringBuilder stringBuilder);
    }
}