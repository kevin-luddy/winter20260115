IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProposalUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProposalUserRole];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertProposalUserRole]
(
	@ProposalID int,
	@RoleID int,
	@RoleTypeID int,
	@UserID int
	)
AS
/******************************************************************************
**		 
**		Name: insertProposalUserRole
**		Desc: Inserts a record into the Proposal User Role table for DTO
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
**		5/13/13		dcanuso				WI 18300
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[ProposalUserRole] WHERE	
					 UserID = @UserID AND
					 ProposalID = @ProposalID AND
				 	 RoleID = @RoleID AND
				 	 ISNULL(RoleTypeID, -99) = ISNULL(@RoleTypeID, -99)
			)
			BEGIN
				SET @ErrorMessage =   'This Proposal User Role already exists.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN		
		
		DECLARE @UpdateDate datetime2 = GETDATE(),
				@ProposalUserRoleID int
			
		INSERT INTO [dbo].[ProposalUserRole]
				   ([UserID]
				   ,[RoleID]
				   ,[ProposalID]
				   ,[UpdateDate]
				   ,[RoleTypeID])		 
		OUTPUT inserted.ProposalUserRoleID INTO @Inserted
			 VALUES
				   (
					@UserID, 
					@RoleID, 
					@ProposalID, 
					@UpdateDate,
					@RoleTypeID					
					)		 
		SELECT @ProposalUserRoleID = ID FROM @Inserted
		
/*
Code is now going to do this		
		IF @RoleID = 3	/*Pricer*/
			/*
			When a New Pricer is assigned
			Proposal.DateAssigned gets updated
			*/			
			BEGIN
				UPDATE dbo.Proposal
				SET DateAssigned = @UpdateDate
				WHERE ProposalID = @ProposalID
			END
*/					
			

		
	END
	

IF @@ERROR = 0
	SELECT @ProposalUserRoleID as ProposalUserRoleID

GO