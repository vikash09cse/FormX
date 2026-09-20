CREATE OR ALTER PROCEDURE dbo.sp_village_code_exists
    @tenantid  UNIQUEIDENTIFIER,
    @code      NVARCHAR(50),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.villages
    WHERE tenantid = @tenantid AND isdeleted = 0 AND code = @code
      AND (@excludeid IS NULL OR villageid <> @excludeid);
END
GO
