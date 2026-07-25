CREATE OR ALTER PROCEDURE dbo.sp_get_tenant_user_by_id
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.userid,
        u.email,
        u.firstname,
        u.lastname,
        u.designation,
        u.usertype AS role,
        u.userstatus AS status,
        u.lastloginat,
        u.createdat
    FROM dbo.users u
    WHERE u.tenantid = @tenantid
      AND u.userid = @userid
      AND u.isdeleted = 0;
END
GO
