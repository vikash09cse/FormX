IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'districts' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.districts (
        districtid  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_districts PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        stateid     UNIQUEIDENTIFIER NOT NULL,
        name        NVARCHAR(200)    NOT NULL,
        code        NVARCHAR(50)     NOT NULL,
        status      TINYINT          NOT NULL CONSTRAINT DF_districts_status DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_districts_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_districts_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_districts_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_districts_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_districts_state FOREIGN KEY (stateid) REFERENCES dbo.states (stateid)
    );

    CREATE UNIQUE INDEX UQ_districts_tenant_code
        ON dbo.districts (tenantid, code)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_districts_tenant_state_name
        ON dbo.districts (tenantid, stateid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_districts_tenantid ON dbo.districts (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_districts_stateid ON dbo.districts (stateid) WHERE isdeleted = 0;
END
GO
