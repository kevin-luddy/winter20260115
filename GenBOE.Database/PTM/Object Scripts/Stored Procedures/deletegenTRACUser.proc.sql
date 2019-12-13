IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletegenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletegenTRACUser];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deletegenTRACUser]
(
@UserID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletegenTRACUser]
**		Desc: Delete genTRACUser 
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/5/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[genTRACUser] WHERE UserID = @UserID ) = @UpdateDT
		BEGIN
		

			DELETE FROM dbo.genTRACUser
			WHERE UserID = @UserID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The User with ID ' + CAST(@UserID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO