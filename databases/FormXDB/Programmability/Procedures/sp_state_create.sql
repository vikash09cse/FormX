CREATE OR ALTER PROCEDURE dbo.sp_state_create
    @stateid   UNIQUEIDENTIFIER,
    @tenantid  UNIQUEIDENTIFIER,
    @name      NVARCHAR(200),
    @code      NVARCHAR(50),
    @status    TINYINT,
    @createdby UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.states (stateid, tenantid, name, code, status, createdby, updatedby)
    VALUES (@stateid, @tenantid, @name, @code, @status, @createdby, @createdby);
    SELECT @stateid;
END
GO
