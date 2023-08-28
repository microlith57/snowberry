using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.UI;
using Snowberry.Editor.UI.Menus;

namespace Snowberry.Editor.Tools;

public class MapInfoTool : Tool {

    public override string GetName() => Dialog.Clean("SNOWBERRY_EDITOR_TOOL_MAPINFO");

    public override UIElement CreatePanel(int height){
        var ret = new UIElement{
            Width = 160,
            Height = height,
            GrabsClick = true,
            GrabsScroll = true,
            Background = Calc.HexToColor("202929") * 0.5f
        };

        Map map = Editor.Instance.Map;

        Vector2 titleOffset = new(12, 3), optionOffset = new(4, 3);

        ret.AddBelow(new UILabel($"editing {map.Name}"){
            Underline = true
        }, titleOffset);

        ret.AddBelow(UIPluginOptionList.StringOption("icon", map.Meta.Icon ?? "", i => map.Meta.Icon = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.StringOption("cassette song", map.Meta.CassetteSong ?? "", i => map.Meta.CassetteSong = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.LiteralValueOption<float>("darkness alpha", map.Meta.DarknessAlpha?.ToString() ?? "0.05", i => map.Meta.DarknessAlpha = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.LiteralValueOption<float>("bloom base", map.Meta.BloomBase?.ToString() ?? "0", i => map.Meta.BloomBase = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.LiteralValueOption<float>("bloom strength", map.Meta.BloomStrength?.ToString() ?? "1", i => map.Meta.BloomStrength = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.DropdownOption("intro type", map.Meta.IntroType ?? Player.IntroTypes.None, i => map.Meta.IntroType = i), optionOffset);
        ret.AddBelow(UIPluginOptionList.DropdownOption("core mode", map.Meta.CoreMode ?? Session.CoreModes.None, i => map.Meta.CoreMode = i), optionOffset);

        return ret;
    }

    public override void Update(bool canClick){}
}