using System.Collections.Concurrent;

public class InMemoryStore
{
    // Aseguramos que el Update sobreescriba, no duplique
    public ConcurrentDictionary<Guid, object> Records { get; } = new();
}