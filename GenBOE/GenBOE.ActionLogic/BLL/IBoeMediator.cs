// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BLL
{
    using System.Collections.Generic;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public interface IBoeMediator
    {
        /// <summary>
        /// Saves BOEs.
        /// 
        /// NOTE: this method requires callers to pass in the original boes. it helps with performance.
        /// </summary>
        /// <param name="workspace">Workspace that owns the inBOECollection</param>
        /// <param name="inModifiedBOECollection">BOEs to modify</param>
        /// <returns>Dictionary where key = old boe id and value = new boe id</returns>
        IDictionary<int, int> MediatedSaveBOEs(FullWorkspace workspace, ICollection<BoeDTO> inModifiedBOECollection);

        /// <summary>
        /// Saves BOEs.
        /// </summary>
        /// <param name="workspace">FullWorkspace object containing the Boe.</param>
        /// <param name="inModifiedBoeDTO">Boe to be updated.</param>
        /// <returns>Dictionary of old Boe Id to new Boe Id.</returns>
        IDictionary<int, int> MediatedSave(FullWorkspace workspace, BoeDTO inModifiedBoeDTO);

        /// <summary>
        /// When saving the BOE Header (description, sources of data, BOE level custom fields), do not go through the MediatedSaves
        /// There is no reason these updates will effect rates, recalculations, etc. Also, we do not want to go through the normal BOE
        /// Clear Cache method since we will be taking an unnecessary hit of clearing out keys that aren't applicable for this type of a save
        /// Every other BOE Save should follow the process of using the MediatedSaves
        /// </summary>
        /// <param name="inboe"></param>
        void SaveEditBoeHeader(BoeDTO inboe);
    }
}
