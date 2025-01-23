// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Web.Http;
	using System.Web.Http.Cors;

	/// <summary>
	/// User data controller for getting user data.
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class UserDataController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Service data controller logic.
		/// </summary>
		private UserDataControllerLogic userDataControllerLogic { get; set; }

		/// <summary>
		/// Loggers
		/// </summary>
		private Logger logger = new Logger("UserDataController");

		/// <summary>
		/// Ctor
		/// </summary>
		public UserDataController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, UserDataControllerLogic userDataControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.userDataControllerLogic = userDataControllerLogic;
		}
		#endregion

		/// <summary>
		/// Gets the user look up data by Ntid.
		/// </summary>
		/// <param name="ntid">Ntid</param>
		/// <returns>User data matching ntid.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<UserDataViewModel> GetUserLookupData(string ntid)
		{
			IESSingleResponse<UserDataViewModel> result = new IESSingleResponse<UserDataViewModel>();

			try
			{
				result.Data = userDataControllerLogic.GetUserLookupData(ntid);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning user lookup data: {ex.Message}");
			}

			return result;
		}
	}
}