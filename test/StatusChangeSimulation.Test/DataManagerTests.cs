using StateChangeSimulation;

namespace StatusChangeSimulation.Test;

public class DataManagerTests
{
    private readonly DataManager _datamanager = new();
    private const string SchoolcensusSpring = "SchoolCensus2025_Spring";

    [Fact]
    public async Task ShouldReturnStatus()
    {
        var results = await _datamanager.GetCensusSummary(SchoolcensusSpring, "3712195");
        Assert.NotNull(results);
    }

    [Fact]
    public async Task ShouldGetCount()
    {
        var count = await _datamanager.GetLedgerCount();
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task ShouldClearDownLedger()
    {
        await _datamanager.ClearDownLedger();
        var count = await _datamanager.GetLedgerCount();
        Assert.True(count == 0);
    }

    [Fact]
    public async Task ShouldUpdateCensusLedger()
    {
        var updated = await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 7);
        var count = await _datamanager.GetLedgerCount();
        Assert.True(count >= 0);
    }
}