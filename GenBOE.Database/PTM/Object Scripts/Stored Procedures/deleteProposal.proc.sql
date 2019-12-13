IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposal];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposal]
(
@ProposalID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposal]
**		Desc: Delete Proposal
**			
**		
**
**		Auth: Don Canuso
**		Date: 04/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      12/21/2016	twilson3			BOEJ-1143 Approval Emailer -- remove old Email tables/stored procs
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
**		9/7/2017	twilson3			BOEJ-2459 Add Attachment table
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.Attachment WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalUserRole WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractTypeXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalCostElementXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Proposal WHERE ProposalID = @ProposalID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO