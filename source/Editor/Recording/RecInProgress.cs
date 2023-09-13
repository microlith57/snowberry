using System.Collections.Generic;
using Session = Celeste.Session;

namespace Snowberry.Editor.Recording;

public static class RecInProgress {

    public static readonly List<Recorder> Recorders = new();

    private static bool recInProgress = false;

    public static void BeginRecording() {
        DiscardRecording();

        foreach (var sup in Recorder.Recorders)
            Recorders.Add(sup());

        recInProgress = true;
    }

    public static void FinishRecording() {
        recInProgress = false;
    }

    public static void DiscardRecording() {
        // TODO: dispose anything that might need disposing?
        Recorders.Clear();
        recInProgress = false;
    }

    // hooks

    public static void Load() {
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Celeste.LevelEnter.ctor += OnBeginLevelEnter;
    }

    public static void Unload() {
        On.Celeste.Level.Update -= OnLevelUpdate;
        On.Celeste.LevelEnter.ctor -= OnBeginLevelEnter;
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Celeste.Level self) {
        orig(self);
        if(recInProgress)
            foreach(Recorder r in Recorders)
                r.UpdateInGame(self);
    }

    private static void OnBeginLevelEnter(On.Celeste.LevelEnter.orig_ctor orig, Celeste.LevelEnter a, Session b, bool c) {
        orig(a, b, c);
        DiscardRecording();
    }
}