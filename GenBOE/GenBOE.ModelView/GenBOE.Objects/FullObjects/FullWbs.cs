// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using GenBOE.Dtos;
    using IES.Common.classes;

    [Serializable()]
    public class FullWbs : WbsDTO
    {
        private ReadOnlyCollection<ClinDTO> clins;
        private ReadOnlyCollection<FullBoe> boes;
        private ReadOnlyCollection<FullBoe> boesWithNesting;
        private FullWorkspace workspace;

        [NonSerialized]
        private IRetriever retriever;

        internal FullWbs() : base()
        {
            this.retriever = GenBOEUnityContainer.Resolve<IRetriever>();
        }

        /// <summary>
        /// Constructor based on the dto
        /// </summary>
        /// <param name="wbs">Wbs Dto</param>
        internal FullWbs(WbsDTO wbs) : this()
        {
            if (wbs != null)
            {
                foreach (PropertyInfo prop in typeof(WbsDTO).GetProperties())
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(wbs, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Clin Dtos
        /// </summary>
        public IReadOnlyCollection<ClinDTO> Clins
        {
            get
            {
                if (this.clins == null)
                {
                    this.clins = this.retriever.GetClinsByIds(this.ClinIDs).ToList().AsReadOnly();
                }

                return this.clins;
            }
        }

        /// <summary>
        /// Boes for the Wbs
        /// </summary>
        public IReadOnlyCollection<FullBoe> Boes
        {
            get
            {
                if (this.boes == null)
                {
                    this.boes = this.retriever.GetBoesByWbs(this.Id).ToList().AsReadOnly();
                }

                return this.boes;
            }
        }

        /// <summary>
        /// Boes WITH nesting for the Wbs
        /// </summary>
        public IReadOnlyCollection<FullBoe> BoesWithNesting
        {
            get
            {
                if (this.boesWithNesting == null)
                {
                    this.boesWithNesting = this.retriever.GetBoesWithNestingByWbs(this.Id).ToList().AsReadOnly();
                }

                return this.boesWithNesting;
            }
        }

        /// <summary>
        /// Associated workspace
        /// </summary>
        public FullWorkspace Workspace
        {
            get
            {
                if (this.workspace == null)
                {
                    this.workspace = this.retriever.GetFullWorkspaceById(this.WorkspaceID);
                }

                return this.workspace;
            }
        }

        /// <summary>
        /// All Parent Wbs Elements
        /// </summary>
        public ICollection<FullWbs> AllParentWbs
        {
            get
            {
                return this.retriever.GetParentWbs(this.WorkspaceID, this.WbsNumber);
            }
        }
        
        /// <summary>
        /// All Child Wbs Elements
        /// </summary>
        public ICollection<FullWbs> AllChildWbs
        {
            get
            {
                return this.retriever.GetChildWbs(this.WorkspaceID, this.WbsNumber);
            }
        }

        /// <summary>
        /// Gets task variable ids for Wbs
        /// </summary>
        public ICollection<int> TaskVariableIds
        {
            get
            {
                return this.retriever.GetTaskVariablesByWbsId(this.Id);
            }
        }

        /// <summary>
        /// Gets workspace variables for the Wbs
        /// </summary>
        public ICollection<int> WorkspaceVariableIds
        {
            get
            {
                return this.retriever.GetWorkspaceVariablesByWbsId(this.Id);
            }
        }
    }
}
