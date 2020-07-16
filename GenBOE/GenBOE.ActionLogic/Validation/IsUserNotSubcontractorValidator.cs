// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class IsUserNotSubcontractorValidator : Validator
    {
        private ISecurityInformation _SecurityInformation;
        private IUserDTODataLoader userDataLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityInformation">SecurityInformation</param>
        public IsUserNotSubcontractorValidator(ISecurityInformation inSecurityInformation, IUserDTODataLoader userDataLoader) 
        {
            this._SecurityInformation = inSecurityInformation;
            this.userDataLoader = userDataLoader;
        }


        /// <summary>
        /// Determines if a value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Collection of reasons why not valid</returns>
        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(string))
            {
                throw new InvalidCastException("value");
            }

            UserDTO user = this.userDataLoader.GetOrCreateUserByNtid((string)value);

            if (this._SecurityInformation.IsSubcontractorUser(user.NTID, user.IsSubcontractor))
            {
                response.Add("Subcontractor users are restricted from Cost Volume Lead/Pricer permissions.");
            }

            return response;
        }
    }
}
