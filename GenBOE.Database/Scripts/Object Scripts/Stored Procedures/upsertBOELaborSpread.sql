IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOELaborSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOELaborSpread];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOELaborSpread]
(
@BOELaborSpreadID int,
@BOELaborTypeID int,
@LaborSpreadDate date,
@LaborSpreadValue DECIMAL (18, 6)
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOELaborSpread]
**		Desc: Insert/Update data into LM Labor Spread Section of BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		10/13/10	dcanuso				Developer Request - Rempove optimistic locking 
**										from BOE Labor Spread table 
**		5/26/11		dcanuso				Spread Value to bigint		
**		10/7/14		dcanuso				Precision Story
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@InsertedBOELaborSpread AS Table (BOELaborSpreadID int)

IF @BOELaborSpreadID  < 0  /*Insert Record*/
	BEGIN

	INSERT INTO [dbo].[BOELaborSpread]
           (
           [BOELaborTypeID]
           ,[LaborSpreadDate]
           ,[LaborSpreadValue]
           )
     OUTPUT inserted.BOELaborSpreadID INTO @InsertedBOELaborSpread
     VALUES
           (
           @BOELaborTypeID,
           @LaborSpreadDate,
           @LaborSpreadValue
           )
           
           
	SELECT 	@BOELaborSpreadID = BOELaborSpreadID FROM @InsertedBOELaborSpread
	END
ELSE
	BEGIN
		UPDATE [dbo].[BOELaborSpread]
		   SET 
				[BOELaborTypeID] = @BOELaborTypeID,
				[LaborSpreadDate] = @LaborSpreadDate,
				[LaborSpreadValue] = @LaborSpreadValue
		 WHERE
				BOELaborSpreadID = @BOELaborSpreadID
	END

IF @@ERROR = 0
	SELECT @BOELaborSpreadID AS BOELaborSpreadID
GO