DROP VIEW [dbo].[vwProgramArea];
GO

CREATE VIEW [dbo].[vwProgramArea]
AS
SELECT [ProgramAreaID]
      ,[ProgramAreaName]
  FROM [dbo].[ProgramAreaLU]

GO