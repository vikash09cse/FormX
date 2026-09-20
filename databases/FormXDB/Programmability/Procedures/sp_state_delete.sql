CREATE OR ALTER PROCEDURE dbo.sp_state_delete
    @tenantid  UNIQUEIDENTIFIER,
    @stateid   UNIQUEIDENTIFIER,
    @updatedby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.states
    SET isdeleted = 1, updatedby = @updatedby, updatedat = SYSUTCDATETIME()
    WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0;
END
GO
