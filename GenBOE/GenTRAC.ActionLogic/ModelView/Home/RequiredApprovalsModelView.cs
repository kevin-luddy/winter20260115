// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Home
{
    using System.Reflection;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// RequiredApprovalsModelView
    /// </summary>
    public class RequiredApprovalsModelView : ApprovalsDto
    {
        /// <summary>
        /// Public Constructor
        /// </summary>
        public RequiredApprovalsModelView()
        {
        }

        /// <summary>
        /// Constructor from the dto
        /// </summary>
        /// <param name="input">Approvals Dto</param>
        public RequiredApprovalsModelView(ApprovalsDto input) : this()
        {
            if (input != null)
            {
                foreach (PropertyInfo prop in typeof(ApprovalsDto).GetProperties())
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(input, null), null);
                    }
                }
            }
        }
    }
}