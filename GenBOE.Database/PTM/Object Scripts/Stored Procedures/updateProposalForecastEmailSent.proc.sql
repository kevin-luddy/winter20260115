IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalForecastEmailSent]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalForecastEmailSent];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalForecastEmailSent]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7)
)
AS
/******************************************************************************
**          
**          Name: [updateProposalForecastEmailSent]
**          Desc: Update Proposal Forecast Email Sent
**                
**          
**
**          Auth: twilson3
**          Date: 3/14/18
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
    BEGIN
                  
        SET @UpdateDate = GETDATE()
                              
        UPDATE [dbo].[Proposal]
            SET  [UpdateDate] = @UpdateDate
                ,[ForecastEmailSent] = 1
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

GO