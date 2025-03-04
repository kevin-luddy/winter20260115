//// -----------------------------------------------------------------------
//// <copyright company="Lockheed Martin Corporation">
////     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
//// </copyright>
//// -----------------------------------------------------------------------

//namespace GenBOE.DataBridge.Core.DTO.FullObjects
//{
//	using System.Collections.Generic;
//	using System.Collections.ObjectModel;
//	using System.Linq;
//	using IES.Common.Core.Models;

//	/// <summary>
//	/// Full Workspace object for Project Map Workspaces.
//	/// </summary>
//	/// <seealso cref="FullWorkspace" />
//	public class FullProjectMapWorkspace : FullWorkspace
//	{
//		/// <summary>
//		/// The project map data.
//		/// </summary>
//		private IReadOnlyCollection<ProjectMapModelView> projectMapData;

//		/// <summary>
//		/// Dictionary of the boes by boe Id.
//		/// </summary>
//		private ReadOnlyDictionary<int, FullBoe> boesById;

//		/// <summary>
//		/// Default constructor
//		/// </summary>
//		internal FullProjectMapWorkspace() : base()
//		{
//		}

//		/// <summary>
//		/// Constructor from workspace Dto
//		/// </summary>
//		/// <param name="workspace">Workspace</param>
//		internal FullProjectMapWorkspace(WorkspaceDTO workspace) : base(workspace)
//		{
//		}

//		/// <summary>
//		/// Initializes a new instance of the <see cref="FullProjectMapWorkspace"/> class.
//		/// </summary>
//		/// <param name="workspace">The workspace.</param>
//		internal FullProjectMapWorkspace(FullProjectMapWorkspace workspace) : base(workspace)
//		{
//			projectMapData = workspace.projectMapData;
//			SetConvertedData();
//		}

//		/// <summary>
//		/// Gets the boes by id.
//		/// </summary>
//		public IReadOnlyDictionary<int, FullBoe> BoesById
//		{
//			get
//			{
//				LoadBoes();

//				return boesById;
//			}
//		}

//		/// <summary>
//		/// Gets the project map data.
//		/// </summary>
//		public IReadOnlyCollection<ProjectMapModelView> ProjectMapData
//		{
//			get
//			{
//				if (projectMapData == null)
//				{
//					RetrieveProjectMapData();
//				}

//				return projectMapData;
//			}
//		}

//		#region Overrides that auto-retrieve from DB

//		/// <summary>
//		/// Clins belonging to the Workspace
//		/// </summary>
//		public override IReadOnlyCollection<FullClin> Clins
//		{
//			get
//			{
//				if (clins == null)
//				{
//					SetConvertedData();
//				}

//				return clins;
//			}
//		}

//		/// <summary>
//		/// All task elements belonging to the workspace.
//		/// </summary>
//		public override IReadOnlyCollection<BoeTaskElementDTO> TaskElements
//		{
//			get
//			{
//				if (taskElements == null)
//				{
//					SetConvertedData();
//				}

//				return taskElements;
//			}
//		}

//		/// <summary>
//		/// Wbs Elements belonging to the Workspace
//		/// </summary>
//		public override IReadOnlyCollection<FullWbs> WbsElements
//		{
//			get
//			{
//				if (wbsElements == null)
//				{
//					SetConvertedData();
//				}

//				return wbsElements;
//			}
//		}

//		#endregion Overrides that auto-retrieve from DB

//		/// <summary>
//		/// Clins belonging to the Workspace without Multi Clin
//		/// </summary>
//		public override IReadOnlyCollection<FullClin> ClinsNoMultiClin
//		{
//			get
//			{
//				return Clins;
//			}
//		}

//		/// <summary>
//		/// Wbs Elements belonging to the Workspace without Multi
//		/// </summary>
//		public override IReadOnlyCollection<FullWbs> WbsElementsNoMultiWbs
//		{
//			get
//			{
//				return WbsElements;
//			}
//		}

//		/// <summary>
//		/// Loads BOEs into the property.. Used if you want to preload the BOEs ahead of time
//		/// </summary>
//		public override void LoadBoes()
//		{
//			if (boes == null)
//			{
//				SetConvertedData();
//			}
//		}

//		/// <summary>
//		/// Populates RTE data for all Boes
//		/// </summary>
//		public override void LoadBoesAndTaskElementsRTEData()
//		{
//			if (boes == null)
//			{
//				SetConvertedData();
//			}
//		}

//		/// <summary>
//		/// Loads BOEs and Clins into the properties.. Used if you want to preload the BOEs and Clins ahead of time.
//		/// </summary>
//		public override void LoadClinsAndBoes(bool loadBoeRteData = false)
//		{
//			if (boes == null)
//			{
//				SetConvertedData();
//			}
//		}

//		/// <summary>
//		/// Populates RTE data for Task Elements
//		/// </summary>
//		public override void LoadTaskElementRTEData()
//		{
//			if (taskElements == null)
//			{
//				SetConvertedData();
//			}
//		}

//		/// <summary>
//		/// Performing Orgs based on Boes's Tasks.
//		/// </summary>
//		public override IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes
//		{
//			get
//			{
//				if (performingOrgsUsedInBoes == null)
//				{
//					ICollection<int> performingOrgIds = TaskElements.AsParallel().SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value)
//							.Distinct().ToList();

//					performingOrgsUsedInBoes = retriever.GetPerformingOrgsByIds(performingOrgIds).ToList().AsReadOnly();
//				}

//				return performingOrgsUsedInBoes;
//			}
//		}

//		/// <summary>
//		/// Returns back a collection of resources used by the Boes in the entire WS
//		/// </summary>
//		public override IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes
//		{
//			get
//			{
//				if (resourcesUsedInBoes == null)
//				{
//					ICollection<int> resourceIds = TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList();

//					resourcesUsedInBoes = retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
//				}

//				return resourcesUsedInBoes;
//			}
//		}

//		/// <summary>
//		/// Retrieves the project map data.
//		/// </summary>
//		private void RetrieveProjectMapData()
//		{
//			projectMapData = retriever.GetProjectMapDataByWorkspaceId(Id).ToList().AsReadOnly();
//		}

//		/// <summary>
//		/// Sets the converted data.
//		/// </summary>
//		/// <param name="models">The models.</param>
//		private void SetConvertedData()
//		{
//			ConvertedProjectMapDTO convertedModels = ProjectMapConverter.ConvertToWorkspace(ProjectMapData, this);

//			List<FullBoe> fullBoes = convertedModels.Boes.ToList();

//			boes = fullBoes.AsReadOnly();
//			boesById = new ReadOnlyDictionary<int, FullBoe>(fullBoes.ToDictionary(b => b.Id));
//			taskElements = convertedModels.Tasks.ToList().AsReadOnly();
//			clins = convertedModels.Clins.ToList().AsReadOnly();
//			wbsElements = convertedModels.Wbs.ToList().AsReadOnly();
//		}
//	}
//}
