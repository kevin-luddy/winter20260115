// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard.OfficeUtilities
{
    /// <summary>
    /// Used to store information for an Excel Defined Name
    /// </summary>
    public class DefinedNameData
    {
        /// <summary>
        /// Unique name given to the cell
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Sheet name this defined name is found on
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// Starting column for the cell (ie. B)
        /// </summary>
        public string StartColumn { get; set; }

        /// <summary>
        /// End column for the cell if in a range
        /// </summary>
        public string EndColumn { get; set; }

        /// <summary>
        /// Start row for the cell (ie. 2)
        /// </summary>
        public string StartRow { get; set; }

        /// <summary>
        /// End row for the cell if in a range
        /// </summary>
        public string EndRow { get; set; }

        /// <summary>
        /// Start Column + Start Row (ie. B2)
        /// </summary>
        public string CellReference { get; set; }
    }
}
