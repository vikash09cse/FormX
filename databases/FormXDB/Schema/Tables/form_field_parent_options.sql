IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'form_field_parent_options' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.form_field_parent_options (
        fieldid     UNIQUEIDENTIFIER NOT NULL,
        optionid    UNIQUEIDENTIFIER NOT NULL,
        createdat   DATETIME2        NOT NULL CONSTRAINT DF_form_field_parent_options_createdat DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_form_field_parent_options PRIMARY KEY (fieldid, optionid),
        CONSTRAINT FK_form_field_parent_options_field FOREIGN KEY (fieldid) REFERENCES dbo.form_fields (fieldid),
        CONSTRAINT FK_form_field_parent_options_option FOREIGN KEY (optionid) REFERENCES dbo.form_field_options (optionid)
    );

    CREATE INDEX IX_form_field_parent_options_optionid ON dbo.form_field_parent_options (optionid);
END
GO
