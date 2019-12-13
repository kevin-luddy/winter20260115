// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System.Collections.Generic;
    using GenBOE.Objects;

    public interface IConflictBOE
    {
        ICollection<BoesWithConflicts> CopyBOEConflicts(FullBoe sourceBOE, FullBoe destinationBOE, ICollection<int> selectedTaskElementsToCopy);
    }
}
