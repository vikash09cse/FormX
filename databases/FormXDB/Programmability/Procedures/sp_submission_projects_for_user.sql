CREATE OR ALTER PROCEDURE dbo.sp_submission_projects_for_user
    @tenantid UNIQUEIDENTIFIER,
    @userid   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.user_scopes WHERE userid = @userid)
    BEGIN
        SELECT p.projectid, p.projectname, p.code
        FROM dbo.projects p
        INNER JOIN dbo.user_scopes us ON us.projectid = p.projectid AND us.userid = @userid
        WHERE p.tenantid = @tenantid AND p.isdeleted = 0 AND p.status = 1
        ORDER BY p.projectname;
        RETURN;
    END;

    SELECT p.projectid, p.projectname, p.code
    FROM dbo.projects p
    WHERE p.tenantid = @tenantid AND p.isdeleted = 0 AND p.status = 1
    ORDER BY p.projectname;
END
GO
