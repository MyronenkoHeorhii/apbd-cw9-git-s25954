using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.DTO;
using UniversityTasksDbFirstApi.Models;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    readonly UniversityTasksDbContext _context;
    
    public SubmissionsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission([FromBody] CreateSubmissionDto createSubmissionDto)
    {
        var studentExists = await _context.Students
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.StudentId == createSubmissionDto.StudentId);

        var assignmentExists = await _context.Assignments
            .Where(a => a.AssignmentId == createSubmissionDto.AssignmentId)
            .FirstOrDefaultAsync();

        if (studentExists == null)
        {
            return NotFound("Student not found");
        }

        if (!studentExists.IsActive)
        {
            return BadRequest("Student is not active");
        }

        if (assignmentExists == null)
        {
            return NotFound();
        }

        if (!assignmentExists.IsPublished)
        {
            return BadRequest("Assignment not published");
        }
        
        
        var isEnrolled = studentExists.Enrollments
            .Any(e =>
                e.CourseId == assignmentExists.CourseId &&
                (e.Status == "Active" || e.Status == "Completed"));

        if (!isEnrolled)
            return BadRequest("Student is not enrolled in the course or enrollment is not active/completed");

        var submissionExists = await _context.Submissions
            .FirstOrDefaultAsync(s =>
                s.StudentId == studentExists.StudentId && s.AssignmentId == assignmentExists.AssignmentId);

        if (submissionExists != null)
        {
            return BadRequest("This submission already exists");
        }

        string status;

        if (DateTime.UtcNow > assignmentExists.DueDate)
        {
            status = "Late";
        }
        else
        {
            status = "Submitted";
        }
        
        if (string.IsNullOrWhiteSpace(createSubmissionDto.RepositoryUrl) ||
            !createSubmissionDto.RepositoryUrl.StartsWith("https://"))
        {
            return BadRequest("RepositoryUrl must be non-empty and start with https://");
        }
        
        
        var submission = new Submission
        {
            StudentId = createSubmissionDto.StudentId,
            AssignmentId = createSubmissionDto.AssignmentId,
            RepositoryUrl = createSubmissionDto.RepositoryUrl,
            SubmittedAt = DateTime.UtcNow,
            Status = status
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return Created();
    }

    [HttpPut("{idSubmission}/grade")]
    public async Task<ActionResult> GradeSubmission([FromRoute] int idSubmission,
        [FromBody] GradeSubmissionDto gradeSubmissionDto)
    {
        var submissionExists = await _context.Submissions
            .Include(s=>s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);

        if (submissionExists == null)
        {
            return NotFound("Submission not found");
        }

        if (gradeSubmissionDto.Score < 0)
        {
            return BadRequest("Score cannot be negative");
        }

        if (submissionExists.Assignment.MaxPoints < gradeSubmissionDto.Score)
        {
            return BadRequest("Score cannot be bigger than the Assignment's MaxPoints");
        }

        //string status = "Graded";

        submissionExists.Score = gradeSubmissionDto.Score;
        submissionExists.Feedback = gradeSubmissionDto.Feedback;
        submissionExists.Status = "Graded";
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{idSubmission}")]
    public async Task<ActionResult> DeleteSubmission([FromRoute] int idSubmission)
    {
        var submissionExists = await _context.Submissions
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);

        if (submissionExists == null)
        {
            return NotFound("Submission not found");
        }

        if (submissionExists.Status.Equals("Graded"))
        {
            return BadRequest("Submission is already Graded");
        }

        _context.Submissions.Remove(submissionExists);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}