// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Dtos;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Full Workspace object for Project Map Workspaces.
    /// </summary>
    /// <seealso cref="GenBOE.Objects.FullWorkspace" />
    public class FullProjectMapWorkspace : FullWorkspace
    {
        /// <summary>
        /// The project map data.
        /// </summary>
        private IReadOnlyCollection<ProjectMapModelView> projectMapData;

        /// <summary>
        /// Dictionary of the boes by boe Id.
        /// </summary>
        private ReadOnlyDictionary<int, FullBoe> boesById;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal FullProjectMapWorkspace() : base()
        {
        }

        /// <summary>
        /// Constructor from workspace Dto
        /// </summary>
        /// <param name="workspace">Workspace</param>
        internal FullProjectMapWorkspace(WorkspaceDTO workspace) : base(workspace)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullProjectMapWorkspace"/> class.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        internal FullProjectMapWorkspace(FullProjectMapWorkspace workspace) : base(workspace)
        {
            this.projectMapData = workspace.projectMapData;
            this.SetConvertedData();
        }

        /// <summary>
        /// Gets the boes by id.
        /// </summary>
        public IReadOnlyDictionary<int, FullBoe> BoesById
        {
            get
            {
                this.LoadBoes();

                return this.boesById;
            }
        }

        /// <summary>
        /// Gets the project map data.
        /// </summary>
        public IReadOnlyCollection<ProjectMapModelView> ProjectMapData
        {
            get
            {
                if (this.projectMapData == null)
                {
                    this.RetrieveProjectMapData();
                }

                return this.projectMapData;
            }
        }

        #region Overrides that auto-retrieve from DB

        /// <summary>
        /// Clins belonging to the Workspace
        /// </summary>
        public override IReadOnlyCollection<FullClin> Clins
        {
            get
            {
                if (this.clins == null)
                {
                    this.SetConvertedData();
                }

                return this.clins;
            }
        }

        /// <summary>
        /// All task elements belonging to the workspace.
        /// </summary>
        public override IReadOnlyCollection<BoeTaskElementDTO> TaskElements
        {
            get
            {
                if (this.taskElements == null)
                {
                    this.SetConvertedData();
                }

                return this.taskElements;
            }
        }

        /// <summary>
        /// Wbs Elements belonging to the Workspace
        /// </summary>
        public override IReadOnlyCollection<FullWbs> WbsElements
        {
            get
            {
                if (this.wbsElements == null)
                {
                    this.SetConvertedData();
                }

                return this.wbsElements;
            }
        }

        #endregion Overrides that auto-retrieve from DB

        /// <summary>
        /// Clins belonging to the Workspace without Multi Clin
        /// </summary>
        public override IReadOnlyCollection<FullClin> ClinsNoMultiClin
        {
            get
            {
                return this.Clins;
            }
        }

        /// <summary>
        /// Wbs Elements belonging to the Workspace without Multi
        /// </summary>
        public override IReadOnlyCollection<FullWbs> WbsElementsNoMultiWbs
        {
            get
            {
                return this.WbsElements;
            }
        }

        /// <summary>
        /// Loads BOEs into the property.. Used if you want to preload the BOEs ahead of time
        /// </summary>
        public override void LoadBoes()
        {
            if (this.boes == null)
            {
                this.SetConvertedData();
            }
        }

        /// <summary>
        /// Populates RTE data for all Boes
        /// </summary>
        public override void LoadBoesRTEData()
        {
            if (this.boes == null)
            {
                this.SetConvertedData();
            }
        }

        /// <summary>
        /// Loads BOEs and Clins into the properties.. Used if you want to preload the BOEs and Clins ahead of time.
        /// </summary>
        public override void LoadClinsAndBoes()
        {
            if (this.boes == null)
            {
                this.SetConvertedData();
            }
        }

        /// <summary>
        /// Populates RTE data for Task Elements
        /// </summary>
        public override void LoadTaskElementRTEData()
        {
            if (this.taskElements == null)
            {
                this.SetConvertedData();
            }
        }

        /// <summary>
        /// Performing Orgs based on Boes's Tasks.
        /// </summary>
        public override IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes
        {
            get
            {
                if (this.performingOrgsUsedInBoes == null)
                {
                    ICollection<int> performingOrgIds = this.TaskElements.AsParallel().SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value)
                            .Distinct().ToList();

                    this.performingOrgsUsedInBoes = this.retriever.GetPerformingOrgsByIds(performingOrgIds).ToList().AsReadOnly();
                }

                return this.performingOrgsUsedInBoes;
            }
        }

        /// <summary>
        /// Returns back a collection of resources used by the Boes in the entire WS
        /// </summary>
        public override IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes
        {
            get
            {
                if (this.resourcesUsedInBoes == null)
                {
                    ICollection<int> resourceIds = this.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
                        .Distinct().ToList();

                    this.resourcesUsedInBoes = this.retriever.GetResourcesByIds(resourceIds).ToList().AsReadOnly();
                }

                return this.resourcesUsedInBoes;
            }
        }

        /// <summary>
        /// Retrieves the project map data.
        /// </summary>
        private void RetrieveProjectMapData()
        {
            this.projectMapData = this.retriever.GetProjectMapDataByWorkspaceId(this.Id).ToList().AsReadOnly();
        }

        /// <summary>
        /// Sets the converted data.
        /// </summary>
        /// <param name="models">The models.</param>
        private void SetConvertedData()
        {
            ConvertedProjectMapDTO convertedModels = ProjectMapConverter.ConvertToWorkspace(this.ProjectMapData, this);

            List<FullBoe> fullBoes = convertedModels.Boes.ToList();

            this.boes = fullBoes.AsReadOnly();
            this.boesById = new ReadOnlyDictionary<int, FullBoe>(fullBoes.ToDictionary(b => b.Id));
            this.taskElements = convertedModels.Tasks.ToList().AsReadOnly();
            this.clins = convertedModels.Clins.ToList().AsReadOnly();
            this.wbsElements = convertedModels.Wbs.ToList().AsReadOnly();
        }
    }
}
