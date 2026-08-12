namespace Interviews.RottingOranges;

/// <summary>
/// Which neighbouring cells a rotten orange can reach in one minute.
/// </summary>
public enum SpreadPattern
{
    /// <summary>
    /// The four cells sharing an edge - up, down, left and right. This is how the original
    /// problem is stated, and the default.
    /// </summary>
    Orthogonal = 0,

    /// <summary>
    /// All eight surrounding cells: the four orthogonal neighbours plus the four corners.
    /// Rot can then cut across a diagonal gap that would otherwise stop it.
    /// </summary>
    Diagonal = 1,
}
