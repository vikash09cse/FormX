CREATE OR ALTER PROCEDURE dbo.sp_project_delete
    @tenantid  UNIQUEIDENTIFIER,
    @projectid UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.projects
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND projectid = @projectid AND isdeleted = 0;
END
GO
