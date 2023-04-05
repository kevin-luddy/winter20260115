/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using EBS.Core;
	using EBS.ProPricer.Data;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Model.General;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Controller for Summary Field Definitions
	/// </summary>
	public class SummaryFieldDefinitionsController : ProPricerController
	{
		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public SummaryFieldDefinitionsController(ILogger<SummaryFieldDefinitionsController> logger, PoolManagerList poolManagerList) : base(logger)
		{
			this.poolManagerList = poolManagerList;
		}

		////// GET api/SummaryFieldDefinitions
		/////// <summary>
		/////// Returns the list of SummaryFieldDefinitions in the instance of PROPRICER.
		/////// </summary>
		/////// <param name="instanceId">The instance identifier.</param>
		/////// <returns>
		/////// Returns a collection of summary field definitions from the instance of PROPRICER.
		/////// </returns>
		////public IEnumerable<SummaryFieldDefinitionsDto> Get(int instanceId)
		////{
		////    List<SummaryFieldDefinitionsDto> sfdl = new List<SummaryFieldDefinitionsDto>();
		////    using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
		////    {
		////        if (ppc.Workspace != null)
		////        {                    
		////            ppc.Workspace.summary.ProposalUserFieldDefinitions.ProposalDefaults.SummaryFieldDefinitions.Open();
		////            foreach (SummaryFieldDefinition sfd in ppc.Workspace.ProposalDefaults.SummaryFieldDefinitions.Items())
		////            {
		////                SummaryFieldDefinitionsDto sfddto = new SummaryFieldDefinitionsDto
		////                {
		////                    Id = sfd.Id.ToString(),
		////                    Name = sfd.Name,
		////                    DataType = sfd.DataType.ToString(),
		////                    MaxLength = sfd.MaxLength,
		////                    SortType = sfd.SortType.ToString(),
		////                    Required = sfd.Required
		////                };
		////                if (sfddto.DataType == "List")
		////                {
		////                    List<SummaryFieldListDto> sflist = new List<SummaryFieldListDto>();
		////                    foreach (SummaryFieldStandardValue sf in sfd.ValueList.Items())
		////                    {
		////                        SummaryFieldListDto sfdto = new SummaryFieldListDto
		////                        {
		////                            Value = sf.Value,
		////                            Description = sf.Description
		////                        };
		////                        sflist.Add(sfdto);
		////                    }

		////                    sfddto.ValueList = sflist;
		////                }

		////                sfdl.Add(sfddto);
		////            }

		////            ppc.Workspace.ProposalDefaults.SummaryFieldDefinitions.Close();
		////        }
		////    }
		////    return sfdl;
		////}

		// GET api/SummaryFieldDefinitions/instanceId/id
		/// <summary>
		/// Returns the list of SummaryFieldDefinitions in the given proposal.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal to obtain the summary field definitions. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		/// <returns>
		/// Returns a collection of summary field definitions from the proposal.
		/// </returns>
		[HttpGet]
		[Authorize]
		[Route("{instanceId}/{id}")]
		public IEnumerable<SummaryFieldDefinitionsDto> Get(int instanceId, string id)
		{
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal pr;

				// GUID or Name|Version?
				if (id.Contains('|'))
				{
					// Name
					string[] parts = id.Split('|');
					pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
				}
				else
				{
					// GUID
					EntityId pEntityId = new(new Guid(id));
					pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
				}

				List<SummaryFieldDefinitionsDto> sfdl = new();
				if (pr != null)
				{
					pr.Open();
					pr.SummaryFieldDefinitions.Open();
					foreach (SummaryFieldDefinition sfd in pr.SummaryFieldDefinitions.Items())
					{
						SummaryFieldDefinitionsDto sfddto = new()
						{
							Id = sfd.Id.ToString(),
							Name = sfd.Name,
							DataType = sfd.DataType.ToString(),
							MaxLength = sfd.MaxLength,
							SortType = sfd.SortType.ToString()
						};
						try
						{
							sfddto.TitleTable = sfd.TitleTable != null ? sfd.TitleTable.Name : string.Empty;
							sfddto.Validate = sfd.Validate;
						}
						catch (Exception ex)
						{
							this.Logger.LogError(ex, "Error retrieving Field Definitions");
						}

						sfddto.Required = sfd.Required;

						if (sfddto.DataType == "List")
						{
							List<SummaryFieldListDto> sflist = new();
							foreach (SummaryFieldStandardValue sf in sfd.ValueList.Items())
							{
								SummaryFieldListDto sfdto = new()
								{
									Value = sf.Value,
									Description = sf.Description
								};
								sflist.Add(sfdto);
							}

							sfddto.ValueList = sflist;
						}

						sfdl.Add(sfddto);
					}

					pr.SummaryFieldDefinitions.Close();
					pr.Close();
				}

				return sfdl;
			}
		}

		// POST api/SummaryFieldDefinitions/proposalAndSummaryFields
		/// <summary>
		/// Adds the list of SummaryFieldDefinitions for a proposal using ProposalDto.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="proposalAndSummaryFields">A collection of summary field definitions contained in a proposal (ProposalDto).</param>
		[HttpPost]
		[Authorize]
		[Route("{instanceId}")]
		public void Post(int instanceId, [FromBody] ProposalDto proposalAndSummaryFields)
		{
			if (proposalAndSummaryFields == null)
			{
				return;
			}
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				////   Delete(proposalAndSummaryFields);
				Proposal ppProposal = null;
				try
				{
					EntityId pEntityId = new(new Guid(proposalAndSummaryFields.Id));
					ppProposal = ppc.Workspace.Proposals.Find(pEntityId).Value();

					//lock the proposal for modification.
					ppProposal.Open();
					ppProposal.BeginEdit();

					this.AddSumFieldDefs(ppc, ppProposal, proposalAndSummaryFields);

					//End edits and release the record
					ppProposal.EndEdit();

					//Close the proposal
					ppProposal.Close();
				}
				//exception block to catch any broken rules relating to the API business logic
				catch (BOBrokenRulesException ex)
				{
					this.Logger.LogError(ex, "Error Posting broken rules");
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (ppProposal != null && ppProposal.IsEditing())
					{
						ppProposal.EndEdit();
					}

					if (ppProposal != null && ppProposal.IsOpened())
					{
						ppProposal.Close();
					}
				}
				//exception block to catch any other issues with the data
				catch (Exception ex)
				{
					this.Logger.LogError(ex, "Error posting Summary Fields");
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (ppProposal != null && ppProposal.IsEditing())
					{
						ppProposal.EndEdit();
					}

					if (ppProposal != null && ppProposal.IsOpened())
					{
						ppProposal.Close();
					}
				}
			}
		}

		// Put api/SummaryFieldDefinitions/proposalAndSummaryFields
		/// <summary>
		/// Updates the list of SummaryFieldDefinitions for a proposal using ProposalDto.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="proposalAndSummaryFields">A collection of summary field definitions contained in a proposal (ProposalDto).</param>
		[HttpPut]
		[Authorize]
		[Route("{instanceId}")]
		public void Put(int instanceId, [FromBody] ProposalDto proposalAndSummaryFields)
		{
			if (proposalAndSummaryFields == null)
			{
				return;
			}

			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal ppProposal = null;
				try
				{
					EntityId pEntityId = new(new Guid(proposalAndSummaryFields.Id));
					ppProposal = ppc.Workspace.Proposals.Find(pEntityId).Value();

					//lock the proposal for modification.
					ppProposal.Open();
					ppProposal.BeginEdit();

					foreach (SummaryFieldDefinitionsDto sfddto in proposalAndSummaryFields.SumFieldDefs)
					{
						this.UpdateSumFieldDef(ppProposal, sfddto);
					}

					//End edits and release the record
					ppProposal.EndEdit();

					//Close the proposal
					ppProposal.Close();
				}
				//exception block to catch any broken rules relating to the API business logic
				catch (BOBrokenRulesException ex)
				{
					this.Logger.LogError(ex, "Error Putting Field Definitions with broken rules");
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (ppProposal.IsEditing())
					{
						ppProposal.EndEdit();
					}

					if (ppProposal.IsOpened())
					{
						ppProposal.Close();
					}
				}
				//exception block to catch any other issues with the data
				catch (Exception ex)
				{
					this.Logger.LogError(ex, "Error Putting Field Definitions");
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (ppProposal.IsEditing())
					{
						ppProposal.EndEdit();
					}

					if (ppProposal.IsOpened())
					{
						ppProposal.Close();
					}
				}
			}
		}

		// Delete api/SummaryFieldDefinitions/proposalAndSummaryFields
		/// <summary>
		/// Deletes the given list of SummaryFieldDefinitions for a proposal using ProposalDto.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="proposalAndSummaryFields">A collection of summary field definitions contained in a proposal (ProposalDto).</param>
		[HttpDelete]
		[Authorize]
		[Route("{instanceId}")]
		public void Delete(int instanceId, [FromBody] ProposalDto proposalAndSummaryFields)
		{
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal ppProposal = null;
				try
				{
					EntityId pEntityId = new(new Guid(proposalAndSummaryFields.Id));
					ppProposal = ppc.Workspace.Proposals.Find(pEntityId).Value();

					//lock the proposal for modification.
					ppProposal.Open();
					ppProposal.BeginEdit();

					foreach (SummaryFieldDefinitionsDto sfddto in proposalAndSummaryFields.SumFieldDefs)
					{
						foreach (SummaryFieldDefinition sfd in ppProposal.SummaryFieldDefinitions.Items())
						{
							if (sfd.Id.ToString() == sfddto.Id)
							{
								////  EBS.Core.Optional <SummaryFieldDefinition> sfd2 = ppProposal.SummaryFieldDefinitions.Find(sfddto.id);
								////   sfd.BeginEdit();
								//// ppProposal.SummaryFieldDefinitions.Delete(ppProposal.SummaryFieldDefinitions.IndexOf(sfddto.Id));
								sfd.Delete();
								////    sfd.EndEdit();
								break; //or loop will crash
							}
						}
					}

					//End edits and release the record
					ppProposal.EndEdit();

					//Close the proposal
					ppProposal.Close();
				}
				//exception block to catch any issues with deleting
				catch (Exception ex)
				{
					this.Logger.LogError(ex, "Error deleting Field Definitions");
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (ppProposal.IsEditing())
					{
						ppProposal.EndEdit();
					}

					if (ppProposal.IsOpened())
					{
						ppProposal.Close();
					}
				}
			}
		}

		// Delete api/SummaryFieldDefinitions/id
		/// <summary>
		/// Deletes all of the SummaryFieldDefinitions for a given proposal.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="id">The EntityId of the proposal to obtain the summary field definitions. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
		[HttpDelete]
		[Authorize]
		[Route("{instanceId}/{id}")]
		public void Delete(int instanceId, string id)
		{
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				Proposal pr;

				// GUID or Name|Version?
				if (id.Contains('|'))
				{
					// Name
					string[] parts = id.Split('|');
					pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
				}
				else
				{
					// GUID
					EntityId pEntityId = new(new Guid(id));
					pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
				}

				if (pr != null)
				{
					try
					{
						pr.Open();
						pr.BeginEdit();
						pr.SummaryFieldDefinitions.DeleteAll();
						pr.EndEdit();
						pr.Close();
					}
					//exception block to catch any issues with deleting
					catch (Exception ex)
					{
						this.Logger.LogError(ex, "Error deleting Field Definitions");
						System.Diagnostics.Debug.WriteLine(ex.Message);
						if (pr.IsEditing())
						{
							pr.EndEdit();
						}

						if (pr.IsOpened())
						{
							pr.Close();
						}
					}
				}
			}
		}

		private void AddSumFieldDefs(IProPricerConnection ppc, Proposal proposal, ProposalDto proposalAndSummaryFields)
		{
			foreach (SummaryFieldDefinitionsDto sfddto in proposalAndSummaryFields.SumFieldDefs)
			{
				this.AddSumFieldDef(ppc, proposal, sfddto);
			}
		}

		private void AddSumFieldDef(IProPricerConnection ppc, Proposal proposal, SummaryFieldDefinitionsDto sfddto)
		{
			SummaryFieldDefinition sfd = proposal.SummaryFieldDefinitions.AddNew();

			sfd.BeginEdit();

			try
			{
				sfd.Name = sfddto.Name;

				sfd.MaxLength = sfddto.MaxLength;

				Enum.TryParse(sfddto.SortType, out SummaryFieldSortType sst);
				sfd.SortType = sst;

				if (sfddto.TitleTable != null)
				{
					Optional<TitleTable> tt = ppc.Workspace.GlobalLibrary.TitleTables.Find(sfddto.TitleTable);
					////   foreach (var trvdes in ppc.workspace.GlobalLibrary.TitleTables)
					////  {
					////       string what = trvdes.Name;
					////   }
					////   sfd.TitleTable.Open();
					////   sfd.TitleTable.BeginEdit();
					////   sfd.TitleTable.Name = sfddto.TitleTable;
					////   sfd.TitleTable.EndEdit(
					sfd.TitleTable = tt.Value; ////.Name = sfddto.TitleTable;
				}

				sfd.Validate = sfddto.Validate;
				sfd.Required = sfddto.Required;
				////   sfd.ValueList = sfddto.ValueList;

				Enum.TryParse(sfddto.DataType, out SummaryFieldDataType sdt);
				sfd.DataType = sdt;
			}

			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Error adding summary field");
				System.Diagnostics.Debug.WriteLine("Error adding summary field: " + ex.Message);
			}

			sfd.EndEdit();
		}

		private void UpdateSumFieldDef(Proposal proposal, SummaryFieldDefinitionsDto sfddto)
		{
			try
			{
				foreach (SummaryFieldDefinition sfd in proposal.SummaryFieldDefinitions.Items())
				{
					if (sfd.Id.ToString() == sfddto.Id)
					{
						sfd.BeginEdit();
						sfd.Name = sfddto.Name;
						sfd.EndEdit();
						break; //or loop will crash
					}
				}
			}

			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Error updating summary field.");
				System.Diagnostics.Debug.WriteLine("Error updating summary field: " + ex.Message);
			}
		}
	}
}