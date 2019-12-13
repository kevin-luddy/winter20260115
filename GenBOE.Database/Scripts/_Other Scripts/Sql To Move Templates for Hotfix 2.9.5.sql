--NOTE: This script copies templates from ProdClone (ISGS) to Dev (SSC and MST). Database names will need to be updated to copy between Prod databases (Prod ISGS to Prod SSC and MST)

DECLARE @spaceDB VARCHAR(100);
DECLARE @mstDB VARCHAR(100);
DECLARE @sourceIsgsDB VARCHAR(100);

SET @sourceIsgsDB = 'GenBoe';

SET @spaceDB = 'GenBOESpace';
SET @mstDB = 'genBOEMST';

DECLARE @sqlToExecute VARCHAR(8000);
SET @sqlToExecute = '

--ISGS TO SSC
SET IDENTITY_INSERT [' + @spaceDB + '].[dbo].[OutputFormatTemplate] ON;

INSERT INTO [' + @spaceDB + '].[dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID) 
	SELECT TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID 
		FROM [' + @sourceIsgsDB + '].[dbo].[OutputFormatTemplate] as src
		WHERE TemplateID < 9001 AND ParentTemplateID != 9001 
			AND NOT EXISTS 
				(SELECT * 
					FROM [' + @spaceDB + '].[dbo].[OutputFormatTemplate] 
					WHERE src.TemplateID = TemplateID)

INSERT INTO [' + @spaceDB + '].[dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID) 
	SELECT (TemplateID - 100950), UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID 
		FROM [' + @sourceIsgsDB + '].[dbo].[OutputFormatTemplate] as src
		WHERE TemplateID > 9001 AND ParentTemplateID != 9001 
			AND NOT EXISTS 
				(SELECT * 
					FROM [' + @spaceDB + '].[dbo].[OutputFormatTemplate] 
					WHERE src.TemplateID = TemplateID)

SET IDENTITY_INSERT [' + @spaceDB + '].[dbo].[OutputFormatTemplate] OFF;

--ISGS to MST
SET IDENTITY_INSERT [' + @mstDB + '].[dbo].[OutputFormatTemplate] ON;

INSERT INTO [' + @mstDB + '].[dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID) 
	SELECT TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID 
		FROM [' + @sourceIsgsDB + '].[dbo].[OutputFormatTemplate] as src
		WHERE TemplateID < 9001 AND ParentTemplateID != 9001 
			AND NOT EXISTS (
				SELECT * 
				FROM [' + @mstDB + '].[dbo].[OutputFormatTemplate] 
				WHERE src.TemplateID = TemplateID)

INSERT INTO [' + @mstDB + '].[dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID) 
	SELECT (TemplateID - 100950), UpdateDT, Template, TemplateDescription, TemplateFile, IsActive, ParentTemplateID 
		FROM [' + @sourceIsgsDB + '].[dbo].[OutputFormatTemplate] as src
		WHERE TemplateID > 9001 AND ParentTemplateID != 9001 
			AND NOT EXISTS (
			SELECT * 
			FROM [' + @mstDB + '].[dbo].[OutputFormatTemplate] 
			WHERE src.TemplateID = TemplateID)

SET IDENTITY_INSERT [' + @mstDB + '].[dbo].[OutputFormatTemplate] OFF;
';

EXECUTE (@sqlToExecute);