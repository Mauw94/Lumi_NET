namespace Lumi.StdLib;

/// <summary>
/// Represents the signature of a standard library method, including its parameter types and return type.
/// </summary>
/// <param name="ParameterTypes">The types of the parameters accepted by the method.</param>
/// <param name="ReturnType">The type of the value returned by the method.</param>
public sealed record StdLibMethodDescriptor(IReadOnlyList<StdLibTypeDescriptor> ParameterTypes, StdLibTypeDescriptor ReturnType);