IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getLaborType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getLaborType];

GO

-- PROPH-1676 - e302876 - removed getLaborType since it is never used