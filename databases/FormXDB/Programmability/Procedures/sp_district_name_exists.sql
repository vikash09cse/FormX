CREATE OR ALTER PROCEDURE dbo.sp_district_name_exists
    @tenantid  UNIQUEIDENTIFIER,
    @stateid   UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.districts
    WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0 AND name = @name
      AND (@excludeid IS NULL OR districtid <> @excludeid);
END
GO
