using System.Collections.Generic;
using System.Linq;
using Celeste;
using FMOD;
using FMOD.Studio;
using Snowberry.UI;

namespace Snowberry.Mixer;

using FmodResult = RESULT;

public class Mixer : UIScene {

    protected override void BeginContent() {
        Audio.SetMusic(null);
        Audio.SetAmbience(null);

        MixerUi();
    }

    private void MixerUi() {
        List<(string name, List<string> events)> audio = GetBigAudioList();
        var bigScrollPane = new UIScrollPane {
            Width = UI.Width,
            Height = UI.Height
        };
        var bigTree = new UITree(new UILabel("all music"));

        foreach ((string name, List<string> events) bank in audio) {
            var bankTree = new UITree(new UILabel(bank.name));
            foreach (string e in bank.events) {
                UIElement track = new();
                track.Add(new UILabel(e));
                track.AddRight(new UIButton(UIScene.ActionbarAtlas.GetSubtexture(9, 101, 6, 6), 3, 4) {
                    OnPress = () => Audio.Play(e)
                });
                track.CalculateBounds();
                bankTree.Add(track);
            }
            bankTree.Layout();
            bigTree.Add(bankTree);
        }

        bigTree.Layout();
        bigScrollPane.Add(bigTree);
        UI.Add(bigScrollPane);
    }

    private List<(string name, List<string> events)> GetBigAudioList() {
        IEnumerable<Bank> banks = Audio.Banks.Banks.Values.Union(Audio.Banks.ModCache.Values).ToList();
        List<(string, List<string>)> banksList = new(banks.Count());
        foreach (Bank b in banks) {
            if (!b.isValid() || b.getEventList(out EventDescription[] events) != FmodResult.OK)
                continue;

            banksList.Add((Audio.GetBankName(b), (
                from e in events
                where e.isValid()
                select Audio.GetEventName(e)
            ).ToList()));
        }

        return banksList;
    }
}