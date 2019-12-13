using System;
using System.Collections.ObjectModel;
using IES.Common;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{

    public class FindReplaceResultsModelView : PagedResultsModelView<GenBOE.Web.ModelView.FindReplaceResult>
    {

        public FindReplaceResultsModelView()
        {
            WorkspaceID = 0;
            WorkspaceName = string.Empty;
            this.FindResults = new Collection<FindReplaceResult>();
            CurrentPage = 1;
            PagedIndexes = new Collection<FindReplaceResult>();
            ResultsPerPage = 10;
        }

        public FindReplaceResultsModelView(Collection<FindReplaceDTO> inFindDTOColl)
        {
            if (inFindDTOColl == null)
            {
                throw new ArgumentNullException(nameof(inFindDTOColl));
            }

            WorkspaceID = 0;
            WorkspaceName = string.Empty;
            this.FindResults = new Collection<FindReplaceResult>();
            CurrentPage = 1;
            PagedIndexes = new Collection<FindReplaceResult>();
            ResultsPerPage = 10;

            foreach (FindReplaceDTO findDTO in inFindDTOColl)
            {
                FindReplaceResult newResult = new FindReplaceResult(findDTO);
                this.PagedIndexes.Add(newResult);
                if (FindResults.Count < ResultsPerPage)
                {
                    this.FindResults.Add(newResult);
                }
            }

        }

        public Collection<FindReplaceResult> FindResults { get; set; }

        public int WorkspaceID { get; set; }
        public string WorkspaceName { get; set; }
        
        
    }

    public class FindReplaceResult
    {
        public FindReplaceResult()
        {
            BOEId = -1;
            TaskElementID = -1;
            WorkspaceID = -1;
            FieldName = null;
            FieldEnum = -1;
            FoundText = null;
            Occurences = -1;
            WBSInfo = null;
            ClinInfo = null;
            FindText = null;
            ReplaceText = null;
            CenteredText = null;
            TextAfterReplace = null;
            TextBeforeFind = null;
            TextAfterFind = null;
            toSave = false;
            tooLong = false;
            FindReplaceTaskElementType = FindReplaceElementType.BOE;
        }


        public FindReplaceResult(FindReplaceDTO inFindDTO)
        {
            if (inFindDTO == null)
            {
                throw new ArgumentNullException(nameof(inFindDTO));
            }

            BOEId = inFindDTO.BOEId;
            BOETitle = inFindDTO.BOETitle;
            TaskElementID = inFindDTO.TaskElementID;
            WorkspaceID = inFindDTO.WorkspaceID;
            FieldName = inFindDTO.FieldName;
            FieldEnum = inFindDTO.FieldEnum;
            FoundText = inFindDTO.FoundText;
            Occurences = inFindDTO.Occurences;
            WBSInfo = inFindDTO.WBSInfo;
            ClinInfo = inFindDTO.ClinInfo;
            FindText = inFindDTO.FindText;
            ReplaceText = inFindDTO.ReplaceText;
            CenteredText = inFindDTO.CenteredText;
            TextAfterReplace = inFindDTO.TextAfterReplace;
            TextBeforeFind = inFindDTO.TextBeforeFind;
            TextAfterFind = inFindDTO.TextAfterFind;
            toSave = inFindDTO.toSave;
            tooLong = inFindDTO.tooLong;
            FindReplaceTaskElementType = inFindDTO.FindReplaceTaskElementType;

        }

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