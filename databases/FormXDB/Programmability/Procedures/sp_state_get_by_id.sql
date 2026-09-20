CREATE OR ALTER PROCEDURE dbo.sp_state_get_by_id
    @tenantid UNIQUEIDENTIFIER,
    @stateid  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT stateid, name, code, status, createdat, updatedat
    FROM dbo.states
    WHERE tenantid = @tenantid AND stateid = @stateid AND isdeleted = 0;
END
GO
