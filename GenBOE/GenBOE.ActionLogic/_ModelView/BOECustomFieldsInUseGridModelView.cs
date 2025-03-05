// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.ComponentModel.DataAnnotations;
    using IES.Common;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View for the BOE Custom Fields Grid
    /// </summary>
    public class BOECustomFieldsInUseGridModelView : BOECustomFieldsGridModelView
	{
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOECustomFieldsInUseGridModelView() : base()
        {
            this.inUse = false;
        }

        /// <summary>
        /// Constructor taking in a Custom Field DTO
        /// </summary>
        /// <param name="customFieldDTO">Custom Field DTO</param>
        public BOECustomFieldsInUseGridModelView(CustomFieldDTO customFieldDTO)
            : base(customFieldDTO)
        {
            if (customFieldDTO != null)
            {
                this.inUse = false;
            }
        }

        /// <summary>
        /// Gets/Sets bool noting if Custom Field is in use
        /// </summary>
        public bool inUse { get; set; }
    }
}
