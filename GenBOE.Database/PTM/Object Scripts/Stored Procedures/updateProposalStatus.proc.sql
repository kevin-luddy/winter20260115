IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalStatus];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalStatus]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @ProposalStatusID [int]
)
AS
/******************************************************************************
**          
**          Name: [updateProposalStatus]
**          Desc: Update Proposal Status
**                
**          
**
**          Auth: Don Canuso
**          Date: 6/14/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOEJ-3405 - remove TempProposalSubmittalDate
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

            IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
                  BEGIN
                  
                  SET @UpdateDate = GETDATE()
                              
                  UPDATE [dbo].[Proposal]
                        SET  [UpdateDate] = @UpdateDate
                          ,[ProposalStatusID] = @ProposalStatusID
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