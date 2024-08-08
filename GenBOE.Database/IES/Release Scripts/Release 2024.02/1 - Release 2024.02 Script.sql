EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.02';


/* Author: Timothy Wilson
Story: SLMX_POLM_PROPH-2241 */

SET IDENTITY_INSERT [dbo].[CobraCode1LU] ON; 
GO

IF NOT EXISTS(SELECT * FROM [dbo].[CobraCode1LU] WHERE [Description] = 'DIRECT')
BEGIN
    INSERT INTO [dbo].[CobraCode1LU] ([ID], [Description]) VALUES ( 3, 'DIRECT');
END
GO

SET IDENTITY_INSERT [dbo].[CobraCode1LU] OFF; 
GO