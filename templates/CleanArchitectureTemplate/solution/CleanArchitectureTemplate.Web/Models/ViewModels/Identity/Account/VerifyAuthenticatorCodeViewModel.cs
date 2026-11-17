// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Account;

public class VerifyAuthenticatorCodeViewModel
{
    [Required]
    [Display(Name = "Code")]
    public string Code { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    [Display(Name = "Remember this browser?")]
    public bool RememberBrowser { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
