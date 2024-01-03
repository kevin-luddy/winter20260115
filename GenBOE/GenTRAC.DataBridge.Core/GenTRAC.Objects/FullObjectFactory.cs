// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Objects
{
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects.FullObject;
    using IES.Core.Exceptions;

    /// <summary>
    /// Factory for creating full objects
    /// </summary>
    public class FullObjectFactory : GenTRAC.Objects.IFullObjectFactory 
    {
        /// <summary>
        /// Creates a full proposal
        /// </summary>
        /// <param name="proposal">proposal Dto</param>
        /// <returns>Full proposal</returns>
        public FullProposal CreateFullProposal(ProposalDto proposal)
        {
            if (proposal == null)
            {
                throw new AuthorizationException("proposal not found");
            }

            return new FullProposal(proposal);
        }
    }
}
