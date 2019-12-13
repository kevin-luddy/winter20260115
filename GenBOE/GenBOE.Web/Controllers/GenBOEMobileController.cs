using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using GenBOE.Business.Metrics;
using GenBOE.Common;
using GenBOE.DataBridge.Common;
using GenBOE.DataBridge.Common.Interfaces;
using GenBOE.DataBridge.DTO;
using GenBOE.Objects;
using GenBOE.Web.Common;
using GenBOE.ActionLogic.ModelView.BOE;
using System.IO;
using GenBOE.ActionLogic.ControllerLogic;
using GenBOE.ActionLogic;
using GenBOE.Dtos;
using System.Text.RegularExpressions;

namespace GenBOE.Web.Controllers
{
    public class GenBOEMobileController : GenBOEController
    {
        private IReportsControllerLogic reportsControllerLogic;
        private BOECommentsControllerLogic boeCommentsLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public GenBOEMobileController(ISecurityAccess inSecurityAccess, 
            CommonDataMapper inCommonDataMapper, SiteMasterUtilities inSiteMasterUtilities,
            UserDTODataLoader inUserDTODataLoader,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IPermissionsDTODataLoader inPermissionsLoader,
            IReportsControllerLogic inReportsControllerLogic,
            BOECommentsControllerLogic inBoeCommentsLogic,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, inPermissionsLoader, inControllerLogic)
        {
            this.reportsControllerLogic = inReportsControllerLogic;
            this.boeCommentsLogic = inBoeCommentsLogic;
        }

        /// <summary>
        /// Returns details of BOE
        /// </summary>
        /// <param name="boeID"></param>
        /// <returns></returns>
        virtual public JsonResult GetBOEDetails(string workspace, int boeID)
        {
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            IDictionary<int, BOEStateModelView> boeStateDictionary = this._CommonDataMapper.getBOEStatesDictionary();
            BOEMobileModelView theBOE = new BOEMobileModelView(boe, boeStateDictionary);
            theBOE.BOEComments = boeCommentsLogic.GetCommentsByBOEId(boeID);

            // remove HTML tags from description
            const string HTML_TAG_PATTERN = "<.*?>";
            theBOE.Description = Regex.Replace(theBOE.Description, HTML_TAG_PATTERN, string.Empty, RegexOptions.None, Constants.REGEX_TIMEOUT);
            
            ICollection<BoeApproverResponseDTO> approverResponses = boe.ApproverResponses;
            if (approverResponses.Any())
            {
                UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
                Collection<BoeApproverResponseDTO> currentUserApproverResponses = approverResponses.Where(x => x.ETIUserID == currentUser.UserID).ToCollection();

                if (currentUserApproverResponses.Any())
                {
                    theBOE.BOEApprovalLastUpdate = currentUserApproverResponses.First().UpdateDate;
                    theBOE.BOEApprovalLastUpdateInt = theBOE.BOEApprovalLastUpdate.Ticks;
                }
            }
                
            return Json(theBOE, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Returns MS Word export of boe
        /// </summary>
        /// <param name="boeId"></param>
        /// <returns>Base64 encoded string as plain text</returns>
        public ContentResult GetExportedBOEWordFile(string workspace, int boeID)
        {
            FullBoe boeToExport = Factory.CreateFullBoe(boeID);
            FullWorkspace ws = Factory.CreateFullWorkspace(workspace);
            
            bool isSubcontractorUser = (from p in this.PermissionsLoader.GetBOEPotentialPermissionsForWorkspace(boeToExport.Workspace.Id)
                                        where p.Role == Role.SubcontractorAuthor && p.ETIUserId == ws.CurrentActiveUser.UserID
                                        select p).Any();

            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                this.reportsControllerLogic.ExportSingleBOEReport(ws, isSubcontractorUser, boeID, ms);
                ms.Position = 0;
                bytes = ms.ToArray();
            }

            return Content(Convert.ToBase64String(bytes), "text/plain");
        }
    }
}
