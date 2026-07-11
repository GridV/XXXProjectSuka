using System;
using System.Collections.Generic;

[Serializable]
public class AIDirectorValidationResult
{
    // Whether the response is considered valid for runtime use.
    public bool IsValid = false;

    // Errors found during validation.
    public List<string> Errors = new List<string>();

    // Non-fatal warnings.
    public List<string> Warnings = new List<string>();

    public static AIDirectorValidationResult ValidResult()
    {
        return new AIDirectorValidationResult { IsValid = true };
    }

    public static AIDirectorValidationResult Invalid(params string[] errors)
    {
        var r = new AIDirectorValidationResult { IsValid = false };
        if (errors != null) r.Errors.AddRange(errors);
        return r;
    }
}
