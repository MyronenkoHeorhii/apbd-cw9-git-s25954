namespace UniversityTasksDbFirstApi.DTO;

public class SubmissionDto
{
    public int SubmissionId { get; set; }
    public int Student { get; set; }
    public int Assignment { get; set; }
    public string RepositoryUrl { get; set; }
    public string Status {get;set;}
    public int? Score { get; set; }
    public string? Feedback {get;set;}
}