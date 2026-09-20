CREATE OR ALTER PROCEDURE dbo.sp_district_code_exists
    @tenantid  UNIQUEIDENTIFIER,
    @code      NVARCHAR(50),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.districts
    WHERE tenantid = @tenantid AND isdeleted = 0 AND code = @code
      AND (@excludeid IS NULL OR districtid <> @excludeid);
END
GO
