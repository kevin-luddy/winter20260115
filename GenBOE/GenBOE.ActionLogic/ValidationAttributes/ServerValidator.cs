// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ValidationAttributes
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Validation;

    public class ServerValidator : DataAnnotationsModelValidator<ServerValidationAttribute>
    {
        ValidationType _validationToPerform;
        string _message;

        public ServerValidator(ModelMetadata metadata, ControllerContext context,
            ServerValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            this._validationToPerform = attribute.ValidationToPerform;
            this._message = attribute.ErrorMessage;
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this._message,
                ValidationType = "server"
            };
            rule.ValidationParameters.Add("type", this._validationToPerform);

            return new[] { rule };
        }
    }
}