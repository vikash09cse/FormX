CREATE OR ALTER PROCEDURE dbo.sp_update_tenant
    @tenantid                 UNIQUEIDENTIFIER,
    @hospitalname             NVARCHAR(200),
    @primarycontactfirstname  NVARCHAR(100),
    @primarycontactlastname   NVARCHAR(100),
    @primarycontactemail      NVARCHAR(100),
    @primarycontactphone      NVARCHAR(15),
    @tenantaddress            NVARCHAR(500),
    @timezone                 NVARCHAR(50),
    @logourl                  NVARCHAR(500) = NULL,
    @website                  NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tenants
    SET hospitalname            = @hospitalname,
        primarycontactfirstname = @primarycontactfirstname,
        primarycontactlastname  = @primarycontactlastname,
        primarycontactemail     = @primarycontactemail,
        primarycontactphone     = @primarycontactphone,
        tenantaddress           = @tenantaddress,
        timezone                = @timezone,
        logourl                 = @logourl,
        website                 = NULLIF(LTRIM(RTRIM(@website)), ''),
        updatedat               = SYSUTCDATETIME()
    WHERE tenantid = @tenantid
      AND isdeleted = 0;
END
GO
