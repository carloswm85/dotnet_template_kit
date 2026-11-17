using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CleanArchitectureTemplate.Web.Tools;

/// <summary>
/// Development aids. Output goes to the debugger (Debug.WriteLine), so it is
/// compiled out of Release builds.
/// </summary>
public static class DebugHelper
{
    public static void ModelStateToConsole(ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
        {
            Debug.WriteLine(">>> No errors.");
            Debug.WriteLine(">>> Model state is valid.");
            return;
        }

        Debug.WriteLine(">>>>>>>>> ERRORS FOUND <<<<<<<<<");

        foreach (var state in modelState)
        {
            if (state.Value is { Errors.Count: > 0 } entry)
            {
                var key = state.Key;
                var attemptedValue = entry.AttemptedValue;
                var errors = string.Join(", ", entry.Errors.Select(e => e.ErrorMessage));

                Debug.WriteLine(
                    $"Key: {key} • AttemptedValue: {attemptedValue} • Errors: {errors}"
                );
            }
        }

        Debug.WriteLine(">>>>>>>>> END ERRORS <<<<<<<<<");
    }
}
