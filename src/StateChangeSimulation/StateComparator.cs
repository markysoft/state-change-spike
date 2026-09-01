namespace StateChangeSimulation;

public class StateComparator
{
    private const string SchoolCensusSpring = "SchoolCensus2025_Spring";
    private const int SchoolCensusSpringDcId = 1172;
    DataManager _datamanager = new DataManager();
    public async Task BuildComparison()
    {
    var latestRows = await _datamanager.GetMinCollectStatus(SchoolCensusSpring);
    var previousRows = await _datamanager.GetMinLedgerStatus(SchoolCensusSpring);
    var firstLatest = latestRows.FirstOrDefault();
    var firstPrevious = previousRows.FirstOrDefault();
    }
}