using System;
using GenBOE.Business.Workspace;

namespace GenBOE.ActionLogic.ModelView.BOE
{
    /// <summary>
    /// ModelView for the dates available to update for the BOE date changing
    /// </summary>
    public class BOEUpdateDatesModelView : DateChangeModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BOEUpdateDatesModelView()
        {
        }

        /// <summary>
        /// Should we send out an email
        /// </summary>
        public bool SendEmailForBoeUpdates { get; set; }

        public DateChangeFlowdown GetDateChangeFlowdown()
        {
            DateChangeFlowdown dateChange = new DateChangeFlowdown()
            {
                FlowdownUpdateSelection = this.FlowdownUpdateSelection,
                DiscreteResourceSelection = this.DiscreteResourceSelection,
                DiscreteResourcesExist = this.DiscreteResourcesExist,
                SendEmailForBoeUpdates = this.SendEmailForBoeUpdates,
                NewStartDate = !string.IsNullOrEmpty(NewStartDate) ? Convert.ToDateTime(NewStartDate) : new DateTime(),
                NewEndDate = !string.IsNullOrEmpty(NewEndDate) ? Convert.ToDateTime(NewEndDate) : new DateTime()
            };

            return dateChange;
        }
    }
}