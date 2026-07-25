IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'projects' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.projects (
        projectid   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_projects PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        projectname NVARCHAR(200)    NOT NULL,
        code        NVARCHAR(50)     NULL,
        status      TINYINT          NOT NULL CONSTRAINT DF_projects_status DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_projects_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_projects_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_projects_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_projects_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid)
    );

    CREATE UNIQUE INDEX UQ_projects_tenant_name
        ON dbo.projects (tenantid, projectname)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_projects_tenant_code
        ON dbo.projects (tenantid, code)
        WHERE code IS NOT NULL AND isdeleted = 0;

    CREATE INDEX IX_projects_tenantid ON dbo.projects (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_projects_status ON dbo.projects (tenantid, status) WHERE isdeleted = 0;
END
GO
