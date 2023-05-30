// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An extension class for <see cref="object"/>s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UdpForwarder.Extensions;

/// <summary>
/// An extension class for <see cref="object"/>s.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// Checks whether the enumerable is empty or null.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns>A value indicating whether the enumerable is empty or null.</returns>
    internal static bool IsEmptyOrNull<T>([NotNullWhen(false)] this IEnumerable<T>? enumerable)
    {
        if (enumerable is null)
        {
            return true;
        }

        return !enumerable.Any();
    }
}