CREATE OR ALTER PROCEDURE dbo.sp_delete_tenant_user
    @tenantid  UNIQUEIDENTIFIER,
    @userid    UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET isdeleted = 1,
        updatedby = @updatedby,
        updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0
      AND usertype IN (2, 3); -- Staff, Doctor only
END
GO
