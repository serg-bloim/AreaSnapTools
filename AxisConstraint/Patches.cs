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

namespace AxisConstraint;
[HarmonyPatchCategory(AxisConstraint.HARMONY_PATCH_CATEGORY)]
[HarmonyPatch(typeof(BattleShip))]
[HarmonyPatch(nameof(BattleShip.TryUnloadCargo))]
public class PatchBattleShip
{
    public static IGameConsole console;

    private static MethodInfo removeBufferIfEmpty =
        AccessTools.Method(typeof(BattleShip), "removeBufferIfEmpty");
    static bool Prefix(
        ref BattleShip __instance,
        ref ProductQuantity __result,
        Quantity maxQuantity,
        IReadOnlySet<ProductProto> productsToSkip)
    {
        try
        {
            __result = MethodReplacement(__instance, maxQuantity, productsToSkip);
            return false;
        }
        catch(Exception e)
        {
            Log.Exception(e, "Error during a method patch(Mafi.Core.World.BattleShip.TryUnloadCargo), rolling back to the original method.");
            return true;
        }
    }

    private static ProductQuantity MethodReplacement(BattleShip battleShip, Quantity maxQuantity, IReadOnlySet<ProductProto> productsToSkip)
    {
        {
            IProductBuffer buffer;
            try
            {
                buffer = battleShip.Cargo.Values
                    .Where(pb => !productsToSkip.Contains(pb.Product))
                    .MinElement(pb => pb.Quantity);
            }
            catch (Exception e)
            {
                return ProductQuantity.None;
            }

            Quantity quantity = buffer.RemoveAsMuchAs(maxQuantity);
            removeBufferIfEmpty.Invoke(battleShip, [buffer]);
            if (quantity.IsPositive)
                return new ProductQuantity(buffer.Product, quantity);
            throw new Exception("Patched method could not find a positive quantity. Let the original method run");
        }
    }
}