namespace IES.Common.Core.Authorization
{
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using Microsoft.AspNetCore.Authorization;

	public class GroupsCheckHandler : AuthorizationHandler<GroupsCheckRequirement>
	{
		private readonly IActiveDirectoryService activeDirectoryUtilities;

		public GroupsCheckHandler(IActiveDirectoryService activeDirectoryUtilities)
		{
			this.activeDirectoryUtilities = activeDirectoryUtilities;
		}
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
												  GroupsCheckRequirement requirement)
		{
			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement), "Groups Check Requirement not setup correctly");
			}
			bool result = false;
			if (requirement.Groups == null || requirement.Groups.Count == 0)
			{
				// pass-through allowing everyone (all groups)
				result = true;
			}
			else
			{
				if (context.User?.Identity?.Name != null)
				{
					string ntId = CommonUtilities.StripDomain(context.User.Identity.Name);
					foreach (string group in requirement.Groups)
					{
						ICollection<UserData> users = this.activeDirectoryUtilities.GetAdGroupUsers(group);
						if (users.Any(u => u.Ntid.ToLower() == ntId))
						{
							result = true;
							break;
						}
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
