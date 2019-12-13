IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEFormIBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEFormIBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOEFormIBOE]
(
@IBOEFormID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOEFormIBOE]
**		Desc: Delete all parts of IBOE
**			
**		
**
**		Auth: Tim Wilson
**		Date: 10/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		10/27/16	twilson3			BOEJ-1531 Add BOE Forms CLIN xref table
**
*****************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDT FROM [dbo].[BOEFormIBOE] WHERE IBOEFormID = @IBOEFormID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GETDATE()
			
			DELETE FROM [dbo].[BOEFormIBOEResourcesXREF] WHERE [IBOEFormID] = @IBOEFormID
			DELETE FROM [dbo].[BOEFormIBOECLINsXREF] WHERE [IBOEFormID] = @IBOEFormID

			DELETE FROM dbo.[BOEFormIBOE]	
			WHERE [IBOEFormID] = @IBOEFormID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The IBOE with ID ' + CAST(@IBOEFormID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO