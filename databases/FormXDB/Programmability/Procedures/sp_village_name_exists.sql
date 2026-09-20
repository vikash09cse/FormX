CREATE OR ALTER PROCEDURE dbo.sp_village_name_exists
    @tenantid  UNIQUEIDENTIFIER,
    @blockid   UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.villages
    WHERE tenantid = @tenantid AND blockid = @blockid AND isdeleted = 0 AND name = @name
      AND (@excludeid IS NULL OR villageid <> @excludeid);
END
GO
