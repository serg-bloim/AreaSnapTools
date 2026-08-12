using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Console;
using Mafi.Core.Entities.Static;
using Mafi.Core.Products;
using Mafi.Core.World;
using Mafi.Numerics;
using Mafi.Unity.InputControl;
using UnityEngine;

namespace AxisConstraint;

[HarmonyPatchCategory(AxisConstraint.HARMONY_PATCH_CATEGORY)]
[HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateIdle")]
public class PatchPolygonEditState_updateIdle
{
    internal class State
    {
        public int activeVertexIndex = -1;
    }

    static void Prefix(
        Vector2f cursor,
        bool primaryDown,
        object __instance,
        ref State __state
    )
    {
        __state = new State();
        if (primaryDown)
        {
            // console.WriteLine("test");
            __state.activeVertexIndex = get_activeVertexIndex(__instance);
            // console.WriteLine($"activeVertexIndex = {__state.activeVertexIndex}");
            // Log.Info("test");
        }


        // console.WriteLine("It worked");
    }

    private static int get_activeVertexIndex(object o)
    {
        var propertyGetter = AccessTools.PropertyGetter("Mafi.Unity.Ui.Controllers.PolygonEditState:ActiveVertexIndex");
        return (int)propertyGetter.Invoke(o, []);
    }

    public static Vector2f startingPoint = Vector2f.Zero;

    static void Postfix(
        Vector2f cursor,
        bool primaryDown,
        object __instance,
        Polygon2fMutable ___Polygon,
        ref State __state)
    {
        if (primaryDown)
        {
            var activeVertexIndex = get_activeVertexIndex(__instance);
            AxisConstraint.console.WriteLine($"Before ({__state.activeVertexIndex}) After ({activeVertexIndex})");
            if (activeVertexIndex >= 0)
            {
                startingPoint = ___Polygon[activeVertexIndex];
            }
        }
        else
        {
            startingPoint = Vector2f.Zero;
        }
    }
}

[HarmonyPatchCategory(AxisConstraint.HARMONY_PATCH_CATEGORY)]
[HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateTranslateVertex")]
public class PatchPolygonEditState_updateTranslateVertex
{
    private static KeyBindings anyShiftKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftShift, KeyCode.RightShift);
    private static KeyBindings anyCtrlKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftControl, KeyCode.RightControl);

    static void Prefix(
        ref Vector2f cursor,
        object __instance
    )
    {
        if (IsCtrlDown())
        {
            cursor = new Vector2f(cursor.X.RoundToIntMultipleOf(4), cursor.Y.RoundToIntMultipleOf(4));
        }
        if (IsShiftDown())
        {
            var diff = cursor - PatchPolygonEditState_updateIdle.startingPoint;
            if (diff.X.Abs() > diff.Y.Abs())
            {
                cursor = new Vector2f(cursor.X, PatchPolygonEditState_updateIdle.startingPoint.Y);
            }
            else
            {
                cursor = new Vector2f(PatchPolygonEditState_updateIdle.startingPoint.X, cursor.Y);
            }
        }
    }

    private static bool IsShiftDown()
    {
        return AxisConstraint.shortcutsManager.IsOn(anyShiftKey);
    }
    private static bool IsCtrlDown()
    {
        return AxisConstraint.shortcutsManager.IsOn(anyCtrlKey);
    }
}