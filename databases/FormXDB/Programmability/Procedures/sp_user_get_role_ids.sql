CREATE OR ALTER PROCEDURE dbo.sp_user_get_role_ids
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ur.roleid
    FROM dbo.user_roles ur
    INNER JOIN dbo.users u ON u.userid = ur.userid
    INNER JOIN dbo.roles r ON r.roleid = ur.roleid
    WHERE u.tenantid = @tenantid AND ur.userid = @userid AND u.isdeleted = 0 AND r.isdeleted = 0;
END
GO
