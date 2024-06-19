IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMOQTypeSelectionTableDataResourceHours]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMOQTypeSelectionTableDataResourceHours];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteMOQTypeSelectionTableDataResourceHours]
(
	@MOQTypeSelectionTableDataId int
)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteMOQTypeSelectionTableDataResourceHours]
	**		Desc: Delete all parts of MOQ Type Selection Table Data Resource Hours Table
	**			
	**		
	**
	**		Auth: Thomas Asuncion
	**		Date: 6/2024
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**      6/10/24		e405721 			PROPH-2061 Initial creation of delete
	*****************************************************************************/
	BEGIN
		DELETE FROM dbo.[MOQTypeSelectionTableDataResourceHours]
		WHERE [MOQTypeSelectionTableDataId] = @MOQTypeSelectionTableDataId

		SELECT @@ROWCOUNT AS RowsAffected;
	END
GO