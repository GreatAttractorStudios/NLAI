public interface ISense
{
    /// <summary>
    /// A unique name to identify this specific sense instance.
    /// </summary>
    string Name { get; }

    bool Evaluate();
} 