using GenBOE.Objects;
using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.ActionLogic
{

    public interface IWBSControllerLogic
    {
        /// <summary>
        /// Saves updates to the ManageWBS page, Updates, inserts or deletes. 
        /// </summary>
        /// <param name="wbsCollection">Collection of WBS's to update</param>
        /// <param name="ws">the full workspace</param>
         void SaveManageWBS(Collection<ManageWBSModelView> wbsCollection, FullWorkspace ws);
    }
}
