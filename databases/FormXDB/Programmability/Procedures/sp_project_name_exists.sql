CREATE OR ALTER PROCEDURE dbo.sp_project_name_exists
    @tenantid  UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.projects
    WHERE tenantid = @tenantid AND isdeleted = 0 AND projectname = @name
      AND (@excludeid IS NULL OR projectid <> @excludeid);
END
GO
