using System;
using HarmonyLib;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Console;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

namespace AxisConstraint;

public sealed class AxisConstraint : IMod
{
    public const string HARMONY_PATCH_CATEGORY = "AxisConstraint";
    private readonly Harmony harm = new("AxisConstraint");

    // Mod constructor will be called on mod loading.
    public AxisConstraint(ModManifest manifest) : base()
    {
        Manifest = manifest;
        JsonConfig = new ModJsonConfig(this);
    }


    public void RegisterPrototypes(ProtoRegistrator registrator)
    {
        Log.Info("AxisConstraint: applying patches");
        try
        {
            harm.UnpatchCategory(HARMONY_PATCH_CATEGORY);
            harm.PatchCategory(HARMONY_PATCH_CATEGORY);
            Log.Info("AxisConstraint: patching complete");
        }
        catch (Exception e)
        {
            Log.Exception(e, "AxisConstraint: failed to patch");
        }
    }

    public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool gameWasLoaded)
    {
    }

    public void EarlyInit(DependencyResolver resolver)
    {
        Log.Info($"AxisConstraint: v2");
    }

    public void Initialize(DependencyResolver resolver, bool gameWasLoaded)
    {
        Log.Info($"AxisConstraint: v1");
        PatchBattleShip.console = resolver.Resolve<IGameConsole>();
    }

    public void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }

    public ModManifest Manifest { get; }
    public bool IsUiOnly { get; }
    public Option<IConfig> ModConfig { get; }
    public ModJsonConfig JsonConfig { get; }

    public void Dispose()
    {
    }
}