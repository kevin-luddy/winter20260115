-- Drop SPs first
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementOrdinaryVariableviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementOrdinaryVariableviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOETaskElementOrdinaryVariableviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOETaskElementOrdinaryVariableviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementOrdinaryVariableviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementOrdinaryVariableviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_OrdinaryVariable' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_OrdinaryVariable];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_OrdinaryVariable] AS TABLE(
	[OrdinaryVariableID] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[OrdinaryVariableName] [varchar](50) NOT NULL,
	[OrdinaryVariableValue] [decimal](29, 10) NULL,
	[BOETaskElementID] [int] NOT NULL,
	[SortByID] [int] NOT NULL,
	[ValueTypeID] [int] NOT NULL,
	[IsPercentage] [bit] NULL,
	[DefaultSize] [varchar](200) NULL,
	[OrderID] [int] NOT NULL
);
GO


-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertBOETaskElementOrdinaryVariableviaTableParameter]
(
@OrdinaryVariable [dbo].[TT_OrdinaryVariable] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElementOrdinaryVariable]
**		Desc: Insert/Update Ordinary Variable data into BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		3/24/11		dcanuso				variable changed to decimal (29,10)
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		8/3/11		dcanuso				Developer Request to Add IsPercentage
**		8/31/11		dcanuso				User is now able to select the Resource
**										Types that will be used for Summing BOEs
**		9/13/11		dcanuso				Moving 8/31 code to new insert/delete SP
**		12/19/13	dcanuso				WI 24983 - @DefaultSize varchar (200) added
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@ErrorMessage varchar (500)
DECLARE @UpdateDT datetime2(7)

		IF EXISTS	(SELECT 1 FROM [dbo].[OrdinaryVariable] O
						INNER JOIN @OrdinaryVariable T ON 
								O.BOETaskElementID = T.BOETaskElementID AND
								O.OrdinaryVariableName = T.OrdinaryVariableName
					)
			BEGIN
				SET @ErrorMessage =   'There is already an Ordinary Variable with one of the Ordinary Variable Names supplied ' 
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END


				SET @UpdateDT = GETDATE()
				
				INSERT INTO [dbo].[OrdinaryVariable]
					   ([OrdinaryVariableName]
					   ,[OrdinaryVariableValue]
					   ,[SortByID]
					   ,[ValueTypeID]
					   ,[BOETaskElementID]
					   ,[IsPercentage]
					   ,[UpdateDT]
					   ,[DefaultSize]
					   )
				 SELECT 
						OrdinaryVariableName
					   ,OrdinaryVariableValue
					   ,SortByID
					   ,ValueTypeID
					   ,BOETaskElementID
					   ,IsPercentage
					   ,@UpdateDT
					   ,DefaultSize
				FROM @OrdinaryVariable

IF @@ERROR = 0
	SELECT	O.OrdinaryVariableID AS OrdinaryVariableID,
			O.UpdateDT
	FROM [dbo].[OrdinaryVariable] O
		INNER JOIN @OrdinaryVariable T ON 
				O.OrdinaryVariableName = T.[OrdinaryVariableName] AND
				O.OrdinaryVariableValue = T.[OrdinaryVariableValue] AND
				O.SortByID = T.[SortByID] AND
				O.ValueTypeID = T.[ValueTypeID] AND
				O.BOETaskElementID = T.[BOETaskElementID] AND
				O.IsPercentage = T.[IsPercentage] AND
				IsNull(O.DefaultSize, -999) = IsNull(T.[DefaultSize], -999)
	WHERE O.UpdateDT = @UpdateDT
	ORDER BY T.OrderID
GO
CREATE PROCEDURE [dbo].[deleteBOETaskElementOrdinaryVariableviaTableParameter]
(
@OrdinaryVariable [dbo].[TT_OrdinaryVariable] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementOrdinaryVariable]
**		Desc: Deletes the BOE Task Element's use of an Ordinary Variable
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		8/31/11		dcanuso				Added Reference tables
*******************************************************************************/
SET NOCOUNT ON 

		
DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
FROM dbo.SumOfBOE_OrdinaryVariableXREF X  
	INNER JOIN @OrdinaryVariable T ON 
		X.OrdinaryVariableID = T.OrdinaryVariableID
 
DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF  
FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X  
	INNER JOIN @OrdinaryVariable T ON 
		X.OrdinaryVariableID = T.OrdinaryVariableID

DELETE FROM dbo.OrdinaryVariable
FROM dbo.OrdinaryVariable X
	INNER JOIN @OrdinaryVariable T ON 
		X.OrdinaryVariableID = T.OrdinaryVariableID AND
		X.UpdateDT = T.UpdateDT
GO
CREATE PROCEDURE [dbo].[updateBOETaskElementOrdinaryVariableviaTableParameter]
(
@OrdinaryVariable [dbo].[TT_OrdinaryVariable] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElementOrdinaryVariable]
**		Desc: Insert/Update Ordinary Variable data into BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		3/24/11		dcanuso				variable changed to decimal (29,10)
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		8/3/11		dcanuso				Developer Request to Add IsPercentage
**		8/31/11		dcanuso				User is now able to select the Resource
**										Types that will be used for Summing BOEs
**		9/13/11		dcanuso				Moving 8/31 code to new insert/delete SP
**		12/19/13	dcanuso				WI 24983 - @DefaultSize varchar (200) added
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDT datetime2(7) = GETDATE()
DECLARE @ErrorMessage varchar (max)

IF EXISTS (
	SELECT 1 
	FROM [dbo].[OrdinaryVariable] OV
		LEFT OUTER JOIN @OrdinaryVariable T ON
			OV.BOETaskElementID = T.BOETaskElementID AND
			OV.OrdinaryVariableName = T.OrdinaryVariableName
	WHERE	
		OV.OrdinaryVariableID <> T.OrdinaryVariableID AND
		OV.OrdinaryVariableID IS NOT NULL AND 
		T.OrdinaryVariableID IS NOT NULL
		)
			BEGIN
				SET @ErrorMessage =   'The Ordinary Variable Name is already in use'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END


UPDATE [dbo].[OrdinaryVariable]
SET 
	[OrdinaryVariableName] = T.OrdinaryVariableName,
	[OrdinaryVariableValue] = T.OrdinaryVariableValue,
	[SortByID] = T.SortByID,
	[ValueTypeID] = T.ValueTypeID,
	[IsPercentage] = T.IsPercentage,
	[UpdateDT] = @UpdateDT,
	[DefaultSize] = T.DefaultSize
FROM [dbo].[OrdinaryVariable] O
	INNER JOIN @OrdinaryVariable T ON
		O.OrdinaryVariableID = T.OrdinaryVariableID AND
		O.UpdateDT = T.UpdateDT
				
IF @@ERROR = 0
	SELECT 
		O.OrdinaryVariableID AS OrdinaryVariableID,
		O.UpdateDT
FROM [dbo].[OrdinaryVariable] O
	INNER JOIN @OrdinaryVariable T ON
		O.OrdinaryVariableID = T.OrdinaryVariableID 
ORDER BY T.OrderID
GO
