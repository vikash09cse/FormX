CREATE OR ALTER PROCEDURE dbo.sp_form_field_save
    @tenantid                   UNIQUEIDENTIFIER,
    @formid                     UNIQUEIDENTIFIER,
    @fieldid                    UNIQUEIDENTIFIER,
    @formgroupid                UNIQUEIDENTIFIER,
    @controllabel               NVARCHAR(300),
    @controltype                TINYINT,
    @controlmaxlength           INT              = NULL,
    @controlrequired            BIT,
    @displayorder               INT,
    @controlnotes               NVARCHAR(1000)   = NULL,
    @fieldkey                   NVARCHAR(200),
    @displaycontrollabel        NVARCHAR(50),
    @classname                  NVARCHAR(100)    = NULL,
    @parentfieldid              UNIQUEIDENTIFIER = NULL,
    @issendemailnotification    BIT,
    @validationregexpresetid    UNIQUEIDENTIFIER = NULL,
    @displayonlist              BIT              = 0,
    @optionsjson                NVARCHAR(MAX)    = NULL, -- [{ "optionid","optiontext","optionvalue","displayorder" }]
    @parentoptionids            NVARCHAR(MAX)    = NULL, -- comma-separated GUIDs
    @actorid                    UNIQUEIDENTIFIER,
    @isnew                      BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.forms WHERE tenantid = @tenantid AND formid = @formid AND isdeleted = 0)
    BEGIN
        RAISERROR('Form not found.', 16, 1);
        RETURN;
    END;

    -- Label control is never shown on list
    IF @controltype = 7
        SET @displayonlist = 0;

    IF @displayonlist = 1
    BEGIN
        DECLARE @listCount INT;
        SELECT @listCount = COUNT(1)
        FROM dbo.form_fields
        WHERE formid = @formid AND isdeleted = 0 AND displayonlist = 1 AND fieldid <> @fieldid;

        IF @listCount >= 5
        BEGIN
            RAISERROR('At most 5 fields can be shown on the list page.', 16, 1);
            RETURN;
        END;
    END;

    BEGIN TRANSACTION;

    IF @isnew = 1
    BEGIN
        INSERT INTO dbo.form_fields
            (fieldid, formid, formgroupid, controllabel, controltype, controlmaxlength, controlrequired, displayorder,
             controlnotes, fieldkey, displaycontrollabel, classname, parentfieldid, issendemailnotification,
             validationregexpresetid, displayonlist, createdby, updatedby)
        VALUES
            (@fieldid, @formid, @formgroupid, @controllabel, @controltype, @controlmaxlength, @controlrequired, @displayorder,
             @controlnotes, @fieldkey, @displaycontrollabel, @classname, @parentfieldid, @issendemailnotification,
             @validationregexpresetid, @displayonlist, @actorid, @actorid);
    END
    ELSE
    BEGIN
        UPDATE dbo.form_fields SET
            formgroupid = @formgroupid,
            controllabel = @controllabel,
            controltype = @controltype,
            controlmaxlength = @controlmaxlength,
            controlrequired = @controlrequired,
            displayorder = @displayorder,
            controlnotes = @controlnotes,
            fieldkey = @fieldkey,
            displaycontrollabel = @displaycontrollabel,
            classname = @classname,
            parentfieldid = @parentfieldid,
            issendemailnotification = @issendemailnotification,
            validationregexpresetid = @validationregexpresetid,
            displayonlist = @displayonlist,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        WHERE fieldid = @fieldid AND formid = @formid AND isdeleted = 0;

        UPDATE dbo.form_field_options
        SET isdeleted = 1, updatedby = @actorid, updatedat = SYSUTCDATETIME()
        WHERE fieldid = @fieldid AND isdeleted = 0;

        DELETE FROM dbo.form_field_parent_options WHERE fieldid = @fieldid;
    END;

    IF @optionsjson IS NOT NULL AND LTRIM(RTRIM(@optionsjson)) <> '' AND LTRIM(RTRIM(@optionsjson)) <> '[]'
    BEGIN
        DECLARE @opt TABLE (
            optionid     UNIQUEIDENTIFIER NOT NULL,
            optiontext   NVARCHAR(250)    NOT NULL,
            optionvalue  NVARCHAR(250)    NOT NULL,
            displayorder INT              NOT NULL
        );

        INSERT INTO @opt (optionid, optiontext, optionvalue, displayorder)
        SELECT
            TRY_CAST(j.optionid AS UNIQUEIDENTIFIER),
            LTRIM(RTRIM(j.optiontext)),
            LTRIM(RTRIM(j.optionvalue)),
            ISNULL(j.displayorder, 0)
        FROM OPENJSON(@optionsjson)
        WITH (
            optionid     NVARCHAR(50)  '$.optionid',
            optiontext   NVARCHAR(250) '$.optiontext',
            optionvalue  NVARCHAR(250) '$.optionvalue',
            displayorder INT           '$.displayorder'
        ) j
        WHERE TRY_CAST(j.optionid AS UNIQUEIDENTIFIER) IS NOT NULL
          AND j.optiontext IS NOT NULL
          AND LTRIM(RTRIM(j.optiontext)) <> '';

        UPDATE o SET
            fieldid = @fieldid,
            optiontext = s.optiontext,
            optionvalue = s.optionvalue,
            displayorder = s.displayorder,
            isdeleted = 0,
            updatedby = @actorid,
            updatedat = SYSUTCDATETIME()
        FROM dbo.form_field_options o
        INNER JOIN @opt s ON s.optionid = o.optionid;

        INSERT INTO dbo.form_field_options
            (optionid, fieldid, optiontext, optionvalue, displayorder, createdby, updatedby)
        SELECT s.optionid, @fieldid, s.optiontext, s.optionvalue, s.displayorder, @actorid, @actorid
        FROM @opt s
        WHERE NOT EXISTS (SELECT 1 FROM dbo.form_field_options o WHERE o.optionid = s.optionid);
    END;

    IF @parentoptionids IS NOT NULL AND LTRIM(RTRIM(@parentoptionids)) <> ''
    BEGIN
        INSERT INTO dbo.form_field_parent_options (fieldid, optionid)
        SELECT @fieldid, TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER)
        FROM STRING_SPLIT(@parentoptionids, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS UNIQUEIDENTIFIER) IS NOT NULL;
    END;

    COMMIT TRANSACTION;
END
GO
