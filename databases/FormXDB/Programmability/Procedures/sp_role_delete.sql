CREATE OR ALTER PROCEDURE dbo.sp_role_delete
    @tenantid  UNIQUEIDENTIFIER,
    @roleid    UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.roles
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND roleid = @roleid AND isdeleted = 0;
END
GO
