public interface IAction
{
    /// <summary>
    /// A unique name to identify this specific action instance.
    /// </summary>
    string Name { get; }
    
    NodeStatus Execute();
} 