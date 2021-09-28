// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
 
	public class UserDateChangeInfo
    {
        public string dateShiftType { get; set; }
        public int? userID { get;  set; }
        public string workspaceName { get; set; }
        public int workspaceId { get; set; }
        public string email { get; set; }
        public string changedBy { get; set; }

        private HashSet<DateChangeInfo> dateChangeInfo = new HashSet<DateChangeInfo>();

        public void AddBoeInfo(DateChangeInfo inBoe)
        {
            this.dateChangeInfo.Add(inBoe);
        }

        public HashSet<DateChangeInfo> GetAllDateChangeInfo()
        {
            return this.dateChangeInfo;
        }

        public DateChangeInfo GetBoeInfo(int inBoeID)
        {
            DateChangeInfo toReturn = (from x in this.dateChangeInfo
                       where x.boeID == inBoeID
                       select x).FirstOrDefault();

            return toReturn;
        }
    }

    public class DateChangeInfo
    {
        public int userID { get; set; }
        public int boeID { get; set; }
        public string wbsDisplayID { get; set; }
        public string wbsTitle { get; set; }
        public string clinDisplayID { get; set; }
        public string clinTitle { get; set; }
        public DateTime originalStartDate { get; set; }
        public DateTime originalEndDate { get; set; }
        public DateTime newStartDate { get; set; }
        public DateTime newEndDate { get; set; }
        public BOEState originalState { get; set; }

        public DateChangeInfo(int inUserID, int inBoeID, string inWbsDisplayID, string inWbsTitle, string inClinDisplayID, string inClinTitle, DateTime inOriginalStartDate,
                            DateTime inOriginalEndDate, DateTime inNewStartDate, DateTime inNewEndDate, BOEState inOriginalState)
        {
            this.userID = inUserID;
            this.boeID = inBoeID;
            this.wbsDisplayID = inWbsDisplayID;
            this.wbsTitle = inWbsTitle;
            this.clinDisplayID = inClinDisplayID;
            this.clinTitle = inClinTitle;
            this.originalStartDate = inOriginalStartDate;
            this.originalEndDate = inOriginalEndDate;
            this.newStartDate = inNewStartDate;
            this.newEndDate = inNewEndDate;
            this.originalState = inOriginalState;
        }

        public DateChangeInfo() { }
    }

}
