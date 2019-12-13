IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalRolesByProposalID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalRolesByProposalID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalRolesByProposalID]
(
@ProposalID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposalRolesByProposalID]
**		Desc: Deletes all Proposal User Roles from Proposal User Role
**				fpr a Proposal
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/17/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
******************************************************************************/
SET NOCOUNT ON 

DELETE FROM dbo.ProposalUserRole
WHERE [ProposalID] = @ProposalID

GO