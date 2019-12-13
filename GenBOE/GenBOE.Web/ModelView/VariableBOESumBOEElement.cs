namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Model view to represent nested rows in the VariableBOESumByCLIN view.
    /// </summary>
    public class VariableBOESumBOEElement
    {
        public VariableBOESumBOEElement()
        {
            BOEID = -1;
            WBSTitle = string.Empty;
            CLINTitle = string.Empty;
            BOEStatus = string.Empty;
            BOETotal = 0;
            Disabled = false;
        }

        /// <summary>
        /// The BOE ID. Will be hidden.
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// WBS Number and Title.
        /// </summary>
        public string WBSTitle { get; set; }

        /// <summary>
        /// CLIN Number and Title.
        /// </summary>
        public string CLINTitle { get; set; }

        /// <summary>
        /// The current status of the BOE.
        /// </summary>
        public string BOEStatus { get; set; }

        /// <summary>
        /// The total of all labor under this BOE.
        /// </summary>
        public decimal BOETotal { get; set; }

        /// <summary>
        /// Specifies whether the BOE should be disabled in the View, usually due to it introducing
        /// a circular reference into the system.
        /// </summary>
        public bool Disabled { get; set; }
    }
}