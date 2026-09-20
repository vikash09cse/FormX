CREATE OR ALTER PROCEDURE dbo.sp_state_code_exists
    @tenantid  UNIQUEIDENTIFIER,
    @code      NVARCHAR(50),
    @excludeid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM dbo.states
    WHERE tenantid = @tenantid AND isdeleted = 0 AND code = @code
      AND (@excludeid IS NULL OR stateid <> @excludeid);
END
GO
