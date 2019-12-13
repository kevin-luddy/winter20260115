-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeYearviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCodeYear' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCodeYear];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCodeYear] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18, 6),
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeYearviaTableParameter]
**		Desc: Update data in RateCodeYear Table.
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
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[RateCodeYear]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Year] = TT.Year,
	[Rate] = TT.Rate
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[ID] = TT.[ID] 
				
IF @@ERROR = 0
SELECT	RCY.ID AS ID,
		RCY.[UpdateDate]
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[RateCodeID] = TT.[RateCodeID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRateCodeYearviaTableParameter]
**		Desc: Insert data in RateCodeYear Table.
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
**		10/19/2017	Dusan				Fixed rate being nullable
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
**		7/25/2019	twilson3			BOEJ-4269 Truncate the temp table in between while loop runs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCodeYear table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCodeYear TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18,6),
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCodeYear
SELECT * FROM @RateCodeYear

DECLARE @ID [int],
		@RateCodeID [int],
		@Year [int],
		@Rate [decimal](18,6),
		@OrderID [int]

DECLARE @InsertedRateCodeYear AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCodeYear WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Year = Year,
		@Rate = Rate,
		@OrderID =  OrderID
	FROM @TT_RateCodeYear
	WHERE ID < 0


	INSERT INTO [dbo].[RateCodeYear]
           ([UpdateDate]
		   ,[RateCodeID]
           ,[Year]
           ,[Rate]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCodeYear
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Year
           ,@Rate
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCodeYear
	
	UPDATE @TT_RateCodeYear
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

	DELETE FROM @InsertedRateCodeYear
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCodeYear TT
		ORDER BY OrderID
GO