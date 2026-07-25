IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'global_document_templates' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.global_document_templates (
        globaldocumenttemplateid UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_global_document_templates PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        templatetype             TINYINT          NOT NULL,
        name                     NVARCHAR(200)    NOT NULL,
        subject                  NVARCHAR(300)    NULL,
        bodyhtml                 NVARCHAR(MAX)    NOT NULL,
        isdefault                BIT              NOT NULL CONSTRAINT DF_global_document_templates_isdefault DEFAULT (0),
        isdeleted                BIT              NOT NULL CONSTRAINT DF_global_document_templates_isdeleted DEFAULT (0),
        createdby                UNIQUEIDENTIFIER NOT NULL,
        createdat                DATETIME2        NOT NULL CONSTRAINT DF_global_document_templates_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby                UNIQUEIDENTIFIER NOT NULL,
        updatedat                DATETIME2        NOT NULL CONSTRAINT DF_global_document_templates_updatedat DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_global_document_templates_type
        ON dbo.global_document_templates (templatetype, name)
        WHERE isdeleted = 0;

    EXEC(N'
    CREATE UNIQUE INDEX UQ_global_document_templates_default
        ON dbo.global_document_templates (templatetype)
        WHERE isdefault = 1 AND isdeleted = 0;
    ');
END
GO
