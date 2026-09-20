CREATE OR ALTER PROCEDURE dbo.sp_state_update
    @tenantid  UNIQUEIDENTIFIER,
    @stateid   UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @code      NVARCHAR(50),
    @status    TINYINT,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.states
    SET name = @name, code = @code, status = @status,
        updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0;
END
GO
