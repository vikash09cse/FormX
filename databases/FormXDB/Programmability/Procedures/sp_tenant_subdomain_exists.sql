CREATE OR ALTER PROCEDURE dbo.sp_tenant_subdomain_exists
    @subdomain NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1) AS subdomaincount
    FROM dbo.tenants
    WHERE subdomain = @subdomain
      AND isdeleted = 0;
END
GO
