-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProjectMapviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProjectMapviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ProjectMap' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_ProjectMap];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_ProjectMap] AS TABLE(
	[WorkspaceId] [int] NOT NULL,
	[WbsNumber] varchar(120) NOT NULL,
	[WbsElementTitle] varchar(255) NOT NULL,
	[ActivityID] varchar(100) NOT NULL,
	[ActivityName] varchar(max) NOT NULL,
	[Resource] varchar(20) NOT NULL,
	[CostCenter] varchar(20) NOT NULL,
	[StartDate] date NOT NULL,
	[EndDate] date NOT NULL,
	[CLIN] varchar(120) NOT NULL,
	[Task] varchar(max) NULL,
	[SOW] varchar(255) NULL,
	[SOWTitle] varchar(max) NULL,
	[Rationale] varchar(max) NULL,
	[CamName] varchar(255) NULL,
	[Category] varchar(255) NULL,
	[Hours] decimal(18,6) NULL,
	[Dollars] decimal(18,6) NULL,
	[CanOffload] bit NOT NULL,
	[AddOrDelete] varchar(1) NULL,
	[ClassOfCost] varchar(3) NULL,
	[TieredPercentage] decimal(4,1) NULL,
	[LegacyResourceID] int NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertProjectMapviaTableParameter]
(
@ProjectMap [dbo].[TT_ProjectMap] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertProjectMapviaTableParameter]
**		Desc: Insert data into Project Map
**			
**		
**
**		Auth: twilson3
**		Date: 9/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		4/04/2018	brunworg			BOEJ-3177 Added TieredPercentage column.
**		4/17/18		pattoncr			BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
*******************************************************************************/
SET NOCOUNT ON 

INSERT INTO [dbo].[ProjectMap]
           (
			[WorkspaceId]
			,[WbsNumber]
			,[WbsElementTitle]
			,[ActivityID]
			,[ActivityName]
			,[Resource]
			,[CostCenter] 
			,[StartDate] 
			,[EndDate] 
			,[CLIN] 
			,[Task] 
			,[SOW] 
			,[SOWTitle] 
			,[Rationale]
			,[CamName] 
			,[Category] 
			,[Hours]
			,[Dollars] 
			,[CanOffload]
			,[AddOrDelete] 
			,[ClassOfCost] 
			,[TieredPercentage]
			,[LegacyResourceID]
			,[OrderID]			
           )
SELECT 	    T.WorkspaceId
			,T.[WbsNumber]
			,T.[WbsElementTitle]
			,T.[ActivityID]
			,T.[ActivityName]
			,T.[Resource]
			,T.[CostCenter] 
			,T.[StartDate] 
			,T.[EndDate] 
			,T.[CLIN] 
			,T.[Task] 
			,T.[SOW] 
			,T.[SOWTitle] 
			,T.[Rationale]
			,T.[CamName] 
			,T.[Category] 
			,T.[Hours]
			,T.[Dollars] 
			,T.[CanOffload]
			,T.[AddOrDelete] 
			,T.[ClassOfCost] 
			,T.[TieredPercentage]
			,T.[LegacyResourceID]
			,T.[OrderID]
FROM  @ProjectMap T
ORDER BY T.OrderID

IF @@ERROR = 0
	DECLARE @WorkspaceID int
    Select @WorkspaceID = (Select TOP 1 WorkspaceID FROM @ProjectMap)

	SELECT ID 
	FROM [dbo].[ProjectMap]
	WHERE [WorkspaceId] = @WorkspaceID
	ORDER BY OrderID
GO