namespace StateChangeSimulation;

public class CensusStatus
{

    public string SchoolName { get; set; } = string.Empty;
    
    public string Laestab { get; set; } = string.Empty;
    
    public int ReturnStatus { get; set; }
    
    public int? PreviousReturnStatus { get; set; }
    
    public int Errors { get; set; }

    public int Queries { get; set; }

    public int OkdErrorsQueries { get; set; }

    public string Hash { get; set; } = string.Empty;
    
    public DateTime UpdatedAt { get; set; }
    
    public int DcId { get; set; }

}