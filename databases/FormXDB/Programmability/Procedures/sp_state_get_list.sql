CREATE OR ALTER PROCEDURE dbo.sp_state_get_list
    @tenantid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT stateid, name, code, status, createdat, updatedat
    FROM dbo.states
    WHERE tenantid = @tenantid AND isdeleted = 0
    ORDER BY name;
END
GO
