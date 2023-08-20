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
        ret.AddBelow(UIPluginOptionList.DropdownOption<Player.IntroTypes>("intro type", map.Meta.IntroType ?? Player.IntroTypes.None, i => map.Meta.IntroType = i), optionOffset);

        return ret;
    }

    public override void Update(bool canClick){}
}