IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'blocks' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.blocks (
        blockid     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_blocks PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid    UNIQUEIDENTIFIER NOT NULL,
        districtid  UNIQUEIDENTIFIER NOT NULL,
        name        NVARCHAR(200)    NOT NULL,
        code        NVARCHAR(50)     NOT NULL,
        status      TINYINT          NOT NULL CONSTRAINT DF_blocks_status DEFAULT (1),
        isdeleted   BIT              NOT NULL CONSTRAINT DF_blocks_isdeleted DEFAULT (0),
        createdby   UNIQUEIDENTIFIER NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_blocks_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby   UNIQUEIDENTIFIER NULL,
        updatedat   DATETIME2        NOT NULL CONSTRAINT DF_blocks_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_blocks_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_blocks_district FOREIGN KEY (districtid) REFERENCES dbo.districts (districtid)
    );

    CREATE UNIQUE INDEX UQ_blocks_tenant_code
        ON dbo.blocks (tenantid, code)
        WHERE isdeleted = 0;

    CREATE UNIQUE INDEX UQ_blocks_tenant_district_name
        ON dbo.blocks (tenantid, districtid, name)
        WHERE isdeleted = 0;

    CREATE INDEX IX_blocks_tenantid ON dbo.blocks (tenantid) WHERE isdeleted = 0;
    CREATE INDEX IX_blocks_districtid ON dbo.blocks (districtid) WHERE isdeleted = 0;
END
GO
