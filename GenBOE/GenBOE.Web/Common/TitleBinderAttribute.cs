// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    /// <summary>
    /// Attribute for the Boe title custom binder for all companies
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field |
        AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class TitleBinderAttribute : CustomModelBinderAttribute
    {
        public override IModelBinder GetBinder()
        {
            return new TitleModelBinder();
        }

        
    }

}