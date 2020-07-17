// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common.classes;

    public class BOEHeaderISGSModelView : BOEHeaderModelView, IBOEHeaderModelView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BOEHeaderISGSModelView()
        {
            this.Title = String.Empty;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BOEHeaderISGSModelView(BoeDTO inBoe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers)
            : base(inBoe, answers)
        {
            if (inBoe == null)
            {
                throw new ArgumentNullException(nameof(inBoe), "inBoe cannot be null");
            }
            this.Title = inBoe.Title;
        }

        /// <summary>
        /// Indicates if the Title field is required
        /// </summary>
        public override Boolean IsTitleRequired 
        { 
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Sources of data label
        /// </summary>
        public new String LabelSourcesOfData 
        { 
            get
            {
                return CommonConstants.LABEL_TEXT_SOURCE_OF_DATA_ISGS;
            }
        }
    }
}