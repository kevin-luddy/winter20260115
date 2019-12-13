IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateUserAccessReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateUserAccessReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateUserAccessReport]
(
 	@Duration int = NULL
)
AS

SET NOCOUNT ON 

SELECT 	
	A.[DisplayName],
	A.[NTID], 
	CASE 
		WHEN A.[LogInUpdateDT] IS NULL THEN 'Access Date Unavailable' 
		ELSE A.[LogInUpdateDT]
	END AS [LogInUpdateDT]
FROM
(
SELECT DISTINCT 
	U.[DisplayName],
	U.[NTID], 
 convert(varchar(20),A.[LogInUpdateDT],101)+ ' '+
 convert(varchar(20),A.[LogInUpdateDT],108)+ ' ' +
 right(convert(varchar(30),A.[LogInUpdateDT],109),2)
AS [LogInUpdateDT]
/*
For Testing:
	,
DateDiff (DD, A.[LogInUpdateDT], getDate()) 
*/
FROM [dbo].[ETIuser] U
	INNER JOIN [dbo].[UserAccessReport] A ON 
		A.[NTID] = U.[NTID]
WHERE 	
	A.[NTID] IS NOT NULL AND
	(
	DateDiff (DD, A.[LogInUpdateDT], getDate()) >= @Duration 
	OR
	@Duration IS NULL
	)

UNION

SELECT DISTINCT 
	U.[DisplayName],
	U.[NTID], 
 convert(varchar(20),A.[LogInUpdateDT],101)+ ' '+
 convert(varchar(20),A.[LogInUpdateDT],108)+ ' ' +
 right(convert(varchar(30),A.[LogInUpdateDT],109),2)
AS [LogInUpdateDT]
/*
For Testing:
	,
DateDiff (DD, A.[LogInUpdateDT], getDate()) 
*/
FROM [dbo].[ETIuser] U
	LEFT OUTER JOIN [dbo].[UserAccessReport] A ON 
		A.[NTID] = U.[NTID]
WHERE 
	A.[NTID] IS NULL AND
	(
		DateDiff (DD, A.[LogInUpdateDT], getDate()) >= @Duration 
		OR
		@Duration IS NULL
	)
) A 
ORDER BY 
	YEAR (A.[LogInUpdateDT]) DESC,
	MONTH (A.[LogInUpdateDT]) DESC,
	DAY (A.[LogInUpdateDT]) DESC

GO