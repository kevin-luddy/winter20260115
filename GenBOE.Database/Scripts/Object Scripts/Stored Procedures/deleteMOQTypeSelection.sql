IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMOQTypeSelection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMOQTypeSelection];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteMOQTypeSelection]
(
@MOQTypeSelectionId int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteMOQTypeSelection]
**		Desc: Delete MOQ Type Selection for the given ID
**			
**		
**
**		Auth: ranzalon
**		Date: 9/11/2020
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[MOQTypeSelection] WHERE [MOQTypeSelectionId] = @MOQTypeSelectionId) = @UpdateDT
		BEGIN

			DELETE FROM [dbo].[MOQTypeSelectionTableData]
			WHERE [MOQTypeSelectionId] = @MOQTypeSelectionId

			DELETE FROM [dbo].[MOQTypeSelection]
			WHERE [MOQTypeSelectionId] = @MOQTypeSelectionId

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The MOQ Type Selection Table Data with ID ' + CAST(@MOQTypeSelectionId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END

GO