using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StateChangeSimulation;

public class CensusSummary
{
    [Description("Return status code")]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int ReturnStatusCode { get; set; }

    [Description("Collection name")]
    public string Collection { get; set; } = string.Empty;

    [Description("Local Authority Establishment number")]
    public string Laestab { get; set; } = string.Empty;

    [Description("School name")]
    public string SchoolName { get; set; } = string.Empty;

    [Description("Number of errors")]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int Errors { get; set; }

    [Description("Number of queries")]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int Queries { get; set; }

    [Description("Number of OK'd errors/queries")]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int OkdErrorsQueries { get; set; }

    [Description("Date census was submitted")]
    public DateTime? SubmittedDate { get; set; }

    [Description("Date census was approved")]
    public DateTime? ApprovedDate { get; set; }

    [Description("Date census was authorised")]
    public DateTime? AuthorisedDate { get; set; }
}