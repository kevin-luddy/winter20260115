USE genTRAC; 
GO 

DECLARE @NTID varchar(20); 

-- add admin
SET @NTID = 't-ipear2'; 

IF (SELECT COUNT(*) FROM genTRACUser where EXISTS (select * from genTRACUser where NTID = @NTID)) < 1
BEGIN 
   insert into genTRACUser values ((select GETDATE()), @NTID, 'acct04', 'Fc-Ipe, Tarev2 (RESOURCE)', '', null, 'Tarev2', 'Fc-Ipe', 0)
END 
        
IF (SELECT COUNT(*) FROM SystemUserRole where EXISTS (select * from SystemUserRole where UserID = (select UserID from genTRACUser where NTID = @NTID) AND RoleID = 10)) < 1
BEGIN 
   insert into SystemUserRole values ((select GETDATE()), (select UserID from genTRACUser where NTID = @NTID), 10)
END 

-- add system pricer
SET @NTID = 't-iped1'; 

IF (SELECT COUNT(*) FROM genTRACUser where EXISTS (select * from genTRACUser where NTID = @NTID)) < 1
BEGIN 
   insert into genTRACUser values ((select GETDATE()), @NTID, 'acct04', 'Fc-Ipe, Tdel1 (RESOURCE)', '', null, 'Tdel1', 'Fc-Ipe', 0)
END 
        
IF (SELECT COUNT(*) FROM SystemUserRole where EXISTS (select * from SystemUserRole where UserID = (select UserID from genTRACUser where NTID = @NTID) AND RoleID = 14)) < 1
BEGIN 
   insert into SystemUserRole values ((select GETDATE()), (select UserID from genTRACUser where NTID = @NTID), 14)
END

-- add viewer
SET @NTID = 't-iped2'; 

IF (SELECT COUNT(*) FROM genTRACUser where EXISTS (select * from genTRACUser where NTID = @NTID)) < 1
BEGIN 
   insert into genTRACUser values ((select GETDATE()), @NTID, 'acct04', 'Fc-Ipe, Tdel2 (RESOURCE)', '', null, 'Tdel2', 'Fc-Ipe', 0)
END 
        
IF (SELECT COUNT(*) FROM SystemUserRole where EXISTS (select * from SystemUserRole where UserID = (select UserID from genTRACUser where NTID = @NTID) AND RoleID = 13)) < 1
BEGIN 
   insert into SystemUserRole values ((select GETDATE()), (select UserID from genTRACUser where NTID = @NTID), 13)
   insert into ProductLineViewerXREF values ((Select Scope_Identity()), 1)
END  

-- add non-US system pricer
SET @NTID = 'au_test'; 

IF (SELECT COUNT(*) FROM genTRACUser where EXISTS (select * from genTRACUser where NTID = @NTID)) < 1
BEGIN 
   insert into genTRACUser values ((select GETDATE()), @NTID, 'AU', 'Fc-Eo, Au_Test (RESOURCE)', '', null, 'Au_Test', 'Fc-Eo', 0)
END 
        
IF (SELECT COUNT(*) FROM SystemUserRole where EXISTS (select * from SystemUserRole where UserID = (select UserID from genTRACUser where NTID = @NTID) AND RoleID = 14)) < 1
BEGIN 
   insert into SystemUserRole values ((select GETDATE()), (select UserID from genTRACUser where NTID = @NTID), 14)
END
