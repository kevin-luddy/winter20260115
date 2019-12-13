IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMSTZoneTravelOrigin]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMSTZoneTravelOrigin];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertMSTZoneTravelOrigin]
(
	@OriginID int,
	@Origin varchar (100),
	@Site varchar (10),
	@ResPRZ1ID int,
	@ResPRZ1 varchar (20),
	@ResPRZ2ID int,
	@ResPRZ2 varchar (20),
	@ResPRZ3ID int,
	@ResPRZ3 varchar (20),
	@ResPRZ4ID int,
	@ResPRZ4 varchar (20),
	@ResPRZ5ID int,
	@ResPRZ5 varchar (20),
	@ResPRZ6ID int,
	@ResPRZ6 varchar (20),
	@ResTRZ1ID int,
	@ResTRZ1 varchar (20),
	@ResTRZ2ID int,
	@ResTRZ2 varchar (20),
	@ResTRZ3ID int,
	@ResTRZ3 varchar (20),
	@ResTRZ4ID int,
	@ResTRZ4 varchar (20),
	@ResTRZ5ID int,
	@ResTRZ5 varchar (20),
	@ResTRZ6ID int,
	@ResTRZ6 varchar (20)
)
AS
/******************************************************************************
**		 
**		Name: [upsertMSTZoneTravelOrigin]
**		Desc: Update/Insert an MST Zone Travel Origin and its 12 Resources  
**
**		Auth: RJ Anzalone
**		Date: 8/16/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/16/16		ranzalon			SP created. Updates/Inserts Origin and its 12 resources.
**		9/01/16		ranzalon			Bug fix
******************************************************************************/
SET NOCOUNT ON

DECLARE @InsertedOrigin AS TABLE (OriginID int)
DECLARE @ErrorMessage varchar (500)

IF @OriginID < 0 /* Insert */
BEGIN	
	IF EXISTS (SELECT 1 FROM dbo.MSTZoneTravelOrigin WHERE Origin = @Origin) /* Duplicate Check */
	BEGIN
		SET @ErrorMessage =   'A Zone Travel Origin with Origin ' + @Origin + ' already exists.'
			RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[MSTZoneTravelOrigin]
			OUTPUT inserted.OriginID INTO @InsertedOrigin
			VALUES
			(
				@Origin,
				@Site
			)		
		SELECT @OriginID = OriginID FROM @InsertedOrigin		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ1,
			1,
			0,
			@OriginID,
			@Origin + 'PR1',
			@Origin + ' Per Diem/Misc Zone1'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ2,
			2,
			0,
			@OriginID,
			@Origin + 'PR2',
			@Origin + ' Per Diem/Misc Zone2'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ3,
			3,
			0,
			@OriginID,
			@Origin + 'PR3',
			@Origin + ' Per Diem/Misc Zone3'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ4,
			4,
			0,
			@OriginID,
			@Origin + 'PR4',
			@Origin + ' Per Diem/Misc Zone4'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ5,
			5,
			0,
			@OriginID,
			@Origin + 'PR5',
			@Origin + ' Per Diem/Misc Zone5'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResPRZ6,
			6,
			0,
			@OriginID,
			@Origin + 'PR6',
			@Origin + ' Per Diem/Misc Zone6'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ1,
			1,
			1,
			@OriginID,
			@Origin + 'TR1',
			@Origin + ' Airfare Zone1'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ2,
			2,
			1,
			@OriginID,
			@Origin + 'TR2',
			@Origin + ' Airfare Zone2'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ3,
			3,
			1,
			@OriginID,
			@Origin + 'TR3',
			@Origin + ' Airfare Zone3'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ4,
			4,
			1,
			@OriginID,
			@Origin + 'TR4',
			@Origin + ' Airfare Zone4'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ5,
			5,
			1,
			@OriginID,
			@Origin + 'TR5',
			@Origin + ' Airfare Zone5'
		)		
		INSERT INTO [dbo].[MSTZoneTravelResource]
		VALUES
		(
			@ResTRZ6,
			6,
			1,
			@OriginID,
			@Origin + 'TR6',
			@Origin + ' Airfare Zone6'
		)		
	END
END
ELSE /* Update */
BEGIN	
	IF EXISTS (SELECT 1 FROM dbo.MSTZoneTravelOrigin WHERE Origin = @Origin AND OriginID <> @OriginID) /* Duplicate Check */
	BEGIN
		SET @ErrorMessage =   'A Zone Travel Origin with Origin ' + @Origin + ' already exists.'
		RAISERROR (
			@ErrorMessage, -- Message text.
			11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END																	
	ELSE
	BEGIN	
		UPDATE [dbo].[MSTZoneTravelOrigin]
		SET
			[Origin] = @Origin,
			[Site] = @Site
		WHERE [OriginID] = @OriginID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ1,
			[LookupValue] = @Origin + 'PR1',
			[Description] = @Origin + ' Per Diem/Misc Zone1'
		WHERE [ResourceID] = @ResPRZ1ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ2,
			[LookupValue] = @Origin + 'PR2',
			[Description] = @Origin + ' Per Diem/Misc Zone2'
		WHERE [ResourceID] = @ResPRZ2ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ3,
			[LookupValue] = @Origin + 'PR3',
			[Description] = @Origin + ' Per Diem/Misc Zone3'
		WHERE [ResourceID] = @ResPRZ3ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ4,
			[LookupValue] = @Origin + 'PR4',
			[Description] = @Origin + ' Per Diem/Misc Zone4'
		WHERE [ResourceID] = @ResPRZ4ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ5,
			[LookupValue] = @Origin + 'PR5',
			[Description] = @Origin + ' Per Diem/Misc Zone5'
		WHERE [ResourceID] = @ResPRZ5ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResPRZ6,
			[LookupValue] = @Origin + 'PR6',
			[Description] = @Origin + ' Per Diem/Misc Zone6'
		WHERE [ResourceID] = @ResPRZ6ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ1,
			[LookupValue] = @Origin + 'TR1',
			[Description] = @Origin + ' Airfare Zone1'
		WHERE [ResourceID] = @ResTRZ1ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ2,
			[LookupValue] = @Origin + 'TR2',
			[Description] = @Origin + ' Airfare Zone2'
		WHERE [ResourceID] = @ResTRZ2ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ3,
			[LookupValue] = @Origin + 'TR3',
			[Description] = @Origin + ' Airfare Zone3'
		WHERE [ResourceID] = @ResTRZ3ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ4,
			[LookupValue] = @Origin + 'TR4',
			[Description] = @Origin + ' Airfare Zone4'
		WHERE [ResourceID] = @ResTRZ4ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ5,
			[LookupValue] = @Origin + 'TR5',
			[Description] = @Origin + ' Airfare Zone5'
		WHERE [ResourceID] = @ResTRZ5ID		
		UPDATE [dbo].[MSTZoneTravelResource]
		SET
			[Resource] = @ResTRZ6,
			[LookupValue] = @Origin + 'TR6',
			[Description] = @Origin + ' Airfare Zone6'
		WHERE [ResourceID] = @ResTRZ6ID	
	END	
END

IF @@ERROR = 0
	SELECT @OriginID AS [OriginID];

GO