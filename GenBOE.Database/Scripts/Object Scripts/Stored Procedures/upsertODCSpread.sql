IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCSpread];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertODCSpread]
(
@ODCSpreadID int,
@ODCTypeID int,
@ODCSpreadDate date,
@ODCSpreadValue bigint
)
AS
/******************************************************************************
**		 
**		Name: [upsertODCSpread]
**		Desc: Insert/Update data into ODC Spread Section of ODC for BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 



DECLARE	@InsertedODCSpread AS Table (ODCSpreadID int)

IF @ODCSpreadID  < 0  /*Insert Record*/
	BEGIN

	INSERT INTO [dbo].[ODCSpread]
           (
           [ODCTypeID]
           ,[ODCSpreadDate]
           ,[ODCSpreadValue]
           )
     OUTPUT inserted.ODCSpreadID INTO @InsertedODCSpread
     VALUES
           (
           @ODCTypeID,
           @ODCSpreadDate,
           @ODCSpreadValue
           )
           
           
	SELECT 	@ODCSpreadID = ODCSpreadID FROM @InsertedODCSpread
	END
ELSE
	BEGIN
		UPDATE [dbo].[ODCSpread]
		   SET 
				[ODCTypeID] = @ODCTypeID,
				[ODCSpreadDate] = @ODCSpreadDate,
				[ODCSpreadValue] = @ODCSpreadValue
		 WHERE
				ODCSpreadID = @ODCSpreadID
	END

IF @@ERROR = 0
	SELECT @ODCSpreadID AS ODCSpreadID
GO