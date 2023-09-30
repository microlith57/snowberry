using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Triggers;

[Plugin("everest/changeInventoryTrigger")]
public class Plugin_ChangeInventoryTrigger : Trigger {

    [Option("inventory")] public InventoryType Inventory = InventoryType.Default;

    public override void Render() {
        base.Render();
        Fonts.Pico8.Draw($"({Inventory})", Center + new Vector2(0, 6), new(1), new(0.5f), Color.Black);
    }

    public new static void AddPlacements() {
        Placements.Create("Change Inventory Trigger (Everest)", "everest/changeInventoryTrigger", trigger: true);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum InventoryType{
        Prologue, Default, OldSite, CH6End, TheSummit, Core, Farewell
    }
}