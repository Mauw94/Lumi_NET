namespace Lumi.StdLib;

/// <summary>
/// Represents a global variable or function in the standard library, with its name and type descriptor. 
/// </summary>
/// <param name="Name">The name of the global variable or function.</param>
/// <param name="Type">The type descriptor of the global variable or function.</param>
public sealed record PreludeGlobalDescriptor(string Name, StdLibTypeDescriptor Type);