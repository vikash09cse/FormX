IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'document_templates' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.document_templates (
        documenttemplateid       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_document_templates PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        tenantid                 UNIQUEIDENTIFIER NOT NULL,
        globaldocumenttemplateid UNIQUEIDENTIFIER NULL,
        templatetype             TINYINT          NOT NULL,
        name                     NVARCHAR(200)    NOT NULL,
        subject                  NVARCHAR(300)    NULL,
        bodyhtml                 NVARCHAR(MAX)    NOT NULL,
        isdefault                BIT              NOT NULL CONSTRAINT DF_document_templates_isdefault DEFAULT (0),
        isdeleted                BIT              NOT NULL CONSTRAINT DF_document_templates_isdeleted DEFAULT (0),
        createdby                UNIQUEIDENTIFIER NOT NULL,
        createdat                DATETIME2        NOT NULL CONSTRAINT DF_document_templates_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby                UNIQUEIDENTIFIER NOT NULL,
        updatedat                DATETIME2        NOT NULL CONSTRAINT DF_document_templates_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_document_templates_tenant FOREIGN KEY (tenantid) REFERENCES dbo.tenants (tenantid),
        CONSTRAINT FK_document_templates_global FOREIGN KEY (globaldocumenttemplateid) REFERENCES dbo.global_document_templates (globaldocumenttemplateid)
    );

    CREATE INDEX IX_document_templates_tenant_type
        ON dbo.document_templates (tenantid, templatetype, name)
        WHERE isdeleted = 0;

    EXEC(N'
    CREATE UNIQUE INDEX UQ_document_templates_tenant_default
        ON dbo.document_templates (tenantid, templatetype)
        WHERE isdefault = 1 AND isdeleted = 0;
    ');
END
GO
