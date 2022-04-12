IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOEFormIBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOEFormIBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOEFormIBOE]
(
@IBOEFormID int,
@UpdateDT datetime2,
@WorkspaceID int,
@FormName varchar(200),
@Description varchar(max) = NULL,
@ProposalTitle varchar(200) = NULL,
@ProposalDate varchar(10) = NULL,
@Poc varchar(65) = NULL,
@PocPhone varchar(60) = NULL,
@Approver varchar(65) = NULL,
@ApproverPhone varchar(60) = NULL,
@BusinessArea varchar(50) = NULL,
@FormVersion int,
@ResourceIDs varchar (max),
@ClinContractTypes varchar(max),
@Revision int
)
AS
/******************************************************************************
**		 
**		Name:	[upsertBOEFormIBOE]
**		Desc:	Insert/Update BOE Forms IBOE
**			
**		
**
**		Auth: Tim Wilson
**		Date: October 2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		10/27/16	twilson3			BOEJ-1531 Add BOE Forms CLIN xref table
**		03/08/17	ranzalon			BOEJ-1944 Increase size of poc fields
**		10/27/17	twilson3			BOEJ-2578 Partial Save PBOE/IBOE
*******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

/*
Process Resource IDs
*/
IF RIGHT(@ResourceIDs, 1) <> ','
	SET @ResourceIDs = @ResourceIDs + ','

DECLARE @Resources TABLE (ResourceID INT)
WHILE (SELECT CHARINDEX (',', @ResourceIDs) ) > 1
	BEGIN	
		INSERT INTO @Resources (ResourceID)
		SELECT LEFT (@ResourceIDs, CHARINDEX (',', @ResourceIDs) -1)
		SET @ResourceIDs = RIGHT (@ResourceIDs, LEN (@ResourceIDs) - CHARINDEX (',', @ResourceIDs) )
	END

/*
Process Clin Contract Types
*/
IF RIGHT(@ClinContractTypes, 1) <> ','
	SET @ClinContractTypes = @ClinContractTypes + ','

DECLARE @Clins TABLE (ClinID INT, ContractType INT)
DECLARE @Breaker INT
WHILE (SELECT CHARINDEX (',', @ClinContractTypes) ) > 1
	BEGIN	
		SET @Breaker = CHARINDEX (':', @ClinContractTypes)
		INSERT INTO @Clins (ClinID, ContractType)
		SELECT LEFT (@ClinContractTypes, @Breaker -1), SUBSTRING (@ClinContractTypes, @Breaker + 1, CHARINDEX (',', @ClinContractTypes) - @Breaker - 1)
		SET @ClinContractTypes = RIGHT (@ClinContractTypes, LEN (@ClinContractTypes) - CHARINDEX (',', @ClinContractTypes) )
	END

DECLARE @InsertedIBOE AS Table (IBOEID int)

IF @IBOEFormID  < 0  /*Insert Record*/
	BEGIN	
		SET @UpdateDT = GETDATE()	
		INSERT INTO [dbo].[BOEFormIBOE]
			   (
				[UpdateDT]
			    ,[WorkspaceID]
				,[FormName]
				,[Description]
				,[ProposalTitle]
				,[ProposalDate]
				,[Poc]
				,[PocPhone]
				,[Approver]
				,[ApproverPhone]
				,[BusinessArea]
				,[Revision]
				,[FormVersion]
			   )
		OUTPUT inserted.IBOEFormID INTO @InsertedIBOE            
		VALUES
			   (
			   @UpdateDT,
			   @WorkspaceID,
			   @FormName,
			   @Description,
			   @ProposalTitle,
			   @ProposalDate,
			   @Poc,
			   @PocPhone,
			   @Approver,
			   @ApproverPhone,
			   @BusinessArea,
			   0, -- Revision always starts at 0
			   @FormVersion
			   )
		SELECT @IBOEFormID = IBOEID FROM @InsertedIBOE
	
		IF EXISTS (SELECT * FROM @Resources)
			BEGIN 
				INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
				   ([IBOEFormID]
				   ,[ResourceID]
				   )
				 SELECT
				   @IBOEFormID,
				   ResourceID
				   FROM @Resources
			END	

		IF EXISTS (SELECT * FROM @Clins)
			BEGIN 
				INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
				   ([IBOEFormID]
				   ,[ClinID]
				   ,[ContractType]
				   )
				 SELECT
				   @IBOEFormID,
				   [ClinID],
				   [ContractType]
				   FROM @Clins
			END	
	END	
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[BOEFormIBOE] WHERE [IBOEFormID] = @IBOEFormID) = @UpdateDT
			BEGIN			
				SET @UpdateDT = GETDATE()				
				UPDATE [dbo].[BOEFormIBOE]
					SET 
						[UpdateDT] = @UpdateDT,
						[FormName] = @FormName,
						[Description] = @Description,
						[ProposalTitle] = @ProposalTitle,
						[ProposalDate] = @ProposalDate,
						[Poc] = @Poc,
						[PocPhone] = @PocPhone,
						[Approver] =@Approver,
						[ApproverPhone] = @ApproverPhone,
						[BusinessArea] = @BusinessArea,
						[Revision] = @Revision

				WHERE
					[IBOEFormID] = @IBOEFormID

				DELETE FROM [dbo].[BOEFormIBOEResourcesXREF] WHERE [IBOEFormID] = @IBOEFormID
				DELETE FROM [dbo].[BOEFormIBOECLINsXREF] WHERE [IBOEFormID] = @IBOEFormID

				IF EXISTS (SELECT * FROM @Resources)
				BEGIN 
					INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
						([IBOEFormID]
						,[ResourceID]
						)
						SELECT
						@IBOEFormID,
						ResourceID
						FROM @Resources
				END	

				IF EXISTS (SELECT * FROM @Clins)
				BEGIN 
					INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
					   ([IBOEFormID]
					   ,[ClinID]
					   ,[ContractType]
					   )
					 SELECT
					   @IBOEFormID,
					   [ClinID],
					   [ContractType]
					   FROM @Clins
				END	
			END		
			ELSE
				BEGIN
				
					SET @ErrorMessage =   'The IBOE has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END	
	END

IF @@ERROR = 0
	SELECT	@IBOEFormID AS IBOEFormID
GO