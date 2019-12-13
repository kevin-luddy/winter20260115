IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEApprover]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEApprover];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBOEApprover]
(
@BOEApprovalID int,
@UpdateDT datetime2,
@ChangedByETIUserID int
)
AS
/******************************************************************************
**		 
**		Name:	[deleteBOEApprover]
**		Desc:	Delete BOE Approver
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/23/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/25/11		dcanuso				WI4307:Remove linkage from 
**										BOEApprovalHistory table to 
**										BOEApproval table 
**										Developer Request: SP should take in 
**										columns from BOE Approval.  Delete the 
**										row from BOE Approval.  Delete the row 
**										from BOE User Role.  Synch BOE Approval table. 
**		9/20/11		dcanuso				WI 5160 BOE User Role History added
**										Also needed to add the user who made
**										the change
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.BOEApproval WHERE BOEApprovalID = @BOEApprovalID) = @UpdateDT
	BEGIN
		SET @UpdateDT = GETDATE()
		
		DECLARE @BOEID int,
				@ETIUserID int

		SELECT	@BOEID = BOEID,
				@ETIUserID = ApprovalETIUserID
		FROM dbo.BOEApproval 
		WHERE BOEApprovalID = @BOEApprovalID
		
		DELETE FROM dbo.BOEApproval WHERE BOEApprovalID = @BOEApprovalID

		INSERT INTO [dbo].[BOEUserRoleHistory]
			   ([UpdateDT]
			   ,[CurrentETIUserID]
			   ,[UpdatedETIUserID]
			   ,[RoleID]
			   ,[BOEID]
			   ,[FieldID]
			   ,[ChangedByETIUserID])
		 VALUES
			   (@UpdateDT
			   ,@ETIUserID /*Person being deleted as Approver*/
			   ,NULL /*Approver was deleted*/
			   ,3 /*RoleID for Approver is 3*/
			   ,@BOEID
			   ,8 /*FieldID for Approver is 8 */
			   ,@ChangedByETIUserID)

		
		DELETE FROM dbo.BOEUserRole 
		WHERE 
		ETIUserID = @ETIUserID AND
		RoleID = 3	/*Approver*/ AND
		BOEID = @BOEID

END





GO


