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
        //Console.WriteLine("im here");
        
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


    [HttpGet("{idCourse}/assignments")]
    public async Task<ActionResult<IEnumerable<AssignmentDto>>> AssignmentsForOneCourse(int idCourse,
        [FromQuery] bool publishedOnly = false)
    {
        
        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == idCourse);

        if (!courseExists)
        {
            return NotFound();
        }

        var query = _context.Assignments
            .AsNoTracking()
            .Where(a => a.CourseId == idCourse);

        if (publishedOnly)
        {
            query = query.Where(a => a.IsPublished);
        }

        var assignments = await query
            .Select(a => new AssignmentDto()
            {
                AssignmentId = a.AssignmentId,
                Title = a.Title,
                DueDate = a.DueDate,
                MaxPoints = a.MaxPoints,
                IsPublished = a.IsPublished,
                SubmissionCount = a.Submissions.Count()
            })
            .ToListAsync();
        
        Console.WriteLine(assignments);
        
        return Ok(assignments);
    }
}