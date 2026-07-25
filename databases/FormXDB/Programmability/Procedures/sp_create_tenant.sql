CREATE OR ALTER PROCEDURE dbo.sp_create_tenant
    @tenantid                 UNIQUEIDENTIFIER,
    @userid                   UNIQUEIDENTIFIER,
    @tenantsettingsid         UNIQUEIDENTIFIER,
    @hospitalname             NVARCHAR(200),
    @subdomain                NVARCHAR(50),
    @primarycontactfirstname  NVARCHAR(100),
    @primarycontactlastname   NVARCHAR(100),
    @primarycontactemail      NVARCHAR(100),
    @primarycontactphone      NVARCHAR(15),
    @tenantaddress            NVARCHAR(500),
    @timezone                 NVARCHAR(50),
    @tenantstatus             TINYINT,
    @passwordhash             NVARCHAR(256),
    @patientidprefix          NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    INSERT INTO dbo.tenants (
        tenantid, hospitalname, subdomain, primarycontactfirstname, primarycontactlastname,
        primarycontactemail, primarycontactphone, tenantaddress, timezone, tenantstatus)
    VALUES (
        @tenantid, @hospitalname, @subdomain, @primarycontactfirstname, @primarycontactlastname,
        @primarycontactemail, @primarycontactphone, @tenantaddress, @timezone, @tenantstatus);

    INSERT INTO dbo.tenant_settings (
        tenantsettingsid, tenantid, visitfeeamount, freevisitwindowdays, currency, patientidprefix)
    VALUES (
        @tenantsettingsid, @tenantid, 0, 10, 'INR', @patientidprefix);

    INSERT INTO dbo.users (
        userid, tenantid, email, passwordhash, firstname, lastname, usertype, userstatus)
    VALUES (
        @userid, @tenantid, @primarycontactemail, @passwordhash,
        @primarycontactfirstname, @primarycontactlastname, 1, 1);

    -- Copy all global document templates into the new tenant
    INSERT INTO dbo.document_templates (
        documenttemplateid, tenantid, globaldocumenttemplateid,
        templatetype, name, subject, bodyhtml, isdefault,
        createdby, updatedby)
    SELECT
        NEWID(), @tenantid, g.globaldocumenttemplateid,
        g.templatetype, g.name, g.subject, g.bodyhtml, g.isdefault,
        @userid, @userid
    FROM dbo.global_document_templates g
    WHERE g.isdeleted = 0;

    COMMIT TRAN;

    SELECT @tenantid AS tenantid;
END
GO
