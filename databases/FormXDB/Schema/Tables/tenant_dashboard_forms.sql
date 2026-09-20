IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tenant_dashboard_forms' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.tenant_dashboard_forms (
        tenantid UNIQUEIDENTIFIER NOT NULL,
        formid   UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_tenant_dashboard_forms PRIMARY KEY (tenantid, formid),
        CONSTRAINT FK_tenant_dashboard_forms_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_tenant_dashboard_forms_form FOREIGN KEY (formid) REFERENCES dbo.forms (formid)
    );

    CREATE INDEX IX_tenant_dashboard_forms_form
        ON dbo.tenant_dashboard_forms (formid);
END
GO
