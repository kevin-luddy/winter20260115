IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOECommentHistory]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOECommentHistory];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertBOECommentHistory]
(
	@BOEID int,
	@FieldID int,
	@CurrentComment varchar(500),
	@UpdatedComment varchar(500),
	@ChangedByETIUserID int,
	@UpdateDT datetime2(7)
)           
AS
/******************************************************************************
**		 
**		Name: insertBOECommentHistory
**		Desc: Inserts a record into the BOE Comment History table for DTO
**			
**		
**
**		Auth: Don Canuso
**		Date: 11/16/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

INSERT INTO [dbo].[BOECommentHistory]
           (
			[BOEID]
           ,[FieldID]
           ,[CurrentComment]
           ,[UpdatedComment]
           ,[ChangedByETIUserID]
           ,[UpdateDT])
     VALUES
			(
			@BOEID,
			@FieldID,
			@CurrentComment,
			@UpdatedComment,
			@ChangedByETIUserID,
			@UpdateDT
			)
GO