using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor;
using Snowberry.Editor.Tools;
using Snowberry.UI.Controls;
using Snowberry.UI.Menus;

namespace Snowberry.UI;

public class Example : UIScene {

    // on resizing
    protected List<UIElement> centre = new(), stretch = new();

    protected override void BeginContent() {
        Audio.Stop(Audio.CurrentAmbienceEventInstance);
        Audio.Stop(Audio.CurrentMusicEventInstance);

        UILabel title = new UILabel("snowberry!", 2);
        title.Position = new((UI.Width - title.Width) / 2f, 15);
        UI.Add(title);
        centre.Add(title);

        UILabel time = new UILabel(() => $"it's {DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()} rn");
        time.Position = new((UI.Width - time.Width) / 2f, 45);
        UI.Add(time);
        centre.Add(time);

        UIElement content = new UIScrollPane {
            Position = new(0, 70),
            Width = UI.Width,
            Height = UI.Height - 70,
            BG = Color.Cyan * 0.2f,
            TopPadding = 10
        };
        UI.Add(content);
        stretch.Add(content);

        content.AddBelow(new UIButton("hi! click me!", Fonts.Regular), new(10));
        content.AddBelow(new UIButton("no, click me!", Fonts.Regular, 6, 6), new(10));

        UIButton dropdownButton = null;
        dropdownButton = new UIButton("i can show you stuff \uF036", Fonts.Regular, 6, 6) {
            OnPress = () => {
                content.Add(new UIDropdown(Fonts.Regular,
                    new UIDropdown.DropdownEntry("like this!", null),
                    new UIDropdown.DropdownEntry("cool stuff, right?", null),
                    new UIDropdown.DropdownEntry("i'm sure you're impressed }:3", null)
                ) {
                    Position = dropdownButton.GetBoundsPos() + Vector2.UnitY * (dropdownButton.Height + 2) - content.GetBoundsPos()
                });
            }
        };
        content.AddBelow(dropdownButton, new(10));

        content.AddBelow(new UITextField(Fonts.Regular, 220, "but we still have more to show off!"), new(10));

        content.AddBelow(new UILabel("pick your favourite color! :") {
            Underline = true
        }, new(10));
        content.AddBelow(new UIColorPicker(Color.Yellow), new(10, 4));

        content.AddBelow(UIPluginOptionList.BoolOption("am i cool", true, _ => {}), new(10));
        content.AddBelow(UIPluginOptionList.BoolOption("are you cool", true, _ => {}), new(10, 3));

        UIRibbon first = new UIRibbon("fancy text sometimes needs highlighting");
        content.AddBelow(first, new(10));
        UIRibbon second = new UIRibbon("it's true!", 8, 8, true, false) {
            FG = Color.DarkBlue,
            BG = Color.Aqua,
            BGAccent = Color.DarkOrange
        };
        content.AddBelow(second, new(10 + (first.Width - second.Width), 5));
        UIRibbon third = new UIRibbon("it's an important bit of design", 4, 4, true, true) {
            FG = Color.Black,
            BG = Color.White,
            BGAccent = Color.Gray
        };
        content.AddBelow(third, new(10 + (first.Width - third.Width) / 2f, 5));

        content.AddBelow(new UIButton(UIScene.ActionbarAtlas.GetSubtexture(32, 0, 16, 16), 3, 3) {
            OnPress = () => {
                Message.Clear();

                Message.AddElement(new UILabel("would you like to Leave"), new(0, -20), hiddenJustifyY: -0.1f);
                Message.AddElement(UIMessage.YesAndNoButtons(() => {
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
                    Message.Shown = false;
                }, () => Message.Shown = false), new(0, 20));
                Message.Shown = true;
            }
        }, new(10));

        UIElement brushModes = new();
        UIButton blue = null, red = null;
        void UpdateBrushColors() {
            foreach(var button in brushModes.Children.Cast<UIButton>())
                if (blue == red && blue == button)
                    button.BG = button.PressedBG = button.HoveredBG = Tool.BothSelectedBtnBg;
                else if (button == blue)
                    button.BG = button.PressedBG = button.HoveredBG = Tool.LeftSelectedBtnBg;
                else if (button == red)
                    button.BG = button.PressedBG = button.HoveredBG = Tool.RightSelectedBtnBg;
                else
                    button.ResetBgColors();
        }
        foreach (var mode in Enum.GetValues(typeof(TileBrushTool.TileBrushMode))) {
            UIButton button = null;
            button = new UIButton(TileBrushTool.brushes.GetSubtexture(0, 16 * (int)mode, 16, 16), 3, 3) {
                OnPress = () => {
                    blue = button;
                    UpdateBrushColors();
                },
                OnRightPress = () => {
                    red = button;
                    UpdateBrushColors();
                },
                ButtonTooltip = Dialog.Clean($"SNOWBERRY_EDITOR_TILE_BRUSH_{mode.ToString().ToUpperInvariant()}_TT")
            };
            brushModes.AddRight(button, new(0, 4));
            blue ??= button;
            red ??= button;
        }
        UIButton.Group(brushModes.Children.OfType<UIButton>().ToList());
        brushModes.CalculateBounds();
        content.AddBelow(brushModes, new(10, 10));

        int v = 0;
        content.AddBelow(new UISlider {
            OnInputChanged = i => v = (int)i,
            Min = 0,
            Max = 10
        }, new(4, 10));
        content.AddBelow(new UILabel(() => v.ToString()), new(4, 5));

        // TODO: simplify searching
        List<string> thingsToSearch = new() {
            "snowberry",
            "snowberries",
            "the snow berries",
            "pennies and pounds",
            "h",
            "AnAnonymousFox's various debug strings",
            "winter",
            "raspberry",
            "blueberries and blackberries",
            "summer",
            "this is just like ahorn",
            "who needs an ImGui anyways",
            "void",
            "farewell",
            "i'm running out of ideas here",
            "please help",
            "blah blah",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit,",
            "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            "so true, latin scholars",
            "..."
        };
        thingsToSearch.AddRange("abcdefghijklmnopqrstuvwxyz".Select(c => c.ToString()));

        const int searchDemoWidth = 230;
        UIElement searchDemo = new() {
            Width = searchDemoWidth,
            Height = 310
        };
        UIScrollPane searchable = new UIScrollPane {
            Width = searchDemoWidth,
            Height = 300,
            TopPadding = 5,
            BG = Color.White * 0.5f
        };
        List<(string value, UIElement display)> elements = new();
        foreach (string s in thingsToSearch) {
            UILabel label = new(s);
            elements.Add((s, label));
            searchable.AddBelow(label, new(5));
        }
        UISearchBar<string> searchBar = null;
        searchBar = new UISearchBar<string>(searchDemoWidth - 10, (entry, term) => entry.Contains(term)) {
            InfoText = "search stuff here...",
            Entries = thingsToSearch.ToArray(),
            OnInputChange = _ => {
                searchable.Scroll = 0;
                int y = 5;
                foreach (var b in elements) {
                    var label = b.display;
                    var active = searchBar.Found == null || searchBar.Found.Contains(b.value);
                    label.Visible = active;
                    if (active) {
                        label.Position.Y = y;
                        y += label.Height + 5;
                    }
                }
            }
        };
        searchDemo.AddBelow(searchBar, new(5, 0));
        searchDemo.AddBelow(searchable, new(0, 5));

        content.AddRight(searchDemo, new(20, 10));

        // woooooooooooo
        UITextField text = new UITextField(Fonts.Regular, 200);
        content.AddRight(text, new(20));
        var drop2downButton = new UIButton("\uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                List<List<string>> texts = GFX.Game.Textures.Keys
                    .Select(x => x.Split('/').ToList())
                    .Where(x => x.Count > 0 && x[0] == "bgs")
                    .ToList();
                Tree<string> theBigOne = Tree<string>.FromPrefixes(texts, "");
                UIMultiDropdown<string> dd = new UIMultiDropdown<string>(theBigOne, x => {
                    return new UIDropdown.DropdownEntry(x.Value + (x.Children.Count > 0 ? "/" : ""), null) {
                        OnPress = () => {
                            string entire = x.AggregateUp((l, r) => l + "/" + r).Substring(1);
                            text.UpdateInput(entire);
                        }
                    };
                }) {
                    Position = text.GetBoundsPos() + new Vector2(-2, text.Height + 3) - content.GetBoundsPos()
                };
                content.Add(dd);
            }
        };
        content.AddRight(drop2downButton, new(3, 18));

        UIKnob knob = new UIKnob {
            Min = 1,
            Max = 100,
            Value = 30
        };
        content.AddRight(knob, new(60, 40));
        content.AddRight(new UILabel(() => knob.Value.ToString(CultureInfo.InvariantCulture)), new(0, 40));
    }

    protected override void OnScreenResized() {
        foreach (UIElement e in centre)
            e.Position = new((UI.Width - e.Width) / 2f, e.Position.Y);
        foreach(UIElement e in stretch) {
            e.Width = UI.Width;
            e.Height = (int)(UI.Height - e.Position.Y);
        }
    }
}