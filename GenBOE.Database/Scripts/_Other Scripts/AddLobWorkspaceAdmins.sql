BEGIN TRANSACTION UserRoles;

    INSERT INTO WorkspaceUserRole (UpdateDT, ETIUserID, RoleID, WorkspaceID, HideHelp)
        SELECT GETDATE() as UpdateDate, g.ETIUserID, 4, g.WorkspaceID, 0
        FROM (
            select newUser.* --, r.*
            from 
            (Select u.etiuserId, s.workspaceId 
                from 
                    (select etiuserId from etiuser e 
                        where e.NTID in (
                            'e342740',
                            'e430378',
                            'e295500',
                            'e373738',
                            'e413614',
                            'e308448',
                            'roberjs',
                            'e385949',
                            'meitins',
                            'jiaquint',
                            'mnnguyen',
                            'e428665',
                            'e371900',
                            'e294875',
                            'e428629',
                            'blanchmj',
                            'labater'
                        )  
                    ) as u,
                    (select WorkspaceID from workspace w
                        where w.LineOfBusinessID in (
                            SELECT [LineOfBusinessID]
                            FROM [GenBOESpace_ProdClone].[dbo].[LineOfBusiness]
                            where LineOfBusinessName in ('Civil Space',  --
                            'Comm Space',  --
                            'Commercial Launch', --
                            'Commercial Ventures (COM)', --
                            'Commercial/Civil Space') --
                        )
                    ) as s
            ) as newUser
            left join WorkspaceUserRole r on newUser.etiuserId = r.ETIUserID AND newUser.workspaceId = r.WorkspaceID AND r.RoleID = 4
            where r.RoleID is null
        ) as g
 -- 31705 total
 -- 30113 missing

--ROLLBACK TRANSACTION UserRoles;
COMMIT TRANSACTION UserRoles;