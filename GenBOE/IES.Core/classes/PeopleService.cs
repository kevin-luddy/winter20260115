// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Runtime.Serialization;
	using System.Threading.Tasks;
	using GraphQL;
	using GraphQL.Client.Http;
	using GraphQL.Client.Serializer.SystemTextJson;
	using Microsoft.Extensions.Logging;

	public class PeopleService
	{
		#region Internal Classes

		/// <summary>
		/// User group result
		/// </summary>
		public class UserGroupResult
		{
			/// <summary>
			/// Display name
			/// </summary>
			public string DisplayName { get; set; }

			/// <summary>
			/// User NTID
			/// </summary>
			public string NTID { get; set; }
		}

		/// <summary>
		/// This class is used by People Service and should not be changed, or breakage of functionality could result
		/// </summary>
		[DataContract]
		public class Person
		{
#pragma warning disable IDE1006 // Naming Styles turned off since this must match json coming back from People Service

			/// <summary>
			/// Gets or sets the first name.
			/// </summary>
			public string firstName { get; set; }

			/// <summary>
			/// Gets or sets the last name.
			/// </summary>
			public string lastName { get; set; }

			/// <summary>
			/// Gets or sets the gal.
			/// </summary>
			public string gal { get; set; }


			/// <summary>
			/// Gets or sets the name of the user principal.
			/// </summary>
			public string userPrincipalName { get; set; }

			/// <summary>
			/// Gets or sets the email.
			/// </summary>
			public string email { get; set; }

			/// <summary>
			/// Gets or sets the domain.
			/// </summary>
			public string domain { get; set; }

			/// <summary>
			/// Gets or sets the nt identifier.
			/// </summary>
			public string ntId { get; set; }

			/// <summary>
			/// Gets or sets the title.
			/// </summary>
			public string title { get; set; }

			/// <summary>
			/// Gets or sets the Department Name/Title.
			/// </summary>
			public string departmentName { get; set; }

			/// <summary>
			/// Gets or sets the Display Name
			/// </summary>
			public string displayName { get; set; }

			/// <summary>
			/// Gets or sets whether this is an employee of LMC.
			/// </summary>
			public bool? isEmployee { get; set; }

			/// <summary>
			/// Gets or sets the Person Type (i.e. US, International)
			/// </summary>
			public string personType { get; set; }

			/// <summary>
			/// Gets or sets the Employee Type.  (E = Employee, G = Guest, R = Resource) -- contractors?
			/// </summary>
			public string employeeType { get; set; }

			/// <summary>
			/// Gets or sets the Employee Id
			/// </summary>
			public string employeeId { get; set; }

			/// <summary>
			/// Gets the source for the data (i.e LMP, NEAT, Personnel)
			/// </summary>
			public string source { get; set; }

			/// <summary>
			/// Gets or sets the Employer.
			/// </summary>
			public string[] adGroups { get; set; }

			/// <summary>
			/// Gets or sets the shop name.
			/// </summary>
			public string shopName { get; set; }

#pragma warning restore IDE1006 // Naming Styles

			/// <summary>
			/// Gets the full name for the person.
			/// </summary>
			/// <returns>The full name for the person.</returns>
			public string GetFullName()
			{
				return this.firstName + " " + this.lastName;
			}

			/// <summary>
			/// Returns whether this is a US Employee.
			/// </summary>
			/// <returns>Whether this is a US Employee.</returns>
			public bool IsUSEmployee()
			{
				return this.IsLMCEmployee() && this.personType == "US";
			}

			public bool IsLMCEmployee()
			{
				return this.isEmployee == true || (this.isEmployee == null && this.employeeType == "E");
			}
		}

		/// <summary>
		/// Person Result
		/// </summary>
		internal class PersonResult
		{
			/// <summary>
			/// Person
			/// </summary>
			public Person Person { get; set; }
		}

		/// <summary>
		/// Get User Groups Result
		/// </summary>
		internal class GetUserGroupsResult
		{
			public string[] GetUserGroups { get; set; }
		}

		/// <summary>
		/// People in group result
		/// </summary>
		internal class PeopleInGroupResult
		{
			/// <summary>
			/// Person in group
			/// </summary>
			public Person[] PeopleInGroup { get; set; }
		}

		/// <summary>
		/// Group Result
		/// </summary>
		internal class GroupResult
		{
			public bool IsInGroup { get; set; }
		}

		#endregion

		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger<PeopleService> logger;

		/// <summary>
		/// Http Client
		/// </summary>
		private readonly HttpClient httpClient;

		/// <summary>
		/// Graph QL HTTP Client Options
		/// </summary>
		private static GraphQLHttpClientOptions Options { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		public PeopleService(ILogger<PeopleService> logger, HttpClient httpClient)
		{
			this.logger = logger;
			this.httpClient = httpClient;
			Options = new()
			{
				EndPoint = new Uri(ConfigurationUtilities.GetAppSetting("PeopleSvcUrl")),
				HttpMessageHandler = new HttpClientHandler
				{
					AllowAutoRedirect = true,
					ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
				}
			};
		}

		/// <summary>
		/// Determines whether a user is inside a group
		/// </summary>
		/// <param name="ntid">The ntid of a user.</param>
		/// <param name="adGroup">An AD group to check.</param>
		/// <param name="token">The auth token.</param>
		/// <returns>Is user in group</returns>
		public async Task<bool> IsInGroup(string ntid, string adGroup, string token)
		{
			_ = ntid ?? throw new ArgumentNullException(nameof(ntid));
			_ = adGroup ?? throw new ArgumentNullException(nameof(adGroup));
			_ = token ?? throw new ArgumentNullException(nameof(token));

			bool inGroup = false;

			GraphQLRequest isInGroupRequest = new()
			{
				Query = @"
					query isInGroup($ntid:String!,$groupName:String!) {
					  isInGroup(ntid: $ntid, groupName: $groupName)
					}",
				OperationName = "isInGroup",
				Variables = new { ntid, groupName = adGroup }
			};

			using (GraphQLHttpClient graphQLClient = new(Options, new SystemTextJsonSerializer(), httpClient))
			{
				graphQLClient.HttpClient.AddAuthorizationHeader(token);

				try
				{
					inGroup = (await graphQLClient.SendQueryAsync<GroupResult>(isInGroupRequest)).Data.IsInGroup;
					this.logger.LogInformation($"IsInGroup was successful for user: {LoggerConstants.UserNTID}. Group: {LoggerConstants.AdGroup}.", ntid, adGroup);
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, $"IsInGroup failed for user: {LoggerConstants.UserNTID}. Group: {LoggerConstants.AdGroup}.", ntid, adGroup);
				}

				return inGroup;
			}
		}

		/// <summary>
		/// Get Person By Ntid
		/// </summary>
		/// <param name="ntid">The ntid of a user.</param>
		/// <param name="token">The auth token.</param>
		/// <returns>Person for the NTID</returns>
		public async Task<Person> GetPersonByNtid(string ntid, string token)
		{
			_ = ntid ?? throw new ArgumentNullException(nameof(ntid));
			_ = token ?? throw new ArgumentNullException(nameof(token));

			Person person = new();

			GraphQLRequest peopleSvcRequest = new()
			{
				Query = @"
					query person($ntid:String!) {
						person(ntid: $ntid, includeNEAT: true) {
							firstName
							lastName
							preferredFirstName
							gal
							userPrincipalName
							email
							domain
							ntId
							title
						}
					}",
				OperationName = "person",
				Variables = new { ntid }
			};

			using (GraphQLHttpClient graphQLClient = new(Options, new SystemTextJsonSerializer(), httpClient))
			{
				graphQLClient.HttpClient.AddAuthorizationHeader(token);

				try
				{
					person = (await graphQLClient.SendQueryAsync<PersonResult>(peopleSvcRequest)).Data.Person;
					logger.LogInformation($"GetPersonByNtid was successful for user: {LoggerConstants.UserNTID}.", ntid);
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, $"GetPersonByNtid failed for user {LoggerConstants.UserNTID}.", ntid);
				}
			}

			return person;
		}

		/// <summary>
		/// Get users from AD group
		/// </summary>
		/// <param name="adGroup">AD group</param>
		/// <param name="token">Auth token</param>
		/// <returns>List of persons</returns>
		public async Task<ICollection<UserGroupResult>> GetUsersFromADGroup(string adGroup, string token)
		{
			_ = adGroup ?? throw new ArgumentNullException(nameof(adGroup));
			_ = token ?? throw new ArgumentNullException(nameof(token));

			List<Person> result = new();
			GraphQLRequest request;

			using (GraphQLHttpClient graphQLClient = new(Options, new SystemTextJsonSerializer(), httpClient))
			{
				graphQLClient.HttpClient.AddAuthorizationHeader(token);

				request = new()
				{
					Query = @"
							query peopleInGroup($adGroup:String!) {
								 peopleInGroup(groupName: $adGroup)
								  {
									ntId,
									gal
								  }
							}",
					OperationName = "peopleInGroup",
					Variables = new { adGroup }
				};

				try
				{
					GraphQLResponse<PeopleInGroupResult> query = await graphQLClient.SendQueryAsync<PeopleInGroupResult>(request);
					result.AddRange(query.Data.PeopleInGroup);
					logger.LogInformation($"GetUsersFromADGroup was successful.");
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, $"GetUsersFromADGroup failed.");
				}
			}

			return result.Select(x => new UserGroupResult() { DisplayName = x.gal, NTID = x.ntId })
										.Where(s => !s.DisplayName.ToLower().Contains("resource"))
										.GroupBy(i => i.NTID).Select(i => i.FirstOrDefault())
										.OrderBy(s => s.DisplayName).ToList();
		}

		public async Task<ICollection<UserGroupResult>> GetADGroupsForUser(string ntid, string token)
		{
			_ = ntid ?? throw new ArgumentNullException(nameof(ntid));
			_ = token ?? throw new ArgumentNullException(nameof(token));

			List<string> result = new();
			GraphQLRequest request;

			using (GraphQLHttpClient graphQLClient = new(Options, new SystemTextJsonSerializer(), httpClient))
			{
				graphQLClient.HttpClient.AddAuthorizationHeader(token);

				request = new()
				{
					Query = @"
							query getUserGroups($ntId:String!) {
								 getUserGroups(ntid: $ntId)
							}",
					OperationName = "getUserGroups",
					Variables = new { ntid }
				};

				try
				{
					GraphQLResponse<GetUserGroupsResult> query = await graphQLClient.SendQueryAsync<GetUserGroupsResult>(request);
					result.AddRange(query.Data.GetUserGroups);
					logger.LogInformation($"GetUsersFromADGroup was successful.");
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, $"GetUsersFromADGroup failed.");
				}
			}

			return result.Select(x => new UserGroupResult() { NTID = x })
										//.Where(s => !s.DisplayName.ToLower().StartsWith("dl"))
										.GroupBy(i => i.NTID).Select(i => i.FirstOrDefault())
										.OrderBy(s => s.NTID).ToList();
		}
	}
}
