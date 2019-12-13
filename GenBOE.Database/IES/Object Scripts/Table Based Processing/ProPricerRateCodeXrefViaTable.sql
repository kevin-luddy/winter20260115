-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ProPricerRateCodeXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_ProPricerRateCodeXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_ProPricerRateCodeXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateProPricerRateCodeXrefviaTableParameter]
**		Desc: Update data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[ProPricerRateCodeXref]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Description] = TT.Description,
	[RateCodeExtensionID] = TT.RateCodeExtensionID,
	[ResourceClassID] = TT.ResourceClassID
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID]
				
IF @@ERROR = 0
SELECT	PPX.ID AS ID,
		PPX.[UpdateDate]
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertProPricerRateCodeXrefviaTableParameter]
**		Desc: Insert data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_ProPricerXref table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_ProPricerXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar] (255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_ProPricerXref
SELECT * FROM @ProPricerRateCodeXref

DECLARE @ID [int],
		@RateCodeID [int],
		@Description [varchar](255),
		@RateCodeExtensionID [int],
		@ResourceClassID [int],
		@OrderID [int]

DECLARE @InsertedProPricerXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_ProPricerXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Description = Description,
		@RateCodeExtensionID = RateCodeExtensionID,
		@ResourceClassID = ResourceClassID,
		@OrderID = OrderID
	FROM @TT_ProPricerXref
	WHERE ID < 0

	INSERT INTO [dbo].[ProPricerRateCodeXref]
           ([UpdateDate]
		   ,[RateCodeID]
		   ,[Description]
		   ,[RateCodeExtensionID]
		   ,[ResourceClassID]
		   )
     OUTPUT inserted.ID INTO @InsertedProPricerXref
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Description
           ,@RateCodeExtensionID
 		   ,@ResourceClassID
           ) 
            
	SELECT @ID = ID FROM @InsertedProPricerXref
	
	UPDATE @TT_ProPricerXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_ProPricerXref TT
		ORDER BY OrderID
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteProPricerRateCodeXrefviaTableParameter]
**		Desc: Delete data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[ProPricerRateCodeXref]
WHERE ID IN (SELECT ID FROM @ProPricerRateCodeXref)

GO
