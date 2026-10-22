using System;
using OverTheHill;

foreach (int multiplier in new[] { 1, 2, 4, 10, 100 })
{
    float radius = AreaScale.Radius((DiscoveryArea)multiplier);
    if (MathF.Abs(radius * radius - multiplier) > 0.0001f)
        throw new Exception($"Wrong area for {multiplier}: {radius * radius}");
}
foreach (int invalid in new[] { -100, 0, 3, 101, int.MaxValue })
    if (AreaScale.Radius((DiscoveryArea)invalid) != 1f)
        throw new Exception($"Invalid setting {invalid} must fall back to vanilla.");
Console.WriteLine("PASS: all five area multipliers and invalid-setting fallback.");

// Check the actual generated wrapper without loading the Unity/IL2CPP runtime.
string gameDir = args.Length > 0 ? args[0] : @"D:\SteamLibrary\steamapps\common\over the hill";
using var assembly = System.IO.File.OpenRead(System.IO.Path.Combine(gameDir,
    "MelonLoader", "Il2CppAssemblies", "Assembly-CSharp.dll"));
using var pe = new System.Reflection.PortableExecutable.PEReader(assembly);
var metadata = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
int targets = 0;
bool nativeTarget = false;
foreach (var handle in metadata.TypeDefinitions)
{
    var type = metadata.GetTypeDefinition(handle);
    if (metadata.GetString(type.Name) != "FogOfWarManager" || metadata.GetString(type.Namespace) != "Il2Cpp") continue;
    foreach (var methodHandle in type.GetMethods())
    {
        var method = metadata.GetMethodDefinition(methodHandle);
        if (metadata.GetString(method.Name) != "InitializeFogOfWarTexture") continue;
        var signature = metadata.GetBlobReader(method.Signature);
        signature.ReadSignatureHeader();
        if (signature.ReadCompressedInteger() == 0 &&
            (method.Attributes & System.Reflection.MethodAttributes.Static) == 0) targets++;
    }
    foreach (var fieldHandle in type.GetFields())
        if (metadata.GetString(metadata.GetFieldDefinition(fieldHandle).Name)
            .StartsWith("NativeMethodInfoPtr_InitializeFogOfWarTexture_")) nativeTarget = true;
}
if (targets != 1 || !nativeTarget)
    throw new Exception("Expected one parameterless instance fog initialization method with an IL2CPP native target.");
Console.WriteLine("PASS: fog initialization patch target exists and has an IL2CPP native method pointer.");
