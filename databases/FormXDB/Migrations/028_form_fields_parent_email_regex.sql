-- Upgrade form_fields created before parent/email/regex columns existed.

IF COL_LENGTH('dbo.form_fields', 'parentfieldid') IS NULL
BEGIN
    ALTER TABLE dbo.form_fields ADD parentfieldid UNIQUEIDENTIFIER NULL;
END
GO

IF COL_LENGTH('dbo.form_fields', 'issendemailnotification') IS NULL
BEGIN
    ALTER TABLE dbo.form_fields ADD issendemailnotification BIT NOT NULL
        CONSTRAINT DF_form_fields_issendemailnotification DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.form_fields', 'validationregexpresetid') IS NULL
BEGIN
    ALTER TABLE dbo.form_fields ADD validationregexpresetid UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_fields_parent_field')
   AND COL_LENGTH('dbo.form_fields', 'parentfieldid') IS NOT NULL
BEGIN
    ALTER TABLE dbo.form_fields
        ADD CONSTRAINT FK_form_fields_parent_field
        FOREIGN KEY (parentfieldid) REFERENCES dbo.form_fields (fieldid);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_form_fields_validation_regex_preset')
   AND COL_LENGTH('dbo.form_fields', 'validationregexpresetid') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'validation_regex_presets')
BEGIN
    ALTER TABLE dbo.form_fields
        ADD CONSTRAINT FK_form_fields_validation_regex_preset
        FOREIGN KEY (validationregexpresetid) REFERENCES dbo.validation_regex_presets (validationregexpresetid);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_form_fields_parentfieldid' AND object_id = OBJECT_ID('dbo.form_fields'))
BEGIN
    CREATE INDEX IX_form_fields_parentfieldid
        ON dbo.form_fields (parentfieldid)
        WHERE parentfieldid IS NOT NULL AND isdeleted = 0;
END
GO
