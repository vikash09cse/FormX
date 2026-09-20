IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'villages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.villages (
        villageid   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_villages PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        blockid     UNIQUEIDENTIFIER NOT NULL,
        name        NVARCHAR(200)    NOT NULL,
        code        NVARCHAR(50)     NOT NULL,
        status      TINYINT          NOT NULL CONSTRAINT DF_villages_status DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_villages_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_villages_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_villages_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_villages_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_villages_block FOREIGN KEY (blockid) REFERENCES dbo.blocks (blockid)
    );

    CREATE UNIQUE INDEX UQ_villages_tenant_code
        ON dbo.villages (tenantid, code)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_villages_tenant_block_name
        ON dbo.villages (tenantid, blockid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_villages_tenantid ON dbo.villages (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_villages_blockid ON dbo.villages (blockid) WHERE isdeleted = 0;
END
GO
