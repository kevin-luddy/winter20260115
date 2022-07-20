EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.25';
GO

/*
                ## START ##

                7/18/2021 [Dusan/RJ] - IES-1496 - Add Cage Codes
*/

IF OBJECT_ID('dbo.CageCodes', 'U') IS NULL
BEGIN
                CREATE TABLE dbo.CageCodes (
                                CageCode                                                           VARCHAR(10)                    PRIMARY KEY,
                                Address1                                                             VARCHAR(100)  NOT NULL,
                                Address2                                                             VARCHAR(100)  NULL,
                                City                                                                        VARCHAR(50)                    NOT NULL,
                                "State"                                                                 VARCHAR(2)                      NOT NULL,
                                Zip                                                                                          VARCHAR(10)                    NOT NULL
                ); 
END
GO

IF NOT EXISTS(SELECT 1 FROM CageCodes)
BEGIN
                INSERT INTO CageCodes (CageCode, Address1, Address2, City, "State", Zip)
                VALUES ('02GJ5', '6801 Rockledge Dr', NULL, 'Bethesda' ,'MD' ,'20817-1877'),
                                                ('04236', '12257 S Wadsworth Blvd', NULL, 'Littleton' ,'CO' ,'80125-8504'),
                                                ('06887', '1111 Lockheed Martin Way BLDG 157', NULL, 'Sunnyvale' ,'CA' ,'94089-1212'),
                                                ('53100', '13800 Old Gentilly Road', NULL, 'New Orleans' ,'LA' ,'70129-2218'),
                                                ('58691', '6304 Spine Rd', NULL, 'Boulder' ,'CO', '80301-3320'),
                                                ('65113', '3251 Hanover St', NULL, 'Palo Alto' ,'CA' ,'94304-1121'),
                                                ('79272', '230 Mall Blvd', NULL, 'King of Prussia' ,'PA' ,'19406-2902'),
                                                ('3CTQ3', '7474 Greenway Center Dr STE 200', NULL, 'Greenbelt' ,'MD' ,'20770-3504'),
                                                ('3VQD6', '9970 Federal Dr', NULL, 'Colorado Springs' ,'CO' ,'80921-3616'),
                                                ('3VUJ2', '3100 Zanker Rd', NULL, 'San Jose' ,'CA' ,'95134-1965'),
                                                ('3YXP0', '3201 Airpark Dr STE 204', NULL, 'Santa Maria' ,'CA' ,'93455-1833'),
                                                ('4MZH8', '13560 Dulles Technology Dr', NULL, 'Herndon' ,'VA' ,'20171-3414'),
                                                ('4MZK6', '13560 Dulles Technology Dr', NULL, 'Herndon' ,'VA' ,'20171-3414'),
                                                ('5D177', '4800 Bradford DR NW', NULL, 'Huntsville' ,'AL' ,'35805-1949'),
                                                ('7MU49', '700 N Frederick Ave', NULL, 'Gaithersburg' ,'MD' ,'20879-3328'),
                                                ('7MU70', '12999 Deer Creek Canyon Blvd', NULL, 'Littleton' ,'CO' ,'80127-5146'),
                                                ('0BHW2', '1111 Lockheed Martin Way', NULL, 'Sunnyvale' ,'CA' ,'94089-1212'),
                                                ('2J881', '135 S Taylor Ave', NULL, 'Louisville' ,'CO' ,'80027-3025'),
                                                ('7MTC3', '700 N. Frederick Ave', NULL, 'Gaithersburg' ,'MD' ,'20879-3328'),
                                                ('91ZJ7', '655 Space Center Dr', NULL, 'Colorado Springs' ,'CO' ,'80915-3604'),
                                                ('7X6A9', '1102 John Glenn Blvd', NULL, 'Titusville' ,'FL' ,'32780-7910'),
                                                ('08YH6', '480 Wooten Road, Suite 104', NULL, 'Colorado Springs' ,'CO' ,'80916-4712');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CageCode' AND Object_ID = Object_ID(N'dbo.ProposalContractsData'))
BEGIN
    ALTER TABLE dbo.ProposalContractsData
        ADD CageCode VARCHAR(10) NULL,
        FOREIGN KEY(CageCode) REFERENCES dbo.CageCodes(CageCode);
END
GO

/*
                ## END ##

                7/18/2021 [Dusan/RJ] - IES-1496 - Add Cage Codes
*/

/*
                ## START ##

                7/19/2021 [RJ] - IES-1504 - Contract Action Type
*/

IF OBJECT_ID('dbo.ContractActionTypeLU', 'U') IS NULL
BEGIN
                CREATE TABLE dbo.ContractActionTypeLU (
                        ID                      int             PRIMARY KEY,
                        ContractActionType      VARCHAR(100)    NOT NULL
                ); 
END
GO

IF NOT EXISTS(SELECT 1 FROM dbo.ContractActionTypeLU)
BEGIN
                INSERT INTO dbo.ContractActionTypeLU (ID, ContractActionType)
                VALUES (1, 'New Contract'),
                        (2, 'Letter Contract'),
                        (3, 'Change Order'),
                        (4, 'Unpriced Order'),
                        (5, 'Price Revision / Redetermination'),
                        (6, 'Other');
END
GO

IF COL_LENGTH('dbo.Proposal', 'ContractActionType') IS NULL
BEGIN
    ALTER TABLE dbo.Proposal
        ADD ContractActionType int NULL,
            ContractActionTypeOtherText VARCHAR(100) NULL,
        FOREIGN KEY(ContractActionType) REFERENCES dbo.ContractActionTypeLU(ID);
END

/*
                ## END ##

                7/19/2021 [RJ] - IES-1504 - Contract Action Type
*/

/*
                ## START ##

                7/19/2021 [RJ] - IES-1505 - Cost through COM
*/

IF COL_LENGTH('dbo.Proposal', 'CostThroughCom') IS NULL
BEGIN
    ALTER TABLE dbo.ProposalChecklist
        ADD CostThroughCom bigint NULL;
END

/*
                ## END ##

                7/19/2021 [RJ] - IES-1505 - Cost through COM
*/
