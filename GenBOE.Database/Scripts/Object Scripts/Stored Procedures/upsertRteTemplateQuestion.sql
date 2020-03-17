IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRteTemplateQuestion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRteTemplateQuestion];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertRteTemplateQuestion]
(
@QuestionID int,
@UpdateDT datetime2,
@TemplateID int,
@Text varchar(500),
@SortOrder int,
@Required bit
)
AS
/******************************************************************************
**		 
**		Name:	[upsertRteTemplateQuestion]
**		Desc:	Insert/Update RTE Template Question
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
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @InsertedTemplateQuestion AS Table (QuestionID int)
DECLARE	@ErrorMessage varchar (500)

IF @QuestionID < 0  /*Insert Record*/
	BEGIN
		
		SET @UpdateDT = GETDATE()
			
		INSERT INTO [dbo].[RteTemplateQuestion]
			([UpdateDT],
			[TemplateID],
			[Text],
			[SortOrder],
			[Required])
			OUTPUT inserted.QuestionID INTO @InsertedTemplateQuestion
			VALUES
				(
				@UpdateDT,
				@TemplateID,
				@Text,
				@SortOrder,
				@Required
				)
		           
		SELECT @QuestionID = QuestionID FROM @InsertedTemplateQuestion
	END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT 
				FROM [dbo].[RteTemplateQuestion] 
				WHERE	QuestionID = @QuestionID 
		) = @UpdateDT
			/*Update Dates Match*/
			BEGIN
				SET @UpdateDT = GETDATE()
						
				UPDATE [dbo].[RteTemplateQuestion]
					SET [UpdateDT] = @UpdateDT,
						[TemplateID] = @TemplateID,
						[Text] = @Text,
						[SortOrder] = @SortOrder,
						[Required] = @Required
					WHERE QuestionID = @QuestionID
			END
		ELSE
			BEGIN

				SET @ErrorMessage =   'The RTE Template with Template Question ID ' + @QuestionID + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN 
			END
	END

IF @@ERROR = 0
	SELECT	@QuestionID AS QuestionID
GO