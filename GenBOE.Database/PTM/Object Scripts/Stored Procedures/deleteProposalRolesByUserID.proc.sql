IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalRolesByUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalRolesByUserID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalRolesByUserID]
(
@UserID int,
@UpdateDT datetime2,
@ProposalID int,
@RoleID int,
@RoleTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposalRolesByUserID]
**		Desc: Deletes a Proposal User Role from Proposal User Role
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/3/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM dbo.ProposalUserRole WHERE 
			UserID = @UserID AND 
			ProposalID = @ProposalID AND 
			RoleID = @RoleID AND
			IsNull(RoleTypeID,-99) = ISNULL(@RoleTypeID,-99)
			) = @UpdateDT
	BEGIN
		DELETE FROM dbo.ProposalUserRole
		WHERE 
			[UserID] = @UserID AND
			[ProposalID] = @ProposalID AND 
			[RoleID] = @RoleID AND
			IsNull(RoleTypeID,-99) = ISNULL(@RoleTypeID,-99)
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The Proposal User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END

GO