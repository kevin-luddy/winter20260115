// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common.classes;

    public class BOEHeaderSpaceModelView : BOEHeaderModelView, IBOEHeaderModelView
    {       
        public BOEHeaderSpaceModelView()
        {
            base.Title = String.Empty;
        }

        public BOEHeaderSpaceModelView(BoeDTO inBoe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers) : base(inBoe, answers)
        {
            if (inBoe == null)
            {
                throw new ArgumentNullException(nameof(inBoe), "inBoe cannot be null");
            }
            base.Title = inBoe.Title;
        }

        [Required(ErrorMessage = BoeDTO.BOE_TITLE_REQUIRED)]
        public override String Title
        {
            get { return base.Title; }
            set { base.Title = value; }
        }

        /// <summary>
        /// Indicates if the Title field is required
        /// </summary>
        public override Boolean IsTitleRequired
        {
            get
            {
                return true;
            }

        }

        /// <summary>
        /// Gets the Sources of data label
        /// </summary>
        public new String LabelSourcesOfData
        {
            get
            {
                return CommonConstants.LABEL_TEXT_SOURCE_OF_DATA_SSC;
            }
        }
    }
}