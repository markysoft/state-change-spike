using StateChangeSimulation;
using StatusChangeSimulation.Test.obj;

namespace StatusChangeSimulation.Test;

public class StateComparatorTests
{
    private readonly DataManager _datamanager = new();
    private const string SchoolCensusSpring = "SchoolCensus2025_Spring";
    private const int SchoolCensusSpringDcId = 1172;

    [Fact]
    public async Task StateComparatorTest()
    {
        var mockDataManager = new MockDataManager();
        mockDataManager.PreviousStates = new List<MinCensusState>
        {
            new()
            {
                SchoolName = "School A",
                Laestab = "8503023",
                Collection = SchoolCensusSpring,
                DcId = SchoolCensusSpringDcId,
                Errors = 1,
                Queries = 2,
                OkdErrorsQueries = 3,
                ReturnStatusCode = 7
            },
            new()
            {
                SchoolName = "School B",
                Laestab = "8153008",
                Collection = SchoolCensusSpring,
                DcId = SchoolCensusSpringDcId,
                Errors = 3,
                Queries = 4,
                OkdErrorsQueries = 5,
                ReturnStatusCode = 7
            }
        };
        mockDataManager.CurrentStates = new List<MinCensusState>
        {
            new()
            {
                SchoolName = "School A",
                Laestab = "8503023",
                Collection = SchoolCensusSpring,
                DcId = SchoolCensusSpringDcId,
                Errors = 1,
                Queries = 2,
                OkdErrorsQueries = 3,
                ReturnStatusCode = 7
            },
            new()
            {
                SchoolName = "School B",
                Laestab = "8153008",
                Collection = SchoolCensusSpring,
                DcId = SchoolCensusSpringDcId,
                Errors = 0,
                Queries = 4,
                OkdErrorsQueries = 6,
                ReturnStatusCode = 8
            }
        };
        var stateComparator = new StateComparator(mockDataManager, SchoolCensusSpring);
        var updates = await stateComparator.BuildComparison();
        Assert.NotNull(updates);
        Assert.Single(updates);
        var updated = updates.First();
        Assert.Equal(SchoolCensusSpringDcId, updated.DcId);
        Assert.Equal(0, updated.Errors);
        Assert.Equal(4, updated.Queries);
        Assert.Equal(6, updated.OkdErrorsQueries);
        Assert.Equal(8, updated.ReturnStatusCode);
    }

    [Fact]
    public async Task SimulateDataChange()
    {
        var stateComparator = new StateComparator(_datamanager, SchoolCensusSpring);
        // baseline existing state rows
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8503023", 1, 2, 3, 7);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8153008", 3, 4, 5, 7);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8502389", 4, 5, 6, 7);

        await _datamanager.ClearDownLedger();
        var firstPass = await stateComparator.BuildComparison();
        Assert.Equal(21873, firstPass.Count);
        await _datamanager.AddNewStates(firstPass);

        // simulate a change for three schools
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8503023", 10, 11, 12, 8);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8153008", 11, 12, 13, 8);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8502389", 12, 13, 14, 8);

        var secondPass = await stateComparator.BuildComparison();
        Assert.Equal(3, secondPass.Count);
        await _datamanager.AddNewStates(secondPass);

        var result1 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8503023");
        Assert.NotNull(result1);
        Assert.Equal(10, result1.Errors);
        Assert.Equal(11, result1.Queries);
        Assert.Equal(12, result1.OkdErrorsQueries);
        Assert.Equal(8, result1.ReturnStatusCode);
        Assert.Equal(7, result1.PreviousReturnStatusCode);

        var result2 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8153008");
        Assert.NotNull(result2);
        Assert.Equal(11, result2.Errors);
        Assert.Equal(12, result2.Queries);
        Assert.Equal(13, result2.OkdErrorsQueries);
        Assert.Equal(8, result2.ReturnStatusCode);
        Assert.Equal(7, result2.PreviousReturnStatusCode);

        var result3 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8502389");
        Assert.NotNull(result3);
        Assert.Equal(12, result3.Errors);
        Assert.Equal(13, result3.Queries);
        Assert.Equal(14, result3.OkdErrorsQueries);
        Assert.Equal(8, result3.ReturnStatusCode);
        Assert.Equal(7, result3.PreviousReturnStatusCode);

        // set back to original to ensure we are tracking changes, not unique rows

        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8503023", 1, 2, 3, 7);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8153008", 3, 4, 5, 7);
        await _datamanager.SimulateDataUpdate(SchoolCensusSpring, "8502389", 4, 5, 6, 7);
        
        var thirdPass = await stateComparator.BuildComparison();
        Assert.Equal(3, thirdPass.Count);
        await _datamanager.AddNewStates(thirdPass);

        var result4 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8503023");
        Assert.NotNull(result4);
        Assert.Equal(1, result4.Errors);
        Assert.Equal(2, result4.Queries);
        Assert.Equal(3, result4.OkdErrorsQueries);
        Assert.Equal(7, result4.ReturnStatusCode);
        Assert.Equal(8, result4.PreviousReturnStatusCode);

        var result5 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8153008");
        Assert.NotNull(result5);
        Assert.Equal(3, result5.Errors);
        Assert.Equal(4, result5.Queries);
        Assert.Equal(5, result5.OkdErrorsQueries);
        Assert.Equal(7, result5.ReturnStatusCode);
        Assert.Equal(8, result5.PreviousReturnStatusCode);

        var result6 = await _datamanager.GetCensusStatus(SchoolCensusSpringDcId, "8502389");
        Assert.NotNull(result6);
        Assert.Equal(4, result6.Errors);
        Assert.Equal(5, result6.Queries);
        Assert.Equal(6, result6.OkdErrorsQueries);
        Assert.Equal(7, result6.ReturnStatusCode);
        Assert.Equal(8, result6.PreviousReturnStatusCode);
    }
}