namespace Lumi.StdLib.Tests;

[TestClass]
public sealed class StandardLibraryRegistryTests
{
    [TestMethod]
    public void PreludeGlobals_Contains_File_Global()
    {
        var globals = StandardLibraryRegistry.PreludeGlobals.ToArray();

        Assert.HasCount(1, globals);
        Assert.AreEqual(StandardLibraryRegistry.FilePreludeName, globals[0].Name);
        Assert.AreEqual(StdLibTypeDescriptor.NativeObject(StandardLibraryRegistry.FilePreludeName), globals[0].Type);
    }

    [TestMethod]
    public void TryGetPreludeGlobal_File_Returns_Descriptor()
    {
        var found = StandardLibraryRegistry.TryGetPreludeGlobal(StandardLibraryRegistry.FilePreludeName, out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(StandardLibraryRegistry.FilePreludeName, descriptor.Name);
        Assert.AreEqual(StdLibTypeDescriptor.NativeObject(StandardLibraryRegistry.FilePreludeName), descriptor.Type);
    }

    [TestMethod]
    public void TryGetPreludeGlobal_Unknown_Returns_False()
    {
        var found = StandardLibraryRegistry.TryGetPreludeGlobal("Missing", out var descriptor);

        Assert.IsFalse(found);
        Assert.IsNull(descriptor);
    }

    [TestMethod]
    public void TryGetPreludeMethod_ReadText_Returns_String_Signature()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "readText", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(1, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TryGetPreludeMethod_WriteText_Returns_Two_String_Parameters()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "writeText", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(2, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[1]);
        Assert.AreEqual(StdLibTypeDescriptor.Undefined(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TryGetPreludeMethod_UnknownMethod_Returns_False()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "unkown", out var descriptor);

        Assert.IsFalse(found);
        Assert.IsNull(descriptor);
    }

    [TestMethod]
    public void TryGetArrayMethod_Length_Returns_Int_Signature()
    {
        var found = StandardLibraryRegistry.TryGetArrayMethod("length", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(0, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.Int(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TryGetArrayMethod_Add_Returns_Array_Signature()
    {
        var found = StandardLibraryRegistry.TryGetArrayMethod("add", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(1, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.Unknown(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.Array(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TryGetPreludeMethod_WriteLines_Returns_Array_Signature()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "writeLines", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(2, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.Array(), descriptor.ParameterTypes[1]);
        Assert.AreEqual(StdLibTypeDescriptor.Undefined(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TryGetArrayMethod_UnknownMethod_Returns_False()
    {
        var found = StandardLibraryRegistry.TryGetArrayMethod("map", out var descriptor);

        Assert.IsFalse(found);
        Assert.IsNull(descriptor);
    }

    [TestMethod]
    public void TrGetPreludeMethod_ReadLines_Returns_Array_Signature()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "readLines", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(1, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.Array(), descriptor.ReturnType);
    }

    [TestMethod]
    public void TrGetPreludeMethod_AppendText_Returns_Two_String_Parameters()
    {
        var found = StandardLibraryRegistry.TryGetPreludeMethod(StandardLibraryRegistry.FilePreludeName, "appendText", out var descriptor);

        Assert.IsTrue(found);
        Assert.IsNotNull(descriptor);
        Assert.HasCount(2, descriptor.ParameterTypes);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[0]);
        Assert.AreEqual(StdLibTypeDescriptor.String(), descriptor.ParameterTypes[1]);
        Assert.AreEqual(StdLibTypeDescriptor.Undefined(), descriptor.ReturnType);
    }
}
