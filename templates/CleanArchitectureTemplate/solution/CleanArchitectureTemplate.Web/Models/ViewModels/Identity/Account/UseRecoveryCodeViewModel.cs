// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Account;

public class UseRecoveryCodeViewModel
{
    [Required]
    [Display(Name = "Recovery code")]
    public string? Code { get; set; }

    public string? ReturnUrl { get; set; }
}
