using Celeste;

namespace Snowberry.Editor.Entities;

[Plugin("ridgeGate")]
public class Plugin_RidgeGate : Entity {

    [Option("texture")] public string Texture = "";
    [Option("ridge")] public bool RidgeTexture = true;

    public override int MinWidth => 8;
    public override int MinHeight => 8;
    public override int MaxNodes => 1;

    public override void Render() {
        base.Render();

        string tex = string.IsNullOrEmpty(Texture) ? (RidgeTexture ? "objects/ridgeGate" : "objects/farewellGate") : Texture;
        GFX.Game[tex].Draw(Position);
    }

    public override void SaveAttrs(BinaryPacker.Element e) {
        // only save Texture if non-empty
        if(!string.IsNullOrEmpty(Texture))
            e.Attributes["texture"] = Texture;
        e.Attributes["ridge"] = RidgeTexture;
    }

    public static void AddPlacements() {
        Placements.Create("Ridge Gate", "ridgeGate");
    }
}