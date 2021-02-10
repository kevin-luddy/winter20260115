EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.7';
GO

/*
		## START ##
		10/28/19		twilson3			BOEJ-4378 Datamart Geeps update
*/


-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDataMartEmployees]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertDataMartEmployees];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_DatamartEmployee' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_DatamartEmployee];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_DatamartEmployee] AS TABLE(
	[empl_ser_no] varchar(50) NOT NULL,
	[empl_first_nm] varchar(30) NOT NULL,
	[empl_last_nm] varchar(30) NOT NULL,
	[nt_domain_nm] varchar(15) NOT NULL,
	[nt_account_nm] varchar(20) NOT NULL,
	[rpt_to_ser_no] varchar(11) NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertDataMartEmployees]
(
@Employees [dbo].[TT_DatamartEmployee] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertDataMartEmployees]
**		Desc: Insert/Update data into DataMart table
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 10/2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [DataMart].[DataMartEmployee]

INSERT INTO [DataMart].[DataMartEmployee]
           (
			[empl_ser_no],
			[empl_first_nm],
			[empl_last_nm],
			[nt_domain_nm],
			[nt_account_nm],
			[rpt_to_ser_no]
           )
SELECT 	    E.[empl_ser_no],
			E.[empl_first_nm],
			E.[empl_last_nm],
			E.[nt_domain_nm],
			E.[nt_account_nm],
			E.[rpt_to_ser_no]
FROM  @Employees E



IF @@ERROR = 0
SELECT E.OrderID
FROM  @Employees E
INNER JOIN [DataMart].[DataMartEmployee] X ON
	E.[empl_ser_no] = X.[empl_ser_no]
ORDER BY OrderID
GO

/*
		## END ##
		10/28/19		twilson3			BOEJ-4378 Datamart Geeps update
*/