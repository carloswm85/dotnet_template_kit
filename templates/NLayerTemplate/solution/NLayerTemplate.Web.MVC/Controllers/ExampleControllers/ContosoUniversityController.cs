using NLayerTemplate.Data.Model;
using NLayerTemplate.Service.Dtos.ContosoUniversity;
using NLayerTemplate.Service.Models;
using NLayerTemplate.Service.Services.ExampleServices.Interfaces;
using NLayerTemplate.Web.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NLayerTemplate.Web.MVC.Controllers.ExampleControllers;

[Route("ContosoUniversity")]
public class ContosoUniversityController : Controller
{
    private readonly ILogger<ContosoUniversityController> _logger;
    private readonly IContosoUniversityService _contosoService;

    public ContosoUniversityController(
        IContosoUniversityService contosoService,
        ILogger<ContosoUniversityController> logger
    )
    {
        _logger = logger;
        _contosoService = contosoService;
    }

    #region TestStudents

    // GET: /ContosoUniversity/List
    [HttpGet("List")]
    public async Task<IActionResult> Index(
        string currentFilter = "",
        int pageIndex = 1,
        int pageSize = 10,
        string searchString = "",
        string sortOrder = ""
    )
    {
        try
        {
            var students = await _contosoService.GetTestStudentsPaginatedListAsync(
                currentFilter,
                pageIndex,
                pageSize,
                searchString,
                sortOrder,
                HttpContext.RequestAborted
            );

            var paginatedListViewModel = new PaginatedListViewModel<TestStudentDto>(
                paginatedList: students,
                currentFilter: searchString,
                currentSort: sortOrder,
                sortColumnOne: string.IsNullOrEmpty(sortOrder)
                    ? CurrentSort.LastNameDesc
                    : CurrentSort.LastNameAsc,
                sortColumnTwo: sortOrder == CurrentSort.DateAsc
                    ? CurrentSort.DateDesc
                    : CurrentSort.DateAsc,
                pageSize: pageSize
            );

            return View(paginatedListViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the student list.");
            throw;
        }
    }

    // GET: /ContosoUniversity/Details/5
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _contosoService.GetTestStudentAsync(
            id.Value,
            cancellationToken: HttpContext.RequestAborted
        );

        if (student == null)
            return NotFound();

        return View(student);
    }

    // GET: /ContosoUniversity/Create
    [AllowAnonymous]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /ContosoUniversity/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    // In this method: Use entity classes with model binding instead of view models.
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] TestStudentDto studentDto)
    {
        if (!ModelState.IsValid)
            return View(studentDto);

        try
        {
            studentDto = await UploadImage(studentDto, "students");

            await _contosoService.CreateTestStudentAsync(
                studentDto,
                HttpContext.RequestAborted
            );
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new student.");
            ModelState.AddModelError(
                "",
                "Unable to save changes. "
                    + "Try again, and if the problem persists "
                    + "see your system administrator."
            );
            return View(studentDto);
        }
    }

    // GET: /ContosoUniversity/Edit/5
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _contosoService.GetTestStudentAsync(
            id.Value,
            cancellationToken: HttpContext.RequestAborted
        );

        if (student == null)
            return NotFound();

        return View(student);
    }

    // To protect from overposting attacks, enable the specific properties
    // you want to bind to.
    // POST: /ContosoUniversity/Edit/5
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int Id, [FromForm] TestStudentDto studentDto)
    {
        if (Id != studentDto.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(studentDto);

        try
        {
            if (studentDto.Image != null)
                studentDto = await UploadImage(studentDto, "students");

            await _contosoService.UpdateTestStudentAsync(
                Id,
                studentDto,
                HttpContext.RequestAborted
            );
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!_contosoService.TestStudentExists(studentDto.Id))
                return NotFound();

            _logger.LogWarning(
                ex,
                "Concurrency conflict while updating student ID {TestStudentId}.",
                studentDto.Id
            );
            ModelState.AddModelError(
                "",
                "The record you attempted to edit was modified by another user."
            );
            return View(studentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while editing the student with ID {TestStudentId}.",
                studentDto.Id
            );
            ModelState.AddModelError(
                "",
                "Unable to save changes. Try again, and if the problem persists, contact your system administrator."
            );
            return View(studentDto);
        }
    }

    // GET: /ContosoUniversity/Delete/5
    // Display student before deletion.
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
    {
        if (id == null)
            return NotFound();

        var student = await _contosoService.GetTestStudentAsync(
            studentId: id.Value,
            asNoTracking: true,
            cancellationToken: HttpContext.RequestAborted
        );

        if (student == null)
            return NotFound();

        if (saveChangesError.GetValueOrDefault())
        {
            ViewData["ErrorMessage"] =
                "Delete failed. Try again, and if the problem persists "
                + "see your system administrator.";
        }

        return View(student);
    }

    // POST: /ContosoUniversity/Delete/5
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _contosoService.GetTestStudentAsync(
            studentId: id,
            asNoTracking: true,
            cancellationToken: HttpContext.RequestAborted
        );

        if (student == null)
            return RedirectToAction(nameof(Index));

        try
        {
            await _contosoService.DeleteTestStudentAsync(id, HttpContext.RequestAborted);
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while deleting the student with ID {TestStudentId}.",
                id
            );
            return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
        }
    }

    // ADIITONAL METHODS CAN GO HERE

    /*
     *  | Method    | Scope                | Request Body        | Example Use Case                 |
     *  | --------- | -------------------- | ------------------- | -------------------------------- |
     *  | **PUT**   | Full replacement     | Full entity         | Replace an entire student record |
     *  | **PATCH** | Partial modification | JSON Patch document | Update one or more fields only   |
     */

    // THIS IS JUST AN EXAMPLE, unused
    // PUT: /ContosoUniversity/TestStudent/5
    [HttpPut("TestStudent/{id}")]
    public async Task<IActionResult> UpdateTestStudent(int id, [FromBody] TestStudent student)
    {
        if (id != student.TestStudentId)
            return BadRequest("TestStudent ID mismatch.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var studentDto = await _contosoService.GetTestStudentAsync(
                id,
                cancellationToken: HttpContext.RequestAborted
            );
            if (studentDto == null)
                return NotFound();

            // Replace all properties (full update)
            studentDto.FirstMidName = student.FirstMidName;
            studentDto.LastName = student.LastName;
            studentDto.TestEnrollmentDate = student.TestEnrollmentDate;

            studentDto = await UploadImage(studentDto, "students");

            await _contosoService.CreateTestStudentAsync(
                studentDto,
                HttpContext.RequestAborted
            );
            return NoContent(); // 204 - successful update
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating student.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating data.");
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> About()
    {
        var studentData = await _contosoService.GetTestEnrollmentDateDataAsync(
            HttpContext.RequestAborted
        );
        return View(studentData);
    }

    #endregion

    [HttpGet("AdditionalInstructions")]
    public IActionResult ContosoUniversityInstructions()
    {
        var instructions =
            @"
                == INSTRUCTIONS FOR THE CONTOSO UNIVERSITY CONTENT ==

                Link to tutorial: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/?view=aspnetcore-8.0

                - The tutorial is not completed in this example content on purpose.
                - Sections COMPLETED are the following:
                    1. Get started with EF Core in an ASP.NET MVC web app
                    2. Implement CRUD Functionality
                    3. Add sorting, filtering, and paging
                    4. Apply migrations to the Contoso University sample
                    5. Create a complex data model
                - The following sections are NOT COMPLETED, the developer should complete them as an exercise:
                    6. Read related data
                    7. Update related data
                    8. Handle concurrency
                    9. Implement inheritance
                    10. Learn about advanced scenarios
                    
                == SUGGESTED STEPS ==
                
                - General:
                    • From point (5) all required models were included (in the code base and the database)
                    • Review the model relationships, fix if necessary
                    • Implement complete CRUD for all entities, only `TestStudent` entity was completed
                    • Instructions on how to do that are on points (6) to (10) from tutorial
                - What you can do and where (these are just general guidelines, not all steps to take):
                    A. Data Layer:
                        • Separate database annotations (used for EF) from view annotations (used in MVC)
                    B. Repository Layer:
                        • Include missing repositories
                    C. Service Layer:
                        • Generate missing DTOs and mapping profiles
                        • Add interfaces and services, you can use the TestStudentService as example
                    D. API Layer:
                        • Generate missing endpoints
                    E. MVC Layer:
                        • Generate controllers, view models and views for full CRUD (and listing)
                            → Hint: VS (IDE) scaffolding engine can help to quicken this part of the development
                        • View annotations from Data Layer should be moved here to the view models
                        • For the sake of briefty, student view works directly with DTOs, you should change the code to work with view models
                
                == ADDITIONAL SUGGESTIONS ==
                
                - You can connect the API directly to MVC, instead of going directly from the Service layer to MVC
                - Implement all the endpoints from the API in a client application, such as a mobile project
                ";
        return Ok(instructions);
    }

    // REFACTOR
    private async Task<dynamic> UploadImage(dynamic imageContainer, string folderName)
    {
        if (imageContainer.Image != null)
        {
            var imageFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                folderName
            );

            // Ensure directory exists
            if (!Directory.Exists(imageFolder))
                Directory.CreateDirectory(imageFolder);

            // Generate filename
            string fileName =
                $"{imageContainer.Id}_{Guid.NewGuid()}{Path.GetExtension(imageContainer.Image.FileName)}";

            var filePath = Path.Combine(imageFolder, fileName);

            // Save file
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageContainer.Image.CopyToAsync(fileStream, HttpContext.RequestAborted);

            // ✅ Store ONLY relative path
            imageContainer.ImagePath = $"/images/{folderName}/{fileName}";
        }
        else
        {
            // ✅ Also relative
            imageContainer.ImagePath = $"/images/defaults/default-{folderName}.png";
        }

        return imageContainer;
    }
}
