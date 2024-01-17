namespace IES.Common.Core.Authorization
{
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
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
				ICollection<GroupData> groups = activeDirectoryUtilities.GetGroupsForUser(context.User.Identity.Name);
				foreach (GroupData group in groups)
				{
					if (requirement.Groups.Contains(group.Ntid.ToLower()))
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
