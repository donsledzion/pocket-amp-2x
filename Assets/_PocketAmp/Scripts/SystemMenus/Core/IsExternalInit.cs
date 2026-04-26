namespace System.Runtime.CompilerServices
{
    // This class is required to use C# 9.0 'init' properties in projects targeting frameworks
    // that don't include it (like Unity's .NET Standard 2.0 / 2.1 depending on specific version).
    internal static class IsExternalInit { }
}
