-- Dashboard layout defaults on tenant_settings
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
