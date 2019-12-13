IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAttachment];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteAttachment]
(
@AttachmentID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteAttachment]
**		Desc: Delete Attachment
**			
**		
**
**		Auth: twilson3
**		Date: 9/7/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[Attachment] WHERE ID = @AttachmentID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[Attachment] WHERE ID = @AttachmentID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO