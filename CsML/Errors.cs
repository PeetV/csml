using System;

namespace CsML.Errors;

/// <summary>Error messages.</summary>
public static class Types
{
    /// <summary>Error message if input is empty.</summary>
    public const string E1 = "Input must not be empty";

    /// <summary>Error message if input lengths differ.</summary>
    public const string E2 = "Inputs must be same length";

    /// <summary>Error message if model has not been trained.</summary>
    public const string E3 = "Model must be trained first.";

    /// <summary>
    /// Error message if the model was trained on a different number of
    /// columns.
    /// </summary>
    public const string E4 = "Same number of columns as trained on needed.";

    /// <summary>Error message if model mode is not recognised.</summary>
    public const string E5 = "Mode must be 'classify' or 'regress'";

    /// <summary>Error message if method does not apply given mode.</summary>
    public const string E6 = "Method only valid if method is 'classify'";
}
