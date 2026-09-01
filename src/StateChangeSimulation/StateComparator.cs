namespace StateChangeSimulation;

public class StateComparator
{
    DataManager _datamanager = new DataManager();
    public async Task BuildComparison()
    {
    var latestRows = await _datamanager.GetCensusSummary("SchoolCensus2025_Spring");
    }
}