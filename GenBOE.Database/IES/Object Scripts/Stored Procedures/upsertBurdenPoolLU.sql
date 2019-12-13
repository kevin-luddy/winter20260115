IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBurdenPoolLU];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertBurdenPoolLU]
(
	 @Id			int
	,@UpdateDate	datetime2(7)
	,@RevisionID	int
	,@BurdenPool	varchar(50)
    ,@Description	varchar(4000)
    ,@IsGaT2ApplicableForMissionSolutions bit
	,@IncludeGaT2InBurdAndCommBurdTables bit
	,@IsCommercial bit
	,@ExcludeFCCOM bit)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertBurdenPoolLU]
	**		Desc:	Insert/Update Burden Pool values 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		08/03/2017	brunworg			Change stored procedure name and add
	**										RevisionID parameter.
	**		01/04/2018	ranzalon			BOEJ-2698 Burden Pool Categorization - 
	**										Add IsCommercial bit
	**		09/18/2018	twilson3			BOEJ-3805 Add ExcludeFCCOM
	**		08/13/2019	ranzalon			BOEJ-4314 IncludeGaT2InBurdAndCommBurdTables column
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[BurdenPoolLU]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[BurdenPool]
					   ,[Description]
					   ,[IsGaT2ApplicableForMissionSolutions]
					   ,[IncludeGaT2InBurdAndCommBurdTables]
					   ,[IsCommercial]
					   ,[ExcludeFCCOM])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@BurdenPool
					   ,@Description
					   ,@IsGaT2ApplicableForMissionSolutions
					   ,@IncludeGaT2InBurdAndCommBurdTables
					   ,@IsCommercial
					   ,@ExcludeFCCOM)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].BurdenPoolLU
					   SET UpdateDate = @UpdateDate
						  ,RevisionID = @RevisionID
						  ,BurdenPool = @BurdenPool
						  ,Description = @Description
						  ,IsGaT2ApplicableForMissionSolutions = @IsGaT2ApplicableForMissionSolutions
						  ,IncludeGaT2InBurdAndCommBurdTables = @IncludeGaT2InBurdAndCommBurdTables
						  ,IsCommercial = @IsCommercial
						  ,ExcludeFCCOM = @ExcludeFCCOM
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO