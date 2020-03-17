// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
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
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

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

        private readonly IRteTemplateDataLoader rteTemplateDataLoader;

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
        /// <param name="rteTemplateDataLoader">The Template Loader.</param>
        public RTETemplatesController(ISecurityAccess securityAccess,
            ICommonDataMapper commonDataMapper,
            SiteMasterUtilities siteMasterUtilities,
            SystemMetrics systemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionsLoader,
            IGenBOEControllerLogic controllerLogic,
            IRteTemplateDataLoader rteTemplateDataLoader)
            : base(securityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, factory, userLoader,
                 permissionsLoader, controllerLogic)
        {
            this.rteTemplateDataLoader = rteTemplateDataLoader;
        }

        /// <summary>
        /// The initial view for performing a DateShift
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public ActionResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.Read, ws, null);

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_MANAGE_RTE_TEMPLATES, workspace);
            ViewData["ContainsOCI"] = ws.ContainsOCI;
            ViewData["WorkspaceId"] = ws.Id;
            ViewData["TemplateSources"] = this.rteTemplateDataLoader.GetSources();
            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, sw);
            return toReturn;
        }

        /// <summary>
        /// Save the templates
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        public JsonResult SaveRTETemplatesModel(string workspace, ICollection<RteCustomTemplateModelView> templates)
        {
            if (templates == null || templates.None())
            {
                throw new ArgumentNullException(nameof(templates));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_SAVE_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            // Validation (make sure only one template has each section selected)
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(t.Description)))
            {
                validationErrors.Add(new ValidationMessage("Template Name is required."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && !t.Questions.Any(q => q.Updateable != UpdateType.Deleted)))
            {
                validationErrors.Add(new ValidationMessage("At least one prompt is required for a Template."));
            }

            if (templates.Any(t => t.Updateable != UpdateType.Deleted && t.Questions.Any(q => q.Updateable != UpdateType.Deleted && string.IsNullOrWhiteSpace(q.Text))))
            {
                validationErrors.Add(new ValidationMessage("Prompt text is required."));
            }

            if (validationErrors.Any())
            {
                throw new GenValidationException(validationErrors);
            }

            // Set current user as author
            foreach (RteCustomTemplateModelView template in templates)
            {
                template.AuthorId = ws.CurrentActiveUser.UserID;
                template.WorkspaceId = ws.Id;
                if (template.Updateable != UpdateType.Deleted)
                {
                    template.Updateable = UpdateType.Upsert;
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.rteTemplateDataLoader.Save(templates);

                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_SAVE_RTE_TEMPLATES, sw);
            return this.Json(new { Status = true });
        }

        public JsonResult SearchTemplates(string workspace, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                throw new ArgumentNullException(nameof(search));
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_SEARCH_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            // Search for templates
            ICollection<RteCustomTemplateModelView> templates = this.rteTemplateDataLoader.Search(search);

            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_SEARCH_RTE_TEMPLATES, sw);
            return this.Json(templates);
        }

        public JsonResult CopyTemplate(string workspace, int templateId)
        {
            if (templateId <= 0)
            {
                throw new ArgumentOutOfRangeException("templateId", templateId, "templateId must be a positive number.");
            }

            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_COPY_RTE_TEMPLATE, SecurityPage.RTETemplates, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

            // Retrieve template
            RteCustomTemplateModelView template = this.rteTemplateDataLoader.GetById(templateId);

            if (template == null)
            {
                throw new GenValidationException("Template not found");
            }

            // Reset template
            int newId = -1;
            template.Id = newId--;
            template.Assigned = new List<int>();
            template.AuthorId = ws.CurrentActiveUser.UserID;
            template.Updateable = UpdateType.Upsert;
            template.WorkspaceId = ws.Id;

            if (template.Questions.Any())
            {
                template.Questions.ForEach(question =>
                {
                    question.Id = newId--;
                    question.TemplateId = template.Id;
                    question.Updateable = UpdateType.Upsert;
                });
            }

            // save template with current workspace
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.rteTemplateDataLoader.Save(template);

                scope.Complete();
            }

            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_COPY_RTE_TEMPLATE, sw);
            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Gets the RTE Templates Model for this workspace
        /// </summary>
        /// <param name="workspace">The workspace to retrieve data from.</param>
        /// <returns>The model for this workspace.</returns>
        public JsonResult GetRTETemplatesModel(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(this.logger, WebConstants.ACTION_GET_RTE_TEMPLATES, SecurityPage.RTETemplates, SecurityAuthorization.Read, ws, null);

            // Retrieve Templates for this workspace
            ICollection<RteCustomTemplateModelView> templates = this.rteTemplateDataLoader.GetTemplates(ws.Id);

            foreach (RteCustomTemplateModelView template in templates)
            {
                IList<RteCustomTemplateQuestionModelView> questions = template.Questions.OrderBy(q => q.SortOrder).ToList();

                // make sort order unique
                for(int i = 0; i < questions.Count; i++)
                {
                    questions[i].SortOrder = i;
                }

                template.Questions = questions;
            }

            // Finalize Action
            FinalizeAction(this.logger, WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES, sw);
            return this.Json(templates);
        }
    }
}