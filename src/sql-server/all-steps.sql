USE [CollectStateLedger];
EXEC AddChangedCollectReturnStatus 
    @CensusName = 'SchoolCensus2025_Spring',
    @DaysSubtract = 7;


SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]  

USE [CollectStateLedger];
EXEC AddChangedCollectReturnStatus 
    @CensusName = 'SchoolCensus2025_Spring',
    @DaysSubtract = 7;


SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]    

  update [COLLECTPortal].[dbo].[DataReturn]
  set DRStatus = 8, 
  HighErrors = 99,
  LowErrors = 100,
  OKErrors = 101
  where DataReturnID = 7383500


  USE [CollectStateLedger];
  EXEC AddChangedCollectReturnStatus 
      @CensusName = 'SchoolCensus2025_Spring',
      @DaysSubtract = 6;

SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]    

  USE [CollectStateLedger];
  EXEC AddChangedCollectReturnStatus 
      @CensusName = 'SchoolCensus2025_Spring',
      @DaysSubtract = 6;

SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]    

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

   update [COLLECTPortal].[dbo].[DataReturn]
  set DRStatus = 7, 
  HighErrors = 0,
  LowErrors = 0,
  OkErrors = 4
  where DataReturnID = 7383500

      USE [CollectStateLedger];
  EXEC AddChangedCollectReturnStatus 
      @CensusName = 'SchoolCensus2025_Spring',
      @DaysSubtract = 5;

SELECT count(*) FROM [CollectStateLedger].[dbo].[CollectReturnStatus]          


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