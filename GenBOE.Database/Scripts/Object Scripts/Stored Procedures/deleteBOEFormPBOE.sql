IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEFormPBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEFormPBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOEFormPBOE]
(
@PBOEFormID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOEFormPBOE]
**		Desc: Delete all parts of PBOE
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

	IF (SELECT UpdateDT FROM [dbo].[BOEFormPBOE] WHERE PBOEFormID = @PBOEFormID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GETDATE()
			
			DELETE FROM [dbo].[BOEFormPBOEResourcesXREF] WHERE [PBOEFormID] = @PBOEFormID
			DELETE FROM [dbo].[BOEFormPBOECLINsXREF] WHERE [PBOEFormID] = @PBOEFormID

			DELETE FROM dbo.[BOEFormPBOE]	
			WHERE [PBOEFormID] = @PBOEFormID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The PBOE with ID ' + CAST(@PBOEFormID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO