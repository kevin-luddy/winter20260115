using System;
using IES.Common;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class FindReplaceDTO
    {
        public int BOEId { get; set; }
        public string BOETitle { get; set; }
        public int TaskElementID { get; set; }
        public int WorkspaceID { get; set; }
        public string FieldName { get; set; }
        public int FieldEnum { get; set; }
        public string FoundText { get; set; }
        public int Occurences { get; set; }
        public string WBSInfo { get; set; }
        public string ClinInfo { get; set; }
        public string FindText { get; set; }
        public string ReplaceText { get; set; }
        public string CenteredText { get; set; }
        public string TextAfterReplace { get; set; }
        public string TextBeforeFind { get; set; }
        public string TextAfterFind { get; set; }
        public bool toSave { get; set; }
        public bool tooLong { get; set; }
        public FindReplaceElementType FindReplaceTaskElementType { get; set; }
        
    }
}