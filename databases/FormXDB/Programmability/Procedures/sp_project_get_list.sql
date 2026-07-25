CREATE OR ALTER PROCEDURE dbo.sp_project_get_list
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT projectid, projectname, code, status, createdat, updatedat
    FROM dbo.projects
    WHERE tenantid = @tenantid AND isdeleted = 0
    ORDER BY projectname;
END
GO
