IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'validation_regex_presets' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.validation_regex_presets (
        validationregexpresetid UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_validation_regex_presets PRIMARY KEY,
        name                    NVARCHAR(100)    NOT NULL,
        pattern                 NVARCHAR(500)    NOT NULL,
        description             NVARCHAR(300)    NULL,
        displayorder            INT              NOT NULL CONSTRAINT DF_validation_regex_presets_displayorder DEFAULT (0),
        isactive                BIT              NOT NULL CONSTRAINT DF_validation_regex_presets_isactive DEFAULT (1)
    );

    CREATE UNIQUE INDEX UQ_validation_regex_presets_name ON dbo.validation_regex_presets (name);
END
GO
