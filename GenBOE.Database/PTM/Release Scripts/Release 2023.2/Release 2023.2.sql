EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.2';
GO

/*
                ## START ##

                2/22/23 [RJ] - ACV-325 - Customer Due Date
*/

IF COL_LENGTH('dbo.ProposalContractsData', 'CustomerDueDate') IS NULL
BEGIN
    ALTER TABLE dbo.ProposalContractsData
         ADD CustomerDueDate DATE
END
GO

/*
                ## END ##

                2/22/23 [RJ] - ACV-325 - Customer Due Date
*/