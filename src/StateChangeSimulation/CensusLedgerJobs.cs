namespace StateChangeSimulation;

public static class CensusLedgerJobs
{
    public static async Task RunUpdateCensusLedger()
    {
        DataManager dm = new DataManager();
        try
        {
            Console.WriteLine("Updating Census Ledger...");
            
            var rowBefore = await dm.GetLedgerCount();
            await dm.UpdateCensusLedger("SchoolCensus2025_Spring", 0);
            var rowsAfter = await dm.GetLedgerCount();
            Console.WriteLine($"Update complete, added ledger rows {rowsAfter - rowBefore}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}