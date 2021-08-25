// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using GenBOE.Dtos;

    public class BOEFormIBOEModelView : BOEFormModelView
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public BOEFormIBOEModelView()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public BOEFormIBOEModelView(BOEFormIBOEDTO dto) : base(dto)
        {
            if (ReferenceEquals(dto, null))
            {
                throw new ArgumentNullException(nameof(dto));
            }

            this.BusinessArea = dto.BusinessArea;
        }


        /// <summary>
        /// The business area.
        /// </summary>
        public string BusinessArea { get; set; }
    }
}
