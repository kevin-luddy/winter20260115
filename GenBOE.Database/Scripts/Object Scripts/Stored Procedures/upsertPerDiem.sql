IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertPerDiem]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertPerDiem];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertPerDiem]
(
@PerDiemID int,
@PerDiemDestination varchar(40),
@Qualification varchar(40) ,
@HotelRate decimal(7, 2),
@MIERate decimal(6, 2),
@PerDiemNotes varchar(100),
@UpdatedByETIUserID int,
@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: [upsertPerDiem]
**		Desc: Insert/Update data into [upsertPerDiem]
**			
**				NOTES: 
**				
**				For Per Diem, question is what happens when you change a rate
**				I assume there is an update to the current rates.
**		
**
**		Auth: Don Canuso
**		Date: 7/20/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/24/11	dcanuso				PerDiem.PerDiemLastUpdateDT changed from
**										datetime2 to date
**		11/28/11	dcanuso				Removing RentalCarRate decimal (6,2) from PerDiem
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@Inserted AS Table (ID int)
/*
Stored procedure uses GetDate and update date serveral times
and will overwrite the @UpdateDT so 
using a new one
*/
DECLARE @UpdateDate datetime2 = GetDate() 

IF @PerDiemID < 0  /*Insert Record*/
	BEGIN
	
		INSERT INTO [dbo].[PerDiem]
           ([UpdateDT]
           ,[PerDiemDestination]
           ,[Qualification]
           ,[HotelRate]
           ,[MIERate]
           ,[PerDiemNotes]
           ,[PerDiemLastUpdateETIUserID]
           ,[PerDiemLastUpdateDT]
			)
        OUTPUT inserted.PerDiemID INTO @Inserted
     VALUES
           (@UpdateDate--GETDATE()--@UpdateDT
           ,@PerDiemDestination
           ,@Qualification
           ,@HotelRate
           ,@MIERate
           ,@PerDiemNotes
           ,@UpdatedByETIUserID
           ,CAST (@UpdateDate AS DATE) /*On an insert the Last Update Date is the current date*/)

		SELECT 	@PerDiemID = [ID] FROM @Inserted 
	        
	END        
ELSE 
BEGIN
	IF (SELECT UpdateDT FROM [dbo].[PerDiem] WHERE PerDiemID = @PerDiemID) = @UpdateDT	
	BEGIN
	
		/*
		Setting Per Diem Last Update By and Dates
		
		Per Diem Last Updated By
		This field is output only and shows who made the last change, including when it was created, to any of the following data:
		Hotel
        MIE Rate
        Per diem notes
        Rental Car
        
  
		Per Diem Last Updated Date
		This field is output only and shows the date of the last change, including when it was created, made to any of the following data:
        Hotel
        MIE Rate
        Per diem notes
        Rental Car
        */
 
		UPDATE [dbo].[PerDiem]
		   SET [UpdateDT] = @UpdateDate--GETDATE()--@UpdateDT
			  ,[PerDiemDestination] = @PerDiemDestination
			  ,[Qualification] = @Qualification
			  ,[HotelRate] = @HotelRate
			  ,[MIERate] = @MIERate
			  ,[PerDiemNotes] = @PerDiemNotes
			  ,[PerDiemLastUpdateDT] = 
					CASE WHEN	(
					/*
					PerDiem Wireframes changed to:
					any of the following data:
					 Hotel
					 MIE Rate
					 Per diem notes
					*/					
								[HotelRate] <> @HotelRate OR 
								[MIERate] <> @MIERate OR 
								IsNull([PerDiemNotes],'') <> IsNull(@PerDiemNotes,'') 
								) THEN CAST (@UpdateDate AS DATE)
					ELSE
						[PerDiemLastUpdateDT]
					END
			  ,[PerDiemLastUpdateETIUserID] = 
					CASE WHEN	(
					/*
					PerDiem Wireframes changed to:
					any of the following data:
					 Hotel
					 MIE Rate
					 Per diem notes
					*/					
								[HotelRate] <> @HotelRate OR 
								[MIERate] <> @MIERate OR 
								IsNull([PerDiemNotes],'') <> IsNull(@PerDiemNotes,'') 
								) THEN @UpdatedByETIUserID
					ELSE
						[PerDiemLastUpdateETIUserID]
					END
		 WHERE 
			PerDiemID = @PerDiemID
	END	
	ELSE
			BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Per Diem with ID ' + CAST(@PerDiemID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
			END
END

IF @@ERROR = 0
	SELECT @PerDiemID AS PerDiemID
GO