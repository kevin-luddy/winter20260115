IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertLocation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertLocation];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertLocation]
(
@LocationName varchar(40),
@UpdatedByETIUserID int
)
AS
/******************************************************************************
**		 
**		Name: [insertLocation]
**		Desc: Insert/Update data into Location
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/20/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/2/11		dcanuso				WI 4354: DB - System Admin - Add Trip - 
**										Remove "Code" fields
**		8/19/11		dcanuso				Add Code field back
**		11/30/11	dcanuso				Allow Multiple Codes per Location
**		12/19/11	dcanuso				WI 6261:
**										ADD:
**										[version].[Trip].[LocationCode]
**										[dbo].[Trip].[LocationCode](DestinationLocationCode and DepartureLocationCode)
**										DROP:
**										[version].[Location].[LocationCode]
**										[dbo].[Location].[LocationCode]
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@Inserted AS Table (ID int)

IF NOT EXISTS	(
					SELECT 1 FROM [dbo].[Location] 
					WHERE 
					[LocationName] = @LocationName
				)
BEGIN
		INSERT INTO [dbo].[Location]
           ([UpdateDT]
           ,[LocationName]
           ,[UpdatedByETIUserID])
        OUTPUT inserted.LocationID INTO @Inserted
		VALUES
			   (GETDATE()
			   ,@LocationName
			   ,@UpdatedByETIUserID)
			   
		IF @@ERROR = 0 SELECT [ID] AS LocationID FROM @Inserted 
END
ELSE
	BEGIN
		IF @@ERROR = 0 
			SELECT LocationID AS LocationID 
				FROM [dbo].[Location] 
				WHERE 
					[LocationName] = @LocationName 
	END
GO