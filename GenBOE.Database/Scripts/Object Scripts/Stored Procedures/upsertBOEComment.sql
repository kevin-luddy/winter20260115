IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOEComment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOEComment];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOEComment]
(
	@BOECommentID int,
	@FieldID int,
	@BOEComment varchar(500),
	@ETIUserID int,
	@BOEResponseCommentID int,
	@BOEID int,
	@UpdateDT datetime2
)           
AS
/******************************************************************************
**		 
**		Name: upsertBOEComment
**		Desc:	Inserts/Updates a record into the BOE Comment table. Comment history will
**				go into BOE Comment History table.  Stored procedure should only
**				be used for changes as anything passed in with be saved.
**				
**			
**		
**
**		Auth: Don Canuso
**		Date: 11/30/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		12/21/10	dillea				Requirements stated that an author could be
**										changed and the new author could edit existing
**										author responses. This requires that the UserID
**										be updated when a comment is updated. So, the
**										BOECommentETIUserID is now set on update.
**		1/14/11		dcanuso				Developer request to add optimistic locking
*******************************************************************************/
SET NOCOUNT ON 

DECLARE		@PreviousComment varchar(500),
			@ErrorMessage varchar (500)





/*Table used to pass ID back to application*/
DECLARE @InsertedBOEComment AS TABLE (BOECommentID int)

/*
Obtain the previous comment for BOE Comment History table
If no comment exists, a NULL will be inserted as Previous Value and 
the value being passed in is the Updated value
*/

SELECT @PreviousComment = BOEComments FROM dbo.BOEComment WHERE BOECommentID = @BOECommentID



IF @BOECommentID < 0
BEGIN

SET @UpdateDT = GetDate()
/*Insert record and get inserted ID*/
INSERT INTO [dbo].[BOEComment]
           ([FieldID]
           ,[BOEComments]
           ,[BOECommentETIUserID]
           ,[UpdateDT]
           ,[BOEResponseToCommentID]
           ,[BOEID])
	OUTPUT inserted.BOECommentID INTO @InsertedBOEComment
    VALUES
           (@FieldID
           ,@BOEComment
           ,@ETIUserID
           ,@UpdateDT
           ,@BOEResponseCommentID
           ,@BOEID)  

SELECT @BOECommentID = BOECommentID FROM @InsertedBOEComment


/*Insert the change into the BOE Comment History Table*/

INSERT INTO [dbo].[BOECommentHistory]
           ([BOECommentID]
           ,[BOEID]
           ,[FieldID]
           ,[CurrentComment]
           ,[UpdatedComment]
           ,[ChangedByETIUserID]
           ,[UpdateDT])
     VALUES
           (@BOECommentID
           ,@BOEID
           ,@FieldID
           ,@PreviousComment
           ,@BOEComment
           ,@ETIUserID
           ,@UpdateDT)



SELECT @BOECommentID AS BOECommentID

END
ELSE
BEGIN
	/*Optimistic Locking Added*/
	IF (SELECT UpdateDT FROM dbo.BOEComment WHERE BOECommentID = @BOECommentID) = @UpdateDT
	BEGIN
	SET @UpdateDT = GetDate()
	/*Update the comment*/
	UPDATE [dbo].[BOEComment]
		SET 
			[BOEComments] = @BOEComment,
			[BOECommentETIUserID] = @ETIUserID,
			[UpdateDT] = @UpdateDT
	WHERE
		BOECommentID = @BOECommentID
	
	
	
	/*Insert the change into the BOE Comment History Table*/

	INSERT INTO [dbo].[BOECommentHistory]
           ([BOECommentID]
           ,[BOEID]
           ,[FieldID]
           ,[CurrentComment]
           ,[UpdatedComment]
           ,[ChangedByETIUserID]
           ,[UpdateDT])
     VALUES
           (@BOECommentID
           ,@BOEID
           ,@FieldID
           ,@PreviousComment
           ,@BOEComment
           ,@ETIUserID
           ,@UpdateDT)



	SELECT @BOECommentID AS BOECommentID

	END
	ELSE
		BEGIN
			
			SET @ErrorMessage =   'The BOE Comment with ID ' + CAST(@BOECommentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
END
GO