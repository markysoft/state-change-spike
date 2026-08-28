# Simulation of Status Change

Checkout the [SchoolAccount-LocalDevTools](https://github.com/DFE-Digital/SchoolAccount-LocalDevTools) and switch to the `default-network` branch
Obtain a database backup and start the project via `docker compose up`

run `docker compose up` from the root of the project to add a `CollectStateLedger` database with table `CollectReturnStatus` and a Stored Procedure `AddChangedCollectReturnStatus`

The [script](./src/sql-server/setup.sql) creates the table with the following SQL:
```sql
CREATE TABLE [CollectReturnStatus]
(
    [Id]                    INT IDENTITY(1,1) PRIMARY KEY,
    [SchoolName]           [nvarchar] (250)   NOT NULL,
    [LAEStab]              [nvarchar] (50)   NOT NULL,
    [ReturnStatus]         [int] NOT NULL,
    [Errors]               [int] NOT NULL,
    [Queries]              [int] NOT NULL,
    [OkdErrorQueries]      [int] NOT NULL,
    [Hash]                  [nvarchar] (36)   NOT NULL,
    UpdatedAt              [datetime]         NOT NULL,
    DCID                  [int]              NOT NULL,
);
```

The snapshot of the database contains data for the 2025 Spring Census.

## .NET Tests to demonstrate status tracking

The repository contains a .NET project that can connect to the database, run queries, update and the stored procedure.

Of particular interest is the `SimulateDataChange` test. Running the test performs the following:

- Set 3 schools data in the `CollectPortal` `DataReturn` table to a known state.
- Clears down all data in the `CollectStateLedger` `CollectReturnStatus` table
- Runs the stored procedure to populate the current statuses in th `CollectReturnStatus` table with a time stamp of one week ago
- Updates three schools data in the `CollectPortal` `DataReturn` table to change the number of errors, queries, ok'd errors and status
- Runs the stored procedure again
- Checks that only one row has been added
- Runs the SP again, demonstrating no rows are added on a re-run
- Validates the updated records have the expected values

## SQL commands to demonstrate status tracking

Run an initial load of the census state into the `CollectReturnStatus` table, with the datestamp set to a week ago:
```
USE [CollectStateLedger];
EXEC AddChangedCollectReturnStatus 
    @CensusName = 'SchoolCensus2025_Spring',
    @DaysSubtract = 7;
```

Verify there are rows in the table:
```
SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]
```

  There should be 21873 records

Running the same query again should not increase the number of rows.

The following query will return all rows in the data for the census:
  ```
SELECT TOP (1000) [DataReturnID]
      ,[DCID]
      ,[AgentOrganisationRoleID]
      ,[SourceOrganisationRoleID]
      ,[DRStatus]
      ,[DRControl]
      ,[SubmittedDate]
      ,[ApprovedDate]
      ,[AuthorisedDate]
      ,[DRState]
      ,[SourceQueueID]
      ,[AgentQueueID]
      ,[CollectorQueueID]
      ,[CollectorAgentQueueID]
      ,[LowErrors]
      ,[MediumErrors]
      ,[HighErrors]
      ,[OKErrors]
      ,[ValidatingHost]
      ,[UserStamp]
      ,[DateStamp]
  FROM [COLLECTPortal].[dbo].[DataReturn]
  where DCID = 1172
  ```

  The starting ID for census 1172 is 7383500.

  Select a record to update, i.e.
  ```
  update [COLLECTPortal].[dbo].[DataReturn]
  set DRStatus = 8, 
  HighErrors = 99,
  LowErrors = 100,
  OKErrors = 101
  where DataReturnID = 7383500
  ```

  run the query again to simulate an update 6 days ago:
  ```
  USE [CollectStateLedger];
  EXEC AddChangedCollectReturnStatus 
      @CensusName = 'SchoolCensus2025_Spring',
      @DaysSubtract = 6;
  ```

  There should now be 21874 rows. The number should not change if running the query again.
  Check the records for the school, which for the above record is:
  ```
  SELECT TOP (1000) [Id]
      ,[SchoolName]
      ,[LAEStab]
      ,[ReturnStatus]
      ,[Errors]
      ,[Queries]
      ,[OkdErrorQueries]
      ,[Hash]
      ,[UpdatedAt]
      ,[DCID]
  FROM [CollectStateLedger].[dbo].[CollectReturnStatus]
  where LAEStab = '3712195'
 order by UpdatedAt desc
 ```

 Change the figures back to what they were, i.e.
```
  update [COLLECTPortal].[dbo].[DataReturn]
  set DRStatus = 7, 
  HighErrors = 0,
  LowErrors = 0,
  OkErrors = 4
  where DataReturnID = 7383500
  ```

  Run the SP again to update the changes setting the date to 5 days ago:
```
    USE [CollectStateLedger];
  EXEC AddChangedCollectReturnStatus 
      @CensusName = 'SchoolCensus2025_Spring',
      @DaysSubtract = 5;
```

Count is now 21875
```
SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]
```

Repeatedly running the stored procedure does not add any further rows