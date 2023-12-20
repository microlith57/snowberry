using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Snowberry.Editor;
using Snowberry.UI.Layout;

namespace Snowberry.UI.Menus;

public class UISelectionPane : UIScrollPane{

    public UISelectionPane(){
        GrabsClick = true;
    }

    public void Display(List<Selection> selection){
        Clear();
        HashSet<Entity> seen = [];
        if(selection != null){
            int y = 0;
            foreach (Selection s in selection){
                if(s is EntitySelection{ Entity: var e }){
                    if (!seen.Add(e))
                        continue;
                }else if (s is TileSelection)
                    continue;

                UIElement entry = AddEntry(s);
                entry.Position.Y = y;
                y += entry.Height + 8;
            }
        }
    }

    private UIElement AddEntry(Selection s){
        UIRibbon name = new UIRibbon(s.Name(), 8, 8, true, false){
            BG = Util.Colors.DarkGray,
            BGAccent = s.Accent()
        };
        name.Position.X += Width - name.Width;
        UIElement entry = name;

        if(s is EntitySelection{ Entity: var e }){
            UILabel id = new UILabel($"#{e.EntityID}"){
                FG = Util.Colors.White * 0.5f
            };
            id.Position.X = name.Position.X - id.Width - 4;

            UIPluginOptionList options = new UIPluginOptionList(e){
                Position = new Vector2(3, name.Height + 3)
            };

            entry = Regroup(id, name, options);
        }else if(s is DecalSelection{ Decal: var d }){
            UIElement options = new UIElement{
                Position = new Vector2(3, name.Height + 3)
            };

            Vector2 offset = new(4, 3);

            options.AddBelow(UIPluginOptionList.LiteralValueOption(Dialog.Clean("SNOWBERRY_EDITOR_DECAL_OPT_SCALE_X"), d.Scale.X, sc => d.Scale.X = sc), offset);
            options.AddBelow(UIPluginOptionList.LiteralValueOption(Dialog.Clean("SNOWBERRY_EDITOR_DECAL_OPT_SCALE_Y"), d.Scale.Y, sc => d.Scale.Y = sc), offset);
            options.AddBelow(UIPluginOptionList.LiteralValueOption(Dialog.Clean("SNOWBERRY_EDITOR_DECAL_OPT_ROTATION"), d.Rotation, r => d.Rotation = r), offset);
            options.AddBelow(UIPluginOptionList.ColorOption(Dialog.Clean("SNOWBERRY_EDITOR_DECAL_OPT_COLOUR"), d.Color, c => d.Color = c));
            options.CalculateBounds();

            entry = Regroup(name, options);
        }

        Add(entry);
        return entry;
    }
}