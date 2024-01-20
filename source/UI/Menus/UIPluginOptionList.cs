using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Snowberry.Editor;
using Snowberry.UI.Controls;

namespace Snowberry.UI.Menus;

public class UIPluginOptionList : UIElement {
    public class UIOption : UIElement {
        public readonly UIElement Input;
        private readonly UILabel Label;

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
        foreach (var option in Plugin.Info.Options) {
            object value = option.Value.GetValue(Plugin);

            if (Plugin.CreateOptionUi(option.Key) is (UIElement i, var height)) {
                UIOption ui = new UIOption(option.Key, i, Plugin.GetTooltipFor(option.Key));
                Add(ui);
                ui.Position.Y = l;
                l += height;
            }
            // TODO: this is kind of silly
            else if (option.Value.FieldType == typeof(bool)) {
                UIOption ui;
                Add(ui = BoolOption(option.Key, (bool)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (option.Value.FieldType == typeof(Color)) {
                UIOption ui;
                Add(ui = ColorOption(option.Key, value is Color c ? c : Color.White, Plugin));
                ui.Position.Y = l;
                l += 91;
            } else if (option.Value.FieldType == typeof(int)) {
                UIOption ui;
                Add(ui = LiteralValueOption(option.Key, (int)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (option.Value.FieldType == typeof(long)) {
                UIOption ui;
                Add(ui = LiteralValueOption(option.Key, (long)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (option.Value.FieldType == typeof(float)) {
                UIOption ui;
                Add(ui = LiteralValueOption(option.Key, (float)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (option.Value.FieldType == typeof(double)) {
                UIOption ui;
                Add(ui = LiteralValueOption(option.Key, (double)value, Plugin));
                ui.Position.Y = l;
                l += spacing;
            } else if (option.Value.FieldType == typeof(Tileset)) {
                UIOption ui;
                Add(ui = TilesetDropdownOption(option.Key, (Tileset)value, Plugin));
                ui.Position.Y = l;
                l += spacing + 6;
            } else if (option.Value.FieldType.IsEnum) {
                UIOption ui;
                Add(ui = DropdownOption(option.Key, option.Value.FieldType, value, Plugin));
                ui.Position.Y = l;
                l += spacing + 6;
            } else {
                UIOption ui;
                Add(ui = StringOption(option.Key, value?.ToString() ?? "", Plugin));
                ui.Position.Y = l;
                l += spacing;
            }
            Height = l;
            Width = Math.Max(Children.Max(k => k.Width), Width);
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