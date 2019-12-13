IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllODCTaskElements]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllODCTaskElements];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[deleteAllODCTaskElements]
(
@BoeId int
)
AS
/******************************************************************************
**		 
**		Name: [deleteAllODCTaskElements]
**		Desc: Deletes all ODC's in a BOE 
**			
**		
**
**		Auth: brunworg
**		Date: 1/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*****************************************************************************/
DECLARE @OdcCursor CURSOR, @ODCTaskElementID int, @UpdateDT datetime2(7);
SET NOCOUNT ON 
	BEGIN
		SET @OdcCursor = CURSOR FOR
			SELECT [ODCTaskElementID],[UpdateDT]
			FROM [dbo].[ODCTaskElement]
			WHERE BOEID = @BoeId;

		OPEN @OdcCursor
		FETCH NEXT FROM @OdcCursor
		INTO @ODCTaskElementID, @UpdateDT;

		WHILE @@FETCH_STATUS = 0
		BEGIN

			EXEC [dbo].[deleteODCTaskElement] @ODCTaskElementID, @UpdateDT

			FETCH NEXT FROM @OdcCursor
			INTO @ODCTaskElementID, @UpdateDT;

		END
	END
GO