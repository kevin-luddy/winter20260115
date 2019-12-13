-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProjectMapSpreadviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProjectMapSpreadviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ProjectMapSpread' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_ProjectMapSpread];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_ProjectMapSpread] AS TABLE(
	[WorkspaceId] [int] NOT NULL,
	[ProjectMapId] [int] NOT NULL,
	[SpreadDate] [date] NOT NULL,
	[SpreadValue] [decimal](18, 6) NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertProjectMapSpreadviaTableParameter]
(
@ProjectMapSpread [dbo].[TT_ProjectMapSpread] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertProjectMapSpreadviaTableParameter]
**		Desc: Insert data into Spread Section of Project Map
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
*******************************************************************************/
SET NOCOUNT ON 

INSERT INTO [dbo].[ProjectMapSpread]
           (
           [WorkspaceId]
		   ,[ProjectMapId]
           ,[SpreadDate]
           ,[SpreadValue]
           )
SELECT 	    T.WorkspaceId,
			T.ProjectMapId,
			T.SpreadDate,
			T.SpreadValue
FROM  @ProjectMapSpread T



IF @@ERROR = 0
BEGIN
	-- These IDs are thrown away
	SELECT OrderID from @ProjectMapSpread
END
GO