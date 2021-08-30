// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View for Home Page Metrics
    /// </summary>
    public class GenBOEMetricsModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenBOEMetricsModelView"/> class.
        /// </summary>
        /// <param name="inGenBOEMetricsDTO">The in gen BOE metrics dto.</param>
        /// <exception cref="System.ArgumentNullException">inGenBOEMetricsDTO</exception>
        public GenBOEMetricsModelView(GenBOEMetricsDTO inGenBOEMetricsDTO)
        {
            if (inGenBOEMetricsDTO == null)
            {
                throw new ArgumentNullException(nameof(inGenBOEMetricsDTO));
            }

            this.usersTotal = inGenBOEMetricsDTO.usersTotal;
            
            #region Populate Usage Metrics MV

            this.UsageMetrics = new UsageMetricsModelView();
            this.UsageMetrics.WorkspacesAll = inGenBOEMetricsDTO.WorkspacesAll;
            this.UsageMetrics.WorkspacesClosed = inGenBOEMetricsDTO.WorkspacesClosed;
            this.UsageMetrics.WorkspacesComplete = inGenBOEMetricsDTO.WorkspacesComplete;
            this.UsageMetrics.WorkspacesInitialization = inGenBOEMetricsDTO.WorkspacesInitialization;
            this.UsageMetrics.WorkspacesLocked = inGenBOEMetricsDTO.WorkspacesLocked;
            this.UsageMetrics.WorkspacesWorking = inGenBOEMetricsDTO.WorkspacesWorking;
            this.UsageMetrics.BoesAll = inGenBOEMetricsDTO.BoesAll;
            this.UsageMetrics.BoesApproved = inGenBOEMetricsDTO.BoesApproved;
            this.UsageMetrics.BoesAwaitingApproval = inGenBOEMetricsDTO.BoesAwaitingApproval;
            this.UsageMetrics.BoesDraft = inGenBOEMetricsDTO.BoesDraft;
            this.UsageMetrics.BoesUnassigned = inGenBOEMetricsDTO.BoesUnassigned;

            #endregion
        }

        /// <summary>
        /// Gets or sets the users total.
        /// </summary>
        public int usersTotal { get; set; }

        /// <summary>
        /// Gets or sets the usage metrics.
        /// </summary>
        public UsageMetricsModelView UsageMetrics { get; set; }
    }
}