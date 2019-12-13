IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportCobraRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportCobraRates];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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