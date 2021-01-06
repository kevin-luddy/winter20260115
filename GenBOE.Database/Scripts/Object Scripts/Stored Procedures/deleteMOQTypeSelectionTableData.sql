IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMOQTypeSelectionTableData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMOQTypeSelectionTableData];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteMOQTypeSelectionTableData] (@MOQTypeSelectionTableDataId int, @UpdateDT datetime2)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteMOQTypeSelectionTableData]
	**		Desc: Delete MOQ Type Selection Table Data for the given ID
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
	**		1/13/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
	*******************************************************************************/
	SET NOCOUNT ON 
	IF (SELECT UpdateDT FROM [dbo].[MOQTypeSelectionTableData] WHERE [MOQTypeSelectionTableDataId] = @MOQTypeSelectionTableDataId) = @UpdateDT
		BEGIN
			DELETE FROM [dbo].[MoqTypeTableCustomFieldValueXREF] WHERE MoqTypeTableDataId = @MOQTypeSelectionTableDataId
			DELETE FROM [dbo].[MOQTypeSelectionTableData] WHERE [MOQTypeSelectionTableDataId] = @MOQTypeSelectionTableDataId
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500) = 'The MOQ Type Selection Table Data with ID ' + CAST(@MOQTypeSelectionTableDataId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN 
		END
GO