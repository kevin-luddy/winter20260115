IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteChecklist];

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteChecklist]
(
@ProposalID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteChecklist]
**		Desc: Deletes the Checklist for a Proposal.  Needed when switching the ProposalClass to Forecasted.
**			
**		
**
**		Auth: Chris Patton
**		Date: 04/10/18
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      04/10/2018	pattoncr			Initial creation.  BOEJ-3301 - Checklist and Post Submittal Attachments not cleared when changing a Proposal to Forecasted
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProposalChecklist] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID;
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Proposal Checklist for the Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO