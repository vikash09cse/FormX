IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_field_options' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_field_options (
        optionid     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_field_options PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        fieldid      UNIQUEIDENTIFIER NOT NULL,
        optiontext   NVARCHAR(250)    NOT NULL,
        optionvalue  NVARCHAR(250)    NOT NULL,
        displayorder INT              NOT NULL CONSTRAINT DF_form_field_options_displayorder DEFAULT (0),
        isdeleted    BIT              NOT NULL CONSTRAINT DF_form_field_options_isdeleted DEFAULT (0),
        createdby    UNIQUEIDENTIFIER NULL,
        createdat    DATETIME2        NOT NULL CONSTRAINT DF_form_field_options_createdat DEFAULT (SYSUTCDATETIME()),
        updatedby    UNIQUEIDENTIFIER NULL,
        updatedat    DATETIME2        NOT NULL CONSTRAINT DF_form_field_options_updatedat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_form_field_options_field FOREIGN KEY (fieldid) REFERENCES dbo.form_fields (fieldid)
    );

    CREATE INDEX IX_form_field_options_fieldid
        ON dbo.form_field_options (fieldid, displayorder)
        WHERE isdeleted = 0;
END
GO
