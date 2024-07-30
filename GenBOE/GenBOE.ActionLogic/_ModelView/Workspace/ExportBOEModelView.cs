//// -----------------------------------------------------------------------
//// <copyright company="Lockheed Martin Corporation">
////     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
//// </copyright>
//// -----------------------------------------------------------------------

//namespace GenBOE.ActionLogic.ModelView.Workspace
//{
//    using System;
//    using GenBOE.DataBridge.DTO;
//    using GenBOE.Dtos;
//    using IES.Common;
//    using IES.Common.classes;

//    public class ExportBOEModelView : PersistedDataModelView
//    {
//        public ExportBOEModelView()
//        {
//            this.BoeID = -1;
//            this.BOETitle = string.Empty;
//            this.WBSText = String.Empty;
//            this.CLINText = string.Empty;
//            this.Status = string.Empty;
//            this.State = BOEState.None;
//            this.isMaterial = false;
//        }

//        public ExportBOEModelView(BoeDTO inBoeDTO, WbsDTO inWbsDTO, ClinDTO inClinDTO, string inStatus, bool inIsMaterial)
//            : this()
//        {
//            if (inBoeDTO == null)
//            {
//                throw new ArgumentNullException(nameof(inBoeDTO));
//            }

//            this.BoeID = inBoeDTO.Id;
//            this.BOETitle = inBoeDTO.Title;
//            this.WBSText = inWbsDTO != null ? inWbsDTO.WbsString : CommonConstants.Unassigned_WBS_Display_Text;
//            this.CLINText = inClinDTO != null ? inClinDTO.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;
//            this.PaddedWbsName = (inWbsDTO != null) ? inWbsDTO.WbsPaddedNumber : string.Empty;
//            this.PaddedClinName = (inClinDTO != null) ? inClinDTO.ClinPaddedNumber : string.Empty;
//            this.Status = inStatus;
//            this.State = inBoeDTO.State;
//            this.isMaterial = inIsMaterial;
//        }

//        public int? BoeID { get; set; }
//        public string BOETitle { get; set; }
//        public string WBSText { get; set; }
//        public string CLINText { get; set; }
//        public string Status { get; set; }
//        public BOEState State { get; set; }
//        public bool isMaterial { get; set; }
//        public string PaddedWbsName { get; set; }
//        public string PaddedClinName { get; set; }
//    }
//}
