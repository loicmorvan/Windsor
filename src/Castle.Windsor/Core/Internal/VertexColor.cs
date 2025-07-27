namespace Castle.Windsor.Core.Internal;

internal enum VertexColor
{
    NotInThisSet,

    /// <summary>The node has not been visited yet</summary>
    White,

    /// <summary>This node is in the process of being visited</summary>
    Gray,

    /// <summary>This now was visited</summary>
    Black
}