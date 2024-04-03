EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.33';
GO

/* Author: Katie Pham (e309214)
Story: SLMX_POLM_PROPH-1166 */

INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-ATLO(1)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-E&T(2)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-Ignite(7)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-Other(4)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-Production(6)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-Quality(5)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Core-Test Labs(3)');
INSERT INTO [dbo].[ResourceClassLU] ([Description]) VALUES ('Enterprise-Labor-Services-Other(4)');

GO