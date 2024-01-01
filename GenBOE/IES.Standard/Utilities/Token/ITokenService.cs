// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard
{
	using System.Threading.Tasks;

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
