IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tenant_settings' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tenant_settings (
        tenantsettingsid        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tenant_settings PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid                UNIQUEIDENTIFIER NOT NULL,
        visitfeeamount          DECIMAL(18, 2)   NOT NULL CONSTRAINT DF_tenant_settings_visitfee DEFAULT (0),
        freevisitwindowdays     INT              NOT NULL CONSTRAINT DF_tenant_settings_freewindow DEFAULT (10),
        currency                NVARCHAR(3)      NOT NULL CONSTRAINT DF_tenant_settings_currency DEFAULT ('INR'),
        patientidprefix         NVARCHAR(10)     NULL,
        brandingprimarycolor    NVARCHAR(20)     NULL,
        brandingsecondarycolor  NVARCHAR(20)     NULL,
        receiptheadertext       NVARCHAR(300)    NULL,
        receiptfootertext       NVARCHAR(300)    NULL,
        gsttaxnumber            NVARCHAR(50)     NULL,
        dashboardlayout         NVARCHAR(20)     NOT NULL CONSTRAINT DF_tenant_settings_dashboardlayout DEFAULT (N'table'),
        dashboardtopn           INT              NOT NULL CONSTRAINT DF_tenant_settings_dashboardtopn DEFAULT (12),
        createdat               DATETIME2        NOT NULL CONSTRAINT DF_tenant_settings_createdat DEFAULT (SYSUTCDATETIME()),
        updatedat               DATETIME2        NOT NULL CONSTRAINT DF_tenant_settings_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tenant_settings_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT UQ_tenant_settings_tenant UNIQUE (tenantid)
    );
END
GO

IF COL_LENGTH('dbo.tenant_settings', 'dashboardlayout') IS NULL
BEGIN
    ALTER TABLE dbo.tenant_settings
        ADD dashboardlayout NVARCHAR(20) NOT NULL
            CONSTRAINT DF_tenant_settings_dashboardlayout DEFAULT (N'table');
END
GO

IF COL_LENGTH('dbo.tenant_settings', 'dashboardtopn') IS NULL
BEGIN
    ALTER TABLE dbo.tenant_settings
        ADD dashboardtopn INT NOT NULL
            CONSTRAINT DF_tenant_settings_dashboardtopn DEFAULT (12);
END
GO
