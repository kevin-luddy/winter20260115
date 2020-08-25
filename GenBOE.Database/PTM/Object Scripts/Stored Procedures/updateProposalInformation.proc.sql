IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalInformation]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @ProposalStatusID [int],
      @ProposalSubmittalDate [date],
      @ISGSTotalPrice [bigint],
      @PricerChecklistSubmittalDate [date],
      @PeerChecklistSubmittalDate [date],
	  @InformationComments VARCHAR(MAX) = NULL
)
AS
/******************************************************************************
**          
**          Name: [updateProposalInformation]
**          Desc: Manage Proposal Information - Admin Function
**                
**          
**
**          Auth: Don Canuso
**          Date: 7/3/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                -------------------------------
**			8/7/13		dcanuso					Total price now isgs total price
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOEJ-3405 - remove TempProposalSubmittalDate
**			8/10/2020	ranzalon				BOEJ-4649 Manage Proposal Info Comments
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
      BEGIN
      
      SET @UpdateDate = GETDATE()

	  IF @ProposalStatusID IS NOT NULL
		  BEGIN                  
			  UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
						,[ProposalStatusID] = @ProposalStatusID
					 WHERE 
						  ProposalID = @ProposalID
		  END


		IF @ISGSTotalPrice IS NOT NULL OR @ProposalSubmittalDate IS NOT NULL
			BEGIN		
				  UPDATE [dbo].[ProposalChecklist]
						SET  [UpdateDate] = @UpdateDate
							,[ISGSTotalPrice] = IsNull(@ISGSTotalPrice, ISGSTotalPrice)
							/*WI 29822 */ 
							,[ProposalSubmittalDate] =	ISNULL(@ProposalSubmittalDate, [ProposalSubmittalDate])							
						 WHERE 
							  ProposalID = @ProposalID
			END
			
			
		IF @PricerChecklistSubmittalDate IS NOT NULL
			BEGIN
				UPDATE dbo.ProposalChecklistComplete	
					SET	 [UpdateDate] = @UpdateDate
						,[SubmitDate]	= @PricerChecklistSubmittalDate
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = 1 /*Pricer*/
			END


		IF @PeerChecklistSubmittalDate IS NOT NULL
			BEGIN
				UPDATE dbo.ProposalChecklistComplete	
					SET	 [UpdateDate] = @UpdateDate
						,[SubmitDate]	= @PeerChecklistSubmittalDate
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = 2 /*Peer*/
			END

		IF @InformationComments IS NOT NULL
		  BEGIN                  
			  UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
						,[InformationComments] = @InformationComments
					 WHERE 
						  ProposalID = @ProposalID
		  END
    
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