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
        Logger _log = new Logger(typeof(BOEHistoryController));

        BOEHistoryDTODataLoader _boeHistoryDataLoader = null;

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
        public ViewResult DisplayBOEHistory(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEHistory", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

            // Perform Action
            Collection<BOEHistoryModelView> theModelViews = new Collection<BOEHistoryModelView>();
            Collection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(new List<int>() { boeID });
            IDictionary<int, FieldTypeModelView> allFieldTypes = _CommonDataMapper.getFieldTypesDictionary();

            foreach (BOEHistoryDTO boeHistoryDTO in _boeHistoryDataLoader.GetBOEHistory(boeID))
            {
                BOEHistoryModelView mv = new BOEHistoryModelView
                {
                    Field = allFieldTypes[(int)boeHistoryDTO.Field].FieldTypeName,
                    NewValue = boeHistoryDTO.NewValue,
                    OldValue = boeHistoryDTO.OldValue,
                    PerformedBy = this.UserLoader.GetUserByID(boeHistoryDTO.PerformedByETIUserId).DisplayName,
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
                    mv.OldValue = boeHistoryDTO.OldValue != null ? this.UserLoader.GetUserByID(Convert.ToInt32(boeHistoryDTO.OldValue)).DisplayName : string.Empty;
                    mv.NewValue = boeHistoryDTO.NewValue != null ? this.UserLoader.GetUserByID(Convert.ToInt32(boeHistoryDTO.NewValue)).DisplayName : string.Empty;

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
            FinalizeAction(_log, "DisplayBOEHistory", sw);
            return toReturn;
        }
    }
}
