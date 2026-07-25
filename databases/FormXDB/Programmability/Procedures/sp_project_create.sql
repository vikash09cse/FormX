CREATE OR ALTER PROCEDURE dbo.sp_project_create
    @projectid   UNIQUEIDENTIFIER,
    @tenantid    UNIQUEIDENTIFIER,
    @projectname NVARCHAR(200),
    @code        NVARCHAR(50) = NULL,
    @status      TINYINT,
    @createdby   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.projects (projectid, tenantid, projectname, code, status, createdby, updatedby)
    VALUES (@projectid, @tenantid, @projectname, @code, @status, @createdby, @createdby);
    SELECT @projectid;
END
GO
