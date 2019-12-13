using GenBOE.Common;
using GenBOE.DataBridge.DTO;
using GenBOE.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GenBOE.Common.Permissions
{
    public class GroupPermissionUtilities
    {
        private PermissionsDTOMapper _permissionsMapper = null;
        private UserDTODataMapper _userMapper = null;
        private ActiveDirectoryUtilities _ADUtils = null;
        public GroupPermissionUtilities(PermissionsDTOMapper inPermissionsMapper,
            UserDTODataMapper inUserMapper,
            ActiveDirectoryUtilities inADUtils)
        {
            this._permissionsMapper = inPermissionsMapper;
            this._userMapper = inUserMapper;
            this._ADUtils = inADUtils;
        }

        public Collection<UserDTO> GetBOEPotentialPermissionsForWorkspace(int wsId, Role? role)
        {
            Collection<UserDTO> retVal = new Collection<UserDTO>();

            Collection<int> userIds = new Collection<int>();
            if (role != null)
            {
                userIds = (from p in _permissionsMapper.GetBOEPotentialPermissionsForWorkspace(wsId)
                           where p.Role == role
                           select p.ETIUserId).Distinct().ToCollection();
            }
            else
            {
                userIds = (from p in _permissionsMapper.GetBOEPotentialPermissionsForWorkspace(wsId)
                           select p.ETIUserId).Distinct().ToCollection();
            }

            foreach (int currentUserId in userIds)
            {
                UserDTO currentUser = _userMapper.GetUserByID(currentUserId);

                if (currentUser.DisplayName.Contains('.')) // AD group name
                {
                    var groupDomain = _ADUtils.GetDomainName(UserType.Group, currentUser.DisplayName);
                    var members = _ADUtils.GetAdGroupUsers(currentUser.DisplayName, groupDomain);
                    IEnumerable<UserData> orderedMembers = members.OrderBy(m => m.DisplayName);
                    foreach (UserData userDataElement in orderedMembers)
                    {
                        retVal.Add(_userMapper.GetUserByUserData(userDataElement));
                    }
                }
                else
                {
                    retVal.Add(currentUser);
                }
            }
            retVal = retVal.GroupBy(u => u.UserID).Select(grp => grp.First()).ToCollection();
            return retVal;
        }
    }
}
