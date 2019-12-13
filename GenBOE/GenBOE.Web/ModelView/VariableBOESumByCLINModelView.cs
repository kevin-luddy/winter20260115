using System.Collections.ObjectModel;
using System.Linq;

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Model View to represent summary rows in the VariableBOESumByCLIN View.
    /// </summary>
    public class VariableBOESumByCLINModelView
    {
        public VariableBOESumByCLINModelView()
            : base()
        {
            CLINID = -1;
            CLINTitle = string.Empty;
            BOEs = new Collection<VariableBOESumBOEElement>();
            WorkspaceDecimalPrecision = 0;
        }

        /// <summary>
        /// The ID of the CLIN for this row in the table. This will be hidden.
        /// </summary>
        public int CLINID { get; set; }

        /// <summary>
        /// The CLIN Title to display as a summary item of several BOEs. No padding in the view.
        /// </summary>
        public string CLINTitle { get; set; }

        /// <summary>
        /// Return the SUM of all BOEs associated with this CLIN.
        /// </summary>
        public decimal CLINTotal
        {
            get
            {
                decimal toReturn = 0;

                if (BOEs.Count > 0)
                {
                    toReturn = BOEs.Sum(b => b.BOETotal);
                }

                return toReturn;
            }
        }

        /// <summary>
        /// All BOEs that are associated with this CLIN.
        /// </summary>
        public Collection<VariableBOESumBOEElement> BOEs { get; set; }


        /// <summary>
        /// Decimal Precision for the workspace to be used by the variable
        /// </summary>
        public int? WorkspaceDecimalPrecision { get; set; }
    }
}