CREATE OR ALTER PROCEDURE dbo.sp_block_name_exists
    @tenantid   UNIQUEIDENTIFIER,
    @districtid UNIQUEIDENTIFIER,
    @name       NVARCHAR(200),
    @excludeid  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.blocks
    WHERE tenantid = @tenantid AND districtid = @districtid AND isdeleted = 0 AND name = @name
      AND (@excludeid IS NULL OR blockid <> @excludeid);
END
GO
