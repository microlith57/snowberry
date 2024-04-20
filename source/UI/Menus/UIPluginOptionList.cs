using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Snowberry.Editor;
using Snowberry.UI.Controls;

namespace Snowberry.UI.Menus;

public class UIPluginOptionList : UIElement {
    public class UIOption : UIElement {
        public readonly UIElement Input;
        public readonly UILabel Label;

        internal bool locked = false;

        public UIOption(string name, UIElement input, string tooltip = default) {
            Input = input;

            Add(Label = new UILabel($"{name} : ") {
                FG = Color.Gray,
                LabelTooltip = tooltip
            });
            int w = Label.Width + 1;

            if (input != null) {
                Add(input);
                input.Position = Vector2.UnitX * w;
            }

            Width = w + (input?.Width ?? 0);
            Height = Math.Max(Fonts.Regular.LineHeight, input?.Height ?? 0);
        }

        public UIOption WithTooltip(string tooltip) {
            Label.LabelTooltip = tooltip;
            return this;
        }
    }

    public readonly Plugin Plugin;

    public UIPluginOptionList(Plugin plugin) {
        Plugin = plugin;
        Refresh();
    }

    public void Refresh() {
        int l = 0;
        const int spacing = 13;
        foreach (var (key, option) in Plugin.Info.Options)
            l = handle(key, option.FieldType, option.GetValue(Plugin), true);
        foreach (var (key, value) in Plugin.UnknownAttrs)
            l = handle(key, value.GetType(), value, false);

        int handle(string key, Type ftype, object value, bool known) {
            UIOption ui;
            if (Plugin.CreateOptionUi(key) is (UIElement i, var height)) {
                ui = new UIOption(key, i, Plugin.GetTooltipFor(key));
                Add(ui);
                ui.Position.Y = l;
                l += height;
            }
            // TODO: this is kind of silly
            else if (ftype == typeof(bool)) {
                Add(ui = BoolOption(key, (bool)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (ftype == typeof(Color)) {
                Add(ui = ColorOption(key, value is Color c ? c : Color.White, Plugin));
                ui.Position.Y = l;
                l += 91;
            } else if (ftype == typeof(int)) {
                Add(ui = LiteralValueOption(key, (int)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (ftype == typeof(long)) {
                Add(ui = LiteralValueOption(key, (long)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (ftype == typeof(float)) {
                Add(ui = LiteralValueOption(key, (float)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (ftype == typeof(double)) {
                Add(ui = LiteralValueOption(key, (double)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (ftype == typeof(Tileset)) {
                Add(ui = TilesetDropdownOption(key, (Tileset)value, Plugin));
                ui.Position.Y = l;
                l += spacing + 6;
            } else if (ftype.IsEnum) {
                Add(ui = DropdownOption(key, ftype, value, Plugin));
                ui.Position.Y = l;
                l += spacing + 6;
            } else {
                Add(ui = StringOption(key, value?.ToString() ?? "", Plugin));
                ui.Position.Y = l;
                l += spacing;
            }

            if (!known) {
                // Fun
                string now = ui.Label.Value()[..^3] + "? : ";
                ui.Label.Value = () => now;
                ui.Label.FG = Color.Orange;
                ui.Label.UpdateBoundsFromText();
                ui.Input.Position.X = ui.Label.Width + 1;
                ui.CalculateBounds();
            }

            Height = l;
            Width = Math.Max(Children.Max(k => k.Width), Width);
            return l;
        }
    }

    private static UIOption WithPropListener<T>(string name, Plugin plugin, UIOption e, Action<T> changed){
        Action<string, object> pluginOnOnPropChange = ((pname, pvalue) => {
            if (pname == name && pvalue is T t && !e.locked)
                changed(t);
        });
        plugin.OnPropChange += pluginOnOnPropChange;
        e.OnUninitialized += () => plugin.OnPropChange -= pluginOnOnPropChange;
        return e;
    }

    private static Action<T> Locking<T>(Action<T> underlying, Func<UIOption> e) {
        return u => {
            e().locked = true;
            underlying(u);
            e().locked = false;
        };
    }

    public static UIOption StringOption(string name, string value, Action<string> onChange, int width = 80) {
        var field = new UITextField(Fonts.Regular, width, value) {
            OnInputChange = str => onChange?.Invoke(str)
        };
        return new UIOption(name, field);
    }

    public static UIOption StringOption(string name, string value, Plugin plugin, int width = 80) {
        UIOption o = null;
        var field = new UITextField(Fonts.Regular, width, value) {
            OnInputChange = Locking<string>(str => plugin.SnapshotWeakAndSet(name, str), () => o)
        };
        o = new UIOption(name, field, plugin.GetTooltipFor(name));
        return WithPropListener<string>(name, plugin, o, s => field.UpdateInput(s, false));
    }

    public static UIOption LiteralValueOption<T>(string name, T value, Action<T> onChange, int width = 80) {
        var field = new UIValueTextField<T>(Fonts.Regular, width, value.ToInvString()) {
            OnValidInputChange = v => onChange?.Invoke(v)
        };
        return new UIOption(name, field);
    }

    public static UIOption LiteralValueOption<T>(string name, T value, Plugin plugin, int width = 80) {
        UIOption o = null;
        var field = new UIValueTextField<T>(Fonts.Regular, width, value.ToInvString()) {
            OnValidInputChange = Locking<T>(v => plugin.SnapshotWeakAndSet(name, v), () => o)
        };
        o = new UIOption(name, field, plugin.GetTooltipFor(name));
        return WithPropListener<T>(name, plugin, o, t => field.UpdateInput(t.ToInvString(), false));
    }

    public static UIOption BoolOption(string name, bool value, Action<bool> onChange) {
        var checkbox = new UICheckBox(-1, value) {
            OnPress = b => onChange?.Invoke(b)
        };
        return new UIOption(name, checkbox);
    }

    public static UIOption BoolOption(string name, bool value, Plugin plugin) {
        UIOption o = null;
        var checkbox = new UICheckBox(-1, value) {
            OnPress = Locking<bool>(b => plugin.SnapshotAndSet(name, b), () => o)
        };
        o = new UIOption(name, checkbox, plugin.GetTooltipFor(name));
        return WithPropListener<bool>(name, plugin, o, checkbox.SetChecked);
    }

    public static UIOption ColorOption(string name, Color value, Action<Color> onChange) {
        var colorpicker = new UIColorPicker(value) {
            OnColorChange = (color, _) => onChange?.Invoke(color)
        };
        return new UIOption(name, colorpicker);
    }

    public static UIOption ColorAlphaOption(string name, Color value, float alpha, Action<Color, float> onChange) {
        var colorpicker = new UIColorPicker(value, alpha, alphaWheel: true) {
            OnColorChange = (color, alpha) => onChange?.Invoke(color, alpha)
        };
        return new UIOption(name, colorpicker);
    }

    public static UIOption ColorOption(string name, Color value, Plugin plugin) {
        UIOption o = null;
        var colorpicker = new UIColorPicker(value) {
            OnColorChange = (color, _) => {
                o.locked = true;
                plugin.SnapshotWeakAndSet(name, color);
                o.locked = false;
            }
        };
        o = new UIOption(name, colorpicker, plugin.GetTooltipFor(name));
        return WithPropListener<Color>(name, plugin, o, colorpicker.SetColor);
    }

    public static UIOption DropdownOption<T>(string name, T value, Action<T> onChange) where T : Enum {
        return DropdownOption(name, typeof(T), value, v => onChange((T)v));
    }

    public static UIOption DropdownOption(string name, Type t, object value, Action<object> onChange, string tooltip = default) {
        UIButton button = null;
        button = new UIButton(value + " \uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                var dropdown = UIDropdown.OfEnum(Fonts.Regular, t, it => {
                    onChange?.Invoke(it);
                    button.SetText(it + " \uF036");
                });
                dropdown.Position = button.GetBoundsPos() + Vector2.UnitY * (button.Height + 2) - Editor.Editor.Instance.ToolPanel.GetBoundsPos();
                Editor.Editor.Instance.ToolPanel.Add(dropdown);
            }
        };

        return new UIOption(name, button, tooltip);
    }

    public static UIOption DropdownOption(string name, Type t, object value, Plugin plugin) {
        UIOption o = null;
        UIButton button = null;
        button = new UIButton(value + " \uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                var dropdown = UIDropdown.OfEnum(Fonts.Regular, t, it => {
                    o.locked = true;
                    plugin.SnapshotAndSet(name, it);
                    o.locked = false;
                    button.SetText(it + " \uF036");
                });
                dropdown.Position = button.GetBoundsPos() + Vector2.UnitY * (button.Height + 2) - Editor.Editor.Instance.ToolPanel.GetBoundsPos();
                Editor.Editor.Instance.ToolPanel.Add(dropdown);
            }
        };
        o = new UIOption(name, button, plugin.GetTooltipFor(name));
        return WithPropListener<object>(name, plugin, o, it => button.SetText(it + " \uF036"));
    }

    public static UIOption TilesetDropdownOption(string name, Tileset value, Plugin plugin) {
        UIOption o = null;
        UIButton button = null;
        button = new UIButton(value.Name + " \uF036", Fonts.Regular, 2, 2) {
            OnPress = () => {
                var dropdown = new UIDropdown(Fonts.Regular, Tileset.FgTilesets
                    .Where(ts => ts.Key != '0')
                    .Select(ts => new UIDropdown.DropdownEntry(ts.Name, () => {
                        o.locked = true;
                        plugin.SnapshotAndSet(name, ts.Key);
                        o.locked = false;
                        button.SetText(ts.Name + " \uF036");
                    }) {
                        Icon = ts.Tile.Tiles[0, 0]
                    }).ToArray()) {
                    Position = button.GetBoundsPos() + Vector2.UnitY * (button.Height + 2) - Editor.Editor.Instance.ToolPanel.GetBoundsPos()
                };

                Editor.Editor.Instance.ToolPanel.Add(dropdown);
            }
        };
        o = new UIOption(name, button, plugin.GetTooltipFor(name));
        return WithPropListener<Tileset>(name, plugin, o, ts => button.SetText(ts.Name + " \uF036"));
        // TODO: give the button an icon as well
    }
}