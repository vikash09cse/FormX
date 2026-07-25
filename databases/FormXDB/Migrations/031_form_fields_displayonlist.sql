SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Show selected fields as columns on the submissions list page.

IF COL_LENGTH('dbo.form_fields', 'displayonlist') IS NULL
BEGIN
    ALTER TABLE dbo.form_fields ADD displayonlist BIT NOT NULL
        CONSTRAINT DF_form_fields_displayonlist DEFAULT (0);
END
GO
