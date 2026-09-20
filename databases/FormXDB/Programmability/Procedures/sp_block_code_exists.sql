CREATE OR ALTER PROCEDURE dbo.sp_block_code_exists
    @tenantid  UNIQUEIDENTIFIER,
    @code      NVARCHAR(50),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.blocks
    WHERE tenantid = @tenantid AND isdeleted = 0 AND code = @code
      AND (@excludeid IS NULL OR blockid <> @excludeid);
END
GO
