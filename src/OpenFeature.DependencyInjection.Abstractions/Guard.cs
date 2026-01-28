using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenFeature.DependencyInjection.Abstractions;

[DebuggerStepThrough]
/// <summary>
/// Guard helpers for validating arguments.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> when the argument is null.
    /// </summary>
    public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }
}
