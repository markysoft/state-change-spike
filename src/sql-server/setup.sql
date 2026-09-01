USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CollectStateLedger')
    BEGIN
        CREATE DATABASE [CollectStateLedger];
    END
GO

ALTER DATABASE CollectStateLedger COLLATE Latin1_General_CI_AS;

USE [CollectStateLedger];
GO

IF OBJECT_ID('CollectReturnStatus', 'U') IS NULL
BEGIN
CREATE TABLE [CollectReturnStatus]
(
    [Id]                    INT IDENTITY(1,1) PRIMARY KEY,
    [SchoolName]           [nvarchar] (250)   NOT NULL,
    [LAEStab]              [nvarchar] (50)   NOT NULL,
    [ReturnStatus]         [int] NOT NULL,
    [Errors]               [int] NOT NULL,
    [Queries]              [int] NOT NULL,
    [OkdErrorsQueries]      [int] NOT NULL,
    [Hash]                  [nvarchar] (36)   NOT NULL,
    UpdatedAt              [datetime]         NOT NULL,
    DCID                  [int]              NOT NULL,
);

END

GO

CREATE OR ALTER PROCEDURE AddChangedCollectReturnStatus
    @CensusName NVARCHAR(128),
    @DaysSubtract INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DCID INT
    
    -- Get the DCID based on the census name
    SELECT @DCID = DCID
    FROM COLLECTPortal.dbo.DataCollection
    WHERE DCBladeSQLDatabase = @CensusName
    
    -- Validate that we found a matching DCID
    IF @DCID IS NULL
    BEGIN
        RAISERROR('No DataCollection record found for census: %s', 16, 1, @CensusName)
        RETURN
    END
    
    -- Only insert rows where the hash is different from the last updated row or where no existing row exists
    INSERT INTO CollectStateLedger.dbo.CollectReturnStatus
    (
        SchoolName,
        LAEStab,
        ReturnStatus,
        Errors,
        Queries,
        OkdErrorsQueries,
        Hash,
        UpdatedAt,
        DCID
    )            
    SELECT 
        src.SchoolName,
        src.LAEStab,
        src.ReturnStatus,
        src.Errors,
        src.Queries,
        src.OkdErrorsQueries,
        src.Hash,
        src.UpdatedAt,
        src.DCID
    FROM
    (
        -- Calculate the current values
        SELECT     
            o.OrganisationName AS SchoolName,
            o.OrganisationNativeID AS LAEStab, 
            dr.DRStatus AS ReturnStatus,
            dr.HighErrors AS Errors,
            dr.LowErrors AS Queries,
            dr.OKErrors AS OkdErrorsQueries,
            CONVERT(VARCHAR(32), HASHBYTES('MD5', 
                CONCAT(CAST(dr.DRStatus AS VARCHAR), '|', 
                       CAST(dr.HighErrors AS VARCHAR), '|', 
                       CAST(dr.LowErrors AS VARCHAR), '|', 
                       CAST(dr.OKErrors AS VARCHAR))), 2) AS Hash,     
            DATEADD(DAY, -@DaysSubtract, GETDATE()) AS UpdatedAt,                   
            @DCID AS DCID
        FROM COLLECTPortal.dbo.Organisation o
        INNER JOIN COLLECTPortal.dbo.OrganisationRole orol
            ON orol.OrganisationID = o.OrganisationID
        INNER JOIN COLLECTPortal.dbo.DataReturn dr
            ON dr.SourceOrganisationRoleID = orol.OrganisationRoleID
        INNER JOIN COLLECTPortal.dbo.OrganisationRole orol2
            ON dr.AgentOrganisationRoleID = orol2.OrganisationRoleID
        INNER JOIN COLLECTPortal.dbo.Organisation o2
            ON orol2.OrganisationID = o2.OrganisationID
        WHERE dr.DCID = @DCID
    ) AS src
LEFT JOIN
    (
        -- Get ONLY the latest row's hash for each LAEStab and DCID
        SELECT 
            LAEStab,
            DCID,
            Hash,
            UpdatedAt,
            ROW_NUMBER() OVER (
                PARTITION BY LAEStab, DCID 
                ORDER BY UpdatedAt DESC, ID DESC
            ) AS rn
        FROM CollectStateLedger.dbo.CollectReturnStatus
        WHERE DCID = @DCID
    ) AS tgt
        ON tgt.LAEStab = src.LAEStab
        AND tgt.DCID = src.DCID
        AND tgt.rn = 1  -- Only the most recent row
    WHERE tgt.LAEStab IS NULL      -- No existing row exists
       OR tgt.Hash <> src.Hash     -- Hash differs from the latest row
END
GO