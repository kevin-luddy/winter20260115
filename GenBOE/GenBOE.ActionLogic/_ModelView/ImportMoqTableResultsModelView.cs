// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    public class ImportMoqTableResultsModelView : MoqTableData
    {
        public ImportMoqTableResultsModelView()
            :base ()
        {
        }

        /// <summary>
        /// Import type - either create or error type
        /// </summary>
        public int ImportType { get; set; }
    }
}
