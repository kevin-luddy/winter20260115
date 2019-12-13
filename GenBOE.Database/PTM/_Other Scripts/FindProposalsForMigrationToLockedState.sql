/*
 * This script will find PTM proposals that need to be migrated from the LOBApproved or PostApprovalFollowup states to the locked state.
 * The migration will need to be done prior to the release (via the app) so that validation can be performed prior to locking the proposals.
 */
select pslu.ProposalStatus, 
	case p.WorkflowStatus
		when 110 then 'LOBApproved'
		when 120 then 'PostApprovalFollowup'
	end as WorkflowStatus,
	rlu.Role,
	u.DisplayName,
	p.* 
from dbo.Proposal p
join dbo.ProposalStatusLU pslu on p.ProposalStatusID = pslu.ProposalStatusID
join dbo.RoleLU rlu on rlu.Role = 'Lead Estimator'
join dbo.ProposalUserRole pur on pur.ProposalID = p.ProposalID AND pur.RoleID = rlu.RoleID
join dbo.genTRACUser u on pur.UserID = u.UserID
where WorkflowStatus in (110, 120);