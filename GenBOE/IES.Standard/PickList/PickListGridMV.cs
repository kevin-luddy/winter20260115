// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard.PickList
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    /// <summary>
    /// Model View for the Pick List Grid
    /// </summary>
    public class PickListGridMV
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickListGridMV"/> class.
        /// </summary>
        public PickListGridMV()
        {
            this.PickLists = new List<PickListDto>();
            this.Messages = new List<ValidationMessage>();
        }

        /// <summary>
        /// Gets or sets the pick lists.
        /// </summary>
        public ICollection<PickListDto> PickLists { get; set; }

        /// <summary>
        /// Gets or sets the pick list identifier.
        /// </summary>
        public int PickListId { get; set; }

        /// <summary>
        /// Gets or sets the name of the pick list.
        /// </summary>
        public string PickListName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this Pick List contains read only values.
        /// </summary>
        public bool ContainsReadOnly
        {
            get
            {
                return this.PickLists.Any(p => p.IsReadOnly);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [contains parent].
        /// </summary>
        public bool ContainsParent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Pick List allows multiple parents.
        /// </summary>
        public bool AllowsMultipleParents { get; set; }

        /// <summary>
        /// Gets or sets the parents available for this Pick List.
        /// </summary>
        public ICollection<SelectListItem> Parents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [contains children].
        /// </summary>
        public bool ContainsChildren { get; set; }

        /// <summary>
        /// Gets or sets the children available for this Pick List.
        /// </summary>
        public ICollection<PickListDto> Children { get; set; }

        /// <summary>
        /// Gets or sets the message to be shown to the user.
        /// </summary>
        public ICollection<ValidationMessage> Messages { get; set; }
    }
}
