IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalPPRChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalPPRChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalPPRChecklist]
(
	@ProposalID [int],
	@PPRChecklist_Response [varchar](1000),
	@ResponseTypeID [int],
	@Comment [varchar] (max),
	@CompletedByUserID int,
	@IsSubmittal bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateProposalPPRChecklist]
**		Desc:	Update Proposal PPR Checklist Responses
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
DECLARE 
	@ErrorMessage varchar (500),
	@SubmitDate datetime2(7),
	@UpdateDate datetime2(7)
	
SET @UpdateDate = GETDATE()

IF @IsSubmittal = 1 SET @SubmitDate = @UpdateDate

	


/*
Process @PPRChecklist_Response
*/
IF RIGHT(@PPRChecklist_Response, 1) <> ';'
	SET @PPRChecklist_Response = @PPRChecklist_Response + ';'

DECLARE @Checklist TABLE 
(
	ChecklistID INT,
	ResponseID int
)

DECLARE @NextString varchar(1000)
WHILE (SELECT CHARINDEX (';', @PPRChecklist_Response) ) > 1
BEGIN
	
	
	SELECT @NextString = LEFT (@PPRChecklist_Response, CHARINDEX (';', @PPRChecklist_Response) -1)
	
	INSERT INTO @Checklist
	SELECT 
		LEFT (@NextString, CHARINDEX (',', @NextString) -1),
		RIGHT (@NextString, LEN (@NextString) - CHARINDEX (',', @NextString))
	
	SET @PPRChecklist_Response = RIGHT (@PPRChecklist_Response, LEN (@PPRChecklist_Response) - LEN (@NextString) -1)
	SET @NextString = NULL
	/*
	for testing
	SELECT * FROM @Checklist
	SELECT @PPRChecklist_Response
	*/
END



UPDATE dbo.ProposalPPRChecklistXREF
SET ResponseID = C.ResponseID/*,
	Comment = CASE C.ResponseID
				WHEN 4 THEN @Comment
				ELSE NULL
			  END*/
FROM  dbo.ProposalPPRChecklistXREF X
	INNER JOIN @Checklist C ON X.PPRChecklistContentID = C.ChecklistID
WHERE
X.ProposalID = @ProposalID AND
X.ResponseTypeID = @ResponseTypeID AND
C.ResponseID <> 4 /*DO NOT UPDATE COMMENTS*/


/*
UPDATE dbo.ProposalPPRChecklistXREF
SET Comment = @Comment
FROM  dbo.ProposalPPRChecklistXREF X
	INNER JOIN @Checklist C ON X.PPRChecklistContentID = C.ChecklistID
WHERE
X.ProposalID = @ProposalID AND
X.ResponseTypeID = @ResponseTypeID AND
C.ResponseID = 4 /*UPDATE COMMENTS*/
*/



/*Update completed table*/
IF NOT EXISTS (SELECT 1 
				FROM dbo.ProposalChecklistComplete 
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = @ResponseTypeID AND
					ChecklistTypeID = 1 /*Proposal Pricing Review*/
			 )
BEGIN
INSERT INTO [dbo].[ProposalChecklistComplete]
           ([UpdateDate]
           ,[ProposalID]
           ,[CompletedByUserID]
           ,[ResponseTypeID]
           ,[ChecklistTypeID]
           ,[Comment]
           ,[SubmitDate]
           ,[LastSaveDate]
           )
     VALUES
           (
			@UpdateDate
           ,@ProposalID
           ,@CompletedByUserID
           ,@ResponseTypeID
           ,1 /*Proposal Pricing Review*/
           ,@Comment
           ,@SubmitDate
           ,@UpdateDate
           )
END
ELSE
BEGIN
UPDATE [dbo].[ProposalChecklistComplete]
   SET [UpdateDate] = @UpdateDate
      ,[CompletedByUserID] = @CompletedByUserID
      ,[Comment] = @Comment
      ,[SubmitDate] = @SubmitDate
      ,[LastSaveDate] = @UpdateDate
   WHERE
		ProposalID = @ProposalID AND
		ResponseTypeID = @ResponseTypeID AND
		ChecklistTypeID = 1 /*Proposal Pricing Review*/
		
IF @IsSubmittal = 1
	BEGIN			
		UPDATE [dbo].[Proposal]
			SET [UpdateDate] = @UpdateDate
		WHERE
			ProposalID = @ProposalID 
	END
	
END

GO