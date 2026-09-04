using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace StateChangeSimulation;

public interface IDataManager
{
    Task<List<MinCensusState>> GetMinCollectStatus(string collection);
    Task<List<MinCensusState>> GetMinLedgerStatus(string collection);
    Task ClearDownLedger();
    Task<int> GetLedgerCount();
    Task<int> UpdateCensusLedger(string censusName, int daysSubtract);

    Task SimulateDataUpdate(string censusName, string laestab, int errors, int queries, int okdErrors,
        int status);
}

public class DataManager : IDataManager
{
    private readonly string _connectionString =
        "Server=localhost;Database=CollectStateLedger;User Id=SA;Password=MyStrongPassword123!;TrustServerCertificate=True;Encrypt=True;";

    private const string MIN_CORE_QUERY = @"  
            SELECT       
                         o.OrganisationName AS SchoolName,
                         o.OrganisationNativeID AS LAEstab, 
                         dr.DRStatus AS ReturnStatusCode,
                         dr.HighErrors AS Errors,
                         dr.LowErrors AS Queries,
                         dr.OKErrors AS OkdErrorsQueries,
						 dc.DCBladeSQLDatabase As Collection,
						 dc.DCID
            FROM COLLECTPortal.dbo.Organisation o
            INNER JOIN COLLECTPortal.dbo.OrganisationRole orol
                        ON orol.OrganisationID = o.OrganisationID
            INNER JOIN COLLECTPortal.dbo.DataReturn dr
                        ON dr.SourceOrganisationRoleID = orol.OrganisationRoleID
            INNER JOIN CollectPortal.dbo.OrganisationRole orol2
                        ON dr.AgentOrganisationRoleID = orol2.OrganisationRoleID
            INNER JOIN COLLECTPortal.dbo.Organisation o2
                        ON orol2.OrganisationID = o2.OrganisationID
			INNER JOIN COLLECTPortal.dbo.DataCollection dc
			ON dc.DCID = dr.DCID
			WHERE dc.DCBladeSQLDatabase = @Collection
";

    public async Task<CensusSummary?> GetCensusSummary(string collection, string laestab)
    {
        var sql = $"{MIN_CORE_QUERY} AND o.OrganisationNativeID = @laestab";
        await using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<CensusSummary>(
            sql,
            new { Collection = collection, Laestab = laestab }
        );

        return result;
    }

    public async Task<List<MinCensusState>> GetMinCollectStatus(string collection)
    {
        await using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<MinCensusState>(
            MIN_CORE_QUERY,
            new { Collection = collection }
        );

        return result.ToList();
    }

    public async Task<CensusStatus?> GetLedgerStatus(int dcid, string laestab)
    {
        string sql = @"  
        SELECT [SchoolName]
              ,[LAEStab]
              ,[ReturnStatusCode]
              ,LAG([ReturnStatusCode]) OVER (ORDER BY [UpdatedAt] ASC) AS [PreviousReturnStatusCode]
              ,[Errors]
              ,[Queries]
              ,[OkdErrorsQueries]
              ,[Hash]
              ,[UpdatedAt]
              ,[Collection]
              ,[DCID]
          FROM [CollectStateLedger].[dbo].[CollectReturnStatus]
        where LAEStab = @LAEStab and DCID = @DCID
        order by UpdatedAt desc
";
        await using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryFirstOrDefaultAsync<CensusStatus>(
            sql,
            new { DcId = dcid, Laestab = laestab }
        );

        return result;
    }

    public async Task<List<MinCensusState>> GetMinLedgerStatus(string collection)
    {
        string sql = @"  
WITH RankedReturns AS (
    SELECT [SchoolName]
          ,[LAEStab]
          ,[Errors]
          ,[Queries]
          ,[OkdErrorsQueries]
          ,[ReturnStatusCode]
          ,[Collection]
          ,[DCID]
          ,[UpdatedAt]
          ,ROW_NUMBER() OVER (
               PARTITION BY [LAEStab], [DCID]
               ORDER BY [UpdatedAt] DESC
           ) AS RowNum
      FROM [CollectStateLedger].[dbo].[CollectReturnStatus]
     WHERE Collection = @Collection
)
SELECT [SchoolName]
      ,[LAEStab]
      ,[Errors]
      ,[Queries]
      ,[OkdErrorsQueries]
      ,[ReturnStatusCode]
      ,[Collection]
      ,[DCID]
      ,[UpdatedAt]
  FROM RankedReturns
 WHERE RowNum = 1
 ORDER BY [UpdatedAt] DESC;
";
        await using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<MinCensusState>(
            sql,
            new { Collection = collection }
        );

        return result.ToList();
    }

    public async Task ClearDownLedger()
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = "delete from [CollectStateLedger].[dbo].[CollectReturnStatus]";
        await connection.ExecuteAsync(sql);
    }

    public async Task<int> GetLedgerCount()
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = "select count(*) from [CollectStateLedger].[dbo].[CollectReturnStatus]";
        var result = await connection.QueryFirstOrDefaultAsync<int>(sql);
        return result;
    }

    public async Task<int> UpdateCensusLedger(string censusName, int daysSubtract)
    {
        await using var connection = new SqlConnection(_connectionString);
        var storedProcedureName = "[CollectStateLedger].[dbo].[AddChangedCollectReturnStatus]";
        var values = new { CensusName = censusName, DaysSubtract = daysSubtract };
        var result =
            await connection.ExecuteAsync(storedProcedureName, values, commandType: CommandType.StoredProcedure);
        return result;
    }

    public async Task SimulateDataUpdate(string censusName, string laestab, int errors, int queries, int okdErrors,
        int status)
    {
        await using var connection = new SqlConnection(_connectionString);
        string updateSql = @"
UPDATE dr
SET DRStatus = @Status, 
  HighErrors = @HighErrors,
  LowErrors = @LowErrors,
  OKErrors = @OKErrors
FROM COLLECTPortal.dbo.DataReturn dr
            INNER JOIN COLLECTPortal.dbo.OrganisationRole orol
                        ON dr.SourceOrganisationRoleID = orol.OrganisationRoleID
            INNER JOIN COLLECTPortal.dbo.Organisation o
                        ON orol.OrganisationID = o.OrganisationID
            INNER JOIN CollectPortal.dbo.OrganisationRole orol2
                        ON dr.AgentOrganisationRoleID = orol2.OrganisationRoleID
            INNER JOIN COLLECTPortal.dbo.Organisation o2
                        ON orol2.OrganisationID = o2.OrganisationID
			INNER JOIN COLLECTPortal.dbo.DataCollection dc
			ON dc.DCID = dr.DCID

  WHERE
                         o.OrganisationNativeID = @Laestab
						 and dc.DCBladeSQLDatabase = @CensusName

";

        var values = new
        {
            CensusName = censusName, Laestab = laestab, HighErrors = errors, LowErrors = queries, OKErrors = okdErrors,
            Status = status
        };
        await connection.ExecuteAsync(updateSql, values);
    }

    public async Task<int> InsertCollectReturnStatuses(List<MinCensusState> states)
    {
        // this could be quicker using https://dapper-plus.net/bulk-insert but is another library and more config
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO [CollectStateLedger].[dbo].[CollectReturnStatus] 
       (
       SchoolName,
      LAEStab,
      ReturnStatusCode,
      Errors,
      Queries,
      OkdErrorsQueries,
      Hash,
      UpdatedAt,
      Collection,
      DCID
      ) 
    VALUES (
        @SchoolName,
        @LAEStab,
        @ReturnStatusCode,
        @Errors,
        @Queries,
        @OkdErrorsQueries,
        'hash',
        GETDATE(),
        @Collection,
        @DCID
    )
";
        return await connection.ExecuteAsync(sql, states);
    }
}