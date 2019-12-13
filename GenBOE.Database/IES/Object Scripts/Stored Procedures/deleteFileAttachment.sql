IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteFileAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteFileAttachment]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteFileAttachment]
	**		Desc:	Delete a File Attachment
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
	
	IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE FileAttachmentId = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[FileAttachment] WHERE FileAttachmentId = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO
