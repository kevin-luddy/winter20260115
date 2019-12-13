using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Model View to represent summary rows in the VariableBOESumByCLIN View.
    /// </summary>
    public class VariableBOESumByWBSModelView
    {
        public VariableBOESumByWBSModelView()
            : base()
        {
            WBSID = -1;
            WBSLevel = 0;
            WBSNumber = string.Empty;
            WBSName = string.Empty;
            WBSTotal = 0;
            BOEs = new Collection<VariableBOESumBOEElement>();
            WorkspaceDecimalPrecision = 0;
        }
        
        /// <summary>
        /// The ID of the WBS for this row in the table. This will be hidden.
        /// </summary>
        public int WBSID { get; set; }

        /// <summary>
        /// The level of nesting (padding) for this WBS in the table.
        /// </summary>
        public int WBSLevel { get; set; }

        /// <summary>
        /// The WBS Number to display as a summary item of several WBSs and BOEs.
        /// </summary>
        public string WBSNumber { get; set; }

        /// <summary>
        /// The WBS Name to display as a summary item of several WBSs and BOEs.
        /// </summary>
        public string WBSName { get; set; }

        /// <summary>
        /// Return the SUM of all BOEs associated with this WBS.
        /// </summary>
        public decimal WBSTotal { get; set; }

        /// <summary>
        /// All BOEs that are directly associated with this WBS.
        /// </summary>
        public Collection<VariableBOESumBOEElement> BOEs { get; set; }

        /// <summary>
        /// Decimal Precision for the workspace to be used by the variable
        /// </summary>
        public int? WorkspaceDecimalPrecision { get; set; }
    }
}