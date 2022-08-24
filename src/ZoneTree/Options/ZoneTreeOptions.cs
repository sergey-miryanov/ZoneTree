﻿using Tenray.ZoneTree.Exceptions;
using Tenray.ZoneTree.Segments.Disk;
using Tenray.ZoneTree.WAL;
using Tenray.ZoneTree.Collections.BTree.Lock;
using Tenray.ZoneTree.Logger;
using Tenray.ZoneTree.Serializers;
using Tenray.ZoneTree.Comparers;

namespace Tenray.ZoneTree.Options;

/// <summary>
/// A delegate to query value deletion state.
/// </summary>
/// <typeparam name="TValue">The value type</typeparam>
/// <param name="value">Value to be queried</param>
/// <returns>true if value is deleted, false otherwise</returns>
public delegate bool IsValueDeletedDelegate<TValue>(in TValue value);

/// <summary>
/// A delegate to mark a value deleted.
/// </summary>
/// <typeparam name="TValue">The value type</typeparam>
/// <param name="value">The value to be marked as deleted</param>
public delegate void MarkValueDeletedDelegate<TValue>(ref TValue value);

/// <summary>
/// Represents configuration options of a ZoneTree.
/// </summary>
/// <typeparam name="TKey">The key type</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
public class ZoneTreeOptions<TKey, TValue>
{
    /// <summary>
    /// Mutable segment maximumum key-value pair count.
    /// When the maximum count is reached 
    /// MoveSegmentZero is called and current mutable segment is enqueued to
    /// ReadOnlySegments layer.
    /// </summary>
    public int MutableSegmentMaxItemCount { get; set; } = 1_000_000;

    /// <summary>
    /// The key comparer.
    /// </summary>
    public IRefComparer<TKey> Comparer { get; set; }

    /// <summary>
    /// The key serializer.
    /// </summary>
    public ISerializer<TKey> KeySerializer { get; set; }

    /// <summary>
    /// The value serializer.
    /// </summary>
    public ISerializer<TValue> ValueSerializer { get; set; }

    /// <summary>
    /// Delegate to query value deletion state.
    /// </summary>
    public IsValueDeletedDelegate<TValue> IsValueDeleted { get; set; } = (in TValue x) => false;

    /// <summary>
    /// Delegate to mark value deleted.
    /// </summary>
    public MarkValueDeletedDelegate<TValue> MarkValueDeleted { get; set; } = (ref TValue x) => { x = default; };

    /// <summary>
    /// Write Ahead Log Options. The options is being used
    /// for creation of new Write Ahead Logs.
    /// Existing WALs is being created with their existing options.
    /// </summary>
    public WriteAheadLogOptions WriteAheadLogOptions { get; set; } = new();

    /// <summary>
    /// Disk Segment options. The options is being used 
    /// for creation of new disk segments.
    /// Existing disk segments is being created with
    /// their existing options.
    /// </summary>
    public DiskSegmentOptions DiskSegmentOptions { get; set; } = new();

    /// <summary>
    /// Controls lock granularity of in memory BTree that represents
    /// mutable segment.
    /// </summary>
    public BTreeLockMode BTreeLockMode { get; set; } = BTreeLockMode.NodeLevelMonitor;

    /// <summary>
    /// ZoneTree Logger.
    /// </summary>
    public ILogger Logger { get; set; } = new ConsoleLogger();

    /// <summary>
    /// Tries validate ZoneTree options without throwing an exception.
    /// </summary>
    /// <param name="exception">The exception outcome of validation.</param>
    /// <returns>true if validation succeeds, false otherwise.</returns>
    public bool TryValidate(out Exception exception)
    {
        if (KeySerializer == null)
        {
            exception = new MissingOptionException("KeySerializer");
            return false;
        }
        if (ValueSerializer == null)
        {
            exception = new MissingOptionException("ValueSerializer");
            return false;
        }

        if (Comparer == null)
        {
            exception = new MissingOptionException("Comparer");
            return false;
        }

        if (RandomAccessDeviceManager == null)
        {
            exception = new MissingOptionException("RandomAccessDeviceManager");
            return false;
        }

        if (WriteAheadLogProvider == null)
        {
            exception = new MissingOptionException("WriteAheadLogProvider");
            return false;
        }
        exception = null;
        return true;
    }

    /// <summary>
    /// Validats the ZoneTree options.
    /// </summary>
    public void Validate()
    {
        if (!TryValidate(out Exception exception))
            throw exception;
    }

    /// <summary>
    /// Configures the write ahead log provider.
    /// </summary>
    public IWriteAheadLogProvider WriteAheadLogProvider { get; set; }

    /// <summary>
    /// Configures the random access device manager.
    /// </summary>
    public IRandomAccessDeviceManager RandomAccessDeviceManager { get; set; }
}