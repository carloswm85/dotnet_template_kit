// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Manage;

public class VerifyPhoneNumberViewModel
{
    [Required]
    [Display(Name = "Code")]
    public string Code { get; set; } = default!;

    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = default!;
}
