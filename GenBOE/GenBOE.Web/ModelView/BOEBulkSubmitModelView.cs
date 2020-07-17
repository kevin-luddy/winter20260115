// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using DataBridge.DTO;
    using Dtos;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// A Model view for a BOE Bulk Submit row
    /// </summary>
    public class BOEBulkSubmitModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEBulkSubmitModelView"/> class.
        /// </summary>
        public BOEBulkSubmitModelView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEBulkSubmitModelView"/> class.
        /// </summary>
        /// <param name="boe">The BOE.</param>
        /// <param name="wbsDTO">The WBS dto.</param>
        /// <param name="clinDTO">The CLIN dto.</param>
        public BOEBulkSubmitModelView(BoeDTO boe, WbsDTO wbsDTO, ClinDTO clinDTO)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            this.Id = boe.Id;
            this.BOETitle = boe.Title;
            this.State = boe.State;
            this.WBSText = wbsDTO != null ? wbsDTO.WbsString : CommonConstants.Unassigned_WBS_Display_Text;
            this.CLINText = clinDTO != null ? clinDTO.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the BOE title.
        /// </summary>
        public string BOETitle { get; set; }
        
        /// <summary>
        /// Gets or sets the WBS text.
        /// </summary>
        public string WBSText { get; set; }
        
        /// <summary>
        /// Gets or sets the CLIN text.
        /// </summary>
        public string CLINText { get; set; }
        
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public BOEState State { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this boe has validation messages.
        /// </summary>
        public bool Invalid { get; set; }
        
        /// <summary>
        /// Gets the status.
        /// </summary>
        public string Status
        {
            get
            {
                if (this.State == BOEState.None)
                {
                    return string.Empty;
                }
                else if (this.State == BOEState.DraftLocked)
                {
                    return BOEState.Draft.GetDescription();
                }
                else
                {
                    return this.State.GetDescription();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is awaiting approval.
        /// </summary>
        public bool isAwaitingApproval
        {
            get
            {
                return this.State == BOEState.AwaitingApproval;
            }
        }
    }
}

