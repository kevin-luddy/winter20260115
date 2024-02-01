// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// user mediator
    /// </summary>
    public class UserMediator : IUserMediator
    {
        /// <summary>
        /// The user mapper
        /// </summary>
        private IInternalUserMapper userMapper = null;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="inUserMapper">User Mapper</param>
        public UserMediator(
            IUserMapper inUserMapper)
        {
            this.userMapper = inUserMapper as IInternalUserMapper;
        }

        /// <summary>
        /// Saves a user DTO
        /// </summary>
        /// <param name="inUser">User Dto</param>
        /// <returns>Saved user id</returns>
        public int? SaveUser(UserDTO inUser)
        {
            // Pre save business logic
            if (inUser == null)
            {
                throw new ArgumentNullException(nameof(inUser));
            }

			// Save
			int? toReturn = this.userMapper.Save(inUser);

            // Post save business logic

            // Return
            return toReturn;
        }
    }
}
