CREATE OR ALTER PROCEDURE dbo.sp_form_name_exists
    @tenantid  UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.forms
    WHERE tenantid = @tenantid AND isdeleted = 0 AND name = @name
      AND (@excludeid IS NULL OR formid <> @excludeid);
END
GO
