CREATE OR ALTER PROCEDURE dbo.sp_validation_regex_preset_get_list
AS
BEGIN
    SET NOCOUNT ON;
    SELECT validationregexpresetid, name, pattern, description, displayorder
    FROM dbo.validation_regex_presets
    WHERE isactive = 1
    ORDER BY displayorder, name;
END
GO
