using System.Reflection;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

// Runtime (not compile-time) soft dependency on any installed Act 4 mod. We never reference
// their assembly directly - only look it up by name at startup, then resolve an entry point
// via reflection. If it's missing, renamed, or errors, we just don't offer the option; we
// never let this take down the rest of the mod.
//
// TODO: This is a scaffold. For each Act 4 mod you want to support, you need to:
//   1. Find its actual assembly name (check the .dll filename in the mod's release/workshop item).
//   2. Decompile it (ILSpy/dnSpy) far enough to find how it triggers entry into Act 4 - almost
//      certainly its own transpiler patch on RunManager.EnterNextAct, similar to Patch_EnterNextAct
//      in this project. The type/method it calls into is what EntryPointCandidates should target.
// Until then this will find nothing and IsAnyPresent will just be false, which is a safe no-op.
public static class Act4Compat
{
    private record Candidate(string AssemblyNameContains, string TypeName, string MethodName);

    private static readonly List<Candidate> EntryPointCandidates =
    [
        // Example placeholder - replace with the real assembly/type/method once identified:
        // new Candidate("Act4Placeholder", "Act4Placeholder.EntryPoint", "EnterActFour"),
    ];

    private static Func<Task>? _cachedEntryPoint;
    private static bool _scanned;

    public static bool IsAnyPresent
    {
        get
        {
            EnsureScanned();
            return _cachedEntryPoint != null;
        }
    }

    // Returns a callable that transitions the player into the modded Act 4, or null if no
    // compatible Act 4 mod was found/resolvable.
    public static Func<Task>? TryGetEntryPoint()
    {
        EnsureScanned();
        return _cachedEntryPoint;
    }

    private static void EnsureScanned()
    {
        if (_scanned) return;
        _scanned = true;

        try
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var candidate in EntryPointCandidates)
            {
                var assembly = loadedAssemblies.FirstOrDefault(a =>
                    a.GetName().Name?.Contains(candidate.AssemblyNameContains, StringComparison.OrdinalIgnoreCase) == true);

                if (assembly == null) continue;

                var type = assembly.GetType(candidate.TypeName);
                var method = type?.GetMethod(candidate.MethodName, BindingFlags.Public | BindingFlags.Static);

                if (method == null)
                {
                    MainFile.Logger.Warn($"[UncappedActs] Found {candidate.AssemblyNameContains} but couldn't resolve {candidate.TypeName}.{candidate.MethodName}; Act 4 option unavailable.");
                    continue;
                }

                _cachedEntryPoint = () => (Task)method.Invoke(null, null)!;
                MainFile.Logger.Info($"[UncappedActs] Detected Act 4 mod: {assembly.GetName().Name}. Loop event will offer an Act 4 branch.");
                return;
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[UncappedActs] Act4Compat scan failed: {e}");
        }
    }
}
