// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
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
		/// The loader for date shifts.
		/// </summary>
		private readonly IDateShiftDTODataLoader dateShiftLoader;

		/// <summary>
		/// The boe loader
		/// </summary>
		private readonly IBoeDTODataLoader boeLoader;

		/// <summary>
		/// The workspace version data loader.
		/// </summary>
		private readonly IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataDTODataLoader;

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
		public DateShiftCalculation()
		{
			this.dateShiftLoader = GenBOEUnityContainer.Container.Resolve(typeof(IDateShiftDTODataLoader)) as IDateShiftDTODataLoader;
			this.boeLoader = GenBOEUnityContainer.Container.Resolve(typeof(IBoeDTODataLoader)) as IBoeDTODataLoader;
			this.userDTOLoader = GenBOEUnityContainer.Container.Resolve(typeof(IUserDTODataLoader)) as IUserDTODataLoader;
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
		/// <param name="dateShiftDTO">The date shiftable object.</param>
		/// <param name="dateShiftModel">The dateshift model.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="validateOnly">If this should only validate the dateshift.</param>
		/// <param name="parentLevel">The parent level.</param>
		/// <param name="workspaceShortname">Workspace ShortName</param>
		/// <param name="fullWorkspace">The full workspace</param>
		/// <exception cref="ArgumentNullException">on incoming params</exception>
		public void PerformDateShift(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel, DateTime? parentStart, DateTime? parentEnd,
			bool validateOnly, Level parentLevel, string workspaceShortname, FullWorkspace fullWorkspace)
		{
			if (dateShiftDTO == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTO));
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

			if (!dateShiftDTO.StartDate.HasValue || !dateShiftDTO.EndDate.HasValue)
			{
				throw new ArgumentException("Object to DateShift does not have a valid start or end time.");
			}

			// Perform Shifts
			PerformShifts(dateShiftDTO, dateShiftModel, parentStart, parentEnd, null, parentLevel, workspaceShortname);

			// Error handling - only need to throw the errors for the last detail (duration change) in case there are both a shift and duration change
			if (dateShiftModel.Details.Last().Errors.Messages.Any())
			{
				throw new GenValidationException(dateShiftModel.Details.Last().Errors.Messages.Select(m => new ValidationMessage(m)));
			}

			if (!validateOnly)
			{
				// Pre-load emails (to get original start/end dates)
				this.GenerateEmails(dateShiftDTO, dateShiftModel);


				// Check if we have Skill Mix enabled to adjust Skill Mix table data as needed
				if (Utilities.ShowSkillMixForWorkspace(fullWorkspace?.CreationDate, fullWorkspace.Shortname))
				{

					// Iterate through each task and check for Skill Mix
					foreach (BoeTaskElementDTO task in fullWorkspace?.TaskElements)
					{
						if (BOETaskUtility.ShowSkillMixForTask(fullWorkspace, task))
						{
							bool isBRCEnabled = Utilities.IsBRCEnabledForWorkspace(workspaceShortname);

							// Run Skill Mix update
							ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours = fullWorkspace?.MoqTypeSelections
								.SelectMany(moqType => moqType.TableData)
								.SelectMany(tableData => tableData.ResourceHours)
								.ToList();

							FullBoe fullBoe = fullWorkspace.Boes.First(b => b.Id == task.BoeID);
							LaborTaskDataModelView laborTasks = this.boeLaborControllerLogic.ConvertDtoToModelView(fullWorkspace, fullBoe, task);

							bool allAutomaticMOQTypes = fullWorkspace.MoqTypeSelections?.All(m => m.SelectedMOQType == MOQType.Comparative || m.SelectedMOQType == MOQType.Historical) ?? true;

							bool isManual = !fullWorkspace.EnableSAPConnection || !allAutomaticMOQTypes;

							RefreshSkillMixModelView response = SkillMixUtility.RefreshSkillMixTables(resourceHours,
								laborTasks.LaborTypesData, task.SkillMixTable, task.CommonDisclosureTable, isBRCEnabled, isManual);

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
				this.Save(dateShiftDTO, dateShiftModel);

				// Send emails
				this.SendEmails(dateShiftModel);
			}
		}

		/// <summary>
		/// Performs the shifts.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable object.</param>
		/// <param name="dateShiftModel">The dateshift model containing details.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="parentBoeId">The parent's BOE ID.</param>
		/// <param name="parentLevel">The parent's Level.</param>
		internal static void PerformShifts(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
		{
			// Perform dateshift on this object
			if (!dateShiftDTO.StartDate.HasValue || !dateShiftDTO.EndDate.HasValue)
			{
				dateShiftDTO.StartDate = parentStart;
				dateShiftDTO.EndDate = parentEnd;
			}

			foreach (DateShiftDetailModelView detail in dateShiftModel.Details)
			{
				PerformShiftOperation(dateShiftDTO, detail);
				Validate(dateShiftDTO, detail, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);
				PerformChildrenShift(dateShiftDTO, parentStart, parentEnd, parentBoeId, detail, dateShiftModel, workspaceShortname);
			}
		}

		/// <summary>
		/// Performs the children shift.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="modelView">The Dateshift modelview.</param>
		/// <param name="workspaceShortname">Workspace shortname</param>
		/// <exception cref="NotSupportedException"></exception>
		private static void PerformChildrenShift(DateShiftDTO dateShiftDTO, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, DateShiftDetailModelView detail, DateShiftModelView modelView, string workspaceShortname)
		{
			// Perform dateshift on child objects
			if (dateShiftDTO.HasSpread)
			{
				switch (dateShiftDTO.DateShiftLevel)
				{
					case Level.Labor:
						PerformLaborSpreadShift(dateShiftDTO, detail, modelView);
						Validate(dateShiftDTO, detail, parentStart, parentEnd, parentBoeId, Level.Task, workspaceShortname);
						break;
					default:
						throw new NotSupportedException(string.Format("Data class setup incorrectly, Class with level {0} has Spreads.", dateShiftDTO.DateShiftLevel.ToString()));
				}
			}
			else
			{
				RecursiveChildShifts(dateShiftDTO, detail, parentStart, parentEnd, modelView, workspaceShortname);
			}
		}

		/// <summary>
		/// Recursively dateshifts the children.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable object to pull children from.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="grandParentStart">The object's parent start.</param>
		/// <param name="grandParentEnd">The object's parent end.</param>
		/// <param name="workspaceShortname">workspace shortname</param>
		internal static void RecursiveChildShifts(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, DateTime? grandParentStart, DateTime? grandParentEnd, DateShiftModelView modelView, string workspaceShortname)
		{
			if (dateShiftDTO.Children.Any())
			{
				// if parent's start/end are not valid, use grandparent's
				DateTime? pStart = dateShiftDTO.StartDate;
				DateTime? pEnd = dateShiftDTO.EndDate;
				if (!pStart.HasValue || !pEnd.HasValue)
				{
					pStart = grandParentStart;
					pEnd = grandParentEnd;
				}

				int? parentBoeId = GetAttachedBoeId(dateShiftDTO);

				foreach (DateShiftDTO child in dateShiftDTO.Children)
				{
					if (detail.ChildModificationType == ChildModificationType.NoChange)
					{
						// validate the no change
						Validate(child, detail, pStart, pEnd, parentBoeId, dateShiftDTO.DateShiftLevel, workspaceShortname);

						// recursive call to recursive validate
						PerformChildrenShift(child, pStart, pEnd, parentBoeId, detail, modelView, workspaceShortname);
					}
					else
					{
						PerformChildShift(child, detail, pStart, pEnd);

						// validate the shift
						Validate(child, detail, pStart, pEnd, parentBoeId, dateShiftDTO.DateShiftLevel, workspaceShortname);

						// recursive call
						PerformChildrenShift(child, pStart, pEnd, parentBoeId, detail, modelView, workspaceShortname);
					}
				}
			}
		}

		/// <summary>
		/// Performs the shift for a child.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="parentStart">The parent start time</param>
		/// <param name="parentEnd">The parent end time.</param>
		internal static void PerformChildShift(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd)
		{
			if (dateShiftDTO.StartDate.HasValue && dateShiftDTO.EndDate.HasValue)
			{
				dateShiftDTO.Updateable = UpdateType.Upsert;
				switch (detail.ChildModificationType)
				{
					case ChildModificationType.FlowDown:
						PerformShiftOperation(dateShiftDTO, detail);
						break;
					case ChildModificationType.ToEnd:
						if (parentEnd.HasValue && dateShiftDTO.StartDate.HasValue && dateShiftDTO.EndDate.HasValue)
						{
							int negativeDuration = -1 * dateShiftDTO.StartDate.Value.MonthDifference(dateShiftDTO.EndDate.Value);
							dateShiftDTO.EndDate = parentEnd.Normalize();
							if (detail.Operation == Operation.Shift)
							{
								// make sure this was a negative shift
								if (detail.MonthChange > 0)
								{
									throw new NotSupportedException("Positive Shifts cannot have Child Modification Type set to End.");
								}
								dateShiftDTO.StartDate = dateShiftDTO.EndDate.Value.AddMonths(negativeDuration).Normalize();
							}
						}
						break;
					case ChildModificationType.ToPoP:
						if (parentStart.HasValue && parentEnd.HasValue)
						{
							dateShiftDTO.StartDate = parentStart.Value.Normalize();
							dateShiftDTO.EndDate = parentEnd.Value.Normalize();
						}
						break;
					case ChildModificationType.ToStart:
						if (parentStart.HasValue && dateShiftDTO.StartDate.HasValue && dateShiftDTO.EndDate.HasValue)
						{
							if (detail.Operation != Operation.Shift || detail.MonthChange < 0)
							{
								throw new NotSupportedException("Only a Positive Shift can have Child Modification Type set to Start.");
							}

							int duration = dateShiftDTO.StartDate.Value.MonthDifference(dateShiftDTO.EndDate.Value);
							dateShiftDTO.StartDate = parentStart.Normalize();
							dateShiftDTO.EndDate = dateShiftDTO.StartDate.Value.AddMonths(duration);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Performs the shift operation.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		internal static void PerformShiftOperation(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail)
		{
			switch (detail.Operation)
			{
				case Operation.Shift:
					dateShiftDTO.StartDate = dateShiftDTO.StartDate.Value.AddMonths(detail.MonthChange).Normalize();
					break;
				case Operation.DurationChange:
					break;
			}

			dateShiftDTO.EndDate = dateShiftDTO.EndDate.Value.AddMonths(detail.MonthChange).Normalize();
			dateShiftDTO.Updateable = UpdateType.Upsert;
		}

		/// <summary>
		/// Validates the specified date shiftable.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="parentBoeId">The parent's BOE ID.</param>
		/// <param name="parentLevel">The parent's level.</param>
		/// <param name="workspaceShortname">Workspace short name</param>
		internal static void Validate(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
		{
			CheckErrors(dateShiftDTO, detail, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);

			bool rerunError1Check = detail.Errors.HasError1 && detail.Error1FixSingleMonth == true;
			bool rerunError2Check = detail.Errors.HasError2 && detail.Error2Handling.HasValue;
			if (rerunError1Check || rerunError2Check)
			{
				DateShiftDetailModelView secondCheck = detail.Clone();
				CheckErrors(dateShiftDTO, secondCheck, parentStart, parentEnd, parentBoeId, parentLevel, workspaceShortname);

				if (rerunError1Check)
				{
					if (secondCheck.Errors.HasError1)
					{
						detail.Errors.HasError1 = true;
						AddErrorInfo(dateShiftDTO, detail, parentBoeId);
						detail.Errors.Messages.Add(string.Format("Negative duration for a {0}.", dateShiftDTO.DateShiftLevel.ToDescription()));
					}
				}

				if (rerunError2Check)
				{
					if (secondCheck.Errors.HasError2)
					{
						detail.Errors.HasError2 = true;
						AddErrorInfo(dateShiftDTO, detail, parentBoeId);
						detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftDTO.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
					}
				}
			}
		}

		/// <summary>
		/// Checks the errors.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="parentStart">The parent start.</param>
		/// <param name="parentEnd">The parent end.</param>
		/// <param name="parentBoeId">The parent's BOE ID.</param>
		/// <param name="parentLevel">The parent's Level.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "<Pending>")]
		private static void CheckErrors(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, DateTime? parentStart, DateTime? parentEnd, int? parentBoeId, Level parentLevel, string workspaceShortname)
		{
			if (dateShiftDTO.StartDate.HasValue && dateShiftDTO.EndDate.HasValue)
			{
				// Check for negative duration
				if (dateShiftDTO.EndDate.Normalize() < dateShiftDTO.StartDate.Normalize())
				{
					if (detail.Error1FixSingleMonth == true)
					{
						dateShiftDTO.EndDate = dateShiftDTO.StartDate.Normalize();
					}
					else
					{
						detail.Errors.HasError1 = true;
						AddErrorInfo(dateShiftDTO, detail, parentBoeId);
						detail.Errors.Messages.Add(string.Format("Negative duration for a {0}.", dateShiftDTO.DateShiftLevel.ToDescription()));
					}
				}

				// check to make sure range is within PoP (Period of Performance)
				if (parentStart.HasValue && parentEnd.HasValue)
				{
					DateTime oneLMXStartDate = Utilities.OneLmxStartDate;
					DateTime previousStartDate = dateShiftDTO.StartDate.Value;
					DateTime previousEndDate = dateShiftDTO.EndDate.Value;
					DateTime? shiftedStartDate = null;
					DateTime? shiftedEndDate = null;

					if (dateShiftDTO.StartDate.Normalize() < parentStart.Normalize())
					{
						if (detail.Error2Handling.HasValue)
						{
							switch (detail.Error2Handling.Value)
							{
								case ChildModificationType.NoChange:
									AddErrorInfo(dateShiftDTO, detail, parentBoeId);
									break;
								case ChildModificationType.ToStart:
									if (detail.Operation == Operation.DurationChange || detail.MonthChange < 0)
									{
										throw new NotSupportedException("This Child Modification Type is not allowed for Error Child outside the PoP of the Parent. To Start is only allowed for Error Handling when the Operation is a positive Shift.");
									}
									dateShiftDTO.StartDate = parentStart;
									shiftedStartDate = parentStart.Value;

									if (dateShiftDTO.EndDate.Normalize() < parentStart.Normalize())
									{
										dateShiftDTO.EndDate = parentStart;
										shiftedEndDate = parentStart.Value;
									}
									break;
								case ChildModificationType.ToEnd:
									dateShiftDTO.EndDate = parentEnd;
									shiftedEndDate = parentEnd.Value;
									if (dateShiftDTO.StartDate.Normalize() > parentEnd.Normalize())
									{
										dateShiftDTO.StartDate = parentEnd;
										shiftedStartDate = parentEnd.Value;
									}
									break;
								case ChildModificationType.ToPoP:
									dateShiftDTO.StartDate = parentStart;
									dateShiftDTO.EndDate = parentEnd;
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
							AddErrorInfo(dateShiftDTO, detail, parentBoeId);
							detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftDTO.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
						}
					}

					if (dateShiftDTO.EndDate.Normalize() > parentEnd.Normalize())
					{
						if (detail.Error2Handling.HasValue)
						{
							switch (detail.Error2Handling.Value)
							{
								case ChildModificationType.NoChange:
									AddErrorInfo(dateShiftDTO, detail, parentBoeId);
									break;
								case ChildModificationType.ToEnd:
									int duration = dateShiftDTO.StartDate.Value.MonthDifference(dateShiftDTO.EndDate.Value);
									dateShiftDTO.EndDate = parentEnd;
									shiftedEndDate = parentEnd.Value;
									if (detail.Operation == Operation.Shift)
									{
										// only change start date if this is a shift
										dateShiftDTO.StartDate = parentEnd.Value.AddMonths(-1 * duration);
										shiftedStartDate = parentEnd.Value.AddMonths(-1 * duration);
									}
									break;
								case ChildModificationType.ToPoP:
									dateShiftDTO.StartDate = parentStart;
									dateShiftDTO.EndDate = parentEnd;
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
							AddErrorInfo(dateShiftDTO, detail, parentBoeId);
							detail.Errors.Messages.Add(string.Format("{0} is outside the PoP of parent {1}.", dateShiftDTO.DateShiftLevel.ToDescription(), parentLevel.ToDescription()));
						}
					}

					if (Utilities.IsBRCEnabledForWorkspace(workspaceShortname))
					{
						if (previousStartDate < oneLMXStartDate)
						{
							if (shiftedStartDate.HasValue && shiftedStartDate.Value >= oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftDTO, detail, parentBoeId, "Resource and Business Resource Code", "Start Date", "shifted to the right of");
							}
						}

						if (previousEndDate < oneLMXStartDate)
						{
							if (shiftedEndDate.HasValue && shiftedEndDate.Value >= oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftDTO, detail, parentBoeId, "Resource", "End Date", "shifted to the right of");
							}
						}

						if (previousStartDate >= oneLMXStartDate)
						{
							if (shiftedStartDate.HasValue && shiftedStartDate.Value < oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftDTO, detail, parentBoeId, "Resource and Business Resource Code", "Start Date", "shifted to the left of");
							}
						}

						if (previousEndDate >= oneLMXStartDate)
						{
							if (shiftedEndDate.HasValue && shiftedEndDate.Value < oneLMXStartDate)
							{
								AddResourceTypeError(dateShiftDTO, detail, parentBoeId, "Resource and Business Resource Code", "End Date", "shifted to the left of");
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Populate Details Model with proper Error message
		/// </summary>
		/// <param name="dateShiftDTO">Shiftable Model</param>
		/// <param name="detail">Date Shift Table Model</param>
		/// <param name="parentBoeId">Parent BOE ID</param>
		/// <param name="type">Type affect (Resource and/or Business Resource Code)</param>
		/// <param name="affectedDate">Start or End Date affected</param>
		/// <param name="shift">Shift to left or right of 1LMX Cutoff Date</param>
		private static void AddResourceTypeError(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, int? parentBoeId, string type, string affectedDate, string shift)
		{
			detail.Errors.HasError2 = true;
			AddErrorInfo(dateShiftDTO, detail, parentBoeId);
			detail.Errors.Messages.Add($"{affectedDate} has {shift} 1LMX Start Date. {type} will be affected");
		}

		/// <summary>
		/// Performs the task spreads shift.
		/// </summary>
		/// <param name="parentTask">The parent task.</param>
		/// <param name="detail">The detail.</param>
		/// <param name="modelView">The model view.</param>
		/// <exception cref="NotSupportedException">The parentTask is set to Level.Labor but is not a Resource Labor Type.</exception>
		internal static void PerformLaborSpreadShift(DateShiftDTO parentTask, DateShiftDetailModelView detail, DateShiftModelView modelView)
		{
			if (detail.ChildModificationType != ChildModificationType.NoChange)
			{
				if (parentTask.DateShiftLevel != Level.Labor)
				{
					throw new NotSupportedException("The parentTask is set to Level.Labor but is not a Resource Labor Type.");
				}

				// move the spreads
				PerformSpreadsShift(parentTask, detail, modelView);
			}
		}

		/// <summary>
		/// Performs a DateShift on the Spreads of a Resource Labor.
		/// </summary>
		/// <param name="labor">The Resource Labor.</param>
		/// <param name="detail">The details for the DateShift.</param>
		/// <param name="modelView">The model view.</param>
		internal static void PerformSpreadsShift(DateShiftDTO labor, DateShiftDetailModelView detail, DateShiftModelView modelView)
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

								if (spread.LaborSpreadDate.CompareTo(labor.EndDate) > 0 && (detail.SpreadHandling == SpreadHandling.DiscreteToFirst || detail.SpreadHandling == SpreadHandling.DiscreteToLast))
								{
									tempTotal += spread.LaborSpreadValue;
								}

								spread.Updateable = UpdateType.Deleted;
							}

							LaborSpreadRequest spreadRequest = new LaborSpreadRequest()
							{
								CurveID = labor.SpreadCurveID,
								HourSpread = 0,
								StartDate = labor.StartDate.Value,
								EndDate = labor.EndDate.Value
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

									if (spreadDate.CompareTo(labor.StartDate) == 0)
									{
										spread.LaborSpreadValue += tempTotal;
									}
								}
								else if (detail.SpreadHandling == SpreadHandling.DiscreteToLast)
								{
									DateTime spreadDate = GenBOEUtilities.AdjustDateTimePrecision(spread.LaborSpreadDate, DateTimePrecision.Month);

									if (spreadDate.CompareTo(labor.EndDate) == 0)
									{
										spread.LaborSpreadValue += tempTotal;
									}
								}

								spread.Updateable = UpdateType.Upsert;
								spread.BoeID = labor.BoeId;
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
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="dateShiftModel">The model view.</param>
		private void GenerateEmails(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			switch (dateShiftModel.EmailOption)
			{
				case EmailOption.EmailBOEAuthors:
					this.GenerateEmailBOEAuthors(dateShiftDTO, dateShiftModel);
					this.GenerateEmailBoeAuthorsErrorsOnly(dateShiftDTO, dateShiftModel);
					break;
				case EmailOption.EmailBOEAuthorsErrorsOnly:
					this.GenerateEmailBoeAuthorsErrorsOnly(dateShiftDTO, dateShiftModel);
					break;
				default:
					// do nothing
					break;
			}
		}

		/// <summary>
		/// Saves the specified date shiftable objects.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable parent object.</param>
		/// <param name="dateShiftModel">The model view.</param>
		private void Save(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			if (dateShiftDTO.DateShiftLevel == Level.Workspace)
			{
				// separate transaction for backup
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					// make a backup first
					string versionName = "SYSTEM: DATE SHIFT " + DateTime.Now.ToString();
					ICollection<WorkspaceVersionMetaDataDTO> workspaceVersions = dateShiftDTO.WorkspaceVersionMetaData;

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
							VersionState = dateShiftDTO.WorkspaceState
						}
					};

					this.workspaceVersionMetaDataDTODataLoader.Save(toSave, dateShiftDTO.Id);

					scope.Complete();
				}
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{

				ICollection<DateShiftDTO> dateShiftedItems = new List<DateShiftDTO> { dateShiftDTO };

				this.dateShiftLoader.Update(dateShiftedItems);

				// Transition BOEs to Draft if needed
				this.TransitionBOEs(dateShiftDTO, dateShiftModel);

				scope.Complete();
			}
		}

		/// <summary>
		/// Transitions the BOEs to Draft if needed.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable parent object.</param>
		/// <param name="dateShiftModel">The model view.</param>
		private void TransitionBOEs(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			// get all the affected BOEs that are not in Draft
			ICollection<DateShiftDTO> affectedBOEs = this.GetAffectedBOEs(dateShiftDTO, dateShiftModel);

			IList<DateShiftDTO> boesToChangeState = affectedBOEs.Where(b =>
				b.BOEStateID != (int)BOEState.Draft &&
				b.BOEStateID != (int)BOEState.DraftLocked &&
				b.BOEStateID != (int)BOEState.DateShiftDraft
			).ToList();

			if (boesToChangeState.Any())
			{
				// we need to get the updated boes from the db (updateddate)
				ICollection<FullBoe> updatedBOEs = this.factory.CreateFullBoes(boesToChangeState.Select(b => b.Id).ToList());

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
		/// Adds email information.
		/// </summary>
		/// <param name="workspace">The workspace to adjust.</param>
		/// <param name="userID">user id.</param>
		/// <param name="boeData">boe.</param>
		/// <param name="wbsDisplayID">wbs display id.</param>
		/// <param name="wbsTitle">wbs title.</param>
		/// <param name="clinDisplayID">clin display id.</param>
		/// <param name="clinTitle">clin title.</param>
		/// <param name="orgSD">org SD.</param>
		/// <param name="orgED">org ED.</param>
		/// <param name="workspaceAdmin">workspace admin.</param>
		/// <param name="isError">If this is for a BOE with errors.</param>
		private void AddEmailInfo(WorkspaceDTO workspace, int userID, DateShiftDTO boeData, string wbsDisplayID, string wbsTitle, string clinDisplayID, string clinTitle,
		string workspaceAdmin, DateTime orgSD, DateTime orgED, string dateShiftType, bool isError)
		{
			HashSet<UserDateChangeInfo> dateChangeInfo = isError ? this.userDateChangeInfoErrors : this.userDateChangeInfo;
			DateChangeInfo currentInfo = new DateChangeInfo
			{
				userID = userID,
				originalStartDate = orgSD,
				originalEndDate = orgED,
				boeID = boeData.Id,
				wbsDisplayID = wbsDisplayID,
				wbsTitle = wbsTitle,
				clinDisplayID = clinDisplayID,
				clinTitle = clinTitle,
				newStartDate = boeData.StartDate.Value,
				newEndDate = boeData.EndDate.Value
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
		/// <param name="boeDTO">boe that changed</param>
		/// <param name="dateShiftModel">The date shift model.</param>
		/// <param name="isError">If this is for a BOE with errors.</param>
		private void GenerateEmail(DateShiftModelView dateShiftModel, Level level, DateShiftDTO boeDTO, bool isError)
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

			if (boeDTO.WbsId.HasValue)
			{
				WbsDTO wbs = (from w in dateShiftModel.Workspace.WbsElements
							  where w.Id == boeDTO.WbsId.Value
							  select w).FirstOrDefault();

				if (wbs != null)
				{
					wbsTitle = wbs.WbsTitle;
					wbsDisplayID = wbs.WbsNumber;
				}
			}

			if (boeDTO.ClinId.HasValue)
			{
				ClinDTO clin = (from c in dateShiftModel.Workspace.Clins
								where c.Id == boeDTO.ClinId.Value
								select c).FirstOrDefault();

				if (clin != null)
				{
					clinTitle = clin.ClinTitle;
					clinDisplayID = clin.ClinNumber;
				}
			}

			// get the original start/end from databse
			DateShiftDTO originalBoe = this.dateShiftLoader.GetDateShiftObject(Level.BOE, boeDTO.Id);

			// for each Author
			foreach (int authorID in authorsList)
			{
				this.AddEmailInfo(dateShiftModel.Workspace, authorID, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, workspaceAdmin.DisplayName, originalBoe.StartDate.Value, originalBoe.EndDate.Value, GetDateShiftString(dateShiftModel, level), isError);
			}

			// Gather the approvers
			if (boeDTO.BOEStateID != (int)BOEState.Draft && boeDTO.BOEStateID != (int)BOEState.DraftLocked && boeDTO.BOEStateID != (int)BOEState.DateShiftDraft)
			{
				List<int> approvers = permissions.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).ToList();

				// build info for each approver
				foreach (int approver in approvers)
				{
					this.AddEmailInfo(dateShiftModel.Workspace, approver, boeDTO, wbsDisplayID, wbsTitle, clinDisplayID, clinTitle, workspaceAdmin.DisplayName, originalBoe.StartDate.Value, originalBoe.EndDate.Value, GetDateShiftString(dateShiftModel, level), isError);
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
		private void GenerateEmailBoeAuthorsErrorsOnly(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			// Get list of boeids that are in error
			ICollection<int> boeIds = dateShiftModel.Details.Last().Errors.AffectedBoeIds.ToList();

			ICollection<DateShiftDTO> boes;
			switch (dateShiftDTO.DateShiftLevel)
			{
				case Level.BOE:
					boes = new DateShiftDTO[] { dateShiftDTO };
					break;
				case Level.CLIN:
					boes = dateShiftDTO.Children.Where(clin => boeIds.Contains(clin.Id)).ToList();
					break;
				default:
					boes = dateShiftModel.Workspace.Boes.Select(boe => new DateShiftDTO
					{
						DateShiftLevel = IES.Common.Level.BOE,
						BOEStateID = boe.Id,
						ParentId = boe.CLINID ?? boe.WorkspaceID,
						ClinId = boe.CLINID,
						WbsId = boe.WBSID,
						WorkspaceId = boe.WorkspaceID,
						StartDate = boe.StartDate,
						EndDate = boe.EndDate,
						HasSpread = boe.HasSpread,
						Updateable = boe.Updateable
					}).Where(d => boeIds.Contains(d.BOEStateID)).ToList();
					break;
			}

			foreach (DateShiftDTO boe in boes)
			{
				this.GenerateEmail(dateShiftModel, dateShiftDTO.DateShiftLevel, boe, true);
			}
		}

		/// <summary>
		/// Emails the boe authors.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="dateShiftModel">The date shift model.</param>
		private void GenerateEmailBOEAuthors(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			ICollection<DateShiftDTO> boes = this.GetAffectedBOEs(dateShiftDTO, dateShiftModel);
			foreach (DateShiftDTO boe in boes)
			{
				this.GenerateEmail(dateShiftModel, dateShiftDTO.DateShiftLevel, boe, false);
			}
		}

		/// <summary>
		/// Gets the affected BOEs
		/// </summary>
		/// <param name="dateShiftDTO"></param>
		/// <param name="dateShiftModel"></param>
		/// <returns></returns>
		private ICollection<DateShiftDTO> GetAffectedBOEs(DateShiftDTO dateShiftDTO, DateShiftModelView dateShiftModel)
		{
			switch (dateShiftDTO.DateShiftLevel)
			{
				case Level.BOE:
					return new DateShiftDTO[] { dateShiftDTO };
				case Level.CLIN:
					ICollection<DateShiftDTO> clinBoes = dateShiftDTO.Children.Where(d => d.Updateable == UpdateType.Upsert).ToList();
					return clinBoes;
				default:
					// Default at workspace level.
					IEnumerable<DateShiftDTO> boesAsDateShiftDTOs = dateShiftModel.Workspace.Boes.Select(b => new DateShiftDTO
					{
						DateShiftLevel = IES.Common.Level.BOE,
						BOEStateID = b.Id,
						ParentId = b.CLINID ?? b.WorkspaceID,
						ClinId = b.CLINID,
						WbsId = b.WBSID,
						WorkspaceId = b.WorkspaceID,
						StartDate = b.StartDate,
						EndDate = b.EndDate,
						HasSpread = b.HasSpread,
						Updateable = b.Updateable
					});

					IEnumerable<DateShiftDTO> upsertBoes = boesAsDateShiftDTOs.Where(d => d.Updateable == UpdateType.Upsert);
					ICollection<DateShiftDTO> boes = upsertBoes.ToList();
					return boes;
			}
		}

		/// <summary>
		/// Gets the attached boe identifier.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <returns>The boe id if found.</returns>
		private static int? GetAttachedBoeId(DateShiftDTO dateShiftDTO)
		{
			int? boeId = dateShiftDTO.BoeId;
			return boeId;
		}

		/// <summary>
		/// Adds the error information.
		/// </summary>
		/// <param name="dateShiftDTO">The date shiftable.</param>
		/// <param name="detail">The detail.</param>
		private static void AddErrorInfo(DateShiftDTO dateShiftDTO, DateShiftDetailModelView detail, int? parentBoeId)
		{
			int? boeId = GetAttachedBoeId(dateShiftDTO);
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
