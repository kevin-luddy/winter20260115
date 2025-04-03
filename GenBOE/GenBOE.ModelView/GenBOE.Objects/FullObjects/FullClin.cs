// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    using IES.Common;
    using IES.Common.classes;

    [Serializable()]
    public class FullClin : ClinDTO, IDateShiftable
    {
        [NonSerialized]
        private IRetriever retriever;

        private FullWorkspace workspace;
        private ReadOnlyCollection<FullBoe> boes;
        private ReadOnlyCollection<int> workspaceVariableIds;
        private ReadOnlyCollection<int> boeTaskVariableIds;

        internal FullClin() : base()
        {
            this.retriever = GenBOEUnityContainer.Resolve<IRetriever>();
        }

        /// <summary>
        /// Constructor based on the dto
        /// </summary>
        /// <param name="clin">Clin Dto</param>
        internal FullClin(ClinDTO clin) : this()
        {
            if (clin != null)
            {
                foreach (PropertyInfo prop in typeof(ClinDTO).GetProperties())
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(clin, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Boe Dto
        /// </summary>
        public IReadOnlyCollection<FullBoe> Boes
        {
            get
            {
                if (this.boes == null)
                {
                    this.boes = this.retriever.GetFullBoesByClinId(this.Id).ToList().AsReadOnly();
                }

                return this.boes;
            }
        }

        /// <summary>
        /// Workspace for the BOE
        /// </summary>
        public FullWorkspace Workspace
        {
            get
            {
                if (this.workspace == null)
                {
                    this.workspace = retriever.GetFullWorkspaceById(this.WorkspaceID);
                }

                return this.workspace;
            }
        }

        /// <summary>
        /// Gets Workspace Variables associated w/ the Clin
        /// </summary>
        public IReadOnlyCollection<int> WorkspaceVariableIds
        {
            get
            {
                if (this.workspaceVariableIds == null)
                {
                    this.workspaceVariableIds = this.retriever.GetWorkspaceVariablesByClinId(this.Id).ToList().AsReadOnly();
                }

                return this.workspaceVariableIds;
            }
        }

        /// <summary>
        /// Gets Boe Task Variables associated w/ the Clin
        /// </summary>
        public IReadOnlyCollection<int> BoeTaskVariableIds
        {
            get
            {
                if (this.boeTaskVariableIds == null)
                {
                    this.boeTaskVariableIds = this.retriever.GetTaskVariableIdsByClinId(this.Id).ToList().AsReadOnly();
                }

                return this.boeTaskVariableIds;
            }
        }

        /// <summary>
        /// Gets the children that can be shifted.
        /// </summary>
        public ICollection<IDateShiftable> Children
        {
            get
            {
                List<IDateShiftable> children = new List<IDateShiftable>(this.Boes);

                return children;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a spread of values.
        /// </summary>
        public bool HasSpread
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the date shift level.
        /// </summary>
        public Level DateShiftLevel
        {
            get
            {
                return Level.CLIN;
            }
        }

        /// <summary>
        /// Filters the input collection to only those BOEs with the same clin ID.
        /// </summary>
        /// <param name="inBoes"></param>
        public void SetBoes(ICollection<FullBoe> inBoes)
        {
            this.boes = inBoes.Where(x => x.CLINID == this.Id).ToList().AsReadOnly();
        }

		/// <summary>
		/// Convert from Full Object to DTO
		/// </summary>
		/// <returns>DTO version of this FullObject</returns>
		public ClinDTO ToDTO()
		{
			ClinDTO clin = new ClinDTO();
			Type type = typeof(ClinDTO);
			foreach (PropertyInfo prop in type.GetProperties())
			{
				if (prop.CanRead && prop.CanWrite)
				{
					type.GetProperty(prop.Name).SetValue(clin, prop.GetValue(this, null), null);
				}
			}

			return clin;
		}
	}
}