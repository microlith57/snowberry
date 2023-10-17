using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Celeste;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Snowberry.UI;
using Snowberry.UI.Controls;
using Snowberry.UI.Layout;

namespace Snowberry.Mixer;

using FmodResult = RESULT;

public class Mixer : UIScene {

    private const int ScrollpaneWidth = 320;
    private const int HeaderHeight = 40;

    private readonly List<(EventInstance, UIElement)> Playing = new();
    private UIScrollPane SoundsPane;

    protected override void BeginContent() {
        Audio.SetMusic(null);
        Audio.SetAmbience(null);

        MixerUi();
    }

    public override void End() {
        base.End();

        foreach((EventInstance instance, UIElement) values in Playing)
            Audio.Stop(values.instance);
    }

    private void MixerUi() {
        UIElement topBar = new() {
            Background = Color.DarkBlue * 0.5f,
            Width = UI.Width,
            Height = HeaderHeight
        };

        topBar.AddRight(new UILabel("snowberry", Fonts.Regular, 2), new(8, 8));
        topBar.AddRight(new UILabel(Dialog.Clean("SNOWBERRY_MIXER_TITLE"), Fonts.Bold, 2) {
            FG = Color.Blue
        }, new(8, 8));

        UI.Add(topBar);

        List<(string name, List<EventDescription> events)> audio = GetBigAudioList();
        var bigScrollPane = new UIScrollPane {
            Position = new(0, HeaderHeight),
            Width = ScrollpaneWidth,
            Height = UI.Height - HeaderHeight,
            Background = Color.DarkCyan * 0.5f
        };
        var bigTree = new UITree(new UILabel(Dialog.Clean("SNOWBERRY_MIXER_TREEHEADING")));

        foreach ((string name, List<EventDescription> events) bank in audio) {
            var bankTree = new UITree(new UILabel(bank.name));
            foreach (EventDescription e in bank.events.OrderBy(Audio.GetEventName)) {
                UIElement track = new();
                track.Add(new UILabel(Audio.GetEventName(e)));
                track.AddRight(new UIButton(UIScene.ActionbarAtlas.GetSubtexture(9, 101, 6, 6), 3, 4) {
                    OnPress = () => StartPlaying(e)
                }, new(3, -2));
                track.CalculateBounds();
                bankTree.Add(track);
            }
            bankTree.Layout();
            bigTree.Add(bankTree);
        }

        bigTree.Layout();
        bigScrollPane.Add(bigTree);
        UI.Add(bigScrollPane);

        SoundsPane = new UIScrollPane {
            Position = new(ScrollpaneWidth, HeaderHeight),
            Width = UI.Width - ScrollpaneWidth,
            Height = UI.Height - HeaderHeight,
            TopPadding = 5
        };
        UI.Add(SoundsPane);
    }

    private List<(string name, List<EventDescription> events)> GetBigAudioList() {
        IEnumerable<Bank> banks = Audio.Banks.Banks.Values.Union(Audio.Banks.ModCache.Values).ToList();
        List<(string, List<EventDescription>)> banksList = new(banks.Count());
        foreach (Bank b in banks) {
            if (!b.isValid() || b.getEventList(out EventDescription[] events) != FmodResult.OK)
                continue;

            banksList.Add((Audio.GetBankName(b), (
                from e in events
                where e.isValid()
                select e
            ).ToList()));
        }

        return banksList;
    }

    private void StartPlaying(EventDescription e) {
        if (e.createInstance(out var instance) != FmodResult.OK)
            return;
        instance.start();

        UIElement eventPanel = new(){
            Background = Color.DarkGreen
        };
        eventPanel.AddBelow(new UILabel(Audio.GetEventName(e)), new(5));
        e.getParameterCount(out int count);
        IEnumerable<PARAMETER_DESCRIPTION> parameters = Enumerable.Range(0, count)
            .Select(x => {
                e.getParameterByIndex(x, out var p);
                return p;
            })
            .OrderBy(x => x.name);
        foreach (var param in parameters) {
            UIElement local = new(); UISlider slider;
            local.AddRight(new UILabel(param.name), new(5));
            local.AddRight(slider = new UISlider {
                Value = param.defaultvalue,
                Min = param.minimum,
                Max = param.maximum,
                Width = 140,
                OnInputChanged = v => instance.setParameterValue(param.name, v)
            }, new(7, 2));
            local.AddRight(new UILabel(() => slider.Value.ToString(CultureInfo.CurrentCulture)), new(10, 5));
            local.CalculateBounds();
            eventPanel.AddBelow(local, new(5));
        }
        eventPanel.AddBelow(new UIButton(Dialog.Clean("SNOWBERRY_MIXER_STOP"), Fonts.Regular, 2, 2) {
            OnPress = () => {
                Playing.RemoveAll(x => x.Item1 == instance);
                eventPanel.RemoveSelf();

                Audio.Stop(instance);
                Relist();
            }
        }, new(5));
        eventPanel.AddBelow(new UIButton(Dialog.Clean("SNOWBERRY_MIXER_REPLAY"), Fonts.Regular, 2, 2) {
            OnPress = () => instance.start()
        }, new(5));
        eventPanel.CalculateBounds();
        eventPanel.Width += 5;
        eventPanel.Height += 5;

        SoundsPane.AddBelow(eventPanel, new(5));
        Playing.Add((instance, eventPanel));
    }

    private void Relist() {
        int y = 5;
        foreach ((EventInstance, UIElement elem) value in Playing) {
            value.elem.Position.Y = y;
            y += value.elem.Height + 5;
        }
    }
}