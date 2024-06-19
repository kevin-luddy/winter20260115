IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMOQTypeSelectionTableDataResourceHoursviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertMOQTypeSelectionTableDataResourceHoursParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_MOQTypeSelectionTableDataResourceHours' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_MOQTypeSelectionTableDataResourceHours];
GO

CREATE TYPE [dbo].[TT_MOQTypeSelectionTableDataResourceHours] AS TABLE(
		[ResourceName] varchar(20) NULL,
		[WbsHours] decimal(11,2) NOT NULL,
		[TotalHours] decimal(11,2) NOT NULL,
		[MOQTypeSelectionTableDataId] int NOT NULL,
		[BOETaskElementID] int NOT NULL,
		[BOEID] int NOT NULL
);
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertMOQTypeSelectionTableDataResourceHoursviaTableParameter]
(
	@MOQTypeSelectionTableDataResourceHoursTableParameter [dbo].[TT_MOQTypeSelectionTableDataResourceHours] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertMOQTypeSelectionTableDataResourceHoursviaTableParameter]
**		Desc: Insert/Update data into MOQTypeSelectionTableDataResourceHours Table
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
**      06/18/24	e405721				PROPH-2062 Initial insert logic
*******************************************************************************/
BEGIN
	DECLARE @DistinctMOQTypeSelectionID int
	SELECT @DistinctMOQTypeSelectionID = MOQTypeSelectionTableDataId
	FROM (
		SELECT DISTINCT MOQTypeSelectionTableDataId
		FROM @MOQTypeSelectionTableDataResourceHoursTableParameter
	) AS temp_MOQTypeSelectionTableDataResourceHours

	DELETE FROM [dbo].[MOQTypeSelectionTableDataResourceHours]
	WHERE [MOQTypeSelectionTableDataId] = @DistinctMOQTypeSelectionID

	INSERT INTO [dbo].[MOQTypeSelectionTableDataResourceHours]
		([ResourceName]
		 ,[WbsHours]
		 ,[TotalHours]
		 ,[MOQTypeSelectionTableDataId]
		 ,[BOETaskElementID]
		 ,[BOEID]
		 )
	SELECT ResourceName
		,WbsHours
		,TotalHours
		,MOQTypeSelectionTableDataId
		,BOETaskElementID
		,BOEID
	FROM @MOQTypeSelectionTableDataResourceHoursTableParameter
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @MOQTypeSelectionTableDataResourceHoursTableParameter
GO