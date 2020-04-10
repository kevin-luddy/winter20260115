IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRteTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRteTemplate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertRteTemplate]
(
@TemplateID int,
@UpdateDT datetime2,
@WorkspaceID int,
@Description varchar(200),
@AuthorID int,
@Assigned varchar(max) NULL
)
AS
/******************************************************************************
**		 
**		Name:	[upsertRteTemplate]
**		Desc:	Insert/Update RTE Template
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
**		04/10/20	ranzalon			BOEJ-4546 - Clear underlying template on assign
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @InsertedTemplate AS Table (TemplateID int)
DECLARE	@ErrorMessage varchar (500)

IF @TemplateID < 0  /*Insert Record*/
	BEGIN
		
		SET @UpdateDT = GETDATE()
			
		INSERT INTO [dbo].[RteTemplate]
			([UpdateDT],
			[WorkspaceID],
			[Description],
			[AuthorID],
			[CreatedOn])
			OUTPUT inserted.TemplateID INTO @InsertedTemplate
			VALUES
				(
				@UpdateDT,
				@WorkspaceID,
				@Description,
				@AuthorID,
				@UpdateDT
				)
		           
		SELECT @TemplateID = TemplateID FROM @InsertedTemplate
	END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT 
				FROM [dbo].[RteTemplate] 
				WHERE	TemplateID = @TemplateID 
		) = @UpdateDT
			/*Update Dates Match*/
			BEGIN
				SET @UpdateDT = GETDATE()
						
				UPDATE [dbo].[RteTemplate]
					SET [UpdateDT] = @UpdateDT,
						[WorkspaceID] = @WorkspaceID,
						[Description] = @Description,
						[AuthorID] = @AuthorID
					WHERE TemplateID = @TemplateID 
			END
		ELSE
			BEGIN

				SET @ErrorMessage =   'The RTE Template with Template ID ' + @TemplateID + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN 
			END
	END

-- Now clear out old Assigned and put in new Assigned
DELETE FROM [dbo].[RteTemplateAssigned] WHERE [TemplateID] = @TemplateID

IF @Assigned IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[RteTemplateAssigned]
			([TemplateID],
			[RteTemplateSourceId])
		SELECT @TemplateID, Item FROM SplitString(@Assigned, ',', 0)

		-- Clear out underlying data
		IF EXISTS (SELECT 1 FROM SplitString(@Assigned, ',', 0) WHERE Item = 1) -- BOE Description
			BEGIN
				UPDATE [dbo].[BOE]
				SET [BOEDescription] = ''
				WHERE [WorkspaceID] = @WorkspaceID
			END

		IF EXISTS (SELECT 1 FROM SplitString(@Assigned, ',', 0) WHERE Item = 2) -- BOE Sources
			BEGIN
				UPDATE [dbo].[BOE]
				SET [DataSource] = ''
				WHERE [WorkspaceID] = @WorkspaceID
			END

		IF EXISTS (SELECT 1 FROM SplitString(@Assigned, ',', 0) WHERE Item = 3) -- Task Description
			BEGIN
				UPDATE [dbo].[BOETaskElement]
				SET [TaskDescription] = ''
				WHERE [BOEID] IN (
					SELECT [BOEID]
					FROM [dbo].[BOE]
					WHERE [WorkspaceID] = @WorkspaceID
					)
			END

		IF EXISTS (SELECT 1 FROM SplitString(@Assigned, ',', 0) WHERE Item = 4) -- Task MOQ Rationale
			BEGIN
				UPDATE [dbo].[BOETaskElement]
				SET [MOQText] = ''
				WHERE [BOEID] IN (
					SELECT [BOEID]
					FROM [dbo].[BOE]
					WHERE [WorkspaceID] = @WorkspaceID
					)
			END
	END

-- Now clear out old answers for old assigned
DELETE FROM [dbo].[RteTemplateAnswer]
	FROM [dbo].[RteTemplateAnswer] RTA 
		INNER JOIN [RteTemplateQuestion] RTQ ON RTQ.[QuestionID] = RTA.[QuestionID]
		WHERE RTQ.[TemplateID] = @TemplateID AND RTA.[RteTemplateSourceId] NOT IN
			(SELECT Item FROM SplitString(@Assigned, ',', 0))

IF @@ERROR = 0
	SELECT	@TemplateID AS TemplateID
GO