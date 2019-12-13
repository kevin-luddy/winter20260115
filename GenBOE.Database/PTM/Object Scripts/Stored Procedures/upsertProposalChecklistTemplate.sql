IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalChecklistTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalChecklistTemplate];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalChecklistTemplate]
(
      @ProposalID [int],
	  @ProposalChecklistTypeID [int],
	  @ChangeChecklistFlag [bit]
)
AS
/******************************************************************************
**          
**          Name: [upsertProposalChecklistTemplate]
**          Desc: Insert/Update/Delete Proposal Checklist Skeleton - Unanswered Questions
**                
**          
**
**          Auth: Don Canuso
**          Date: 7/27/2015
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
******************************************************************************/
SET NOCOUNT ON 


IF @ChangeChecklistFlag = 1
BEGIN
	/*THERE IS A CURRENT CHECKLIST THAT NEEDS TO BE REMOVED*/


		/*Determine the current checklist being used for deletion*/
		DECLARE @CurrentProposalAdequacyReviewID INT
		DECLARE @CurrentProposalPricingReviewID INT
		
		SELECT DISTINCT @CurrentProposalAdequacyReviewID = ProposalAdequacyReviewID
		FROM dbo.PARChecklistContent Q
			INNER JOIN dbo.ProposalPARChecklistXREF A ON Q.PARChecklistContentID = A.PARChecklistContentID
		WHERE ProposalID = @ProposalID

		SELECT DISTINCT @CurrentProposalPricingReviewID = ProposalPricingReviewID
		FROM dbo.PPRChecklistContent Q
			INNER JOIN dbo.ProposalPPRChecklistXREF A ON Q.PPRChecklistContentID = A.PPRChecklistContentID
		WHERE ProposalID = @ProposalID


        /*Delete the old/current checklist values*/
        DELETE FROM dbo.ProposalPARChecklistXREF
        FROM dbo.ProposalPARChecklistXREF A
			INNER JOIN 
				(
					SELECT 
						PARChecklistContentID, 
						ProposalAdequacyReviewID
					FROM dbo.PARChecklistContent
					WHERE ProposalAdequacyReviewID = @CurrentProposalAdequacyReviewID
				) Q ON A.PARChecklistContentID = Q.PARChecklistContentID
        WHERE
			A.ProposalID = @ProposalID 
        

        DELETE FROM dbo.ProposalPPRChecklistXREF
        FROM dbo.ProposalPPRChecklistXREF A
			INNER JOIN 
				(
					SELECT 
						PPRChecklistContentID, 
						ProposalPricingReviewID
					FROM dbo.PPRChecklistContent
					WHERE ProposalPricingReviewID = @CurrentProposalPricingReviewID
				) Q ON A.PPRChecklistContentID = Q.PPRChecklistContentID
        WHERE
			A.ProposalID = @ProposalID 



		/*Also need to delete any commnets that were added*/
       DELETE FROM dbo.ProposalChecklistComplete
       WHERE ProposalID = @ProposalID 

END


/*Add New Checklist*/
      
            DECLARE @ProposalAdequacyReviewID INT
            SELECT TOP 1 @ProposalAdequacyReviewID = ProposalAdequacyReviewID
            FROM dbo.ProposalAdequacyReview
            WHERE IsCurrent = 1 AND [ProposalChecklistTypeID] = @ProposalChecklistTypeID
            
            DECLARE @ProposalPricingReviewID INT
            SELECT TOP 1 @ProposalPricingReviewID = ProposalPricingReviewID
            FROM dbo.ProposalPricingReview
            WHERE IsCurrent = 1 AND [ProposalChecklistTypeID] = @ProposalChecklistTypeID           
            
            DECLARE @QuestionID int
            SELECT @QuestionID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Question'

/*
Comments are stored in dbo.ProposalChecklistComplete
Rows for comment answers are not needed for XREF Table
            
            DECLARE @CommentID int
            SELECT @CommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Comment'
            
			DECLARE @PeerCommentID int
			SELECT  @PeerCommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Peer Comment'

			DECLARE @PricerCommentID int
			SELECT @PricerCommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Pricer Comment'
            
*/
            INSERT INTO [dbo].[ProposalPARChecklistXREF]
           ([ProposalID]
           ,[PARChecklistContentID]
           ,[ResponseID]
           ,[ResponseTypeID]
                  )

            SELECT 
                  @ProposalID,
                  [PARChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,1
                  
        FROM [dbo].[PARChecklistContent]
        WHERE ProposalAdequacyReviewID = @ProposalAdequacyReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/
        
            UNION
            SELECT 
                  @ProposalID,
                  [PARChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,2
                  
        FROM [dbo].[PARChecklistContent]
        WHERE ProposalAdequacyReviewID = @ProposalAdequacyReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/


            INSERT INTO [dbo].[ProposalPPRChecklistXREF]
           ([ProposalID]
           ,[PPRChecklistContentID]
           ,[ResponseID]
           ,[ResponseTypeID]
           )

            SELECT 
                  @ProposalID,
                  [PPRChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,1
                  
        FROM [dbo].[PPRChecklistContent]
        WHERE ProposalPricingReviewID = @ProposalPricingReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/
        
        
        
        /*
		First Question in PPR Checklist should be set to NO rather than N/A
		*/
		UPDATE [dbo].[ProposalPPRChecklistXREF]
		SET [ResponseID] = 2 /*NO*/
		WHERE 
				ProposalID = @ProposalID AND
				[ResponseID] = 5 /*NOT SET FROM ABOVE*/ AND
				[ResponseTypeID] = 1 AND
				[PPRChecklistContentID] = 
					(
						SELECT TOP 1 [PPRChecklistContentID]
						FROM [dbo].[PPRChecklistContent]
						WHERE 
						TextTypeID = 4 /*QUESTION*/ AND
						ProposalPricingReviewID = @ProposalPricingReviewID
						ORDER BY SortOrder ASC
					)
GO