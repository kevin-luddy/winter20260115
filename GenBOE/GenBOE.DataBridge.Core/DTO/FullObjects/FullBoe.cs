//// -----------------------------------------------------------------------
//// <copyright company="Lockheed Martin Corporation">
////     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
//// </copyright>
//// -----------------------------------------------------------------------

//namespace GenBOE.DataBridge.Core.DTO.FullObjects
//{
//	using System;
//	using System.Collections.Generic;
//	using System.Collections.ObjectModel;
//	using System.Linq;
//	using GenBOE.DataBridge.Core.DTO;
//	using GenBOE.DataBridge.Core.ModelView;
//	using IES.Common.Core.Enums;
//	using IES.Common.Core.Interfaces;

//	[Serializable()]
//	public class FullBoe : BoeDTO, IDateShiftable
//	{
//		private FullWbs wbs;
//		private FullClin clin;
//		private FullWorkspace workspace;
//		private ReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables;
//		private ReadOnlyCollection<int> workspaceVariablesIdsForBoe;
//		private ReadOnlyCollection<MaterialDTO> materials;
//		private int? materialCount;
//		private ReadOnlyCollection<ResourceTypeDto> laborTypes;
//		private ReadOnlyCollection<BoeTaskElementDTO> taskElements;
//		private ReadOnlyCollection<int> taskVariableIds;
//		private ReadOnlyCollection<OtherDirectCostDTO> otherDirectCosts;
//		private ReadOnlyCollection<TravelDTO> travels;
//		private ReadOnlyCollection<BoeApproverResponseDTO> approverResponses;
//		private ReadOnlyCollection<BOECommentDTO> comments;
//		private ReadOnlyCollection<MoqTypeSelection> moqTypeSelections;

//		/// <summary>
//		/// Boe's Clin
//		/// </summary>
//		public FullClin Clin
//		{
//			get
//			{
//				return clin;
//			}
//		}

//		/// <summary>
//		/// Boe's Comments
//		/// </summary>
//		public IReadOnlyCollection<BOECommentDTO> Comments
//		{
//			get
//			{
//				return comments;
//			}
//		}

//		/// <summary>
//		/// Boe's Wbs
//		/// </summary>
//		public FullWbs Wbs
//		{
//			get
//			{
//				return wbs;
//			}
//		}

//		/// <summary>
//		/// Workspace for the BOE
//		/// </summary>
//		public FullWorkspace Workspace
//		{
//			get
//			{
//				return workspace;
//			}

//			// Added this so it can be manually set, avoiding the need to load it from the DB
//			set
//			{
//				workspace = value;
//			}
//		}

//		/// <summary>
//		/// All workspace variables belonging to the workspace.
//		/// </summary>
//		public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables
//		{
//			get
//			{
//				return workspaceVariables;
//			}
//		}

//		/// <summary>
//		/// All workspace variable Ids where the BOE is part of the sum of BOEs for the variable.
//		/// </summary>
//		public IReadOnlyCollection<int> WorkspaceVariablesIdsForBoe
//		{
//			get
//			{
//				return workspaceVariablesIdsForBoe;
//			}
//		}

//		/// <summary>
//		/// Gets all Materials for the BOE.
//		/// </summary>
//		public IReadOnlyCollection<MaterialDTO> Materials
//		{
//			get
//			{
//				return materials;
//			}
//		}

//		/// <summary>
//		/// Gets the number of materials for the BOE
//		/// </summary>
//		public int MaterialCount
//		{
//			get
//			{
//				return materialCount.Value;
//			}
//		}

//		/// <summary>
//		/// Gets all Other Direct Costs for the BOE.
//		/// </summary>
//		public IReadOnlyCollection<OtherDirectCostDTO> OtherDirectCosts
//		{
//			get
//			{
//				return otherDirectCosts;
//			}
//		}

//		/// <summary>
//		/// Labor Types
//		/// </summary>
//		public IReadOnlyCollection<ResourceTypeDto> LaborTypes
//		{
//			get
//			{
//				return laborTypes;
//			}
//		}

//		/// <summary>
//		/// Gets a list of task elements associated with the Boe.
//		/// </summary>
//		public IReadOnlyCollection<BoeTaskElementDTO> TaskElements
//		{
//			get
//			{
//				return taskElements;
//			}
//		}

//		/// <summary>
//		/// Gets a collection of Travels elements associated with the Boe.
//		/// </summary>
//		public IReadOnlyCollection<TravelDTO> Travels
//		{
//			get
//			{
//				return travels;
//			}
//		}

//		/// <summary>
//		/// Gets a list of approver responses associated with a Boe.
//		/// </summary>
//		/// <param name="boeId">Boe Id.</param>
//		/// <returns>Approver responses associated to a Boe.</returns>
//		public IReadOnlyCollection<BoeApproverResponseDTO> ApproverResponses
//		{
//			get
//			{
//				return approverResponses;
//			}
//		}

//		/// <summary>
//		/// Task Variable Ids
//		/// </summary>
//		public IReadOnlyCollection<int> TaskVariableIds
//		{
//			get
//			{
//				return taskVariableIds;
//			}
//		}

//		/// <summary>
//		/// Does Boe contain Discrete Labor Types
//		/// </summary>
//		public bool ContainsDiscreteLaborTypes
//		{
//			get
//			{
//				return LaborTypes.Any(x => x.SpreadCurveID == SpreadCurves.DiscreteHours);
//			}
//		}

//		/// <summary>
//		/// Gets the children that can be shifted.
//		/// </summary>
//		public ICollection<IDateShiftable> Children
//		{
//			get
//			{
//				List<IDateShiftable> children = new List<IDateShiftable>();
//				if (TaskElements.Any())
//				{
//					children.AddRange(TaskElements);
//				}

//				if (Travels.Any())
//				{
//					children.AddRange(Travels);
//				}

//				return children;
//			}
//		}

//		/// <summary>
//		/// Gets a value indicating whether this instance has a spread of values.
//		/// </summary>
//		public bool HasSpread
//		{
//			get
//			{
//				return false;
//			}
//		}

//		/// <summary>
//		/// Gets the date shift level.
//		/// </summary>
//		public Level DateShiftLevel
//		{
//			get
//			{
//				return Level.BOE;
//			}
//		}

//		/// <summary>
//		/// Gets or sets the start date.
//		/// </summary>
//		DateTime? IDateShiftable.StartDate
//		{
//			get
//			{
//				return StartDate;
//			}

//			set
//			{
//				StartDate = value ?? DateTime.MinValue;
//			}
//		}

//		/// <summary>
//		/// Gets or sets the end date.
//		/// </summary>
//		DateTime? IDateShiftable.EndDate
//		{
//			get
//			{
//				return EndDate;
//			}

//			set
//			{
//				EndDate = value ?? DateTime.MinValue;
//			}
//		}

//		/// <summary>
//		/// Overrides the task elements
//		/// </summary>
//		/// <param name="tasks">Task Elements</param>
//		public void SetTaskElements(IEnumerable<BoeTaskElementDTO> tasks)
//		{
//			taskElements = tasks.Where(t => t.BoeID == Id).ToList().AsReadOnly();
//		}

//		/// <summary>
//		/// Filters the input collection to only those travels with the same boe ID.
//		/// </summary>
//		/// <param name="travels">The travels.</param>
//		public void SetTravels(ICollection<TravelDTO> travelDtos)
//		{
//			travels = travelDtos.Where(t => t.BoeID == Id).ToList().AsReadOnly();
//		}

//		/// <summary>
//		/// Updates the workspace variables.
//		/// </summary>
//		/// <param name="variables">The variables.</param>
//		public void UpdateWorkspaceVariables(ICollection<WorkspaceVariableDTO> variables)
//		{
//			workspaceVariables = variables.ToList().AsReadOnly();
//		}

//		/// <summary>
//		/// Template Questions & Answers
//		/// </summary>
//		public ICollection<RTECustomTemplateQuestionAnswerModelView> TemplateQuestionsAndAnswers { get; set; }

//		/// <summary>
//		/// MOQ Type Selections for the BOE
//		/// </summary>
//		public IReadOnlyCollection<MoqTypeSelection> MoqTypeSelections
//		{
//			get
//			{
//				return moqTypeSelections;
//			}
//		}
//	}
//}
