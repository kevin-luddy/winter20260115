IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[unlockProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[unlockProposal];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[unlockProposal]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @UnlockOptionID [int]
)
AS
/******************************************************************************
**          
**          Name: [unlockProposal]
**          Desc: unlock Proposal using Proposal Update Date
**                
**
**          Auth: Don Canuso
**          Date: 7/2/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOJE-3405 - remove TempProposalSubmittalDate
**			9/5/2018	twilson3				BOEJ-3761 Remove id for new Submitted status
**			9/11/2018	twilson3				BOEJ-3818 Change status to In progress if submitted
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
If the proposal is not Active (i.e., Archived, Revision, Deleted), 
then no one should be able to unlock the checklist.
*/
IF EXISTS (
				SELECT ProposalStatusID 
				FROM dbo.Proposal 
				WHERE ProposalID = @ProposalID AND
				ProposalStatusID IN (3,4,5)
				) 
                  BEGIN
                        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' is in a state that can not be unlocked'
                        RAISERROR (
                              @ErrorMessage, -- Message text.
                          11, -- Severity,/*Severity Changed to 11*/
                              1 -- State,
                              )
                        RETURN
                  END

			
IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
      BEGIN                  
      SET @UpdateDate = GETDATE()

	  /*
		UnlockOptionID
			1.Unlock Pricer
			2. Unlock Peer
			3. Unlock both Pricer and Peer
	  */      
      UPDATE dbo.ProposalChecklistComplete
      SET 
		UpdateDate = @UpdateDate,
		SubmitDate = NULL 
      WHERE 
      ProposalID = @ProposalID AND 
      (
		  (
				ResponseTypeID = 1 /*Pricer*/ AND
				@UnlockOptionID = 1
		  ) OR
			(
			ResponseTypeID = 2 /*Peer*/ AND
			@UnlockOptionID = 2
		  ) OR
		( 
			@UnlockOptionID = 3 AND
			(
				ResponseTypeID = 1 /*Pricer*/ OR 
				ResponseTypeID = 2 /*Peer*/
			) 
		) 
	)



/*
Completed/Submitted States would change to InProgess
*/
UPDATE [dbo].[Proposal] 
    SET  [ProposalStatusID] = 1 /*In Progress*/
     WHERE 
          ProposalID = @ProposalID AND
          (ProposalStatusID = 2 /*Completed*/ OR
		   ProposalStatusID = 6) /*Submitted*/


/*Update Date for the Proposal gets updated*/
UPDATE [dbo].[Proposal] 
    SET  [UpdateDate] = @UpdateDate
     WHERE 
          ProposalID = @ProposalID

END
                  
            ELSE
                  BEGIN
                        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
                        RAISERROR (
                              @ErrorMessage, -- Message text.
                          11, -- Severity,/*Severity Changed to 11*/
                              1 -- State,
                              )
                        RETURN
                  END
   
   

   
                        
IF @@ERROR = 0
      SELECT @ProposalID as ProposalID

GO