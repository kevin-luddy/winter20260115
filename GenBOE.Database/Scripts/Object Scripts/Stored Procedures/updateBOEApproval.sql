IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOEApproval]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOEApproval];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateBOEApproval]
(
	@BOEApprovalID int,
	@BOEID int,
	@ApprovalETIUserID int,
	@ApprovedFlag bit,
	@UpdateDT datetime2
)           
AS
/******************************************************************************
**		 
**		Name:	updateBOEApproval
**		Desc:	Updates a record into the BOE Approval table. 
**				Approval/Rejection will go into the BOE Approval History
**				table.  
**				History will say Accepted or Rejected
**				When the first rejection happens the BOE Approval table gets cleared 
**				(Set to NULL), but the BOE Approval History table would keep the history
**
**		Auth: Don Canuso
**		Date: 12/7/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/21/11		dcanuso				Developer request to add optimistic 
**										locking
**		7/25/11		dcanuso				WI4307:Remove linkage from 
**										BOEApprovalHistory table to 
**										BOEApproval table 
*******************************************************************************/
SET NOCOUNT ON 
DECLARE		@ErrorMessage varchar (500)

DECLARE @Inserted AS Table (ID int)


IF (SELECT UpdateDT FROM [dbo].[BOEApproval] WHERE BOEID = @BOEID AND ApprovalETIUserID = @ApprovalETIUserID) = @UpdateDT
BEGIN

	SET	@UpdateDT = GetDate()

	IF @ApprovedFlag = 1 /*Approved*/

	BEGIN

		/*Update the Acceptance for the user.*/
		UPDATE [dbo].[BOEApproval]
		   SET [ApprovedFlag] = 1
			  ,[UpdateDT] = @UpdateDT
		 WHERE BOEApprovalID = @BOEApprovalID 
				----this is the id that needs to come back


		/*Insert the record into the History Table as well for reporting*/
		INSERT INTO [dbo].[BOEApprovalHistory]
				   ([BOEID]
				   ,[Approval]
				   ,[ApprovalETIUserID]
				   ,[UpdateDT])
			 VALUES
				   (@BOEID
				   ,'Approved'
				   ,@ApprovalETIUserID
				   ,@UpdateDT)

	END
	ELSE IF @ApprovedFlag = 0 /*Rejected*/
	/*Rejected - Thus all Previous Approvals are Cleared*/
		BEGIN

			UPDATE [dbo].[BOEApproval]
			   SET [ApprovedFlag] = NULL
				  ,[UpdateDT] = @UpdateDT
			WHERE      
			   [BOEID] = @BOEID


			/*Insert the record into the History Table as well for reporting*/
			INSERT INTO [dbo].[BOEApprovalHistory]
					   ([BOEID]
					   ,[Approval]
					   ,[ApprovalETIUserID]
					   ,[UpdateDT])
				 VALUES
					   (@BOEID
					   ,'Rejected'
					   ,@ApprovalETIUserID
					   ,@UpdateDT)
	   
		END
	ELSE IF @ApprovedFlag IS NULL /*None*/
		/*None - No history entry necessary*/
		BEGIN

			/*Update the Acceptance for the user.*/
			UPDATE [dbo].[BOEApproval]
			   SET [ApprovedFlag] = NULL
				  ,[UpdateDT] = @UpdateDT
			 WHERE BOEApprovalID = @BOEApprovalID 
	   
		END


		SELECT @BOEApprovalID AS BOEApprovalID


	END
ELSE /*Update Date Does Not Match*/
	BEGIN
		SET @ErrorMessage =   'The BOE Approval with BOE ID ' + CAST(@BOEID  AS varchar(10)) + ' and Approval ID ' + CAST(@ApprovalETIUserID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
		RETURN
	END
GO