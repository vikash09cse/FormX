IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'forms' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.forms (
        formid       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_forms PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid     UNIQUEIDENTIFIER NOT NULL,
        projectid    UNIQUEIDENTIFIER NOT NULL,
        name         NVARCHAR(200)    NOT NULL,
        description  NVARCHAR(1000)   NULL,
        status       TINYINT          NOT NULL CONSTRAINT DF_forms_status DEFAULT (1),
        displayorder INT              NOT NULL CONSTRAINT DF_forms_displayorder DEFAULT (0),
        isdeleted    BIT              NOT NULL CONSTRAINT DF_forms_isdeleted DEFAULT (0),
        createdby    UNIQUEIDENTIFIER NULL,
        createdat    DATETIME2        NOT NULL CONSTRAINT DF_forms_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby    UNIQUEIDENTIFIER NULL,
        updatedat    DATETIME2        NOT NULL CONSTRAINT DF_forms_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_forms_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_forms_project FOREIGN KEY (projectid) REFERENCES dbo.projects (projectid)
    );

    CREATE UNIQUE INDEX UQ_forms_tenant_name
        ON dbo.forms (tenantid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_forms_tenantid ON dbo.forms (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_forms_status ON dbo.forms (tenantid, status, displayorder) WHERE isdeleted = 0;
    CREATE INDEX IX_forms_projectid ON dbo.forms (tenantid, projectid) WHERE isdeleted = 0;
END
GO
