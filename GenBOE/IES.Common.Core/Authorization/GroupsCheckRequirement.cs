namespace IES.Common.Core.Authorization
{
	using Microsoft.AspNetCore.Authorization;

	public class GroupsCheckRequirement : IAuthorizationRequirement
	{
		public ICollection<string> Groups { get; private set; }

		public GroupsCheckRequirement(string groups)
		{
			this.Groups = new List<string>();
			IEnumerable<string> splitGroups = groups.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(g => g.ToLower());
			foreach (string splitGroup in splitGroups)
			{
				string actualGroup = splitGroup;
				if (splitGroup.Contains('\\'))
				{
					string[] split = splitGroup.Split('\\');
					actualGroup = split.Last();
				}

				this.Groups.Add(actualGroup);
			}
		}
	}
}
