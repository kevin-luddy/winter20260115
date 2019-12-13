IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSystemSetting]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSystemSetting];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertSystemSetting]
(
	@Key		VARCHAR(255),
	@Value		VARCHAR(4000)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertSystemSetting]
	**		Desc:	Insert/Update System Setting values 
	**			
	**		
	**
	**		Auth: Greg Brunworth
	**		Date: 6/28/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 
 
    MERGE [dbo].SystemSetting AS myTarget
    USING (SELECT @Key [Key], @Value [Value]) AS mySource
        ON mySource.[Key] = myTarget.[Key]
    WHEN MATCHED THEN UPDATE
        SET [Value] = mySource.[Value]
    WHEN NOT MATCHED THEN 
        INSERT ([Key], [Value]) 
        VALUES (@Key, @Value);

	IF @@ERROR = 0
		SELECT @Key

GO