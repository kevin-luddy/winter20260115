// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BOEFormIBOEDTO : BOEFormDTO
    {
        /// <summary>
        /// ctor
        /// </summary>
        public BOEFormIBOEDTO() :
            base()
        {
        }

        /// <summary>
        /// The business area.
        /// </summary>
        public string BusinessArea { get; set; }

        /// <summary>
        /// The BOE Form type.
        /// </summary>
        public override BOEFormType BOEFormType
        {
            get
            {
                return BOEFormType.IBOE;
            }
        }
    }
}
