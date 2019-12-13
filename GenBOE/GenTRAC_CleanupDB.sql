USE [genTRAC]
GO

DECLARE @ID int,
@date datetime2(7)

DECLARE proposalCursor CURSOR FOR
  SELECT ProposalID, UpdateDate FROM Proposal where ProposalTitle like '%Mock%' OR ProposalTitle like 'AutomatedProposal_%'
  
OPEN proposalCursor
FETCH NEXT FROM proposalCursor INTO @ID, @date
WHILE @@FETCH_STATUS = 0
BEGIN

EXEC	[dbo].[deleteProposal]
		@ProposalID = @ID,
		@UpdateDate = @date

  FETCH NEXT FROM proposalCursor INTO @ID, @date
END

CLOSE proposalCursor
DEALLOCATE proposalCursor


DECLARE userCursor CURSOR FOR
  SELECT UserID, UpdateDT FROM genTRACUser where NTID like '%testNtid%' OR NTDomain like '%testdomain%'
  
OPEN userCursor
FETCH NEXT FROM userCursor INTO @ID, @date
WHILE @@FETCH_STATUS = 0
BEGIN

EXEC	[dbo].deletegenTRACUser
		@UserID = @ID,
		@UpdateDT = @date

  FETCH NEXT FROM userCursor INTO @ID, @date
END

CLOSE userCursor
DEALLOCATE userCursor
GO
