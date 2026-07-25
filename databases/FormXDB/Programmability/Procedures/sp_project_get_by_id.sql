CREATE OR ALTER PROCEDURE dbo.sp_project_get_by_id
    @tenantid   UNIQUEIDENTIFIER,
    @projectid  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT projectid, projectname, code, status, createdat, updatedat
    FROM dbo.projects
    WHERE tenantid = @tenantid AND projectid = @projectid AND isdeleted = 0;
END
GO
