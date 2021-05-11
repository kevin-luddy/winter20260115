// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;

    /// <summary>
    /// Contracts controller logic
    /// </summary>
    public class ContractsControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityAccess'">Security Access</param>
        /// <param name="proposalLoader">Proposal Loader</param>
        /// <param name="userMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="checklistMediator">Checklist Mediator</param>
        /// <param name="proposalMediator">Proposal Mediator</param>
        public ContractsControllerLogic(ISecurityAccess securityAccess, IProposalLoader proposalLoader, IUserMapper userMapper, IFullObjectFactory objectFactory, IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader, IChecklistMediator checklistMediator, IProposalMediator proposalMediator)
            : base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
        {
        }
    }
}
