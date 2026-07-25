CREATE OR ALTER PROCEDURE dbo.sp_set_tenant_status
    @tenantid     UNIQUEIDENTIFIER,
    @tenantstatus TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tenants
    SET tenantstatus = @tenantstatus,
        updatedat    = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND isdeleted = 0;
END
GO
