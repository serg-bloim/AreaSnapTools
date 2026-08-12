using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Mafi;
using Mafi.Numerics;
using Mafi.Unity.InputControl;
using UnityEngine;

namespace AreaSnapTools;

class Patches
{
    private static Vector2f startingPoint;
    private static Vector2f startingPointOffset;

    private static KeyBindings anyShiftKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftShift, KeyCode.RightShift);

    private static KeyBindings anyCtrlKey = KeyBindings.FromKeys(KbCategory.Tools, ShortcutMode.Game,
        KeyCode.LeftControl, KeyCode.RightControl);

    private static bool IsShiftDown()
    {
        return AreaSnapTools.shortcutsManager.IsOn(anyShiftKey);
    }

    private static bool IsCtrlDown()
    {
        return AreaSnapTools.shortcutsManager.IsOn(anyCtrlKey);
    }

    [HarmonyPatchCategory(AreaSnapTools.HARMONY_PATCH_CATEGORY)]
    [HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateIdle")]
    public class PatchPolygonEditState_updateIdle
    {
        private static readonly MethodInfo ActiveVertexIndexProprtyGetter =
            AccessTools.PropertyGetter("Mafi.Unity.Ui.Controllers.PolygonEditState:ActiveVertexIndex");

        private static readonly MethodInfo ActiveEdgeIndexProprtyGetter =
            AccessTools.PropertyGetter("Mafi.Unity.Ui.Controllers.PolygonEditState:ActiveEdgeIndex");

        static void Postfix(
            bool primaryDown,
            object __instance,
            bool __result,
            Vector2f cursor,
            Polygon2fMutable ___Polygon)
        {
            try
            {
                if (primaryDown && __result)
                {
                    var startingPointIndex = getActiveVertexIndex(__instance);
                    if (startingPointIndex < 0)
                    {
                        startingPointIndex = getActiveEdgeIndex(__instance);
                        if (startingPointIndex < 0)
                            startingPointIndex = 0;
                    }

                    startingPoint = ___Polygon[startingPointIndex];
                    startingPointOffset = startingPoint - cursor;
                }
                else
                {
                    startingPoint = Vector2f.Zero;
                    startingPointOffset = Vector2f.Zero;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, "AreaSnapTools Patches:updateIdle:Postfix failed");
            }
        }

        private static int getActiveVertexIndex(object o)
        {
            return (int)ActiveVertexIndexProprtyGetter.Invoke(o, []);
        }

        private static int getActiveEdgeIndex(object o)
        {
            return (int)ActiveEdgeIndexProprtyGetter.Invoke(o, []);
        }
    }

    [HarmonyPatchCategory(AreaSnapTools.HARMONY_PATCH_CATEGORY)]
    [HarmonyPatch("Mafi.Unity.Ui.Controllers.PolygonEditState", "updateTranslateVertex")]
    public class PatchPolygonEditState_updateTranslateVertex
    {
        static void Prefix(ref Vector2f cursor)
        {
            try
            {
                if (IsCtrlDown())
                {
                    cursor = new Vector2f(cursor.X.RoundToIntMultipleOf(4), cursor.Y.RoundToIntMultipleOf(4));
                }

                if (IsShiftDown())
                {
                    var diff = cursor - startingPoint;
                    if (diff.X.Abs() > diff.Y.Abs())
                    {
                        cursor = new Vector2f(cursor.X, startingPoint.Y);
                    }
                    else
                    {
                        cursor = new Vector2f(startingPoint.X, cursor.Y);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, "AreaSnapTools Patches:updateTranslateVertex:Prefix failed");
            }
        }
    }

    [HarmonyPatchCategory(AreaSnapTools.HARMONY_PATCH_CATEGORY)]
    [HarmonyPatch]
    public class PatchPolygonEditState_updateTranslateEdgeAndPolygon
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            Type type = AccessTools.TypeByName("Mafi.Unity.Ui.Controllers.PolygonEditState");
            yield return AccessTools.Method(type, "updateTranslateEdge");
            yield return AccessTools.Method(type, "updateTranslatePolygon");
        }

        static void Prefix(ref Vector2f cursor, MethodInfo __originalMethod)
        {
            try
            {
                if (IsCtrlDown())
                {
                    var actualStartingPointPos = cursor + startingPointOffset;
                    var correctedStartingPointPos = new Vector2f(actualStartingPointPos.X.RoundToIntMultipleOf(4),
                        actualStartingPointPos.Y.RoundToIntMultipleOf(4));
                    var correction = correctedStartingPointPos - actualStartingPointPos;
                    cursor += correction;
                }

                if (IsShiftDown())
                {
                    var actualStartingPointPos = cursor + startingPointOffset;
                    var diff = actualStartingPointPos - startingPoint;
                    if (diff.X.Abs() > diff.Y.Abs())
                    {
                        diff = diff.SetY(0);
                    }
                    else
                    {
                        diff = diff.SetX(0);
                    }

                    var correctedStartingPointPos = startingPoint + diff;
                    var correction = correctedStartingPointPos - actualStartingPointPos;
                    cursor += correction;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, $"AreaSnapTools Patches:{__originalMethod.Name}:Prefix failed");
            }
        }
    }
}