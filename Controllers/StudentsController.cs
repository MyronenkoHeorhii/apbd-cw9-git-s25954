using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.Models;
using UniversityTasksDbFirstApi.DTO;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;
    
    public StudentsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet("{idStudent}/dashboard")]
    public async Task<ActionResult<StudentDashboardDto>> StudentDetails(int idStudent)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(s => s.StudentId == idStudent)
            .Select(s => new StudentDashboardDto
            {
                StudentId = s.StudentId,
                IndexNumber = s.IndexNumber,
                FullName = s.FirstName + " " + s.LastName,
                ActiveStatus = s.IsActive,

                Enrollments = s.Enrollments.ToList(),
                Submissions = s.Submissions.ToList()
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return NotFound();
        }

        return Ok(student);
    }
}