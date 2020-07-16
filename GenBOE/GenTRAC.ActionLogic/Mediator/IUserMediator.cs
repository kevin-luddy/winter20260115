// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// user mediator interface
    /// </summary>
    public interface IUserMediator
    {
        /// <summary>
        /// Saves a user
        /// </summary>
        /// <param name="inUser">The user to save</param>
        /// <returns>The user's primary key</returns>
        int? SaveUser(UserDTO inUser);
    }
}
