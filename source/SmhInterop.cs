using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using MonoMod.Utils;

namespace Snowberry;

public static class SmhInterop {

    private static readonly EverestModuleMetadata SmhMeta = new() { Name = "SkinModHelper", Version = new(0, 6, 1) };

    public static List<(string id, string key)> PlayerSkinIds = new();

    public static void LoadGraphics(){
        PlayerSkinIds.Clear();

        if(Everest.Loader.TryGetDependency(SmhMeta, out var smh)) {
            DynamicData smhData = new DynamicData(smh);
            // Adapted from https://github.com/bigkahuna443/SkinModHelper/blob/63411aab060d9f624821d5082109d9155ec63648/Code/SkinModHelperModule.cs#L266 under MIT
            foreach(DictionaryEntry config in smhData.Get<IDictionary>("skinConfigs"))
                if(config.Key is string id and not "Default"){
                    smhData.Invoke("CombineSpriteBanks", GFX.SpriteBank, id, $"Graphics/{id.Replace('_', '/')}/Sprites.xml");
                    if(GFX.SpriteBank.Has($"player_{id}"))
                        PlayerSkinIds.Add((id, new DynamicData(config.Value).Get<string>("SkinDialogKey")));
                }
        }
    }

    public static void RunWithSkin(Action a, string skin = "Default"){
        if(Everest.Loader.TryGetDependency(SmhMeta, out var smh)){
            DynamicData settingsData = new DynamicData(smh._Settings);
            string old = settingsData.Invoke<string>("get_SelectedSkinMod");
            settingsData.Invoke("set_SelectedSkinMod", skin);
            a();
            settingsData.Invoke("set_SelectedSkinMod", old);
        }else
            a();
    }
}