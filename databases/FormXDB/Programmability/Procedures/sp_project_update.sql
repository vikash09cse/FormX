CREATE OR ALTER PROCEDURE dbo.sp_project_update
    @tenantid    UNIQUEIDENTIFIER,
    @projectid   UNIQUEIDENTIFIER,
    @projectname NVARCHAR(200),
    @code        NVARCHAR(50) = NULL,
    @status      TINYINT,
    @updatedby   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.projects
    SET projectname = @projectname, code = @code, status = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND projectid = @projectid AND isdeleted = 0;
END
GO
