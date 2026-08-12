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

namespace AreaSnapTools;

[HarmonyPatchCategory(AreaSnapTools.HARMONY_PATCH_CATEGORY)]
[HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateIdle")]
public class PatchPolygonEditState_updateIdle
{
    static void Postfix(
        bool primaryDown,
        object __instance,
        Polygon2fMutable ___Polygon)
    {
        if (primaryDown)
        {
            var activeVertexIndex = get_activeVertexIndex(__instance);
            if (activeVertexIndex >= 0)
            {
                AreaSnapTools.startingPoint = ___Polygon[activeVertexIndex];
            }
        }
        else
        {
            AreaSnapTools.startingPoint = Vector2f.Zero;
        }
    }

    private static int get_activeVertexIndex(object o)
    {
        var propertyGetter = AccessTools.PropertyGetter("Mafi.Unity.Ui.Controllers.PolygonEditState:ActiveVertexIndex");
        return (int)propertyGetter.Invoke(o, []);
    }
}

[HarmonyPatchCategory(AreaSnapTools.HARMONY_PATCH_CATEGORY)]
[HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateTranslateVertex")]
public class PatchPolygonEditState_updateTranslateVertex
{
    private static KeyBindings anyShiftKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftShift, KeyCode.RightShift);
    private static KeyBindings anyCtrlKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftControl, KeyCode.RightControl);

    static void Prefix(
        ref Vector2f cursor
    )
    {
        if (IsCtrlDown())
        {
            cursor = new Vector2f(cursor.X.RoundToIntMultipleOf(4), cursor.Y.RoundToIntMultipleOf(4));
        }
        if (IsShiftDown())
        {
            var diff = cursor - AreaSnapTools.startingPoint;
            if (diff.X.Abs() > diff.Y.Abs())
            {
                cursor = new Vector2f(cursor.X, AreaSnapTools.startingPoint.Y);
            }
            else
            {
                cursor = new Vector2f(AreaSnapTools.startingPoint.X, cursor.Y);
            }
        }
    }

    private static bool IsShiftDown()
    {
        return AreaSnapTools.shortcutsManager.IsOn(anyShiftKey);
    }
    private static bool IsCtrlDown()
    {
        return AreaSnapTools.shortcutsManager.IsOn(anyCtrlKey);
    }
}