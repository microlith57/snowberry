using Celeste.Mod;

namespace Snowberry;

public class SnowberrySettings : EverestModuleSettings {
    [SettingName("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN")]
    [SettingSubText("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN_SUB")]
    public bool MiddleClickPan { get; set; } = true;

    [SettingName("SNOWBERRY_SETTINGS_FANCY_RENDER")]
    [SettingSubText("SNOWBERRY_SETTINGS_FANCY_RENDER_SUB")]
    public bool FancyRender { get; set; } = true;

    [SettingName("SNOWBERRY_SETTINGS_SG_PREVIEW")]
    [SettingSubText("SNOWBERRY_SETTINGS_SG_PREVIEW_SUB")]
    public bool StylegroundsPreview { get; set; } = true;

    [SettingName("SNOWBERRY_SETTINGS_AGGRESSIVE_SNAP")]
    [SettingSubText("SNOWBERRY_SETTINGS_AGGRESSIVE_SNAP_SUB")]
    public bool AggressiveSnap { get; set; } = false;
}