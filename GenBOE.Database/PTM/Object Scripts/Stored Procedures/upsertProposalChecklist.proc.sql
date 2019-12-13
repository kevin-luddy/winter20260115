IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalChecklist]
(
	@ProposalChecklistID [int],
	@UpdateDate [datetime2](7),
	@ProposalID [int],
	@ProposalSubmittalDate [date],
	@ISGSTotalPrice [bigint],
	@ProfitFee [bigint],
	@ROSPercentage [decimal](4, 2),
	@LMLaborHours [decimal](11, 2),
	@LMLaborCost [bigint],
	@SubcontractorCost [bigint],
	@MaterialCost [bigint],
	@IWTACost [bigint],
	@TravelCost [bigint],
	@OtherDirectCost [bigint],
	@DeliverChecklistDFARS [bit],
	@AbsoluteValue [bigint] = NULL
)
AS
/******************************************************************************
**		 
**		Name:	[upsertProposalChecklist]
**		Desc:	Insert/Update Proposal Checlist 
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/16/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/27/13		dcanuso				additional columns added
**		11/20/13	dcanuso				Removed LMIS Total Price
**		1/29/14		dcanuso				On the UI, the entire checklist page 
**										is one transaction.
**										The Optimistic locking is affecting users 
**										from saving when 2 users are saving different
**										 sections of the page.  So, a decision was made
**										 to ignore the optimistic locking for this page
**										 and specifically for this SP because only one 
**										user type (pricer) is allowed to edit this 
**										information and it will be controlled 
**										by the application.
**		5/27/14		dcanuso				New Column Added: TempProposalSubmittalDate
**		11/03/15	dturk				New Column Added: TempProposalSubmittalDate
**		1/5/2017	pattoncr			Removing Segment
**      1/5/2017	twilson3			BOEJ-1706 Remove ICE fields
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not 
**										display technical details to the user
**		02/02/17	tglick				added new field [AbsoluteValue]
**		3/20/2017	twilson3			BOEJ-1957 Move CCPD from Checklist to Proposal
**		5/31/2018	ranzalon			BOEJ-3405 - remove TempProposalSubmittalDate
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF @ProposalChecklistID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

INSERT INTO [dbo].[ProposalChecklist]
           ([UpdateDate]
           ,[ProposalID]
           ,[ProposalSubmittalDate]
           ,[ISGSTotalPrice]
           ,[ProfitFee]
           ,[ROSPercentage]
           ,[LMLaborHours]
           ,[LMLaborCost]
           ,[SubcontractorCost]
           ,[MaterialCost]
           ,[IWTACost]
           ,[TravelCost]
           ,[OtherDirectCost]
		   ,[DeliverChecklistDFARS]
		   ,[AbsoluteValue]
           )
     OUTPUT inserted.ProposalChecklistID INTO @Inserted
     VALUES
           (
             @UpdateDate
            ,@ProposalID
            ,@ProposalSubmittalDate
            ,@ISGSTotalPrice
			,@ProfitFee
			,@ROSPercentage
			,@LMLaborHours
			,@LMLaborCost
			,@SubcontractorCost
			,@MaterialCost
			,@IWTACost
			,@TravelCost
			,@OtherDirectCost
			,@DeliverChecklistDFARS
			,@AbsoluteValue
           )

	SELECT @ProposalChecklistID = ID FROM @Inserted
	

	END
ELSE
	/*Update*/
	BEGIN
		/*We are ignoring Optimistic Locking on this page
		IF (SELECT ProposalChecklistID FROM [dbo].[ProposalChecklist] WHERE ProposalChecklistID = @ProposalChecklistID) = @ProposalChecklistID
			BEGIN*/
			
	SET @UpdateDate = GETDATE()


		UPDATE [dbo].[ProposalChecklist]
		   SET [UpdateDate] = @UpdateDate
			  ,[ProposalID] = @ProposalID
			  ,[ProposalSubmittalDate] = @ProposalSubmittalDate
			  ,[ISGSTotalPrice] = @ISGSTotalPrice
			  ,[ProfitFee] = @ProfitFee
			  ,[ROSPercentage] = @ROSPercentage
			  ,[LMLaborHours] = @LMLaborHours
			  ,[LMLaborCost] = @LMLaborCost
			  ,[SubcontractorCost] = @SubcontractorCost
			  ,[MaterialCost] = @MaterialCost
			  ,[IWTACost] = @IWTACost
			  ,[TravelCost] = @TravelCost
			  ,[OtherDirectCost] = @OtherDirectCost
			  ,[DeliverChecklistDFARS] = @DeliverChecklistDFARS
			  ,[AbsoluteValue] = @AbsoluteValue
				 WHERE 
					ProposalChecklistID = @ProposalChecklistID

	END
/*
	Since Optimistic Locking is not allowed, this error will no longer exist			
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Proposal Checklist entry with ID ' + CAST(@ProposalChecklistID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
   
           			
END
*/   

IF @@ERROR = 0
	SELECT @ProposalChecklistID AS ProposalChecklistID

GO