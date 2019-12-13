using System;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Class encapsulating properties needed to work with paged views
    /// </summary>
    /// <typeparam name="T">The data to carry in the view, passed back and forth on request/responses so PICK SOMETHING SMALL like an int.
    ///                     In some cases this is not possible (if your paged data is not a DTO that has a primary key for example).
    ///                     In these cases you can use the type of the object being paged and the entire object collection will be stored
    ///                     in the 'PagedResults' property of this class.  Just BEWARE that the ENTIRE OBJECT COLLECTION is being 
    ///                     serialized and deserialized in the View and back to the Controller everytime a new 'page' is selected
    ///                     by the user.  YOU HAVE BEEN WARNED!!  Have a nice day.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class PagedResultsModelView<T>
    {
        // Whenever this reference gets deleted, please remove the reference to System.Web.Extensions from this project.
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected JavaScriptSerializer _serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

        public int CurrentPage { get; set; }
        public Collection<T> PagedIndexes { get; set; }
        public int ResultsPerPage { get; set; }
        public String SearchFilter { get; set; }

        public int StartArrayIndex
        {
            get
            {
                // the code below makes sure that the current page doesn't go past the bounds it shouldn't
                if (this.CurrentPage > this.NumPages) { this.CurrentPage = this.NumPages; }
                if (this.CurrentPage < 1) { this.CurrentPage = 1; }

                return this.CurrentPage * this.ResultsPerPage - this.ResultsPerPage;
            }
        }
        public int EndArrayIndex
        {
            get
            {
                int endArrayIndex = this.CurrentPage * this.ResultsPerPage - 1;
                // verify end is not above the total.
                return endArrayIndex >= this.PagedIndexes.Count ? this.PagedIndexes.Count - 1 : endArrayIndex;
            }
        }
        public int NumPages
        {
            get
            {
                int numPages = this.PagedIndexes.Count / this.ResultsPerPage;
                // Add a page if the division is not clean.
                return (this.PagedIndexes.Count % this.ResultsPerPage == 0) ? numPages : numPages + 1;
            }
        }

        // Whenever this JS array gets deleted, please remove the reference to System.Web.Extensions from this project.
        [Obsolete("No longer used in phase 2.0 paging pattern")]
        public string PagedResultsJSArray
        {
            get
            {
                return this._serializer.Serialize(this.PagedIndexes);
            }
        }
    }
}