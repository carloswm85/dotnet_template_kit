using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Entities;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;
using CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;
using CleanArchitectureTemplate.Infrastructure.Interfaces.IdentityInterfaces;
using CleanArchitectureTemplate.Web.Models.ViewModels.TestContact;

namespace CleanArchitectureTemplate.Web.Controllers.Identity;

[Authorize]
public class TestContactController : Controller
{
    private readonly IContosoUniversityService _contactService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IUserManagerService _userManagerService;

    public TestContactController(
        IContosoUniversityService contactService,
        IAuthorizationService authorizationService,
        ILoggerFactory loggerFactory,
        IMapper mapper,
        IUserManagerService userManagerService
    )
    {
        _contactService = contactService;
        _authorizationService = authorizationService;
        _logger = loggerFactory.CreateLogger<TestContactController>();
        _mapper = mapper;
        _userManagerService = userManagerService;
    }

    #region Listing/Index

    public async Task<IActionResult> Index()
    {
        var isAuthorized =
            User.IsInRole(RoleConstants.ManagersRole)
            || User.IsInRole(RoleConstants.AdministratorsRole);

        var currentUserId = _userManagerService.GetUserId(User);

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Challenge();
        }

        var contactList = await _contactService.GetTestContactsAsync(
            isAuthorized,
            currentUserId,
            HttpContext.RequestAborted
        );

        var listVm = new ContacListViewModel { TestContacts = contactList.ToList() };

        return View(listVm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var contact = await _contactService.GetByIdAsync(id,
            HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var isAuthorized =
            User.IsInRole(RoleConstants.ManagersRole)
            || User.IsInRole(RoleConstants.AdministratorsRole);

        var currentUserId = _userManagerService.GetUserId(User);

        if (
            !isAuthorized
            && currentUserId != contact.OwnerID
            && contact.Status != TestContactStatus.Approved
        )
        {
            return Forbid();
        }

        var viewModel = _mapper.Map<TestContactViewModel>(contact);
        viewModel.TestContact = contact;

        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Instructions()
    {
        var instructions =
            @"
            == INSTRUCTIONS FOR CONTACT MANAGER EXAMPLE ==

            Link to tutorial: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-10.0

            - This example requires the creation of users with roles Admin, Manager, and User.
            - Works with Identity API.
        ";

        return Ok(instructions);
    }

    #endregion

    #region Edit

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var contact = await _contactService.GetByIdAsync(id, HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            TestContactOperations.Update
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        var viewModel = _mapper.Map<TestContactViewModel>(contact);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TestContactViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        var contact = await _contactService.GetByIdAsNoTrackingAsync(id, HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            TestContactOperations.Update
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        viewModel.OwnerID = contact.OwnerID;
        var contactMapped = _mapper.Map<TestContact>(viewModel);

        if (contactMapped.Status == TestContactStatus.Approved)
        {
            var canApprove = await _authorizationService.AuthorizeAsync(
                User,
                contactMapped,
                TestContactOperations.Approve
            );

            if (!canApprove.Succeeded)
                contactMapped.Status = TestContactStatus.Submitted;
        }

        await _contactService.UpdateAsync(contactMapped, HttpContext.RequestAborted);

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Save

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TestContactViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        viewModel.OwnerID = _userManagerService.GetUserId(User);

        var contact = _mapper.Map<TestContact>(viewModel);

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            TestContactOperations.Create
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        await _contactService.CreateAsync(contact, HttpContext.RequestAborted);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(int id, TestContactStatus status)
    {
        var contact = await _contactService.GetByIdAsync(id, HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var contactOperation =
            (status == TestContactStatus.Approved)
                ? TestContactOperations.Approve
                : TestContactOperations.Reject;

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            contactOperation
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        await _contactService.UpdateStatusAsync(id, status, HttpContext.RequestAborted);

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _contactService.GetByIdAsync(id, HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            TestContactOperations.Delete
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        var viewModel = _mapper.Map<TestContactViewModel>(contact);
        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contact = await _contactService.GetByIdAsNoTrackingAsync(id, HttpContext.RequestAborted);

        if (contact == null)
            return NotFound();

        var isAuthorized = await _authorizationService.AuthorizeAsync(
            User,
            contact,
            TestContactOperations.Delete
        );

        if (!isAuthorized.Succeeded)
            return Forbid();

        await _contactService.DeleteAsync(contact, HttpContext.RequestAborted);

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Private methods

    // No methods

    #endregion
}
