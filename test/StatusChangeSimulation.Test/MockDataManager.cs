using StateChangeSimulation;

namespace StatusChangeSimulation.Test;
public class MockDataManager: IDataManager
{
    public List<MinCensusState> PreviousStates { get; set; } = new List<MinCensusState>();
    public List<MinCensusState> CurrentStates { get; set; } = new List<MinCensusState>();
    
    public Task<List<MinCensusState>> GetMinCollectStatus(string collection)
    {
        return Task.FromResult(CurrentStates);
    }

    public Task<List<MinCensusState>> GetMinLedgerStatus(string collection)
    {
        return Task.FromResult(PreviousStates);
    }

    public Task ClearDownLedger()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetLedgerCount()
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateCensusLedger(string censusName, int daysSubtract)
    {
        throw new NotImplementedException();
    }

    public Task SimulateDataUpdate(string censusName, string laestab, int errors, int queries, int okdErrors, int status)
    {
        throw new NotImplementedException();
    }
}