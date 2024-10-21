using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenFeature;

[DebuggerStepThrough]
internal static class Guard
{
    public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }
}
