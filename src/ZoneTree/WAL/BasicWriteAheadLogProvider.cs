﻿using Tenray.WAL;
using ZoneTree.Core;

namespace ZoneTree.WAL;

public class BasicWriteAheadLogProvider<TKey, TValue> : IWriteAheadLogProvider<TKey, TValue>
{
    readonly Dictionary<int, IWriteAheadLog<TKey, TValue>> WALTable = new();
    
    public ISerializer<TKey> KeySerializer { get; }
    
    public ISerializer<TValue> ValueSerializer { get; }
    
    public string WalDirectory { get; }

    public BasicWriteAheadLogProvider(
        ISerializer<TKey> keySerializer,
        ISerializer<TValue> valueSerializer,
        string walDirectory = "data/wal")
    {
        KeySerializer = keySerializer;
        ValueSerializer = valueSerializer;
        WalDirectory = walDirectory;
        Directory.CreateDirectory(walDirectory);
    }

    public IWriteAheadLog<TKey, TValue> GetOrCreateWAL(int segmentId)
    {
        var walPath = Path.Combine(WalDirectory, segmentId + ".wal");
        if (WALTable.TryGetValue(segmentId, out var value))
        {
            return value;
        }
        var wal = new FileSystemWriteAheadLog<TKey, TValue>(
            KeySerializer,
            ValueSerializer,
            walPath);
        WALTable.Add(segmentId, wal);
        return wal;
    }

    public IWriteAheadLog<TKey, TValue> GetWAL(int segmentId)
    {
        if (WALTable.TryGetValue(segmentId, out var value))
        {
            return value;
        }
        return null;
    }

    public bool RemoveWAL(int segmentId)
    {
        return WALTable.Remove(segmentId);
    }

    public void DropStore()
    {
        if (Directory.Exists(WalDirectory))
            Directory.Delete(WalDirectory, true);
    }
}