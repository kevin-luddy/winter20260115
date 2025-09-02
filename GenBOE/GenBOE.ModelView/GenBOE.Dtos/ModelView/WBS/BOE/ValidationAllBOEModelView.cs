// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// This model view is used to return any validation messages that need to be shown
	/// to a user after the "Validate All BOE's" button has been selected
	/// </summary>
	[ExcludeFromCodeCoverage]
    public class ValidationAllBOEModelView 
    {
        public ValidationAllBOEModelView()
        {
            AllBOEs = new Collection<ValidationBOEModelView>();
        }
        //Collection of each BOE validation method.
        public ICollection<ValidationBOEModelView> AllBOEs { get; set; }
    }
}
