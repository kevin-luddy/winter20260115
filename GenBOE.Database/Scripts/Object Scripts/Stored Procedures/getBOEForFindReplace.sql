IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEForFindReplace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEForFindReplace];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getBOEForFindReplace]
(
	@Search nvarchar (400),
	@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [getBOEForFindReplace]
**				Returns founded information
**					Developer will be padding words with "*SEARCH*"
**					ORDER BY is not defined in wireframes
**					MOQ Equation was removed from wireframes for search
**					Because of the model tool, a temp table is required 
**					The tool could not understand the CASE statements
**		Auth: Don Canuso
**		Date: 3/28/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		4/19/11		dcanuso				Updated based on Develer Requests
**		5/9/11		dcanuso				Because, in theory, a user can enter 
**										100 characters and because
**										the SQL code needs to have spaces, 
**										commas, and double quotes, I 
**										discussed with Developer and will
**										be updating parameters to allow 
**										more charcters - easiest to just double
**										Front end code will limit the input that
**										user will be able to enter
**										Good example is, the following 3-10 character
**										strings turns into 41 characters when you 
**										add commas, spaces, and double quotes
**										('"A123456789", "B123456789", "C123456789",')
**										@Search nvarchar (200) to 400
**		5/26/11		dcanuso				MOQEquation renamed to MOQHoursEquation
**		12/15/11	dcanuso				WI 6443 Update getBOEForFindReplace SP
**										The MOQHoursEquation is a field we no 
**										longer need to search through
**										Removing from code
**		01/24/12	dcanuso				WI 6441: Add all Task Elelemnt Tables:
**										BOE
**										Material
**										ODC
**										Travel Trip
**										Need to generalize Tables
**		4/13/12		dcanuso				WI8356/this goes along with developer bug 8360
**		4/5/13		dcanuso				WI 17282 BOE Title added by Space
**		3/19/14		dcanuso				Bug 27600: SP updates to @Result Table Data Types
**		3/21/19		twilson3			Removed ODC and Material as valid Task Element Type
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Result TABLE
(
	BOEID int,
	BOEDescription varchar(max),
	BOEDescriptionOccurrences int,
	DataSource varchar(MAX),
	DataSourceOccurrences int,
	TaskElementID int, 
	/*
	Task Element Type is for developers
	to know what this Task Element is
	Valid Values:
	BOE
	
	Travel
	*/
	TaskElementType varchar(20), 
	TaskTitle varchar(100), 
	TaskTitleOccurrences int,	
	TaskDescription varchar(max),
	TaskDescriptionOccurrences int,
	MOQText varchar(MAX),
	MOQTextOccurrences int,

	BOETitle varchar(100),
	BOETitleOccurrences int
	
)


INSERT INTO @Result (BOEID, BOEDescription, BOEDescriptionOccurrences, DataSource, DataSourceOccurrences,
						TaskElementID, TaskElementType, TaskTitle, TaskTitleOccurrences, TaskDescription,
						TaskDescriptionOccurrences, MOQText, MOQTextOccurrences,
						BOETitle, BOETitleOccurrences
						)

/*BOE*/						
SELECT 
B.BOEID  AS BOEID, 

B.BOEDescription AS BOEDescription,
CASE
	WHEN CONTAINS(B.BOEDescription, @Search) THEN 
		(LEN(B.BOEDescription) - LEN(REPLACE(B.BOEDescription, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS BOEDescriptionOccurrences,	

B.DataSource AS DataSource,
CASE
	WHEN CONTAINS(B.DataSource, @Search) THEN 
		(LEN(B.DataSource) - LEN(REPLACE(B.DataSource, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS DataSourceOccurrences,	

TE.BOETaskElementID AS TaskElementID, 

CASE 
	WHEN TE.BOETaskElementID IS NULL THEN NULL
	ELSE 'BOE'
END  AS TaskElementType,

TE.TaskTitle AS TaskTitle, 
CASE
	WHEN CONTAINS(TE.TaskTitle, @Search) THEN 
		(LEN(TE.TaskTitle) - LEN(REPLACE(TE.TaskTitle, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS TaskTitleOccurrences,	

TE.TaskDescription AS TaskDescription,
CASE
	WHEN CONTAINS(TE.TaskDescription, @Search) THEN 
		(LEN(TE.TaskDescription) - LEN(REPLACE(TE.TaskDescription, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
END AS TaskDescriptionOccurrences,	

TE.MOQText AS MOQText,
CASE
	WHEN CONTAINS(TE.MOQText, @Search)  THEN 
		(LEN(TE.MOQText) - LEN(REPLACE(TE.MOQText, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*',''))
	ELSE 0
END AS MOQTextOccurrences,




B.BOETitle AS BOETitle,
CASE
	WHEN CONTAINS(B.BOETitle, @Search) THEN 
		(LEN(B.BOETitle) - LEN(REPLACE(B.BOETitle, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS BOETitleOccurrences	
	
FROM dbo.Workspace WS
	INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
	/*LEFT OUTER HERE SO BOEs without children are included*/
	LEFT OUTER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
WHERE
	WS.WorkspaceID = @WorkspaceID AND
		(
			CONTAINS(B.BOEDescription, @Search) OR
			CONTAINS(B.DataSource, @Search) OR
			
			CONTAINS(B.BOETitle, @Search) OR
			
			CONTAINS(TE.TaskTitle, @Search) OR
			CONTAINS(TE.TaskDescription, @Search) OR
			CONTAINS(TE.MOQText, @Search) 
		)
		
/*Need to combine the results from other table*/
/*Travel Trip*/
UNION
SELECT 
B.BOEID  AS BOEID, 

B.BOEDescription AS BOEDescription,
CASE
	WHEN CONTAINS(B.BOEDescription, @Search) THEN 
		(LEN(B.BOEDescription) - LEN(REPLACE(B.BOEDescription, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS BOEDescriptionOccurrences,	

B.DataSource AS DataSource,
CASE
	WHEN CONTAINS(B.DataSource, @Search) THEN 
		(LEN(B.DataSource) - LEN(REPLACE(B.DataSource, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS DataSourceOccurrences,	

TE.TravelTripTaskElementID AS TaskElementID, 

'Travel' AS TaskElementType,

TE.TravelTaskTitle AS TaskTitle, 
CASE
	WHEN CONTAINS(TE.TravelTaskTitle, @Search) THEN 
		(LEN(TE.TravelTaskTitle) - LEN(REPLACE(TE.TravelTaskTitle, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS TaskTitleOccurrences,	

TE.TravelTaskDescription AS TaskDescription,
CASE
	WHEN CONTAINS(TE.TravelTaskDescription, @Search) THEN 
		(LEN(TE.TravelTaskDescription) - LEN(REPLACE(TE.TravelTaskDescription, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
END AS TaskDescriptionOccurrences,	
/*
There is no MOQ for Travel Trip Task Element
TE.MOQText AS MOQText,
CASE
	WHEN CONTAINS(TE.MOQText, @Search)  THEN 
		(LEN(TE.MOQText) - LEN(REPLACE(TE.MOQText, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*',''))
	ELSE 0
END AS MOQTextOccurrences
*/
'' AS MOQText,
0 AS MOQTextOccurrences,

B.BOETitle AS BOETitle,
CASE
	WHEN CONTAINS(B.BOETitle, @Search) THEN 
		(LEN(B.BOETitle) - LEN(REPLACE(B.BOETitle, REPLACE(REPLACE (@Search, '"',''),'*',''), '')))/LEN(REPLACE(REPLACE (@Search, '"',''),'*','')) 
	ELSE 0
	END AS BOETitleOccurrences	

FROM dbo.Workspace WS
	INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
	INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
WHERE
	WS.WorkspaceID = @WorkspaceID AND
		(
			CONTAINS(B.BOEDescription, @Search) OR
			CONTAINS(B.DataSource, @Search) OR
			CONTAINS(B.BOETitle, @Search) OR
			CONTAINS(TE.TravelTaskTitle, @Search) OR
			CONTAINS(TE.TravelTaskDescription, @Search) 
			/*
			There is no MOQ for Travel Trip Task Element
			OR
			CONTAINS(TE.MOQText, @Search) 
			*/
		)		

SELECT DISTINCT
BOEID AS BOEID, 
CASE 
	WHEN BOEDescriptionOccurrences = 0 THEN NULL 
	ELSE BOEDescription 
END	AS BOEDescription, 
BOEDescriptionOccurrences AS BOEDescriptionOccurrences, 
CASE 
	WHEN DataSourceOccurrences = 0 THEN NULL 
	ELSE DataSource 
END AS DataSource, 
DataSourceOccurrences AS DataSourceOccurrences,
CASE
	WHEN TaskTitleOccurrences = 0 AND TaskDescriptionOccurrences = 0 AND MOQTextOccurrences = 0 THEN NULL
	ELSE TaskElementID
END AS TaskElementID, 
CASE
	WHEN TaskTitleOccurrences = 0 AND TaskDescriptionOccurrences = 0 AND MOQTextOccurrences = 0 THEN NULL
	ELSE TaskElementType
END AS TaskElementType,
CASE
	WHEN TaskTitleOccurrences = 0 AND TaskDescriptionOccurrences = 0 AND MOQTextOccurrences = 0 THEN NULL
	ELSE TaskTitle 
END AS TaskTitle, 
TaskTitleOccurrences AS TaskTitleOccurrences, 
CASE
	WHEN TaskTitleOccurrences = 0 AND TaskDescriptionOccurrences = 0 AND MOQTextOccurrences = 0 THEN NULL
	ELSE TaskDescription 
END AS TaskDescription,
TaskDescriptionOccurrences AS TaskDescriptionOccurrences, 
CASE
	WHEN TaskTitleOccurrences = 0 AND TaskDescriptionOccurrences = 0 AND MOQTextOccurrences = 0 THEN NULL
	ELSE MOQText
END AS MOQText, 
MOQTextOccurrences AS MOQTextOccurrences,

CASE 
	WHEN BOETitleOccurrences = 0 THEN NULL 
	ELSE BOETitle 
END	AS BOETitle, 
BOETitleOccurrences AS BOETitleOccurrences

FROM @Result

GO