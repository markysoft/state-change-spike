namespace StateChangeSimulation;

public class ReturnStatusComparer : IEqualityComparer<MinCensusState>
{
    public bool Equals(MinCensusState? x, MinCensusState? y) =>
        x?.Laestab == y?.Laestab
        && x?.Errors == y?.Errors
        && x?.Queries == y?.Queries
        && x?.OkdErrorsQueries == y?.OkdErrorsQueries
        && x?.ReturnStatusCode == y?.ReturnStatusCode
        && x?.DcId == y?.DcId;

    public int GetHashCode(MinCensusState obj) =>
        HashCode.Combine(obj.Laestab, obj.Errors, obj?.Queries, obj?.OkdErrorsQueries, obj?.ReturnStatusCode, obj?.DcId);
}

public class StateComparator(IDataManager datamanager, string collection)
{
    private readonly ReturnStatusComparer _comparer = new();

    public async Task<List<MinCensusState>> BuildComparison()
    {
        var latestRows = await datamanager.GetMinCollectStatus(collection);
        var previousRows = await datamanager.GetMinLedgerStatus(collection);
        var changes = latestRows.Except(previousRows, _comparer);
        return changes.ToList();
    }
}