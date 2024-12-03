PRINT '###### SCRIPT IS STARTING ######';
/*
    This file was auto-generated for Release: 2018.1, on 1/30/2018.
    It contains all of the Release specific scripts, modifying data/tables as well as all of the Stored Procedures and User Defined Table Types.
*/

/*
    File: \Release 2018.1\0 - Gregs Long Script.sql
*/
PRINT '### Starting file: \Release 2018.1\0 - Gregs Long Script.sql';
/*
       ## START ##
       12/22/17 [brunworg] - BOEJ-2885 Add an extra field of "Resource Class" into ProPricer Direct Rate Mappings
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResourceClassLU]') AND type in (N'U'))
BEGIN
       CREATE TABLE [dbo].[ResourceClassLU](
              [ID] [int] IDENTITY(1,1) NOT NULL,
              [Description] [varchar](4000) NULL,
       CONSTRAINT [PK_ResourceClassLU] PRIMARY KEY CLUSTERED 
       (
              [ID] ASC
       )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
       ) ON [PRIMARY];

       SET IDENTITY_INSERT [dbo].[ResourceClassLU] ON; 
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(1, 'Actuals-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(2, 'Actuals-Services');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(3, 'IWTA-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(4, 'IWTA-Services');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(5, 'Labor-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(6, 'Labor-Core-ATC-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(7, 'Labor-Core-ATC-California');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(8, 'Labor-Core-ATC-California-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(9, 'Labor-Core-ATC-California-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(10, 'Labor-Core-ATC-California-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(11, 'Labor-Core-ATC-California-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(12, 'Labor-Core-ATC-California-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(13, 'Labor-Core-ATC-California-Touch(6)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(14, 'Labor-Core-ATC-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(15, 'Labor-Core-ATC-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(16, 'Labor-Core-ATC-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(17, 'Labor-Core-ATC-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(18, 'Labor-Core-ATC-Touch(6)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(19, 'Labor-Core-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(20, 'Labor-Core-California');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(21, 'Labor-Core-California-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(22, 'Labor-Core-California-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(23, 'Labor-Core-California-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(24, 'Labor-Core-California-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(25, 'Labor-Core-California-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(26, 'Labor-Core-California-Touch(6)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(27, 'Labor-Core-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(28, 'Labor-Core-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(29, 'Labor-Core-Mission_Solutions');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(30, 'Labor-Core-Mission_Solutions-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(31, 'Labor-Core-Mission_Solutions-California');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(32, 'Labor-Core-Mission_Solutions-California-ATLO(1)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(33, 'Labor-Core-Mission_Solutions-California-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(34, 'Labor-Core-Mission_Solutions-California-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(35, 'Labor-Core-Mission_Solutions-California-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(36, 'Labor-Core-Mission_Solutions-California-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(37, 'Labor-Core-Mission_Solutions-Engineering(2)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(38, 'Labor-Core-Mission_Solutions-Labs(3)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(39, 'Labor-Core-Mission_Solutions-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(40, 'Labor-Core-Mission_Solutions-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(41, 'Labor-Core-Mission_Solutions-Touch(6)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(42, 'Labor-Core-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(43, 'Labor-Core-Quality(5)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(44, 'Labor-Core-Touch(6)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(45, 'Labor-Services-Other(4)');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(46, 'Material-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(47, 'Material-Services');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(48, 'Other-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(49, 'Other-Services');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(50, 'Service_Center-Labor');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(51, 'Service_Center-Non-Labor');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(52, 'Subcontractor-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(53, 'Subcontractor-Services');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(54, 'Travel-Core');
       INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(55, 'Travel-Services');
	   INSERT INTO [dbo].[ResourceClassLU] (ID, Description) VALUES(56, 'Labor-Core-BUSCOE(7)');
       SET IDENTITY_INSERT [dbo].[ResourceClassLU] OFF; 
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ResourceClassID' AND Object_ID = Object_ID(N'[dbo].[ProPricerRateCodeXref]'))
BEGIN
       ALTER TABLE [dbo].[ProPricerRateCodeXref] ADD [ResourceClassID] int NULL;
       ALTER TABLE [dbo].[ProPricerRateCodeXref] WITH CHECK ADD CONSTRAINT [FK_ProPricerRateCodeXref_ResourceClassLU] FOREIGN KEY ([ResourceClassID]) REFERENCES [dbo].[ResourceClassLU]([ID]);

       Delete dbo.TempRates;

       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('538003LB',50),'Civil Space Planetary Explor Sys - Lbr','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('538003NL',50),'Civil Space Planetary Explor Sys - NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541110NL',50),'California Earthquake Property Insurance','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541150LB',50),'Engineering Management Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541150NL',50),'Engineering Management Service Center NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541155LB',50),'Test Labs Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541155NL',50),'Test Labs Service Center NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541165NL',50),'Engineering Computing Service Center NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541175LB',50),'Production Comm Process Svc Ctr Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541175NL',50),'Production Comm Process Svc Ctr Non Labo','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541180LB',50),'Quality Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541180NL',50),'Quality Service Center NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541182LB',50),'Quality Assurance Remote Svc Ctr','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541182NL',50),'Quality Assurance Remote Svc Ctr ODC','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183LB',50),'Metrology Service Center Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183LB-C',50),'Metrology Service Center Labor-CORE','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183LB-S',50),'Metrology Service Center Labor-Services','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183NL',50),'Metrology Service Center Non Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183NL-C',50),'Metrology Service Center Non Labor-CORE','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541183NL-S',50),'Metrology Service Center Non Labor-Services','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541185LB',50),'ATLO Svc Ctr Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541185NL',50),'ATLO Svc Ctr Non Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541190LB',50),'Aerojet CSSM Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541190NL',50),'Aerojet CSSM Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541191LB',50),'ATK CSSM Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541191NL',50),'ATK Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541192LB',50),'BAE CSSM Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541192NL',50),'BAE CSSM Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541193LB',50),'Honeywell CSSM Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541193NL',50),'Honeywell CSSM Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541194LB',50),'ITT CSSM Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541194NL',50),'ITT CSSM Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541196LB',50),'Critical Supplier Mgt (CSSM )Svc Ctr LBR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541196NL',50),'Critical Supplier Mgt (CSSM )Svc Ctr NL','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541199LB',50),'Procurement Management Service Center','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541199NL',50),'Procurement Management Service Center NL','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541262LB',50),'FBM Common Manufacturing Services','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541262NL',50),'FBM Common Manufacturing Services - Non-','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541311LB',50),'Advanced Technology Center (ATC) Busines','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541311NL',50),'Advanced Technology Center (ATC) Busines','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541501LB',50),'Reproduction Service Center LB','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541501NL',50),'Reproduction Service Center NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541710LB',50),'Military Space - Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541710NL',50),'Military Space - Non-Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541720LB',50),'Civil Space - Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541720NL',50),'Civil Space - Non-Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541730LB',50),'Commercial Ventures - Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541730NL',50),'Commercial Ventures - Non-Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541740LB',50),'Comm Ventures Wind Energy Sys- Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541740NL',50),'Comm Ventures Wind Energy Sys- NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541750LB',50),'Advanced Technology Center MSC (ATC) LBR','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541750NL',50),'Advanced Technology Center MSC (ATC) NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541751LB',50),'Advanced Technology Center Bus Ops LBR','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541751NL',50),'Advanced Technology Center  Bus Ops NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541760LB',50),'Commercial Civil Space Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541760NL',50),'Commercial Civil Space Service Center','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541905LB',50),'Thermal Protection Products Service Cent','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541905NL',50),'Thermal Protection Products Service Cent','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541910LB',50),'Orion Facilities Service Center - Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541910NL',50),'Orion Facilities Service Center - Non-La','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541920LB',50),'Michoud Production Support Service Cente','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541920NL',50),'Michoud Production Support Service Cente','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541921LB',50),'Civil Space - ULA Contracts - Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541921NL',50),'Civil Space  - ULA Contracts - Non-Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541940CLB',50),'Mission Solutions MSC Core LBR','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541940CNL',50),'Mission Solutions MSC Core NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541940SLB',50),'Mission Solutions MSC Services LBR','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541940SNL',50),'Mission Solutions MSC Services NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541951LB',50),'Global Communications Mgmt Service Cente','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541951NL',50),'Global Communications Mgmt Service Cente','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541953LB',50),'Strategic Missile Programs MSC','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541953NL',50),'Strategic Missile Programs MSC NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541954LB',50),'THAAD Program Management Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541954NL',50),'THAAD Program Management Service Center','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541955LB',50),'Sensing & Explorations West Coast Servic','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541955NL',50),'Sensing & Explorations West Coast Sevice','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541958LB',50),'Commercial Space Systems Management Serv','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541958NL',50),'Commerical Space Systems Management Serv','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541959LB',50),'Missile Defense Systems - Huntsville MSC','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541959NL',50),'Missile Defense Systems - Huntsville MSC','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541961LB',50),'Advance Technology Center (ATC) MSC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541961NL',50),'Advance Technology Center (ATC) MSC ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541962LB',50),'Missile Defense Systems Management Servi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541962NL',50),'Missile Defense Systems Management Servi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541963LB',50),'Special Programs Management Service Cent','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541963NL',50),'Special Programs Management Service Cent','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541964LB',50),'Sensing & Exploration Systems Management','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541964NL',50),'Sensing & Explorations Systems Mgmt Serv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541970LB',50),'Surv & Nav Sys MSC-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541970NL',50),'Surv & Nav Sys MSC-NonLabor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541971LB',50),'Surveillance & Intelligence Systems S&IS','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541971NL',50),'Surveillance & Intelligence Systems S&IS','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541972LB',50),'Strategic Missile Defense LOB MSC','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541972NL',50),'Strategic Missile Defense LOB MSC NL','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541975LB',50),'Missile Defense Programs Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541975NL',50),'Missile Defense Programs Service Center','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541976LB',50),'Bus Center of Excellence Service Center','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541976NL',50),'Bus Center of Excellence Service Center','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541980LB',50),'Human Space Flight Service Center - Labo','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541980NL',50),'Human Space Flight Service Center - Non-','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541986LB',50),'Michoud Operations Management Service Ce','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541986NL',50),'Michoud Operations Management Service Ce','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541987LB',50),'S&MDS Adv Programs Svc Ctr Labor','Service_Center-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541987NL',50),'S&MDS Adv Programs Svc Ctr Non Labor','Service_Center-Non-Labor');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541990LB',50),'Coherent Technologies Service Center - L','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('541990NL',50),'Coherent Technologies Service Center - N','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('5422VANL',50),'IRD VAX SCSC ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ABT$',50),'Abatement','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ABTSERV$',50),'Services Abatement','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTDEV',50),'SSC Development Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTDEV-$',50),'SSC Development Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTDEV*',50),'SSC Development Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTDEV-LB',50),'SSC Development Actuals LB Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTDEV-LB*',50),'SSC Development Actuals LB Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFBM',50),'SSC FBM Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFBM-$',50),'SSC FBM Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFBM*',50),'SSC FBM Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFBM-LB',50),'SSC FBM Actuals Lb Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFBM-LB*',50),'SSC FBM Actuals Lb Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFISAC',50),'SSC FISAC Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFISAC-$',50),'SSC FISAC Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFISAC*',50),'SSC FISAC Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFISAC-LB',50),'SSC FISAC Actuals Lb Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTFISAC-LB*',50),'SSC FISAC Actuals Lb Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTHNT',50),'SSC Huntsville Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTHNT-$',50),'Actual Huntsville labor Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTHNT*',50),'SSC Huntsville Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTHNT-LB',50),'SSC Huntsville Actuals LB Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTHNT-LB*',50),'SSC Huntsville Actuals LB Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTIWTA',50),'Actual IWTA Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTIWTA-$',50),'Actual IWTA Cost','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTIWTA-$*',50),'Actual IWTA Cost - Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJDEV',50),'SSC Jobshopper Development Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJDEV-$',50),'SSC Jobshopper Development Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJDEV*',50),'SSC Jobshopper Dev Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJFBM',50),'SSC Jobshopper FBM Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJFBM-$',50),'SSC Jobshopper FBM Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJFBM*',50),'SSC Jobshopper FBM Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJHNT',50),'SSC Jobshopper Huntsville  Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJHNT-$',50),'SSC Jobshopper Huntsville  Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJHNT*',50),'SSC Jobshopper HSV  Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJLVS',50),'SSC Jobshopper LVS Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJLVS-$',50),'SSC Jobshopper LVS Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJLVS*',50),'SSC Jobshopper LVS Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJMIC',50),'SSC Jobshopper Michoud Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJMIC-$',50),'SSC Jobshopper Michoud Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJMIC*',50),'SSC Jobshopper Michoud Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJPRD',50),'SSC Jobshopper Production Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJPRD-$',50),'SSC Jobshopper Production Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJPRD*',50),'SSC Jobshopper Prod Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJRMT',50),'SSC Jobshopper Remote Actual Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJRMT-$',50),'SSC Jobshopper Remote Actual $','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJRMT*',50),'SSC Jobshopper Remote Actual Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVOFF',50),'Services Job Shopper Offsite Actual Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVOFF-$',50),'Services Job Shopper Offsite Actual $','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVOFF*',50),'Services Jobshopper Offsite Actual Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVON',50),'Services Job Shopper Onsite Actual Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVON-$',50),'Services Job Shopper Onsite Actual $','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTJSERVON*',50),'Services Jobshopper Onsite Actual Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTLVS',50),'SSC Launch Vehicle Services Actuals Hour','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTLVS-$',50),'SSC Launch Vehicle Services Acts Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTLVS*',50),'SSC Launch Vehicle Svs Actuals Hour-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTLVS-LB',50),'SSC Launch Vehicle Services Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTLVS-LB*',50),'SSC Launch Vehicle Svs Svc Ctr- Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTMIC',50),'SSC Michoud Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTMIC-$',50),'SSC Michoud Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTMIC*',50),'SSC Michoud Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTMIC-LB',50),'SSC Michoud Actuals LB Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTMIC-LB*',50),'SSC Michoud Actuals LB Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTNLA-$',50),'SSC Non Labor Service Center Allocation','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTNLA-$*',50),'SSC Non Labor Service Center Alloc-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTODC-$',50),'SSC Actual Other Direct Cost','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTODC-$*',50),'SSC Actual Other Direct Cost-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTPRD',50),'SSC Production Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTPRD-$',50),'SSC Production Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTPRD*',50),'SSC Production Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTPRD-LB',50),'SSC Production Actuals LB Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTPRD-LB*',50),'SSC Production Actuals LB Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTRMT',50),'SSC Remote Actuals Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTRMT-$',50),'SSC Remote Actuals Dollars','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTRMT*',50),'SSC Remote Actuals Hours-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTRMT-LB',50),'SSC Remote Actuals LB Svc Ctr','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTRMT-LB*',50),'SSC Remote Actuals LB Svc Ctr-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1OFF',50),'Services T1 Offsite Actuals Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1OFF-$',50),'Services T1 Offsite Actuals Dollars','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1OFF*',50),'Services T1 Offsite Actuals Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1OFF-LB',50),'Services T1 Offsite Actuals LB Svc Ctr','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1OFF-LB*',50),'Services T1 Offsite Actuals LB Svc Ctr-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1ON',50),'Services T1 Onsite Actuals Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1ON-$',50),'Services T1 Onsite Actuals Dollars','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1ON*',50),'Services T1 Onsite Actuals Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1ON-LB',50),'Services T1 Onsite Actuals LB Svc Ctr','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV1ON-LB*',50),'Services T1 Onsite Actuals LB Svc Ctr-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2OFF',50),'Services T2 Offsite Actuals Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2OFF-$',50),'Services T2 Offsite Actuals Dollars','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2OFF*',50),'Services T2 Offsite Actuals Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2OFF-LB',50),'Services T2 Offsite Actuals LB Svc Ctr','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2OFF-LB*',50),'Services T2 Offsite Actuals LB Svc Ctr-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2ON',50),'Services T2 Onsite Actuals Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2ON-$',50),'Services T2 Onsite Actuals Dollars','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2ON*',50),'Services T2 Onsite Actuals Hours-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2ON-LB',50),'Services T2 Onsite Actuals LB Svc Ctr','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERV2ON-LB*',50),'Services T2 Onsite Actuals LB Svc Ctr-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVIWTA',50),'Actual Services IWTA Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVIWTA-$',50),'Actual Services IWTA','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVIWTA-$*',50),'Actual Services IWTA - Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVNLA-$',50),'Services SSC Non Labor Service Center Allocation','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVNLA-$*',50),'Services SSC Non Labor Service Center Alloc-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVODC-$',50),'Services SSC Actual Other Direct Cost','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVODC-$*',50),'Services SSC Actual Other Direct Cost-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVSUB',50),'Actual Services Sub Hours','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVSUB-$',50),'Actual Services Sub Cost','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVSUB-$*',50),'Actual Services Sub Cost - Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVTRV',50),'Services SSC Travel Actuals','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSERVTRV*',50),'Services SSC Travel Actuals-Rate','Actuals-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSUB',50),'Actual Sub Hours','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSUB-$',50),'Actual Sub Cost','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTSUB-$*',50),'Actual Sub Cost - Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTTRV',50),'SSC Travel Actuals','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ACTTRV*',50),'SSC Travel Actuals-Rate','Actuals-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAA',50),'Adv Tech Ctr Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAB',50),'Adv Tech Ctr Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAC',50),'Adv Tech Ctr Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAD',50),'Adv Tech Ctr Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAE',50),'Adv Tech Ctr Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDAX',50),'Adv Tech Ctr Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVA',50),'ATC Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVB',50),'ATC Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVC',50),'ATC Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVD',50),'ATC Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVE',50),'ATC Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDEVX',50),'ATC Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDLB',50),'Advanced Technology Ctr Denver Dev Svc C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDNL',50),'Advanced Technology Ctr Denver Dev Svc C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATDDPA',50),'Adv Tech Ctr Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAA',50),'ATC Nantero Dev Hourly & NES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAB',50),'ATCNantero Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAC',50),'ATC Nantero Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAD',50),'ATC Nantero Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAE',50),'ATC Nantero Dev Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDAX',50),'ATC Nantero Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVA',50),'ATC Nano Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVB',50),'ATC Nano Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVC',50),'ATC Nano Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVD',50),'ATC Nano Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVE',50),'ATC Nano Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDEVX',50),'ATC Nano Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATJDPA',50),'ATC Nantero Dev Hourly & NES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFA',50),'ATC Offsite Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFB',50),'ATC Offsite Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFC',50),'ATC Offsite Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFD',50),'ATC Offsite Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFE',50),'ATC Offsite Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATROFFX',50),'ATC Offsite Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAA',50),'Adv Tech Ctr Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAB',50),'Adv Tech Ctr Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAC',50),'Adv Tech Ctr Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAD',50),'Adv Tech Ctr Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAE',50),'Adv Tech Ctr Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRAX',50),'Adv Tech Ctr Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATRRPA',50),'Adv Tech Ctr Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAA',50),'Adv Tech Ctr Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAB',50),'Adv Tech Ctr Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAC',50),'Adv Tech Ctr Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAD',50),'Adv Tech Ctr Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAE',50),'Adv Tech Ctr Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAJ',50),'Adv Tech Ctr Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAK',50),'Adv Tech Ctr Snvle Dev Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAL',50),'Adv Tech Ctr Snvle Dev Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAM',50),'Adv Tech Ctr Snvle Dev Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAN',50),'Adv Tech Ctr Snvle Dev Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDAX',50),'Adv Tech Ctr Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVA',50),'ATC SV Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVB',50),'ATC SV Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVC',50),'ATC SV Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVD',50),'ATC SV Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVE',50),'ATC SV Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDEVX',50),'ATC SV Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDLB',50),'Advanced Technology Ctr Sunnyvale Dev Sv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDNL',50),'Advanced Technology Ctr Sunnyvale Dev Sv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSDPA',50),'Adv Tech Ctr Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAA',50),'Adv Tech Ctr Snyvle Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAB',50),'Adv Tech Ctr Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAC',50),'Adv Tech Ctr Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAD',50),'Adv Tech Ctr Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAE',50),'Adv Tech Ctr Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ATSPAX',50),'Adv Tech Ctr Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Award Fee',50),'Award Fee','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('AWARD FEE SERV',50),'Services Award Fee','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLDC4',50),'SBIRS GEO56 Bus Ops Factor Denver','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLSA4',50),'SBIRS GEO56 Bus Ops Factor S’vale','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLSB4',50),'GEO56 Bus Ops Factor S’vale','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLSC4',50),'GEO56 Bus Ops Factor S’vale','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLSD4',50),'SBIRS GEO56 Bus Ops Factor S’vale','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BOPLSE2',50),'SBIRS GEO56 Bus Ops Factor S’vale','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CM_DM HFDD',50),'Bus Ops CER - Config & Data Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CM_DM HFLM',50),'Bus Ops CER - Config & Data Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CM_DM HFRR',50),'Bus Ops CER - Config & Data Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CNTRCT HFDD',50),'Bus Ops CER - Contracts Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CNTRCT HFLM',50),'Bus Ops CER - Contracts Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CNTRCT HFRR',50),'Bus Ops CER - Contracts Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CST MGT HFDD',50),'Bus Ops CER - Cost Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CST MGT HFLM',50),'Bus Ops CER - Cost Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS CST MGT HFRR',50),'Bus Ops CER - Cost Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS EST HFDD',50),'Bus Ops CER - Estimating Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS EST HFLM',50),'Bus Ops CER - Estimating Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS EST HFRR',50),'Bus Ops CER - Estimating Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Bus Ops Factor 1',50),'Bus Ops Factor 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Bus Ops Factor 2',50),'Bus Ops Factor 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS HR MGT HFDD',50),'Bus Ops CER - Human Res Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS HR MGT HFLM',50),'Bus Ops CER - Hunam Res Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS HR MGT HFRR',50),'Bus Ops CER - Human Res Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS IT MGT HFDD',50),'Bus Ops CER - IT Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS IT MGT HFLM',50),'Bus Ops CER - IT Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS IT MGT HFRR',50),'Bus Ops CER - IT MGMT HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS LC COST HFDD',50),'Bus Ops CER - Life Cycle Cost HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS LC COST HFLM',50),'Bus Ops CER - Life Cycle Cost HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS LC COST HFRR',50),'Bus Ops CER - Life Cycle Cost HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG1 HFDD',50),'Bus Ops CER - Planning Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG1 HFLM',50),'Bus Ops CER - Planning Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG1 HFRR',50),'Bus Ops CER - Planning Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG2 HFDD',50),'Bus Ops CER - Planning Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG2 HFLM',50),'Bus Ops CER - Planning Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PLNG2 HFRR',50),'Bus Ops CER - Planning Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PRP MGT HFDD',50),'Bus Ops CER - Property Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PRP MGT HFLM',50),'Bus Ops CER - Property Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS PRP MGT HFRR',50),'Bus Ops CER - Property Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS RSK MGT HFDD',50),'Bus Ops CER - Risk Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS RSK MGT HFLM',50),'Bus Ops CER - Risk Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS RSK MGT HFRR',50),'Bus Ops CER - Risk Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS SC MGT HFDD',50),'Bus Ops CER - Subcontract Mgmt HFDD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS SC MGT HFLM',50),'Bus Ops CER - Subcontract Mgmt HFLM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('BUS OPS SC MGT HFRR',50),'Bus Ops CER - subcontract Mgmt HFRR','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA',50),'Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA1',50),'ATLO Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA2',50),'ENG Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA3',50),'LABS Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA4',50),'OTHER Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA5',50),'QUAL Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAA6',50),'Touch Core MTN_South Dev HRLY NES ST','Labor-Core-Mission_Solutions-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB',50),'Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB1',50),'ATLO Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB2',50),'ENG Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB3',50),'LABS Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB4',50),'OTHER Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAB5',50),'QUAL Core MTN_South Dev Lvl 1&2','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC',50),'Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC1',50),'ATLO Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC2',50),'ENG Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC3',50),'LABS Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC4',50),'OTHER Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAC5',50),'QUAL Core MTN_South Dev Lvl 3&4','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD',50),'Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD1',50),'ATLO Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD2',50),'ENG Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD3',50),'LABS Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD4',50),'OTHER Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAD5',50),'QUAL Core MTN_South Dev Lvl 5','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE',50),'Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE1',50),'ATLO Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE2',50),'ENG Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE3',50),'LABS Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE4',50),'OTHER Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAE5',50),'QUAL Core MTN_South Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX',50),'Core MTN_South Dev Composite','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX1',50),'ATLO Core MTN_South Dev Composite','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX2',50),'ENG Core MTN_South Dev Composite','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX3',50),'LABS Core MTN_South Dev Composite','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX4',50),'OTHER Core MTN_South Dev Composite','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDAX5',50),'QUAL Core MTN_South Dev Composite','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA',50),'Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA1',50),'ATLO Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA2',50),'ENG Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA3',50),'LABS Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA4',50),'OTHER Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MDPA5',50),'QUAL Core MTN_South Dev HRLY NES OT','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAA4',50),'Core MTN_South Remote HRLY NES ST','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAB4',50),'Core MTN_South Remote Lvl 1&2','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAC4',50),'Core MTN_South Remote Lvl 3&4','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAD4',50),'Core MTN_South Remote Lvl 5','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAE4',50),'Core MTN_South Remote Lvl 6&Up','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRAX4',50),'Core MTN_South Remote Composite','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1MRPA4',50),'Core MTN_South Remote HRLY NES OT','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA',50),'Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA1',50),'ATLO Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA2',50),'ENG Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA3',50),'LABS Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA4',50),'OTHER Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAA5',50),'QUAL Core NorthEast Dev HRLY NES ST','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB',50),'Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB1',50),'ATLO Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB2',50),'ENG Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB3',50),'LABS Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB4',50),'OTHER Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAB5',50),'QUAL Core NorthEast Dev Lvl 1&2','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC',50),'Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC1',50),'ATLO Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC2',50),'ENG Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC3',50),'LABS Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC4',50),'OTHER Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAC5',50),'QUAL Core NorthEast Dev Lvl 3&4','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD',50),'Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD1',50),'ATLO Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD2',50),'ENG Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD3',50),'LABS Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD4',50),'OTHER Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAD5',50),'QUAL Core NorthEast Dev Lvl 5','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE',50),'Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE1',50),'ATLO Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE2',50),'ENG Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE3',50),'LABS Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE4',50),'OTHER Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAE5',50),'QUAL Core NorthEast Dev Lvl 6&Up','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX',50),'Core NorthEast Dev Composite','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX1',50),'ATLO Core NorthEast Dev Composite','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX2',50),'ENG Core NorthEast Dev Composite','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX3',50),'LABS Core NorthEast Dev Composite','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX4',50),'OTHER Core NorthEast Dev Composite','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDAX5',50),'QUAL Core NorthEast Dev  Composite','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA',50),'Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA1',50),'ATLO Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA2',50),'ENG Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA3',50),'LABS Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA4',50),'OTHER Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NDPA5',50),'QUAL Core NorthEast Dev HRLY NES OT','Labor-Core-Mission_Solutions-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAA4',50),'Core NorthEast Remote HRLY NES ST','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAB4',50),'Core NorthEast Remote Lvl 1&2','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAC4',50),'Core NorthEast Remote Lvl 3&4','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAD4',50),'Core NorthEast Remote Lvl 5','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAE4',50),'Core NorthEast Remote Lvl 6&Up','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRAX4',50),'Core NorthEast Remote Composite','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1NRPA4',50),'Core NorthEast Remote HRLY NES OT','Labor-Core-Mission_Solutions-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA',50),'Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA1',50),'ATLO Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA2',50),'ENG Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA3',50),'LABS Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA4',50),'OTHER Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAA5',50),'QUAL Core West Dev HRLY NES ST','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB',50),'Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB1',50),'ATLO Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB2',50),'ENG Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB3',50),'LABS Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB4',50),'OTHER Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAB5',50),'QUAL Core West Dev Lvl 1&2','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC',50),'Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC1',50),'ATLO Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC2',50),'ENG Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC3',50),'LABS Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC4',50),'OTHER Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAC5',50),'QUAL Core West Dev Lvl 3&4','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD',50),'Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD1',50),'ATLO Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD2',50),'ENG Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD3',50),'LABS Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD4',50),'OTHER Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAD5',50),'QUAL Core West Dev Lvl 5','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE',50),'Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE1',50),'ATLO Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE2',50),'ENG Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE3',50),'LABS Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE4',50),'OTHER Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAE5',50),'QUAL Core West Dev Lvl 6&Up','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX',50),'Core West Dev Composite','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX1',50),'ATLO Core West Dev Composite','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX2',50),'ENG Core West Dev Composite','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX3',50),'LABS Core West Dev Composite','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX4',50),'OTHER Core West Dev Composite','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDAX5',50),'QUAL Core West Dev Composite','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA',50),'Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA1',50),'ATLO Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA2',50),'ENG Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA3',50),'LABS Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA4',50),'OTHER Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WDPA5',50),'QUAL Core West Dev HRLY NES OT','Labor-Core-Mission_Solutions-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAA4',50),'Core West Remote HRLY NES ST','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAB4',50),'Core West Remote Lvl 1&2','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAC4',50),'Core West Remote Lvl 3&4','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAD4',50),'Core West Remote Lvl 5','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAE4',50),'Core West Remote Lvl 6&Up','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRAX4',50),'Core West Remote Composite','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('C1WRPA4',50),'Core West Remote HRLY NES OT','Labor-Core-Mission_Solutions-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1C',50),'Orion Cat 1 S/C $ - DO NOT USE','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1H',50),'Orion Cat 1 S/C Hrs - DO NOT USE','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1I_C',50),'Orion Cat 1 IWTA $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1I_F',50),'Orion Cat 1 IWTA FCCOM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1I_H',50),'Orion Cat 1 IWTA Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1S_C',50),'Orion Cat 1 S/C $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1S_C_SERV',50),'Services Orion Cat 1 S/C $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT1S_H',50),'Orion Cat 1 S/C Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT2_4C',50),'Orion Cat 2-4 S/C $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT2_4C_SERV',50),'Services Orion Cat 2-4 S/C $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CAT2_4H',50),'Orion Cat 2-4 S/C Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKD19',50),'Commercial Space Newtown CLIN 19','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAA',50),'Com Space Sys Newtown Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAB',50),'Com Space Sys Newtown Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAC',50),'Com Space Sys Newtown Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAD',50),'Com Space Sys Newtown Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAE',50),'Com Space Sys Newtown Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDAX',50),'Com Space Sys Newtown Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDLB',50),'Commercial Space Sys Newtown Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKDNL',50),'Commercial Space Sys Newtown Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAA',50),'Com Space Sys Newtown Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAB',50),'Com Space Sys Newtown Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAC',50),'Com Space Sys Newtown Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAD',50),'Com Space Sys Newtown Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAE',50),'Com Space Sys Newtown Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPAX',50),'Com Space Sys Newtown Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPLB',50),'Commercial Space Sys Newtown Prod Svc Ct','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMKPNL',50),'Commercial Space Sys Newtown Prod Svc Ct','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAA',50),'Com Space Sys Ofste Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAB',50),'Com Space Sys Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAC',50),'Com Space Sys Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAD',50),'Com Space Sys Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAE',50),'Com Space Sys Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRAX',50),'Com Space Sys Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRLB',50),'Commercial Space Sys Offsite Svc Ctr Lab','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMRRNL',50),'Commercial Space Sys Offsite Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAA',50),'Com Space Sys Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAB',50),'Com Space Sys Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAC',50),'Com Space Sys Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAD',50),'Com Space Sys Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAE',50),'Com Space Sys Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDAX',50),'Com Space Sys Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSDPA',50),'Com Space Sys Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAA',50),'Com Space Sys Snyvle Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAB',50),'Com Space Sys Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAC',50),'Com Space Sys Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAD',50),'Com Space Sys Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAE',50),'Com Space Sys Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CMSPAX',50),'Com Space Sys Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CONTINGENT',50),'NTE/ROM/WAG CONTINGENCY','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core DFC Dollars',50),'Core DFC Dollars','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor $ Base $ Derivative no Burdens',50),'Core Factor $ Base $ Derivative no Burdens','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor $ Base $ Derivative with Burdens',50),'Core Factor $ Base $ Derivative with Burdens','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor $ Base Hrs Dev Pool Derivative',50),'Core Factor $ Base Hrs Dev Pool Derivative','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor Hrs Base $ Derivative no Burdens',50),'Core Factor Hrs Base $ Derivative no Burdens','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor Hrs Base $ Derivative with Burdens',50),'Core Factor Hrs Base $ Derivative with Burdens','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Core Factor Hrs Base Hrs Dev Pool Derivative',50),'Core Factor Hrs Base Hrs Dev Pool Derivative','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAA',50),'Coherent Tech Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAB',50),'Coherent Tech Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAC',50),'Coherent Tech Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAD',50),'Coherent Tech Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAE',50),'Coherent Tech Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDAX',50),'Coherent Tech Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVA',50),'Coherent Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVB',50),'Coherent Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVC',50),'Coherent Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVD',50),'Coherent Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVE',50),'Coherent Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDEVX',50),'Coherent Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDLB',50),'Coherent Technologies Denver Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDNL',50),'Coherent Technologies Denver Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDDPA',50),'Coherent Tech Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAA',50),'Coherent Tech Den Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAB',50),'Coherent Tech Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAC',50),'Coherent Tech Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAD',50),'Coherent Tech Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAE',50),'Coherent Tech Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPAX',50),'Coherent Tech Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPPA',50),'Coherent Tech Den Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDA',50),'Coherent Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDB',50),'Coherent Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDC',50),'Coherent Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDD',50),'Coherent Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDE',50),'Coherent Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CTDPRDX',50),'Coherent Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAA',50),'Pre-2009 Civil Space Den Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAB',50),'Pre-2009 Civil Space Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAC',50),'Pre-2009 Civil Space Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAD',50),'Pre-2009 Civil Space Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAE',50),'Pre-2009 Civil Space Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDAX',50),'Pre-2009 Civil Space Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDLB',50),'Pre-2009 Civil Space Denver Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDNL',50),'Pre-2009 Civil Space Denver Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDDPP',50),'CVDD Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAA',50),'Civil Space Den Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAB',50),'Civil Space Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAC',50),'Civil Space Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAD',50),'Civil Space Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAE',50),'Civil Space Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVDPAX',50),'Civil Space Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAA',50),'Pre-2009 Civil Space Ofste Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAB',50),'Pre-2009 Civil Space Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAC',50),'Pre-2009 Civil Space Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAD',50),'Pre-2009 Civil Space Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAE',50),'Pre-2009 Civil Space Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRAX',50),'Pre-2009 Civil Space Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRLB',50),'Pre-2009 Civil Space Offsite Svc Ctr Lab','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVRRNL',50),'Pre-2009 Civil Space Offsite Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAA',50),'Pre-2009 Civil Space Snyvle Dev Hour & S','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAB',50),'Pre-2009 Civil Space Snyvle Dev Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAC',50),'Pre-2009 Civil Space Snyvle Dev Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAD',50),'Pre-2009 Civil Space Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAE',50),'Pre-2009 Civil Space Snyvle Dev Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDAX',50),'Pre-2009 Civil Space Snyvle Dev Composit','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDLB',50),'Pre-2009 Civil Space Sunnyvale Dev Svc C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSDNL',50),'Pre-2009 Civil Space Sunnyvale Dev Svc C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAA',50),'Pre-2009 Civil Space Snyvle Prod Hour &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAB',50),'Pre-2009 Civil Space Snyvle Prod Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAC',50),'Pre-2009 Civil Space Snyvle Prod Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAD',50),'Pre-2009 Civil Space Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAE',50),'Pre-2009 Civil Space Snyvle Prod Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPAX',50),'Pre-2009 Civil Space Snyvle Prod Composi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPLB',50),'Pre-2009 Civil Space Sunnyvale Prod Svc','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('CVSPNL',50),'Pre-2009 Civil Space Sunnyvale Prod Svc','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVA',50),'Denver Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVB',50),'Denver Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVC',50),'Denver Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVD',50),'Denver Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVE',50),'Denver Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENDEVX',50),'Denver Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDA',50),'Denver Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDB',50),'Denver Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDC',50),'Denver Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDD',50),'Denver Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDE',50),'Denver Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DENPRDX',50),'Denver Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DRELO TVL',50),'Discrete Relocation Costs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS ODC Lab Occy',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS ODC Occy/IT/Fac',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS Pur 11',50),'Pur Parts','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS PUR 11 SERV',50),'Services Pur Parts','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS Pur 54',50),'Licensed SW','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS PUR 54 SERV',50),'Services Licensed SW','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS Pur 55',50),'Lic SW Maint','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS PUR 55 SERV',50),'Services Lic SW Maint','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DS Travel',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DTDY TVL',50),'Discrete TDY-EDY Costs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DTS ODC',50),'Discrete Transportation and Shipping ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('DUO ODC',50),'Discrete Use and Occupancy ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EBS_IWTA_ESC',50),'Autogroup EBS IWTA Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EBS_IWTA_HEF',50),'EBS IWTA HEF','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EIS-$S',50),'EIS SERVICES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EIS-$TRVL',50),'EIS SERVICES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EIS-HR',50),'EIS HOURS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('EIS-LB$',50),'EIS Labor$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAA4',50),'Cape Canaveral (FL)-FBM ER HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAB4',50),'Cape Canaveral (FL)-FBM ER Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAC4',50),'Cape Canaveral (FL)-FBM ER Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAD4',50),'Cape Canaveral (FL)-FBM ER Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAE4',50),'Cape Canaveral (FL)-FBM ER Lvl 6','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFAX4',50),'Cape Canaveral (FBM ER)','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBEFPA4',50),'Cape Canaveral (FL)-FBM ER HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAA4',50),'Kings Bay (GA)-SWFLANT HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAB4',50),'Kings Bay (GA)-SWFLANT Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAC4',50),'Kings Bay (GA)-SWFLANT Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAD4',50),'Kings Bay (GA)-SWFLANT Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAE4',50),'Kings Bay (GA)-SWFLANT Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFAX4',50),'Kings Bay (GA)-SWFLANT Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBGFPA4',50),'Kings Bay (GA)-SWFLANT HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA',50),'Sunnyvale (CA) Dev Strat Mis HR&SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA1',50),'ATLO Sunnyvale Development Strategic MissilesHourly & NES Straight Time Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA2',50),'ENG Sunnyvale Development Strategic MissilesHourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA3',50),'LABS Sunnyvale Development Strategic MissilesHourly & NES Straight Time Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA4',50),'OTHER Sunnyvale Development Strategic MissilesHourly & NES Straight Time Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAA5',50),'QUAL Sunnyvale Development Strategic MissilesHourly & NES Straight Time Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB',50),'Sunnyvale (CA) Dev Strategic Mis Lvl 1&2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB1',50),'ATLO Sunnyvale Development Strategic Missiles Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB2',50),'ENG Sunnyvale Development Strategic Missiles Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB3',50),'LABS Sunnyvale Development Strategic Missiles Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB4',50),'OTHER Sunnyvale Development Strategic Missiles Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAB5',50),'QUAL Sunnyvale Development Strategic Missiles Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC',50),'Sunnyvale (CA) Dev Strategic Mis Lvl 3&4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC1',50),'ATLO Sunnyvale Development Strategic MissilesLvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC2',50),'ENG Sunnyvale Development Strategic MissilesLvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC3',50),'LABS Sunnyvale Development Strategic MissilesLvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC4',50),'OTHER Sunnyvale Development Strategic MissilesLvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAC5',50),'QUAL Sunnyvale Development Strategic MissilesLvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD',50),'Sunnyvale (CA) Dev Strategic Mis Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD1',50),'ATLO Sunnyvale Development Strategic Missiles Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD2',50),'ENG Sunnyvale Development Strategic Missiles Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD3',50),'LABS Sunnyvale Development Strategic Missiles Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD4',50),'OTHER Sunnyvale Development Strategic Missiles Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAD5',50),'QUAL Sunnyvale Development Strategic Missiles Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE',50),'Sunnyvale (CA) Dev Strategic Mis Lvl 6+','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE1',50),'ATLO Sunnyvale Development Strategic Missiles Lvl 6 & Up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE2',50),'ENG Sunnyvale Development Strategic Missiles Lvl 6 & Up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE3',50),'LABS Sunnyvale Development Strategic Missiles Lvl 6 & Up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE4',50),'OTHER Sunnyvale Development Strategic Missiles Lvl 6 & Up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAE5',50),'QUAL Sunnyvale Development Strategic Missiles Lvl 6 & Up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX',50),'Sunnyvale Development Strategic Missiles','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX1',50),'ATLO Sunnyvale Development Strategic Missiles Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX2',50),'ENG Sunnyvale Development Strategic Missiles Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX3',50),'LABS Sunnyvale Development Strategic Missiles Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX4',50),'OTHER Sunnyvale Development Strategic Missiles Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDAX5',50),'QUAL Sunnyvale Development Strategic Missiles Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA',50),'Sunnyvale (CA) Dev Strat Mis HR&SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA1',50),'ATLO Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA2',50),'ENG Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA3',50),'LABS Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA4',50),'OTHER Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA5',50),'QUAL Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSDPA6',50),'TOUCH Sunnyvale Development Strategic MissilesHourly & NES Overtime Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA',50),'Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA1',50),'ATLO Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA2',50),'ENG Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA3',50),'LABS Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA4',50),'OTHER Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAA5',50),'QUAL Sunnyvale (CA) Prod Strat Mis HR&SNES ST','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB',50),'Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB1',50),'ATLO Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB2',50),'ENG Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB3',50),'LABS Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB4',50),'OTHER Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAB5',50),'QUAL Sunnyvale (CA) Prod Strategic Mis Lvl1&2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC',50),'Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC1',50),'ATLO Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC2',50),'ENG Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC3',50),'LABS Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC4',50),'OTHER Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAC5',50),'QUAL Sunnyvale (CA) Prod Strategic Mis Lvl3&4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD',50),'Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD1',50),'ATLO Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD2',50),'ENG Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD3',50),'LABS Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD4',50),'OTHER Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAD5',50),'QUAL Sunnyvale (CA) Prod Strategic Mis Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE',50),'Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE1',50),'ATLO Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE2',50),'ENG Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE3',50),'LABS Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE4',50),'OTHER Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAE5',50),'QUAL Sunnyvale (CA) Prod Strategic Mis Lvl 6+','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX',50),'Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX1',50),'ATLO Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX2',50),'ENG Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX3',50),'LABS Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX4',50),'OTHER Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPAX5',50),'QUAL Sunnyvale (CA) Prod Strategic Mis Comp','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA',50),'Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA1',50),'ATLO Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA2',50),'ENG Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA3',50),'LABS Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA4',50),'OTHER Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA5',50),'QUAL Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBSPPA6',50),'TOUCH Sunnyvale (CA) Prod Strat Mis HR&SNES OT','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA',50),'Valley Forge (PA) Development HR/SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA1',50),'ATLO Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA2',50),'ENG Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA3',50),'LABS Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA4',50),'OTHER Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA5',50),'QUAL Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAA6',50),'TOUCH Valley Forge DevelopmentHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB',50),'Valley Forge (PA) Development Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB1',50),'ATLO Valley Forge Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB2',50),'ENG Valley Forge Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB3',50),'LABS Valley Forge Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB4',50),'OTHER Valley Forge Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAB5',50),'QUAL Valley Forge Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC',50),'Valley Forge (PA) Development Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC1',50),'ATLO Valley Forge DevelopmentLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC2',50),'ENG Valley Forge DevelopmentLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC3',50),'LABS Valley Forge DevelopmentLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC4',50),'OTHER Valley Forge DevelopmentLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAC5',50),'QUAL Valley Forge DevelopmentLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD',50),'Valley Forge (PA) Development Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD1',50),'ATLO Valley Forge Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD2',50),'ENG Valley Forge Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD3',50),'LABS Valley Forge Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD4',50),'OTHER Valley Forge Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAD5',50),'QUAL Valley Forge Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE',50),'Valley Forge (PA) Development Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE1',50),'ATLO Valley Forge Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE2',50),'ENG Valley Forge Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE3',50),'LABS Valley Forge Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE4',50),'OTHER Valley Forge Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAE5',50),'QUAL Valley Forge Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX',50),'Valley Forge (PA) Development Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX1',50),'ATLO Valley Forge Development Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX2',50),'ENG Valley Forge Development Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX3',50),'LABS Valley Forge Development Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX4',50),'OTHER Valley Forge Development Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDAX5',50),'QUAL Valley Forge Development Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA',50),'Valley Forge (PA) Development HR/SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA1',50),'ATLO Valley Forge DevelopmentHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA2',50),'ENG Valley Forge DevelopmentHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA3',50),'LABS Valley Forge DevelopmentHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA4',50),'OTHER Valley Forge DevelopmentHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA5',50),'QUAL Valley Forge DevelopmentHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVDPA6',50),'TOUCH Valley Forge  DevelopmentHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA',50),'Valley Forge (PA) Production HR/SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA1',50),'ATLO Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA2',50),'ENG Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA3',50),'LABS Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA4',50),'OTHER Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA5',50),'QUAL Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAA6',50),'TOUCH Valley Forge ProductionHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB',50),'Valley Forge (PA) Production Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB1',50),'ATLO Valley Forge Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB2',50),'ENG Valley Forge Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB3',50),'LABS Valley Forge Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB4',50),'OTHER Valley Forge Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB5',50),'QUAL Valley Forge Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAB6',50),'TOUCH Valley Forge Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC',50),'Valley Forge (PA) Production Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC1',50),'ATLO Valley Forge ProductionLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC2',50),'ENG Valley Forge ProductionLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC3',50),'LABS Valley Forge ProductionLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC4',50),'OTHER Valley Forge ProductionLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC5',50),'QUAL Valley Forge ProductionLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAC6',50),'TOUCH Valley Forge ProductionLvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD',50),'Valley Forge (PA) Production Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD1',50),'ATLO Valley Forge Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD2',50),'ENG Valley Forge Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD3',50),'LABS Valley Forge Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD4',50),'OTHER Valley Forge Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD5',50),'QUAL Valley Forge Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAD6',50),'TOUCH Valley Forge Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE',50),'Valley Forge (PA) Production Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE1',50),'ATLO Valley Forge Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE2',50),'ENG Valley Forge Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE3',50),'LABS Valley Forge Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE4',50),'OTHER Valley Forge Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE5',50),'QUAL Valley Forge Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAE6',50),'TOUCH Valley Forge Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX',50),'Valley Forge (PA) Production Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX1',50),'ATLO Valley Forge Production Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX2',50),'ENG Valley Forge Production Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX3',50),'LABS Valley Forge Production Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX4',50),'OTHER Valley Forge Production Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX5',50),'QUAL Valley Forge Production Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPAX6',50),'TOUCH Valley Forge Production Composite','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA',50),'Valley Forge (PA) Production HR/SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA1',50),'ATLO Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA2',50),'ENG Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA3',50),'LABS Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA4',50),'OTHER Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA5',50),'QUAL Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBVPPA6',50),'TOUCH Valley Forge ProductionHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAA4',50),'Bangor(WA)-SWFPAC HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAB4',50),'Bangor(WA)-SWFPAC Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAC4',50),'Bangor(WA)-SWFPAC Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAD4',50),'Bangor(WA)-SWFPAC Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAE4',50),'Bangor(WA)-SWFPAC Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFAX4',50),'Bangor(WA)-SWFPAC Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FBWFPA4',50),'Bangor(WA)-SWFPAC HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FCCOM IWTA',50),'IWTA Cost of Money','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FCCOMIWTA',50),'IWTA Cost of Money (THAAD)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Fee',50),'Fee','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FEE SERV',50),'Services Fee','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FMS SUB ESC',50),'FMS SUB ESC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDD',50),'Functional Denver Development','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAA',50),'Funct Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAB',50),'Funct Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAC',50),'Funct Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAD',50),'Funct Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAE',50),'Funct Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDAX',50),'Funct Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDLB',50),'Functional Denver Dev Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDNL',50),'Functional Denver Dev Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDPA',50),'Funct Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDDPP',50),'FNDD Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDP',50),'Functional Denver Production','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAA',50),'Funct Den Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAB',50),'Funct Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAC',50),'Funct Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAD',50),'Funct Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAE',50),'Funct Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPAX',50),'Funct Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPLB',50),'Functional Denver Prod Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPNL',50),'Functional Denver Prod Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPPA',50),'Funct Den Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNDPPP',50),'FNDP Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAA',50),'Newtown Functional Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAB',50),'Newtown Functional Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAC',50),'Newtown Functional Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAC (Thru 06/12)',50),'Newtown Functional Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAD',50),'Newtown Functional Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAD (Thru 06/12)',50),'Newtown Functional Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAE',50),'Newtown Functional Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAE (Thru 06/12)',50),'Newtown Functional Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDAX',50),'Newtown Functional Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDLB',50),'Newtown Functional Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDNL',50),'Newtown Functional Dev Svc Ctr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKDPA',50),'Newtown Functional Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAA',50),'Newtown Functional Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAB',50),'Newtown Functional Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAC',50),'Newtown Functional Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAD',50),'Newtown Functional Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAE',50),'Newtown Functional Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPAX',50),'Newtown Functional Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPLB',50),'Newtown Functional Prod Svc Ct','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPNL',50),'Newtown Functional Prod Svc Ct','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNKPPA',50),'Newtown Functional Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAA',50),'Funct Stennis Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAB',50),'Funct Stennis Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAC',50),'Funct Stennis Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAD',50),'Funct Stennis Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAE',50),'Funct Stennis Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDAX',50),'Funct Stennis Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDLB',50),'Functional Stennis Dev Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDNL',50),'Functional Stennis Dev Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMDPA',50),'Funct Stennis Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAA',50),'Funct Stennis Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAB',50),'Funct Stennis Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAC',50),'Funct Stennis Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAD',50),'Funct Stennis Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAE',50),'Funct Stennis Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPAX',50),'Funct Stennis Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPLB',50),'Functional Stennis Prod Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPNL',50),'Functional Stennis Prod Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNMPPA',50),'Funct Stennis Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAA',50),'Funct Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAB',50),'Funct Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAC',50),'Funct Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAD',50),'Funct Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAE',50),'Funct Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRAX',50),'Funct Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNRRPA',50),'Funct Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAA',50),'Funct Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAB',50),'Funct Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAC',50),'Funct Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAD',50),'Funct Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAE',50),'Funct Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDAX',50),'Funct Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDLB',50),'Functional Sunnyvale Dev Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDNL',50),'Functional Sunnyvale Dev Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDPA',50),'Funct Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSDPP',50),'FNSD Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAA',50),'Funct Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAB',50),'Funct Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAC',50),'Funct Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAD',50),'Funct Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAE',50),'Funct Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPAX',50),'Funct Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPLB',50),'Functional Sunnyvale Prod Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPNL',50),'Functional Sunnyvale Prod Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNSPPA',50),'Funct Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAA',50),'Funct Harlingen Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAB',50),'Funct Harlingen Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAC',50),'Funct Harlingen Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAD',50),'Funct Harlingen Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAE',50),'Funct Harlingen Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTDAX',50),'Funct Harlingen Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAA',50),'Funct Harlingen Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAB',50),'Funct Harlingen Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAC',50),'Funct Harlingen Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAD',50),'Funct Harlingen Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAE',50),'Funct Harlingen Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FNTPAX',50),'Funct Harlingen Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FP ODC',50),'Factored Program ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FP TVL',50),'Factored Program Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Frank 541940',50),'Frank 541940','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Freight',50),'Freight Factor','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Freight Serv',50),'Services Freight Factor','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA1',50),'ATLO Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA2',50),'ENG Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA3',50),'LABS Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA4',50),'OTHER Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA5',50),'QUAL Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAA6',50),'TOUCH Denver FBM Development Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB1',50),'ATLO Denver FBM Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB2',50),'ENG Denver FBM Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB3',50),'LABS Denver FBM Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB4',50),'OTHER Denver FBM Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB5',50),'QUAL Denver FBM Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAB6',50),'TOUCH Denver FBM Development Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC1',50),'ATLO Denver FBM Development Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC2',50),'ENG Denver FBM Development Lvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC3',50),'LABS Denver FBM Development Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC4',50),'OTHER Denver FBM Developmen Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC5',50),'QUAL Denver FBM Development Lvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAC6',50),'TOUCH Denver FBM Development Lvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD1',50),'ATLO Denver FBM Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD2',50),'ENG Denver FBM Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD3',50),'LABS Denver FBM Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD4',50),'OTHER Denver FBM Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD5',50),'QUAL Denver FBM Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAD6',50),'TOUCH Denver FBM Development Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE1',50),'ATLO Denver FBM Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE2',50),'ENG Denver FBM Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE3',50),'LABS Denver FBM Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE4',50),'OTHER Denver FBM Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE5',50),'QUAL Denver FBM Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDAE6',50),'TOUCH Denver FBM Development Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA1',50),'ATLO Denver FBM Development Hourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA2',50),'ENG Denver FBM Development Hourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA3',50),'LABS Denver FBM Development Hourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA4',50),'OTHER Denver FBM Developmen tHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA5',50),'QUAL Denver FBM Development Hourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDDPA6',50),'TOUCH Denver FBM Development Hourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA1',50),'ATLO Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA2',50),'ENG Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA3',50),'LABS Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA4',50),'OTHER Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA5',50),'QUAL Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAA6',50),'TOUCH Denver FBM Production Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB1',50),'ATLO Denver FBM Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB2',50),'ENG Denver FBM Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB3',50),'LABS Denver FBM ProductionLvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB4',50),'OTHER Denver FBM Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB5',50),'QUAL Denver FBM Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAB6',50),'TOUCH Denver FBM Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC1',50),'ATLO Denver FBM Production Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC2',50),'ENG Denver FBM Production Lvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC3',50),'LABS Denver FBM Production Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC4',50),'OTHER Denver FBM Production Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC5',50),'QUAL Denver FBM Production Lvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAC6',50),'TOUCH Denver FBM Production Lvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD1',50),'ATLO Denver FBM Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD2',50),'ENG Denver FBM Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD3',50),'LABS Denver FBM Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD4',50),'LABS Denver FBM Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD5',50),'QUAL Denver FBM Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAD6',50),'TOUCH Denver FBM Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE1',50),'ATLO Denver FBM Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE2',50),'ENG Denver FBM Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE3',50),'LABS Denver FBM Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE4',50),'OTHER Denver FBM Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE5',50),'QUAL Denver FBM Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPAE6',50),'TOUCH Denver FBM Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA1',50),'ATLO Denver FBM Production Hourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA2',50),'ENG Denver FBM Production Hourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA3',50),'LABS Denver FBM Production Hourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA4',50),'OTHER Denver FBM ProductionHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA5',50),'QUAL Denver FBM Production Hourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXDPPA6',50),'TOUCH Denver FBM Production Hourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA1',50),'ATLO Titusville Development Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA2',50),'ENG Titusville Development Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA3',50),'LABS Titusville Development Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA4',50),'OTHER Titusville Development Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA5',50),'QUAL Titusville Development Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAA6',50),'TOUCH Titusville Development Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB1',50),'ATLO Titusville Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB2',50),'ENG Titusville Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB3',50),'LABS Titusville Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB4',50),'OTHER Titusville Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB5',50),'QUAL Titusville Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAB6',50),'TOUCH Titusville Development Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC1',50),'ATLO Titusville DevelopmentLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC2',50),'ENG Titusville DevelopmentLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC3',50),'LABS Titusville DevelopmentLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC4',50),'OTHER Titusville DevelopmentLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC5',50),'QUAL Titusville DevelopmentLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAC6',50),'TOUCH Titusville DevelopmentLvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD1',50),'ATLO Titusville Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD2',50),'ENG Titusville Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD3',50),'LABS Titusville Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD4',50),'OTHER Titusville Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD5',50),'QUAL Titusville Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAD6',50),'TOUCH Titusville Development Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE1',50),'ATLO Titusville Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE2',50),'ENG  Titusville Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE3',50),'LABS Titusville Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE4',50),'OTHER Titusville Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE5',50),'QUAL Titusville Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDAE6',50),'TOUCH Titusville Development Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA1',50),'ATLO Titusville Development Hourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA2',50),'ENG  Titusville Development Hourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA3',50),'LABS  Titusville Development Hourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA4',50),'OTHER  Titusville Development Hourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA5',50),'QUAL  Titusville Development Hourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEDPA6',50),'TOUCH  Titusville Development Hourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA1',50),'ATLO Titusville Production Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA2',50),'ENG Titusville Production Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA3',50),'LABS Titusville Production Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA4',50),'OTHER Titusville Production Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA5',50),'QUAL Titusville Production Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAA6',50),'TOUCH Titusville Production Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB1',50),'ATLO Titusville Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB2',50),'ENG Titusville Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB3',50),'LABS Titusville Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB4',50),'OTHER Titusville ProductionLvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB5',50),'QUAL Titusville Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAB6',50),'TOUCH Titusville Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC1',50),'ATLO Titusville Production Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC2',50),'ENG Titusville Production tLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC3',50),'LABS Titusville Production Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC4',50),'OTHER Titusville Production Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC5',50),'QUAL Titusville Production tLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAC6',50),'TOUCH Titusville Production Lvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD1',50),'ATLO Titusville Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD2',50),'ENG Titusville Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD3',50),'LABS Titusville Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD4',50),'OTHER Titusville Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD5',50),'QUAL Titusville Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAD6',50),'TOUCH Titusville Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE1',50),'ATLO Titusville Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE2',50),'ENG Titusville Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE3',50),'LABS Titusville Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE4',50),'OTHER Titusville Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE5',50),'QUAL Titusville Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPAE6',50),'TOUCH Titusville Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA1',50),'ATLO Titusville Production Hourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA2',50),'ENG Titusville Production Hourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA3',50),'LABS Titusville Production Hourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA4',50),'OTHER Titusville Production Hourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA5',50),'QUAL Titusville Production Hourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('FXEPPA6',50),'TOUCH Titusville Production Hourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD Labor$',50),'GD Subcontract Labor $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD MAT$',50),'GD Subcontract Material $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD ODC$',50),'GD Subcontract ODC $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD SUB$',50),'GD Subcontract SUB $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD TRVL$',50),'GD Subcontract Travel $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GD$',50),'GD Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GDAIS Hours',50),'GDAIS Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAA',50),'Global Communication Den Dev HR & SNES S','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAB',50),'Global Communication Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAC',50),'Global Communication Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAD',50),'Global Communication Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAE',50),'Global Communication Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDAX',50),'Global Communication Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNDDPA',50),'Global Communication Den Dev HR & SNES O','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAA',50),'GCS Newtown Development Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAB',50),'GCS Newtown Development Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAC',50),'GCS Newtown Development Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAD',50),'GCS Newtown Development Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAE',50),'GCS Newtown Development Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKDAX',50),'GCS Newtown Development Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAA',50),'GCS Newtown Production Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAB',50),'GCS Newtown Production Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAC',50),'GCS Newtown Production Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAD',50),'GCS Newtown Production Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAE',50),'GCS Newtown Production Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNKPAX',50),'GCS Newtown Production Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAA',50),'Global Communication Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAB',50),'Global Communication Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAC',50),'Global Communication Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAD',50),'Global Communication Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAE',50),'Global Communication Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRAX',50),'Global Communication Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNRRPA',50),'Global Communication Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAA',50),'Global Comm Snyvle Dev Hour & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAB',50),'Global Communication Snyvle Dev Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAC',50),'Global Communication Snyvle Dev Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAD',50),'Global Communication Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAE',50),'Global Communication Snyvle Dev Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDAX',50),'Global Communication Snyvle Dev Composit','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSDPA',50),'Global Comm Snyvle Dev Hour & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAA',50),'Global Comm Snyvle Prd Hour & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAB',50),'Global Communication Snyvle Prod Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAC',50),'Global Communication Snyvle Prod Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAD',50),'Global Communication Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAE',50),'Global Communication Snyvle Prod Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPAX',50),'Global Communication Snyvle Prod Composi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GNSPPA',50),'Global Comm Snyvle Prd Hour & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GPSDEV',50),'GPSIII Development','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GPSIIIPMO-11',50),'PMO Newtown Labor Cost CER (CLIN 011)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GPSIIIPMO-16',50),'PMO Newtown Labor Cost CER (CLIN 016)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GPSPRD',50),'GPSIII Production OVHD Labor Pool','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('GPSRMT',50),'GPSIII Remote OVHD Labor Pool','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HEF-A',50),'Factored Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAA',50),'Human Space Flight Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAB',50),'Human Space Flight Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAC',50),'Human Space Flight Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAD',50),'Human Space Flight Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAE',50),'Human Space Flight Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDAX',50),'Human Space Flight Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDLB',50),'Human Space Flight Denver Dev Svc Ctr La','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDNL',50),'Human Space Flight Denver Dev Svc Ctr OD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFDDPA',50),'Human Space Flight Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAB',50),'Human Space Flight Michoud Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAC',50),'Human Space Flight Michoud Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAD',50),'Human Space Flight Michoud Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAE',50),'Human Space Flight Michoud Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMAX',50),'Human Space Flight Michoud Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFLMPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAA',50),'HSF Michoud Mfg Build Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAB',50),'HSF Michoud Mfg Build Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAC',50),'HSF Michoud Mfg Build Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAD',50),'HSF Michoud Mfg Build Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAE',50),'HSF Michoud Mfg Build Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBAX',50),'HSF Michoud Mfg Build Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMBPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAA',50),'HSF Michoud Engineering Hours & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAB',50),'HSF Michoud Engineering Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAC',50),'HSF Michoud Engineering Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAD',50),'HSF Michoud Engineering Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAE',50),'HSF Michoud Engineering Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEAX',50),'HSF Michoud Engineering Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMEPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAA',50),'HSF Michoud Facilities Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAB',50),'HSF Michoud Facilities Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAC',50),'HSF Michoud Facilities Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAD',50),'HSF Michoud Facilities Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAE',50),'HSF Michoud Facilities Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFAX',50),'HSF Michoud Facilities Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMFPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAA',50),'HSF Michoud Inspection Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAB',50),'HSF Michoud Inspection Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAC',50),'HSF Michoud Inspection Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAD',50),'HSF Michoud Inspection Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAE',50),'HSF Michoud Inspection Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIAX',50),'HSF Michoud Inspection Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMIPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAB',50),'Human Space Flight Michoud Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAC',50),'Human Space Flight Michoud Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAD',50),'Human Space Flight Michoud Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAE',50),'Human Space Flight Michoud Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMMAX',50),'Human Space Flight Michoud Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAA',50),'HSF Michoud Other Hours & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAB',50),'HSF Michoud Other Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAC',50),'HSF Michoud Other Lvl 3&4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAD',50),'HSF Michoud Other Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAE',50),'HSF Michoud Other Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOAX',50),'HSF Michoud Other Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMOPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAA',50),'HSF Michoud Production Hours & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAB',50),'HSF Michoud Production Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAC',50),'HSF Michoud Production Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAD',50),'HSF Michoud Production Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAE',50),'HSF Michoud Production Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMPAX',50),'HSF Michoud Production Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAA',50),'HSF Michoud Quality Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAB',50),'HSF Michoud Quality Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAC',50),'HSF Michoud Quality Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAD',50),'HSF Michoud Quality Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAE',50),'HSF Michoud Quality Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQAX',50),'HSF Michoud Quality Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMQPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAA',50),'HSF Michoud Req to Mfg Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAB',50),'HSF Michoud Req to Mfg Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAC',50),'HSF Michoud Req to Mfg Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAD',50),'HSF Michoud Req to Mfg Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAE',50),'HSF Michoud Req to Mfg Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRAX',50),'HSF Michoud Req to Mfg Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMRPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAA',50),'HSF Michoud Mfg Support Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAB',50),'HSF Michoud Mfg Support Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAC',50),'HSF Michoud Mfg Support Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAD',50),'HSF Michoud Mfg Support Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAE',50),'HSF Michoud Mfg Support Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSAX',50),'HSF Michoud Mfg Support Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMSPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAA',50),'HSF Michoud Tooling Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAB',50),'HSF Michoud Tooling Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAC',50),'HSF Michoud Tooling Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAD',50),'HSF Michoud Tooling Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAE',50),'HSF Michoud Tooling Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTAX',50),'HSF Michoud Tooling Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFMTPA',50),'Human Space Flight Michoud Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAA',50),'Human Space Flight Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAB',50),'Human Space Flight Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAC',50),'Human Space Flight Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAD',50),'Human Space Flight Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAE',50),'Human Space Flight Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRAX',50),'Human Space Flight Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRLB',50),'Human Space Flight Remote Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRNL',50),'Human Space Flight Remote Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HFRRPA',50),'Human Space Flight Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLA',50),'Huntsville Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLB',50),'Huntsville Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLC',50),'Huntsville Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLD',50),'Huntsville Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLE',50),'Huntsville Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HNTSVLX',50),'Huntsville Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HRNESESC',50),'Hourly & NES Labor Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HSF Cntng Fct No Sub',50),'HSF Contingency Factor No Subs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HSF Contng Fact',50),'HSF Contingency Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('HSF_Misc_ODC',50),'HSF Miscellaneous ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAA',50),'Int Launch Services Ofste Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAB',50),'Int Launch Services Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAC',50),'Int Launch Services Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAD',50),'Int Launch Services Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAE',50),'Int Launch Services Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ILRIAX',50),'Int Launch Services Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Insurance',50),'Launch Insurance','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Insurance Fee',50),'Fee on Insurance','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('INTERN',50),'LMSSC Intern','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS $',50),'IWTA IS&GS Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS CAS',50),'IWTA IS&GS CAS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS FCCOM',50),'IWTA IS&GS CAS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS H',50),'IWTA IS&GS Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS HRS',50),'IWTA IS&GS HRS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS LBR',50),'IS&GS LABOR$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS MAT',50),'IS&GS Material','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS SUB$',50),'IS&GS SUB$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS TRVL$',50),'IS&GS TRAVEL $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS-OS $',50),'IS&GS-OS Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS-SS $',50),'IS&GS-SS Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS-SS CAS',50),'IS&GS-SS CAS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IS&GS-SS H',50),'IS&GS-SS Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ISE HRS',50),'IWTA IS&GS HRS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ISE LB$',50),'Subcontract$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ISE-$T',50),'Subcontract$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT Hrs',50),'ITT Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT LB$',50),'ITT Lbr$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT MAT$',50),'ITT Mat$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT SUB$',50),'ITT Subcontract $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT TRAV$',50),'ITT Travel $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ITT$',50),'ITT Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA',50),'IWTA Dollars and Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA FCCOM',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA SERV',50),'Services Services IWTA Dollars and Hours','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$02',50),'IWTA DOLLARS (IS&GS)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$1',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$10',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$11',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$12',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$13',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$14',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$15',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$16',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$17',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$18',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$19',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$2',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$20',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$3',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$4',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$5',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$6',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$7',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$8',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$9',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$F1',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$F2',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$F3',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$F4',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA$F5',50),'IWTA FCCOM','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_$',50),'IWTA Dollars','IWTA-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_F',50),'IWTA FCCOM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_FCCOM',50),'IWTA FCCOM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_H',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_SERV$',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA_SERV_H',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA1',50),'IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA2',50),'IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTA3',50),'IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAFCCOM',50),'IWTA FCCOM $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH1',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH10',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH11',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH12',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH13',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH14',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH15',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH16',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH17',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH18',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH19',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH2',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH20',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH21',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH22',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH23',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH24',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH25',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH3',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH4',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH5',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH6',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH7',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH8',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAH9',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTAHR',50),'IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$1',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$2',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$3',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$4',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV$5',50),'Services IWTA Dollars','IWTA-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV1',50),'Services IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV2',50),'Services IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERV3',50),'Services IWTA Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH1',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH10',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH11',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH12',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH13',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH14',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH15',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH2',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH3',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH4',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH5',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH6',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH7',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH8',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVH9',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('IWTASERVHR',50),'Services IWTA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JDPA',50),'Woburn (MA) Development HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBDAX',50),'Job Shoppers Development Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBDAX-$',50),'Job Shoppers Development Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBHAX',50),'Job Shoppers Huntsville Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBHAX-$',50),'Job Shoppers Huntsville Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBPAX',50),'Job Shoppers Production Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBPAX-$',50),'Job Shoppers Production Comp','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBRAX',50),'Job Shoppers Remote','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBRAX-$',50),'Job Shoppers Remote','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBSERVOFFAX',50),'Services Job Shoppers Offsite Comp','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBSERVOFFAX-$',50),'Services Job Shoppers Offsite Comp','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBSERVONAX',50),'Services Job Shoppers Onsite Comp','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('JOBSERVONAX-$',50),'Services Job Shoppers Onsite Comp','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Base PDSP',50),'Labor Base PDSP','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base Core PMO - LB',50),'Labor Dollar Base Core PMO - LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base Core PMO - NL',50),'Labor Dollar Base Core PMO - NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base PDSP - LB',50),'Labor Dollar Base PDSP - LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base PDSP - NL',50),'Labor Dollar Base PDSP - NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base PDSP Offsite - LB',50),'Labor Dollar Base PDSP Offsite - LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Dollar Base PDSP Offsite - NL',50),'Labor Dollar Base PDSP Offsite - NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Hours Base PDSP Offsite - LB',50),'Labor Hours Base PDSP Offsite - LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Hours Base PDSP Offsite - NL',50),'Labor Hours Base PDSP Offsite - NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor Hours Base PDSP T2 Offsite - LB',50),'Labor Hours Base PDSP T2 Offsite - LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Labor$ Dev',50),'IRAD ROM Labor$ Dev','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LAuaUeng00',50),'ULA Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LMEBS$',50),'LMEBS Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LMEBS_H',50),'LMEBS Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LMPA',50),'Michoud (LA) HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0001',50),'VA Software Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0002',50),'VA Software Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0003',50),'VA Systems Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0004',50),'VA Systems Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0005',50),'VA Systems Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0006',50),'VA Project Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0007',50),'VA Procurement Engr Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0008',50),'VA Info Assurance Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0009',50),'VA Info Assurance Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0010',50),'VA Mult Func Fin Anal Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0011',50),'VA Program Planner Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0012',50),'VA Contracts Neg Prin','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0013',50),'VA Subcontract Administrator Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0014',50),'VA Sub Mgmt Mgr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0015',50),'PA Bus Oper Mgr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0016',50),'PA Software Engineer Asc','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0017',50),'PA Software Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0018',50),'PA Software Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0019',50),'PA Software Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0020',50),'PA Systems Engineer Asc','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0021',50),'PA Systems Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0022',50),'PA Systems Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0023',50),'PA Systems Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0024',50),'PA Systems Engineer Sr Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0025',50),'PA Software Quality Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0026',50),'PA Software Quality Engineer Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0027',50),'PA Sys Integratn/Test Eng','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0028',50),'PA Sys Integratn/Test Eng Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0029',50),'VA Sys Integratn/Test Eng Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0030',50),'PA Industrial Security Rep Asc','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0031',50),'PA Industrial Security Rep','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0032',50),'PA Regulatory Doc Analyst','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0033',50),'PA Regulatory Doc Analyst Sr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0034',50),'PA PM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0035',50),'VA Deputy PM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0036',50),'PA Chief Engineer','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0037',50),'PA Chief Architect','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0038',50),'VA Program Control Lead','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0039',50),'PA Chief Architect Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS LM LBR HR 0040',50),'PA Program Management Asc Mgr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 0001',50),'NP Database Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 0002',50),'Adsum Systems Engineer Prin','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 0003',50),'KeyW Software Engineer Sr Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 016',50),'NG Software Engineer Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 017',50),'NG Software Engineer Sr Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 018',50),'NG Software Engineering Mgr','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 019',50),'IMCS Software Engineer Prin','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 020',50),'ESRI Principal/GIS Consultant/PM 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 021',50),'ESRI Sr GIS Consultant/PM 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 022',50),'ESRI GIS Consultant/PM 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 023',50),'ESRI Sr. GIS System/SW Architect 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 024',50),'ESRI GIS System/SW Developer 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 025',50),'ESRI GIS Technical Specialist/Eng 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 026',50),'ESRI GIS DB Specialist/Analyst','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 027',50),'ESRI GIS Data Processor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 028',50),'KeyW DB Engineer Sr Stf','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS SUB HRS 029',50),'KeyW Software Engineer Principle','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('LS TRAVEL',50),'LM Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MAT $ No Esc',50),'Material Dollars','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MAT $ to be Esc',50),'Material Dollars','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MAT$',50),'Material Dollars','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mat$ Esc',50),'Material Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MAT$ ODC',50),'Material / ODC $','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mat$NoFee',50),'Material No Fee (THAAD)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MAT_ODC',50),'Material / ODC $','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Material Escalation',50),'Autogroup Material  Escalation only ***','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Material Factor',50),'Material Factor','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATESC',50),'Material Escalation','Material-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATFLT',50),'Material-Flight Hardware','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATMAF2',50),'HSF Material Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATMAFSERV2',50),'Services HSF Material Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERV$',50),'Services Material Dollars','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERV$ ESC',50),'Services Material Escalation','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERV$ NO ESC',50),'Services Material Dollars','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERV$ TO BE ESC',50),'Services Material Dollars','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERV$NOFEE',50),'Services Material No Fee (THAAD)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERVESC',50),'Services Material Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MATSERVFLT',50),'Services Material-Flight Hardware','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAA',50),'Msl Def Courtland Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAB',50),'Msl Def Courtland Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAC',50),'Msl Def Courtland Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAD',50),'Msl Def Courtland Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAE',50),'Msl Def Courtland Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBDAX',50),'Msl Def Courtland Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAA',50),'Msl Def Courtland Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAB',50),'Msl Def Courtland Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAC',50),'Msl Def Courtland Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAD',50),'Msl Def Courtland Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAE',50),'Msl Def Courtland Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDBPAX',50),'Msl Def Courtland Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDD25X',50),'Msl Def Den Dev-Targets','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDD7X',50),'Msl Def Den Dev-MRBM-7 Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAA',50),'Msl Def Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAB',50),'Msl Def Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAC',50),'Msl Def Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAD',50),'Msl Def Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAE',50),'Msl Def Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDAX',50),'Msl Def Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDDPA',50),'Msl Def Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAA',50),'Msl Def Den Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAB',50),'Msl Def Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAC',50),'Msl Def Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAD',50),'Msl Def Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAE',50),'Msl Def Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPAX',50),'Msl Def Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDDPPA',50),'Msl Def Den Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHH25X',50),'Msl Def Huntsville-Targets','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHH3X',50),'Targets DO23 Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHH4X',50),'Targets DO24 Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHH5X',50),'Targets DO25 Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHH7X',50),'Ms Def Hnts-MRBM-7 Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAA',50),'Msl Def Hunts  HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAB',50),'Msl Def Hunts  Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAC',50),'Msl Def Hunts  Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAD',50),'Msl Def Hunts  Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAE',50),'Msl Def Hunts  Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAX',50),'Msl Def Hunts  Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAX22',50),'Msl Def Huntsville Composite DO22','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAX23',50),'Msl Def Huntsville Composite DO23','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAX24',50),'Msl Def Huntsville Composite DO24','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAX25',50),'Msl Def Huntsville Composite DO25','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHAZ',50),'Missile Def HNTV CLIN 7 Launch','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHLB',50),'Missile Defense Huntsville  Svc Ctr Labo','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHNL',50),'Missile Defense Huntsville  Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHPA',50),'Msl Def Hunts  HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDHHSP',50),'THAAD SP Composite MDHH','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAA',50),'Msl Def Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAB',50),'Msl Def Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAC',50),'Msl Def Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAD',50),'Msl Def Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAE',50),'Msl Def Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAX',50),'Msl Def Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRAX (In Country)',50),'In Country Remote Offsite Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDRRPA',50),'Msl Def Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAA',50),'Msl Def Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAB',50),'Msl Def Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAC',50),'Msl Def Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAD',50),'Msl Def Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAE',50),'Msl Def Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDAX',50),'Msl Def Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDPA',50),'Msl Def Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSDSP',50),'THAAD SP Composite MDSD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAA',50),'Msl Def Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAB',50),'Msl Def Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAC',50),'Msl Def Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAD',50),'Msl Def Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAE',50),'Msl Def Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPAX',50),'Msl Def Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MDSPPA',50),'Msl Def Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('METROLOGY LB CER',50),'Metrology CER-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('METROLOGY LB FACTOR',50),'Metrology Factor-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('METROLOGY NL CER',50),'Metrology CER-Non-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('METROLOGY NL FACTOR',50),'Metrology Factor-Non-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MGRE',50),'Management Reserve','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MGRE SERV',50),'Services Management Reserve','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mich Inspctn LB Fact',50),'Michoud Inspection LBR Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH INSPCTN LBR CER',50),'Michoud Inspection Lbr CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mich MFG Spt LB Fact',50),'Michoud Mfg Support LB Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH MFG SPT LBR CER',50),'Michoud Manuf Support Lbr CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mich OTS LB Factor',50),'Michoud OTS LBR Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH OTS LBR CER',50),'Michoud OTS Labor CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH PA SPT LBR CER',50),'Michoud PA Support Lbr CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mich PA Spt Lbr Fact',50),'Michoud Prod Assurance LB Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH PTS LBR CER',50),'Michoud Prod Technical Support CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH TOOL MNT LB CER',50),'Michoud Tool Maintenance Lbr CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICH TST PANEL LBR',50),'Michoud Test Panel Lbr CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODA',50),'Michoud Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODB',50),'Michoud Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODC',50),'Michoud Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODD',50),'Michoud Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODE',50),'Michoud Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MICHODX',50),'Michoud Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mileage',50),'Personal Car Travel $/Mile','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Mileage Serv',50),'Services Personal Car Travel $/Mile','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMH23X',50),'Msl Def HNT Composite DO23 M & S','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMH24X',50),'Msl Def HNT Composite DO24 LP','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMH25X',50),'Msl Def HNT Composite DO25 LO','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH10',50),'MD HSV-Pgm Mgt & Bus Ops (Pgm Level)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH11',50),'MD HSV - Pgm Mgt & Bus Ops (CLIN Level)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH20',50),'MD HSV - Systems and Eng','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH25X',50),'Huntsville-Targets','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH30',50),'MD HSV - PA&SS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH40',50),'MD HSV - Mission Success','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH50',50),'MD HSV - Subcontract Mgmt','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH60',50),'MD HSV - Mission Planning','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH70',50),'MD HSV - ILS','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH80',50),'MD HSV - Launch Ops','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH90',50),'MD HSV - Operations','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHH91',50),'MD HSV - SIC Sustainment','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAA4',50),'Huntsville (AL) HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAB4',50),'Huntsville (AL) Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAC4',50),'Huntsville (AL) Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAD4',50),'Huntsville (AL) Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAE4',50),'Huntsville (AL) Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHAX4',50),'Huntsville (AL) Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHPA4',50),'Huntsville (AL) HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMHHSS',50),'MD HSV ?�� Special Studies/Task Instructions','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMRR80',50),'MD remote Launch-Ops (TDY)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA',50),'Sunnyvale (CA) Dev Mis Def HR & SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA1',50),'ATLO Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA2',50),'ENG Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA3',50),'LABS Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA4',50),'OTHER Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA5',50),'QUAL Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAA6',50),'TOUCH Sunnyvale Development Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB',50),'Sunnyvale (CA) Dev Mis Def Lvl 1 & 2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB1',50),'ATLO Sunnyvale Development Missile Defense Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB2',50),'ENG Sunnyvale Development Missile Defense Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB3',50),'LABS Sunnyvale Development Missile Defense Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB4',50),'OTHER Sunnyvale Development Missile Defense Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAB5',50),'QUAL Sunnyvale Development Missile Defense Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC',50),'Sunnyvale (CA) Dev Mis Def Lvl 3 & 4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC1',50),'ATLO Sunnyvale Development Missile DefenseLvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC2',50),'ENG Sunnyvale Development Missile DefenseLvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC3',50),'LABS Sunnyvale Development Missile DefenseLvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC4',50),'OTHER Sunnyvale Development Missile DefenseLvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAC5',50),'QUAL Sunnyvale Development Missile DefenseLvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD',50),'Sunnyvale (CA) Dev Mis Def Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD1',50),'ATLO Sunnyvale Development Missile Defense Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD2',50),'ENG Sunnyvale Development Missile Defense Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD3',50),'LABS Sunnyvale Development Missile Defense Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD4',50),'OTHER Sunnyvale Development Missile Defense Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAD5',50),'QUAL Sunnyvale Development Missile Defense Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE',50),'Sunnyvale (CA) Dev Mis Def Lvl 6 & up','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE1',50),'ATLO Sunnyvale Development Missile Defense Lvl 6 & Up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE2',50),'ENG Sunnyvale Development Missile Defense Lvl 6 & Up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE3',50),'LABS Sunnyvale Development Missile Defense Lvl 6 & Up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE4',50),'OTHER Sunnyvale Development Missile Defense Lvl 6 & Up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAE5',50),'QUAL Sunnyvale Development Missile Defense Lvl 6 & Up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX',50),'Sunnyvale (CA) Dev Mis Def Composite','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX1',50),'ATLO Sunnyvale Development Missile Defense Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX2',50),'ENG Sunnyvale Development Missile Defense Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX3',50),'LABS Sunnyvale Development Missile Defense Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX4',50),'OTHER Sunnyvale Development Missile Defense Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDAX5',50),'QUAL Sunnyvale Development Missile Defense Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA',50),'Sunnyvale (CA) Dev Mis Def HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA1',50),'ATLO Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA2',50),'ENG Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA3',50),'LABS Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA4',50),'OTHER Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA5',50),'QUAL Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSDPA6',50),'TOUCH Sunnyvale Development Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA',50),'Sunnyvale (CA) Prod Mis Def HR & SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA1',50),'ATLO Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA2',50),'ENG Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA3',50),'LABS Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA4',50),'OTHER Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA5',50),'QUAL Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAA6',50),'TOUCH Sunnyvale Production Missile DefenseHourly & NES Straight Time Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB',50),'Sunnyvale (CA) Prod Mis Def Lvl 1 & 2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB1',50),'ATLO Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB2',50),'ENG Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB3',50),'LABS Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB4',50),'OTHER Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB5',50),'QUAL Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAB6',50),'TOUCH Sunnyvale Production Missile Defense Lvl 1 & 2','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC',50),'Sunnyvale (CA) Prod Mis Def Lvl 3 & 4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC1',50),'ATLO Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC2',50),'ENG Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC3',50),'LABS Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC4',50),'OTHER Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC5',50),'QUAL Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAC6',50),'TOUCH Sunnyvale Production Missile DefenseLvl 3 & 4','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD',50),'Sunnyvale (CA) Prod Mis Def Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD1',50),'ATLO Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD2',50),'ENG Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD3',50),'LABS Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD4',50),'OTHER Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD5',50),'QUAL Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAD6',50),'TOUCH Sunnyvale Production Missile Defense Lvl 5','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE',50),'Sunnyvale (CA) Prod Mis Def Lvl 6 & up','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE1',50),'ATLO Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE2',50),'ENG Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE3',50),'LABS Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE4',50),'OTHER Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE5',50),'QUAL Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAE6',50),'TOUCH Sunnyvale Production Missile Defense Lvl 6 & Up','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX',50),'Sunnyvale (CA) Prod Mis Def Composite','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX1',50),'ATLO Sunnyvale Production Missile Defense Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX2',50),'ENG Sunnyvale Production Missile Defense Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX3',50),'LABS Sunnyvale Production Missile Defense Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX4',50),'OTHER Sunnyvale Production Missile Defense Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX5',50),'QUAL Sunnyvale Production Missile Defense Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPAX6',50),'TOUCH Sunnyvale Production Missile Defense Composite','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA',50),'Sunnyvale (CA) Prod Mis Def HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA1',50),'ATLO Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA2',50),'ENG Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA3',50),'LABS Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA4',50),'OTHER Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA5',50),'QUAL Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMSPPA6',50),'TOUCH Sunnyvale Production Missile DefenseHourly & NES Overtime Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAA4',50),'FISAC Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAB4',50),'FISAC Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAC4',50),'FISAC Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAD4',50),'FISAC Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAE4',50),'FISAC Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUAX4',50),'FISAC Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MMUUPA4',50),'FISAC Hourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAA',50),'Pre-2009 Mil Space Den Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAB',50),'Pre-2009 Mil Space Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAC',50),'Pre-2009 Mil Space Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAD',50),'Pre-2009 Mil Space Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAE',50),'Pre-2009 Mil Space Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDDAX',50),'Pre-2009 Mil Space Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAA',50),'Mil Space Den Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAB',50),'Mil Space Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAC',50),'Mil Space Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAD',50),'Mil Space Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAE',50),'Mil Space Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSDPAX',50),'Mil Space Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAA',50),'Pre-2009 Mil Space Ofste Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAB',50),'Pre-2009 Mil Space Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAC',50),'Pre-2009 Mil Space Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAD',50),'Pre-2009 Mil Space Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAE',50),'Pre-2009 Mil Space Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSRRAX',50),'Pre-2009 Mil Space Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAA',50),'Pre-2009 Mil Space Snyvle Dev Hour & SNE','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAB',50),'Pre-2009 Mil Space Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAC',50),'Pre-2009 Mil Space Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAD',50),'Pre-2009 Mil Space Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAE',50),'Pre-2009 Mil Space Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSDAX',50),'Pre-2009 Mil Space Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAA',50),'Pre-2009 Mil Space Snyvle Prod Hour & SN','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAB',50),'Pre-2009 Mil Space Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAC',50),'Pre-2009 Mil Space Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAD',50),'Pre-2009 Mil Space Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAE',50),'Pre-2009 Mil Space Snyvle Prod Lvl 6 & u','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('MSSPAX',50),'Pre-2009 Mil Space Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NG DFC Dollars',50),'NG DFC Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NGAS$',50),'Northrop Grumman Subcontract $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NGAS_Hrs',50),'Northrop Grumman Subcontract Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NGES H',50),'NGES Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NGES_H',50),'Northrop Grumman Sub Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIBK$',50),'LMTO (IS&GS) Newtown Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIBKHrs',50),'LMTO (IS&GS) Newtown Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIWI$',50),'IS&GS Defense Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIWIEHrs',50),'IS&GS Defense Engineering Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIWIFHrs',50),'IS&GS Defense Finance Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIWS$',50),'LMTO (IS&GS) Sunnyvale Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NIWSHrs',50),'LMTO (IS&GS) Sunnyvale Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NLBESCCH',50),'Autogroup NonLabor Escalation only ***','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NLBESCCHSERV',50),'Services Autogroup NonLabor Escalation only ***','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NMGRT',50),'New Mexico Gross Receipts Tax','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NPEDEV',50),'NAV Payload Support DEV CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NPEDEV-1',50),'NAV Payload Support DEV CER (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NPEDEV-4',50),'NAV Payload Support DEV CER (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NPEPRD-1',50),'NAV Payload Support PRD CER (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NPEPRD-4',50),'NAV Payload Support PRD CER (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSBH$',50),'Harris Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSULA$',50),'ULA$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSULAHrs',50),'ULA Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSWB$',50),'Boeing Sunnyvale Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSWBEHrs',50),'Boeing Sunnyvale Engineering Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSWBFHrs',50),'Boeing Sunnyvale Finance Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSWG$',50),'General Dynamics Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NSWGHrs',50),'General Dynamics Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVA',50),'Newtown Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVB',50),'Newtown Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVC',50),'Newtown Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVD',50),'Newtown Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVE',50),'Newtown Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKDEVX',50),'Newtown Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDA',50),'Newtown Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDB',50),'Newtown Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDC',50),'Newtown Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDD',50),'Newtown Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDE',50),'Newtown Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NTKPRDX',50),'Newtown Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPLBA',50),'Newtown Functional Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPLBB',50),'Newtown Functional Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPLBC',50),'Newtown Functional Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPLBD',50),'Newtown Functional Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPLBE',50),'Newtown Functional Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('NWPSLBR',50),'NewtownProc Spt CER FNKPAX','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC',50),'ODC','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC - HRS - CT',50),'Coherent Technologies ODC Hrs','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC $',50),'ODC','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC $ - CT',50),'Coherent Technologies ODC $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC CER Dev',50),'ODC CER DEV','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC CER Prod',50),'ODC CER PROD','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC CER Rmt',50),'ODC CER RMT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC Factor',50),'ODC Factor','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC Factor CER',50),'ODC Factor CER','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC Factor HEF',50),'ODC Factor HEF','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC Factor HEF SERV',50),'Services ODC Factor HEF','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC Factor S&ES',50),'ODC Factor S&ES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC SERV',50),'Services ODC','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC SERV $',50),'Services ODC','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC SERV Factor',50),'Services ODC Factor','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC$',50),'ODC $','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC/TRVL$',50),'ODCs & Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC/TRVLSERV$',50),'Services ODCs & Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC_Travel_Factor',50),'ODC / Travel Factor (Labor Hour Base)','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC_TRAVEL_FACTOR_SERV',50),'Services ODC / Travel Factor (Labor Hour Base)','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODCD',50),'Discrete ODC $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODCDSERV',50),'Services Discrete ODC $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODC-Facilities',50),'Facilities','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ODCSERV$',50),'Services ODC $','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Odd Work Week',50),'Odd Work Week','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFF_LIA',50),'Offset Liability','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFF_LIA_SERV',50),'Services Offset Liability','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTEA',50),'Offsite Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTEB',50),'Offsite Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTEC',50),'Offsite Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTED',50),'Offsite Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTEE',50),'Offsite Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('OFFSTEX',50),'Offsite Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PA&SS CER',50),'PA&SS Hours from base Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Pgm Mgmt CER',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOCERDEV',50),'PMO Newtown Development CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOCERPRD',50),'PMO Newtown Production CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMODEV',50),'PMO Development','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMODEV-01',50),'PMO Newtown Development HEF (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMODEV-04',50),'PMO Newtown Development HEF (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMODEV-16',50),'PMO Newtown Development HEF (016)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOFBM',50),'PMO Fleet Ballistic Missile','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOHNT',50),'PMO Huntsville','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBA',50),'SBIRS PMO S&NS SVL Dev HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBA2',50),'ENG SBIRS PMO Sunnyvale Development - Space Hourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBA4',50),'OTHER SBIRS PMO S&NS SVL Dev HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBB',50),'SBIRS PMO S&NS SVL Dev Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBB2',50),'ENG SBIRS PMO Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBB4',50),'OTHER SBIRS PMO S&NS SVL Dev Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBC',50),'SBIRS PMO S&NS SVL Dev Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBC2',50),'ENG SBIRS PMO Sunnyvale Development - Space Lvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBC4',50),'OTHER SBIRS PMO S&NS SVL Dev Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBD',50),'SBIRS PMO S&NS SVL Dev Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBD2',50),'ENG SBIRS PMO Sunnyvale Development - Space Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBD4',50),'OTHER SBIRS PMO S&NS SVL Dev Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBE',50),'SBIRS PMO S&NS SVL Dev Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBE2',50),'ENG SBIRS PMO Sunnyvale Development - Space Lvl 6 & up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBE4',50),'OTHER SBIRS PMO S&NS SVL Dev Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBR',50),'Program Management Labor SNSDAX','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOLBX',50),'SBIRS PMO S&NS SVL Dev Avg','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOMIC',50),'PMO Michoud','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOOFF',50),'PMO Remote','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOPRD',50),'PMO Production','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOPRD-01',50),'PMO Newtown Production HEF (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOPRD-04',50),'PMO Newtown Production HEF (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PMOPRD-16',50),'PMO Newtown Production HEF (016)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('PROG MGMT',50),'Program Management Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Relocation',50),'FMS Relocation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('RISK_FMS',50),'Risk % Rate for FMS Bid','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ROM',50),'ROM Factor','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ROM SERV',50),'Services ROM Factor','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAA4',50),'Services Tier1 MTN_South Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAB4',50),'Services Tier1 MTN_South Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAC4',50),'Services Tier1 MTN_South Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAD4',50),'Services Tier1 MTN_South Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAE4',50),'Services Tier1 MTN_South Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOAX4',50),'Services Tier1 MTN_South Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MOPA4',50),'Services Tier1 MTN_South Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAA4',50),'Services Tier1 MTN_South Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAB4',50),'Services Tier1 MTN_South Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAC4',50),'Services Tier1 MTN_South Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAD4',50),'Services Tier1 MTN_South Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAE4',50),'Services Tier1 MTN_South Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRAX4',50),'Services Tier1 MTN_South Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1MRPA4',50),'Services Tier1 MTN_South Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAA4',50),'Services Tier1 NorthEast Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAB4',50),'Services Tier1 NorthEast Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAC4',50),'Services Tier1 NorthEast Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAD4',50),'Services Tier1 NorthEast Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAE4',50),'Services Tier1 NorthEast Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOAX4',50),'Services Tier1 NorthEast Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NOPA4',50),'Services Tier1 NorthEast Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAA4',50),'Services Tier1 NorthEast Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAB4',50),'Services Tier1 NorthEast Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAC4',50),'Services Tier1 NorthEast Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAD4',50),'Services Tier1 NorthEast Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAE4',50),'Services Tier1 NorthEast Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRAX4',50),'Services Tier1 NorthEast Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1NRPA4',50),'Services Tier1 NorthEast Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAA4',50),'Services Tier1 West Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAB4',50),'Services Tier1 West Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAC4',50),'Services Tier1 West Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAD4',50),'Services Tier1 West Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAE4',50),'Services Tier1 West Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOAX4',50),'Services Tier1 West Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WOPA4',50),'Services Tier1 West Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAA4',50),'Services Tier1 West Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAB4',50),'Services Tier1 West Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAC4',50),'Services Tier1 West Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAD4',50),'Services Tier1 West Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAE4',50),'Services Tier1 West Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRAX4',50),'Services Tier1 West Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRDA4',50),'Services Tier1 West Remote Union Double Time','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1WRPA4',50),'Services Tier1 West Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S1ZRAE',50),'Services Tier 1 Program Unique Lvl 6 & up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAA4',50),'Services Tier2 MTN_South Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAB4',50),'Services Tier2 MTN_South Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAC4',50),'Services Tier2 MTN_South Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAD4',50),'Services Tier2 MTN_South Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAE4',50),'Services Tier2 MTN_South Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOAX4',50),'Services Tier2 MTN_South Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MOPA4',50),'Services Tier2 MTN_South Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAA4',50),'Services Tier2 MTN_South Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAB4',50),'Services Tier2 MTN_South Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAC4',50),'Services Tier2 MTN_South Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAD4',50),'Services Tier2 MTN_South Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAE4',50),'Services Tier2 MTN_South Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRAX4',50),'Services Tier2 MTN_South Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2MRPA4',50),'Services Tier2 MTN_South Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAA4',50),'Services Tier2 NorthEast Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAB4',50),'Services Tier2 NorthEast Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAC4',50),'Services Tier2 NorthEast Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAD4',50),'Services Tier2 NorthEast Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAE4',50),'Services Tier2 NorthEast Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOAX4',50),'Services Tier2 NorthEast Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NOPA4',50),'Services Tier2 NorthEast Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAA4',50),'Services Tier2 NorthEast Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAB4',50),'Services Tier2 NorthEast Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAC4',50),'Services Tier2 NorthEast Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAD4',50),'Services Tier2 NorthEast Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAE4',50),'Services Tier2 NorthEast Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRAX4',50),'Services Tier2 NorthEast Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2NRPA4',50),'Services Tier2 NorthEast Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAA4',50),'Services Tier2 West Onsite HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAB4',50),'Services Tier2 West Onsite Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAC4',50),'Services Tier2 West Onsite Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAD4',50),'Services Tier2 West Onsite Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAE4',50),'Services Tier2 West Onsite Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOAX4',50),'Services Tier2 West Onsite Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WOPA4',50),'Services Tier2 West Onsite HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAA4',50),'Services Tier2 West Remote HRLY NES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAB4',50),'Services Tier2 West Remote Lvl 1&2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAC4',50),'Services Tier2 West Remote Lvl 3&4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAD4',50),'Services Tier2 West Remote Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAE4',50),'Services Tier2 West Remote Lvl 6&Up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRAX4',50),'Services Tier2 West Remote Composite','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRDA4',50),'Services Tier 2 West Remote Union Double Time','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2WRPA4',50),'Services Tier2 West Remote HRLY NES OT','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2ZRAA',50),'Services Tier 2 Program Unique HR & SNES ST','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2ZRAB',50),'Services Tier 2 Program Unique Lvl 1 & 2','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2ZRAC',50),'Services Tier 2 Program Unique Lvl 3 & 4','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2ZRAD',50),'Services Tier 2 Program Unique Lvl 5','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S2ZRAE',50),'Services Tier 2 Program Unique Lvl 6 & up','Labor-Services-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SAIC $',50),'SAIC Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SALRYESC',50),'Salary Labor Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS ATLO TRV',50),'SBIRS ATLO Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS FOS TRV',50),'SBIRS FOS Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS LVI TRV',50),'SBIRS LVI Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS PAYLD TRAVEL',50),'SBIRS PAYLD Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS PM IWT CER',50),'SBIRS PM IWTA','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS PM IWT CER 1',50),'SBIRS PM IWTA','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS PM IWT CER 2',50),'SBIRS PM IWTA','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS PMO TRAVEL',50),'SBIRS PMO Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS SCM TRAVEL',50),'SBIRS SCM Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS SEIT TRAVEL',50),'SBIRS SEIT Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SBIRS SPACE TRAVEL',50),'SBIRS SPACE Travel','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SC$',50),'Subcontract Dollars $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAAS$',50),'AASC Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAASSERV$',50),'Services AASC Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCADC$',50),'Adcloe Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCADCSERV$',50),'Services Adcloe Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCADEV',50),'Subcontract Admin Support DEV CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCADEV-01',50),'Subcontract Admin Support DEV CER (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCADEV-04',50),'Subcontract Admin Support DEV CER (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAPRD',50),'Subcontract Admin Support PRD CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAPRD-01',50),'Subcontract Admin Support PRD CER (001)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAPRD-04',50),'Subcontract Admin Support PRD CER (004)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCARF$',50),'Aeroflex Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCARFSERV$',50),'Services Aeroflex Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCARO$',50),'Aerojet Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAROSERV$',50),'Services Aerojet Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAST$',50),'Astrotech Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCASTRO$',50),'Astrotech Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCASTSERV$',50),'Services Astrotech Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATK$',50),'ATK Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATK1$',50),'ATK Sub$ Propulsion Tanks','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATK2$',50),'ATK Sub$ Hinges','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATK3$',50),'ATK Sub$ Yokes/Booms','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATK4$',50),'ATK Sub$ HA-180 Hinge','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCATKSERV$',50),'Services ATK Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAXS$',50),'AXSYS Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCAXSSERV$',50),'Services AXSYS Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBAE$',50),'BAE Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBAESERV$',50),'Services BAE Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBAL$',50),'Ball Subcontractor $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBALSERV$',50),'Services Ball Subcontractor $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBEI$',50),'BEI Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCBEISERV$',50),'Services BEI Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCDA$',50),'CDA Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCDASERV$',50),'Services CDA Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCIC$',50),'CICON Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCICON$',50),'CICON Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCICSERV$',50),'Services CICON Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCMD$',50),'Comdev Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCMDSERV$',50),'Services Comdev Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCSM$',50),'CSSM Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCCSMSERV$',50),'Services CSSM Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCEAP$',50),'Eagle Picher Subcontract$','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCEAPSERV$',50),'Services Eagle Picher Subcontract$','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCEMC$',50),'Emcore Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCEMCSERV$',50),'Services Emcore Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGAD$',50),'GD AIS Subcontract $ (CSSM Applied)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGDY$',50),'GD Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGDYSERV$',50),'Services GD Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGRH$',50),'Goodrich Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGRH1$',50),'Goodrich Sub$ (ESA)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGRH2$',50),'Goodrich Sub$ (Magnetic Torgue Rod)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCGRHSERV$',50),'Services Goodrich Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHNY$',50),'Honeywell Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHNY1$',50),'Honeywell Sub$ (IMU)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHNY2$',50),'Honeywell Sub$ (Reaction Wheel Assembly)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHNY3$',50),'Honeywell Sub$ (On Board Computer)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHNYSERV$',50),'Services Honeywell Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHRS$',50),'Harris Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCHRSSERV$',50),'Services Harris Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCIHI$',50),'IHI Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCIHISERV$',50),'Services IHI Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCIMP$',50),'Imprimis Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCIMPSERV$',50),'Services Imprimis Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCINF$',50),'Infinity Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCINFSERV$',50),'Services Infinity Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCINFTY$',50),'Infinity Subcontract $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCINT$',50),'Int Rectifier Subcontracts','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCITT$',50),'ITT Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCITTSERV$',50),'Services ITT Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCL3C$',50),'L3 Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCL3CSERV$',50),'Services L3 Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCLAB$',50),'Labinal Subcontracts $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCLMGI$',50),'LMGI Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCLMGSERV$',50),'Services LMGI Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBA',50),'SBIRS SCM S&NS SVL Dev HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBA4',50),'OTHER SBIRS SCM S&NS SVL Dev HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBB',50),'SBIRS SCM S&NS SVL Dev Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBB4',50),'OTHER SBIRS SCM S&NS SVL Dev Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBC',50),'SBIRS SCM S&NS SVL Dev Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBC4',50),'OTHER SBIRS SCM S&NS SVL Dev Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBD',50),'SBIRS SCM S&NS SVL Dev Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBD4',50),'OTHER SBIRS SCM S&NS SVL Dev Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBE',50),'SBIRS SCM S&NS SVL Dev Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBE4',50),'OTHER SBIRS SCM S&NS SVL Dev Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBR',50),'Subcontract Management Labor SNSDAX','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMLBX',50),'SBIRS SCM S&NS SVL Dev Avg','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMOG$',50),'Moog Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCMOGSERV$',50),'Services Moog Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNEA$',50),'NEA Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNEASERV$',50),'Services NEA Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGA$',50),'Northrop Grumman Aero Sub $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGAH',50),'Northrop Grumman Aero Sub Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGASERV$',50),'Services Northrop Grumman Aero Sub $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGE$',50),'Northrop Grumman Electronics Sub $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGEH',50),'Northrop Grumman Sub Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGES$',50),'Northrop Grumman Woodland Hills Subcon','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGESERV$',50),'Services Northrop Grumman Electronics Sub $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGR$',50),'Northrop Grumman Sub $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCNGRSERV$',50),'Services Northrop Grumman Sub $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCPAC$',50),'Pacific Scientific Sub$','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCPACSERV$',50),'Services Pacific Scientific Sub$','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCRAY$',50),'Raytheon Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCRAYSERV$',50),'Services Raytheon Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSAF$',50),'SAFT Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSAFSERV$',50),'Services SAFT Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSAFT$',50),'SAFT Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSEK$',50),'Seakr Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSEKSERV$',50),'Services Seakr Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSM&A',50),'SM&A','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSMA$',50),'SM&A Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSMASERV$',50),'Services SM&A Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSMU$',50),'Stan Mu Subcontract $','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSNS$',50),'Sierra Nevada Subcontract Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSNV$',50),'Sierra Nevada Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSNVSERV$',50),'Services Sierra Nevada Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSPD$',50),'Space Dev Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSPDSERV$',50),'Services Space Dev Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSPL$',50),'Spectrolab Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCSPLSERV$',50),'Services Spectrolab Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCTAV$',50),'TAVIS Subcontract$','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCTIM$',50),'Timkin Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCTIMSERV$',50),'Services Timkin Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCULA$',50),'ULA Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCULASERV$',50),'Services ULA Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCUSA$',50),'United Space Allianc Sub$','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCUSASERV$',50),'Services United Space Allianc Sub$','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCUTC$',50),'UTC Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCUTCSERV$',50),'Services UTC Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCVAC$',50),'Subcontract VACCO','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCVAN$',50),'Vanguard Subcontract $','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SCVANSERV$',50),'Services Vanguard Subcontract $','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SDPA',50),'Sunnyvale (CA) Development HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Second Shift Premium',50),'Second Shift Premium','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAA',50),'Pre-2009 Human Space Flight Den Dev Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAB',50),'Pre-2009 Human Space Flight Den Dev Lvl','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAC',50),'Pre-2009 Human Space Flight Den Dev Lvl','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAD',50),'Pre-2009 Human Space Flight Den Dev Lvl','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAE',50),'Pre-2009 Human Space Flight Den Dev Lvl','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SEDDAX',50),'Pre-2009 Human Space Flight Den Dev Comp','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAA',50),'Pre-2009 Human Space Flight Ofste Hour &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAB',50),'Pre-2009 Human Space Flight Ofste Lvl 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAC',50),'Pre-2009 Human Space Flight Ofste Lvl 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAD',50),'Pre-2009 Human Space Flight Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAE',50),'Pre-2009 Human Space Flight Ofste Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SERRAX',50),'Pre-2009 Human Space Flight Ofste Compos','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Serv Insurance Fee',50),'Services Fee on Insurance','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Serv Material Esc',50),'Services Autogroup Material  Escalation only ***','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services DFC Dollars',50),'Services DFC Dollars','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Facilities',50),'Services Facilities','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor $ Base $ Derivative G&A only Burdens',50),'Services Factor $ Base $ Derivative G&A only Burdens','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor $ Base $ Derivative with Burdens',50),'Services Factor $ Base $ Derivative with Burdens','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor $ Base Hrs OnsiteT1 Pool Derivative',50),'Services Factor $ Base Hrs OnsiteT1 Pool Derivative','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor Hrs Base $ Derivative G&A only Burdens',50),'Services Factor Hrs Base $ Derivative G&A only Burdens','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor Hrs Base $ Derivative with Burdens',50),'Services Factor Hrs Base $ Derivative with Burdens','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Factor Hrs Base Hrs OnsiteT1 Pool Derivative',50),'Services Factor Hrs Base Hrs OnsiteT1 Pool Derivative','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Insurance',50),'Services Launch Insurance','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Material Factor',50),'Services Material Factor','Material-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Shift Premium',50),'Services Shift Premium','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Services Sub DFC Dollars',50),'Services Sub DFC Dollars','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('S-IMP',50),'Imprimis','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMAJC',50),'Major S/C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMAJCSERV',50),'Services Major S/C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMAJF',50),'IWTA_FCCM','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMAJH',50),'Major S/C','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAA',50),'Strategic Msls FBM ER HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAB',50),'Strategic Msls FBM ER Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAC',50),'Strategic Msls FBM ER Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAD',50),'Strategic Msls FBM ER Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAE',50),'Strategic Msls FBM ER Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFAX',50),'Strategic Missiles FBM ER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBMA',50),'SMEF Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBMB',50),'SMEF Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBMC',50),'SMEF Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBMD',50),'SMEF Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBME',50),'SMEF Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFBMX',50),'SMEF Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMEFPA',50),'Strategic Msls FBM ER HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAA',50),'Strategic Msls SWFLANT HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAB',50),'Strategic Msls SWFLANT Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAC',50),'Strategic Msls SWFLANT Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAD',50),'Strategic Msls SWFLANT Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAE',50),'Strategic Msls SWFLANT Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFAX',50),'Strategic Msls SWFLANT Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMGFPA',50),'Strategic Msls SWFLANT HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMINC',50),'Minor S/C Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMINCSERV',50),'Services Minor S/C Dollars','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMNH',50),'Minor S/C Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAA',50),'Strategic Msls Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAB',50),'Strategic Msls Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAC',50),'Strategic Msls Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAD',50),'Strategic Msls Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAE',50),'Strategic Msls Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRAX',50),'Strategic Msls Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMRRPA',50),'Strategic Msls Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAA',50),'Strategic Msls Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAB',50),'Strategic Msls Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAC',50),'Strategic Msls Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAD',50),'Strategic Msls Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAE',50),'Strategic Msls Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDAX',50),'Strategic Missiles Sunnyvale Dev','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSDPA',50),'Strategic Msls Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAA',50),'Strategic Msls Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAB',50),'Strategic Msls Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAC',50),'Strategic Msls Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAD',50),'Strategic Msls Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAE',50),'Strategic Msls Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPAX',50),'Strategic Msls Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMSPPA',50),'Strategic Msls Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAA',50),'Strategic Msls V F Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAB',50),'Strategic Msls Valley Forge Dev Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAC',50),'Strategic Msls Valley Forge Dev Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAD',50),'Strategic Msls Valley Forge Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAE',50),'Strategic Msls Valley Forge Dev Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDAX',50),'Strategic Msls Valley Forge Dev Composit','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVDPA',50),'Strategic Msls V F Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAA',50),'Strategic Msls V F Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAB',50),'Strategic Msls Valley Forge Prod Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAC',50),'Strategic Msls Valley Forge Prod Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAD',50),'Strategic Msls Valley Forge Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAE',50),'Strategic Msls Valley Forge Prod Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPAX',50),'Strategic Msls Valley Forge Prod Composi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMVPPA',50),'Strategic Msls V F Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAA',50),'Strategic Msls SWFPAC HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAB',50),'Strategic Msls SWFPAC Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAC',50),'Strategic Msls SWFPAC Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAD',50),'Strategic Msls SWFPAC Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAE',50),'Strategic Msls SWFPAC Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFAX',50),'Strategic Msls SWFPAC Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SMWFPA',50),'Strategic Msls SWFPAC HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNALLO-C3/4r1',50),'SNS Allocation C (L3/4)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNALLO-D5r1',50),'SNS Allocation D (L5)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAA',50),'Surv & Nav Sys Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAB',50),'Surv & Nav Sys Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAC',50),'Surv & Nav Sys Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAD',50),'Surv & Nav Sys Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAE',50),'Surv & Nav Sys Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDAX',50),'Surv & Nav Sys Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDLB',50),'Surveillance & Navigation Systems Denver','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDNL',50),'Surveillance & Navigation Systems Denver','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDPA',50),'Surv & Nav Sys Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNDDPP',50),'SNDD Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAA',50),'SNS  Newtown/VF Dev Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAB',50),'SNS  Newtown/VF Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAC',50),'SNS  Newtown/VF Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAD',50),'SNS  Newtown/VF Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAE',50),'SNS  Newtown/VF Dev Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKDAX',50),'SNS  Newtown/VF Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAA',50),'SNS  Newtown/VF Prod Hourly & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAB',50),'SNS  Newtown/VF Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAC',50),'SNS  Newtown/VF Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAD',50),'SNS  Newtown/VF Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAE',50),'SNS  Newtown/VF Prod Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNKPAX',50),'SNS  Newtown/VF Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRR18',50),'Surv & Nav Sys Ofste CLIN 18','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRR19',50),'Surv & Nav Sys Ofste CLIN 19','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRR20',50),'Surv & Nav Sys Ofste CLIN 20','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAA',50),'Surv & Nav Sys Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAB',50),'Surv & Nav Sys Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAC',50),'Surv & Nav Sys Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAD',50),'Surv & Nav Sys Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAE',50),'Surv & Nav Sys Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRAX',50),'Surv & Nav Sys Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRLB',50),'Surveillance & Navigation Systems Remote','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRNL',50),'Surveillance & Navigation Systems Remote','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNRRPA',50),'Surv & Nav Sys Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAA',50),'Surv & Nav Sys Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAB',50),'Surv & Nav Sys Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAC',50),'Surv & Nav Sys Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAD',50),'Surv & Nav Sys Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAE',50),'Surv & Nav Sys Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDAX',50),'Surv & Nav Sys Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDLB',50),'Surveillance & Navigation Systems Sunnyv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDNL',50),'Surveillance & Navigation Systems Sunnyv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSDPA',50),'Surv & Nav Sys Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAA',50),'Surv & Nav Sys Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAB',50),'Surv & Nav Sys Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAC',50),'Surv & Nav Sys Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAD',50),'Surv & Nav Sys Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAE',50),'Surv & Nav Sys Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPAX',50),'Surv & Nav Sys Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPLB',50),'Surveillance & Navigation Systems Sunnyv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPNL',50),'Surveillance & Navigation Systems Sunnyv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SNSPPA',50),'Surv & Nav Sys Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_Manufacturing',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_MATL$',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_MATL_SERV$',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_SUB1_D',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.1 Bus Ops',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.2 Sys Engrg',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.3 S/C  Engrg',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.3.10 Flight S/W',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.4 AI&T',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SP_x.5 Quality Engrg',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAA',50),'Spcl Prgms Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAB',50),'Spcl Prgms Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAC',50),'Spcl Prgms Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAD',50),'Spcl Prgms Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAE',50),'Spcl Prgms Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDAX',50),'Spcl Prgms Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDLB',50),'Special Programs Denver Dev Svc Ctr Labo','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDNL',50),'Special Programs Denver Dev Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDPA',50),'Spcl Prgms Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDDPP',50),'SPDD Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAA',50),'Spcl Prgms Den Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAB',50),'Spcl Prgms Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAC',50),'Spcl Prgms Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAD',50),'Spcl Prgms Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAE',50),'Spcl Prgms Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPAX',50),'Spcl Prgms Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPDPPA',50),'Spcl Prgms Den Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Special Travel EDY',50),'FMS - Special Travel EDY','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAA',50),'Spcl Prgms Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAB',50),'Spcl Prgms Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAC',50),'Spcl Prgms Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAD',50),'Spcl Prgms Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAE',50),'Spcl Prgms Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRAX',50),'Spcl Prgms Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPRRPA',50),'Spcl Prgms Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAA',50),'Spcl Prgms Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAB',50),'Spcl Prgms Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAC',50),'Spcl Prgms Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAD',50),'Spcl Prgms Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAE',50),'Spcl Prgms Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDAX',50),'Spcl Prgms Snyvle Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSDPA',50),'Spcl Prgms Snyvle Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAA',50),'Spcl Prgms Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAB',50),'Spcl Prgms Snyvle Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAC',50),'Spcl Prgms Snyvle Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAD',50),'Spcl Prgms Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAE',50),'Spcl Prgms Snyvle Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPAX',50),'Spcl Prgms Snyvle Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPSPPA',50),'Spcl Prgms Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVA',50),'Support Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVB',50),'Support Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVC',50),'Support Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVD',50),'Support Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVE',50),'Support Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTDEVX',50),'Support Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTA',50),'Support Huntsville Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTB',50),'Support Huntsville Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTC',50),'Support Huntsville Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTD',50),'Support Huntsville Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTE',50),'Support Huntsville Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTHNTX',50),'Support Huntsville Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICA',50),'Support Michoud Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICB',50),'Support Michoud Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICC',50),'Support Michoud Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICD',50),'Support Michoud Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICE',50),'Support Michoud Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTMICX',50),'Support Michoud Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDA',50),'Support Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDB',50),'Support Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDC',50),'Support Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDD',50),'Support Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDE',50),'Support Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SPTPRDX',50),'Support Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0001',50),'SSC Lbr Hr Fee 0001 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0002',50),'SSC Lbr Hr Fee 0002 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0003',50),'SSC Lbr Hr Fee 0003 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0004',50),'SSC Lbr Hr Fee 0004 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0005',50),'SSC Lbr Hr Fee 0005 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0006',50),'SSC Lbr Hr Fee 0006 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0007',50),'SSC Lbr Hr Fee 0007 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0008',50),'SSC Lbr Hr Fee 0008 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0009',50),'SSC Lbr Hr Fee 0009 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0010',50),'SSC Lbr Hr Fee 0010 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hr Fee 0011',50),'SSC Lbr Hr Fee 0011 - Use for Fee per Hour','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0001',50),'SSC Lbr Hrs 0001 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0002',50),'SSC Lbr Hrs 0002 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0003',50),'SSC Lbr Hrs 0003 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0004',50),'SSC Lbr Hrs 0004 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0005',50),'SSC Lbr Hrs 0005 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0006',50),'SSC Lbr Hrs 0006 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0007',50),'SSC Lbr Hrs 0007 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSC Lbr Hrs 0008',50),'SSC Lbr Hrs 0008 - No burdens applied','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAA',50),'Sensing & Expl Den Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAB',50),'Sensing & Exploration Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAC',50),'Sensing & Exploration Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAD',50),'Sensing & Exploration Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAE',50),'Sensing & Exploration Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDAX',50),'Sensing & Exploration Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDLB',50),'Sensing & Exploration Systems Denver Dev','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDNL',50),'Sensing & Exploration Systems Denver Dev','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSDDPA',50),'Sensing & Expl Den Dev HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAA',50),'Sensing & Exploration Ofste HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAB',50),'Sensing & Exploration Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAC',50),'Sensing & Exploration Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAD',50),'Sensing & Exploration Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAE',50),'Sensing & Exploration Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRAX',50),'Sensing & Exploration Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRLB',50),'Sensing & Exploration Systems Offsite Sv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRNL',50),'Sensing & Exploration Systems Offsite Sv','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSRRPA',50),'Sensing & Exploration Ofste HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAA',50),'Sensing & Expl Snyvle HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAB',50),'Sensing & Exploration Snyvle Dev Lvl 1 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAC',50),'Sensing & Exploration Snyvle Dev Lvl 3 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAD',50),'Sensing & Exploration Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAE',50),'Sensing & Exploration Snyvle Dev Lvl 6 &','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDAX',50),'Sensing & Exploration Snyvle Dev Composi','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDLB',50),'Sensing & Exploration Systems Sunnyvale','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDNL',50),'Sensing & Exploration Systems Sunnyvale','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSDPA',50),'Sensing & Expl Snyvle HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAA',50),'Sensing & Expl Snyvle Prod HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAB',50),'Sensing & Exploration Snyvle Prod Lvl 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAC',50),'Sensing & Exploration Snyvle Prod Lvl 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAD',50),'Sensing & Exploration Snyvle Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAE',50),'Sensing & Exploration Snyvle Prod Lvl 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPAX',50),'Sensing & Exploration Snyvle Prod Compos','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SSSPPA',50),'Sensing & Expl Snyvle Prod HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAA',50),'Space Trans VAFB Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAB',50),'Space Trans VAFB Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAC',50),'Space Trans VAFB Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAD',50),'Space Trans VAFB Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAE',50),'Space Trans VAFB Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STCLAX',50),'Space Trans VAFB Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAA',50),'Space Trans Den Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAB',50),'Space Trans Den Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAC',50),'Space Trans Den Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAD',50),'Space Trans Den Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAE',50),'Space Trans Den Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDDAX',50),'Space Trans Den Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAA',50),'Space Trans Den Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAB',50),'Space Trans Den Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAC',50),'Space Trans Den Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAD',50),'Space Trans Den Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAE',50),'Space Trans Den Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STDPAX',50),'Space Trans Den Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAA',50),'Space Trans CAPE Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAB',50),'Space Trans CAPE Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAC',50),'Space Trans CAPE Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAD',50),'Space Trans CAPE Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAE',50),'Space Trans CAPE Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STFLAX',50),'Space Trans CAPE Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVA',50),'Stennis Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVB',50),'Stennis Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVC',50),'Stennis Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVD',50),'Stennis Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVE',50),'Stennis Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMDEVX',50),'Stennis Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDA',50),'Stennis Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDB',50),'Stennis Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDC',50),'Stennis Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDD',50),'Stennis Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDE',50),'Stennis Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STMPRDX',50),'Stennis Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAA',50),'Space Trans San Diego Dev Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAB',50),'Space Trans San Diego Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAC',50),'Space Trans San Diego Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAD',50),'Space Trans San Diego Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAE',50),'Space Trans San Diego Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNDAX',50),'Space Trans San Diego Dev Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAA',50),'Space Trans San Diego Prod Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAB',50),'Space Trans San Diego Prod Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAC',50),'Space Trans San Diego Prod Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAD',50),'Space Trans San Diego Prod Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAE',50),'Space Trans San Diego Prod Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STNPAX',50),'Space Trans San Diego Prod Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAA',50),'Space Trans Ofste Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAB',50),'Space Trans Ofste Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAC',50),'Space Trans Ofste Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAD',50),'Space Trans Ofste Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAE',50),'Space Trans Ofste Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('STRRAX',50),'Space Trans Ofste Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub DFC Dollars',50),'Sub DFC Dollars','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub Hrs No Burd 0001',50),'Sub Hrs 0001','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub Hrs No Burd 0002',50),'Sub Hrs 0002','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub Hrs No Burd 0003',50),'Sub Hrs 0003','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub Hrs No Burd 0004',50),'Sub Hrs 0004','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub Hrs No Burd 0005',50),'Sub Hrs 0005','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$1',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$10',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$11',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$12',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$13',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$14',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$15',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$16',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$17',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$18',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$19',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$2',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$20',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$21',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$22',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$23',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$24',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$25',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$26',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$27',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$28',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$29',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$3',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$30',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$31',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$32',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$33',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$34',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$35',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$36',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$37',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$38',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$39',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$4',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$40',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$41',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$42',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$43',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$44',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$45',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$46',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$47',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$48',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$49',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$5',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$50',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$6',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$7',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$8',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB$9',50),'Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Sub$C',50),'Critical Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB01',50),'Subcontractor 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB10',50),'Subcontractor 10','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB11',50),'Subcontractor 11','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB12',50),'Subcontractor 12','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB13',50),'Subcontractor 13','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB16',50),'Subcontractor 16','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB17',50),'Subcontractor 17','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB18',50),'Subcontractor 18','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB2',50),'Subcontractor 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB3',50),'Subcontractor 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB4',50),'Subcontractor 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB5',50),'Subcontractor 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB6',50),'Subcontractor 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB7',50),'Subcontractor 7','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB8',50),'Subcontractor 8','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUB9',50),'Subcontractor 9','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBESC',50),'Subcontract Escalation','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH1',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH10',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH11',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH12',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH13',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH14',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH15',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH16',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH17',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH18',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH19',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH2',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH20',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH21',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH22',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH23',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH24',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH25',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH26',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH27',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH28',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH29',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH3',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH30',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH4',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH5',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH6',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH7',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH8',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBH9',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SubHrs',50),'Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$1',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$10',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$100',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$11',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$12',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$13',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$14',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$15',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$16',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$17',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$18',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$19',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$2',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$20',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$21',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$22',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$23',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$24',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$25',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$26',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$27',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$28',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$29',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$3',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$30',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$31',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$32',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$33',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$34',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$35',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$36',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$37',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$38',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$39',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$4',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$40',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$41',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$42',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$43',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$44',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$45',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$46',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$47',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$48',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$49',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$5',50),'Services Subcontracts','Subcontractor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$50',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$51',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$52',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$53',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$54',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$55',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$56',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$57',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$58',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$59',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$6',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$60',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$61',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$62',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$63',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$64',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$65',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$66',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$67',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$68',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$69',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$7',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$70',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$71',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$72',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$73',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$74',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$75',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$76',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$77',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$78',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$79',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$8',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$80',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$81',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$82',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$83',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$84',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$85',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$86',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$87',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$88',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$89',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$9',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$90',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$91',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$92',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$93',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$94',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$95',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$96',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$97',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$98',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$99',50),'Services Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV$C',50),'Services Critical Subcontracts','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV1',50),'Services Subcontractor 1','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV10',50),'Services Subcontractor 10','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV11',50),'Services Subcontractor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV12',50),'Services Subcontractor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV13',50),'Services Subcontractor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV16',50),'Services Subcontractor 16','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV17',50),'Services Subcontractor 17','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV18',50),'Services Subcontractor 18','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV2',50),'Services Subcontractor 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV3',50),'Services Subcontractor 3','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV4',50),'Services Subcontractor 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV5',50),'Services Subcontractor 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV6',50),'Services Subcontractor 6','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV7',50),'Services Subcontractor 7','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV8',50),'Services Subcontractor 8','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERV9',50),'Services Subcontractor 9','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVESC',50),'Services Subcontract Escalation','Subcontractor-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH1',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH10',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH100',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH11',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH12',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH13',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH14',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH15',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH16',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH17',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH18',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH19',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH2',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH20',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH21',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH22',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH23',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH24',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH25',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH26',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH27',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH28',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH29',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH3',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH30',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH31',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH32',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH33',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH34',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH35',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH36',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH37',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH38',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH39',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH4',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH40',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH41',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH42',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH43',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH44',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH45',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH46',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH47',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH48',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH49',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH5',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH50',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH51',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH52',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH53',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH54',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH55',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH56',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH57',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH58',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH59',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH6',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH60',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH61',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH62',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH63',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH64',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH65',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH66',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH67',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH68',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH69',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH7',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH70',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH71',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH72',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH73',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH74',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH75',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH76',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH77',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH78',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH79',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH8',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH80',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH81',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH82',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH83',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH84',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH85',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH86',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH87',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH88',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH89',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH9',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH90',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH91',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH92',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH93',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH94',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH95',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH96',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH97',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH98',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVH99',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SUBSERVHRS',50),'Services Subcontract Hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVA',50),'Sunnyvale Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVB',50),'Sunnyvale Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVC',50),'Sunnyvale Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVD',50),'Sunnyvale Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVE',50),'Sunnyvale Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLDEVX',50),'Sunnyvale Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDA',50),'Sunnyvale Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDB',50),'Sunnyvale Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDC',50),'Sunnyvale Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDD',50),'Sunnyvale Prd Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDE',50),'Sunnyvale Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVLPRDX',50),'Sunnyvale Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPLBA',50),'Funct Snyvle Dev HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPLBB',50),'Funct Snyvle Dev Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPLBC',50),'Funct Snyvle Dev Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPLBD',50),'Funct Snyvle Dev Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPLBE',50),'Funct Snyvle Dev Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SVPSLBR',50),'Snyvle Proc Supt CER SNSDAX','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTA',50),'SWFLANT Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTB',50),'SWFLANT Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTC',50),'SWFLANT Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTD',50),'SWFLANT Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTE',50),'SWFLANT Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFLNTX',50),'SWFLANT Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACA',50),'SWFPAC Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACB',50),'SWFPAC Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACC',50),'SWFPAC Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACD',50),'SWFPAC Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACE',50),'SWFPAC Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SWFPACX',50),'SWFPAC Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('SYST ENG FACTOR',50),'Systems Engineering Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MDEVLB',50),'T&M Development Management Service Center LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MDEVNL',50),'T&M Development Management Service Center NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MGD',50),'GD T&M','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MITT',50),'ITT T&M','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MMICHOUDLB',50),'T&M Michoud Management Service Center LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MMICHOUDNL',50),'T&M Michoud Management Service Center NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MREMOTELB',50),'T&M Remote Management Service Center LB','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('T&MREMOTENL',50),'T&M Remote Management Service Center NL','Other-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TAR 541959',50),'Targets Huntsville MSC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TAR 541972',50),'Targets SMD MSC Special','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Targets ODC',50),'Targets ODC Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Targets Travel',50),'Targets Travel Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TCI Base PDSP',50),'TCI Base PDSP','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TDM BUS OPS',50),'Business Ops CER','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TDY$',50),'Travel TDY $','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TDYSERV$',50),'Services Travel TDY $','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Third Shift Premium',50),'Third Shift Premium','Other-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAV$',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL',50),'TRAVEL $','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel (In Country)',50),'FMS Travel Rate','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL -ATLO',50),'CER for Travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL CER RATE',50),'CER for Travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Discrete',50),'','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Escalation',50),'Autogroup Travel Escalation only ***','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor HEF',50),'Travel Factor HEF','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL FACTOR HEF-11',50),'Travel Factor HEF (CLIN 011)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL FACTOR HEF-16',50),'Travel Factor HEF (CLIN 016)','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor1',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor2',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor3',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor4',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Factor5',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL HEF',50),'Travel Factor HEF','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL -SEIT',50),'CER for Travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL SERV',50),'Services Travel $','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Discrete',50),'Services Discrete Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Esc',50),'Services Autogroup Travel Escalation only ***','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel Serv Factor HEF',50),'Services Travel Factor HEF','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor1',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor2',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor3',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor4',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV Factor5',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel SERV HEF',50),'Services Travel Factor HEF','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL -SV',50),'CER for Travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel$',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL-BOPS',50),'CER for Travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVEL-PM',50),'cer for travel based on labor hours','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVELSERV$',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('Travel-Targets',50),'Targets Travel Factor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVLESC',50),'Travel Escalation','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVLSERVESC',50),'Services Travel Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRAVSERV$',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRVL ESC',50),'Travel Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRVL SERV ESC',50),'Services Travel Escalation','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRVL$',50),'Travel','Travel-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('TRVLSERV$',50),'Services Travel','Travel-Services');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ULA Metrology LB CER',50),'ULA Metrology CER-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ULA Metrology NL CER',50),'ULA Metrology CER-Non-Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVA',50),'Valley Forge Dev Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVB',50),'Valley Forge Dev Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVC',50),'Valley Forge Dev Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVD',50),'Valley Forge Dev Lvl D 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVE',50),'Valley Forge Dev Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVDEVX',50),'Valley Forge Dev Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVPRDA',50),'Valley Forge Prd Lvl A Hour & NES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVPRDB',50),'Valley Forge Prd Lvl B 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVPRDC',50),'Valley Forge Prd Lvl C 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVPRDE',50),'Valley Forge Prd Lvl E 6+','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('VFVPRDX',50),'Valley Forge Prd Lvl X Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA',50),'Denver (CO) Development ATC HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA1',50),'ATLO Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA2',50),'ENG Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA3',50),'LABS Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA4',50),'OTHER Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA5',50),'QUAL Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAA6',50),'TOUCH Denver (CO) Development ATC HR & SNES ST','Labor-Core-ATC-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB',50),'Denver (CO) Development ATC Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB1',50),'ATLO Denver (CO) Development ATC Lvl 1 & 2','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB2',50),'ENG Denver (CO) Development ATC Lvl 1 & 2','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB3',50),'LABS Denver (CO) Development ATC Lvl 1 & 2','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB4',50),'OTHER Denver (CO) Development ATC Lvl 1 & 2','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAB5',50),'QUAL Denver (CO) Development ATC Lvl 1 & 2','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC',50),'Denver (CO) Development ATC Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC1',50),'ATLO Denver (CO) Development ATC Lvl 3 & 4','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC2',50),'ENG Denver (CO) Development ATC Lvl 3 & 4','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC3',50),'LABS Denver (CO) Development ATC Lvl 3 & 4','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC4',50),'OTHER Denver (CO) Development ATC Lvl 3 & 4','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAC5',50),'QUAL Denver (CO) Development ATC Lvl 3 & 4','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD',50),'Denver (CO) Development ATC Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD1',50),'ATLO Denver (CO) Development ATC Lvl 5','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD2',50),'ENG Denver (CO) Development ATC Lvl 5','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD3',50),'LABS Denver (CO) Development ATC Lvl 5','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD4',50),'OTHER Denver (CO) Development ATC Lvl 5','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAD5',50),'QUAL Denver (CO) Development ATC Lvl 5','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE',50),'Denver (CO) Development ATC Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE1',50),'ATLO Denver (CO) Development ATC Lvl 6 & up','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE2',50),'ENG Denver (CO) Development ATC Lvl 6 & up','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE3',50),'LABS Denver (CO) Development ATC Lvl 6 & up','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE4',50),'OTHER Denver (CO) Development ATC Lvl 6 & up','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAE5',50),'QUAL Denver (CO) Development ATC Lvl 6 & up','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX',50),'Denver (CO) Development ATC Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX1',50),'ATLO-Denver (CO) Development ATC Composite','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX2',50),'ENG-Denver (CO) Development ATC Composite','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX3',50),'LABS-Denver (CO) Development ATC Composite','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX4',50),'OTHER-Denver (CO) Development ATC Composite','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDAX5',50),'QUAL-Denver (CO) Development ATC Composite','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA',50),'Denver (CO) Development ATC HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA1',50),'ATLO Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA2',50),'ENG Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA3',50),'LABS Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA4',50),'OTHER Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA5',50),'QUAL Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XADDPA6',50),'TOUCH Denver (CO) Development ATC HR & SNES OT','Labor-Core-ATC-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA',50),'Support Development (CA) HR & SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA1',50),'ATLO Support Development (CA) HR & SNES ST','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA2',50),'ENG Support Development (CA) HR & SNES ST','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA3',50),'LABS Support Development (CA) HR & SNES ST','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA4',50),'OTHER Support Development (CA) HR & SNES ST','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA5',50),'QUAL Support Development (CA) HR & SNES ST','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAA6',50),'TOUCH Support Development (CA) HR & SNES ST','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB',50),'Support Development (CA) Lvl 1 & 2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB1',50),'ATLO Support Development (CA) Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB2',50),'ENG Support Development (CA) Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB3',50),'LABS Support Development (CA) Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB4',50),'OTHER Support Development (CA) Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB5',50),'QUAL Support Development (CA) Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAB6',50),'TOUCH Support Development (CA) Lvl 1 & 2','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC',50),'Support Development (CA) Lvl 3 & 4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC1',50),'ATLO Support Development (CA) Lvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC2',50),'ENG Support Development (CA) Lvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC3',50),'LABS Support Development (CA) Lvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC4',50),'OTHER Support Development (CA) Lvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAC5',50),'QUAL Support Development (CA) Lvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD',50),'Support Development (CA) Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD1',50),'ATLO Support Development (CA) Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD2',50),'ENG Support Development (CA) Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD3',50),'LABS Support Development (CA) Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD4',50),'OTHER Support Development (CA) Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAD5',50),'QUAL Support Development (CA) Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE',50),'Support Development (CA) Lvl 6 & up','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE1',50),'ATLO Support Development (CA) Lvl 6 & up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE2',50),'ENG Support Development (CA) Lvl 6 & up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE3',50),'LABS Support Development (CA) Lvl 6 & up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE4',50),'OTHER Support Development (CA) Lvl 6 & up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAE5',50),'QUAL Support Development (CA) Lvl 6 & up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX',50),'Support Development (CA) Composite','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX1',50),'ATLO Support Development (CA) Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX2',50),'ENG Support Development (CA) Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX3',50),'LABS Support Development (CA) Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX4',50),'OTHER Support Development (CA) Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDAX5',50),'QUAL Support Development (CA) Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA',50),'Support Development (CA) HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA1',50),'ATLO Support Development (CA) HR & SNES OT','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA2',50),'ENG Support Development (CA) HR & SNES OT','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA3',50),'LABS Support Development (CA) HR & SNES OT','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA4',50),'OTHER Support Development (CA) HR & SNES OT','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA5',50),'QUAL Support Development (CA) HR & SNES OT','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZDPA6',50),'TOUCH Support Development (CA) HR & SNES OT','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA',50),'Support Production (CA) HR & SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA1',50),'ATLO Support Production (CA) HR & SNES ST','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA2',50),'ENG Support Production (CA) HR & SNES ST','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA3',50),'LABS Support Production (CA) HR & SNES ST','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA4',50),'OTHER Support Production (CA) HR & SNES ST','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA5',50),'QUAL Support Production (CA) HR & SNES ST','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAA6',50),'TOUCH Support Production (CA) HR & SNES ST','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB',50),'Support Production (CA) Lvl 1 & 2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB1',50),'ATLO Support Production (CA) Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB2',50),'ENG Support Production (CA) Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB3',50),'LABS Support Production (CA) Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB4',50),'OTHER Support Production (CA) Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAB5',50),'QUAL Support Production (CA) Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC',50),'Support Production (CA) Lvl 3 & 4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC1',50),'ATLO Support Production (CA) Lvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC2',50),'ENG Support Production (CA) Lvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC3',50),'LABS Support Production (CA) Lvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC4',50),'OTHER Support Production (CA) Lvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAC5',50),'QUAL Support Production (CA) Lvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD',50),'Support Production (CA) Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD1',50),'ATLO Support Production (CA) Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD2',50),'ENG Support Production (CA) Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD3',50),'LABS Support Production (CA) Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD4',50),'OTHER Support Production (CA) Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAD5',50),'QUAL Support Production (CA) Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE',50),'Support Production (CA) Lvl 6 & up','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE1',50),'ATLO Support Production (CA) Lvl 6 & up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE2',50),'ENG Support Production (CA) Lvl 6 & up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE3',50),'LABS Support Production (CA) Lvl 6 & up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE4',50),'OTHER Support Production (CA) Lvl 6 & up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAE5',50),'QUAL Support Production (CA) Lvl 6 & up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX',50),'Support Production (CA) Composite','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX1',50),'ATLO Support Production (CA) Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX2',50),'ENG Support Production (CA) Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX3',50),'LABS Support Production (CA) Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX4',50),'OTHER Support Production (CA) Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPAX5',50),'QUAL Support Production (CA) Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA',50),'Support Production (CA) HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA1',50),'ATLO Support Production (CA) HR & SNES OT','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA2',50),'ENG Support Production (CA) HR & SNES OT','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA3',50),'LABS Support Production (CA) HR & SNES OT','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA4',50),'OTHER Support Production (CA) HR & SNES OT','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA5',50),'QUAL Support Production (CA) HR & SNES OT','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XCZPPA6',50),'TOUCH Support Production (CA) HR & SNES OT','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAA4',50),'Michoud (LA) MFG HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAB4',50),'Michoud (LA) MFG Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAC4',50),'Michoud (LA) MFG  Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAD4',50),'Michoud (LA) MFG Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAE4',50),'Michoud (LA) MFG Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMAX4',50),'Michoud (LA) MFG Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XMLMPA4',50),'Michoud (LA) MFG HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA',50),'Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA1',50),'ATLO Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA2',50),'ENG Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA3',50),'LABS Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA4',50),'OTHER Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA5',50),'QUAL Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAA6',50),'TOUCH Palo Alto (CA) Development HR & SNES ST','Labor-Core-ATC-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB',50),'Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB1',50),'ATLO Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB2',50),'ENG Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB3',50),'LABS Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB4',50),'OTHER Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAB5',50),'QUAL Palo Alto (CA) Development Lvl 1 & 2','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC',50),'Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC1',50),'ATLO Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC2',50),'ENG Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC3',50),'LABS Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC4',50),'OTHER Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAC5',50),'QUAL Palo Alto (CA) Development Lvl 3 & 4','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD',50),'Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD1',50),'ATLO Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD2',50),'ENG Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD3',50),'LABS Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD4',50),'OTHER Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAD5',50),'QUAL Palo Alto (CA) Development Lvl 5','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE',50),'Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE1',50),'ATLO Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE2',50),'ENG Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE3',50),'LABS Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE4',50),'OTHER Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAE5',50),'QUAL Palo Alto (CA) Development Lvl 6 & up','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX',50),'Palo Alto (CA) Development Composite','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX1',50),'ATLO Palo Alto (CA) Development Composite','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX2',50),'ENG Palo Alto (CA) Development Composite','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX3',50),'LABS Palo Alto (CA) Development Composite','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX4',50),'OTHER Palo Alto (CA) Development Composite','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADAX5',50),'QUAL Palo Alto (CA) Development Composite','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA',50),'Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA1',50),'ATLO Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA2',50),'ENG Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA3',50),'LABS Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA4',50),'OTHER Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA5',50),'QUAL Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXADPA6',50),'TOUCH Palo Alto (CA) Development HR & SNES OT','Labor-Core-ATC-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDD25X',50),'Denver Development-Targets','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA',50),'Denver (CO) Development HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA1',50),'ATLO Denver Development Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA2',50),'ENG Denver Development Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA3',50),'LABS Denver Development Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA4',50),'OTHER Denver Development Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA5',50),'QUAL Denver Development Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA6',50),'TOUCH Denver Development Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAA7',50),'BUS COE Denver Development Hourly & NES Straight Time Rate','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB',50),'Denver (CO) Development Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB1',50),'ATLO Denver Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB2',50),'ENG Denver Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB3',50),'LABS Denver Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB4',50),'OTHER Denver Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB5',50),'QUAL Denver Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAB7',50),'BUS COE Denver Development Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC',50),'Denver (CO) Development Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC1',50),'ATLO Denver DevelopmentLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC2',50),'ENG Denver DevelopmentLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC3',50),'LABS Denver DevelopmentLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC4',50),'OTHER Denver DevelopmentLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC5',50),'QUAL Denver DevelopmentLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAC7',50),'BUS COE Denver DevelopmentLvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD',50),'Denver (CO) Development Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD1',50),'ATLO Denver Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD2',50),'ENG Denver Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD3',50),'LABS Denver Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD4',50),'OTHER Denver Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD5',50),'QUAL Denver Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAD7',50),'BUS COE Denver Development Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE',50),'Denver (CO) Development Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE1',50),'ATLO Denver Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE2',50),'ENG Denver Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE3',50),'LABS Denver Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE4',50),'OTHER Denver Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE5',50),'QUAL Denver Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAE7',50),'BUS COE Denver Development Lvl 6 & Up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX',50),'Denver (CO) Development Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX1',50),'ATLO Denver Development Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX2',50),'ENG Denver Development Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX3',50),'LABS Denver Development Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX4',50),'OTHER Denver Development Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDAX5',50),'QUAL Denver Development Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA',50),'Denver (CO) Development HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA1',50),'ATLO Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA2',50),'ENG Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA3',50),'LABS Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA4',50),'OTHER Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA5',50),'QUAL Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA6',50),'TOUCH Denver DevelopmentHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDDPA7',50),'BUS COE Denver DevelopmentHourly & NES Overtime Rate','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA',50),'Denver (CO) Production HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA1',50),'ATLO Denver Production Hourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA2',50),'ENG Denver Production Hourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA3',50),'LABS Denver Production Hourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA4',50),'OTHER Denver Production Hourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA5',50),'QUAL Denver Production Hourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAA6',50),'TOUCH Denver Production Hourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB',50),'Denver (CO) Production Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB1',50),'ATLO Denver Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB2',50),'ENG Denver Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB3',50),'LABS Denver Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB4',50),'OTHER Denver Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB5',50),'QUAL Denver Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAB6',50),'TOUCH Denver Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC',50),'Denver (CO) Production Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC1',50),'ATLO Denver ProductionLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC2',50),'ENG Denver ProductionLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC3',50),'LABS Denver ProductionLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC4',50),'OTHER Denver ProductionLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC5',50),'QUAL Denver ProductionLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAC6',50),'TOUCH Denver ProductionLvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD',50),'Denver (CO) Production Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD1',50),'ATLO Denver Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD2',50),'ENG Denver Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD3',50),'LABS Denver Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD4',50),'OTHER Denver Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD5',50),'QUAL Denver Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAD6',50),'TOUCH Denver Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE',50),'Denver (CO) Production Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE1',50),'ATLO Denver Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE2',50),'ENG Denver Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE3',50),'LABS Denver Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE4',50),'OTHER Denver Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE5',50),'QUAL Denver Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAE6',50),'TOUCH Denver Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX',50),'Denver (CO) Production Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX1',50),'ATLO Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX2',50),'ENG Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX3',50),'LABS Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX4',50),'OTHER Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX5',50),'QUAL Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPAX6',50),'TOUCH Denver Production Hourly & NES Straight Time Rate Composite','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA',50),'Denver (CO) Production HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA1',50),'ATLO Denver ProductionHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA2',50),'ENG Denver ProductionHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA3',50),'LABS Denver ProductionHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA4',50),'OTHER Denver ProductionHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA5',50),'QUAL Denver ProductionHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXDPPA6',50),'TOUCH Denver ProductionHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAA4',50),'Billerica (MA) Development HR & SNES ST','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAB4',50),'Billerica (MA) Development Lvl 1 & 2','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAC4',50),'Billerica (MA) Development Lvl 3 & 4','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAD4',50),'Billerica (MA) Development Lvl 5','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAE4',50),'Billerica (MA) Development Lvl 6 & up','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDAX4',50),'Billerica (MA) Development Composite','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXJDPA4',50),'Billerica (MA) Development HR & SNES OT','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA',50),'Newtown (PA) Development HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA1',50),'ATLO Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA2',50),'ENG Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA3',50),'LABS Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA4',50),'OTHER Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA5',50),'QUAL Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAA6',50),'TOUCH Newtown DevelopmentHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB',50),'Newtown (PA) Development Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB1',50),'ATLO Newtown Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB2',50),'ENG Newtown Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB3',50),'LABS Newtown Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB4',50),'OTHER Newtown Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAB5',50),'QUAL Newtown Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC',50),'Newtown (PA) Development Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC1',50),'ATLO Newtown DevelopmentLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC2',50),'ENG Newtown DevelopmentLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC3',50),'LABS Newtown DevelopmentLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC4',50),'OTHER Newtown DevelopmentLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAC5',50),'QUAL Newtown DevelopmentLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD',50),'Newtown (PA) Development Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD1',50),'ATLO Newtown Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD2',50),'ENG Newtown Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD3',50),'LABS Newtown Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD4',50),'OTHER Newtown Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAD5',50),'QUAL Newtown Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE',50),'Newtown (PA) Development Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE1',50),'ATLO Newtown Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE2',50),'ENG Newtown Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE3',50),'LABS Newtown Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE4',50),'OTHER Newtown Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAE5',50),'QUAL Newtown Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX',50),'Newtown (PA) Development Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX1',50),'ATLO Newtown Development Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX2',50),'ENG Newtown Development Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX3',50),'LABS Newtown Development Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX4',50),'OTHER Newtown Development Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDAX5',50),'QUAL Newtown Development Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA',50),'Newtown (PA) Development HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA1',50),'ATLO Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA2',50),'ENG Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA3',50),'LABS Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA4',50),'OTHER Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA5',50),'QUAL Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKDPA6',50),'TOUCH Newtown DevelopmentHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA',50),'Newtown (PA) Production HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA1',50),'ATLO Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA2',50),'ENG Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA3',50),'LABS Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA4',50),'OTHER Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA5',50),'QUAL Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAA6',50),'TOUCH Newtown ProductionHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB',50),'Newtown (PA) Production Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB1',50),'ATLO Newtown Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB2',50),'ENG Newtown Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB3',50),'LABS Newtown Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB4',50),'OTHER Newtown Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB5',50),'QUAL Newtown Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAB6',50),'TOUCH Newtown Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC',50),'Newtown (PA) Production Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC1',50),'ATLO Newtown ProductionLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC2',50),'ENG Newtown ProductionLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC3',50),'LABS Newtown ProductionLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC4',50),'OTHER Newtown ProductionLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC5',50),'QUAL Newtown ProductionLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAC6',50),'TOUCH Newtown ProductionLvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD',50),'Newtown (PA) Production Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD1',50),'ATLO Newtown Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD2',50),'ENG Newtown Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD3',50),'LABS Newtown Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD4',50),'OTHER Newtown Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD5',50),'QUAL Newtown Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAD6',50),'TOUCH Newtown Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE',50),'Newtown (PA) Production Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE1',50),'ATLO Newtown Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE2',50),'ENG Newtown Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE3',50),'LABS Newtown Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE4',50),'OTHER Newtown Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE5',50),'QUAL Newtown Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAE6',50),'TOUCH Newtown Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX',50),'Newtown (PA) Production Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX1',50),'ATLO Newtown Production Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX2',50),'ENG Newtown Production Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX3',50),'LABS Newtown Production Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX4',50),'OTHER Newtown Production Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX5',50),'QUAL Newtown Production Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPAX6',50),'TOUCH Newtown Production Composite','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA',50),'Newtown (PA) Production HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA1',50),'ATLO Newtown ProductionHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA2',50),'ENG Newtown ProductionHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA3',50),'LABS Newtown ProductionHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA4',50),'OTHER Newtown ProductionHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA5',50),'QUAL Newtown ProductionHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXKPPA6',50),'TOUCH Newtown ProductionHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAA4',50),'Michoud (LA) HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAB4',50),'Michoud (LA) Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAC4',50),'Michoud (LA) Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAD4',50),'Michoud (LA) Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAE4',50),'Michoud (LA) Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMAX4',50),'Michoud (LA) Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXLMPA4',50),'Michoud (LA) HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA',50),'Stennis (MS) Development HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA1',50),'ATLO Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA2',50),'ENG Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA3',50),'LABS Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA4',50),'OTHER Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA5',50),'QUAL Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAA6',50),'TOUCH Stennis DevelopmentHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB',50),'Stennis (MS) Development Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB1',50),'ATLO Stennis Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB2',50),'ENG Stennis Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB3',50),'LABS Stennis Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB4',50),'OTHER Stennis Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAB5',50),'QUAL Stennis Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC',50),'Stennis (MS) Development Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC1',50),'ATLO Stennis DevelopmentLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC2',50),'ENG Stennis DevelopmentLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC3',50),'LABS Stennis DevelopmentLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC4',50),'OTHER Stennis DevelopmentLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAC5',50),'QUAL Stennis DevelopmentLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD',50),'Stennis (MS) Development Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD1',50),'ATLO Stennis Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD2',50),'ENG Stennis Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD3',50),'LABS Stennis Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD4',50),'OTHER Stennis Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAD5',50),'QUAL Stennis Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE',50),'Stennis (MS) Development Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE1',50),'ATLO Stennis Development Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE2',50),'ENG Stennis Development Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE3',50),'LABS Stennis Development Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE4',50),'OTHER Stennis Development Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAE5',50),'QUAL Stennis Development Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX',50),'Stennis (MS) Development Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX1',50),'ATLO Stennis Development Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX2',50),'ENG Stennis Development Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX3',50),'LABS Stennis Development Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX4',50),'OTHER Stennis Development Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDAX5',50),'QUAL Stennis Development Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA',50),'Stennis (MS) Development HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA1',50),'ATLO Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA2',50),'ENG Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA3',50),'LABS Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA4',50),'OTHER Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA5',50),'QUAL Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMDPA6',50),'TOUCH Stennis DevelopmentHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA',50),'Stennis (MS) Production HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA1',50),'ATLO Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA2',50),'ENG Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA3',50),'LABS Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA4',50),'OTHER Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA5',50),'QUAL Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAA6',50),'TOUCH Stennis ProductionHourly & NES Straight Time Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB',50),'Stennis (MS) Production Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB1',50),'ATLO Stennis Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB2',50),'ENG Stennis Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB3',50),'LABS Stennis Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB4',50),'OTHER Stennis Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB5',50),'QUAL Stennis Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAB6',50),'TOUCH Stennis Production Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC',50),'Stennis (MS) Production Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC1',50),'ATLO Stennis ProductionLvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC2',50),'ENG Stennis ProductionLvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC3',50),'LABS Stennis ProductionLvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC4',50),'OTHER Stennis ProductionLvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC5',50),'QUAL Stennis ProductionLvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAC6',50),'TOUCH Stennis ProductionLvl 3 & 4','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD',50),'Stennis (MS) Production Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD1',50),'ATLO Stennis Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD2',50),'ENG Stennis Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD3',50),'LABS Stennis Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD4',50),'OTHER Stennis Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD5',50),'QUAL Stennis Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAD6',50),'TOUCH Stennis Production Lvl 5','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE',50),'Stennis (MS) Production Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE1',50),'ATLO Stennis Production Lvl 6 & Up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE2',50),'ENG Stennis Production Lvl 6 & Up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE3',50),'LABS Stennis Production Lvl 6 & Up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE4',50),'OTHER Stennis Production Lvl 6 & Up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE5',50),'QUAL Stennis Production Lvl 6 & Up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAE6',50),'TOUCH Stennis Production Lvl 6 & Up','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX',50),'Stennis (MS) Production Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX1',50),'ATLO Stennis Production Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX2',50),'ENG Stennis Production Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX3',50),'LABS Stennis Production Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX4',50),'OTHER Stennis Production Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX5',50),'QUAL Stennis Production Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPAX6',50),'TOUCH Stennis Production Composite','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA',50),'Stennis (MS) Production HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA1',50),'ATLO Stennis ProductionHourly & NES Overtime Rate','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA2',50),'ENG Stennis ProductionHourly & NES Overtime Rate','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA3',50),'LABS Stennis ProductionHourly & NES Overtime Rate','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA4',50),'OTHER Stennis ProductionHourly & NES Overtime Rate','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA5',50),'QUAL Stennis ProductionHourly & NES Overtime Rate','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXMPPA6',50),'TOUCH Stennis ProductionHourly & NES Overtime Rate','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAA4',50),'Louisville (CO) Development HR & SNES ST','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAB4',50),'Louisville (CO) Development Lvl 1 & 2','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAC4',50),'Louisville (CO) Development Lvl 3 & 4','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAD4',50),'Louisville (CO) Development Lvl 5','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAE4',50),'Louisville (CO) Development Lvl 6 & up','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDAX4',50),'Louisville (CO) Development Composite','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQDPA4',50),'Louisville (CO) Development HR & SNES OT','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAA4',50),'Louisville (CO) Production HR & SNES ST','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAB4',50),'Louisville (CO) Production Lvl 1 & 2','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAC4',50),'Louisville (CO) Production Lvl 3 & 4','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAD4',50),'Louisville (CO) Production Lvl 5','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAE4',50),'Louisville (CO) Production Lvl 6 & up','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPAX4',50),'Louisville (CO) Production Composite','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXQPPA4',50),'Louisville (CO) Production HR & SNES OT','Labor-Core-ATC-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAA4',50),'Offsite HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAB4',50),'Offsite Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAC4',50),'Offsite Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAD4',50),'Offsite Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAE4',50),'Offsite Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRAX4',50),'Offsite Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXRRPA4',50),'Offsite HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA',50),'Sunnyvale (CA) Dev Space HR & SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA1',50),'ATLO Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA2',50),'ENG Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA3',50),'LABS Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA4',50),'OTHER Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA5',50),'QUAL Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA6',50),'TOUCH Sunnyvale Development - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAA7',50),'BUS COE Sunnyvale Development - SpaceHourly & NES Straight Time Rate','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB',50),'Sunnyvale (CA) Development Space Lvl 1&2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB1',50),'ATLO Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB2',50),'ENG Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB3',50),'LABS Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB4',50),'OTHER Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB5',50),'QUAL Sunnyvale Development - Space Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAB7',50),'BUS COE Sunnyvale Development - Space Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC',50),'Sunnyvale (CA) Development Space Lvl 3&4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC1',50),'ATLO Sunnyvale Development - SpaceLvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC2',50),'ENG Sunnyvale Development - SpaceLvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC3',50),'LABS Sunnyvale Development - SpaceLvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC4',50),'OTHER Sunnyvale Development - SpaceLvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC5',50),'QUAL Sunnyvale Development - SpaceLvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAC7',50),'BUS COE Sunnyvale Development - SpaceLvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD',50),'Sunnyvale (CA) Development Space Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD1',50),'ATLO Sunnyvale Development - Space Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD2',50),'ENG Sunnyvale Development - Space Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD3',50),'LABS Sunnyvale Development - Space Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD4',50),'OTHER Sunnyvale Development - Space Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD5',50),'QUAL Sunnyvale Development - Space Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAD7',50),'BUS COE Sunnyvale Development - Space Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE',50),'Sunnyvale (CA) Development Space Lvl 6+','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE1',50),'ATLO Sunnyvale Development - Space Lvl 6 & Up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE2',50),'ENG Sunnyvale Development - Space Lvl 6 & Up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE3',50),'LABS Sunnyvale Development - Space Lvl 6 & Up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE4',50),'OTHER Sunnyvale Development - Space Lvl 6 & Up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE5',50),'QUAL Sunnyvale Development - Space Lvl 6 & Up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAE7',50),'BUS COE Sunnyvale Development - Space Lvl 6 & Up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX',50),'Sunnyvale (CA) Development Space Comp','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX1',50),'ATLO Sunnyvale Development - Space Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX2',50),'ENG Sunnyvale Development - Space Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX3',50),'LABS Sunnyvale Development - Space Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX4',50),'OTHER Sunnyvale Development - Space Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDAX5',50),'QUAL Sunnyvale Development - Space Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA',50),'Sunnyvale (CA) Dev Space HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA1',50),'ATLO Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA2',50),'ENG Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA3',50),'LABS Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA4',50),'OTHER Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA5',50),'QUAL Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA6',50),'TOUCH Sunnyvale Development - SpaceHourly & NES Overtime Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSDPA7',50),'BUS COE Sunnyvale Development - SpaceHourly & NES Overtime Rate','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA',50),'Sunnyvale (CA) Prod Space  HR&SNES ST','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA1',50),'ATLO Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA2',50),'ENG Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA3',50),'LABS Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA4',50),'OTHER Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA5',50),'QUAL Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAA6',50),'TOUCH Sunnyvale Production - SpaceHourly & NES Straight Time Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB',50),'Sunnyvale (CA) Production Space Lvl 1&2','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB1',50),'ATLO Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB2',50),'ENG Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB3',50),'LABS Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB4',50),'OTHER Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB5',50),'QUAL Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAB6',50),'TOUCH Sunnyvale Production - Space Lvl 1 & 2','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC',50),'Sunnyvale (CA) Production Space Lvl 3&4','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC1',50),'ATLO Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC2',50),'ENG Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC3',50),'LABS Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC4',50),'OTHER Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC5',50),'QUAL Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAC6',50),'TOUCH Sunnyvale Production - SpaceLvl 3 & 4','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD',50),'Sunnyvale (CA) Production Space Lvl 5','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD1',50),'ATLO Sunnyvale Production - Space Lvl 5','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD2',50),'ENG Sunnyvale Production - Space Lvl 5','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD3',50),'LABS Sunnyvale Production - Space Lvl 5','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD4',50),'OTHER Sunnyvale Production - Space Lvl 5','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD5',50),'QUAL Sunnyvale Production - Space Lvl 5','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAD6',50),'TOUCH Sunnyvale Production - Space Lvl 5','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE',50),'Sunnyvale (CA) Production Space Lvl 6+','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE1',50),'ATLO Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE2',50),'ENG Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE3',50),'LABS Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE4',50),'OTHER Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE5',50),'QUAL Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAE6',50),'TOUCH Sunnyvale Production - Space Lvl 6 & Up','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX',50),'Sunnyvale (CA) Production Space Comp','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX1',50),'ATLO Sunnyvale Production - Space Composite','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX2',50),'ENG Sunnyvale Production - Space Composite','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX3',50),'LABS Sunnyvale Production - Space Composite','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX4',50),'OTHER Sunnyvale Production - Space Composite','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX5',50),'QUAL Sunnyvale Production - Space Composite','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPAX6',50),'TOUCH Sunnyvale Production - Space Composite','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA',50),'Sunnyvale (CA) Prod Space HR & SNES OT','Labor-Core-California');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA1',50),'ATLO Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA2',50),'ENG Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA3',50),'LABS Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA4',50),'OTHER Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA5',50),'QUAL Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXSPPA6',50),'TOUCH Sunnyvale Production - SpaceHourly & NES Overtime Rate','Labor-Core-California-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA',50),'Support Development HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA1',50),'ATLO Support Development HR & SNES ST','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA2',50),'ENG Support Development HR & SNES ST','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA3',50),'LABS Support Development HR & SNES ST','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA4',50),'OTHER Support Development HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA5',50),'QUAL Support Development HR & SNES ST','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAA6',50),'TOUCH Support Development HR & SNES ST','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB',50),'Support Development Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB1',50),'ATLO Support Development Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB2',50),'ENG Support Development Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB3',50),'LABS Support Development Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB4',50),'OTHER Support Development Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAB5',50),'QUAL Support Development Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC',50),'Support Development Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC1',50),'ATLO Support Development Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC2',50),'ENG Support Development Lvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC3',50),'LABS Support Development Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC4',50),'OTHER Support Development Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAC5',50),'QUAL Support Development Lvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD',50),'Support Development Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD1',50),'ATLO Support Development Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD2',50),'ENG Support Development Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD3',50),'LABS Support Development Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD4',50),'OTHER Support Development Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAD5',50),'QUAL Support Development Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE',50),'Support Development Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE1',50),'ATLO Support Development Lvl 6 & up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE2',50),'ENG Support Development Lvl 6 & up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE3',50),'LABS Support Development Lvl 6 & up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE4',50),'OTHER Support Development Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAE5',50),'QUAL Support Development Lvl 6 & up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX',50),'Support Development Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX1',50),'ATLO Support Development Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX2',50),'ENG Support Development Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX3',50),'LABS Support Development Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX4',50),'OTHER Support Development Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDAX5',50),'QUAL Support Development Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA',50),'Support Development HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA1',50),'ATLO Support Development HR & SNES OT','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA2',50),'ENG Support Development HR & SNES OT','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA3',50),'LABS Support Development HR & SNES OT','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA4',50),'OTHER Support Development HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA5',50),'QUAL Support Development HR & SNES OT','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZDPA6',50),'TOUCH Support Development HR & SNES OT','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA',50),'Support Production HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA1',50),'ATLO Support Production HR & SNES ST','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA2',50),'ENG Support Production HR & SNES ST','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA3',50),'LABS Support Production HR & SNES ST','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA4',50),'OTHER Support Production HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA5',50),'QUAL Support Production HR & SNES ST','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAA6',50),'TOUCH Support Production HR & SNES ST','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB',50),'Support Production Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB1',50),'ATLO Support Production Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB2',50),'ENG Support Production Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB3',50),'LABS Support Production Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB4',50),'OTHER Support Production Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAB5',50),'QUAL Support Production Lvl 1 & 2','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC',50),'Support Production Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC1',50),'ATLO Support Production Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC2',50),'ENG Support Production Lvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC3',50),'LABS Support Production Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC4',50),'OTHER Support Production Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAC5',50),'QUAL Support Production Lvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD',50),'Support Production Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD1',50),'ATLO Support Production Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD2',50),'ENG Support Production Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD3',50),'LABS Support Production Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD4',50),'OTHER Support Production Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAD5',50),'QUAL Support Production Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE',50),'Support Production Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE1',50),'ATLO Support Production Lvl 6 & up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE2',50),'ENG Support Production Lvl 6 & up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE3',50),'LABS Support Production Lvl 6 & up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE4',50),'OTHER Support Production Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAE5',50),'QUAL Support Production Lvl 6 & up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX',50),'Support Production Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX1',50),'ATLO Support Production Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX2',50),'ENG Support Production Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX3',50),'LABS Support Production Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX4',50),'OTHER Support Production Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPAX5',50),'QUAL Support Production Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA',50),'Support Production HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA1',50),'ATLO Support Production HR & SNES OT','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA2',50),'ENG Support Production HR & SNES OT','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA3',50),'LABS Support Production HR & SNES OT','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA4',50),'OTHER Support Production HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA5',50),'QUAL Support Production HR & SNES OT','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('XXZPPA6',50),'TOUCH Support Production HR & SNES OT','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZPPA',50),'Support Production HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAA',50),'Support 4-Huntsville HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAB',50),'Support 4-Huntsville Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAC',50),'Support 4-Huntsville Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAD',50),'Support 4-Huntsville Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAE',50),'Support 4-Huntsville Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGAX',50),'Support 4-Huntsville Lvl Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZHGPA',50),'Support 4-Huntsville HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAA',50),'Support 4-Michoud HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAB',50),'Support 4-Michoud Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAC',50),'Support 4-Michoud Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAD',50),'Support 4-Michoud Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAE',50),'Support 4-Michoud Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGAX',50),'Support 4-Michoud Lvl Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZMGPA',50),'Support 4-Michoud HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAA',50),'Supt 4-Prod Pool Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAB',50),'Supt 4-Prod Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAC',50),'Supt 4-Prod Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAD',50),'Supt 4-Prod Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAE',50),'Supt 4-Prod Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPGAX',50),'Supt 4-Prod Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAA',50),'Supt 2-Prod Pool Hour & SNES','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAB',50),'Supt 2-Prod Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAC',50),'Supt 2-Prod Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAD',50),'Supt 2-Prod Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAE',50),'Supt 2-Prod Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPTAX',50),'Supt 2-Prod Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAA',50),'Supt 3-Prod Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAB',50),'Supt 3-Prod Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAC',50),'Supt 3-Prod Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAD',50),'Supt 3-Prod Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAE',50),'Supt 3-Prod Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUAX',50),'Supt 3-Prod Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPUPA',50),'Supt 3-Prod Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAA',50),'Supt 1-Prod Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAB',50),'Supt 1-Prod Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAC',50),'Supt 1-Prod Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAD',50),'Supt 1-Prod Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAE',50),'Supt 1-Prod Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXAX',50),'Supt 1-Prod Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZPXPA',50),'Supt 1-Prod Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAA',50),'Supt 4-Dev Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAB',50),'Supt 4-Dev Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAC',50),'Supt 4-Dev Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAD',50),'Supt 4-Dev Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAE',50),'Supt 4-Dev Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGAX',50),'Supt 4-Dev Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGLB',50),'Support 4-Dev Pool Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGNL',50),'Support 4-Dev Pool Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGPA',50),'Supt 4-Dev Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZGPP',50),'ZZZG Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAA',50),'Supt 2-Dev Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAB',50),'Supt 2-Dev Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAC',50),'Supt 2-Dev Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAD',50),'Supt 2-Dev Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAE',50),'Supt 2-Dev Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTAX',50),'Supt 2-Dev Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTLB',50),'Support 2-Dev Pool Svc Ctr Labor','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTNL',50),'Support 2-Dev Pool Svc Ctr ODC','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTPA',50),'Supt 2-Dev Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZTPP',50),'ZZZT Proposal Prep 2007','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAA',50),'Supt 3-Dev Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAB',50),'Supt 3-Dev Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAC',50),'Supt 3-Dev Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAD',50),'Supt 3-Dev Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAE',50),'Supt 3-Dev Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUAX',50),'Supt 3-Dev Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZUPA',50),'Supt 3-Dev Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAA',50),'Supt 1-Dev Pool HR & SNES ST','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAB',50),'Supt 1-Dev Pool Lvl 1 & 2','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAC',50),'Supt 1-Dev Pool Lvl 3 & 4','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAD',50),'Supt 1-Dev Pool Lvl 5','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAE',50),'Supt 1-Dev Pool Lvl 6 & up','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXAX',50),'Supt 1-Dev Pool Composite','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZXPA',50),'Supt 1-Dev Pool HR & SNES OT','');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA',50),'Program Unique HR & SNES ST','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA1',50),'ATLO Program Unique HR & SNES ST','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA2',50),'ENG Program Unique HR & SNES ST','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA3',50),'LABS Program Unique HR & SNES ST','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA4',50),'OTHER Program Unique HR & SNES ST','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA5',50),'QUAL Program Unique HR & SNES ST','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAA6',50),'TOUCH Program Unique HR & SNES ST','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB',50),'Program Unique Lvl 1 & 2','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB1',50),'ATLO Program Unique Lvl 1 & 2','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB2',50),'ENG Program Unique Lvl 1 & 2','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB3',50),'LABS Program Unique Lvl 1 & 2','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB4',50),'OTHER Program Unique Lvl 1 & 2','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAB5',50),'TOUCH Program Unique Lvl 1 & 2','Labor-Core-Touch(6)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC',50),'Program Unique Lvl 3 & 4','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC1',50),'ATLO Program Unique Lvl 3 & 4','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC2',50),'ENG Program Unique Lvl 3 & 4','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC3',50),'LABS Program Unique Lvl 3 & 4','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC4',50),'OTHER Program Unique Lvl 3 & 4','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAC5',50),'QUAL Program Unique Lvl 3 & 4','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD',50),'Program Unique Lvl 5','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD1',50),'ATLO Program Unique Lvl 5','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD2',50),'ENG Program Unique Lvl 5','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD3',50),'LABS Program Unique Lvl 5','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD4',50),'OTHER Program Unique Lvl 5','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAD5',50),'QUAL Program Unique Lvl 5','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE',50),'Program Unique Lvl 6 & up','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE1',50),'ATLO Program Unique Lvl 6 & up','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE2',50),'ENG Program Unique Lvl 6 & up','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE3',50),'LABS Program Unique Lvl 6 & up','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE4',50),'OTHER Program Unique Lvl 6 & up','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAE5',50),'QUAL Program Unique Lvl 6 & up','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX',50),'Program Unique Composite','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX1',50),'ATLO Program Unique Composite','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX2',50),'ENG Program Unique Composite','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX3',50),'LABS Program Unique Composite','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX4',50),'OTHER Program Unique Composite','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZAX5',50),'QUAL Program Unique Composite','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA',50),'Program Unique HR & SNES OT','Labor-Core');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA1',50),'ATLO Program Unique HR & SNES OT','Labor-Core-ATLO(1)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA2',50),'ENG Program Unique HR & SNES OT','Labor-Core-Engineering(2)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA3',50),'LABS Program Unique HR & SNES OT','Labor-Core-Labs(3)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA4',50),'OTHER Program Unique HR & SNES OT','Labor-Core-Other(4)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA5',50),'QUAL Program Unique HR & SNES OT','Labor-Core-Quality(5)');
       insert into dbo.TempRates (Revision, RateCode, PP0,PP1) VALUES((SELECT Revision FROM dbo.Revision where DatePublished IS NULL),LEFT('ZZZZPA6',50),'TOUCH Program Unique HR & SNES OT','Labor-Core-Touch(6)');

       BEGIN TRANSACTION
              UPDATE dbo.[ProPricerRateCodeXref]
              SET ResourceClassID = sel.ResourceClassId
              FROM dbo.[ProPricerRateCodeXref] xref
              JOIN (
                     select x.xrefId, rclu.ID as ResourceClassID
                     from dbo.TempRates tr
                     join dbo.ResourceClassLU rclu on tr.PP1 = rclu.Description
                     join (
                                  select x.id as xrefId, (rc.RateCode + ISNULL(rcelu.RateCodeExtension,'')) as FullRateCode
                                  from dbo.ProPricerRateCodeXref x
                                  left outer join dbo.RateCodeExtensionLU rcelu on x.RateCodeExtensionID = rcelu.ID
                                  join dbo.RateCode rc on x.RateCodeID = rc.Id
                                  join dbo.Revision r on rc.RevisionID = r.Id and r.DatePublished is null
                           ) x on x.FullRateCode = tr.RateCode
              ) sel on xref.ID = sel.xrefId;
       COMMIT TRANSACTION;

       BEGIN TRANSACTION
              INSERT INTO [dbo].[ProPricerRateCodeXref] (UpdateDate, RateCodeID, RateCodeExtensionID, Description)
                     SELECT GETDATE() as UpdateDate, RateCodeID, RateCodeExtensionID, g.PP0 as Description
                     FROM (
                           SELECT DISTINCT r.ID as RevisionID, rc.ID as RateCodeID, rc.RateCode, tr.PP0, tr.PP1, rcelu.ID as RateCodeExtensionID  
                           FROM (SELECT DISTINCT RateCode, PP0, PP1
                                                       FROM [dbo].[TempRates]) as tr
                           JOIN dbo.Revision r on r.DatePublished is null
                           JOIN [dbo].RateCode rc on rc.RevisionID = r.ID AND (LEFT(tr.RateCode,6) = rc.RateCode OR tr.RateCode = rc.RateCode)
                           LEFT OUTER JOIN [dbo].RateCodeExtensionLU rcelu on LEN(tr.RateCode) = 7 AND RIGHT(tr.RateCode,1) = rcelu.RateCodeExtension
                           LEFT OUTER JOIN [dbo].ProPricerRateCodeXref x on LEN(tr.RateCode) = 7 AND RIGHT(tr.RateCode,1) = x.RateCodeExtensionID and rc.ID = x.RateCodeID
                           WHERE rcelu.RateCodeExtension = '7' and x.RateCodeID is null) as g
                     ORDER BY RateCode, RateCodeExtensionID;
       COMMIT TRANSACTION;

END
GO
/*
       12/22/17 [brunworg] - BOEJ-2885 Add an extra field of "Resource Class" into ProPricer Direct Rate Mappings
       ## END ##
*/

/*
	   ## START ##
       1/15/18 [brunworg] - Additional Cobra Mappings
*/
update dbo.RateCode set CobraRateSet = 'FRINGE', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'FRBENGRS';
update dbo.RateCode set CobraRateSet = 'FRINGEU', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'FR2BNGRS';
update dbo.RateCode set CobraRateSet = 'GA', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'GENADGRS';
update dbo.RateCode set CobraRateSet = 'GAG', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'SVGAGRS';
update dbo.RateCode set CobraRateSet = 'LDOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHDEVNET-G';
update dbo.RateCode set CobraRateSet = 'LFOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHFBMNET-G';
update dbo.RateCode set CobraRateSet = 'LGFOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHSERV1_2OFFNET-G';
update dbo.RateCode set CobraRateSet = 'LGNOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHSERV1_2ONNET-G';
update dbo.RateCode set CobraRateSet = 'LHOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHHNTNET-G';
update dbo.RateCode set CobraRateSet = 'LLOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHOFFNET-G';
update dbo.RateCode set CobraRateSet = 'LMOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHMICNET-G';
update dbo.RateCode set CobraRateSet = 'LPOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHPRDNET-G';
update dbo.RateCode set CobraRateSet = 'LUOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHPRUNET-G';
update dbo.RateCode set CobraRateSet = 'LVSOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHLVSNET';
update dbo.RateCode set CobraRateSet = 'LVSOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHLVSNET-G';
update dbo.RateCode set CobraRateSet = 'LVSOHCS', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'CASLVNET';
update dbo.RateCode set CobraRateSet = 'PCGOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHSERVPRONET-G';
update dbo.RateCode set CobraRateSet = 'PCOH1', CobraCode1ID = 1 from dbo.RateCode rc join dbo.Revision r on r.ID = rc.RevisionID and r.DatePublished is null where rc.RateCode = 'OHPRONET-G';

/*
       1/15/18 [brunworg] - Additional Cobra Mappings
       ## END ##
*/

/*
    File: \Release 2018.1\1 - Release 2018.1 Script.sql
*/
PRINT '### Starting file: \Release 2018.1\1 - Release 2018.1 Script.sql';
/*
	## START ##
	1/2/18 twilson3		BOEJ-2704 File Attachments
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileAttachment]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[FileAttachment](
		[FileAttachmentId] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDate] [datetime2](7) NOT NULL,
		[Name] varchar(100) NOT NULL,
		[Link] varchar(255) NOT NULL,
		[SectionId] int NULL, 
		[RevisionId] int NOT NULL
	 CONSTRAINT [PK_FileAttachment] PRIMARY KEY NONCLUSTERED 
	(
		[FileAttachmentId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE CLUSTERED INDEX [IX_FileAttachment_RevisionId] ON [dbo].[FileAttachment] 
	(
		[RevisionId]   ASC
	) 
	ON [PRIMARY]

	ALTER TABLE [dbo].[FileAttachment] WITH CHECK ADD  CONSTRAINT [FK_FileAttachment_SectionId] 
	FOREIGN KEY([SectionId]) REFERENCES [dbo].[Section] ([Id]);

END
GO

/*
	1/2/18 twilson3		BOEJ-2704 File Attachments
	## END ##
*/

/*
	## START ##
	1/4/18 ranzalon		BOEJ-2698 Burden Pool Categorization 
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'BurdenPoolLU' AND
		C.name = 'IsCommercial' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE [dbo].[BurdenPoolLU] ADD [IsCommercial] BIT NOT NULL DEFAULT 0;
END
GO

UPDATE [dbo].[BurdenPoolLU] SET [IsCommercial] = 1 WHERE [BurdenPool] like '%-G';
GO

/*
	1/4/18 ranzalon		BOEJ-2698 Burden Pool Categorization 
	## END ##
*/
/*
	## START ##
	1/9/18 brunworg		BOEJ-2901 - Remove ProPricer Pricing Codes table in PPR&D section 3.7
*/

DELETE dbo.Section
WHERE SectionContentTypeID = (SELECT ID from dbo.SectionContentTypeLU WHERE Description = 'ProPricer Pricing Table Content');
GO

DELETE dbo.SectionContentTypeLU WHERE Description = 'ProPricer Pricing Table Content'
GO

/*
	1/9/18 brunworg		BOEJ-2901 - Remove ProPricer Pricing Codes table in PPR&D section 3.7
	## END ##
*/
/*
	## START ##
	1/15/18 brunworg	BOEJ-2965 - System error when attempting to move a PPR&D section that has an associated file attachment (rename stored procedure)
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[remapRateCodesForSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[remapRateCodesForSection];
GO
/*
	## END ##
	1/15/18 brunworg	BOEJ-2965 - System error when attempting to move a PPR&D section that has an associated file attachment (rename stored procedure)
*/


/*
    File: \Stored Procedures\copyRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\copyRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[copyRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[copyRevision];
GO

CREATE PROCEDURE [dbo].[copyRevision]
(
	 @Id int
    ,@NewRevision varchar(1000)
	,@NewHistory nvarchar(max)
	,@NewCreatedBy varchar(1000)
	,@NewReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[copyRevision]
	**		Desc:	Copy a PPR&D Revision.  This includes copying all of the 
	**				associated rates, COBRA/ProPricer Mappings, and PPR&D content.
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/10/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/01/2017	brunworg			Added StartYear and EndYear columns,
	**										removed IsDeleted from RateCode table.
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/10/2017	brunworg			BOEJ-2453 - Fix bug so ProPricerBurdenRateMap 
	**										gets RateCodeID values for new revision.
	**		08/15/2017  tglick				modified to pull new burdenpoolIds
	**										for comm/govt burden pools on new ratecodes
	**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
	**										and DataFormat from RateCode table.
	**		08/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		08/21/2017	brunworg			Add "IsInternal" field to Section table.
	**		08/22/2017	brunworg			Remove "IsDeleted" field from all tables.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		09/19/2017	brunworg			Fix bug that caused duplicate sections.
	**		10/05/2017	ranzalon			Update for History and Release Notes
	**		10/10/2017	Dusan				Added RevisionUniqueSectionId
	**		01/02/2018	twilson3			BOEJ-2704 File Attachments
	**		01/04/2018	ranzalon			BOEJ-2698 Burden Pool Categorization - 
	**										Updated for IsCommercial bit
	**		1/18/18		Dusan				Added ResourceClassID into copying
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;
	DECLARE @StartYear int, @EndYear int;
	
	SELECT @StartYear = StartYear, @EndYear = EndYear FROM [dbo].Revision WHERE ID = @Id;
	IF @@ROWCOUNT = 0	
		BEGIN
			SET @ErrorMessage = 'Copy failed - Revision could not be found.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN		
		END
	ELSE
		BEGIN
			DECLARE @RevisionID INT;
			DECLARE @RevisionIdResultSet table (ID INT);
			DECLARE @SectionMap AS TABLE (OldId Int, [NewId] Int);
			DECLARE @RateCodeMap AS TABLE (OldId Int, [NewId] Int);
			DECLARE @BurdenPoolMap as TABLE (oldId Int, [NewId] Int); -- burden pools (comm/govt) for new ratecodes 

			BEGIN TRY
				-- Upsert New Revision
				INSERT INTO @RevisionIdResultSet (ID) EXECUTE [dbo].upsertRevision -1, null, @NewRevision, @NewHistory, @NewCreatedBy, @StartYear, @EndYear, @NewReleaseNotes;
				SELECT TOP 1 @RevisionID = ID FROM @RevisionIdResultSet;

				-- Copy associated PPR&D document (all sections and associated content)
				INSERT INTO [dbo].Section
				(UpdateDate, RevisionID, ParentID, DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId)
				OUTPUT Inserted.ParentID, Inserted.Id INTO @SectionMap
				SELECT GETDATE() AS UpdateDate,
								@RevisionID as RevisionID,
								Id AS ParentID, -- Note that we save the old Id in the new ParentID.
								DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId
				FROM dbo.Section 
				WHERE RevisionID = @ID;

				-- Apply the section Map to update ParentID values for section hierarchy
				UPDATE SectionNew
				SET ParentID = MapNew.[NewId]
				FROM @SectionMap AS M
				INNER JOIN [dbo].Section AS SectionNew ON SectionNew.Id = M.[NewId] -- ROWS we need to fix
				INNER JOIN @SectionMap AS MapOld ON MapOld.OldId = SectionNew.ParentID
				INNER JOIN [dbo].Section AS SectionOld ON SectionOld.Id = M.[OldId] 
				LEFT OUTER JOIN @SectionMap AS MapNew ON MapNew.OldId = SectionOld.ParentID;

				-- Copy File Attachment entries for the revision

				INSERT INTO [dbo].FileAttachment
				(UpdateDate
				,[Name]
				,[Link]
				,[SectionId]
				,[RevisionId])
				SELECT GETDATE() AS UpdateDate,
				faOld.Name,
				faOld.Link,
				m.NewID as SectionId,
				@RevisionID as RevisionID
				FROM [dbo].FileAttachment AS faOld
				LEFT OUTER JOIN @SectionMap m on faOld.SectionId = m.OldId
				WHERE faOld.RevisionId = @Id

				-- Copy BurdenPoolLU entries for the revision
				MERGE
					[dbo].[BurdenPoolLU] as bp
				USING(
					SELECT bpOld.* 
					FROM [dbo].[BurdenPoolLU] as bpOld
					WHERE bpOld.RevisionID = @Id ) as x
				ON (1=0)
				WHEN NOT MATCHED
					THEN INSERT (UpdateDate, BurdenPool, [Description],  IsGaT2ApplicableForMissionSolutions, RevisionID, IsCommercial)
					VALUES(GETDATE(), BurdenPool, [Description], IsGaT2ApplicableForMissionSolutions, @RevisionID, IsCommercial)
				OUTPUT x.[ID], Inserted.Id INTO @BurdenPoolMap;

				-- Copy rates for the revision
				MERGE
					[dbo].[RateCode] AS rc
				USING(
					SELECT rcOld.*, m.NewID as newSectionID
					FROM [dbo].[RateCode] AS rcOld
					LEFT OUTER JOIN @SectionMap m on rcOld.SectionID = m.OldId
					WHERE rcOld.RevisionID = @Id) as x
				ON (1=0)  
				WHEN NOT MATCHED   
					THEN INSERT (UpdateDate, RevisionID, CategoryID, Description,
						   SectionID, RateCode, ResourceTypeID, 
						   GovernmentBurdenPoolID, CommercialBurdenPoolID, 
						   RateTypeID, CobraRateSet, CobraCode1ID)
					VALUES (GETDATE(), @RevisionID, CategoryID, Description,
						   newSectionID, RateCode, ResourceTypeID,
						   -- add new ids for GovernmentBurdenPoolId and CommercialBurdenPoolId
						   ( SELECT [NewId] FROM @BurdenPoolMap WHERE OldId = GovernmentBurdenPoolId),
						   ( SELECT [NewId] FROM @BurdenPoolMap WHERE OldId = CommercialBurdenPoolId),
						   RateTypeID, CobraRateSet, CobraCode1ID)
				OUTPUT x.[ID], Inserted.Id INTO @RateCodeMap;

				-- Copy Rate code years for the revision
				INSERT INTO [dbo].[RateCodeYear] ([UpdateDate],[RateCodeID],[Year],[Rate])
				SELECT GETDATE(), rcMap.NewID, Year, Rate
				FROM [dbo].[RateCodeYear] rcyOld
				LEFT OUTER JOIN @RateCodeMap rcMap on rcyOld.RateCodeID = rcMap.OldId
				JOIN [dbo].RateCode rc on rc.ID = rcyOld.RateCodeID
				WHERE rc.RevisionId = @Id;

				-- Copy ProPricer mapping and cross reference information for the revision
				INSERT INTO [dbo].[ProPricerBurdenRateMap]
						   ([UpdateDate],[BurdenPoolID],[BurdenElementID],[RateCodeID])
				SELECT GETDATE(), bpNew.Id as BurdenPoolID, ppOld.BurdenElementID, rcMap.NewID 
				FROM [dbo].[ProPricerBurdenRateMap] ppOld
				join  [dbo].[BurdenPoolLU] bpOld on bpOld.ID = ppOld.BurdenPoolID and bpOld.RevisionID = @Id
				join  [dbo].[BurdenPoolLU] bpNew on bpOld.BurdenPool = bpNew.BurdenPool and bpNew.RevisionID = @RevisionID
				LEFT OUTER JOIN @RateCodeMap rcMap on ppOld.RateCodeID = rcMap.OldId;

				INSERT INTO [dbo].[ProPricerRateCodeXref] ([UpdateDate],[RateCodeID],[Description],[RateCodeExtensionID], [ResourceClassID])
				SELECT GETDATE(), rcMap.NewID, ppOld.Description, RateCodeExtensionID, ResourceClassID
				FROM [dbo].[ProPricerRateCodeXref] ppOld
				LEFT OUTER JOIN @RateCodeMap rcMap on ppOld.RateCodeID = rcMap.OldId
				JOIN [dbo].RateCode rc on rc.ID = ppOld.RateCodeID
				WHERE rc.RevisionId = @Id;
				
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				IF @ErrorMessage IS NULL
					BEGIN
						SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be copied.'
					END
								
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END

	IF @@ERROR = 0
		SELECT @RevisionId as Id, @NewRevision as Revision;

GO

/*
    File: \Stored Procedures\deleteBurdenPoolLU.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteBurdenPoolLU.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBurdenPoolLU];
GO

CREATE PROCEDURE [dbo].[deleteBurdenPoolLU]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteBurdenPoolLU]
	**		Desc:	Delete a Burden Pool 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/03/2017	brunworg			Change stored procedure name.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE dbo.RateCode SET GovernmentBurdenPoolId = null WHERE GovernmentBurdenPoolId = @Id
			UPDATE dbo.RateCode SET CommercialBurdenPoolId = null WHERE CommercialBurdenPoolId = @Id
			DELETE FROM dbo.ProPricerBurdenRateMap WHERE BurdenPoolID = @Id
			DELETE FROM dbo.BurdenPoolLU WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteCobraFiscalYear.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteCobraFiscalYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCobraFiscalYear];
GO

CREATE PROCEDURE [dbo].[deleteCobraFiscalYear]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteCobraFiscalYear]
	**		Desc:	Delete a COBRA Fiscal Year
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.CobraFiscalYearLU WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO


/*
    File: \Stored Procedures\deleteFileAttachment.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteFileAttachment.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteFileAttachment];
GO

CREATE PROCEDURE [dbo].[deleteFileAttachment]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteFileAttachment]
	**		Desc:	Delete a File Attachment
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 1/2/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE FileAttachmentId = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[FileAttachment] WHERE FileAttachmentId = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteProPricerBurdenRateMap.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProPricerBurdenRateMap.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerBurdenRateMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerBurdenRateMap];
GO

CREATE PROCEDURE [dbo].[deleteProPricerBurdenRateMap]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProPricerBurdenRateMap]
	**		Desc:	Delete a ProPricer Burden Rate Mapping
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProPricerBurdenRateMap] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProPricerBurdenRateMap WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ProPricer Burden Rate Mapping with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteProPricerRateCodeXref.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProPricerRateCodeXref.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXref];
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXref]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProPricerRateCodeXref]
	**		Desc:	Delete a ProPricer Rate Code cross reference
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProPricerRateCodeXref WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteRateCode.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteRateCode.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCode]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRateCode];
GO

CREATE PROCEDURE [dbo].[deleteRateCode]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRateCode]
	**		Desc:	Delete a Rate Code 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/10/2017	brunworg			Remove references to ActivityTypeMap table.
	**		8/10/2017	brunworg			Remove transaction handling.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[RateCode] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			BEGIN TRY
				DELETE FROM dbo.RateCodeYear WHERE RateCodeID = @Id
				DELETE FROM dbo.ProPricerBurdenRateMap WHERE RateCodeID = @Id
				DELETE FROM dbo.ProPricerRateCodeXref WHERE RateCodeID = @Id
				DELETE FROM dbo.RateCode WHERE ID = @Id
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The RateCode with ID ' + CAST(@Id  AS varchar(10)) + ' could not be deleted.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Rate Code with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteRateCodeYear.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteRateCodeYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRateCodeYear];
GO

CREATE PROCEDURE [dbo].[deleteRateCodeYear]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRateCodeYear]
	**		Desc:	Delete a Rate Code Year
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[RateCodeYear] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.RateCodeYear WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Rate Code Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
		
GO



/*
    File: \Stored Procedures\deleteRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRevision];

GO

CREATE PROCEDURE [dbo].[deleteRevision]
(
	@Id			int,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRevision]
	**		Desc:	Delete a PPR&D Revision
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Corrected order of delete statements.
	**										Added code to delete associated Document
	**										and Sections.
	**		7/10/2017	brunworg			Added code to raise an exception if
	**										deleting Revision with associated 
	**										CostVolume.
	**		7/10/2017	brunworg			Remove references to ActivityTypeMap and
	**										ProPricerActivityTypeXref tables.
	**		8/04/2017	brunworg			Updated to reflect new data model with
	**										revisionId in BurdenPoolLU table.
	**		8/10/2017	brunworg			Remove transaction handling.
	**		8/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		8/24/2017	brunworg			Updated to delete ProPricerBurdenPoolMap  
	**										and ProPricerRateCodeXref entries before 
	**										RateCodes.
	**		9/8/2017	brunworg			Modified ProPricerBurdenRateMap delete statement.
	**		1/2/2018	twilson3			BOEJ-2704 File Attachments
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			BEGIN TRY
				DELETE FROM [dbo].[FileAttachment] WHERE [RevisionId] = @Id
				DELETE FROM [dbo].[ProPricerBurdenRateMap] 
				WHERE EXISTS
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[ProPricerRateCodeXref] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCodeYear] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCode] WHERE RevisionID = @Id
				DELETE FROM [dbo].[BurdenPoolLU] where RevisionID = @Id	
				DELETE FROM [dbo].[Section] WHERE RevisionID = @Id
				DELETE FROM [dbo].[Revision] WHERE ID = @Id
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be deleted.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO

/*
    File: \Stored Procedures\deleteSection.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteSection.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSection];
GO

CREATE PROCEDURE [dbo].[deleteSection]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteSection]
	**		Desc:	Delete a document section
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/6/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		09/07/2017	brunworg			Modified to delete child sections.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			-- Retrieve section and all child sections.  
			-- Use a Common Table Expression (CTE) to gather all of the hierarchical section IDs.
			DECLARE @TempSectionIDs TABLE (ID INT, UpdateDate datetime2(7), level INT);

			WITH CteSectionIDs (ParentID, ID, UpdateDate, level)
			AS
			(
				-- start with specified section ID
				SELECT ParentID, ID, UpdateDate, 0 as level FROM dbo.[Section] WHERE ID = @ID
				UNION ALL
				-- recursive child sections
				SELECT child.ParentID, child.Id, child.UpdateDate, level + 1 FROM dbo.[Section] as child INNER JOIN CteSectionIDs as parent ON child.ParentID = parent.ID
			)
			-- Statement that executes the CTE
			INSERT INTO @TempSectionIDs (ID, UpdateDate, level)
			SELECT ID, UpdateDate, level from CteSectionIDs;

			-- Make sure none of the sections are referenced by rate codes
			SELECT * FROM [dbo].[RateCode] WHERE SectionID in (SELECT ID from @TempSectionIDs);
			IF @@ROWCOUNT = 0	
				BEGIN
					BEGIN TRY
						-- Delete the sections found by the CTE
						DECLARE @SectionId int, @SectionUpdateDate datetime2(7);
						DECLARE cur CURSOR LOCAL FOR
							SELECT s.ID, s.Updatedate 
							FROM [dbo].Section s
							JOIN @TempSectionIDs tmp on s.ID = tmp.ID
							ORDER BY tmp.level desc;

						OPEN cur
						FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate

						WHILE @@FETCH_STATUS = 0 BEGIN
							DELETE FROM [dbo].[Section] WHERE Id = @SectionId AND UpdateDate = @SectionUpdateDate
							FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate
						END
						CLOSE cur;
						DEALLOCATE cur;
					END TRY
					BEGIN CATCH
						SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

						SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' (and child sections) could not be deleted.'
						RAISERROR (
								@ErrorMessage, -- Message text.
								@ErrorSeverity, -- Severity,
								@ErrorState -- State,
								)
						RETURN
					END CATCH;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' (and child sections) could not be deleted.  One or more sections are referenced by Rate Codes.'
					RAISERROR (
							@ErrorMessage, -- Message text.
							11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN		
				END
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\ELMAH_GetErrorsXml.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_GetErrorsXml.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorsXml]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_GetErrorsXml];

GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS 
/******************************************************************************
**		 
**		Name: [ELMAH_GetErrorsXml]
**		Desc:	Gets a group of elmah errors in xml format
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT 
        @TotalCount = COUNT(1) 
    FROM 
        [ELMAH_Error]
    WHERE 
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM 
            [ELMAH_Error]
        WHERE   
            [Application] = @Application
        ORDER BY 
            [TimeUtc] DESC, 
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT 
        errorId     = [ErrorId], 
        application = [Application],
        host        = [Host], 
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode], 
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z'
    FROM 
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND 
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC, 
        [Sequence] DESC
    FOR
        XML AUTO

GO

/*
    File: \Stored Procedures\ELMAH_GetErrorXml.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_GetErrorXml.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorXml]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_GetErrorXml];

GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml]
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS
/******************************************************************************
**		 
**		Name: [ELMAH_GetErrorXml]
**		Desc:	Gets a specific elmah error in xml format
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application

GO


/*
    File: \Stored Procedures\ELMAH_LogError.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_LogError.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_LogError]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_LogError];

GO

CREATE PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS
/******************************************************************************
**		 
**		Name:   [ELMAH_LogError]
**		Desc:	Creates an Error Log entry in ELMAH_Error table
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

	INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

GO


/*
    File: \Stored Procedures\ExportCobraRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportCobraRates.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportCobraRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportCobraRates];

GO

CREATE PROCEDURE [dbo].[ExportCobraRates]
(
	@RevisionID INT
)
AS
/******************************************************************************
**		 
**		Name: [genBOE].[ExportCobraRates]
**		Desc: Generate COBRA Rate Export file
**			
**      TODO - Modify to only export changed rates. 
**             Note: This will probably end up being generated via C# code
**                   instead of a stored procedure. But for now, this will  
**                   serve as an example of how to export COBRA rates.
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		08/03/2017	brunworg			Updated to reflect new data model with
**										cobraCode1ID in RateCode table.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT rc.CobraRateSet, cclu.Description, rc.Description, cfylu.FiscalYearStartDate as Date, rcy.Rate as Value
FROM [dbo].[Revision] rev
JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
JOIN [dbo].[CobraCode1LU] cclu on rc.CobraCode1ID = cclu.ID
JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
JOIN [dbo].[CobraFiscalYearLU] cfylu on rcy.[Year] = cfylu.Year
WHERE rev.[ID] = @RevisionID and rc.CobraRateSet is not null
ORDER BY rc.RateCode;

GO

/*
    File: \Stored Procedures\ExportProPricerBurdenRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportProPricerBurdenRates.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportProPricerBurdenRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportProPricerBurdenRates];

GO

CREATE PROCEDURE [dbo].[ExportProPricerBurdenRates]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[ExportProPricerBurdenRates]
**		Desc: Generate ProPricer Burden Rate Export file
**			
**      TODO - Determine why certain years are exported, but others are not? 
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Updated to reflect new data model.
**                                      Changed ESCAL column from being a 
**                                      hard-coded placeholder to one of the 
**                                      burden element columns.
**		08/03/2017	brunworg			Updated to reflect new data model with
**										revisionId in BurdenPoolLU table.
*******************************************************************************/
SET NOCOUNT ON 

SELECT * FROM (
  SELECT bplu.[BurdenPool], bplu.[Description]
	,'' as EffectiveDate
	,rcy.[Year] as Date
	,belu.[BurdenElement] as BurdenElement, rcy.[Rate]
  FROM [dbo].[Revision] rev
  JOIN [dbo].[BurdenPoolLU] bplu on bplu.RevisionId = rev.[ID]
  JOIN [dbo].[ProPricerBurdenRateMap] map on map.[BurdenPoolID] = bplu.[ID]
  JOIN [dbo].[RateCode] rc on map.[RateCodeId] = rc.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  JOIN [dbo].[BurdenElementLU] belu on map.[BurdenElementID] = belu.[ID]
  WHERE rev.[ID] = @RevisionID) as src
  PIVOT (max(src.[Rate]) for src.[BurdenElement] in ([ESCAL],[OH Dev],[OH Prod],[OH FBM],[OH LVS],
			[OH Hunts],[OH Offsite],[OH Michoud],[OH Mich MH],[OH Prg Uni],[OH Pro DIR],
			[OH Pro MSC],[OH Pro Serv],[OH On Serv],[OH Off Serv],[OH PH4],[OH PH5],
			[Fringe],[Fringe T2],[FRG Serv Corp],[FRG Serv LMOS],[G&A],[G&A T2],
			[FCCOM Dev],[FCCOM Prod],[FCCOM FBM],[FCCOM LVS],[FCCOM Hunt],[FCCOM Ofst],
			[FCCOM Mich],[FCM Prg Un],[FCCOM Proc],[FCCOM ProM],[FCCOM G&A],[FCM T2 G&A],
			[FCCM IWTA],[FCCM PH2],[FCCM PH3],[FCCM PH4],[Fee/Prft])) as piv;

GO



/*
    File: \Stored Procedures\ExportProPricerDirectRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportProPricerDirectRates.sql';
 IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportProPricerDirectRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportProPricerDirectRates];

GO

CREATE PROCEDURE [dbo].[ExportProPricerDirectRates]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[ExportProPricerDirectRates]
**		Desc: Generate ProPricer Direct Rate Export data for both Government
**            and Commercial rates.
**            The results will be a UNION of the following:
**            1) Rate Codes that do not need Rate Code Extensions.
**            2) Rate Codes that are mapped by activity type and
**               need to be expanded using the Rate Code Extensions.
**            3) Pro Pricer Equivalent Rate Codes.
**               The rates for these activity types will be
**               exported a second time substituting the first 4 characters
**               of the rate code as follows.
**	    	      XXDD => XADD
**	    	      XXLM => XMLM
**	    	      XXZD => XCZD
**	    	      XXZP => XCZP
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Updated to reflect new data model.
**		7/12/2017	brunworg			Changed RateCodeYear - Year field
**                                      from varchar(50) to int.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,rc.[RateCode] + ISNULL(rcelu.[RateCodeExtension],'') as Resource  
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID
UNION
SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,REPLACE(REPLACE(REPLACE(REPLACE(rc.[RateCode],'XXDD','XXAD'),'XXLM','XMLM'),'XXZD','XCZD'),'XXZP','XCZP') + ISNULL(rcelu.[RateCodeExtension],'') as Resource 
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID AND LEFT(rc.RateCode,4) in ('XXDD', 'XXLM', 'XXZD', 'XXZP')
ORDER BY Resource, EndDate;

GO

/*
    File: \Stored Procedures\GetProPricerBurdenRateCodes.sql
*/
PRINT '### Starting file: \Stored Procedures\GetProPricerBurdenRateCodes.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProPricerBurdenRateCodes]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetProPricerBurdenRateCodes];

GO

CREATE PROCEDURE [dbo].[GetProPricerBurdenRateCodes]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetProPricerBurdenRateCodes]
**		Desc: Get ProPricer Burden Rate Codes
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added ESCAL burden element column.
**		08/03/2017	brunworg			Updated to reflect new data model with
**										revisionId in BurdenPoolLU table.
*******************************************************************************/
SET NOCOUNT ON 

SELECT * FROM (
  SELECT bplu.[BurdenPool], bplu.[Description]
	,belu.[BurdenElement] as BurdenElement, rc.[RateCode]
  FROM [dbo].[Revision] rev
  JOIN [dbo].[BurdenPoolLU] bplu on bplu.RevisionId = rev.[ID]
  JOIN [dbo].[ProPricerBurdenRateMap] map on map.[BurdenPoolID] = bplu.[ID]
  JOIN [dbo].[RateCode] rc on map.[RateCodeId] = rc.[ID]
  JOIN [dbo].[BurdenElementLU] belu on map.[BurdenElementID] = belu.[ID]
  WHERE rev.[ID] = @RevisionID) as src
  PIVOT (max(src.[RateCode]) for src.[BurdenElement] in ([ESCAL],[OH Dev],[OH Prod],[OH FBM],[OH LVS],
			[OH Hunts],[OH Offsite],[OH Michoud],[OH Mich MH],[OH Prg Uni],[OH Pro DIR],
			[OH Pro MSC],[OH Pro Serv],[OH On Serv],[OH Off Serv],[OH PH4],[OH PH5],
			[Fringe],[Fringe T2],[FRG Serv Corp],[FRG Serv LMOS],[G&A],[G&A T2],
			[FCCOM Dev],[FCCOM Prod],[FCCOM FBM],[FCCOM LVS],[FCCOM Hunt],[FCCOM Ofst],
			[FCCOM Mich],[FCM Prg Un],[FCCOM Proc],[FCCOM ProM],[FCCOM G&A],[FCM T2 G&A],
			[FCCM IWTA],[FCCM PH2],[FCCM PH3],[FCCM PH4],[Fee/Prft])) as piv
ORDER BY BurdenPool;

GO


/*
    File: \Stored Procedures\GetRateCodeList.sql
*/
PRINT '### Starting file: \Stored Procedures\GetRateCodeList.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRateCodeList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetRateCodeList];

GO

CREATE PROCEDURE [dbo].[GetRateCodeList]
(
	@RevisionID INT,
	@StartYear  varchar(50) = NULL,
	@EndYear    varchar(50) = NULL
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetRateCodeList]
**		Desc: Get rate codes, rate code details, and rate values by year.
**            If @StartYear and @EndYear parameters are provided, return rates
**            between those years (inclusive).
**            Otherwise, return rates for all years.
**			 
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added Government and Commercial 
**                                      Burden Pool columns.
**                                      Added pivot columns thru 2099.
**		07/03/2017	brunworg			Added StartYear and EndYear parameters.
**                                      Return each year/rate as separate rows.
**      07/03/2017	brunworg			Modified where clause to remove duplicate code.
**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
**										and DataFormat from RateCode table.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT clu.Description, rc.Description, rc.RateCode, ISNULL(s.Title,'') as Section,
	    ISNULL(rc.CobraRateSet,'') as CobraRateSet, rc.CobraCode1ID as CobraCode1, 
	    ISNULL(reslu.Description,'') as ResourceType, 
		ISNULL(bplu1.BurdenPool,'') as GovernmentBurdenPool, ISNULL(bplu2.BurdenPool,'') as CommercialBurdenPool, 
		ISNULL(rtlu.Description,'') as RateType, rcy.year, rcy.rate
FROM dbo.Revision rev
JOIN dbo.RateCode rc on rev.ID = rc.RevisionID
JOIN dbo.CategoryLU clu on rc.CategoryID = clu.ID
LEFT OUTER JOIN dbo.Section s on rc.SectionID = s.ID
LEFT OUTER JOIN dbo.ResourceTypeLU reslu on rc.ResourceTypeID = reslu.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu1 on rc.GovernmentBurdenPoolID = bplu1.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu2 on rc.CommercialBurdenPoolID = bplu2.ID
LEFT OUTER JOIN dbo.RateTypeLU rtlu on rc.RateTypeID = rtlu.ID
JOIN dbo.RateCodeYear rcy on rc.ID= rcy.RateCodeID
WHERE rev.ID = @RevisionID AND ((@StartYear IS NULL AND @EndYear IS NULL) OR (rcy.Year >= @StartYear AND rcy.Year <= @EndYear))
ORDER BY clu.Description, rc.RateCode, rcy.Year;

GO

/*
    File: \Stored Procedures\publishRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\publishRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[publishRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[publishRevision];
GO

CREATE PROCEDURE [dbo].[publishRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
    ,@PublishedBy varchar(1000)
	,@History nvarchar(max)
	,@ReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[publishRevision]
	**		Desc:	Publish a PPR&D Revision.  This includes creating the new Work
	**				In Progress (WIP) revision by making a copy of the published 
	**				revision and incrementing the revision number by 1.
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/7/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/13/2017	brunworg			Add code to make a copy of the 
	**										previous revision.
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/24/2017	brunworg			Change return signature to contain
	**										new WIP Revision ID and Revision name.
	**		10/05/2017	ranzalon			Update for History and Release Notes
	**		10/11/2017	ranzalon			Update to take in History and Release
	**										Note inputs, don't copy release notes
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;
	DECLARE @Revision varchar(50), @NewRevision int;

	SELECT @Revision = Revision FROM [dbo].Revision WHERE ID = @Id;
	IF @@ROWCOUNT = 0	
		BEGIN
			SET @ErrorMessage = 'Publish failed - Revision could not be found.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN		
		END

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
		BEGIN
			BEGIN TRY
				-- Mark the revision as published
				SET @UpdateDate = GETDATE()
				UPDATE [dbo].Revision
					SET UpdateDate = @UpdateDate
						,DatePublished = @UpdateDate
						,PublishedBy = @PublishedBy
						,History = @History
						,ReleaseNotes = @ReleaseNotes
					WHERE 
						ID = @Id;

				-- Create a new Work-In-Progress revision by making a copy of the published revision 
				SET @NewRevision = CONVERT(int, @Revision) + 1;	-- increment the revision number
				EXECUTE dbo.copyRevision @Id=@Id, @NewRevision=@NewRevision, @NewHistory=@History, @NewCreatedBy=@PublishedBy, @NewReleaseNotes=null;
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				IF @ErrorMessage IS NULL
					BEGIN
						SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be published.'
					END
								
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END

	IF @@ERROR = 0
		SELECT @Id AS Id, @Revision as Revision;
GO

/*
    File: \Stored Procedures\remapSectionReferences.sql
*/
PRINT '### Starting file: \Stored Procedures\remapSectionReferences.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[remapSectionReferences]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[remapSectionReferences];
GO

CREATE PROCEDURE [dbo].[remapSectionReferences]
(
	@Id			INT,
	@UpdateDate datetime2,
	@NewId		INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[remapSectionReferences] (originally remapRateCodesForSection)
	**		Desc:	Update all references to @Id (old section Id)
	**				to point to @NewId (new section Id)
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 9/24/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		01/11/2017	brunworg			Renamed and modified to update file 
	**                                      attachment references.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE [dbo].[RateCode] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;

			UPDATE [dbo].[FileAttachment] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\upsertBurdenPoolLU.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertBurdenPoolLU.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBurdenPoolLU];
GO

CREATE PROCEDURE [dbo].[upsertBurdenPoolLU]
(
	 @Id			int
	,@UpdateDate	datetime2(7)
	,@RevisionID	int
	,@BurdenPool	varchar(50)
    ,@Description	varchar(4000)
    ,@IsGaT2ApplicableForMissionSolutions bit
	,@IsCommercial bit)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertBurdenPoolLU]
	**		Desc:	Insert/Update Burden Pool values 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		08/03/2017	brunworg			Change stored procedure name and add
	**										RevisionID parameter.
	**		01/04/2018	ranzalon			BOEJ-2698 Burden Pool Categorization - 
	**										Add IsCommercial bit
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[BurdenPoolLU]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[BurdenPool]
					   ,[Description]
					   ,[IsGaT2ApplicableForMissionSolutions]
					   ,[IsCommercial])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@BurdenPool
					   ,@Description
					   ,@IsGaT2ApplicableForMissionSolutions
					   ,@IsCommercial)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].BurdenPoolLU
					   SET UpdateDate = @UpdateDate
						  ,RevisionID = @RevisionID
						  ,BurdenPool = @BurdenPool
						  ,Description = @Description
						  ,IsGaT2ApplicableForMissionSolutions = @IsGaT2ApplicableForMissionSolutions
						  ,IsCommercial = @IsCommercial
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertCobraFiscalYear.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertCobraFiscalYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCobraFiscalYear];
GO

CREATE PROCEDURE [dbo].[upsertCobraFiscalYear]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Year					int,
	@FiscalYearStartDate	date
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertCobraFiscalYear]
	**		Desc:	Insert/Update a COBRA Fiscal Year 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/11/2017	brunworg			Changed @Year from varchar(50) to int.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].CobraFiscalYearLU
						([UpdateDate]
						,[Year]
						,[FiscalYearStartDate]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Year
						,@FiscalYearStartDate
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].CobraFiscalYearLU
					   SET  UpdateDate = @UpdateDate
							,Year = @Year
							,FiscalYearStartDate = @FiscalYearStartDate
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertFileAttachment.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertFileAttachment.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertFileAttachment];
GO

CREATE PROCEDURE [dbo].[upsertFileAttachment]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Name					varchar(100),
	@Link					varchar(255),
	@SectionId				int = NULL,
	@RevisionId				int
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertFileAttachment]
	**		Desc:	Insert/Update a File Attachment
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 1/2/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[FileAttachment]
						([UpdateDate]
						,[Name]
						,[Link]
						,[SectionId]
						,[RevisionId]
						)
				OUTPUT inserted.FileAttachmentId INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Name
						,@Link
						,@SectionId
						,@RevisionId
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE [FileAttachmentId] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[FileAttachment]
					   SET  UpdateDate = @UpdateDate
							,[Name] = @Name
							,[Link] = @Link
							,[SectionId] = @SectionId
						WHERE 
							FileAttachmentId = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProPricerBurdenRateMap.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProPricerBurdenRateMap.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerBurdenRateMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerBurdenRateMap];
GO

CREATE PROCEDURE [dbo].[upsertProPricerBurdenRateMap]
(
	@Id					INT,
	@UpdateDate			datetime2(7),
	@BurdenPoolID		INT,
	@BurdenElementID	INT,
	@RateCodeID			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProPricerBurdenRateMap]
	**		Desc:	Insert/Update a ProPricer Burden Rate Mapping 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/18/2017	tglick				Removed RevisionID, now part of BurdenPoolLU
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].ProPricerBurdenRateMap
						([UpdateDate]
						,[BurdenPoolID]
						,[BurdenElementID]
						,[RateCodeID]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@BurdenPoolID
						,@BurdenElementID
						,@RateCodeID
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[ProPricerBurdenRateMap] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].ProPricerBurdenRateMap
					   SET  UpdateDate = @UpdateDate
						   ,BurdenPoolID = @BurdenPoolID
						   ,BurdenElementID = @BurdenElementID
						   ,RateCodeID = @RateCodeID
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The ProPricer Burden Rate Mapping with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProPricerRateCodeXref.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProPricerRateCodeXref.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerRateCodeXref];
GO

CREATE PROCEDURE [dbo].[upsertProPricerRateCodeXref]
(
	@Id					INT,
	@UpdateDate			datetime2(7),
	@RateCodeID			INT,
	@Description		varchar(255),
	@RateCodeExtensionID	INT,
	@ResourceClassID	INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProPricerRateCodeXref]
	**		Desc:	Insert/Update a ProPricer Rate Code cross reference
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		12/22/2017	brunworg			Add ResourceClassID field.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].ProPricerRateCodeXref
						([UpdateDate]
						,[RateCodeID]
						,[Description]
						,[RateCodeExtensionID]
						,[ResourceClassID]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@RateCodeID
						,@Description
						,@RateCodeExtensionID
						,@ResourceClassID
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].ProPricerRateCodeXref
					   SET  UpdateDate = @UpdateDate
						   ,RateCodeID = @RateCodeID
						   ,Description = @Description
						   ,RateCodeExtensionID = @RateCodeExtensionID
						   ,ResourceClassID = @ResourceClassID
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRateCode.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRateCode.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRateCode]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRateCode];
GO

CREATE PROCEDURE [dbo].[upsertRateCode]
(
	 @Id int
	,@UpdateDate datetime2(7)
	,@RevisionID int
    ,@CategoryID int
    ,@Description varchar(4000)
    ,@SectionID int
    ,@RateCode varchar(50)
    ,@ResourceTypeID int
    ,@GovernmentBurdenPoolID int
    ,@CommercialBurdenPoolID int
    ,@RateTypeID int
    ,@CobraRateSet varchar(50)
    ,@CobraCode1ID int)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRateCode]
	**		Desc:	Insert/Update Rate Code values 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/05/2017	brunworg			Changed DisclosureSectionID to SectionID.
	**		8/4/2017	rayd				Removed IsDeleted.
	**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
	**										and DataFormat from RateCode table.	
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[RateCode]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[CategoryID]
					   ,[Description]
					   ,[SectionID]
					   ,[RateCode]
					   ,[ResourceTypeID]
					   ,[GovernmentBurdenPoolID]
					   ,[CommercialBurdenPoolID]
					   ,[RateTypeID]
					   ,[CobraRateSet]
					   ,[CobraCode1ID])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@CategoryID
					   ,@Description
					   ,@SectionID
					   ,@RateCode
					   ,@ResourceTypeID
					   ,@GovernmentBurdenPoolID
					   ,@CommercialBurdenPoolID
					   ,@RateTypeID
					   ,@CobraRateSet
					   ,@CobraCode1ID)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[RateCode] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].RateCode
					   SET UpdateDate = @UpdateDate
						  ,RevisionID = @RevisionID
						  ,CategoryID = @CategoryID
						  ,Description = @Description
						  ,SectionID = @SectionID
						  ,RateCode = @RateCode
						  ,ResourceTypeID = @ResourceTypeID
						  ,GovernmentBurdenPoolID = @GovernmentBurdenPoolID
						  ,CommercialBurdenPoolID = @CommercialBurdenPoolID
						  ,RateTypeID = @RateTypeID
						  ,CobraRateSet = @CobraRateSet
						  ,CobraCode1ID = @CobraCode1ID
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Rate Code with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRateCodeYear.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRateCodeYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRateCodeYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRateCodeYear];
GO

CREATE PROCEDURE [dbo].[upsertRateCodeYear]
(
	@Id			INT,
	@UpdateDate	datetime2(7),
	@RateCodeID	INT,
	@Year		INT,
	@Rate		decimal(18,6)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRateCodeYear]
	**		Desc:	Insert/Update Rate Code Year values 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/11/2017	brunworg			Changed @Year from varchar(50) to int.
	**		8/4/2017	rayd				Removed IsDeleted.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].RateCodeYear
						([UpdateDate]
						,[RateCodeID]
						,[Year]
						,[Rate]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@RateCodeID
						,@Year
						,@Rate
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[RateCodeYear] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].RateCodeYear
					   SET  UpdateDate = @UpdateDate
						   ,RateCodeID = @RateCodeID
						   ,Year = @Year
						   ,Rate = @Rate
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Rate Code Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRevision];
GO

CREATE PROCEDURE [dbo].[upsertRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
	,@Revision varchar(50)
    ,@History nvarchar(max)
    ,@CreatedBy varchar(1000) = NULL		-- only used on insert
	,@StartYear int
	,@EndYear int
	,@ReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRevision]
	**		Desc:	Insert/Update a PPR&D Revision 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/07/2017	brunworg			Adjusted parameters as follows:
	**										Removed DateCreated parameter (only set
	**										CreatedBy on insert).
	**										Removed DatePublished and PublishedBy
	**										parameters (created separate 
	**										publishRevision procedure).
	**										Removed InUse and Editing parameters
	**										(created separate lock/unlockRevision
	**										procedures).
	**		08/01/2017	brunworg			Added StartYear and EndYear columns.
	**		10/04/2017	ranzalon			Updating for History and Release Notes
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Revision]
					   ([UpdateDate]
					   ,[Revision]
					   ,[History]
					   ,[DateCreated]
					   ,[CreatedBy]
					   ,[StartYear]
					   ,[EndYear]
					   ,[ReleaseNotes])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@Revision
					   ,@History
					   ,GETDATE()		-- set DateCreated to current date
					   ,@CreatedBy
					   ,@StartYear
					   ,@EndYear
					   ,@ReleaseNotes)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].Revision
					   SET UpdateDate = @UpdateDate
						  ,Revision = @Revision
						  ,History = @History
						  ,StartYear = @StartYear
						  ,EndYear = @EndYear
						  ,ReleaseNotes = @ReleaseNotes
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertSection.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertSection.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSection];
GO

CREATE PROCEDURE [dbo].[upsertSection]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@RevisionID				INT,
	@ParentID				INT,
	@DisplayOrder			INT,
	@Title					varchar(4000),
	@TextContent			nvarchar(max),
	@SectionContentTypeID	INT,
	@IsInternalSection		BIT = 0,
	@DisplayRateCode		BIT = 0,
	@RevisionUniqueSectionId INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertSection]
	**		Desc:	Insert/Update a Section
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/6/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		08/21/2017	brunworg			Add "IsInternalSection" field to Section table.
	**		08/22/2017	brunworg			Remove "IsDeleted" field from all tables.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		10/10/2017	Dusan				Added RevisionUniqueSectionId
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	-- creating a new section, need to generate new RevisionUniqueSectionId
	IF @RevisionUniqueSectionId < 0
	BEGIN
		SELECT @RevisionUniqueSectionId = MAX(RevisionUniqueSectionId) FROM [dbo].[Section]
		SET @RevisionUniqueSectionId = @RevisionUniqueSectionId + 1
	END

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Section]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[ParentID]
					   ,[DisplayOrder]
					   ,[Title]
					   ,[TextContent]
					   ,[SectionContentTypeID]
					   ,[IsInternalSection]
					   ,[DisplayRateCode]
					   ,RevisionUniqueSectionId)
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@ParentID
					   ,@DisplayOrder
					   ,@Title
					   ,@TextContent
					   ,@SectionContentTypeID
					   ,@IsInternalSection
					   ,@DisplayRateCode
					   ,@RevisionUniqueSectionId)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].Section
					   SET  UpdateDate = @UpdateDate
						   ,RevisionID = @RevisionID
					       ,ParentID = @ParentID
					       ,DisplayOrder = @DisplayOrder
					       ,Title = @Title
					       ,TextContent = @TextContent
					       ,SectionContentTypeID = @SectionContentTypeID
						   ,IsInternalSection = @IsInternalSection
					       ,DisplayRateCode = @DisplayRateCode
						   ,RevisionUniqueSectionId = @RevisionUniqueSectionId
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertUserRole.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertUserRole.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertUserLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertUserLog];
GO

CREATE PROCEDURE [dbo].[upsertUserLog]
(
@NTID [varchar](1000),
@DisplayName [varchar](1000),
@Application [varchar](100)
)
AS 
	/******************************************************************************
	**		 
	**		Name:	[upsertUserLog]
	**		Desc:	Insert/Update User Log Entry
	**			
	**		
	**
	**		Auth: RJ Anzalone
	**		Date: 11/22/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**		11/27/17	ranzalon			Remove domain, add application field
	*******************************************************************************/

	SET NOCOUNT ON 
	DECLARE @Today datetime2(7) = GetDate()

	IF EXISTS (SELECT 1 FROM dbo.UserLog WHERE NTID = @NTID AND DisplayName = @DisplayName AND [Application] = @Application)
		UPDATE dbo.UserLog
		SET [LogInUpdateDT] = @Today
		WHERE 
			NTID = @NTID AND
			DisplayName = @DisplayName AND
			[Application] = @Application 		
	ELSE
		INSERT INTO [dbo].[UserLog]
				   ([LogInUpdateDT]
				   ,[NTID]
				   ,[DisplayName]
				   ,[Application])
			 VALUES
				   (@Today, @NTID, @DisplayName, @Application)
GO

/*
    File: \Stored Procedures\_DeleteErrorLogs.sql
*/
PRINT '### Starting file: \Stored Procedures\_DeleteErrorLogs.sql';
-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO

/*
    File: \Table Based Processing\ProPricerRateCodeXrefViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\ProPricerRateCodeXrefViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ProPricerRateCodeXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_ProPricerRateCodeXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_ProPricerRateCodeXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateProPricerRateCodeXrefviaTableParameter]
**		Desc: Update data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[ProPricerRateCodeXref]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Description] = TT.Description,
	[RateCodeExtensionID] = TT.RateCodeExtensionID,
	[ResourceClassID] = TT.ResourceClassID
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID]
				
IF @@ERROR = 0
SELECT	PPX.ID AS ID,
		PPX.[UpdateDate]
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertProPricerRateCodeXrefviaTableParameter]
**		Desc: Insert data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_ProPricerXref table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_ProPricerXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar] (255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_ProPricerXref
SELECT * FROM @ProPricerRateCodeXref

DECLARE @ID [int],
		@RateCodeID [int],
		@Description [varchar](255),
		@RateCodeExtensionID [int],
		@ResourceClassID [int],
		@OrderID [int]

DECLARE @InsertedProPricerXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_ProPricerXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Description = Description,
		@RateCodeExtensionID = RateCodeExtensionID,
		@ResourceClassID = ResourceClassID,
		@OrderID = OrderID
	FROM @TT_ProPricerXref
	WHERE ID < 0

	INSERT INTO [dbo].[ProPricerRateCodeXref]
           ([UpdateDate]
		   ,[RateCodeID]
		   ,[Description]
		   ,[RateCodeExtensionID]
		   ,[ResourceClassID]
		   )
     OUTPUT inserted.ID INTO @InsertedProPricerXref
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Description
           ,@RateCodeExtensionID
 		   ,@ResourceClassID
           ) 
            
	SELECT @ID = ID FROM @InsertedProPricerXref
	
	UPDATE @TT_ProPricerXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_ProPricerXref TT
		ORDER BY OrderID
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteProPricerRateCodeXrefviaTableParameter]
**		Desc: Delete data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[ProPricerRateCodeXref]
WHERE ID IN (SELECT ID FROM @ProPricerRateCodeXref)

GO


/*
    File: \Table Based Processing\RateCodeViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RateCodeViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].updateRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].deleteRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCode' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCode];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCode] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	[CobraRateSet] [varchar](50) NULL,
	[CobraCode1ID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode Table
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2 = GETDATE()

SET @UpdateDate = GetDate()
			
			 
UPDATE [dbo].[RateCode]
	SET 
		[UpdateDate] = TT.[UpdateDate],
		[RevisionID] = TT.[RevisionID],
		[CategoryID] = TT.[CategoryID],
		[Description] = TT.[Description],
		[SectionID] = TT.[SectionID],
		[RateCode] = TT.[RateCode],
		[ResourceTypeID] = TT.[ResourceTypeID],
		[GovernmentBurdenPoolID] = TT.[GovernmentBurdenPoolID],
		[CommercialBurdenPoolID] = TT.[CommercialBurdenPoolID],
		[RateTypeID] = TT.[RateTypeID],
		[CobraRateSet] = TT.[CobraRateSet],
		[CobraCode1ID] = TT.[CobraCode1ID]
FROM [dbo].[RateCode] RC
	INNER JOIN @RateCodeParam TT ON 
		RC.ID = TT.ID 
				
IF @@ERROR = 0
	SELECT 
		RC.ID AS ID, 
		RC.UpdateDate AS UpdateDate 
	FROM @RateCodeParam TT
		INNER JOIN dbo.RateCode RC ON TT.ID = RC.ID
	ORDER BY OrderID
GO
CREATE PROCEDURE [dbo].[deleteRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteRateCodeviaTableParameter]
**		Desc: Delete data from RateCode Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		11/9/2017	ranzalon			Remove ProPricer Burden Rate mappings
*******************************************************************************/
SET NOCOUNT ON 

			DELETE FROM dbo.RateCodeYear
				FROM dbo.RateCodeYear RCY
				INNER JOIN dbo.RateCode RC ON RCY.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerRateCodeXref
				FROM dbo.ProPricerRateCodeXref PPX
				INNER JOIN dbo.RateCode RC ON PPX.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerBurdenRateMap
				FROM dbo.ProPricerBurdenRateMap BRM
				INNER JOIN dbo.RateCode RC ON BRM.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON
					RC.ID = TT.ID
											
			DELETE FROM [dbo].[RateCode]
			FROM [dbo].[RateCode] RC
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The RateCode Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO
CREATE PROCEDURE [dbo].[insertRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode table.
**			
**		
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCode table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCode TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	[CobraRateSet] [varchar](50) NULL,
	[CobraCode1ID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCode
SELECT * FROM @RateCodeParam

DECLARE @ID [int],
		@RevisionID [int],
		@CategoryID [int],
		@Description [varchar](4000),
		@SectionID [int],
		@RateCode [varchar](50),
		@ResourceTypeID [int],
		@GovernmentBurdenPoolID [int],
		@CommercialBurdenPoolID [int],
		@RateTypeID [int],
		@CobraRateSet [varchar](50),
		@CobraCode1ID [int],
		@OrderID [int]

DECLARE @InsertedRateCode AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCode WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RevisionID = RevisionID,
		@CategoryID = CategoryID,
		@Description = Description,
		@SectionID = SectionID,
		@RateCode = RateCode,
		@ResourceTypeID = ResourceTypeID,
		@GovernmentBurdenPoolID = GovernmentBurdenPoolID,
		@CommercialBurdenPoolID = CommercialBurdenPoolID,
		@RateTypeID = RateTypeID,
		@CobraRateSet = CobraRateSet,
		@CobraCode1ID = CobraCode1ID,
		@OrderID =  OrderID
	FROM @TT_RateCode
	WHERE ID < 0

	INSERT INTO [dbo].[RateCode]
           ([UpdateDate]
		   ,[RevisionID]
           ,[CategoryID]
           ,[Description]
           ,[SectionID]
           ,[RateCode]
           ,[ResourceTypeID]
           ,[GovernmentBurdenPoolID]
           ,[CommercialBurdenPoolID]
           ,[RateTypeID]
           ,[CobraRateSet]
           ,[CobraCode1ID]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCode
     VALUES
           (@UpdateDate
		   ,@RevisionID
           ,@CategoryID
           ,@Description
           ,@SectionID
           ,@RateCode
           ,@ResourceTypeID
           ,@GovernmentBurdenPoolID
           ,@CommercialBurdenPoolID
           ,@RateTypeID
           ,@CobraRateSet
           ,@CobraCode1ID
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCode
	
	UPDATE @TT_RateCode
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCode TT
		ORDER BY OrderID
GO

/*
    File: \Table Based Processing\RateCodeYearViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RateCodeYearViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeYearviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCodeYear' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCodeYear];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCodeYear] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18, 6),
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeYearviaTableParameter]
**		Desc: Update data in RateCodeYear Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[RateCodeYear]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Year] = TT.Year,
	[Rate] = TT.Rate
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[ID] = TT.[ID] 
				
IF @@ERROR = 0
SELECT	RCY.ID AS ID,
		RCY.[UpdateDate]
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[RateCodeID] = TT.[RateCodeID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRateCodeYearviaTableParameter]
**		Desc: Insert data in RateCodeYear Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		10/19/2017	Dusan				Fixed rate being nullable
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCodeYear table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCodeYear TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18,6),
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCodeYear
SELECT * FROM @RateCodeYear

DECLARE @ID [int],
		@RateCodeID [int],
		@Year [int],
		@Rate [decimal](18,6),
		@OrderID [int]

DECLARE @InsertedRateCodeYear AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCodeYear WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Year = Year,
		@Rate = Rate,
		@OrderID =  OrderID
	FROM @TT_RateCodeYear
	WHERE ID < 0


	INSERT INTO [dbo].[RateCodeYear]
           ([UpdateDate]
		   ,[RateCodeID]
           ,[Year]
           ,[Rate]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCodeYear
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Year
           ,@Rate
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCodeYear
	
	UPDATE @TT_RateCodeYear
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCodeYear TT
		ORDER BY OrderID
GO

PRINT '###### SCRIPT FINISHED ######';