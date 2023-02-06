namespace APTSPropricerApi
{
	using System.Collections.Generic;
	using EBS.ProPricer.Data;
	using EBS.ProPricer.Model;
	using Newtonsoft.Json;

	/// <summary>
	/// Class used to send back data about Proposals / Folders
	/// </summary>
	[JsonObject(IsReference = true)]
	public class ProposalFolderInfo
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		[JsonProperty("V")]
		public string Version { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[JsonIgnore]
		public EntityId EntityId { get; set; }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public string Id { get { return EntityId.ToString(); } }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is proposal.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is proposal; otherwise, <c>false</c>.
		/// </value>
		[JsonProperty("IsP")]
		public bool IsProposal { get; set; }

		/// <summary>
		/// Gets or sets the child elements.
		/// </summary>
		/// <value>
		/// The child elements.
		/// </value>
		[JsonProperty("Ce")]
		public ICollection<ProposalFolderInfo> ChildElements { get; set; } = new List<ProposalFolderInfo>();

		/// <summary>
		/// Gets or sets the parent entity identifier.
		/// </summary>
		/// <value>
		/// The parent entity identifier.
		/// </value>
		[JsonIgnore]
		public EntityId? ParentEntityId { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ProposalFolderInfo"/> class.
		/// </summary>
		public ProposalFolderInfo() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProposalFolderInfo"/> class.
		/// </summary>
		/// <param name="proposal">The proposal.</param>
		public ProposalFolderInfo(Proposal proposal)
		{
			this.Name = proposal.Name;
			this.Version = proposal.Version;
			this.EntityId = proposal.Id;
			this.ParentEntityId = proposal.ParentFolder?.Id;
			this.IsProposal = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProposalFolderInfo"/> class.
		/// </summary>
		/// <param name="folder">The folder.</param>
		public ProposalFolderInfo(Folder folder)
		{
			this.Init(folder);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProposalFolderInfo"/> class.
		/// </summary>
		/// <param name="folder">The folder.</param>
		/// <param name="child">The child.</param>
		public ProposalFolderInfo(Folder folder, ProposalFolderInfo child)
		{
			this.Init(folder);
			this.ChildElements = new List<ProposalFolderInfo>() { child };
		}

		#endregion

		#region Private Helpers

		/// <summary>
		/// Initializes based on the specified folder.
		/// </summary>
		/// <param name="folder">The folder.</param>
		private void Init(Folder folder)
		{
			this.Name = folder.Name;
			this.EntityId = folder.Id;
			this.IsProposal = false;
			this.ParentEntityId = folder.ParentFolder?.Id;
		}

		#endregion
	}

}