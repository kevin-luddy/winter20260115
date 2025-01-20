EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.03';
GO

/*** ALTER TABLE - Workspace (dbo) ***/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Workspace]') AND type in (N'U'))
BEGIN
	ALTER TABLE [dbo].[Workspace]
	ADD EnableAssignTaskAuthor [bit]
	DEFAULT (0)
END
GO




/*** ALTER TABLE - Workspace (version) ***/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[Workspace]') AND type in (N'U'))
BEGIN
	ALTER TABLE [version].[Workspace]
	ADD EnableAssignTaskAuthor [bit]
	DEFAULT (0)
END
GO




/*** ALTER TABLE - BOETaskElement (dbo) ***/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [dbo].[BOETaskElement]
	ADD AuthorUserId [int]
	DEFAULT NULL
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [dbo].[BOETaskElement]
	WITH CHECK ADD CONSTRAINT [FK_AuthorUserId] FOREIGN KEY([AuthorUserId])
	REFERENCES [dbo].[ETIuser] ([ETIUserID])
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [dbo].[BOETaskElement] CHECK CONSTRAINT [FK_AuthorUserId]
END
GO



/*** ALTER TABLE - BOETaskElement (version) ***/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [version].[BOETaskElement]
	ADD AuthorUserId [int]
	DEFAULT NULL
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [version].[BOETaskElement]
	WITH CHECK ADD CONSTRAINT [FK_AuthorUserId] FOREIGN KEY([AuthorUserId])
	REFERENCES [dbo].[ETIuser] ([ETIUserID])
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[BOETaskElement]') AND type in (N'U'))
BEGIN
	ALTER TABLE [version].BOETaskElement CHECK CONSTRAINT [FK_AuthorUserId]
END
GO



/*** TODO: Add copy of stored procedure updates into release file??? ***/