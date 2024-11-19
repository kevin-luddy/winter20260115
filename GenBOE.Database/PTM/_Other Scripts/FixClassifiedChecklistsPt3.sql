/*   
	proph-2302 twilson3 PTM Cowan issues

	Classified install is missing checklist 15, so when checklist 16 was created there was nothing to copy from
	1.  Insert the missing checklist 15
	2.  Re-run checklist 16 creation scripts
	3.  Add checklists to Proposals missing them

*/

-- Now to Fix any proposals that are missing checklists
DECLARE @newChecklistId int = 17;
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

	-- this will return which proposals will be affected
    SELECT @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag;
    EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag;
END
GO