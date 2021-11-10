using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using GenBOE.DataBridge.Common;
using GenBOE.DataBridge.Common.Interfaces;
using GenBOE.DataBridge.DTO;
using GenBOE.Dtos;
using GenBOE.Objects;
using IES.Common;
using IES.Common.Exceptions;

namespace GenBOE.Web.Controllers
{
	/// <summary>
	/// Base BOE Api data controller
	/// </summary>
	public class BoeDataBaseAPIController : ApiController
	{
        protected ISecurityAccess SecurityAccess { get; set; }
        protected IFullObjectFactory Factory { get; set; }
        protected IUserDTODataLoader UserLoader { get; set; }
        protected IPermissionsDTODataLoader PermissionsLoader { get; set; }

		public BoeDataBaseAPIController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader)
		{
			this.Factory = factory;
			this.UserLoader = userLoader;
			this.PermissionsLoader = permissionsLoader;
			this.SecurityAccess = securityAccess;
		}


        protected SecurityAuthorization CheckPermission(SecurityPage page, WorkspaceDTO workspace)
        {
            Dictionary<SecurityPage, SecurityAuthorization> securityDictionary = new Dictionary<SecurityPage, SecurityAuthorization>();
            int? wsId = workspace == null ? null : (int?)workspace.Id;

            UserDTO user = this.UserLoader.GetUserForActiveUser();
            string overrideNonUsString = ConfigurationUtilities.GetAppSetting("OverrideSubNonUs");
            bool overrideNonUs = string.IsNullOrEmpty(overrideNonUsString) ? false : overrideNonUsString.ToLower() == "true";
            bool? isUsPerson = overrideNonUs ? true : user.IsUsPerson;

            if (isUsPerson == null)
            {
                throw new ValidationException("IsUsPerson cannot be null");
            }

            IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = this.Factory.GetPermissionsForUser(user.NTID);

            SecurityAuthorization authorizationForUser;

            if ((bool)isUsPerson)
            {
                authorizationForUser = SecurityAccess.IsAuthorized(
                    new SecurityPermissionsRequested { PageToCheck = page, WorkspaceId = wsId }, workspace, rolesForUser);
            }
            else
            {
                throw new UnauthorizedAccessException("Access is denied for non-US users.");
            }

            securityDictionary.Add(page, authorizationForUser);

            return securityDictionary.Values.First();
        }

    }
}