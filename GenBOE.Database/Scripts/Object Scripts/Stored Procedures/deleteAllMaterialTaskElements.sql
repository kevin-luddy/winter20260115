IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllMaterialTaskElements]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllMaterialTaskElements];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[deleteAllMaterialTaskElements]
(
@BoeId int
)
AS
/******************************************************************************
**		 
**		Name: [deleteAllMaterialTaskElements]
**		Desc: Deletes All Material elements for a BOE
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
DECLARE @MaterialCursor CURSOR, @MaterialTaskElementID int, @UpdateDT datetime2(7);
SET NOCOUNT ON 
	BEGIN
		SET @MaterialCursor = CURSOR FOR
			SELECT [MaterialTaskElementID],[UpdateDT]
			FROM [dbo].[MaterialTaskElement]
			WHERE BOEID = @BoeId;

		OPEN @MaterialCursor
		FETCH NEXT FROM @MaterialCursor
		INTO @MaterialTaskElementID, @UpdateDT;

		WHILE @@FETCH_STATUS = 0
		BEGIN

			EXEC [dbo].[deleteMaterialTaskElement] @MaterialTaskElementID, @UpdateDT

			FETCH NEXT FROM @MaterialCursor
			INTO @MaterialTaskElementID, @UpdateDT;

		END
	END
GO