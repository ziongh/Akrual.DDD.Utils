﻿using System;

namespace Akrual.DDD.Utils.Internal.Logging
{
    /// <summary>
    /// Represents a way to get a <see cref="Akrual.DDD.Utils.Internal.Logging.ILog"/>
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference.</returns>
        Logger GetLogger(string name);

        /// <summary>
        /// Opens a nested diagnostics context. Not supported in EntLib logging.
        /// </summary>
        /// <param name="message">The message to add to the diagnostics context.</param>
        /// <returns>A disposable that when disposed removes the message from the context.</returns>
        IDisposable OpenNestedContext(string message);

        /// <summary>
        /// Opens a mapped diagnostics context. Not supported in EntLib logging.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>A disposable that when disposed removes the map from the context.</returns>
        IDisposable OpenMappedContext(string key, object value, bool destructure = false);
    }
}