IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOEApprover]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOEApprover];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertBOEApprover]
(
@BOEID int,
@ETIUserID int,
@ChangedByETIUserID int
)
AS
/******************************************************************************
**		 
**		Name:	[insertBOEApprover]
**		Desc:	Insert BOE Approver
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
**		9/20/11		dcanuso				WI 5160 BOE User Role History added
**										Also needed to add the user who made
**										the change
**		9/26/13		dcanuso				Return PK
**		12/4/13		dcanuso				WI 24910
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500),
		@UpdateDT datetime2 = GetDate()

DECLARE @Inserted AS Table (ID int)


		

IF EXISTS (SELECT 1 FROM dbo.BOEPotentialRole WHERE RoleID = 9 AND ETIUserID = @ETIUserID)
BEGIN

	SET @ErrorMessage =   'You can not add any other roles to a user that has the Subcontractor Author role '
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END




IF NOT EXISTS (SELECT 1 FROM dbo.BOEUserRole WHERE BOEID = @BOEID AND ETIUserID = @ETIUserID AND RoleID = 3/*Approver*/)
	BEGIN 
		INSERT INTO [dbo].[BOEUserRole]
				   ([ETIUserID]
				   ,[RoleID]
				   ,[BOEID]
				   ,[UpdateDT]
				   )
		VALUES
				(
				@ETIUserID,
				3, /*Role ID For Approver is 3*/
				@BOEID,
				@UpdateDT
				)

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
           ,NULL /*We are adding a new Approver*/
           ,@ETIUserID /*New Approver we added*/
           ,3 /*RoleID for Approver is 3*/
           ,@BOEID
           ,8 /*FieldID for Approver is 8 */
           ,@ChangedByETIUserID)

	END

IF NOT EXISTS (SELECT 1 FROM dbo.BOEApproval WHERE BOEID = @BOEID AND ApprovalETIUserID = @ETIUserID)
	BEGIN
		INSERT INTO [dbo].[BOEApproval]
			   ([BOEID]
			   ,[ApprovalETIUserID]
			   ,[ApprovedFlag]
			   ,[UpdateDT])
    OUTPUT inserted.[BOEApprovalID] INTO @Inserted     
		VALUES
				(
				@BOEID,
				@ETIUserID,
				NULL,
				@UpdateDT
				)
	END

		SELECT [ID] AS [BOEApprovalID] FROM @Inserted
GO