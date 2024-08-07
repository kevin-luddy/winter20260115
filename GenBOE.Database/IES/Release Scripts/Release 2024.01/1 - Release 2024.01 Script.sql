EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.01';


/* Author: Timothy Wilson
Story: SLMX_POLM_PROPH-2219 */

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'DisplayOrder1LMX' AND Object_ID = Object_ID(N'[dbo].[[BurdenElementLU]]'))
BEGIN
    ALTER TABLE [dbo].[BurdenElementLU] ADD [DisplayOrder1LMX] int NOT NULL default(0);
END

GO

IF NOT EXISTS(SELECT * FROM [dbo].[BurdenElementLU] WHERE [BurdenElement] = 'Frg Ent')
BEGIN
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 1 WHERE [BurdenElement] = 'ESCAL';
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'Frg Ent', 'Enterprise Fringe', 2);
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 3 WHERE [BurdenElement] =  'Fringe';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 4 WHERE [BurdenElement] =  'Fringe T2';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 5 WHERE [BurdenElement] =  'FRG Serv Corp';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 6 WHERE [BurdenElement] =  'FRG Serv LMOS';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 7 WHERE [BurdenElement] =  'OH Dev';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 8 WHERE [BurdenElement] =  'OH Prod';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 9 WHERE [BurdenElement] =  'OH FBM';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 10 WHERE [BurdenElement] =  'OH LVS';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 11 WHERE [BurdenElement] =  'OH Hunts';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 12 WHERE [BurdenElement] =  'OH Offsite';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 13 WHERE [BurdenElement] =  'OH Michoud';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 14 WHERE [BurdenElement] =  'OH Mich MH';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 15 WHERE [BurdenElement] =  'OH Prg Uni';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 16 WHERE [BurdenElement] =  'OH Pro DIR';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 17 WHERE [BurdenElement] =  'OH Pro MSC';
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent Eng', 'Enterprise Engineering OH', 18);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent Core Off', 'Enterprise Core Offste OH', 19);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent MHX', 'Enterprise Material Handling OH', 20);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent Mfg Space', 'Enterprise - MFG OH - Space', 21);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent Serv On', 'Enterprise - Services Onsite OH', 22);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'OH Ent Serv Off', 'Enterprise - Services Offsite OH', 23);
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 24 WHERE [BurdenElement] =  'OH Pro Serv';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 25 WHERE [BurdenElement] =  'OH On Serv';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 26 WHERE [BurdenElement] =  'OH Off Serv';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 27 WHERE [BurdenElement] =  'OH PH4';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 28 WHERE [BurdenElement] =  'OH PH5';
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'TCI G&A', 'TCI G&A', 29);
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 30 WHERE [BurdenElement] =  'G&A';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 31 WHERE [BurdenElement] =  'G&A T2';
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'G&A Ent Core', 'Enterprise Core G&A', 32);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'G&A Ent Serv', 'Enterprise Services G&A', 33);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'XC G&A', 'Cross Charge G&A Core', 34);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'XS G&A', 'Cross Charge G&A Services', 35);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM G&A XC', 'Cross Charge Core FCCOM G&A', 36);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM G&A XS', 'Cross Charge Services FCCOM G&A', 37);
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 38 WHERE [BurdenElement] =  'FCCOM Dev';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 39 WHERE [BurdenElement] =  'FCCOM Prod';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 40 WHERE [BurdenElement] =  'FCCOM FBM';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 41 WHERE [BurdenElement] =  'FCCOM LVS';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 42 WHERE [BurdenElement] =  'FCCOM Hunt';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 43 WHERE [BurdenElement] =  'FCCOM Ofst';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 44 WHERE [BurdenElement] =  'FCCOM Mich';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 45 WHERE [BurdenElement] =  'FCM Prg Un';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 46 WHERE [BurdenElement] =  'FCCOM Proc';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 47 WHERE [BurdenElement] =  'FCCOM ProM';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 48 WHERE [BurdenElement] =  'FCCOM G&A';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 49 WHERE [BurdenElement] =  'FCM T2 G&A';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 50 WHERE [BurdenElement] =  'FCCM IWTA';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 51 WHERE [BurdenElement] =  'FCCM PH2';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 52 WHERE [BurdenElement] =  'FCCM PH3';
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 53 WHERE [BurdenElement] =  'FCCM PH4';
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Mtrl', 'Enterprise - Material Handling COM', 54);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent', 'Enterprise - COM', 55);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Core G&A', 'Enterprise - Core G&A COM', 56);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Mfg Space', 'Enterprise - Manufacturing - Space COM', 57);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Eng', 'Enterprise - Engineering COM', 58);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Core Off', 'Enterprise - Core Offsite COM', 59);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Serv G&A', 'Enterprise - Services G&A COM', 60);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Serv On', 'Enterprise - Services Onsite COM', 61);
    INSERT INTO [dbo].[BurdenElementLU] ([DisplayOrder], [BurdenElement], [Description], [DisplayOrder1LMX]) VALUES ( 0, 'FCCOM Ent Serv Off', 'Enterprise - Services Offsite COM', 62);   
    UPDATE [dbo].[BurdenElementLU] SET [DisplayOrder1LMX] = 63 WHERE [BurdenElement] =  'Fee/Prft';

END
GO