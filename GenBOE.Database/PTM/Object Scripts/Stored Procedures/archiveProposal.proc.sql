IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[archiveProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[archiveProposal];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[archiveProposal]
(	
	@CreateStartDate [date],
	@CreateEndDate [date],
	@LineOfBusinessID [varchar] (100),
	@ProgramAreaID [varchar] (100)
)
AS
/******************************************************************************
**          
**          Name: [archiveProposal]
**          Desc: Archives all Proposals based on parameters
**                
**					Sample SP Call:
**					@CreateStartDate [date] = '4/1/13',
**					@CreateEndDate [date]='5/1/13',
**					@LineOfBusinessID [varchar] (100) = '1',
**					@ProgramAreaID [varchar] (100)='1'
**
**          Auth: Don Canuso
**          Date: 7/17/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ------------------------------
**			6/06/18		brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
Process Product Lines
*/
DECLARE @LineOfBusinessLU TABLE (LineOfBusinessID INT)

IF @LineOfBusinessID IS NULL OR @LineOfBusinessID = 'ALL'
	BEGIN
		INSERT INTO @LineOfBusinessLU
		SELECT LineOfBusinessID FROM dbo.LineOfBusinessLU
	END
ELSE
	BEGIN
		IF RIGHT(@LineOfBusinessID, 1) <> ','
			SET @LineOfBusinessID = @LineOfBusinessID + ','

	WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
		BEGIN
		      
			  INSERT INTO @LineOfBusinessLU
			  SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			  SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
		      
		END
	END


/*
Process Line Of Business
*/
DECLARE @ProgramAreaLU TABLE (ProgramAreaID INT)

IF @ProgramAreaID IS NULL OR @ProgramAreaID = 'ALL'
	BEGIN
		INSERT INTO @ProgramAreaLU
		SELECT ProgramAreaID FROM dbo.ProgramAreaLU
	END
ELSE
	BEGIN
		IF RIGHT(@ProgramAreaID, 1) <> ','
			SET @ProgramAreaID = @ProgramAreaID + ','

	WHILE (SELECT CHARINDEX (',', @ProgramAreaID) ) > 1
		BEGIN
		      
			  INSERT INTO @ProgramAreaLU
			  SELECT LEFT (@ProgramAreaID, CHARINDEX (',', @ProgramAreaID) -1)
			  SET @ProgramAreaID = RIGHT (@ProgramAreaID, LEN (@ProgramAreaID) - CHARINDEX (',', @ProgramAreaID) )
		      
		END
	END




DECLARE @Archive int
SELECT @Archive = ProposalStatusID FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Archived'

DECLARE @ArchiveCount TABLE (ProposalID int)

UPDATE dbo.Proposal
SET	ProposalStatusID = @Archive
OUTPUT inserted.ProposalID INTO @ArchiveCount
FROM dbo.Proposal P
	INNER JOIN @ProgramAreaLU PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN @LineOfBusinessLU LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
WHERE
	(
		@CreateStartDate IS NULL OR
		P.DateCreated > = @CreateStartDate
	) AND
	(
		@CreateEndDate IS NULL OR
		P.DateCreated < = @CreateEndDate
	)  AND
	P.ProposalStatusID IN  (
							1, /*In Progress*/
							2  /*Completed*/
						   )
	
	
SELECT COUNT (ProposalID) AS ArchiveCount FROM @ArchiveCount

GO