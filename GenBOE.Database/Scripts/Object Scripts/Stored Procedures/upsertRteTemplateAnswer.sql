IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRteTemplateAnswer]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRteTemplateAnswer];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertRteTemplateAnswer]
(
@AnswerID int,
@UpdateDT datetime2,
@QuestionID int,
@BOEID int,
@TaskID int null,
@Text varchar(max) null,
@RteTemplateSourceId int
)
AS
/******************************************************************************
**		 
**		Name:	[upsertRteTemplateAnswer]
**		Desc:	Insert/Update RTE Template Answer
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: December 2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		02/19/20	Dusan				Fixed empty answer needs to be deleted
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @InsertedTemplateAnswer AS Table (AnswerID int)
DECLARE	@ErrorMessage varchar (500)

-- delete existing answers that are "blank"
IF @AnswerID > 0 AND (@Text IS NULL OR @Text = '')
BEGIN
	DELETE FROM [dbo].[RteTemplateAnswer] WHERE AnswerId = @AnswerID
END
ELSE IF @AnswerID < 0  /*Insert Record*/
	BEGIN
		
		SET @UpdateDT = GETDATE()
			
		INSERT INTO [dbo].[RteTemplateAnswer]
			([UpdateDT],
			[QuestionID],
			[BOEID],
			[TaskID],
			[Text],
			[RteTemplateSourceId])
			OUTPUT inserted.AnswerID INTO @InsertedTemplateAnswer
			VALUES
				(
				@UpdateDT,
				@QuestionID,
				@BOEID,
				@TaskID,
				@Text,
				@RteTemplateSourceId
				)
		           
		SELECT @AnswerID = AnswerID FROM @InsertedTemplateAnswer
	END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT 
				FROM [dbo].[RteTemplateAnswer] 
				WHERE	AnswerID = @AnswerID 
		) = @UpdateDT
			/*Update Dates Match*/
			BEGIN
				SET @UpdateDT = GETDATE()
						
				UPDATE [dbo].[RteTemplateAnswer]
					SET [UpdateDT] = @UpdateDT,
						[QuestionID] = @QuestionID,
						[BOEID] = @BOEID,
						[TaskID] = @TaskID,
						[Text] = @Text,
						[RteTemplateSourceId] = @RteTemplateSourceId
						
					WHERE AnswerID = @AnswerID
			END
		ELSE
			BEGIN

				SET @ErrorMessage =   'The RTE Template with Template Answer ID ' + @AnswerID + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN 
			END
	END

IF @@ERROR = 0
	SELECT	@AnswerID AS AnswerID
GO