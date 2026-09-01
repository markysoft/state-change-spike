using StateChangeSimulation;

namespace StatusChangeSimulation.Test;

public class DataManagerTests
{
    private readonly DataManager _datamanager = new();
    private const string SchoolcensusSpring = "SchoolCensus2025_Spring";
    private const int SchoolCensusSpringDcId = 1172;

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
        
        // simulate a change for three schools
        var originalCount = await _datamanager.GetLedgerCount();
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8503023", 10, 11, 12, 8);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8153008", 11, 12, 13, 8);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8502389", 12, 13, 14, 8);
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 6);
        var updatedCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, updatedCount - originalCount);
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 6);
        updatedCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, updatedCount - originalCount);
        
        var result1 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8503023");
        Assert.NotNull(result1);
        Assert.Equal(10, result1.Errors);
        Assert.Equal(11, result1.Queries);
        Assert.Equal(12, result1.OkdErrorQueries);
        Assert.Equal(8, result1.ReturnStatus);
        
        var result2 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8153008");
        Assert.NotNull(result2);
        Assert.Equal(11, result2.Errors);
        Assert.Equal(12, result2.Queries);
        Assert.Equal(13, result2.OkdErrorQueries);
        Assert.Equal(8, result2.ReturnStatus);
        
        var result3 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8502389");
        Assert.NotNull(result3);
        Assert.Equal(12, result3.Errors);
        Assert.Equal(13, result3.Queries);
        Assert.Equal(14, result3.OkdErrorQueries);
        Assert.Equal(8, result3.ReturnStatus);
        
        // set back to original to ensure we are tracking changes, not unique rows
        
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8503023", 1, 2, 3, 7);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8153008", 3, 4, 5, 7);
        await _datamanager.SimulateDataUpdate(SchoolcensusSpring, "8502389", 4, 5, 6, 7);
        
        await _datamanager.UpdateCensusLedger(SchoolcensusSpring, 5);
        var finalCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, finalCount - updatedCount);
        // run again to prove it is idempotent
        finalCount = await _datamanager.GetLedgerCount();
        Assert.Equal(3, finalCount - updatedCount);
        
        var result4 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8503023");
        Assert.NotNull(result4);
        Assert.Equal(1, result4.Errors);
        Assert.Equal(2, result4.Queries);
        Assert.Equal(3, result4.OkdErrorQueries);
        Assert.Equal(7, result4.ReturnStatus);
        
        var result5 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8153008");
        Assert.NotNull(result5);
        Assert.Equal(3, result5.Errors);
        Assert.Equal(4, result5.Queries);
        Assert.Equal(5, result5.OkdErrorQueries);
        Assert.Equal(7, result5.ReturnStatus);
        
        var result6 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8502389");
        Assert.NotNull(result6);
        Assert.Equal(4, result6.Errors);
        Assert.Equal(5, result6.Queries);
        Assert.Equal(6, result6.OkdErrorQueries);
        Assert.Equal(7, result6.ReturnStatus);        
    }
}