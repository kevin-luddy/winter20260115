// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class encapsulating properties needed to work with paged views
    /// </summary>
    /// <typeparam name="T">The data to carry in the view, passed back and forth on request/responses so PICK SOMETHING SMALL like an int.
    ///                     In some cases this is not possible (if your paged data is not a DTO that has a primary key for example).
    ///                     In these cases you can use the type of the object being paged and the entire object collection will be stored
    ///                     in the 'PagedResults' property of this class.  Just BEWARE that the ENTIRE OBJECT COLLECTION is being 
    ///                     serialized and deserialized in the View and back to the Controller every time a new 'page' is selected
    ///                     by the user.  YOU HAVE BEEN WARNED!!  Have a nice day.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class PagedResultsModelView<T>
    {
        /// <summary>
        /// Gets or sets the current page being requested
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets a list of all indexes for every page
        /// </summary>
        public Collection<T> PagedIndexes { get; set; }

        /// <summary>
        /// Gets or sets the total number of results to display on each page
        /// </summary>
        public int ResultsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the text for filter/search
        /// </summary>
        public string SearchFilter { get; set; }

        /// <summary>
        /// Gets the array index for the first element on the page
        /// </summary>
        public int StartArrayIndex
        {
            get
            {
                return (this.CurrentPage * this.ResultsPerPage) - this.ResultsPerPage;
            }
        }

        /// <summary>
        /// Gets the array index for the last element on the page
        /// </summary>
        public int EndArrayIndex
        {
            get
            {
                int endArrayIndex = (this.CurrentPage * this.ResultsPerPage) - 1;

                // verify end is not above the total.
                return endArrayIndex >= this.PagedIndexes.Count ? endArrayIndex = this.PagedIndexes.Count - 1 : endArrayIndex;
            }
        }

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        public int NumPages
        {
            get
            {
                int numPages = this.PagedIndexes.Count / this.ResultsPerPage;

                // Add a page if the division is not clean.
                return (this.PagedIndexes.Count % this.ResultsPerPage == 0) ? numPages : numPages + 1;
            }
        }
    }
}
