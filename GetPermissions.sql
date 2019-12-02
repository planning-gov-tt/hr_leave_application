select [permission_id]
from [HRLeaveTestDb].[dbo].[employeerole] er
left join [HRLeaveTestDb].[dbo].[rolepermission] rp
on er.role_id = rp.role_id
where [employee_id]=6
