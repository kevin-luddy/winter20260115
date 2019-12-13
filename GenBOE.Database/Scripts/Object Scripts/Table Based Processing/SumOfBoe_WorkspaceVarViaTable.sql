-- Drop type
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_SumOfBOE_WorkspaceVariableXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_SumOfBOE_WorkspaceVariableXREF];
GO

/*

THIS IS NO LONGER BEING USED, SO WE WILL NOT RECREATE IT.

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_SumOfBOE_WorkspaceVariableXREF] AS TABLE(
	[WVSumID] [bigint] NOT NULL,
	[WorkspaceVariableID] [int] NOT NULL,
	[CLINID] [int] NULL,
	[WBSID] [int] NULL,
	[BOEID] [int] NULL
);
GO
*/