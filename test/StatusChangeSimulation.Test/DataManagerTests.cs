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
    
    [Fact]
    public async Task ShouldSimulateDataUpdate()
    {
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "3712195", 200, 30, 400, 7);
       
        var results = await _datamanager.GetCensusSummary(SchoolcensusSpring, "3712195");
        Assert.NotNull(results);
    }

    [Fact]
    public async Task SimulateDataChange()
    {
        // baseline existing state rows
        
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8503023", 1, 2, 3, 7);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8153008", 3, 4, 5, 7);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8502389", 4, 5, 6, 7);
        
        await _datamanager.ClearDownLedger();
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 7);
        var originalCount = await _datamanager.GetLedgerCount();
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8503023", 10, 11, 12, 8);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8153008", 11, 12, 13, 8);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8502389", 12, 13, 14, 8);
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 0);
        var updatedCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, updatedCount - originalCount);
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 0);
        updatedCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, updatedCount - originalCount);
    }
}