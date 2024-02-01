// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Interfaces
{
	using System.Threading.Tasks;
	using IES.Common.Core.Models;

	/// <summary>
	/// Interface for Token Service
	/// </summary>
	public interface ITokenService
	{
		/// <summary>
		/// Get Token
		/// </summary>
		/// <returns>Token</returns>
		Task<Token> GetToken();
	}
}
