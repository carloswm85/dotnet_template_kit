using System.Diagnostics;
using Asp.Versioning;
using NLayerTemplate.Service.Dtos.ContosoUniversity;
using NLayerTemplate.Service.Services.ExampleServices.Interfaces;
using NLayerTemplate.Web.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NLayerTemplate.Web.API.Controllers.ExampleControllers;

[ApiController]
[AllowAnonymous]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContosoUniversityController : ControllerBase
{
    private readonly IContosoUniversityService _contosoService;

    public ContosoUniversityController(IContosoUniversityService contosoService)
    {
        _contosoService = contosoService;
    }

    #region TestStudents

    //
    // ✅ GET: api/students
    //
    /// <summary>
    /// Retrieves the list of students. ⚠️ CACHED response example (60 seconds, with constant settings).
    /// </summary>
    /// <remarks>
    /// Response is cached on the client for 60 seconds.
    /// </remarks>
    /// <returns>
    /// Returns a <see cref="StatusCodes.Status200OK"/> response containing the list of students
    /// or <see cref="StatusCodes.Status403Forbidden"/> if the request is not authorized.
    /// </returns>
    /// <response code="200">TestStudent list retrieved successfully.</response>
    /// <response code="403">Access forbidden.</response>
    [HttpGet("students")]
    [MapToApiVersion("1.0")]
    //[ResponseCache(Duration = 60)]
    //[ResponseCache(CacheProfileName = "Default60Sec")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default60Sec)]
    [ProducesResponseType(typeof(IEnumerable<TestStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Obsolete("This medhos is obsolete. Use \"students\" endpoint from 'v2'")]
    public async Task<ActionResult> GetTestStudents()
    {
        Debug.WriteLine($"Getting students... Time: {DateTime.Now}");
        var students = await _contosoService.GetTestStudentListAsync(HttpContext.RequestAborted);
        Debug.WriteLine($"Sending students, with caching.");
        return Ok(students);
    }

    //
    // ✅ GET: api/students
    //
    /// <summary>
    /// Retrieves the list of students. ⚠️ CACHED response example (60 seconds, with constant settings).
    /// </summary>
    /// <remarks>
    /// Response is cached on the client for 60 seconds.
    /// </remarks>
    /// <returns>
    /// Returns a <see cref="StatusCodes.Status200OK"/> response containing the list of students
    /// or <see cref="StatusCodes.Status403Forbidden"/> if the request is not authorized.
    /// </returns>
    /// <response code="200">TestStudent list retrieved successfully.</response>
    /// <response code="403">Access forbidden.</response>
    [HttpGet("students")]
    [AllowAnonymous] // ← Add this explicitly
    [MapToApiVersion("2.0")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default60Sec)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetTestStudentsOrderedByDescendingId()
    {
        Debug.WriteLine($"Getting students... Time: {DateTime.Now}");
        var students = await _contosoService.GetTestStudentListAsync(HttpContext.RequestAborted);
        students = students.OrderByDescending(s => s.Id).ToList();
        Debug.WriteLine($"Sending students, with caching.");
        return Ok(students);
    }

    //
    // ✅ GET: api/students/5
    //
    [HttpGet("students/{studentId:int}", Name = "GetTestStudent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTestStudent(int studentId)
    {
        var studentDto = await _contosoService.GetTestStudentAsync(
            studentId,
            cancellationToken: HttpContext.RequestAborted
        );

        if (studentDto == null)
            return NotFound($"TestStudent with id {studentId} was not found");

        return Ok(studentDto);
    }

    //
    // ✅ POST: api/students
    //
    [HttpPost("students")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateTestStudent([FromForm] TestStudentDto studentDto)
    {
        if (studentDto == null)
            return BadRequest(ModelState);

        if (_contosoService.TestStudentExists(studentDto.GovernmentId))
        {
            ModelState.AddModelError("CustomError", "TestStudent already exists");
            return BadRequest(ModelState);
        }

        studentDto = await UploadTestStudentImage(studentDto);

        var studentIdResult = await _contosoService.CreateTestStudentAsync(
            studentDto,
            HttpContext.RequestAborted
        );
        if (studentIdResult == 0)
        {
            ModelState.AddModelError(
                "CustomError",
                $"Something went wrong when trying to save the student {studentDto.FirstMidName} {studentDto.LastName}"
            );
            return StatusCode(500, ModelState);
        }

        var createdTestStudent = await _contosoService.GetTestStudentAsync(
            studentIdResult,
            cancellationToken: HttpContext.RequestAborted
        );
        return CreatedAtRoute("GetTestStudent", new { studentId = createdTestStudent!.Id }, createdTestStudent);
    }

    //
    // ✅ PUT: api/students
    //
    [HttpPut("students/{studentId:int}", Name = "UpdateTestStudent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateTestStudent(int studentId, [FromForm] TestStudentDto studentDto)
    {
        if (studentDto == null)
            return BadRequest(ModelState);

        if (!_contosoService.TestStudentExists(studentDto.GovernmentId))
        {
            ModelState.AddModelError(
                "CustomError",
                $"TestStudent with government id {studentDto.GovernmentId} does not exist"
            );
            return BadRequest(ModelState);
        }

        studentDto = await UploadTestStudentImage(studentDto);

        if (
            !await _contosoService.UpdateTestStudentAsync(
                studentId,
                studentDto,
                HttpContext.RequestAborted
            )
        )
        {
            ModelState.AddModelError(
                "CustomError",
                $"Something went wrong when trying to save the student {studentDto.FirstMidName} {studentDto.LastName}"
            );
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }

    //
    // ✅ DELETE: api/students/5
    //
    [HttpDelete("students/{studentId:int}", Name = "DeleteTestStudent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTestStudent(int studentId)
    {
        if (!await _contosoService.DeleteTestStudentAsync(studentId, HttpContext.RequestAborted))
        {
            ModelState.AddModelError(
                "CustomError",
                $"Something went wrong when trying to eliminate the student with id {studentId}"
            );
            return StatusCode(500, ModelState);
        }
        return NoContent();
    }

    #endregion

    private async Task<TestStudentDto> UploadTestStudentImage(TestStudentDto studentDto)
    {
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}";

        if (studentDto.Image != null)
        {
            var folder = "students";
            var imageFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images",
                folder
            );

            string fileName =
                studentDto.Id
                + Guid.NewGuid().ToString()
                + Path.GetExtension(studentDto.Image.FileName);

            if (!Directory.Exists(imageFolder))
                Directory.CreateDirectory(imageFolder);

            var filePath = Path.Combine(imageFolder, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
                file.Delete();

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await studentDto.Image.CopyToAsync(fileStream, HttpContext.RequestAborted);

            studentDto.ImagePath = $"{baseUrl}/images/{folder}/{fileName}";
        }
        else
        {
            studentDto.ImagePath = DefaultImages.TestStudent;
        }

        return studentDto;
    }
}
