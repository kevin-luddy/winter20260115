EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.44';


/* Author: Timothy Wilson
Story: SLMX_POLM_PROPH-1420 */

UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-ATC-California-Ignite(7)' WHERE [Description] = 'Labor-Core-ATC-California-Ignite(8)';
UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-ATC-Ignite(7)' WHERE [Description] = 'Labor-Core-ATC-Ignite(8)';
UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-California-Ignite(7)' WHERE [Description] = 'Labor-Core-California-Ignite(8)';
UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-Ignite(7)' WHERE [Description] = 'Labor-Core-Ignite(8)';
UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-Mission_Solutions-California-Ignite(7)' WHERE [Description] = 'Labor-Core-Mission_Solutions-California-Ignite(8)';
UPDATE [dbo].[ResourceClassLU] SET [Description] = 'Labor-Core-Mission_Solutions-Ignite(7)' WHERE [Description] = 'Labor-Core-Mission_Solutions-Ignite(8)';

GO