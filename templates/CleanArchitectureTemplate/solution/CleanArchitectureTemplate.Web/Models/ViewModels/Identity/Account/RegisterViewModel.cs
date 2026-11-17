// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "The {0} must be at least {2} characters long."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
    public string? ConfirmPassword { get; set; }
}