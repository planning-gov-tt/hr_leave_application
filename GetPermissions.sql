select [permission_id]
from [dbo].[employeerole] er
left join [dbo].[rolepermission] rp
on er.role_id = rp.role_id
where [employee_id]=6
