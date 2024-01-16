using Microsoft.AspNetCore.Authorization;

namespace IES.Common.Core.Authorization
{
	public class GroupsCheckRequirement : IAuthorizationRequirement
	{
		public ICollection<string> groups;

		public GroupsCheckRequirement(string groups)
		{
			this.groups = new List<string>();
			IEnumerable<string> splitGroups = groups.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(g => g.ToLower());
			foreach (string splitGroup in splitGroups)
			{
				string actualGroup = splitGroup;
				if (splitGroup.Contains('\\'))
				{
					string[] split = splitGroup.Split('\\');
					actualGroup = split.Last();
				}

				this.groups.Add(actualGroup);
			}
		}
	}
}
