using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    /// <summary>
    /// This class will return the different proposal state types
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ElementOfCostTypeModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ElementOfCostTypeModelView()
        {
            ElementOfCostId = 0;
            ElementOfCostName = String.Empty;
        }

        /// <summary>
        /// The ID
        /// </summary>
        public int ElementOfCostId { get; set; }

        /// <summary>
        /// the name of
        /// </summary>
        public string ElementOfCostName { get; set; }
    }
}
