IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProgramArea]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProgramArea]
GO

CREATE VIEW [dbo].[vwProgramArea]
AS
SELECT [ProgramAreaID]
      ,[ProgramAreaName]
  FROM [dbo].[ProgramAreaLU]

GO