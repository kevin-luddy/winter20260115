IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteReplication]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteReplication]
	**		Desc:	Delete a Rate Code Replication
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 3/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT UpdateDate FROM [dbo].[RateCodeReplication] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[RateCodeReplication] WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Rate Code Replication with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO
