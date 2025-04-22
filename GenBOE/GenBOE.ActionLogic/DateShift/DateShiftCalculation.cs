// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.Workspace;
	using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Calculates Date Shift.
    /// </summary>
    public class DateShiftCalculation
    {
        /// <summary>
        /// The workspace loader
        /// </summary>
        private readonly IWorkspaceDTODataLoader workspaceLoader;
        
        /// <summary>
        /// The clin loader
        /// </summary>
        private readonly IClinDTODataLoader clinLoader;
        
        /// <summary>
        /// The boe loader
        /// </summary>
        private readonly IBoeDTODataLoader boeLoader;

        /// <summary>
        /// The task loader
        /// </summary>
        private readonly IBoeTaskElementDTODataLoader taskLoader;

        /// <summary>
        /// The travel loader
        /// </summary>
        private readonly ITravelDTODataLoader travelLoader;

        /// <summary>
        /// The user dto loader
        /// </summary>
        private readonly IUserDTODataLoader userDTOLoader;

        /// <summary>
        /// The boe emailer.
        /// </summary>
        private readonly IBoeEmailer emailer;

        /// <summary>
        /// The permission loader
        /// </summary>
        private readonly IPermissionsDTODataLoader permissionLoader;

        /// <summary>
        /// The user date change information
        /// </summary>
        private readonly HashSet<UserDateChangeInfo> userDateChangeInfo = new HashSet<UserDateChangeInfo>();

        /// <summary>
        /// The user date change information for errors
        /// </summary>
        private readonly HashSet<UserDateChangeInfo> userDateChangeInfoErrors = new HashSet<UserDateChangeInfo>();

        /// <summary>
        /// The workspace version data loader.
        /// </summary>
        private readonly IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataDTODataLoader;

        /// <summary>
        /// The BOE state machine.
        /// </summary>
        private readonly IBOEStateMachine boeStateMachine = null;

        /// <summary>
        /// The full object factory.
        /// </summary>
        private readonly IFullObjectFactory factory = null;

		/// <summary>
		/// The BOE Labor Controller Logic
		/// </summary>
		private readonly IBOELaborControllerLogic boeLaborControllerLogic;

		/// <summary>
		/// Initializes a new instance of the <see cref="DateShiftCalculation"/> class.
		/// </summary>
		/// <param name="workspaceLoader">The workspace loader.</param>
		/// <param name="clinLoader">The clin loader.</param>
		/// <param name="boeLoader">The boe loader.</param>
		/// <param name="taskLoader">The task loader.</param>
		/// <param name="travelLoader">The travel loader.</param>
		public DateShiftCalculation()
        {
            this.workspaceLoader = GenBOEUnityContainer.Container.Resolve(typeof(IWorkspaceDTODataLoader)) as IWorkspaceDTODataLoader;
            this.clinLoader = GenBOEUnityContainer.Container.Resolve(typeof(IClinDTODataLoader)) as IClinDTODataLoader;
            this.boeLoader = GenBOEUnityContainer.Container.Resolve(typeof(IBoeDTODataLoader)) as IBoeDTODataLoader;
            this.taskLoader = GenBOEUnityContainer.Container.Resolve(typeof(IBoeTaskElementDTODataLoader)) as IBoeTaskElementDTODataLoader;
            this.userDTOLoader = GenBOEUnityContainer.Container.Resolve(typeof(IUserDTODataLoader)) as IUserDTODataLoader;
            this.travelLoader = GenBOEUnityContainer.Container.Resolve(typeof(ITravelDTODataLoader)) as ITravelDTODataLoader;
            this.emailer = GenBOEUnityContainer.Container.Resolve(typeof(IBoeEmailer)) as IBoeEmailer;
            this.permissionLoader = GenBOEUnityContainer.Container.Resolve(typeof(IPermissionsDTODataLoader)) as IPermissionsDTODataLoader;
            this.workspaceVersionMetaDataDTODataLoader = GenBOEUnityContainer.Container.Resolve(typeof(IWorkspaceVersionMetaDataDTODataLoader)) as IWorkspaceVersionMetaDataDTODataLoader;
            this.boeStateMachine = GenBOEUnityContainer.Container.Resolve(typeof(IBOEStateMachine)) as IBOEStateMachine;
            this.factory = GenBOEUnityContainer.Container.Resolve(typeof(IFullObjectFactory)) as IFullObjectFactory;
			this.boeLaborControllerLogic = GenBOEUnityContainer.Container.Resolve(typeof(IBOELaborControllerLogic)) as IBOELaborControllerLogic;
		}

		/// <summary>
		/// Performs the date shift.
		/// </summary>
		/// <param name="dateShiftable">The date shiftable object.</param>
		/// <param name="dateShiftModel">The dateshift model.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="validateOnly">If this should only validate the dateshift.</param>
		/// <param name="parentLevel">The parent level.</param>
		/// <param name="workspaceShortname">Workspace ShortName</param>
		/// <param name="fullWorkspace">The full workspace</param>
		/// <exception cref="ArgumentNullException">dateShiftable or details</exception>
		public void PerformDateShift(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel, DateTime? parentStart, DateTime? parentEnd, 
            bool validateOnly, Level parentLevel, string workspaceShortname, FullWorkspace fullWorkspace)
        {
            if (dateShiftable == null)
            {
                throw new ArgumentNullException(nameof(dateShiftable));
            }

            if (dateShiftModel == null)
            {
                throw new ArgumentNullException(nameof(dateShiftModel));
            }

			if (fullWorkspace == null)
			{
				throw new ArgumentNullException(nameof(fullWorkspace));
			}

			if (dateShiftModel.Details == null || dateShiftModel.Details.None())
            {
                throw new ArgumentException("Details for a dateshift cannot be null or empty", nameof(dateShiftModel));
            }

            if (!dateShiftable.StartDate.HasValue || !dateShiftable.EndDate.HasValue)
            {
                throw new ArgumentException("Object to DateShift does not have a valid start or end time.");
            }

            // Perform Shifts
            PerformShifts(dateShiftable, dateShiftModel, parentStart, parentEnd, null, parentLevel, workspaceShortname);

            // Error handling - only need to throw the errors for the last detail (duration change) in case there are both a shift and duration change
            if (dateShiftModel.Details.Last().Errors.Messages.Any())
            {
                throw new GenValidationException(dateShiftModel.Details.Last().Errors.Messages.Select(m => new ValidationMessage(m)));
            }

            if (!validateOnly)
            {
                // Pre-load emails (to get original start/end dates)
                this.GenerateEmails(dateShiftable, dateShiftModel);


				// Check if we have Skill Mix enabled to adjust Skill Mix table data as needed
				if (Utilities.ShowSkillMixForWorkspace(fullWorkspace?.CreationDate))
				{
					// Iterate through each task and check for Skill Mix
					foreach (BoeTaskElementDTO task in fullWorkspace?.TaskElements)
					{
						if (Utilities.ShowSkillMixForTask(fullWorkspace?.CreationDate, BOETaskUtility.IsUsingTMRates(fullWorkspace, task)))
						{
							bool isBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspaceShortname);

							// Run Skill Mix update
							ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours = fullWorkspace?.MoqTypeSelections
								.SelectMany(moqType => moqType.TableData)
								.SelectMany(tableData => tableData.ResourceHours)
								.ToList();

							FullBoe fullBoe = this.factory.CreateFullBoe(task.BoeID);

							LaborTaskDataModelView laborTasks = this.boeLaborControllerLogic.ConvertDtoToModelView(fullWorkspace, fullBoe, task);

							bool allAutomaticMOQTypes = fullWorkspace.MoqTypeSelections?.All(m => m.SelectedMOQType == MOQType.Comparative || m.SelectedMOQType == MOQType.Historical) ?? true;

							bool isManual = !fullWorkspace.EnableSAPConnection || !allAutomaticMOQTypes;

							RefreshSkillMixModelView response = this.boeLaborControllerLogic.RefreshSkillMixTables(resourceHours,
								laborTasks.LaborTypesData, task.SkillMixTable, task.CommonDisclosureTable, isBRCEnabled, isManual, fullWorkspace.UCOTFactor);

							if (response != null)
							{
								// Update the task with the refreshed Skill Mix data
								task.SkillMixTable = response.SkillMixRows;
								task.CommonDisclosureTable = response.CommonDisclosureRows;
							}
						}
					}
				}

				// Save
				this.Save(dateShiftable, dateShiftModel);

                // Send emails
                this.SendEmails(dateShiftModel);
            }
        }

        /// <summary>
        /// Performs the shifts.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable object.</param>
        /// <param name="dateShiftModel">The dateshift model containing details.</param>
        /// <param name="parentStart">The parent start.</param>
        /// <param name="parentEnd">The parent end.</param>
        /// <param name="parentBoeId">The parent's BOE ID.</param>
        /// <param name="parentLevel">The parent's Level.</param>
        internal static void PerformShifts(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
        {
            // Perform dateshift on this object
            if (!dateShiftable.StartDate.HasValue || !dateShiftable.EndDate.HasValue)
            {
                dateShiftable.StartDate = parentStart;
                dateShiftable.EndDate = parentEnd;
            }

            foreach (DateShiftDetailModelView detail in dateShiftModel.Details)
            {
                PerformShiftOperation(dateShiftable, detail);
                Validate(dateShiftable, detail, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);
                PerformChildrenShift(dateShiftable, parentStart, parentEnd, parentBoeId, detail, dateShiftModel, workspaceShortname);
            }
        }

        /// <summary>
        /// Performs the children shift.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="parentStart">The parent start.</param>
        /// <param name="parentEnd">The parent end.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="modelView">The Dateshift modelview.</param>
		/// <param name="workspaceShortname">Workspace shortname</param>
        /// <exception cref="NotSupportedException"></exception>
        private static void PerformChildrenShift(IDateShiftable dateShiftable, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, DateShiftDetailModelView detail, DateShiftModelView modelView, string workspaceShortname)
        {
            // Perform dateshift on child objects
            if (dateShiftable.HasSpread)
            {
                switch (dateShiftable.DateShiftLevel)
                {
					// TODO Thomas: Breaks here when BOE Task because the labor isn't correct?
                    case Level.Labor:
                        PerformLaborSpreadShift(dateShiftable, detail, modelView);
                        Validate(dateShiftable, detail, parentStart, parentEnd, parentBoeId, Level.Task, workspaceShortname);
                        break;
                    case Level.Travel:
                        PerformTravelSpreadShift(dateShiftable, detail);
                        Validate(dateShiftable, detail, parentStart, parentEnd, parentBoeId, Level.BOE, workspaceShortname);
                        break;
                    default:

                        throw new NotSupportedException(string.Format("Data class setup incorrectly, Class with level {0} has Spreads.", dateShiftable.DateShiftLevel.ToString()));
                }
            }
            else
            {
                RecursiveChildShifts(dateShiftable, detail, parentStart, parentEnd, modelView, workspaceShortname);
            }
        }

        /// <summary>
        /// Recursively dateshifts the children.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable object to pull children from.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="grandParentStart">The object's parent start.</param>
        /// <param name="grandParentEnd">The object's parent end.</param>
		/// <param name="workspaceShortname">workspace shortname</param>
        internal static void RecursiveChildShifts(IDateShiftable dateShiftable, DateShiftDetailModelView detail, DateTime? grandParentStart, DateTime? grandParentEnd, DateShiftModelView modelView, string workspaceShortname)
        {
            if (dateShiftable.Children.Any())
            {
                // if parent's start/end are not valid, use grandparent's
                DateTime? pStart = dateShiftable.StartDate;
                DateTime? pEnd = dateShiftable.EndDate;
                if (!pStart.HasValue || !pEnd.HasValue)
                {
                    pStart = grandParentStart;
                    pEnd = grandParentEnd;
                }

                int? parentBoeId = GetAttachedBoeId(dateShiftable);

                foreach (IDateShiftable child in dateShiftable.Children)
                {
                    if (detail.ChildModificationType == ChildModificationType.NoChange)
                    {
                        // validate the no change
                        Validate(child, detail, pStart, pEnd, parentBoeId, dateShiftable.DateShiftLevel, workspaceShortname);

                        // recursive call to recursive validate
                        PerformChildrenShift(child, pStart, pEnd, parentBoeId, detail, modelView, workspaceShortname);
                    }
                    else
                    {
                        PerformChildShift(child, detail, pStart, pEnd);

                        // validate the shift
                        Validate(child, detail, pStart, pEnd, parentBoeId, dateShiftable.DateShiftLevel, workspaceShortname);

                        // recursive call
                        PerformChildrenShift(child, pStart, pEnd, parentBoeId, detail, modelView, workspaceShortname);
                    }
                }
            }
        }

        /// <summary>
        /// Performs the shift for a child.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="parentStart">The parent start time</param>
        /// <param name="parentEnd">The parent end time.</param>
        internal static void PerformChildShift(IDateShiftable dateShiftable, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd)
        {
            if (dateShiftable.StartDate.HasValue && dateShiftable.EndDate.HasValue)
            {
                dateShiftable.Updateable = UpdateType.Upsert;
                switch (detail.ChildModificationType)
                {
                    case ChildModificationType.FlowDown:
                        PerformShiftOperation(dateShiftable, detail);
                        break;
                    case ChildModificationType.ToEnd:
                        if (parentEnd.HasValue && dateShiftable.StartDate.HasValue && dateShiftable.EndDate.HasValue)
                        {
                            int negativeDuration = -1 * dateShiftable.StartDate.Value.MonthDifference(dateShiftable.EndDate.Value);
                            dateShiftable.EndDate = parentEnd.Normalize();
                            if (detail.Operation == Operation.Shift)
                            {
                                // make sure this was a negative shift
                                if (detail.MonthChange > 0)
                                {
                                    throw new NotSupportedException("Positive Shifts cannot have Child Modification Type set to End.");
                                }
                                dateShiftable.StartDate = dateShiftable.EndDate.Value.AddMonths(negativeDuration).Normalize();
                            }
                        }
                        break;
                    case ChildModificationType.ToPoP:
                        if (parentStart.HasValue && parentEnd.HasValue)
                        {
                            dateShiftable.StartDate = parentStart.Value.Normalize();
                            dateShiftable.EndDate = parentEnd.Value.Normalize();
                        }
                        break;
                    case ChildModificationType.ToStart:
                        if (parentStart.HasValue && dateShiftable.StartDate.HasValue && dateShiftable.EndDate.HasValue)
                        {
                            if (detail.Operation != Operation.Shift || detail.MonthChange < 0)
                            {
                                throw new NotSupportedException("Only a Positive Shift can have Child Modification Type set to Start.");
                            }

                            int duration = dateShiftable.StartDate.Value.MonthDifference(dateShiftable.EndDate.Value);
                            dateShiftable.StartDate = parentStart.Normalize();
                            dateShiftable.EndDate = dateShiftable.StartDate.Value.AddMonths(duration);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Performs the shift operation.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="detail">The detail.</param>
        internal static void PerformShiftOperation(IDateShiftable dateShiftable, DateShiftDetailModelView detail)
        {
            switch (detail.Operation)
            {
                case Operation.Shift:
                    dateShiftable.StartDate = dateShiftable.StartDate.Value.AddMonths(detail.MonthChange).Normalize();
                    break;
                case Operation.DurationChange:
                    break;
            }

            dateShiftable.EndDate = dateShiftable.EndDate.Value.AddMonths(detail.MonthChange).Normalize();
            dateShiftable.Updateable = UpdateType.Upsert;
        }

        /// <summary>
        /// Validates the specified date shiftable.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="parentStart">The parent start.</param>
        /// <param name="parentEnd">The parent end.</param>
        /// <param name="parentBoeId">The parent's BOE ID.</param>
        /// <param name="parentLevel">The parent's level.</param>
		/// <param name="workspaceShortname">Workspace short name</param>
        internal static void Validate(IDateShiftable dateShiftable, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
        {
            CheckErrors(dateShiftable, detail, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);

            bool rerunError1Check = detail.Errors.HasError1 && detail.Error1FixSingleMonth == true;
            bool rerunError2Check = detail.Errors.HasError2 && detail.Error2Handling.HasValue;
            if (rerunError1Check || rerunError2Check)
            {
                DateShiftable ds = new DateShiftable
                {
                    DateShiftLevel = dateShiftable.DateShiftLevel,
                    StartDate = dateShiftable.StartDate,
                    EndDate = dateShiftable.EndDate,
                    HasSpread = dateShiftable.HasSpread,
                    BoeId = GetAttachedBoeId(dateShiftable) ?? parentBoeId ?? 0
                };

                DateShiftDetailModelView secondCheck = detail.Clone();
                CheckErrors(ds, secondCheck, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);

                if (rerunError1Check)
                {
                    if (secondCheck.Errors.HasError1)
                    {
                        detail.Errors.HasError1 = true;
                        AddErrorInfo(dateShiftable, detail, parentBoeId);
                        detail.Errors.Messages.Add(string.Format("Negative duration for a {0}.", dateShiftable.DateShiftLevel.ToDescription()));
                    }
                }

                if (rerunError2Check)
                {
                    if (secondCheck.Errors.HasError2)
                    {
                        detail.Errors.HasError2 = true;
                        AddErrorInfo(dateShiftable, detail, parentBoeId);
                        detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftable.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
                    }
                }
            }
        }

		/// <summary>
		/// Checks the errors.
		/// </summary>
		/// <param name="dateShiftable">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="parentBoeId">The parent's BOE ID.</param>
		/// <param name="parentLevel">The parent's Level.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "<Pending>")]
		private static void CheckErrors(IDateShiftable dateShiftable, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
        {
            if (dateShiftable.StartDate.HasValue && dateShiftable.EndDate.HasValue)
            {
                // Check for negative duration
                if (dateShiftable.EndDate < dateShiftable.StartDate)
                {
                    if (detail.Error1FixSingleMonth == true)
                    {
                        dateShiftable.EndDate = dateShiftable.StartDate.Normalize();
                    }
                    else
                    {
                        detail.Errors.HasError1 = true;
                        AddErrorInfo(dateShiftable, detail, parentBoeId);
                        detail.Errors.Messages.Add(string.Format("Negative duration for a {0}.", dateShiftable.DateShiftLevel.ToDescription()));
                    }
                }

                // check to make sure range is within PoP (Period of Performance)
                if (parentStart.HasValue && parentEnd.HasValue)
                {
					DateTime oneLMXStartDate = Utilities.OneLmxStartDate;
					DateTime previousStartDate = dateShiftable.StartDate.Value;
					DateTime previousEndDate = dateShiftable.EndDate.Value;
					DateTime? shiftedStartDate = null;
					DateTime? shiftedEndDate = null;

                    if (dateShiftable.StartDate < parentStart)
                    {
                        if (detail.Error2Handling.HasValue)
                        {
                            switch (detail.Error2Handling.Value)
                            {
                                case ChildModificationType.NoChange:
                                    AddErrorInfo(dateShiftable, detail, parentBoeId);
                                    break;
                                case ChildModificationType.ToStart:
                                    if (detail.Operation == Operation.DurationChange || detail.MonthChange < 0)
                                    {
                                        throw new NotSupportedException("This Child Modification Type is not allowed for Error Child outside the PoP of the Parent. To Start is only allowed for Error Handling when the Operation is a positive Shift.");
                                    }
                                    dateShiftable.StartDate = parentStart;
									shiftedStartDate = parentStart.Value;

									if (dateShiftable.EndDate < parentStart)
                                    {
                                        dateShiftable.EndDate = parentStart;
										shiftedEndDate = parentStart.Value;
                                    }
                                    break;
                                case ChildModificationType.ToEnd:
                                    dateShiftable.EndDate = parentEnd;
									shiftedEndDate = parentEnd.Value;
                                    if (dateShiftable.StartDate > parentEnd)
                                    {
                                        dateShiftable.StartDate = parentEnd;
										shiftedStartDate = parentEnd.Value;
                                    }
                                    break;
                                case ChildModificationType.ToPoP:
                                    dateShiftable.StartDate = parentStart;
									dateShiftable.EndDate = parentEnd;
									shiftedStartDate = parentStart.Value;
									shiftedEndDate = parentEnd.Value;
                                    break;
                                default:
                                    throw new NotSupportedException("This Child Modification Type is not allowed for Error Child outside the PoP of the Parent: " + detail.Error2Handling.Value.ToDescription());
                            }
                        }
                        else
                        {
                            detail.Errors.HasError2 = true;
                            AddErrorInfo(dateShiftable, detail, parentBoeId);
                            detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftable.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
                        }
                    }

                    if (dateShiftable.EndDate > parentEnd)
                    {
                        if (detail.Error2Handling.HasValue)
                        {
                            switch (detail.Error2Handling.Value)
                            {
                                case ChildModificationType.NoChange:
                                    AddErrorInfo(dateShiftable, detail, parentBoeId);
                                    break;
                                case ChildModificationType.ToEnd:
                                    int duration = dateShiftable.StartDate.Value.MonthDifference(dateShiftable.EndDate.Value);
                                    dateShiftable.EndDate = parentEnd;
									shiftedEndDate = parentEnd.Value;
                                    if (detail.Operation == Operation.Shift)
                                    {
                                        // only change start date if this is a shift
                                        dateShiftable.StartDate = parentEnd.Value.AddMonths(-1 * duration);
										shiftedStartDate = parentEnd.Value.AddMonths(-1 * duration);
                                    }
                                    break;
                                case ChildModificationType.ToPoP:
                                    dateShiftable.StartDate = parentStart;
                                    dateShiftable.EndDate = parentEnd;
									shiftedStartDate = parentStart.Value;
									shiftedEndDate = parentEnd.Value;
									break;
                                default:
                                    throw new NotSupportedException("This Child Modification Type is not allowed for Error Child outside the PoP of the Parent: " + detail.Error2Handling.Value.ToDescription());
                            }
                        }
                        else
                        {
                            detail.Errors.HasError2 = true;
                            AddErrorInfo(dateShiftable, detail, parentBoeId);
                            detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftable.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
                        }
                    }

					if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
					{
						if (previousStartDate < oneLMXStartDate)
						{
							if (shiftedStartDate.HasValue && shiftedStartDate.Value >= oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftable, detail, parentBoeId, "Resource and Business Resource Code", "Start Date", "shifted to the right of");
							}
						}

						if (previousEndDate < oneLMXStartDate)
						{
							if (shiftedEndDate.HasValue && shiftedEndDate.Value >= oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftable, detail, parentBoeId, "Resource", "End Date", "shifted to the right of");
							}
						}

						if (previousStartDate >= oneLMXStartDate)
						{
							if (shiftedStartDate.HasValue && shiftedStartDate.Value < oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftable, detail, parentBoeId, "Resource and Business Resource Code", "Start Date", "shifted to the left of");
							}
						}

						if (previousEndDate >= oneLMXStartDate)
						{
							if (shiftedEndDate.HasValue && shiftedEndDate.Value < oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftable, detail, parentBoeId, "Resource and Business Resource Code", "End Date", "shifted to the left of");
							}
						}
					}
                }
            }
        }

		/// <summary>
		/// Populate Details Model with proper Error message
		/// </summary>
		/// <param name="dateShiftable">Shiftable Model</param>
		/// <param name="detail">Date Shift Table Model</param>
		/// <param name="parentBoeId">Parent BOE ID</param>
		/// <param name="type">Type affect (Resource and/or Business Resource Code)</param>
		/// <param name="affectedDate">Start or End Date affected</param>
		/// <param name="shift">Shift to left or right of 1LMX Cutoff Date</param>
		private static void AddResourceTypeError(IDateShiftable dateShiftable, DateShiftDetailModelView detail, int? parentBoeId, string type, string affectedDate, string shift)
		{
			detail.Errors.HasError2 = true;
			AddErrorInfo(dateShiftable, detail, parentBoeId);
			detail.Errors.Messages.Add($"{affectedDate} has {shift} 1LMX Start Date. {type} will be affected");
		}

        /// <summary>
        /// Performs the task spreads shift.
        /// </summary>
        /// <param name="parentTask">The parent task.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="modelView">The model view.</param>
        /// <exception cref="NotSupportedException">The parentTask is set to Level.Labor but is not a Resource Labor Type.</exception>
        internal static void PerformLaborSpreadShift(IDateShiftable parentTask, DateShiftDetailModelView detail, DateShiftModelView modelView)
        {
            if (detail.ChildModificationType != ChildModificationType.NoChange)
            {
                ResourceTypeDto labor = parentTask as ResourceTypeDto;
                if (labor == null)
                {
                    throw new NotSupportedException("The parentTask is set to Level.Labor but is not a Resource Labor Type.");
                }

                // move the spreads
                PerformSpreadsShift(labor, detail, modelView);
            }
        }

        /// <summary>
        /// Performs a DateShift on the Spreads of a Resource Labor.
        /// </summary>
        /// <param name="labor">The Resource Labor.</param>
        /// <param name="detail">The details for the DateShift.</param>
        /// <param name="modelView">The model view.</param>
        internal static void PerformSpreadsShift(ResourceTypeDto labor, DateShiftDetailModelView detail, DateShiftModelView modelView)
        {
            if (labor.SpreadCurveID.HasValue)
            {
                if (labor.SpreadCurveID == SpreadCurves.DiscreteCost || labor.SpreadCurveID == SpreadCurves.DiscreteHours)
                {
                    if (!detail.SpreadHandling.HasValue)
                    {
                        throw new NotSupportedException("The Discrete SpreadHandling is not set and there are Discrete Spreads.");
                    }

                    switch (detail.SpreadHandling.Value)
                    {
                        case SpreadHandling.NotSet:
                            throw new NotSupportedException("The Discrete SpreadHandling is not set and there are Discrete Spreads.");
                        case SpreadHandling.NewCurve:
                            if (!detail.NewCurve.HasValue)
                            {
                                throw new NotSupportedException("New Curve was chosen for Discrete Spread Handling but the Curve was not selected.");
                            }

                            switch (detail.NewCurve.Value)
                            {
                                case SpreadCurves.DiscreteCost:
                                case SpreadCurves.DiscreteHours:
                                case SpreadCurves.Level:
                                case SpreadCurves.Load:
                                case SpreadCurves.None:
                                    throw new NotSupportedException("Invalid Spread Curve chosen for Discrete New Curve.");
                                default:
                                    break;
                            }

                            labor.SpreadCurveID = detail.NewCurve;
                            // do nothing since the spreads are not saved in database
                            break;
                        case SpreadHandling.DiscreteToError:
                        case SpreadHandling.DiscreteToFirst:
                        case SpreadHandling.DiscreteToLast:
                            // the total value that is cutoff from beginning and/or end of the discrete spreads
                            decimal tempTotal = 0;

                            ICollection<ResourceSpreadDto> currentSpread = labor.LaborSpreads;
                            int startDateDiff = detail.Operation == Operation.Shift ? detail.MonthChange : 0;

                            // The hour is saved as AM but the CaclulateLaborSpreadsBasedOnCurve returns the hour as PM.
                            // Need to convert to PM to perform compare.
                            foreach (ResourceSpreadDto spread in currentSpread)
                            {
                                DateTime laborSpreadRequestedDate = spread.LaborSpreadDate.AddMonths(startDateDiff);
                                spread.LaborSpreadDate = GenBOEUtilities.AdjustDateTimePrecision(laborSpreadRequestedDate, DateTimePrecision.Month);

                                if (spread.LaborSpreadDate.CompareTo(labor.EndDateValue) > 0 && (detail.SpreadHandling == SpreadHandling.DiscreteToFirst || detail.SpreadHandling == SpreadHandling.DiscreteToLast))
                                {
                                    tempTotal += spread.LaborSpreadValue;
                                }

                                spread.Updateable = UpdateType.Deleted;
                            }

                            LaborSpreadRequest spreadRequest = new LaborSpreadRequest()
                            {
                                CurveID = labor.SpreadCurveID,
                                HourSpread = 0,
                                StartDate = labor.StartDateValue,
                                EndDate = labor.EndDateValue
                            };
                            int decimalPrecision = labor.SpreadType == SpreadType.Cost ? modelView.Workspace.CostDecimalPrecision : modelView.Workspace.DecimalPrecision;
                            labor.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(spreadRequest, decimalPrecision);

                            foreach (ResourceSpreadDto spread in labor.LaborSpreads)
                            {
                                ResourceSpreadDto matchingSpread = (from x in currentSpread
                                                                    where x.LaborSpreadDate == spread.LaborSpreadDate
                                                                    select x).FirstOrDefault();

                                if (matchingSpread != null)
                                {
                                    spread.LaborSpreadValue = matchingSpread.LaborSpreadValue;
                                }

                                if (detail.SpreadHandling == SpreadHandling.DiscreteToFirst)
                                {
                                    DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

                                    if (spreadDate.CompareTo(labor.StartDateValue) == 0)
                                    {
                                        spread.LaborSpreadValue += tempTotal;
                                    }
                                }
                                else if (detail.SpreadHandling == SpreadHandling.DiscreteToLast)
                                {
                                    DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

                                    if (spreadDate.CompareTo(labor.EndDateValue) == 0)
                                    {
                                        spread.LaborSpreadValue += tempTotal;
                                    }
                                }

                                spread.Updateable = UpdateType.Upsert;
                                spread.BoeID = labor.BoeID;
                                spread.LaborTypeId = labor.Id;
                            }

                            labor.ValueSpread = Utilities.AdjustPrecision(labor.LaborSpreads.Sum(x => x.LaborSpreadValue), decimalPrecision);

                            break;
                    }
                }

                // else do nothing since the spreads are not saved in database
            }
        }

        /// <summary>
        /// Performs the travel spreads/trips shift.
        /// </summary>
        /// <param name="dateShiftable">The parent travel dto.</param>
        /// <param name="detail">The detail.</param>
        internal static void PerformTravelSpreadShift(IDateShiftable parentTravel, DateShiftDetailModelView detail)
        {
            if (detail.ChildModificationType != ChildModificationType.NoChange)
            {
                TravelDTO task = parentTravel as TravelDTO;
                if (task == null)
                {
                    throw new ArgumentException("The parentTask is set to Level.Travel but is not a Travel object.");
                }

                if (task.TravelTrips != null && task.TravelTrips.Any())
                {
                    throw new NotSupportedException("Travel Trips are not supported for Date Shift.");
                }

                if (task.MSTTravelTrips != null && task.MSTTravelTrips.Any())
                {
                    foreach(MSTTravelTripType trip in task.MSTTravelTrips)
                    {
                        int startDateOffset = detail.Operation == Operation.Shift ? detail.MonthChange : 0;
                        trip.TripDate = trip.TripDate.AddMonths(startDateOffset);
                        trip.EstimateDate = DateTime.Now;
                        trip.Updateable = UpdateType.Upsert;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the emails.
        /// </summary>
        /// <param name="dateShiftModel">The model view.</param>
        private void SendEmails(DateShiftModelView dateShiftModel)
        {
            switch (dateShiftModel.EmailOption)
            {
                case EmailOption.EmailBOEAuthors:
                    this.emailer.SendBOEAuthorsApproversDatesUpdated(this.userDateChangeInfo, false);
                    this.emailer.SendBOEAuthorsApproversDatesUpdated(this.userDateChangeInfoErrors, true);
                    break;
                case EmailOption.EmailBOEAuthorsErrorsOnly:
                    this.emailer.SendBOEAuthorsApproversDatesUpdated(this.userDateChangeInfoErrors, true);
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        /// <summary>
        /// Sends the emails.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="dateShiftModel">The model view.</param>
        private void GenerateEmails(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            switch (dateShiftModel.EmailOption)
            {
                case EmailOption.EmailBOEAuthors:
                    this.GenerateEmailBOEAuthors(dateShiftable, dateShiftModel);
                    this.GenerateEmailBoeAuthorsErrorsOnly(dateShiftable, dateShiftModel);
                    break;
                case EmailOption.EmailBOEAuthorsErrorsOnly:
                    this.GenerateEmailBoeAuthorsErrorsOnly(dateShiftable, dateShiftModel);
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        /// <summary>
        /// Saves the specified date shiftable objects.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable parent object.</param>
        /// <param name="dateShiftModel">The model view.</param>
        /// 
        private void Save(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            if (dateShiftable.DateShiftLevel == Level.Workspace)
            {
                // separate transaction for backup
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    FullWorkspace workspace = dateShiftable as FullWorkspace;
                    // make a backup first
                    string versionName = "SYSTEM: DATE SHIFT " + DateTime.Now.ToString();
                    IReadOnlyCollection<WorkspaceVersionMetaDataDTO> workspaceVersions = workspace.WorkspaceVersionMetaData;

                    foreach (WorkspaceVersionMetaDataDTO versionToCheck in workspaceVersions)
                    {
                        if (versionName == versionToCheck.VersionName)
                        {
                            // Version name must be unique
                            versionName += DateTime.Now.Millisecond.ToString();
                        }
                    }

                    Collection<WorkspaceVersionMetaDataDTO> toSave = new Collection<WorkspaceVersionMetaDataDTO>()
                    {
                        new WorkspaceVersionMetaDataDTO()
                        {
                            VersionName = versionName,
                            Updateable = UpdateType.Upsert,
                            CreatedByID = 0, // genBOE System 
                            DateCreated = new DateTime(),
                            VersionState = workspace.WorkspaceState
                        }
                    };

                    this.workspaceVersionMetaDataDTODataLoader.Save(toSave, workspace.Id);

                    scope.Complete();
                }
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {

                switch (dateShiftable.DateShiftLevel)
                {
                    case Level.Workspace:
                        this.SaveWorkspaceData(dateShiftable as FullWorkspace);
                        break;
                    case Level.BOE:
                        this.SaveBoeData(dateShiftable as FullBoe);
                        break;
                    case Level.CLIN:
                        this.SaveClinData(dateShiftable as FullClin);
                        break;
                    case Level.Task:
                        this.SaveTaskData(dateShiftable as BoeTaskElementDTO);
                        break;
                    case Level.Travel:
                        this.SaveTravelData(dateShiftable as TravelDTO);
                        break;
                    default:
                        throw new NotSupportedException("This Date Shift Level is not supported for saving.");
                }

                // Transition BOEs to Draft if needed
                this.TransitionBOEs(dateShiftable, dateShiftModel);

                scope.Complete();
            }
        }

        /// <summary>
        /// Transitions the BOEs to Draft if needed.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable parent object.</param>
        /// <param name="dateShiftModel">The model view.</param>
        private void TransitionBOEs(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            // get all the affected BOEs that are not in Draft
            ICollection<FullBoe> boesBackToDraft = this.GetAffectedBOEs(dateShiftable, dateShiftModel).Where(b => b.State != BOEState.Draft && b.State != BOEState.DraftLocked && b.State != BOEState.DateShiftDraft).ToList();

            if (boesBackToDraft.Any())
            {
                // we need to get the updated boes from the db (updateddate)
                ICollection<FullBoe> updatedBOEs = this.factory.CreateFullBoes(boesBackToDraft.Select(b => b.Id).ToList());

                BOEState newBOEState = BOEState.Draft;

                foreach (FullBoe boe in updatedBOEs)
                {
                    
                    string errorMessage = string.Empty;

                    if (this.boeStateMachine.PerformStateTransitionValidation(boe, dateShiftModel.Workspace, boe.State, newBOEState, out errorMessage))
                    {
                        BOEState oldState = boe.State;
                        boe.State = newBOEState;
                        boe.Updateable = UpdateType.Upsert;
                        boe.UpdatedByUserId = dateShiftModel.Workspace.CurrentActiveUser.UserID;
                        this.boeLoader.Save(boe);

                        // save of BOE worked .. perform transition steps and send email
                        this.boeStateMachine.PerformStateTransitionAction(boe, dateShiftModel.Workspace, oldState, boe.State);
                    }
                    else
                    {
                        // we can't move the BOE back to DRAFT for some reason ... abort
                        // pull this error message from the state machine itself
                        throw new GenValidationException(errorMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the workspace data.
        /// </summary>
        /// <param name="workspace">The workspace dto.</param>
        private void SaveWorkspaceData(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Get the user who is saving the BOE(s)
            int currentUserID = this.userDTOLoader.GetUserForActiveUser().UserID;
            this.workspaceLoader.SaveWorkspaceSettings(currentUserID, workspace);

            // Need to update last update date from DB
            WorkspaceDTO updatedWs = this.workspaceLoader.GetById(workspace.Id);
            workspace.UpdateDate = updatedWs.UpdateDate;

            ICollection<ClinDTO> clins = workspace.Clins.Where(c => c.Updateable == UpdateType.Upsert).ToList<ClinDTO>();
            this.clinLoader.Save(clins);

            List<FullBoe> boeCollection = workspace.Boes.Where(b => b.Updateable == UpdateType.Upsert).ToList();
            this.boeLoader.Save(boeCollection.ToList<BoeDTO>());

            ICollection<BoeTaskElementDTO> taskCollection = workspace.TaskElements.Where(t => t.Updateable == UpdateType.Upsert).ToList();
            ICollection<TravelDTO> travelCollection = workspace.Travels.Where(t => t.Updateable == UpdateType.Upsert).ToList();

            this.travelLoader.SaveTravels(travelCollection);

            // Use bulk save to insert resources and spreads (and correctly delete spreads)
            this.taskLoader.BulkSave(taskCollection);
        }

        /// <summary>
        /// Saves the clin data.
        /// </summary>
        /// <param name="clin">The clin.</param>
        private void SaveClinData(FullClin clin)
        {
            this.clinLoader.Save(clin);

            List<FullBoe> boeCollection = clin.Boes.Where(b => b.Updateable == UpdateType.Upsert).ToList();
            this.boeLoader.Save(boeCollection.ToList<BoeDTO>());

            ICollection<BoeTaskElementDTO> taskCollection = boeCollection.SelectMany(b => b.TaskElements).Where(t => t.Updateable == UpdateType.Upsert).ToList();
            ICollection<TravelDTO> travelCollection = boeCollection.SelectMany(b => b.Travels).Where(t => t.Updateable == UpdateType.Upsert).ToList();

            this.travelLoader.SaveTravels(travelCollection);

            // Use bulk save to insert resources and spreads (and correctly delete spreads)
            this.taskLoader.BulkSave(taskCollection);
        }

        /// <summary>
        /// Saves the boe data.
        /// </summary>
        /// <param name="boe">The boe.</param>
        private void SaveBoeData(FullBoe boe)
        {
            this.boeLoader.Save(boe);

            ICollection<BoeTaskElementDTO> taskCollection = boe.TaskElements.Where(t => t.Updateable == UpdateType.Upsert).ToList();
            ICollection<TravelDTO> travelCollection = boe.Travels.Where(t => t.Updateable == UpdateType.Upsert).ToList();

            this.travelLoader.SaveTravels(travelCollection);

            // Use bulk save to insert resources and spreads (and correctly delete spreads)
            this.taskLoader.BulkSave(taskCollection);
        }

        /// <summary>
        /// Saves the travel data.
        /// </summary>
        /// <param name="travelDTO">The travel dto.</param>
        private void SaveTravelData(TravelDTO travelDTO)
        {
            this.travelLoader.SaveTravels(new List<TravelDTO>() { travelDTO });
        }

        /// <summary>
        /// Saves the task data.
        /// </summary>
        /// <param name="task">The task.</param>
        private void SaveTaskData(BoeTaskElementDTO task)
        {
            // Use bulk save to insert resources and spreads (and correctly delete spreads)
            this.taskLoader.BulkSave(new List<BoeTaskElementDTO>() { task });
        }

        /// <summary>
        /// Adds email information.
        /// </summary>
        /// <param name="workspace">The workspace to adjust.</param>
        /// <param name="userID">user id.</param>
        /// <param name="boeDTO">boe.</param>
        /// <param name="wbsDisplayID">wbs display id.</param>
        /// <param name="wbsTitle">wbs title.</param>
        /// <param name="clinDisplayID">clin display id.</param>
        /// <param name="clinTitle">clin title.</param>
        /// <param name="orgSD">org SD.</param>
        /// <param name="orgED">org ED.</param>
        /// <param name="workspaceAdmin">workspace admin.</param>
        /// <param name="isError">If this is for a BOE with errors.</param>
        private void AddEmailInfo(WorkspaceDTO workspace, int userID, BoeDTO boeDTO, string wbsDisplayID, string wbsTitle, string clinDisplayID, string clinTitle, 
        string workspaceAdmin, DateTime orgSD, DateTime orgED, string dateShiftType, bool isError)
        {
            HashSet<UserDateChangeInfo> dateChangeInfo = isError ? this.userDateChangeInfoErrors : this.userDateChangeInfo;
            DateChangeInfo currentInfo = new DateChangeInfo
            {
                userID = userID,
                originalStartDate = orgSD,
                originalEndDate = orgED,
                boeID = boeDTO.Id,
                wbsDisplayID = wbsDisplayID,
                wbsTitle = wbsTitle,
                clinDisplayID = clinDisplayID,
                clinTitle = clinTitle,
                newStartDate = boeDTO.StartDate,
                newEndDate = boeDTO.EndDate
            };

            List<UserDateChangeInfo> temp = (from x in dateChangeInfo
                        where x.userID == userID
                        select x).ToList();

            if (temp.Any())
            {
                UserDateChangeInfo tempAuthorDateInfo = temp.First();

                tempAuthorDateInfo.AddBoeInfo(currentInfo);
            }
            else
            {
                UserDTO author = this.userDTOLoader.GetUserByID(userID);
                UserDateChangeInfo tempUserDateInfo = new UserDateChangeInfo
                {
                    userID = userID,
                    workspaceName = workspace.Shortname,
                    workspaceId = workspace.Id,
                    changedBy = workspaceAdmin,
                    email = author.EmailAddress,
                    dateShiftType = dateShiftType
                };

                tempUserDateInfo.AddBoeInfo(currentInfo);

                dateChangeInfo.Add(tempUserDateInfo);
            }
        }

        /// <summary>
        /// Generates and sends an email to notify the boe authors of a date change
        /// </summary>
        /// <param name="inputs">The Date Adjust inputs.</param>
        /// <param name="boeDTO">boe that changed</param>
        /// <param name="originalStartDate">original start date</param>
        /// <param name="originalEndDate">original end date</param>
        /// <param name="dateShiftModel">The date shift model.</param>
        /// <param name="isError">If this is for a BOE with errors.</param>
        private void GenerateEmail(DateShiftModelView dateShiftModel, Level level, BoeDTO boeDTO, bool isError)
        {
            string wbsTitle = string.Empty;
            string wbsDisplayID = string.Empty;
            string clinTitle = string.Empty;
            string clinDisplayID = string.Empty;

            UserDTO workspaceAdmin = this.userDTOLoader.GetUserForActiveUser();

            // all boe permissions
            ICollection<PermissionsDTO> permissions = this.permissionLoader.GetBOEPermissions(new List<int>() { boeDTO.Id });

            // retrieve the list of authors on the BOE
            List<int> authorsList = (from x in permissions
                               where x.Role == Role.Author || x.Role == Role.SubcontractorAuthor
                               select x.ETIUserId).ToList();

            if (boeDTO.WBSID.HasValue)
            {
                WbsDTO wbs = (from w in dateShiftModel.Workspace.WbsElements
                              where w.Id == boeDTO.WBSID.Value
                              select w).FirstOrDefault();

                if (wbs != null)
                {
                    wbsTitle = wbs.WbsTitle;
                    wbsDisplayID = wbs.WbsNumber;
                }
            }

            if (boeDTO.CLINID.HasValue)
            {
                ClinDTO clin = (from c in dateShiftModel.Workspace.Clins
                                where c.Id == boeDTO.CLINID.Value
                                select c).FirstOrDefault();

                if (clin != null)
                {
                    clinTitle = clin.ClinTitle;
                    clinDisplayID = clin.ClinNumber;
                }
            }

            // get the original start/end from databse
            BoeDTO originalBoe = this.boeLoader.GetById(boeDTO.Id);

            // for each Author
            foreach (int authorID in authorsList)
            {
                this.AddEmailInfo(dateShiftModel.Workspace, authorID, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, workspaceAdmin.DisplayName, originalBoe.StartDate, originalBoe.EndDate, GetDateShiftString(dateShiftModel, level), isError);
            }

            // Gather the approvers
            if (boeDTO.State != BOEState.Draft && boeDTO.State != BOEState.DraftLocked && boeDTO.State != BOEState.DateShiftDraft)
            {
                List<int> approvers = permissions.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).ToList();
                
                // build info for each approver
                foreach (int approver in approvers)
                {
                    this.AddEmailInfo(dateShiftModel.Workspace, approver, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, workspaceAdmin.DisplayName, originalBoe.StartDate, originalBoe.EndDate, GetDateShiftString(dateShiftModel, level), isError);
                }
            }
        }

        /// <summary>
        /// Get a string representation of the dateshift details.
        /// </summary>
        /// <param name="dateShiftModel">The dateshift model</param>
        /// <returns>A string representation of the dateshift details.</returns>
        private string GetDateShiftString(DateShiftModelView dateShiftModel, Level level)
        {
            List<string> details = new List<string>();
            foreach (DateShiftDetailModelView detail in dateShiftModel.Details)
            {
                switch (detail.Operation)
                {
                    case Operation.DurationChange:
                        if (detail.MonthChange < 0)
                        {
                            details.Add("Duration Shrink");
                        }
                        else if (detail.MonthChange > 0)
                        {
                            details.Add("Duration Expand");
                        }
                        break;
                    case Operation.Shift:
                        if (detail.MonthChange < 0)
                        {
                            details.Add("Shift Left");
                        }
                        else if (detail.MonthChange > 0)
                        {
                            details.Add("Shift Right");
                        }
                        break;
                }
            }

            return string.Join(" and ", details) + " on " + level.ToDescription();
        }

        /// <summary>
        /// Emails the boe authors errors only.
        /// </summary>
        /// <param name="dateShiftModel">The date shift model.</param>
        private void GenerateEmailBoeAuthorsErrorsOnly(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            // Get list of boeids that are in error
            ICollection<int> boeIds = dateShiftModel.Details.Last().Errors.AffectedBoeIds.ToList();

            ICollection<FullBoe> boes;
            switch (dateShiftable.DateShiftLevel)
            {
                case Level.BOE:
                    boes =  new FullBoe[] { dateShiftable as FullBoe };
                    break;
                case Level.CLIN:
                    boes = (dateShiftable as FullClin).Boes.Where(b => boeIds.Contains(b.Id)).ToList();
                    break;
                default:
                    boes = dateShiftModel.Workspace.Boes.Where(b => boeIds.Contains(b.Id)).ToList();
                    break;
            }

            foreach (FullBoe boe in boes)
            {
                this.GenerateEmail(dateShiftModel, dateShiftable.DateShiftLevel, boe, true);
            }
        }

        /// <summary>
        /// Emails the boe authors.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="dateShiftModel">The date shift model.</param>
        private void GenerateEmailBOEAuthors(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            ICollection<FullBoe> boes = this.GetAffectedBOEs(dateShiftable, dateShiftModel);
            foreach (FullBoe boe in boes)
            {
                this.GenerateEmail(dateShiftModel, dateShiftable.DateShiftLevel, boe, false);
            }
        }

        /// <summary>
        /// Gets the affected BOEs
        /// </summary>
        /// <param name="dateShiftable"></param>
        /// <param name="dateShiftModel"></param>
        /// <returns></returns>
        private ICollection<FullBoe> GetAffectedBOEs(IDateShiftable dateShiftable, DateShiftModelView dateShiftModel)
        {
            switch (dateShiftable.DateShiftLevel)
            {
                case Level.BOE:
                    return new FullBoe[] { dateShiftable as FullBoe };
                case Level.CLIN:
                    ICollection<FullBoe> clinBoes = (dateShiftable as FullClin).Boes.Where(b => b.Updateable == UpdateType.Upsert).ToList();

                    return clinBoes;
                default:
                    ICollection<FullBoe> boes = dateShiftModel.Workspace.Boes.Where(b => b.Updateable == UpdateType.Upsert).ToList();

                    return boes;
            }
        }

        /// <summary>
        /// Gets the attached boe identifier.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <returns>The boe id if found.</returns>
        private static int? GetAttachedBoeId(IDateShiftable dateShiftable)
        {
            int? boeId = null;
            if (dateShiftable is DateShiftable)
            {
                boeId = ((DateShiftable)dateShiftable).BoeId;
            }
            else
            {
				// TODO Thomas: Left off with boe being null here for some reason?
                switch (dateShiftable.DateShiftLevel)
                {
					case Level.BOE:
						boeId = ((DateShiftDTO)dateShiftable)?.BoeId;
						break;
					case Level.Task:
						boeId = ((DateShiftDTO)dateShiftable)?.BoeId;
						break;
					case Level.Travel:
						boeId = ((DateShiftDTO)dateShiftable)?.BoeId;
						break;
					default:
                        // no boe attached (clin/workspace/labor)
                        break;
                }
            }
            return boeId;
        }

        /// <summary>
        /// Adds the error information.
        /// </summary>
        /// <param name="dateShiftable">The date shiftable.</param>
        /// <param name="hasError">if set to <c>true</c> [has error].</param>
        /// <param name="detail">The detail.</param>
        private static void AddErrorInfo(IDateShiftable dateShiftable, DateShiftDetailModelView detail, int? parentBoeId)
        {
            int? boeId = GetAttachedBoeId(dateShiftable);
            if (boeId.HasValue)
            {
                detail.Errors.AffectedBoeIds.Add(boeId.Value);
            }
            else if (parentBoeId.HasValue)
            {
                detail.Errors.AffectedBoeIds.Add(parentBoeId.Value);
            }
        }
    }
}
