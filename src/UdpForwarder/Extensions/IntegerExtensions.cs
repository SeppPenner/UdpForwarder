// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegerExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An extension class for <see cref="int"/>s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UdpForwarder.Extensions;

/// <summary>
/// An extension class for <see cref="int"/>s.
/// </summary>
internal static class IntegerExtensions
{
    /// <summary>
    /// Checks whether the given port is valid.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns>A value indicating whether the port is valid or not.</returns>
    internal static bool IsPortValid(this int port)
    {
        return port switch
        {
            < 1 => false,
            > 65535 => false,
            _ => true,
        };
    }
}