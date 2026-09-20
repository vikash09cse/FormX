IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'roles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.roles (
        roleid      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_roles PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        name        NVARCHAR(100)    NOT NULL,
        isleader    BIT              NOT NULL CONSTRAINT DF_roles_isleader DEFAULT (0),
        datascope   TINYINT          NOT NULL CONSTRAINT DF_roles_datascope DEFAULT (0),
        cancreate   BIT              NOT NULL CONSTRAINT DF_roles_cancreate DEFAULT (1),
        canedit     BIT              NOT NULL CONSTRAINT DF_roles_canedit DEFAULT (1),
        candelete   BIT              NOT NULL CONSTRAINT DF_roles_candelete DEFAULT (1),
        rolestatus  TINYINT          NOT NULL CONSTRAINT DF_roles_rolestatus DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_roles_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_roles_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_roles_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_roles_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid)
    );

    CREATE UNIQUE INDEX UQ_roles_tenant_name
        ON dbo.roles (tenantid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_roles_tenantid ON dbo.roles (tenantid) WHERE isdeleted = 0;
END
GO
