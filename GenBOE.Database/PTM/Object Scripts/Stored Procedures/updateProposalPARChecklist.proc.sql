IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalPARChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalPARChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalPARChecklist]
(
	@ProposalID [int],
	@PARChecklist_Response [varchar](MAX),
	@ResponseTypeID [int],
	@Comment [varchar] (max),
	@CompletedByUserID int,
	@IsSubmittal bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateProposalPARChecklist]
**		Desc:	Update Proposal PAR Checklist Responses
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
**		11/10/14	dcanuso				WI 30055 Comment column added so added
**										characters to column
**`		03/05/19	Dusan				Added canned responses
*******************************************************************************/
SET NOCOUNT ON 
DECLARE 
	@ErrorMessage varchar (500),
	@SubmitDate datetime2(7),
	@UpdateDate datetime2(7) = GETDATE()
	
IF @IsSubmittal = 1 SET @SubmitDate = @UpdateDate

SET @PARChecklist_Response = REPLACE (@PARChecklist_Response, '~~', '~NULL~')
SET @PARChecklist_Response = REPLACE (@PARChecklist_Response, '~^', '~NULL^')
/*
Process @PARChecklist_Response
*/
IF RIGHT(@PARChecklist_Response, 1) <> '^'
	SET @PARChecklist_Response = @PARChecklist_Response + '^'

DECLARE @Checklist TABLE 
(
	StringToProcess varchar (MAX),
	ChecklistID INT,
	ResponseID int,
	Comment varchar (500) DEFAULT NULL,
	PageNumber varchar (50) DEFAULT NULL,
	CannedResponseId INT NULL
)

DECLARE @NextString varchar(1000), @NextPlaceHolderID int
WHILE (SELECT CHARINDEX ('^', @PARChecklist_Response) ) > 1
BEGIN
	SELECT @NextString = LEFT (@PARChecklist_Response, CHARINDEX ('^', @PARChecklist_Response) -1)

	INSERT INTO @Checklist (StringToProcess) VALUES (@NextString)

	SET @PARChecklist_Response = RIGHT (@PARChecklist_Response, LEN (@PARChecklist_Response) - LEN (@NextString) -1)
	SET @NextString = NULL

END


/*
	There are three columns that need to updated so handling as three sets of statement
	If performance is an issue, dynamic processing could be added.
*/	

UPDATE @Checklist SET StringToProcess = StringToProcess + '~' WHERE RIGHT (StringToProcess, 1) <> '~'
UPDATE @Checklist 
	SET ChecklistID = LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET ResponseID = LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET Comment = IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET PageNumber = IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET CannedResponseId = TRY_CONVERT(int, IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL))

UPDATE @Checklist
	SET Comment = CASE 
						WHEN Comment = '' THEN NULL 
						WHEN Comment = 'NULL' THEN NULL
						ELSE Comment
					END,
		PageNumber 	 = CASE 
						WHEN PageNumber = '' THEN NULL 
						WHEN PageNumber = 'NULL' THEN NULL
						ELSE PageNumber
					END

UPDATE dbo.ProposalPARChecklistXREF
SET ResponseID = C.ResponseID,
	Comment = C.Comment,
	PageNumber = C.PageNumber,
	CannedResponseId = C.CannedResponseId
FROM  dbo.ProposalPARChecklistXREF X
	INNER JOIN @Checklist C ON X.PARChecklistContentID = C.ChecklistID
WHERE
	X.ProposalID = @ProposalID AND
	X.ResponseTypeID = @ResponseTypeID 

/*Update completed table*/
IF NOT EXISTS (SELECT 1 
				FROM dbo.ProposalChecklistComplete 
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = @ResponseTypeID AND
					ChecklistTypeID = 2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
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
           (GETDATE()
           ,@ProposalID
           ,@CompletedByUserID
           ,@ResponseTypeID
           ,2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
           ,@Comment
           ,@SubmitDate
           ,@UpdateDate
           )
END
ELSE
BEGIN
UPDATE [dbo].[ProposalChecklistComplete]
   SET [UpdateDate] = GETDATE()
      ,[CompletedByUserID] = @CompletedByUserID
      ,[Comment] = @Comment
      ,[SubmitDate] = @SubmitDate
      ,[LastSaveDate] = @UpdateDate
   WHERE
		ProposalID = @ProposalID AND
		ResponseTypeID = @ResponseTypeID AND
		ChecklistTypeID = 2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
END

GO