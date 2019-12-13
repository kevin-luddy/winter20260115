/*
	## START ##

	8/25/2016 [RJ] -- Scripts for Zone Travel
*/


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTZoneTravelDestination]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTZoneTravelDestination](
		[DestinationID] [int] IDENTITY(1,1) NOT NULL,
		[Destination] [varchar](100) NOT NULL,
		[Abbreviation] [char](2) NOT NULL,
		[Zone] [int] NOT NULL CHECK  (([Zone]>=(1) AND [Zone]<=(6))),
		PRIMARY KEY CLUSTERED 
		(
			[DestinationID] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTZoneTravelOrigin]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTZoneTravelOrigin](
		[OriginID] [int] IDENTITY(1,1) NOT NULL,
		[Origin] [varchar](100) NOT NULL,
		[Site] [varchar](10) NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[OriginID] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTZoneTravelResource]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTZoneTravelResource](
		[ResourceID] [int] IDENTITY(1,1) NOT NULL,
		[Resource] [varchar](20) NOT NULL,
		[Zone] [int] NOT NULL CHECK  (([ZONE]>=(1) AND [ZONE]<=(6))),
		[isAirfare] [bit] NOT NULL,
		[OriginID] [int] NOT NULL REFERENCES [dbo].[MSTZoneTravelOrigin] ([OriginID]),
		[LookupValue] [varchar](100) NOT NULL,
		[Description] [varchar](100) NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[ResourceID] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
END
GO


-- These changes are to be executed in MST only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. MST is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 2000 AND LineOfBusinessId < 2999)
BEGIN
	-- ONLY FOR MST
	IF NOT EXISTS (SELECT * FROM [dbo].[MSTZoneTravelDestination])
	BEGIN
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Alabama','AL',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Arkansas','AR',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Arizona ','AZ',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('California ','CA',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Colorado ','CO',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Connecticut','CT',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('District Of Columbia','DC',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Delaware','DE',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Florida','FL',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Georgia','GA',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Hawaii','HI',4);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Iowa','IA',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Idaho','ID',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Illinois','IL',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Indiana','IN',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Kansas','KS',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Kentucky','KY',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Louisiana','LA',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Massachusetts','MA',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Maryland','MD',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Maine','ME',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Michigan','MI',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Minnesota','MN',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Missouri','MO',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Mississippi','MS',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Montana','MT',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('North Carolina','NC',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('North Dakota','ND',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Nebraska','NE',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('New Hampshire','NH',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('New Jersey','NJ',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('New Mexico','NM',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Nevada','NV',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('New York','NY',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Ohio','OH',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Oklahoma','OK',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Oregon','OR',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Pennsylvania','PA',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Rhode Island','RI',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('South Carolina','SC',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('South Dakota','SD',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Tennessee','TN',1);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Texas','TX',2);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Utah','UT',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Virginia ','VA',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Vermont','VT',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Washington','WA',3);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Wisconsin','WI',5);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('West Virginia','WV',6);
		INSERT INTO [dbo].[MSTZoneTravelDestination] VALUES ('Wyoming','WY',3);
	END

	IF NOT EXISTS (SELECT * FROM [dbo].[MSTZoneTravelOrigin])
	BEGIN
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Akron', 'A');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Baltimore', 'V');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Orlando', 'W');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Clearwater', 'G');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Dahlgren', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Newport', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Pax River', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services San Diego', 'SD');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Virginia Beach', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Albuquerque', 'E');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Ft Walton Beach', 'QW');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('MST Services Little Rock', 'L');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Manassas', 'M');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Marion', 'J');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Mitchel Field', 'B');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Moorestown', 'T');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Owego', 'Y');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Palm Beach', 'Q');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('San Diego-MST', 'ED');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Syracuse', 'F');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Albuquerque', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Atlanta', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Baltimore', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Dallas-Ft Worth', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Ft Walton Beach', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('TLS Little Rock', 'S');
		INSERT INTO [dbo].[MSTZoneTravelOrigin] VALUES('Washington DC', 'D');
	END

	IF NOT EXISTS (SELECT * FROM [dbo].[MSTZoneTravelResource])
	BEGIN
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ1',1,0,1,'AkronPR1','Akron Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ2',2,0,1,'AkronPR2','Akron Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ3',3,0,1,'AkronPR3','Akron Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ4',4,0,1,'AkronPR4','Akron Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ5',5,0,1,'AkronPR5','Akron Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('APRZ6',6,0,1,'AkronPR6','Akron Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('ATRZ1',1,1,1,'AkronTR1','Akron Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('ATRZ2',2,1,1,'AkronTR2','Akron Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('ATRZ3',3,1,1,'AkronTR3','Akron Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,1,'AkronTR4','Akron Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('ATRZ5',5,1,1,'AkronTR5','Akron Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('ATRZ6',6,1,1,'AkronTR6','Akron Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ1',1,0,2,'BaltimorePR1','Baltimore Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ2',2,0,2,'BaltimorePR2','Baltimore Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ3',3,0,2,'BaltimorePR3','Baltimore Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ4',4,0,2,'BaltimorePR4','Baltimore Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ5',5,0,2,'BaltimorePR5','Baltimore Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VPRZ6',6,0,2,'BaltimorePR6','Baltimore Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ1',1,1,2,'BaltimoreTR1','Baltimore Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ2',2,1,2,'BaltimoreTR2','Baltimore Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ3',3,1,2,'BaltimoreTR3','Baltimore Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ4',4,1,2,'BaltimoreTR4','Baltimore Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ5',5,1,2,'BaltimoreTR5','Baltimore Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('VTRZ6',6,1,2,'BaltimoreTR6','Baltimore Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,3,'TLS OrlandoPR1','TLS Orlando Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,3,'TLS OrlandoPR2','TLS Orlando Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,3,'TLS OrlandoPR3','TLS Orlando Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ4',4,0,3,'TLS OrlandoPR4','TLS Orlando Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,3,'TLS OrlandoPR5','TLS Orlando Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,3,'TLS OrlandoPR6','TLS Orlando Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ1',1,1,3,'TLS OrlandoTR1','TLS Orlando Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ2',2,1,3,'TLS OrlandoTR2','TLS Orlando Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ3',3,1,3,'TLS OrlandoTR3','TLS Orlando Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ4',4,1,3,'TLS OrlandoTR4','TLS Orlando Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ5',5,1,3,'TLS OrlandoTR5','TLS Orlando Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SORLTRZ6',6,1,3,'TLS OrlandoTR6','TLS Orlando Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ1',1,0,4,'ClearwaterPR1','Clearwater Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ2',2,0,4,'ClearwaterPR2','Clearwater Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ3',3,0,4,'ClearwaterPR3','Clearwater Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ4',4,0,4,'ClearwaterPR4','Clearwater Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ5',5,0,4,'ClearwaterPR5','Clearwater Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GPRZ6',6,0,4,'ClearwaterPR6','Clearwater Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GTRZ1',1,1,4,'ClearwaterTR1','Clearwater Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GTRZ2',2,1,4,'ClearwaterTR2','Clearwater Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GTRZ3',3,1,4,'ClearwaterTR3','Clearwater Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,4,'ClearwaterTR4','Clearwater Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GTRZ5',5,1,4,'ClearwaterTR5','Clearwater Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('GTRZ6',6,1,4,'ClearwaterTR6','Clearwater Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,5,'MST Services DahlgrenPR1','Dahlgren Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,0,5,'MST Services DahlgrenPR2','Dahlgren Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,0,5,'MST Services DahlgrenPR3','Dahlgren Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,5,'MST Services DahlgrenPR4','Dahlgren Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,5,'MST Services DahlgrenPR5','Dahlgren Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,5,'MST Services DahlgrenPR6','Dahlgren Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LGTRZ1',1,1,5,'MST Services DahlgrenTR1','Dahlgren Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,1,5,'MST Services DahlgrenTR2','Dahlgren Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,1,5,'MST Services DahlgrenTR3','Dahlgren Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,5,'MST Services DahlgrenTR4','Dahlgren Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LGTRZ5',5,1,5,'MST Services DahlgrenTR5','Dahlgren Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LGTRZ6',6,1,5,'MST Services DahlgrenTR6','Dahlgren Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,10,'MST Services AlbuquerquePR1','Albuquerque Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,0,10,'MST Services AlbuquerquePR2','Albuquerque Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,0,10,'MST Services AlbuquerquePR3','Albuquerque Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,10,'MST Services AlbuquerquePR4','Albuquerque Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,10,'MST Services AlbuquerquePR5','Albuquerque Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,10,'MST Services AlbuquerquePR6','Albuquerque Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LATRZ1',1,1,10,'MST Services AlbuquerqueTR1','Albuquerque Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,1,10,'MST Services AlbuquerqueTR2','Albuquerque Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,1,10,'MST Services AlbuquerqueTR3','Albuquerque Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,10,'MST Services AlbuquerqueTR4','Albuquerque Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LATRZ5',5,1,10,'MST Services AlbuquerqueTR5','Albuquerque Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LATRZ6',6,1,10,'MST Services AlbuquerqueTR6','Albuquerque Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,11,'MST Services Ft Walton BeachPR1','Ft Walton Beach Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ2',2,0,11,'MST Services Ft Walton BeachPR2','Ft Walton Beach Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ3',3,0,11,'MST Services Ft Walton BeachPR3','Ft Walton Beach Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,11,'MST Services Ft Walton BeachPR4','Ft Walton Beach Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,11,'MST Services Ft Walton BeachPR5','Ft Walton Beach Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,11,'MST Services Ft Walton BeachPR6','Ft Walton Beach Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LFWBTRZ1',1,1,11,'MST Services Ft Walton BeachTR1','Ft Walton Beach Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LFWBTRZ2',2,1,11,'MST Services Ft Walton BeachTR2','Ft Walton Beach Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LFWBTRZ3',3,1,11,'MST Services Ft Walton BeachTR3','Ft Walton Beach Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,11,'MST Services Ft Walton BeachTR4','Ft Walton Beach Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LFWBTRZ5',5,1,11,'MST Services Ft Walton BeachTR5','Ft Walton Beach Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LFWBTRZ6',6,1,11,'MST Services Ft Walton BeachTR6','Ft Walton Beach Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,12,'MST Services Little RockPR1','Little Rock Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ2',2,0,12,'MST Services Little RockPR2','Little Rock Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ3',3,0,12,'MST Services Little RockPR3','Little Rock Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,12,'MST Services Little RockPR4','Little Rock Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,12,'MST Services Little RockPR5','Little Rock Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,12,'MST Services Little RockPR6','Little Rock Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LLRTRZ1',1,1,12,'MST Services Little RockTR1','Little Rock Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LLRTRZ2',2,1,12,'MST Services Little RockTR2','Little Rock Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LLRTRZ3',3,1,12,'MST Services Little RockTR3','Little Rock Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,12,'MST Services Little RockTR4','Little Rock Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LLRTRZ5',5,1,12,'MST Services Little RockTR5','Little Rock Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LLRTRZ6',6,1,12,'MST Services Little RockTR6','Little Rock Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ1',1,0,13,'ManassasPR1','Manassas Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ2',2,0,13,'ManassasPR2','Manassas Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ3',3,0,13,'ManassasPR3','Manassas Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ4',4,0,13,'ManassasPR4','Manassas Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ5',5,0,13,'ManassasPR5','Manassas Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MPRZ6',6,0,13,'ManassasPR6','Manassas Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ1',1,1,13,'ManassasTR1','Manassas Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ2',2,1,13,'ManassasTR2','Manassas Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ3',3,1,13,'ManassasTR3','Manassas Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ4',4,1,13,'ManassasTR4','Manassas Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ5',5,1,13,'ManassasTR5','Manassas Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('MTRZ6',6,1,13,'ManassasTR6','Manassas Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JPRZ1',1,0,14,'MarionPR1','Marion Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JPRZ2',2,0,14,'MarionPR2','Marion Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JPRZ3',3,0,14,'MarionPR3','Marion Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,14, 'MarionPR4', 'Marion Per Diem/Misc Zone4')
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JPRZ5',5,0,14,'MarionPR5','Marion Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JPRZ6',6,0,14,'MarionPR6','Marion Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JTRZ1',1,1,14,'MarionTR1','Marion Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JTRZ2',2,1,14,'MarionTR2','Marion Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JTRZ3',3,1,14,'MarionTR3','Marion Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,14,'MarionTR4','Marion Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JTRZ5',5,1,14,'MarionTR5','Marion Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('JTRZ6',6,1,14,'MarionTR6','Marion Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ1',1,0,15,'Mitchel FieldPR1','Mitchel Field Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ2',2,0,15,'Mitchel FieldPR2','Mitchel Field Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ3',3,0,15,'Mitchel FieldPR3','Mitchel Field Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ4',4,0,15,'Mitchel FieldPR4','Mitchel Field Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ5',5,0,15,'Mitchel FieldPR5','Mitchel Field Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BPRZ6',6,0,15,'Mitchel FieldPR6','Mitchel Field Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BTRZ1',1,1,15,'Mitchel FieldTR1','Mitchel Field Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BTRZ2',2,1,15,'Mitchel FieldTR2','Mitchel Field Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BTRZ3',3,1,15,'Mitchel FieldTR3','Mitchel Field Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BTRZ4',4,1,15,'Mitchel FieldTR4','Mitchel Field Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',5,1,15,'Mitchel FieldTR5','Mitchel Field Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('BTRZ6',6,1,15,'Mitchel FieldTR6','Mitchel Field Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ1',1,0,16,'MoorestownPR1','Moorestown Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ2',2,0,16,'MoorestownPR2','Moorestown Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ3',3,0,16,'MoorestownPR3','Moorestown Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ4',4,0,16,'MoorestownPR4','Moorestown Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ5',5,0,16,'MoorestownPR5','Moorestown Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TPRZ6',6,0,16,'MoorestownPR6','Moorestown Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ1',1,1,16,'MoorestownTR1','Moorestown Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ2',2,1,16,'MoorestownTR2','Moorestown Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ3',3,1,16,'MoorestownTR3','Moorestown Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ4',4,1,16,'MoorestownTR4','Moorestown Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ5',5,1,16,'MoorestownTR5','Moorestown Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('TTRZ6',6,1,16,'MoorestownTR6','Moorestown Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,6,'MST Services NewportPR1','Newport Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,0,6,'MST Services NewportPR2','Newport Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,0,6,'MST Services NewportPR3','Newport Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,6,'MST Services NewportPR4','Newport Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,6,'MST Services NewportPR5','Newport Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,6,'MST Services NewportPR6','Newport Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LNTRZ1',1,1,6,'MST Services NewportTR1','Newport Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,1,6,'MST Services NewportTR2','Newport Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,1,6,'MST Services NewportTR3','Newport Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,6,'MST Services NewportTR4','Newport Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LNTRZ5',5,1,6,'MST Services NewportTR5','Newport Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LNTRZ6',6,1,6,'MST Services NewportTR6','Newport Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YPRZ1',1,0,17,'OwegoPR1','Owego Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YPRZ2',2,0,17,'OwegoPR2','Owego Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YPRZ3',3,0,17,'OwegoPR3','Owego Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,17,'OwegoPR4','Owego Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YPRZ5',5,0,17,'OwegoPR5','Owego Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YPRZ6',6,0,17,'OwegoPR6','Owego Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YTRZ1',1,1,17,'OwegoTR1','Owego Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YTRZ2',2,1,17,'OwegoTR2','Owego Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YTRZ3',3,1,17,'OwegoTR3','Owego Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,17,'OwegoTR4','Owego Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YTRZ5',5,1,17,'OwegoTR5','Owego Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('YTRZ6',6,1,17,'OwegoTR6','Owego Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,7,'MST Services Pax RiverPR1','Pax River Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,0,7,'MST Services Pax RiverPR2','Pax River Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ3',3,0,7,'MST Services Pax RiverPR3','Pax River Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,7,'MST Services Pax RiverPR4','Pax River Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',5,0,7,'MST Services Pax RiverPR5','Pax River Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',6,0,7,'MST Services Pax RiverPR6','Pax River Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NPTRZ1',1,1,7,'MST Services Pax RiverTR1','Pax River Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,1,7,'MST Services Pax RiverTR2','Pax River Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NPTRZ3',3,1,7,'MST Services Pax RiverTR3','Pax River Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,7,'MST Services Pax RiverTR4','Pax River Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',5,1,7,'MST Services Pax RiverTR5','Pax River Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',6,1,7,'MST Services Pax RiverTR6','Pax River Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QPRZ1',1,0,18,'Palm BeachPR1','Palm Beach Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QPRZ2',2,0,18,'Palm BeachPR2','Palm Beach Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QPRZ3',3,0,18,'Palm BeachPR3','Palm Beach Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,18,'Palm BeachPR4','Palm Beach Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QPRZ5',5,0,18,'Palm BeachPR5','Palm Beach Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QPRZ6',6,0,18,'Palm BeachPR6','Palm Beach Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QTRZ1',1,1,18,'Palm BeachTR1','Palm Beach Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QTRZ2',2,1,18,'Palm BeachTR2','Palm Beach Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QTRZ3',3,1,18,'Palm BeachTR3','Palm Beach Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,18,'Palm BeachTR4','Palm Beach Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QTRZ5',5,1,18,'Palm BeachTR5','Palm Beach Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('QTRZ6',6,1,18,'Palm BeachTR6','Palm Beach Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,8,'MST Services San DiegoPR1','CSS San Diego Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ2',2,0,8,'MST Services San DiegoPR2','CSS San Diego Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ3',3,0,8,'MST Services San DiegoPR3','CSS San Diego Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ4',4,0,8,'MST Services San DiegoPR4','CSS San Diego Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,8,'MST Services San DiegoPR5','CSS San Diego Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,8,'MST Services San DiegoPR6','CSS San Diego Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ1',1,1,8,'MST Services San DiegoTR1','CSS San Diego Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ2',2,1,8,'MST Services San DiegoTR2','CSS San Diego Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ3',3,1,8,'MST Services San DiegoTR3','CSS San Diego Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ4',4,1,8,'MST Services San DiegoTR4','CSS San Diego Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ5',5,1,8,'MST Services San DiegoTR5','CSS San Diego Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LDTRZ6',6,1,8,'MST Services San DiegoTR6','CSS San Diego Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ1',1,0,19,'San Diego-MSTPR1','San Diego-MST Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ2',2,0,19,'San Diego-MSTPR2','San Diego-MST Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ3',3,0,19,'San Diego-MSTPR3','San Diego-MST Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ4',4,0,19,'San Diego-MSTPR4','San Diego-MST Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ5',5,0,19,'San Diego-MSTPR5','San Diego-MST Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDPRZ6',6,0,19,'San Diego-MSTPR6','San Diego-MST Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ1',1,1,19,'San Diego-MSTTR1','San Diego-MST Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ2',2,1,19,'San Diego-MSTTR2','San Diego-MST Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ3',3,1,19,'San Diego-MSTTR3','San Diego-MST Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ4',4,1,19,'San Diego-MSTTR4','San Diego-MST Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ5',5,1,19,'San Diego-MSTTR5','San Diego-MST Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('EDTRZ6',6,1,19,'San Diego-MSTTR6','San Diego-MST Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ1',1,0,20,'SyracusePR1','Syracuse Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ2',2,0,20,'SyracusePR2','Syracuse Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ3',3,0,20,'SyracusePR3','Syracuse Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ4',4,0,20,'SyracusePR4','Syracuse Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ5',5,0,20,'SyracusePR5','Syracuse Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FPRZ6',6,0,20,'SyracusePR6','Syracuse Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ1',1,1,20,'SyracuseTR1','Syracuse Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ2',2,1,20,'SyracuseTR2','Syracuse Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ3',3,1,20,'SyracuseTR3','Syracuse Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ4',4,1,20,'SyracuseTR4','Syracuse Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ5',5,1,20,'SyracuseTR5','Syracuse Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('FTRZ6',6,1,20,'SyracuseTR6','Syracuse Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,21,'TLS AlbuquerquePR1','TLS Albuquerque Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,21,'TLS AlbuquerquePR2','TLS Albuquerque Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,0,21,'TLS AlbuquerquePR3','TLS Albuquerque Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,21,'TLS AlbuquerquePR4','TLS Albuquerque Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',5,0,21,'TLS AlbuquerquePR5','TLS Albuquerque Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,21,'TLS AlbuquerquePR6','TLS Albuquerque Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SALBQTRZ1',1,1,21,'TLS AlbuquerqueTR1','TLS Albuquerque Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SALBQTRZ2',2,1,21,'TLS AlbuquerqueTR2','TLS Albuquerque Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',3,1,21,'TLS AlbuquerqueTR3','TLS Albuquerque Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,21,'TLS AlbuquerqueTR4','TLS Albuquerque Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',5,1,21,'TLS AlbuquerqueTR5','TLS Albuquerque Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SALBQTRZ6',6,1,21,'TLS AlbuquerqueTR6','TLS Albuquerque Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,22,'TLS AtlantaPR1','TLS Atlanta Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,22,'TLS AtlantaPR2','TLS Atlanta Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,22,'TLS AtlantaPR3','TLS Atlanta Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,22,'TLS AtlantaPR4','TLS Atlanta Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,22,'TLS AtlantaPR5','TLS Atlanta Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,22,'TLS AtlantaPR6','TLS Atlanta Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SATLTRZ1',1,1,22,'TLS AtlantaTR1','TLS Atlanta Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SATLTRZ2',2,1,22,'TLS AtlantaTR2','TLS Atlanta Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SATLTRZ3',3,1,22,'TLS AtlantaTR3','TLS Atlanta Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,22,'TLS AtlantaTR4','TLS Atlanta Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SATLTRZ5',5,1,22,'TLS AtlantaTR5','TLS Atlanta Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SATLTRZ6',6,1,22,'TLS AtlantaTR6','TLS Atlanta Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,23,'TLS BaltimorePR1','TLS Baltimore Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,23,'TLS BaltimorePR2','TLS Baltimore Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,23,'TLS BaltimorePR3','TLS Baltimore Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ4',4,0,23,'TLS BaltimorePR4','TLS Baltimore Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,23,'TLS BaltimorePR5','TLS Baltimore Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,23,'TLS BaltimorePR6','TLS Baltimore Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ1',1,1,23,'TLS BaltimoreTR1','TLS Baltimore Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ2',2,1,23,'TLS BaltimoreTR2','TLS Baltimore Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ3',3,1,23,'TLS BaltimoreTR3','TLS Baltimore Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ4',4,1,23,'TLS BaltimoreTR4','TLS Baltimore Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ5',5,1,23,'TLS BaltimoreTR5','TLS Baltimore Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SBALTTRZ6',6,1,23,'TLS BaltimoreTR6','TLS Baltimore Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,24,'TLS Dallas-Ft WorthPR1','TLS Dallas-Ft Worth Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,24,'TLS Dallas-Ft WorthPR2','TLS Dallas-Ft Worth Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,24,'TLS Dallas-Ft WorthPR3','TLS Dallas-Ft Worth Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,24,'TLS Dallas-Ft WorthPR4','TLS Dallas-Ft Worth Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,24,'TLS Dallas-Ft WorthPR5','TLS Dallas-Ft Worth Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,24,'TLS Dallas-Ft WorthPR6','TLS Dallas-Ft Worth Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SDFWTRZ1',1,1,24,'TLS Dallas-Ft WorthTR1','TLS Dallas-Ft Worth Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SDFWTRZ2',2,1,24,'TLS Dallas-Ft WorthTR2','TLS Dallas-Ft Worth Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SDFWTRZ3',3,1,24,'TLS Dallas-Ft WorthTR3','TLS Dallas-Ft Worth Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,24,'TLS Dallas-Ft WorthTR4','TLS Dallas-Ft Worth Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SDFWTRZ5',5,1,24,'TLS Dallas-Ft WorthTR5','TLS Dallas-Ft Worth Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SDFWTRZ6',6,1,24,'TLS Dallas-Ft WorthTR6','TLS Dallas-Ft Worth Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,25,'TLS Ft Walton BeachPR1','TLS Ft Walton Beach Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,25,'TLS Ft Walton BeachPR2','TLS Ft Walton Beach Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,25,'TLS Ft Walton BeachPR3','TLS Ft Walton Beach Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,25,'TLS Ft Walton BeachPR4','TLS Ft Walton Beach Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,25,'TLS Ft Walton BeachPR5','TLS Ft Walton Beach Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,25,'TLS Ft Walton BeachPR6','TLS Ft Walton Beach Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SFWBTRZ1',1,1,25,'TLS Ft Walton BeachTR1','TLS Ft Walton Beach Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SFWBTRZ2',2,1,25,'TLS Ft Walton BeachTR2','TLS Ft Walton Beach Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SFWBTRZ3',3,1,25,'TLS Ft Walton BeachTR3','TLS Ft Walton Beach Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,25,'TLS Ft Walton BeachTR4','TLS Ft Walton Beach Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SFWBTRZ5',5,1,25,'TLS Ft Walton BeachTR5','TLS Ft Walton Beach Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SFWBTRZ6',6,1,25,'TLS Ft Walton BeachTR6','TLS Ft Walton Beach Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ1',1,0,26,'TLS Little RockPR1','TLS Little Rock Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ2',2,0,26,'TLS Little RockPR2','TLS Little Rock Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ3',3,0,26,'TLS Little RockPR3','TLS Little Rock Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,26,'TLS Little RockPR4','TLS Little Rock Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ5',5,0,26,'TLS Little RockPR5','TLS Little Rock Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SPRZ6',6,0,26,'TLS Little RockPR6','TLS Little Rock Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SLRTRZ1',1,1,26,'TLS Little RockTR1','TLS Little Rock Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SLRTRZ2',2,1,26,'TLS Little RockTR2','TLS Little Rock Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SLRTRZ3',3,1,26,'TLS Little RockTR3','TLS Little Rock Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,26,'TLS Little RockTR4','TLS Little Rock Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SLRTRZ5',5,1,26,'TLS Little RockTR5','TLS Little Rock Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('SLRTRZ6',6,1,26,'TLS Little RockTR6','TLS Little Rock Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ1',1,0,9,'MST Services Virginia BeachPR1','Virginia Beach Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,0,9,'MST Services Virginia BeachPR2','Virginia Beach Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ3',3,0,9,'MST Services Virginia BeachPR3','Virginia Beach Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,0,9,'MST Services Virginia BeachPR4','Virginia Beach Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ5',5,0,9,'MST Services Virginia BeachPR5','Virginia Beach Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LSPRZ6',6,0,9,'MST Services Virginia BeachPR6','Virginia Beach Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LVTRZ1',1,1,9,'MST Services Virginia BeachTR1','Virginia Beach Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',2,1,9,'MST Services Virginia BeachTR2','Virginia Beach Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LVTRZ3',3,1,9,'MST Services Virginia BeachTR3','Virginia Beach Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('NO-RATE',4,1,9,'MST Services Virginia BeachTR4','Virginia Beach Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LVTRZ5',5,1,9,'MST Services Virginia BeachTR5','Virginia Beach Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('LVTRZ6',6,1,9,'MST Services Virginia BeachTR6','Virginia Beach Airfare to Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ1',1,0,27,'Washington DCPR1','Washington DC Per Diem/Misc Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ2',2,0,27,'Washington DCPR2','Washington DC Per Diem/Misc Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ3',3,0,27,'Washington DCPR3','Washington DC Per Diem/Misc Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ4',4,0,27,'Washington DCPR4','Washington DC Per Diem/Misc Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ5',5,0,27,'Washington DCPR5','Washington DC Per Diem/Misc Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DPRZ6',6,0,27,'Washington DCPR6','Washington DC Per Diem/Misc Zone6');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ1',1,1,27,'Washington DCTR1','Washington DC Airfare to Zone1');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ2',2,1,27,'Washington DCTR2','Washington DC Airfare to Zone2');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ3',3,1,27,'Washington DCTR3','Washington DC Airfare to Zone3');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ4',4,1,27,'Washington DCTR4','Washington DC Airfare to Zone4');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ5',5,1,27,'Washington DCTR5','Washington DC Airfare to Zone5');
		INSERT INTO [dbo].[MSTZoneTravelResource] VALUES ('DTRZ6',6,1,27,'Washington DCTR6','Washington DC Airfare to Zone6');
	END
END

GO

/*
	8/25/2016 [RJ] -- Scripts for Zone Travel

	## END ##
*/

/*
	## START ##

	8/1/2016 BOEJ-1244 [Tim Wilson] -- EP vs hours change
*/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsUsingEquivalentPerson' AND Object_ID = Object_ID('[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace]
		ADD IsUsingEquivalentPerson BIT NOT NULL
		CONSTRAINT DF_Workspace_IsUsingEP DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsUsingEquivalentPerson' AND Object_ID = Object_ID('[version].[Workspace]'))
BEGIN
	ALTER TABLE [version].[Workspace]
		ADD IsUsingEquivalentPerson BIT NOT NULL
		CONSTRAINT DF_Workspace_Version_IsUsingEP DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM [dbo].[ProPricerFieldLU] WHERE ProPricerField = 'EP Or BLANK')
BEGIN
	DECLARE @MaxNum int = (select max(ProPricerFieldID) + 1 from [dbo].[ProPricerFieldLU])
	INSERT INTO [dbo].[ProPricerFieldLU](ProPricerFieldID, ProPricerField, ProPricerTypeID) VALUES (@MaxNum, 'EP Or BLANK', 2)
	Declare @defaultPricerId integer = (SELECT [ProPricerExportID] FROM [dbo].[ProPricerExport] where propricerscopeid = 2 and propricerexportname = 'ProPricer Export Starting Setup')
	Update ProPricerFieldXREF Set ProPricerFieldID = @MaxNum where propricerexportID = @defaultPricerId AND ProPricerTypeID = 2 AND ListOrder = 7
END
GO
/*
	8/1/2016 BOEJ-1244 [Tim Wilson] -- EP (Equivalent Person) vs hours change

	## END ##
*/
/*
	8/31/2016 [Eron] -- Adding the ability to save INL form definitions

	## START ##
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomForm]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CustomForm](
		[CustomFormId]			[int]	IDENTITY(1,1)	NOT NULL,
		[CustomFormName]		[varchar](200)			NOT NULL,
		[CustomFormDesignText]	[varchar](max)			NOT NULL,
		[UpdateDT]				[datetime2](7)			NOT NULL,
		[Active]				[bit]					NOT NULL		DEFAULT (1),
		CONSTRAINT [PK_CustomForm] PRIMARY KEY CLUSTERED  ([CustomFormId] ASC)
			WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

/*
	## END ##

	8/31/2016 [Eron] -- Adding the ability to save INL form definitions
*/
/*
	8/31/2016 [Dusan] -- Adding the ability to save INL form inputs

	## START ##
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFormInputs]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CustomFormInputs]
	(
		FormId		INT				NOT NULL		REFERENCES CustomForm(CustomFormId),
		BoeId		INT				NOT NULL		REFERENCES Boe(BoeId),
		FieldId		INT				NOT NULL,
		Value		VARCHAR(MAX)	NOT NULL,
		ElementType	INT				NOT NULL,
		PRIMARY KEY (FormId, BoeId, FieldId) -- the combination of the 3 has to be unique
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CustomFormInputs]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[CustomFormInputs]
	(
		FormId		INT				NOT NULL,
		BoeId		INT				NOT NULL,
		FieldId		INT				NOT NULL,
		Value		VARCHAR(MAX)	NOT NULL,
		ElementType	INT				NOT NULL,
		VersionId	INT				NOT NULL
	);
END
GO

/*
	## END ##

	8/31/2016 [Dusan] -- Adding the ability to save INL form inputs
*/

/*
	9/7/2016 [twilson3] -- Adding the ability to save INL form selections by BOE

	## START ##
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CustomFormSelectionsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[CustomFormSelectionsXREF]
	(
		FormId		INT				NOT NULL,
		BoeId		INT				NOT NULL,
		VersionId	INT				NOT NULL
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFormSelectionsXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CustomFormSelectionsXREF]
	(
		FormId		INT				NOT NULL		REFERENCES CustomForm(CustomFormId),
		BoeId		INT				NOT NULL		REFERENCES Boe(BoeId)
	);
END
GO
/*
	## END ##
	
	8/31/2016 [twilson3] -- Adding the ability to save INL form selections by BOE
*/

/*
	9/14/2016 [Tom & RJ] -- Additional work for MST Zone Travel

	## START ##	
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTTravelModeLU]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTTravelModeLU](
		[MSTTravelModeID] [int] NOT NULL,
		[MSTTravelMode] [varchar](50) NOT NULL,
		CONSTRAINT [PK_MSTZoneTravelModeLU] PRIMARY KEY CLUSTERED 
		(
			[MSTTravelModeID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTTravelTrip]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTTravelTrip](
		[MSTTravelTripID] [int] IDENTITY(1,1) NOT NULL,
		[ModeID] [int] NOT NULL,
		[IDN] [varchar](10) NULL,
		[TravelTripTaskElementID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[GroupID] [int] NULL,
		[SegmentID] [int] NOT NULL,
		[Purpose] [varchar](35) NULL,
		[PerformingOrganizationID] [int] NOT NULL,
		[TripDate] [date] NOT NULL,
		[EstimateDate] [date] NULL,
		[NumPeople] [int] NOT NULL,
		[NumDays] [int] NOT NULL,
		[ZoneOriginID] [int] NULL,
		[ZoneDestCity] [varchar](35) NULL,
		[ZoneDestState] [varchar](2) NULL,
		[ZoneDestinationID] [int] NULL,
		[ZonePerDiemResourceID] [int] NULL,
		[ZoneAirfareResourceID] [int] NULL,
		[NonZoneFrom] [varchar](150) NULL,
		[NonZoneTo] [varchar](150) NULL,
		[NonZoneTravelAgencyFee] [money] NULL,
		[NonZoneAirFareEstimate] [money] NULL,
		[NonZonePerDiemDaily] [money] NULL,
		[NonZoneCarRentalTrans] [money] NULL,
		[NonZoneNumCars] [int] NULL,
		[NonZoneMisc] [money] NULL,
		CONSTRAINT [PK_dbo.MSTTravelTrip] PRIMARY KEY CLUSTERED 
		(
			[MSTTravelTripID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTTravelModeLU] FOREIGN KEY([ModeID])
		REFERENCES [dbo].[MSTTravelModeLU] ([MSTTravelModeID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTTravelModeLU]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTTravelTrip1] FOREIGN KEY([MSTTravelTripID])
		REFERENCES [dbo].[MSTTravelTrip] ([MSTTravelTripID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTTravelTrip1]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelDestination] FOREIGN KEY([ZoneDestinationID])
		REFERENCES [dbo].[MSTZoneTravelDestination] ([DestinationID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelDestination]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelOrigin] FOREIGN KEY([ZoneOriginID])
		REFERENCES [dbo].[MSTZoneTravelOrigin] ([OriginID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelOrigin]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource] FOREIGN KEY([ZoneAirfareResourceID])
		REFERENCES [dbo].[MSTZoneTravelResource] ([ResourceID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH CHECK ADD  CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource1] FOREIGN KEY([ZonePerDiemResourceID])
		REFERENCES [dbo].[MSTZoneTravelResource] ([ResourceID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_MSTZoneTravelResource1]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_PerformingOrganization] FOREIGN KEY([PerformingOrganizationID])
		REFERENCES [dbo].[PerformingOrganization] ([PerformingOrganizationID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_PerformingOrganization]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_SegmentLU] FOREIGN KEY([SegmentID])
		REFERENCES [dbo].[SegmentLU] ([SegmentID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_SegmentLU]

	ALTER TABLE [dbo].[MSTTravelTrip]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTrip_TravelTripTaskElement] FOREIGN KEY([TravelTripTaskElementID])
		REFERENCES [dbo].[TravelTripTaskElement] ([TravelTripTaskElementID])
	ALTER TABLE [dbo].[MSTTravelTrip] CHECK CONSTRAINT [FK_MSTTravelTrip_TravelTripTaskElement]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MSTTravelTripCustomFieldValueXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MSTTravelTripCustomFieldValueXREF](
		[MSTTCFVID] [int] IDENTITY(1,1) NOT NULL,
		[MSTTravelTripID] [int] NOT NULL,
		[MSTCustomFieldValueID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		CONSTRAINT [PK_MSTTravelTripCustomFieldValueXREF] PRIMARY KEY CLUSTERED 
		(
			[MSTTCFVID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue] FOREIGN KEY([MSTCustomFieldValueID])
		REFERENCES [dbo].[CustomFieldValue] ([CustomFieldValueID])
	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] CHECK CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTCustomFieldValue]

	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF]  WITH NOCHECK ADD  CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip] FOREIGN KEY([MSTTravelTripID])
		REFERENCES [dbo].[MSTTravelTrip] ([MSTTravelTripID])
	ALTER TABLE [dbo].[MSTTravelTripCustomFieldValueXREF] CHECK CONSTRAINT [FK_MSTTravelTripCustomFieldValueXREF_MSTTravelTrip]

END
GO

-- These changes are to be executed in MST only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. MST is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 2000 AND LineOfBusinessId < 2999)
	AND NOT EXISTS(SELECT 1 FROM [dbo].[MSTTravelModeLU] WHERE MSTTravelModeID = 1)
BEGIN
		Insert Into [dbo].[MSTTravelModeLU] Values(1,'Domestic – Zone - No Airfare')
		Insert Into [dbo].[MSTTravelModeLU] Values(2,'Domestic – Zone - Round Trip Airfare')
		Insert Into [dbo].[MSTTravelModeLU] Values(3,'Domestic – Non Zone')
		Insert Into [dbo].[MSTTravelModeLU] Values(4,'International')
END
GO

/*
	## END ##
	
	9/14/2016 [Tom & RJ] -- Additional work for MST Zone Travel
*/