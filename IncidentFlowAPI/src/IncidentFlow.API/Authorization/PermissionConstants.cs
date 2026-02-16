namespace IncidentFlow.API.Authorization;

public static class PermissionConstants
{
    public const string IncidentsRead = "incidents:read";
    public const string IncidentsCreate = "incidents:create";
    public const string IncidentsEditOwn = "incidents:edit:own";
    public const string IncidentsEditAny = "incidents:edit:any";
    public const string IncidentsStatusLimited = "incidents:status:limited";
    public const string IncidentsStatusAny = "incidents:status:any";
    public const string IncidentsSeverityAny = "incidents:severity:any";
    public const string IncidentsAssign = "incidents:assign";
    public const string IncidentsDelete = "incidents:delete";
    public const string IncidentsRestore = "incidents:restore";
    public const string IncidentsAuditRead = "incidents:audit:read";
    public const string UsersManage = "users:manage";
    public const string RolesManage = "roles:manage";
    public const string DashboardBasic = "dashboard:basic";
    public const string DashboardFull = "dashboard:full";
}
