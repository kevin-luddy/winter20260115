-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].updateRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].deleteRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCode' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCode];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCode] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode Table
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		2/21/2018	brunworg			Removed CobraRateSet and CobraCode1ID.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2 = GETDATE()

SET @UpdateDate = GetDate()
			
			 
UPDATE [dbo].[RateCode]
	SET 
		[UpdateDate] = TT.[UpdateDate],
		[RevisionID] = TT.[RevisionID],
		[CategoryID] = TT.[CategoryID],
		[Description] = TT.[Description],
		[SectionID] = TT.[SectionID],
		[RateCode] = TT.[RateCode],
		[ResourceTypeID] = TT.[ResourceTypeID],
		[GovernmentBurdenPoolID] = TT.[GovernmentBurdenPoolID],
		[CommercialBurdenPoolID] = TT.[CommercialBurdenPoolID],
		[RateTypeID] = TT.[RateTypeID]
FROM [dbo].[RateCode] RC
	INNER JOIN @RateCodeParam TT ON 
		RC.ID = TT.ID 
				
IF @@ERROR = 0
	SELECT 
		RC.ID AS ID, 
		RC.UpdateDate AS UpdateDate 
	FROM @RateCodeParam TT
		INNER JOIN dbo.RateCode RC ON TT.ID = RC.ID
	ORDER BY OrderID
GO
CREATE PROCEDURE [dbo].[deleteRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteRateCodeviaTableParameter]
**		Desc: Delete data from RateCode Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		11/9/2017	ranzalon			Remove ProPricer Burden Rate mappings
*******************************************************************************/
SET NOCOUNT ON 

			DELETE FROM dbo.RateCodeYear
				FROM dbo.RateCodeYear RCY
				INNER JOIN dbo.RateCode RC ON RCY.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerRateCodeXref
				FROM dbo.ProPricerRateCodeXref PPX
				INNER JOIN dbo.RateCode RC ON PPX.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerBurdenRateMap
				FROM dbo.ProPricerBurdenRateMap BRM
				INNER JOIN dbo.RateCode RC ON BRM.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON
					RC.ID = TT.ID
											
			DELETE FROM [dbo].[RateCode]
			FROM [dbo].[RateCode] RC
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The RateCode Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO
CREATE PROCEDURE [dbo].[insertRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode table.
**			
**		
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
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCode table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCode TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCode
SELECT * FROM @RateCodeParam

DECLARE @ID [int],
		@RevisionID [int],
		@CategoryID [int],
		@Description [varchar](4000),
		@SectionID [int],
		@RateCode [varchar](50),
		@ResourceTypeID [int],
		@GovernmentBurdenPoolID [int],
		@CommercialBurdenPoolID [int],
		@RateTypeID [int],
		@OrderID [int]

DECLARE @InsertedRateCode AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCode WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RevisionID = RevisionID,
		@CategoryID = CategoryID,
		@Description = Description,
		@SectionID = SectionID,
		@RateCode = RateCode,
		@ResourceTypeID = ResourceTypeID,
		@GovernmentBurdenPoolID = GovernmentBurdenPoolID,
		@CommercialBurdenPoolID = CommercialBurdenPoolID,
		@RateTypeID = RateTypeID,
		@OrderID =  OrderID
	FROM @TT_RateCode
	WHERE ID < 0

	INSERT INTO [dbo].[RateCode]
           ([UpdateDate]
		   ,[RevisionID]
           ,[CategoryID]
           ,[Description]
           ,[SectionID]
           ,[RateCode]
           ,[ResourceTypeID]
           ,[GovernmentBurdenPoolID]
           ,[CommercialBurdenPoolID]
           ,[RateTypeID]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCode
     VALUES
           (@UpdateDate
		   ,@RevisionID
           ,@CategoryID
           ,@Description
           ,@SectionID
           ,@RateCode
           ,@ResourceTypeID
           ,@GovernmentBurdenPoolID
           ,@CommercialBurdenPoolID
           ,@RateTypeID
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCode
	
	UPDATE @TT_RateCode
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCode TT
		ORDER BY OrderID
GO