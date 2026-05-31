using UniversityTasksDbFirstApi.Models;

namespace UniversityTasksDbFirstApi.DTO;

public class StudentDashboardDto
{
    public int StudentId { get; set; }
    public string IndexNumber { get; set; }
    public string FullName { get; set; }
    
    public bool ActiveStatus{get;set;}
    
    
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    
}