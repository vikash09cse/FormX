IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'states' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.states (
        stateid     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_states PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        name        NVARCHAR(200)    NOT NULL,
        code        NVARCHAR(50)     NOT NULL,
        status      TINYINT          NOT NULL CONSTRAINT DF_states_status DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_states_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_states_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_states_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_states_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid)
    );

    CREATE UNIQUE INDEX UQ_states_tenant_code
        ON dbo.states (tenantid, code)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_states_tenant_name
        ON dbo.states (tenantid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_states_tenantid ON dbo.states (tenantid) WHERE isdeleted = 0;
END
GO
