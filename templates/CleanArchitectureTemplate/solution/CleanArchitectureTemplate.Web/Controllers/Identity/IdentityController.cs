using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CleanArchitectureTemplate.ApplicationCore.Common.Pagination;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Web.Models;
using CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;
using CleanArchitectureTemplate.Web.Services;
using CleanArchitectureTemplate.Web.Services.Interfaces;

namespace CleanArchitectureTemplate.Web.Controllers.Identity;

[Authorize(Policy = AuthorizationPolicyNames.IdentityAdministration)]
public class IdentityController : Controller
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IUserManagerService _userManagerService;
    private readonly IRoleManagerService _roleManagerService;
    private readonly IImageStorageService _imageStorage;

    public IdentityController(
        ILoggerFactory loggerFactory,
        IMapper mapper,
        IUserManagerService userManagerService,
        IRoleManagerService roleManagerService,
        IImageStorageService imageStorage
    )
    {
        _logger = loggerFactory.CreateLogger<IdentityController>();
        _mapper = mapper;

        _userManagerService = userManagerService;
        _roleManagerService = roleManagerService;
        _imageStorage = imageStorage;
    }

    #region Listing/Index

    //
    // GET Identity/Index
    public async Task<IActionResult> Index()
    {
        var cancellationToken = HttpContext.RequestAborted;
        var users = _userManagerService.Users.AsNoTracking();
        var now = DateTimeOffset.UtcNow;
        var newUsersSince = DateOnly.FromDateTime(
            DateTime.Today.AddDays(-IdentityDashboardViewModel.NewUsersWindowDays)
        );

        var vm = new IdentityDashboardViewModel
        {
            TotalUsers = await users.CountAsync(cancellationToken),
            ActiveUsers = await users.CountAsync(u => u.IsActive, cancellationToken),
            InactiveUsers = await users.CountAsync(u => !u.IsActive, cancellationToken),
            UnconfirmedEmailUsers = await users.CountAsync(
                u => !u.EmailConfirmed,
                cancellationToken
            ),
            LockedOutUsers = await users.CountAsync(
                u => u.LockoutEnd != null && u.LockoutEnd > now,
                cancellationToken
            ),
            TwoFactorUsers = await users.CountAsync(u => u.TwoFactorEnabled, cancellationToken),
            NewUsers = await users.CountAsync(
                u => u.RegistrationDate != null && u.RegistrationDate >= newUsersSince,
                cancellationToken
            ),
        };

        var userCountsByRoleId = await _roleManagerService.GetUserCountsByRoleIdAsync(
            cancellationToken
        );

        var roles = await _roleManagerService
            .Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        vm.Roles = roles
            .Select(r => new IdentityDashboardRoleItemViewModel
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                IsPublic = r.IsPublic,
                IsDefault = r.IsDefault,
                UserCount = userCountsByRoleId.GetValueOrDefault(r.Id),
            })
            .ToList();

        // Needs attention: locked out, inactive, or holding only the base `User` role.
        // Accounts with any role beyond `User` are excluded by construction.
        var onlyBaseRoleUserIds = _roleManagerService.QueryUserIdsWithOnlyRoles([RoleNames.User]);

        var attentionUsers = users
            .Select(u => new
            {
                User = u,
                IsLockedOut = u.LockoutEnd != null && u.LockoutEnd > now,
                IsInactive = !u.IsActive,
                HasOnlyBaseRole = onlyBaseRoleUserIds.Contains(u.Id),
            })
            .Where(x => x.IsLockedOut || x.IsInactive || x.HasOnlyBaseRole);

        vm.AttentionTotal = await attentionUsers.CountAsync(cancellationToken);

        vm.AttentionItems = (
            await attentionUsers
                .OrderByDescending(x => x.User.RegistrationDate)
                .ThenBy(x => x.User.UserName)
                .Take(IdentityDashboardViewModel.AttentionListSize)
                .Select(x => new
                {
                    x.User.Id,
                    x.User.FirstName,
                    x.User.LastName,
                    x.User.UserName,
                    x.User.Email,
                    x.User.RegistrationDate,
                    x.IsLockedOut,
                    x.IsInactive,
                    x.HasOnlyBaseRole,
                })
                .ToListAsync(cancellationToken)
        )
            .Select(x => new IdentityAttentionItemViewModel
            {
                Id = x.Id,
                DisplayName = string.IsNullOrWhiteSpace($"{x.LastName}{x.FirstName}")
                    ? x.UserName ?? x.Email ?? x.Id
                    : $"{x.LastName}, {x.FirstName}".Trim(' ', ','),
                Email = x.Email,
                RegistrationDate = x.RegistrationDate,
                IsLockedOut = x.IsLockedOut,
                IsInactive = x.IsInactive,
                HasOnlyBaseRole = x.HasOnlyBaseRole,
            })
            .ToList();

        return View(vm);
    }

    //
    // GET Identity/List
    [HttpGet]
    public async Task<IActionResult> UserList(
        string currentFilter = "",
        int pageIndex = 1,
        int pageSize = 10,
        string searchString = "",
        string sortOrder = "",
        bool lockedOnly = false
    )
    {
        var users = await GetUsersPaginatedListAsync(
            currentFilter,
            pageIndex,
            pageSize,
            searchString,
            sortOrder,
            lockedOnly
        );

        // Read by `UserList.cshtml` to keep the "only locked out" filter across links.
        ViewData["LockedOnly"] = lockedOnly;

        var usersPaginateViewModel = new PaginatedListViewModel<_UserListItemViewModel>(
            paginatedList: users,
            currentFilter: searchString,
            currentSort: sortOrder,
            sortColumnOne: string.IsNullOrEmpty(sortOrder)
                ? CurrentSort.LastNameDesc
                : CurrentSort.LastNameAsc,
            sortColumnTwo: sortOrder == CurrentSort.UsernameAsc
                ? CurrentSort.UsernameDesc
                : CurrentSort.UsernameAsc,
            pageSize: pageSize
        );

        return View(usersPaginateViewModel);
    }

    // GET: /Identity/Details/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    [HttpGet("Identity/UserDetails/{id}")]
    public async Task<IActionResult> UserDetails(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManagerService.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        var vm = _mapper.Map<UserViewModel>(user);

        vm.Roles = await _userManagerService.GetRolesAsync(user);
        vm.Claims = await _userManagerService.GetClaimsAsync(user);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> RoleList()
    {
        var roles = await _roleManagerService.Roles.ToListAsync();
        return View(roles);
    }

    [HttpGet("Identity/Placeholder")]
    public IActionResult Placeholder(Guid id)
    {
        return Ok("Placeholder endpoint, for testing purposes only.");
    }

    #endregion

    #region Edit

    // GET: /Identity/UserEdit/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    [HttpGet("Identity/UserEdit/{id}")]
    public async Task<IActionResult> UserEdit(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManagerService.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        var vm = _mapper.Map<UserViewModel>(user);

        vm.Roles = await _userManagerService.GetRolesAsync(user);
        vm.Claims = await _userManagerService.GetClaimsAsync(user);

        return View(vm);
    }

    // POST: /Identity/UserEdit/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    [HttpPost("Identity/UserEdit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(string id, UserViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var current = await _userManagerService.FindByIdAsync(id);
            if (current == null)
                return NotFound();

            SetLockoutState(vm, current);
            return View(vm);
        }
        if (string.IsNullOrWhiteSpace(vm.PhoneNumber))
            vm.PhoneNumberConfirmed = false;

        try
        {
            var user = await _userManagerService.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Lockout fields are display-only (not posted); refill them for any re-render.
            SetLockoutState(vm, user);

            // ---- Email change ----
            if (
                !string.IsNullOrWhiteSpace(vm.Email)
                && !string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase)
            )
            {
                var emailResult = await _userManagerService.SetEmailAsync(user, vm.Email);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError(nameof(vm.Email), error.Description);

                    return View(vm);
                }
            }

            // ---- Username change ----
            if (
                !string.IsNullOrWhiteSpace(vm.UserName)
                && !string.Equals(
                    user.UserName,
                    vm.UserName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                var userNameResult = await _userManagerService.SetUserNameAsync(
                    user,
                    vm.UserName
                );
                if (!userNameResult.Succeeded)
                {
                    foreach (var error in userNameResult.Errors)
                        ModelState.AddModelError(nameof(vm.UserName), error.Description);

                    return View(vm);
                }
            }

            if (vm.Image != null)
                vm.ImagePath = await _imageStorage.SaveAsync(vm.Image, "users", HttpContext.RequestAborted);

            // ---- Custom fields ----
            user.City = vm.City;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.PhoneNumber = vm.PhoneNumber;

            bool isBeingDeactivated = user.IsActive && !vm.IsActive;

            // Lockout state (`LockoutEnd`, `AccessFailedCount`) is read-only here;
            // it is changed only by failed logins and the `UserUnlock` action.
            user.EmailConfirmed = vm.EmailConfirmed;
            user.IsActive = vm.IsActive;
            user.LockoutEnabled = vm.LockoutEnabled;
            user.PhoneNumberConfirmed = vm.PhoneNumberConfirmed;
            user.TwoFactorEnabled = vm.TwoFactorEnabled;

            user.ImagePath = vm.ImagePath;

            var updateResult = await _userManagerService.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );

                return View(vm);
            }

            // Changing the security stamp invalidates the user's existing auth cookies
            // on their next SecurityStampValidator check (see StartUp.cs).
            if (isBeingDeactivated)
            {
                var stampResult = await _userManagerService.UpdateSecurityStampAsync(user);
                if (!stampResult.Succeeded)
                {
                    _logger.LogWarning(
                        "Security stamp could not be updated for deactivated user {UserId}.",
                        user.Id
                    );
                }
            }

            return RedirectToAction(nameof(UserList));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var exists = await _userManagerService.FindByIdAsync(vm.Id);
            if (exists == null)
                return NotFound();

            SetLockoutState(vm, exists);

            _logger.LogWarning(
                ex,
                "Concurrency conflict while updating user ID {UserId}.",
                vm.Id
            );

            ModelState.AddModelError(
                string.Empty,
                "The record you tried to edit was changed by another user. Reload the page and try again."
            );

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while editing the user with ID {UserId}.",
                vm.Id
            );

            ModelState.AddModelError(
                string.Empty,
                "The changes could not be saved. Try again and, if the problem persists, contact the system administrator."
            );

            return View(vm);
        }
    }

    // POST: /Identity/UserUnlock/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    [HttpPost("Identity/UserUnlock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserUnlock(string id, string? returnTo = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userManagerService.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Clear the lockout and the failed-attempt counter in one update. Writing the
        // fields directly (instead of `SetLockoutEndDateAsync`) also works when
        // `LockoutEnabled` is off, so a running lock can always be ended.
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;

        try
        {
            var result = await _userManagerService.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User {UserId} unlocked by {AdminUser}.",
                    user.Id,
                    User.Identity?.Name
                );
                TempData[LockoutMessageKey] = "The account was unlocked.";
            }
            else
            {
                _logger.LogWarning(
                    "User {UserId} could not be unlocked: {Errors}",
                    user.Id,
                    string.Join("; ", result.Errors.Select(e => e.Description))
                );
                TempData[LockoutErrorKey] = "The account could not be unlocked.";
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while unlocking user {UserId}.", id);
            TempData[LockoutErrorKey] =
                "The account was changed by another user. Try again.";
        }

        // Only known actions are accepted, so `returnTo` cannot be used as an open redirect.
        var returnAction = returnTo == nameof(UserDetails) ? nameof(UserDetails) : nameof(UserEdit);
        return RedirectToAction(returnAction, new { id });
    }

    // GET: /Identity/UserCreate
    [HttpGet("Identity/UserCreate")]
    public IActionResult UserCreate()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UserRolesEdit(Guid id)
    {
        var user = await _userManagerService.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var userRoles = await _userManagerService.GetRolesAsync(user);
        var roles = _roleManagerService.Roles.ToList();

        var viewModel = new UserRolesEditViewModel
        {
            UserId = user.Id,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            UserName = user.UserName!,
            Email = user.Email,
            Roles = roles
                .Select(r => new UserRoleEditItemViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name!,
                    IsAssigned = userRoles.Contains(r.Name!) || r.Name == RoleNames.User,
                    IsRequired = r.Name == RoleNames.User,
                })
                .ToList(),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserRolesEdit(UserRolesEditViewModel vm)
    {
        var user = await _userManagerService.FindByIdAsync(vm.UserId);
        if (user == null)
            return NotFound();

        var currentRoles = await _userManagerService.GetRolesAsync(user);

        var selectedRoles = vm
            .Roles.Where(r => r.IsAssigned)
            .Select(r => r.RoleName)
            .Append(RoleNames.User)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rolesToAdd = selectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase);
        var rolesToRemove = currentRoles.Except(
            selectedRoles,
            StringComparer.OrdinalIgnoreCase
        );

        if (rolesToAdd.Any())
            await _userManagerService.AddToRolesAsync(user, rolesToAdd);

        if (rolesToRemove.Any())
            await _userManagerService.RemoveFromRolesAsync(user, rolesToRemove);

        return RedirectToAction(nameof(UserDetails), new { id = vm.UserId });
    }

    // GET: /Identity/RoleCreate
    [HttpGet("Identity/RoleCreate")]
    public IActionResult RoleCreate()
    {
        return View(new RoleViewModel());
    }

    // GET: /Identity/RoleEdit/{id}
    [HttpGet("Identity/RoleEdit/{id}")]
    public async Task<IActionResult> RoleEdit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var role = await _roleManagerService.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        var vm = _mapper.Map<RoleViewModel>(role);

        return View(vm);
    }

    // POST: /Identity/RoleEdit/{id}
    [HttpPost("Identity/RoleEdit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleEdit(string id, RoleViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var role = await _roleManagerService.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            // Map editable fields
            _mapper.Map(vm, role);

            var updateResult = await _roleManagerService.UpdateAsync(role);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(vm);
            }

            return RedirectToAction(nameof(RoleList));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var exists = await _roleManagerService.FindByIdAsync(vm.Id);
            if (exists == null)
                return NotFound();

            _logger.LogWarning(
                ex,
                "Concurrency conflict while updating role ID {RoleId}.",
                vm.Id
            );

            ModelState.AddModelError(
                string.Empty,
                "The role was changed by another user. Reload the page and try again."
            );

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing role ID {RoleId}.", vm.Id);

            ModelState.AddModelError(
                string.Empty,
                "The changes could not be saved. Try again and, if the problem persists, contact the system administrator."
            );

            return View(vm);
        }
    }

    #endregion

    #region Save

    // POST: /Identity/UserCreate
    [HttpPost("Identity/UserCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserCreate(UserViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        try
        {
            var existingUser = await _userManagerService.FindByEmailAsync(vm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return View(vm);
            }

            var existingUsername = await _userManagerService.FindByNameAsync(vm.UserName);
            if (existingUsername != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this username already exists.");
                return View(vm);
            }

            vm.ImagePath =
                await _imageStorage.SaveAsync(vm.Image, "users", HttpContext.RequestAborted)
                ?? ImageStorageService.DefaultImagePath;

            var user = new ApplicationUser
            {
                City = vm.City,
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UserName = vm.UserName,
                EmailConfirmed = vm.EmailConfirmed,
                IsActive = vm.IsActive,
                PhoneNumber = vm.PhoneNumber,
                PhoneNumberConfirmed = vm.PhoneNumberConfirmed,
                ImagePath = vm.ImagePath,
            };

            var defaultPassword = ApplicationConstants.TestPassword;
            var result = await _userManagerService.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }
                return View(vm);
            }

            // TODO Send confirmation email or set password reset token
            // var token = await _userManagerService.GeneratePasswordResetTokenAsync(user);

            _logger.LogInformation(
                "User {Email} created successfully by {AdminUser}",
                user.Email,
                User.Identity?.Name
            );

            TempData.SetMessage(
                new MessageModel
                {
                    Content = $"User {user.UserName} was created with the temporary password {defaultPassword}",
                    Type = MessageType.Success,
                }
            );
            return RedirectToAction(nameof(UserList));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while creating user {Email}",
                vm.Email
            );
            ModelState.AddModelError(
                string.Empty,
                "The changes could not be saved. "
                    + "Try again and, if the problem persists, contact the system administrator."
            );
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while creating user {Email}",
                vm.Email
            );
            ModelState.AddModelError(
                string.Empty,
                "An unexpected error occurred. Try again."
            );
            return View(vm);
        }
    }

    // POST: /Identity/RoleCreate
    [HttpPost("Identity/RoleCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleCreate(RoleViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var role = _mapper.Map<ApplicationRole>(vm);
            role.Id = Guid.NewGuid().ToString();

            var result = await _roleManagerService.CreateAsync(role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(vm);
            }

            return RedirectToAction(nameof(RoleList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating role {RoleName}.", vm.Name);

            ModelState.AddModelError(
                string.Empty,
                "The role could not be created. Try again and, if the problem persists, contact the system administrator."
            );

            return View(vm);
        }
    }

    #endregion

    #region Delete

    // GET: /Identity/Delete/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    // Display student before deletion.
    [HttpGet("Identity/UserDelete/{id}")]
    public async Task<IActionResult> UserDelete(string id, bool? saveChangesError = false)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManagerService.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound();
        var vm = _mapper.Map<UserViewModel>(user);

        if (saveChangesError.GetValueOrDefault())
        {
            ViewData["ErrorMessage"] =
                "Delete failed. Try again and, if the problem persists, "
                + "contact the system administrator.";
        }

        return View(vm);
    }

    // POST: /Identity/Delete/0066d2ba-7012-4b4a-86dd-d5a9e33ee803
    [HttpPost("Identity/UserDelete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserDeleteConfirmed(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _userManagerService.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        try
        {
            var result = await _userManagerService.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(UserList));
            }

            var errors = new List<IdentityError>();
            errors.AddRange(result.Errors);

            return BadRequest(
                new
                {
                    errors = errors.Select(e => new { code = e.Code, description = e.Description }),
                }
            );
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while deleting the user with username {0}.",
                user.UserName
            );
            return RedirectToAction(nameof(UserDelete), new { id, saveChangesError = true });
        }
    }

    // GET: /Identity/RoleDelete/{id}
    [HttpGet("Identity/RoleDelete/{id}")]
    public async Task<IActionResult> RoleDelete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var role = await _roleManagerService.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        var vm = _mapper.Map<RoleViewModel>(role);

        return View(vm);
    }

    // POST: /Identity/RoleDelete/{id}
    [HttpPost("Identity/RoleDelete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleDeleteConfirmed(string id, RoleViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        if (id != vm.Id)
            return BadRequest();

        try
        {
            var role = await _roleManagerService.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManagerService.DeleteAsync(role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("RoleDelete", vm);
            }

            return RedirectToAction(nameof(RoleList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting role ID {RoleId}.", vm.Id);

            ModelState.AddModelError(
                string.Empty,
                "The role could not be deleted. Try again and, if the problem persists, contact the system administrator."
            );

            return View("RoleDelete", vm);
        }
    }

    #endregion

    #region Private methods

    // TempData keys read by `UserEdit.cshtml` to report the unlock result.
    public const string LockoutMessageKey = "LockoutMessage";
    public const string LockoutErrorKey = "LockoutError";

    private static void SetLockoutState(UserViewModel vm, ApplicationUser user)
    {
        vm.LockoutEnd = user.LockoutEnd;
        vm.AccessFailedCount = user.AccessFailedCount;
    }

    private async Task<PaginatedList<_UserListItemViewModel>> GetUsersPaginatedListAsync(
        string currentFilter,
        int pageIndex,
        int pageSize,
        string searchString,
        string sortOrder,
        bool lockedOnly
    )
    {
        var users = _userManagerService.Users.AsQueryable();

        var totalRecords = users.Count();

        // PAGING
        if (searchString != currentFilter)
            pageIndex = 1;
        else
            searchString = currentFilter;

        // SEARCH
        if (!string.IsNullOrEmpty(searchString))
        {
            var term = searchString.Trim();

            users = users.Where(u =>
                (u.LastName != null && u.LastName.Contains(term))
                || (u.FirstName != null && u.FirstName.Contains(term))
            );
        }

        // LOCKED OUT ONLY (same rule as the dashboard's locked-out count)
        if (lockedOnly)
        {
            var now = DateTimeOffset.UtcNow;
            users = users.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);
        }

        var filteredCount = users.Count();

        // SORTING
        switch (sortOrder)
        {
            case CurrentSort.LastNameDesc:
                users = users.OrderByDescending(s => s.LastName);
                break;
            case CurrentSort.UsernameAsc:
                users = users.OrderBy(s => s.UserName);
                break;
            case CurrentSort.UsernameDesc:
                users = users.OrderByDescending(s => s.UserName);
                break;
            default:
                users = users.OrderBy(s => s.LastName);
                break;
        }

        var count = users.Count();

        var usersPage = users.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        var result = new List<_UserListItemViewModel>();

        foreach (var user in usersPage)
        {
            var mappedUser = _mapper.Map<_UserListItemViewModel>(user);
            var roles = await _userManagerService.GetRolesAsync(user);
            mappedUser.Roles = roles;
            result.Add(mappedUser);
        }

        return new PaginatedList<_UserListItemViewModel>(
            items: result,
            count: count,
            pageIndex: pageIndex,
            pageSize: pageSize,
            totalRecords: totalRecords,
            filteredCount: filteredCount
        );
    }

    #endregion
}
