using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IES.Core.Authorization
{
	public class GroupsCheckHandler : AuthorizationHandler<GroupsCheckRequirement>
	{
		private readonly IActiveDirectoryUtilities activeDirectoryUtilities;

		public GroupsCheckHandler(IActiveDirectoryUtilities activeDirectoryUtilities)
		{
			this.activeDirectoryUtilities = activeDirectoryUtilities;
		}
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
												  GroupsCheckRequirement requirement)
		{
			if (requirement == null)
			{
				throw new Exception("Groups Check Requirement not setup correctly");
			}
			bool result = false;
			if (requirement.groups == null || requirement.groups.Count == 0)
			{
				// pass-through allowing everyone (all groups)
				result = true;
			}
			else
			{
				ICollection<GroupData> groups = this.activeDirectoryUtilities.GetGroupsForUser(context.User.Identity.Name);
				foreach (GroupData group in groups)
				{
					if (requirement.groups.Contains(group.Ntid.ToLower()))
					{
						result = true;
						break;
					}
				}
			}
			
			if (result)
			{
				context.Succeed(requirement);
			}

		}


	}
}
