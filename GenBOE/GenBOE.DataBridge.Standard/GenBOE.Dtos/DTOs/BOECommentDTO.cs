// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Standard;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BOECommentDTO : UpdateableDTO, IBOEMembership
    {
        public BOECommentDTO()
        {
            Id = -1;
            FieldID = -1;
            BOEComment = string.Empty;
            BOEResponseToCommentID = null;
            BOECommentETIUserID = -1;
            BoeID = -1;
        }

        public int FieldID { get; set; }
        public string BOEComment { get; set; }
        public int? BOEResponseToCommentID { get; set; }
        public int BOECommentETIUserID { get; set; }
        public int BoeID { get; set; }
    }
}
