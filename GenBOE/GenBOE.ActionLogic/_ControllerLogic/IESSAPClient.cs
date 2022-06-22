// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.IESSAPClient
{
    using System.Net.Http;

    /// <summary>
    /// IES SAP Client
    /// </summary>
    public partial class IESSAPClient
	{
        /// <summary>
        /// Http Client
        /// </summary>
        public HttpClient HttpClient { get { return this._httpClient; } }
    }
}
