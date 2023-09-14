using System.Collections.Generic;
using Session = Celeste.Session;

namespace Snowberry.Editor.Recording;

public static class RecInProgress {

    public static readonly List<Recorder> Recorders = new();

    private static bool recInProgress = false;
    // for tracking time across debugging, other forms of teleports
    // curLevelTime is kept in sync with the current level's TimeActive. if it becomes greater, it's added to timeAccum
    private static float timeAccum, curLevelTime = 0;

    public static void BeginRecording() {
        DiscardRecording();

        foreach (var sup in Recorder.Recorders)
            Recorders.Add(sup());

        recInProgress = true;
    }

    public static void FinishRecording() {
        foreach(Recorder r in Recorders)
            r.FinalizeRecording();

        recInProgress = false;
    }

    public static void DiscardRecording() {
        // TODO: dispose anything that might need disposing?
        Recorders.Clear();
        recInProgress = false;
        timeAccum = curLevelTime = 0;
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

        if(self.TimeActive < curLevelTime)
            timeAccum += curLevelTime;
        curLevelTime = self.TimeActive;

        if(recInProgress)
            foreach(Recorder r in Recorders)
                r.UpdateInGame(self, timeAccum + self.TimeActive);
    }

    private static void OnBeginLevelEnter(On.Celeste.LevelEnter.orig_ctor orig, Celeste.LevelEnter a, Session b, bool c) {
        orig(a, b, c);
        if (b != Editor.PlaytestSession)
            DiscardRecording();
    }
}