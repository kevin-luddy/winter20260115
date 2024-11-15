-- Find proposals that are missing checklists
-- Uncomment the Execute to copy the latest checklists into those proposals

DECLARE @newChecklistId int = 19;
DECLARE @ChangeChecklistFlag bit = 1;
DECLARE @ProposalChecklistTypeID int = 1;
DECLARE @cur CURSOR
SET @cur = CURSOR STATIC FOR
    select p.ProposalID from Proposal p  where p.ProposalID not in (
    select distinct ProposalID from ProposalPPRChecklistXREF)    
DECLARE @ProposalID int
OPEN @cur

WHILE 1 = 1
BEGIN
     FETCH @cur INTO @ProposalID
     IF @@fetch_status <> 0
        BREAK

    SELECT @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag;
    --EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag;
END
GO