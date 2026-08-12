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
    public static IGameConsole console;

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
        var console = resolver.Resolve<IGameConsole>();
        AxisConstraint.console = resolver.Resolve<IGameConsole>();
        // var fieldNames = AccessTools.GetFieldNames(AccessTools.TypeByName("Mafi.Unity.Ui.Controllers.PolygonEditState"));
        // foreach (var fieldName in fieldNames)
        // {
        //     console.WriteLine(fieldName);
        // }
        // Test.test();
        Log.Info("MethodInfo: " + AccessTools
            .PropertyGetter("Mafi.Unity.Ui.Controllers.PolygonEditState:ActiveVertexIndex").ToString());
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

class SomeType(int m_answer)
{
    private int answer()
    {
        return m_answer;
    }
}

public class Test
{
    public static void test()
    {
        object smth = new SomeType(42);

        Log.Info("Before Test example");
        var methodInfo = AccessTools.Method("AxisConstraint.SomeType:answer");
        Func<object, int> myDelegate =
            (Func<object, int>)Delegate.CreateDelegate(typeof(Func<object, int>), methodInfo);
        Log.Info("Test example is: " + myDelegate(smth));
    }
}