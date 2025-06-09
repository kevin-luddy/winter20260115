// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;
	using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.DateShift;
    using GenBOE.ActionLogic.Metrics;
	using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Controller for DateShift
    /// </summary>
    /// <seealso cref="GenBOE.Web.Common.GenBOEController" />
    public class DateShiftController : GenBOEController
    {
        /// <summary>
        /// The date shift calculation class.
        /// </summary>
        private DateShiftCalculation dateShiftCalculation;
        
        /// <summary>
        /// The task loader
        /// </summary>
        private IBoeTaskElementDTODataLoader taskLoader;

		/// <summary>
		/// Date shift dto data loader.
		/// </summary>
		private IDateShiftDTODataLoader dateShiftDTODataLoader;

        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(DateShiftController));

		/// <summary>
		/// Initializes a new instance of the <see cref="DateShiftController" /> class.
		/// </summary>
		/// <param name="securityAccess">The security access.</param>
		/// <param name="commonDataMapper">The common data mapper.</param>
		/// <param name="siteMasterUtilities">The site master utilities.</param>
		/// <param name="systemMetrics">The system metrics.</param>
		/// <param name="factory">The factory.</param>
		/// <param name="userLoader">The user loader.</param>
		/// <param name="permissionsLoader">The permissions loader.</param>
		/// <param name="controllerLogic">The controller logic.</param>
		public DateShiftController(ISecurityAccess securityAccess,
            ICommonDataMapper commonDataMapper,
            SiteMasterUtilities siteMasterUtilities,
            SystemMetrics systemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionsLoader,
            IGenBOEControllerLogic controllerLogic,
            DateShiftCalculation dateShiftCalculation,
            IBoeTaskElementDTODataLoader taskLoader,
			IDateShiftDTODataLoader dateShiftDTODataLoader)
            : base(securityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, factory, userLoader,
                 permissionsLoader, controllerLogic)
        {
            this.dateShiftCalculation = dateShiftCalculation;
            this.taskLoader = taskLoader;
			this.dateShiftDTODataLoader = dateShiftDTODataLoader;
		}

        /// <summary>
        /// Applies a date shift to a specific object (and optionally children).
		/// TODO Thomas: Start here for logic.
        /// </summary>
        /// <param name="id">The id of the parent object to dateshift</param>
        /// <param name="dateShiftLevel">The level of the parent object to dateshift.</param>
        /// <param name="dateShiftModel">The date shift model views.</param>
        /// <param name="validateOnly">If this should only validate the dateshift.</param>
        /// <returns>JsonResult of the success or failure.</returns>
        /// <exception cref="System.NotImplementedException">Not implemented</exception>
        public JsonResult ApplyDateShift(string workspace, int id, Level dateShiftLevel, DateShiftModelView dateShiftModel, bool validateOnly)
        {
            Stopwatch sw;
			IDateShiftable dateShiftable = null; 
			Level parentLevel = Level.Workspace;
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentOutOfRangeException("id", id, "id must be a positive number.");
                }

                if (dateShiftLevel == Level.NotSet)
                {
                    throw new ArgumentNullException(nameof(dateShiftLevel));
                }

                if (dateShiftLevel == Level.CLINCollection)
                {
                    throw new NotImplementedException();
                }

                if (dateShiftModel == null)
                {
                    throw new ArgumentNullException(nameof(dateShiftModel));
                }

                if (dateShiftModel.Details == null || dateShiftModel.Details.None())
                {
                    throw new ArgumentException("Details cannot be null or empty", nameof(dateShiftModel));
                }

                FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);

                // Initialize Action
                DateTime? parentStart = ws.ContractStartDate;
                DateTime? parentEnd = ws.ContractEndDate;

                dateShiftModel.Workspace = ws;

                // set error handling for details from base modelview
                foreach (DateShiftDetailModelView detail in dateShiftModel.Details)
                {
                    detail.Error1FixSingleMonth = dateShiftModel.Error1FixSingleMonth;
                    detail.Error2Handling = dateShiftModel.Error2Handling;

                    if (detail.ChildModificationType == ChildModificationType.NotSet)
                    {
                        throw new GenValidationException("Child Handling Option not set for " + detail.Operation.ToDescription());
                    }
                }

				// create the IDateShiftable based on Level and Id 
				switch (dateShiftLevel)
				{
					case Level.BOE:
						FullBoe boe = this.Factory.CreateFullBoe(id);
						sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, id);
						dateShiftable = boe;
						if (boe.CLINID.HasValue && boe.Clin.StartDate.HasValue && boe.Clin.EndDate.HasValue)
						{
							parentStart = boe.Clin.StartDate;
							parentEnd = boe.Clin.EndDate;
							parentLevel = Level.CLIN;
						}



						// TODO Thomas: Do we need RTE data?
						// preload data
						//boe.LoadTaskElementRTEData();
						//boe.LoadTravelRTEData();

						break;
					case Level.CLIN:
						FullClin clin = this.Factory.CreateFullClin(id);
						sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
						dateShiftable = clin;

						// preload data
						IReadOnlyCollection<FullBoe> boes = clin.Boes;
						foreach (FullBoe clinboe in boes)
						{
							// TODO Thomas: Do we need RTE data?
							//clinboe.LoadTravelRTEData();
							//clinboe.LoadTaskElementRTEData();
						}

						if (!clin.StartDate.HasValue || !clin.EndDate.HasValue)
						{
							clin.StartDate = ws.StartDate;
							clin.EndDate = ws.EndDate;
						}

						break;
					case Level.Task:
						BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(id, ws.DecimalPrecision, ws.CostDecimalPrecision);
						sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, taskElement.BoeID);
						dateShiftable = taskElement;
						FullBoe taskBoe = this.Factory.CreateFullBoe(taskElement.BoeID);
						parentStart = taskBoe.StartDate;
						parentEnd = taskBoe.EndDate;
						parentLevel = Level.BOE;
						break;
					case Level.Travel:
						TravelDTO travel = this.Factory.CreateTravel(id);
						sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, travel.BoeID);
						dateShiftable = travel;
						FullBoe travelBoe = this.Factory.CreateFullBoe(travel.BoeID);
						parentStart = travelBoe.StartDate;
						parentEnd = travelBoe.EndDate;
						parentLevel = Level.BOE;
						break;
					case Level.Workspace:
						sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
						dateShiftable = ws;
						parentStart = parentEnd = null;

						// pre-load the data efficiently
						ws.LoadClinsAndBoes(false);
						ICollection<BoeTaskElementDTO> tasks = ws.TaskElements.ToList();
						ICollection<TravelDTO> travels = ws.Travels.ToList();
						foreach (FullBoe fullboe in ws.Boes)
						{
							fullboe.SetTaskElements(tasks);
							fullboe.SetTravels(travels);
						}

						break;
					default:
						throw new NotSupportedException("This Date Shift Level is not supported: " + dateShiftLevel.GetDescription());
				}

				this.dateShiftCalculation.PerformDateShift(dateShiftable, dateShiftModel, parentStart, parentEnd, validateOnly, parentLevel, workspace, ws);
                this.Factory.ClearWorkspaceCache(ws.Shortname);
            }
            catch (GenValidationException)
            {
                throw;
            }
            catch (NotSupportedException nse)
            {
                throw new GenValidationException(nse.Message);
            }
            catch (Exception ex)
            {
                //wrap error inside a GenValidationException
                this.logger.Error(ex, "Unknown error during DateShift");
                throw new GenValidationException("Unknown Error during dateshift.", ex);
            }

            // Finalize Action
            this.FinalizeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, sw);

			JsonResult toReturn = this.Json(new { Success = true, startDate = dateShiftable.StartDate.Value.ToMonthString(), endDate = dateShiftable.EndDate.Value.ToMonthString() });

			return toReturn;
        }

        /// <summary>
        /// The initial view for performing a DateShift
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="level">The date shift level.</param>
        /// <returns>The initial view for performing a DateShift.</returns>
        public ActionResult Index(string workspace, int id, Level level)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            Level parentLevel = Level.Workspace;
            bool hasDiscrete = false;
            bool discreteOutsidePop = false;
            string startDate = ws.ContractStartDate.ToMonthString();
            string endDate = ws.ContractEndDate.ToMonthString();
            string parentStart = ws.ContractStartDate.ToMonthString();
            string parentEnd = ws.ContractEndDate.ToMonthString();
            string title;
            // Initialize Action
            Stopwatch sw;
            string returnUrl;
            switch (level)
            {
                case Level.BOE:
                    FullBoe boe = this.Factory.CreateFullBoe(id);
                    if (boe.Id != id)
                    {
                        throw new GenValidationException("The BOE Id passed in is invalid.");
                    }
                    hasDiscrete = boe.TaskElements.Any(x => x.taskElementLabors.Any(y => y.SpreadCurveID == SpreadCurves.DiscreteHours || y.SpreadCurveID == SpreadCurves.DiscreteCost));
                    if (hasDiscrete)
                    {
                        discreteOutsidePop = this.CheckOutsidePop(boe.TaskElements) || this.CheckOutsidePop(new FullBoe[] { boe });
                    }
                    startDate = boe.StartDate.ToMonthString();
                    endDate = boe.EndDate.ToMonthString();
                    if (boe.CLINID.HasValue && boe.Clin.StartDate.HasValue && boe.Clin.EndDate.HasValue)
                    {
                        parentStart = boe.Clin.StartDate.Value.ToMonthString();
                        parentEnd = boe.Clin.EndDate.Value.ToMonthString();
                        parentLevel = Level.CLIN;
                    }

                    returnUrl = "/" + ws.Shortname + "/BOE/EditBOEIndex/boe/" + id.ToString();
                    title = "BOE - " + boe.Title ?? string.Empty;
                    sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, boe.Id);
                    break;
                case Level.CLIN:
                    FullClin clin = this.Factory.CreateFullClin(id);
                    if (clin.Id != id)
                    {
                        throw new GenValidationException("The CLIN Id passed in is invalid.");
                    }

                    if (clin.StartDate.HasValue && clin.EndDate.HasValue)
                    {
                        startDate = clin.StartDate.Value.ToMonthString();
                        endDate = clin.EndDate.Value.ToMonthString();
                    }

                    ICollection<int> boeIDs = clin.Boes.Select(x => x.Id).ToCollection();
                    ICollection<BoeTaskElementDTO> taskElements = this.taskLoader.GetByBoeIds(boeIDs, false, ws.DecimalPrecision, ws.CostDecimalPrecision);
                    hasDiscrete = taskElements.Any(x => x.taskElementLabors.Any(y => y.SpreadCurveID == SpreadCurves.DiscreteHours || y.SpreadCurveID == SpreadCurves.DiscreteCost));
                    if (hasDiscrete)
                    {
                        discreteOutsidePop = this.CheckOutsidePop(taskElements) || this.CheckOutsidePop(clin.Boes);
                    }
                    returnUrl = "/" + ws.Shortname + "/CLIN";
                    title = "CLIN - " + clin.ClinString ?? string.Empty;
                    sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
                    break;
                case Level.Task:
                    BoeTaskElementDTO taskElement = this.Factory.CreateTaskElement(id, ws.DecimalPrecision, ws.CostDecimalPrecision);
                    if (taskElement.Id != id)
                    {
                        throw new GenValidationException("The Task Element Id passed in is invalid.");
                    }

                    if (!taskElement.StartDate.HasValue || !taskElement.EndDate.HasValue)
                    {
                        throw new GenValidationException("The Task Element Id passed in does not already have Start and End date values.");
                    }

                    startDate = taskElement.StartDate.Value.ToMonthString();
                    endDate = taskElement.EndDate.Value.ToMonthString();
                    FullBoe taskBoe = this.Factory.CreateFullBoe(taskElement.BoeID);
                    parentStart = taskBoe.StartDate.ToMonthString();
                    parentEnd = taskBoe.EndDate.ToMonthString();
                    parentLevel = Level.BOE;
                    hasDiscrete = taskElement.taskElementLabors.Any(y => y.SpreadCurveID == SpreadCurves.DiscreteHours || y.SpreadCurveID == SpreadCurves.DiscreteCost);
                    if (hasDiscrete)
                    {
                        discreteOutsidePop = this.CheckOutsidePop(new BoeTaskElementDTO[] { taskElement });
                    }
                    returnUrl = "/" + ws.Shortname + "/BOE/EditBOEIndex/boe/" + taskElement.BoeID.ToString() + "#LMLabor/task/" + id.ToString();
                    title = "Task - " + taskElement.TaskTitle ?? string.Empty;
                    sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, taskElement.BoeID);
                    break;
                case Level.Travel:
                    TravelDTO travel = this.Factory.CreateTravel(id);
                    if (travel.Id != id)
                    {
                        throw new GenValidationException("The Travel Id passed in is invalid.");
                    }

                    if (!travel.StartDate.HasValue || !travel.EndDate.HasValue)
                    {
                        throw new GenValidationException("The Travel Id passed in does not already have Start and End date values.");
                    }

                    startDate = travel.StartDate.Value.ToMonthString();
                    endDate = travel.EndDate.Value.ToMonthString();
                    FullBoe travelBoe = this.Factory.CreateFullBoe(travel.BoeID);
                    parentStart = travelBoe.StartDate.ToMonthString();
                    parentEnd = travelBoe.EndDate.ToMonthString();
                    parentLevel = Level.BOE;
                    returnUrl = "/" + ws.Shortname + "/BOE/EditBOEIndex/boe/" + travel.BoeID.ToString() + "#Travel/travel/" + id.ToString();
                    title = "Travel - " + travel.TaskTitle ?? string.Empty;
                    sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.BoeTaskDates, SecurityAuthorization.CreateReadUpdateDelete, ws, travel.BoeID);
                    break;
                case Level.Workspace:
                    if (ws.Id != id)
                    {
                        throw new GenValidationException("The Workspace Id passed in does not match the Workspace Short Name.");
                    }
                    hasDiscrete = ws.TaskElements.Any(x => x.taskElementLabors.Any(y => y.SpreadCurveID == SpreadCurves.DiscreteHours || y.SpreadCurveID == SpreadCurves.DiscreteCost));
                    if (hasDiscrete)
                    {
                        discreteOutsidePop = this.CheckOutsidePop(ws.TaskElements) || this.CheckOutsidePop(ws.Clins);
                    }

                    returnUrl = "/" + ws.Shortname + "/Workspace/WorkspaceSettings#WorkspaceIdentification";
                    title = "Workspace - " + ws.WorkspaceName ?? string.Empty;
                    sw = this.InitializeAction(this.logger, WebConstants.ACTION_APPLY_DATE_SHIFT, SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, ws, null);
                    break;
                default:
                    throw new NotSupportedException("This Date Shift Level is not supported" + level.GetDescription());
            }
            
            ViewData["WorkspaceID"] = ws.Id;
            ViewData["HoursLabel"] = FullObjectHelper.HoursLabel(ws);

            /** Valid Model Check */
            if (!ModelState.IsValid)
            {
                throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(ModelState));
            }

            List<SpreadCurves> results = Enum.GetValues(typeof(SpreadCurves)).Cast<SpreadCurves>().ToList<SpreadCurves>();

            results.Remove(SpreadCurves.DiscreteHours);
            results.Remove(SpreadCurves.DiscreteCost);
            results.Remove(SpreadCurves.Level);
            results.Remove(SpreadCurves.Load);
            results.Remove(SpreadCurves.None);
            List<SelectListItem> spreadCurves = (from x in results
                                                select new SelectListItem()
                                                {
                                                    Text = x.GetDescription(),
                                                    Value = ((int)x).ToString()
                                                }).ToList();

            spreadCurves.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });

            DateShiftIndexModelView model = new DateShiftIndexModelView(id, level, hasDiscrete, discreteOutsidePop, startDate, 
                endDate, spreadCurves, returnUrl, title, parentLevel, parentStart, parentEnd, ws.WorkspaceName, ws.WorkspaceStateName);

            // Finalize Action
            FinalizeAction(this.logger, "Index", sw);
            return this.View(WebConstants.VIEW_INDEX, model);
        }

        private bool CheckOutsidePop(IEnumerable<FullClin> clins)
        {
            bool outsidePoP = clins.Any(c => c.StartDate.HasValue && c.EndDate.HasValue && (this.CheckOutsidePop(c.Boes) || c.Boes.Any(b => !b.StartDate.IsInRange(c.StartDate.Value, c.EndDate.Value) || !b.EndDate.IsInRange(c.StartDate.Value, c.EndDate.Value))));

            return outsidePoP;
        }

        private bool CheckOutsidePop(IEnumerable<FullBoe> boes)
        {
            return boes.Any(b => b.TaskElements.Any(t => t.StartDate.HasValue && t.EndDate.HasValue && (!t.StartDate.Value.IsInRange(b.StartDate, b.EndDate) || !t.EndDate.Value.IsInRange(b.StartDate, b.EndDate))));
        }

        private bool CheckOutsidePop(IEnumerable<BoeTaskElementDTO> taskElements)
        {
            return taskElements.Any(t => t.StartDate.HasValue && t.EndDate.HasValue && t.taskElementLabors.Any(l => l.StartDate.HasValue && l.EndDate.HasValue && (!l.StartDate.Value.IsInRange(t.StartDate.Value, t.EndDate.Value) || !l.EndDate.Value.IsInRange(t.StartDate.Value, t.EndDate.Value))));
        }
    }
}