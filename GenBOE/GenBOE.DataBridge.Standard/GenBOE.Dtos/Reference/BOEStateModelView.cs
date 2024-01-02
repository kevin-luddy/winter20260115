using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    /// <summary>
    /// A BOE State lookup entry
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BOEStateModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOEStateModelView()
        {
            BOEStateID = 0;
            BOEState = String.Empty;
        }

        /// <summary>
        /// The id of the BOEState
        /// </summary>
        public int BOEStateID { get; set; }

        /// <summary>
        /// The state name itself
        /// </summary>
        public string BOEState { get; set; }
    }
}
