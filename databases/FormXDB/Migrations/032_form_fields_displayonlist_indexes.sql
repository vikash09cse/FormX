SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Indexes for list-page field columns (separate from column add to avoid rollback).

IF COL_LENGTH('dbo.form_fields', 'displayonlist') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'IX_form_fields_displayonlist' AND object_id = OBJECT_ID('dbo.form_fields'))
BEGIN
    CREATE INDEX IX_form_fields_displayonlist
        ON dbo.form_fields (formid)
        WHERE displayonlist = 1 AND isdeleted = 0;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_form_submission_values_field_submission'
      AND object_id = OBJECT_ID('dbo.form_submission_values'))
BEGIN
    CREATE INDEX IX_form_submission_values_field_submission
        ON dbo.form_submission_values (fieldid, submissionid);
END
GO
