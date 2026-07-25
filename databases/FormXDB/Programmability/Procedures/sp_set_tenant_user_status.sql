CREATE OR ALTER PROCEDURE dbo.sp_set_tenant_user_status
    @tenantid   UNIQUEIDENTIFIER,
    @userid     UNIQUEIDENTIFIER,
    @userstatus TINYINT,
    @updatedby  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.users
    SET userstatus = @userstatus,
        updatedby  = @updatedby,
        updatedat  = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND userid = @userid
      AND isdeleted = 0;
END
GO
