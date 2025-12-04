using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// The view model for BOE copy conflicts.
    /// </summary>
    public class BOECopyConflictsModelView
    {
        public BOECopyConflictsModelView()
        {
            this.BOECopyConflictModelViews = new Collection<BOECopyConflictModelView>();
            this.TaskElementsToCopy = new Collection<int>();
        }

        /// <summary>
        /// A collection of various copy conflicts found for the BOE.
        /// </summary>
        public ICollection<BOECopyConflictModelView> BOECopyConflictModelViews { get; set; }

        /// <summary>
        /// Individual task elements to copy from this BOE.
        /// </summary>
        public ICollection<int> TaskElementsToCopy { get; set; }

        /// <summary>
        /// The source copy BOE Id.
        /// </summary>
        public int CopyBoeId { get; set; }

        /// <summary>
        /// The destination copy BOE Id.
        /// </summary>
        public int BoeId { get; set; } 
    }
}
