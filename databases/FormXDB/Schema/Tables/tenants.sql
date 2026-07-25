IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tenants' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tenants (
        tenantid                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tenants PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        hospitalname            NVARCHAR(200)    NOT NULL,
        subdomain               NVARCHAR(50)     NOT NULL,
        primarycontactfirstname NVARCHAR(100)    NOT NULL,
        primarycontactlastname NVARCHAR(100)    NOT NULL,
        primarycontactemail     NVARCHAR(100)    NOT NULL,
        primarycontactphone     NVARCHAR(15)     NOT NULL,
        tenantaddress           NVARCHAR(500)    NOT NULL,
        tenantstatus            TINYINT          NOT NULL CONSTRAINT DF_tenants_tenantstatus DEFAULT (1),
        logourl                 NVARCHAR(500)    NULL,
        website                 NVARCHAR(200)    NULL,
        timezone                NVARCHAR(50)     NOT NULL,
        isdeleted               BIT              NOT NULL CONSTRAINT DF_tenants_isdeleted DEFAULT (0),
        createdat               DATETIME2        NOT NULL CONSTRAINT DF_tenants_createdat DEFAULT (SYSUTCDATETIME()),
        updatedat               DATETIME2        NOT NULL CONSTRAINT DF_tenants_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tenants_subdomain UNIQUE (subdomain)
    );

    CREATE INDEX IX_tenants_tenantstatus ON dbo.tenants (tenantstatus) WHERE isdeleted = 0;
END
GO
