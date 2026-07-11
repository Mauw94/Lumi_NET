namespace Lumi.Bytecode;

public enum CaptureSourceKind
{
    Local,
    Capture,
}

public sealed record CaptureBinding(string Name, CaptureSourceKind SourceKind, int SourceIndex);
