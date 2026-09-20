CREATE OR ALTER PROCEDURE dbo.sp_user_get_effective_permissions
    @userid   UNIQUEIDENTIFIER,
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: aggregated capabilities
    SELECT
        CAST(COALESCE(MAX(r.datascope), 0) AS TINYINT) AS datascope,
        CAST(COALESCE(MAX(CAST(r.cancreate AS INT)), 0) AS BIT) AS cancreate,
        CAST(COALESCE(MAX(CAST(r.canedit AS INT)), 0) AS BIT) AS canedit,
        CAST(COALESCE(MAX(CAST(r.candelete AS INT)), 0) AS BIT) AS candelete
    FROM dbo.user_roles ur
    INNER JOIN dbo.roles r ON r.roleid = ur.roleid AND r.isdeleted = 0 AND r.rolestatus = 1
    WHERE ur.userid = @userid AND r.tenantid = @tenantid;

    -- Result set 2: menu keys (union)
    SELECT DISTINCT rm.menukey
    FROM dbo.user_roles ur
    INNER JOIN dbo.roles r ON r.roleid = ur.roleid AND r.isdeleted = 0 AND r.rolestatus = 1
    INNER JOIN dbo.role_menus rm ON rm.roleid = r.roleid
    WHERE ur.userid = @userid AND r.tenantid = @tenantid
    ORDER BY rm.menukey;

    -- Result set 3: district scopes
    SELECT districtid FROM dbo.user_district_scopes WHERE userid = @userid;

    -- Result set 4: project scopes
    SELECT projectid FROM dbo.user_scopes WHERE userid = @userid;
END
GO
