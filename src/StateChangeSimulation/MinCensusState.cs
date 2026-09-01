namespace StateChangeSimulation;

public class MinCensusState
{
    public string SchoolName { get; set; } = string.Empty;
    
    public string Laestab { get; set; } = string.Empty;
    
    public string Collection { get; set; } = string.Empty;
    
    public int DcId { get; set; }
    
    public int Errors { get; set; }

    public int Queries { get; set; }

    public int OkdErrorsQueries { get; set; }
    
    public int ReturnStatusCode { get; set; }
}