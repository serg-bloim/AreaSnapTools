using System;
using HarmonyLib;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Console;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity.InputControl;

namespace AreaSnapTools;

public sealed class AreaSnapTools : IMod
{
    public const string HARMONY_PATCH_CATEGORY = "AreaSnapTools";
    private readonly Harmony harm = new("AreaSnapTools");
    public static Vector2f startingPointOffset;
    public static Vector2f startingPoint;
    public static ShortcutsManager shortcutsManager;

    // Mod constructor will be called on mod loading.
    public AreaSnapTools(ModManifest manifest) : base()
    {
        Manifest = manifest;
        JsonConfig = new ModJsonConfig(this);
    }


    public void RegisterPrototypes(ProtoRegistrator registrator)
    {
        Log.Info("AreaSnapTools: applying patches");
        try
        {
            harm.UnpatchCategory(HARMONY_PATCH_CATEGORY);
            harm.PatchCategory(HARMONY_PATCH_CATEGORY);
            Log.Info("AreaSnapTools: patching complete");
        }
        catch (Exception e)
        {
            Log.Exception(e, "AreaSnapTools: failed to patch");
        }
    }

    public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool gameWasLoaded)
    {
    }

    public void EarlyInit(DependencyResolver resolver)
    {
        Log.Info($"AreaSnapTools: v1");
    }

    public void Initialize(DependencyResolver resolver, bool gameWasLoaded)
    {
        shortcutsManager = resolver.Resolve<ShortcutsManager>();
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