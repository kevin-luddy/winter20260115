// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    /// <summary>
    /// Allows controller-logic classes to return company-specific "ViewResult" objects to the controllers without
    /// requiring them to inherit from the Controller class.
    /// </summary>
    public class ViewResultData
    {
        public string ViewName { get; set; }
        public object Model { get; set; }
    }
}
