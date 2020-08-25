// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using GenBOE.ActionLogic.ModelView;

    public class GettingStartedHelpModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default Constructor. Required for an MVC action.
        /// </summary>
        public GettingStartedHelpModelView()
        {
        }
        
        /// <summary>
        /// When true, hides the help menu items on the home page.
        /// </summary>
        public bool HideGettingStartedHelp { get; set; }       
    }
}
