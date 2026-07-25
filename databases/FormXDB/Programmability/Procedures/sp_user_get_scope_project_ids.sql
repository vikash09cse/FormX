CREATE OR ALTER PROCEDURE dbo.sp_user_get_scope_project_ids
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT us.projectid
    FROM dbo.user_scopes us
    INNER JOIN dbo.users u ON u.userid = us.userid
    INNER JOIN dbo.projects p ON p.projectid = us.projectid
    WHERE u.tenantid = @tenantid AND us.userid = @userid AND u.isdeleted = 0 AND p.isdeleted = 0;
END
GO
