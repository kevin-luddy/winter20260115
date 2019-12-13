IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEComment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEComment];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOEComment]
(
@BOECommentID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOEComment]
**		Desc: Delete BOEComment 
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/10/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[BOEComment] WHERE BOECommentID = @BOECommentID ) = @UpdateDT
		BEGIN
		
			DELETE FROM dbo.BOECommentHistory
			WHERE
				BOECommentID = @BOECommentID
		
			DELETE FROM dbo.BOEComment
			WHERE
				BOECommentID = @BOECommentID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The BOE Comment with ID ' + CAST(@BOECommentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END


GO