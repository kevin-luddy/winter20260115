// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using IES.Common;

    /// <summary>
    /// Controller for DateShift
    /// </summary>
    /// <seealso cref="GenBOE.Web.Common.GenBOEController" />
    public class RTETemplatesController : GenBOEController
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly Logger logger = new Logger(typeof(RTETemplatesController));

        /// <summary>
        /// Controller Logic
        /// </summary>
        private readonly IRTETemplatesControllerLogic controllerLogic;

        /// <summary>
        /// Initializes a new instance of the <see cref="RTETemplatesController" /> class.
        /// </summary>
        /// <param name="securityAccess">The security access.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="siteMasterUtilities">The site master utilities.</param>
        /// <param name="systemMetrics">The system metrics.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="userLoader">The user loader.</param>
        /// <param name="permissionsLoader">The permissions loader.</param>
        /// <param name="controllerLogic">The controller logic.</param>
        public RTETemplatesController(ISecurityAccess securityAccess,
            ICommonDataMapper commonDataMapper,
            SiteMasterUtilities siteMasterUtilities,
            SystemMetrics systemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionsLoader,
            IRTETemplatesControllerLogic controllerLogic)
            : base(securityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, factory, userLoader,
                 permissionsLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
        }

        /// <summary>
        /// The initial view for performing a DateShift
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        [HttpGet]
		public ActionResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.Read, ws, null);

            ViewResult toReturn = GetMasterView(WebConstants.VIEW_MANAGE_RTE_TEMPLATES, workspace);
            ViewData["ContainsOCI"] = ws.ContainsOCI;
            ViewData["WorkspaceId"] = ws.Id;
            ViewData["TemplateSources"] = this.controllerLogic.GetSources(ws.UsingTemplateBOE);

            FinalizeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Save the templates
        /// </summary>
        /// <param name="workspace">The Workspace</param>
        /// <param name="templates">Templates being saved</param>
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <param name="moveToPrompt">Id of prompt to move deleted Prompt data to</param>
        /// <returns></returns>
        [HttpPost]
		public JsonResult SaveRTETemplatesModel(string workspace, ICollection<RteCustomTemplateModelView> templates, bool moveDeletedPromptData, int? moveToPrompt)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_SAVE_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            this.controllerLogic.ValidateTemplates(templates, ws.Id, moveDeletedPromptData, moveToPrompt);
            this.controllerLogic.SaveTemplates(templates, ws, moveDeletedPromptData, moveToPrompt);

            // Finalize Action
            this.FinalizeAction(this.logger, WebConstants.ACTION_SAVE_RTE_TEMPLATES, sw);
            return this.Json(new { Status = true });
        }

		/// <summary>
		/// Search templates
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="search">Search Text</param>
		/// <returns>Matching results</returns>
		[HttpPost]
		public JsonResult SearchTemplates(string workspace, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) { throw new ArgumentNullException(nameof(search)); }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_SEARCH_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            ICollection<RteCustomTemplateModelView> templates = this.controllerLogic.SearchTemplates(search);

            this.FinalizeAction(this.logger, WebConstants.ACTION_SEARCH_RTE_TEMPLATES, sw);
            return this.Json(templates);
        }

		/// <summary>
		/// Copy Templates
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="templateId">Template To Copy</param>
		/// <param name="newTemplateName">New Template Name</param>
		/// <returns>Success / Failure</returns>
		[HttpPost]
		public JsonResult CopyTemplate(string workspace, int templateId, string newTemplateName)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_COPY_RTE_TEMPLATE, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            this.controllerLogic.CopyTemplate(templateId, ws, newTemplateName);

            FinalizeAction(this.logger, WebConstants.ACTION_COPY_RTE_TEMPLATE, sw);
            return this.Json(new { Status = true });
        }

		/// <summary>
		/// Gets the RTE Templates Model for this workspace
		/// </summary>
		/// <param name="workspace">The workspace to retrieve data from.</param>
		/// <returns>The model for this workspace.</returns>
		[HttpPost]
		public JsonResult GetRTETemplatesModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_GET_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.Read, ws, null);

            ICollection<RteCustomTemplateModelView> templates = this.controllerLogic.GetTemplates(ws.Id);

            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, sw);
            return this.Json(templates);
        }
    }
}