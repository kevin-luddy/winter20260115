IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspace];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[upsertWorkspace]
(
@WorkspaceID int,
@WorkspaceName varchar(115),
@WorkspaceShortName varchar(21), 
@WorkspaceStateID int,
@ContractStartDate date,
@ContractEndDate date,
@ProposalSubmitDate date,
@WorkspaceDescription varchar(1000),
@CostVolumeLeadPricerETIUserID int,
@RFPNumber varchar(100),
@TemplateID int,
@ContainsOCI bit,
@TrackingNumber varchar (100),
@ContainsTemplate bit,
@NumProPricerExport int,
@CreatedByETIUserID int,
@ProposalStatusID int,
@StatusComment varchar(1000),
@PerformingOrganizationListID int,
@ResourceListID int,
@UpdateDT datetime2,
@BOEExportSortByID int,
@SegmentID int,
@LineOfBusinessID int,
@ContractTypeID varchar(100),
@ProposalClassID int,
@ProposalTitle varchar (100),
@ResourcePrecision int,
@RecalculationStartedDate DateTime,
@CostPrecision tinyint,
@IsUsingEquivalentPerson bit,
@IsUsingTM bit,
@ProjectMapTypeID int,
@AllowGridEdit bit,
@CustomSorting int,
@ResourceSorting int,
@PerfOrgSorting int,
@LastProPricerInstance int,
@LastProPricerProposal varchar(50),
@RteSizeLimit int,
@RevisedSubmittalDate DateTime2(7),
@TemplateBoe bit,
@EnableSAPConnection bit,
@CurrentPTMWorkspace bit
)
AS
/******************************************************************************
**          
**          Name: [upsertWorkspace]
**          Desc: Insert/Update Workspace
**                
**          
**
**          Auth: Don Canuso
**          Date: 9/21/2010
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ---------------------------------------
**          4/28/17     Greg Bruwnorth			BOEJ-2129 - Add IsUsingTM Column to Workspace
**			5/03/17		ranzalon				BOEJ-2154 - Add ProjectMapTypeID Column to Workspace
**			5/10/2017	ranzalon				BOEJ-2053 - Remove Proposal Type
**			5/17/2017	ranzalon				BOEJ-2142 - Update Workspace History for Project Map Workspaces
**			5/23/2017	Mike O'Meara			BOEJ-2172 - WS Project Map enum change
**			10/19/17	twilson3				BOEJ-2569 Custom Field Sorting
**			12/4/17		twilson3				BOEJ-2685 - Remove Product Line
**			12/7/2017	twilson3				BOEJ-2250 Remove DTC
**			12/7/17		twilson3				BOEJ-1994 - Remove Summary BOE
**			1/25/18 	twilson3 				BOEJ-2972 Merge Backup Sprocs
**			6/18/18		ranzalon				BOEJ-3448 - ProPricer API updates
**			10/2/18		ranzalon				BOEJ-3699 - RTE Size Limit
**			9/25/19		ranzalon				BOEJ-4349 - Revised Submittal Date
**			8/27/20		ranzalon				BOEJ-4760 - Template Boe
**			1/31/23		e405721					ACV-221 - Enable SAP Connection
**          2/14/24     e374897                 PROPH-1445 - Add CurrentPTMWorkspace Column to Workspace
**          2/28/24     e374897                 PROPH-1674 - Remove CurrentPTMWorkspace logic
*******************************************************************************/

/*
TESTING
SELECT 
@WorkspaceID=1568,@WorkspaceName='_RayTest',
@WorkspaceShortName='raytest',@WorkspaceStateID=1,@ContractStartDate='2014-09-15 12:00:00',
@ContractEndDate='2015-10-15 12:00:00',@ProposalSubmitDate=NULL,@WorkspaceDescription=NULL,
@CostVolumeLeadPricerETIUserID=2161,@RFPNumber='x',
@TemplateID=1,@ContainsOCI=0,@TrackingNumber=NULL,@ContainsTemplate=0,@NumProPricerExport=0,
@CreatedByETIUserID=2161,@ProposalStatusID=0,@StatusComment=NULL,@PerformingOrganizationListID=1548,
@ResourceListID=1569,@UpdateDT='2014-10-09 08:44:47.6030000',@BOEExportSortByID=1,
@SegmentID=1,@LineOfBusinessID=3,@ContractTypeID='',
@ProposalClassID=NULL,@IsgenTracImport=0,@ProposalTitle='_RayTest',@genTracProposalID=NULL,@ResourcePrecision=4,
@IsUsingEquivalentPerson = 0,@IsUsingTM = 0,@ProjectMapTypeID = 1
*/



SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF EXISTS   (SELECT 1 FROM dbo.Workspace 
						WHERE 
							  (WorkspaceName = @WorkspaceName AND @WorkspaceID < 0)  OR  /*Workspace Name is unique*/
							  (WorkspaceShortName = @WorkspaceShortName AND @WorkspaceID < 0)  OR /*Workspace Short Name is unique*/
							  (WorkspaceName = @WorkspaceName AND WorkspaceID <> @WorkspaceID)  OR  /*Workspace Name is unique*/
							  (WorkspaceShortName = @WorkspaceShortName AND WorkspaceID <> @WorkspaceID)   /*Workspace Short Name is unique*/
				  )
	  BEGIN
			/*
				  WorkspaceName and Short Name must be unique 
			*/
						SET @ErrorMessage =   'There already exists a Workspace with Workspace Name ' + @WorkspaceName + '.'
						RAISERROR (
							  @ErrorMessage, -- Message text.
						  11, -- Severity,/*Severity Changed to 11*/
							  1 -- State,
							  )
						RETURN
				  END
				  
				  
DECLARE @InsertedWorkspace AS Table (WorkspaceID int)


/*
Process Contract Types
*/
IF RIGHT(@ContractTypeID, 1) <> ','
	SET @ContractTypeID = @ContractTypeID + ','

DECLARE @ContractType TABLE (ContractTypeID INT, Processed bit default (0))


WHILE (SELECT CHARINDEX (',', @ContractTypeID) ) > 1
BEGIN
	
	INSERT INTO @ContractType (ContractTypeID)
	SELECT LEFT (@ContractTypeID, CHARINDEX (',', @ContractTypeID) -1)
	SET @ContractTypeID = RIGHT (@ContractTypeID, LEN (@ContractTypeID) - CHARINDEX (',', @ContractTypeID) )
	
END

IF @WorkspaceID  < 0  /*Insert Record*/
	  BEGIN
	  
	  SET @UpdateDT = GETDATE()
	  
	  INSERT INTO [dbo].[Workspace]
		   ([WorkspaceName]
		   ,[WorkspaceShortName]
		   ,[WorkspaceStateID]
		   ,[ContractStartDate]
		   ,[ContractEndDate]
		   ,[ProposalSubmitDate]
		   ,[WorkspaceDescription]
		   ,[CostVolumeLeadPricerUserID]
		   ,[RFPNumber]
		   ,[TemplateID]
		   ,[ContainsOCI]
		   ,[TrackingNumber]
		   ,[ContainsTemplate]
		   ,[NumProPricerExport]
		   ,[ProposalStatusID]
		   ,[StatusComment]
		   ,[CreatedByETIUserID]
		   ,[UpdateDT]
		   ,[BOEExportSortByID]
		   ,[SegmentID]
		   ,[LineOfBusinessID]
		   ,[ProposalClassID]
		   ,[ProposalTitle]
		   ,[ResourcePrecision]
		   ,[RecalculationStartedDate]
		   ,[CostPrecision]
		   ,[IsUsingEquivalentPerson]
		   ,[IsUsingTM]
		   ,[ProjectMapTypeID]
		   ,[AllowGridEdit]
		   ,[CustomSorting]
		   ,[ResourceSorting]
		   ,[PerfOrgSorting]
		   ,[LastProPricerInstance]
		   ,[LastProPricerProposal]
		   ,[RteSizeLimit]
		   ,[RevisedSubmittalDate]
		   ,[TemplateBoe]
		   ,[EnableSAPConnection]
		   ,[CurrentPTMWorkspace]
		   )
	 OUTPUT inserted.WorkspaceID INTO @InsertedWorkspace           
	 VALUES
		   (@WorkspaceName
		   ,@WorkspaceShortName
		   ,@WorkspaceStateID
		   ,@ContractStartDate
		   ,@ContractEndDate
		   ,@ProposalSubmitDate
		   ,@WorkspaceDescription
		   ,@CostVolumeLeadPricerETIUserID
		   ,@RFPNumber
		   ,@TemplateID
		   ,@ContainsOCI
		   ,@TrackingNumber
		   ,@ContainsTemplate
		   ,@NumProPricerExport
		   ,@ProposalStatusID
		   ,@StatusComment
		   ,@CreatedByETIUserID
		   ,@UpdateDT
		   ,@BOEExportSortByID
		   ,@SegmentID
		   ,@LineOfBusinessID
		   ,@ProposalClassID
		   ,@ProposalTitle
		   ,@ResourcePrecision
		   ,@RecalculationStartedDate
		   ,@CostPrecision
		   ,@IsUsingEquivalentPerson
		   ,@IsUsingTM
		   ,@ProjectMapTypeID
		   ,@AllowGridEdit
		   ,@CustomSorting
		   ,@ResourceSorting
		   ,@PerfOrgSorting
		   ,@LastProPricerInstance
		   ,@LastProPricerProposal
		   ,@RteSizeLimit
		   ,@RevisedSubmittalDate
		   ,@TemplateBoe
		   ,@EnableSAPConnection
		   ,@CurrentPTMWorkspace
		   )

	  SELECT @WorkspaceID = WorkspaceID FROM @InsertedWorkspace
	  
	  



INSERT INTO [dbo].[WorkspaceContractTypeXREF]
		   ([UpdateDT]
		   ,[WorkspaceID]
		   ,[ContractTypeID])
SELECT
			@UpdateDT,
			@WorkspaceID,
			ContractTypeID
FROM @ContractType

	/* Initialization by default */
	DECLARE @UpdatedWSStateID int = 1;
	
	/* Set State to Working for Project Map Workspaces */
	IF @ProjectMapTypeID = 4 OR @ProjectMapTypeID = 3 SET @UpdatedWSStateID = 2


	  /*On Creation of Workspace, a record needs to be added to Workspace History*/
	  INSERT INTO [dbo].[WorkspaceStateHistory]
		   ([WorkspaceID]
		   ,[CurrentWorkspaceStateID]/* No Current Workspace with a New Workspace - Current is NULL*/
		   ,[UpdatedWorkspaceStateID]
		   ,[ChangedByETIUserID]
		   ,[UpdateDT])
	 VALUES
		   (@WorkspaceID
		   ,0 /*None*/
		   ,@UpdatedWSStateID
		   ,@CreatedByETIUserID
		   ,@UpdateDT)
		   
	  
			/*On creation of a Workspace, the Default Resources and Performing Organizations need to be populated*/      
		/*
				  Resource and Performing Organization List now used 
			*/
			
			IF IsNULL(@ResourceListID,-1) < 1 SET @ResourceListID = 1               
			EXECUTE [dbo].[insertDefaultResource] @WorkspaceID, @ResourceListID, @SegmentID
			
			IF IsNULL(@PerformingOrganizationListID,-1) < 1 SET @PerformingOrganizationListID = 1
			EXECUTE [dbo].[insertDefaultPerformingOrganization] @WorkspaceID, @PerformingOrganizationListID
	  END
ELSE
	  /*Update*/
	  BEGIN
			IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID) = @UpdateDT
				  BEGIN
				  
						SET @UpdateDT = GETDATE()
						
						/*For Workspace State History, need to get the current State*/
						DECLARE @CurrentWorkspaceHistoryStateID int
						SELECT @CurrentWorkspaceHistoryStateID = WorkspaceStateID 
									FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID

						DECLARE @CurrentResourcePrecision INT
						SELECT @CurrentResourcePrecision = IsNull (ResourcePrecision, -99) FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID

						IF IsNull (@CurrentResourcePrecision, -99) <> IsNull (@ResourcePrecision, -99)
							/*If there is a change in Precision, Back Up Workspace*/
							BEGIN
							/*Create a Back Up of the current Workspace*/
							DECLARE @VersionName varchar(50)/*,	@WorkspaceStateID int*/


							/*Bug Fix 33309*/
							SELECT	@VersionName = 
							/*Version Name can be 50 characters*/
							/* 
							[SYS:RPC = 9 => 9
							+ WorkspaceShortName = 21 => 30
							+ _ = 1 => 31
							REPLACE (RTRIM(CAST(CONVERT(DATE,GETDATE()) AS varchar (10))), '-', '') = 8 => 39
							+ '_'  = 1 => 40
							+ RTRIM(CAST(CONVERT(TIME,GETDATE()) AS varchar (8))) = 8 => 48
							+  ']' = 1 => 49
							*/

							'[SYS:RPC ' + @WorkspaceShortName + '_' + 

							REPLACE (RTRIM(CAST(CONVERT(DATE,GETDATE()) AS varchar (10))), '-', '') +

							'_' + 

							RTRIM(CAST(CONVERT(TIME,GETDATE()) AS varchar (8))) +

							 ']', @WorkspaceStateID = WorkspaceStateID 

							FROM dbo.Workspace 
							WHERE WorkspaceID = @WorkspaceID

							EXECUTE  [dbo].[createWorkspaceVersion] @VersionName, @CreatedByETIUserID/*CreatedByETIUserID*/,@WorkspaceStateID,@WorkspaceID

							END
						
						UPDATE [dbo].[Workspace]
						   SET      [WorkspaceName] = @WorkspaceName
									,[WorkspaceShortName] = @WorkspaceShortName
									,[WorkspaceStateID] = @WorkspaceStateID
									,[ContractStartDate] = @ContractStartDate
									,[ContractEndDate] = @ContractEndDate
									,[ProposalSubmitDate] = @ProposalSubmitDate
									,[WorkspaceDescription] = @WorkspaceDescription
									,[CostVolumeLeadPricerUserID] = @CostVolumeLeadPricerETIUserID
									,[RFPNumber] = @RFPNumber
									,[TemplateID] = @TemplateID                                   
									,[ContainsOCI] = @ContainsOCI
									,[TrackingNumber] = @TrackingNumber
									,[ContainsTemplate] = @ContainsTemplate
									,[NumProPricerExport] = @NumProPricerExport
									,[ProposalStatusID] = @ProposalStatusID
									,[StatusComment] = @StatusComment
									,[CreatedByETIUserID] = @CreatedByETIUserID
									,[UpdateDT] = @UpdateDT
									,[BOEExportSortByID] = @BOEExportSortByID
									,[SegmentID] = @SegmentID
									,[LineOfBusinessID] = @LineOfBusinessID
									,[ProposalClassID] = @ProposalClassID
									,[ProposalTitle] = @ProposalTitle
									,[ResourcePrecision] = @ResourcePrecision
									,[RecalculationStartedDate] = @RecalculationStartedDate
									,[CostPrecision] = @CostPrecision
									,[IsUsingEquivalentPerson] = @IsUsingEquivalentPerson
									,[IsUsingTM] = @IsUsingTM
									,[ProjectMapTypeID] = @ProjectMapTypeID
									,[AllowGridEdit] = @AllowGridEdit
									,[CustomSorting] = @CustomSorting
									,[ResourceSorting] = @ResourceSorting
									,[PerfOrgSorting] = @PerfOrgSorting
									,[LastProPricerInstance] = @LastProPricerInstance
									,[LastProPricerProposal] = @LastProPricerProposal
									,[RteSizeLimit] = @RteSizeLimit
									,[RevisedSubmittalDate] = @RevisedSubmittalDate
									,[TemplateBoe] = @TemplateBoe
									,[EnableSAPConnection] = @EnableSAPConnection
									,[CurrentPTMWorkspace] = @CurrentPTMWorkspace
						WHERE 
							  WorkspaceID = @WorkspaceID
							  

						DELETE FROM [dbo].[WorkspaceContractTypeXREF]
						WHERE 
							WorkspaceID = @WorkspaceID  AND
							ContractTypeID NOT IN 
									(
										SELECT ContractTypeID
										FROM 
										@ContractType
									)
							
							
						INSERT INTO [dbo].[WorkspaceContractTypeXREF]
								   ([UpdateDT]
								   ,[WorkspaceID]
								   ,[ContractTypeID])
						SELECT
									@UpdateDT,
									@WorkspaceID,
									ContractTypeID
						FROM @ContractType
						WHERE ContractTypeID NOT IN 
							(
								SELECT ContractTypeID
								FROM [dbo].[WorkspaceContractTypeXREF]
								WHERE WorkspaceID = @WorkspaceID
							)


							  
						/*Log the Workspace State Change*/
						IF @CurrentWorkspaceHistoryStateID <> @WorkspaceStateID
						BEGIN
						INSERT INTO [dbo].[WorkspaceStateHistory]
								 ([WorkspaceID]
								 ,[CurrentWorkspaceStateID]
								 ,[UpdatedWorkspaceStateID]
								 ,[ChangedByETIUserID]
								 ,[UpdateDT])
						VALUES
								 (@WorkspaceID
								 ,@CurrentWorkspaceHistoryStateID
								 ,@WorkspaceStateID
								 ,@CreatedByETIUserID
								 ,@UpdateDT)    
						END
 END            
				  
			ELSE
				  BEGIN
						SET @ErrorMessage =   'The Workspace with Name ' + @WorkspaceName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
						RAISERROR (
							  @ErrorMessage, -- Message text.
						  11, -- Severity,/*Severity Changed to 11*/
							  1 -- State,
							  )
						RETURN
				  END
						
END

IF @@ERROR = 0
	  SELECT @WorkspaceID AS WorkspaceID

GO