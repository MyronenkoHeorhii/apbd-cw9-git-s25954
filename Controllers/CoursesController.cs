using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.Models;
using UniversityTasksDbFirstApi.DTO;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;
    
    public CoursesController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> CoursesWithAssignmentCounts([FromQuery] bool activeOnly = false)
    {
        Console.WriteLine("im here");
        
        var query = _context.Courses
            .AsNoTracking()
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var result = await query
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                Credits = c.Credits,
                AssignmentCount = c.Assignments.Count()
            })
            .ToListAsync();

        return Ok(result);
    }
    
}