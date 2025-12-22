// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.classes;

    public class BOEHistoryController : GenBOEController
    {
		private readonly Logger _log = new Logger(typeof(BOEHistoryController));

		private readonly BOEHistoryDTODataLoader _boeHistoryDataLoader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEHistoryController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper, SiteMasterUtilities inSiteMasterUtilities,
            BOEHistoryDTODataLoader inBoeHistoryDataLoader, UserDTODataLoader inUserDTODataLoader,
            SystemMetrics inSystemMetrics, IFullObjectFactory factory, IPermissionsDTODataLoader inPermissionLoader, 
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, inPermissionLoader, inControllerLogic)
        {
            _boeHistoryDataLoader = inBoeHistoryDataLoader;
        }

        /// <summary>
        /// Displays the BOE History view
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <returns></returns>
        [HttpPost]
		public ViewResult DisplayBOEHistory(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HISTORY, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            // Perform Action
            Collection<BOEHistoryModelView> theModelViews = new Collection<BOEHistoryModelView>();
            Collection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(new List<int>() { boeID });
            IDictionary<int, FieldTypeModelView> allFieldTypes = _CommonDataMapper.getFieldTypesDictionary();

			ICollection<BOEHistoryDTO> boeHistories = _boeHistoryDataLoader.GetBOEHistory(boeID);
			ICollection<int> userIds = boeHistories.Select(x => x.PerformedByETIUserId).ToList();

			// pull out the old and new values for user ids
			ICollection<BOEHistoryDTO> oldNewValueHistories = boeHistories.Where(b => b.Field == FieldType.Author || b.Field == FieldType.Approver || b.Field == FieldType.SubcontractorAuthor).ToList();
			foreach (BOEHistoryDTO history in oldNewValueHistories)
			{
				if (history.OldValue != null && int.TryParse(history.OldValue, out int oldUserId))
				{
					userIds.Add(oldUserId);
				}

				if (history.NewValue != null && int.TryParse(history.NewValue, out int newUserId))
				{
					userIds.Add(newUserId);
				}
			}
			
			userIds = userIds.Distinct().ToList();
			ICollection<UserDTO> users = this.UserLoader.GetByIds(userIds);
			// Create Dictionary for easier string comparison
			Dictionary<string, UserDTO> userDictionary = users.ToDictionary(u => u.UserID.ToString());

			foreach (BOEHistoryDTO boeHistoryDTO in boeHistories)
            {
                BOEHistoryModelView mv = new BOEHistoryModelView
                {
                    Field = allFieldTypes[(int)boeHistoryDTO.Field].FieldTypeName,
                    NewValue = boeHistoryDTO.NewValue,
                    OldValue = boeHistoryDTO.OldValue,
                    PerformedBy = users.FirstOrDefault(u => u.UserID == boeHistoryDTO.PerformedByETIUserId)?.DisplayName ?? string.Empty,
                    Timestamp = boeHistoryDTO.Date
                };

                if ((from p in boePermissions
                    where p.Role==Role.SubcontractorAuthor && p.ETIUserId == boeHistoryDTO.PerformedByETIUserId
                    select p).Any())
                {
                    mv.PerformedBy += CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX;  // for display purposes, append (Sub) to all Subcontractor Names
                }

                if (boeHistoryDTO.Field == FieldType.Author || boeHistoryDTO.Field == FieldType.Approver || boeHistoryDTO.Field == FieldType.SubcontractorAuthor)
                {
                    mv.OldValue = boeHistoryDTO.OldValue != null ? userDictionary[boeHistoryDTO.OldValue].DisplayName : string.Empty;
                    mv.NewValue = boeHistoryDTO.NewValue != null ? userDictionary[boeHistoryDTO.NewValue].DisplayName : string.Empty;

                    if (!String.IsNullOrEmpty(mv.NewValue))     // for display purposes, append (Sub) to all Subcontractor Names
                    {
                        if ((from p in boePermissions 
                            where p.Role==Role.SubcontractorAuthor && p.ETIUserId == Convert.ToInt32(boeHistoryDTO.NewValue)
                            select p).Any())

                        {
                            mv.NewValue += CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX;
                        }
                    }
                    if (!String.IsNullOrEmpty(mv.OldValue))
                    {
                        if ((from p in boePermissions 
                            where p.Role==Role.SubcontractorAuthor && p.ETIUserId == Convert.ToInt32(boeHistoryDTO.OldValue)
                            select p).Any())
                        {
                            mv.OldValue += CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX;
                        }
                    }

                }

                theModelViews.Add(mv);
            }

            ViewResult toReturn = View(WebConstants.VIEW_BOE_HISTORY_GRID, theModelViews);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_HISTORY, sw);
            return toReturn;
        }
    }
}
