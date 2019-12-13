IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertFileAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertFileAttachment]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Name					varchar(100),
	@Link					varchar(255),
	@SectionId				int = NULL,
	@RevisionId				int
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertFileAttachment]
	**		Desc:	Insert/Update a File Attachment
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 1/2/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[FileAttachment]
						([UpdateDate]
						,[Name]
						,[Link]
						,[SectionId]
						,[RevisionId]
						)
				OUTPUT inserted.FileAttachmentId INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Name
						,@Link
						,@SectionId
						,@RevisionId
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE [FileAttachmentId] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[FileAttachment]
					   SET  UpdateDate = @UpdateDate
							,[Name] = @Name
							,[Link] = @Link
							,[SectionId] = @SectionId
						WHERE 
							FileAttachmentId = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO