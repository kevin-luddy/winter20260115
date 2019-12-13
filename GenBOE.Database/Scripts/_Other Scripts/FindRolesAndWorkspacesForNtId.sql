--author
select workspaceid, workspacename, workspaceshortname from workspace where workspaceid in (
    select distinct workspaceId from boe where boeid in (
        select distinct r.boeId from boeuserRole r inner join etiuser e on r.etiuserid = e.etiuserid where r.roleid = 1 and e.ntid = 'n8153b'
        )
    )
	
--subcontract author
select workspaceid, workspacename, workspaceshortname from workspace where workspaceid in (
    select distinct workspaceId from boe where boeid in (
        select distinct r.boeId from boeuserRole r inner join etiuser e on r.etiuserid = e.etiuserid where r.roleid = 9 and e.ntid = 'n8153b'
        )
    )