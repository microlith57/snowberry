using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry;

public static class UndoRedo{

    // thanks C#!! very cool!!
    // type erased form of Snapshotter to allow hetrogenous collections
    public interface Snapshotter{
        public object SnapshotRaw();
        public void ApplyRaw(object o);
    }

    public interface Snapshotter<T> : Snapshotter {
        public T Snapshot();
        public void Apply(T t);

        object Snapshotter.SnapshotRaw() => Snapshot();
        void Snapshotter.ApplyRaw(object o) => Apply((T)o);
    }

    public class FlimsySnapshotter(Action apply) : Snapshotter {
        public object SnapshotRaw() => null;
        public void ApplyRaw(object o) => apply();
    }

    public static Snapshotter OfAction(Action a) => new FlimsySnapshotter(a);

    public static IEnumerable<Snapshotter> Unique(IEnumerable<Snapshotter> snapshotters) => snapshotters.Distinct();

    public class EditorAction{
        public string Name;
        public List<(Snapshotter snapshotter, object before, object after)> States = new();
        public bool Weak;

        public EditorAction(string name, bool weak = false){
            Name = name;
            Weak = weak;
        }

        public void Undo(){
            foreach(var state in States)
                state.snapshotter.ApplyRaw(state.before);
        }

        public void Redo(){
            foreach(var state in States)
                state.snapshotter.ApplyRaw(state.after);
        }

        public bool HasMatching(Predicate<Snapshotter> p){
            foreach((Snapshotter sn, var _, var _) in States)
                if(p(sn))
                    return true;
            return false;
        }

        internal void BackupRedoState(){
            var old = States;
            States = new(old.Count);
            foreach (var state in old)
                States.Add((state.snapshotter, state.before, state.snapshotter.SnapshotRaw()));
        }
    }

    public static event Action OnChange;

    // the list of actions that have been completed and possibly undone
    // does *not* include the current in-progress action
    private static readonly List<EditorAction> ActionLog = new();
    // the index into ActionLog that describes the current Editor state
    // if actions are undone, this moves back; without undoing, this is always `ActionLog.Count - 1`
    private static int CurActionIndex = -1;
    // the current in-progress action
    private static EditorAction InProgress = null;

    public static void Reset() {
        ActionLog.Clear();
        CurActionIndex = -1;
        InProgress = null;
    }

    public static void BeginAction(string name, params Snapshotter[] snapshotters) {
        BeginAction(name, snapshotters.AsEnumerable());
    }

    // this is kind of annoying - really i'd like to have `params T[] ts, U u = default` where the latter is a kwarg
    public static void BeginWeakAction(string name, params Snapshotter[] snapshotters) {
        BeginAction(name, snapshotters.AsEnumerable(), true);
    }

    public static void BeginAction(string name, IEnumerable<Snapshotter> snapshotters, bool weak = false) {
        snapshotters = Unique(snapshotters);
        // either you have some undone actions (and nothing in progress)
        // or you have an in progress action (and are at the end of the log)
        // so the order these happen doesn't matter
        TrimLog();
        if(InProgress != null)
            if(InProgress.Weak)
                CompleteAction();
            else
                InProgress.Undo();

        InProgress = new EditorAction(name, weak);
        foreach(var snapshotter in snapshotters)
            InProgress.States.Add((snapshotter, snapshotter.SnapshotRaw(), null));

        TriggerChange();
    }

    public static void CompleteAction(){
        if(InProgress == null)
            throw new ArgumentException("CompleteAction can only be called while an action is in-progress!");

        InProgress.BackupRedoState();
        ActionLog.Add(InProgress);
        CurActionIndex++;
        Snowberry.LogInfo("completed: " + InProgress.Name);
        InProgress = null;

        TriggerChange();
    }

    private static void TrimLog(){
        if(CurActionIndex != ActionLog.Count - 1)
            ActionLog.RemoveRange(CurActionIndex + 1, ActionLog.Count - CurActionIndex - 1);
            // no need to set CurActionIndex since we've just forced ActionLog to match
    }

    public static void Undo(){
        if (InProgress != null)
            InProgress.Undo();
        else if(CurActionIndex > -1){
            ActionLog[CurActionIndex].Undo();
            Snowberry.LogInfo("undid: " + ActionLog[CurActionIndex].Name);
            CurActionIndex--;
        }
        TriggerChange();
    }

    public static void Redo(){
        if(CurActionIndex < ActionLog.Count - 1){
            CurActionIndex++;
            ActionLog[CurActionIndex].Redo();
            Snowberry.LogInfo("redid: " + ActionLog[CurActionIndex].Name);
        }
        TriggerChange();
    }

    // it would kind of suck if anyone modified this improperly
    public static IReadOnlyList<EditorAction> ViewLog() => ActionLog.AsReadOnly();
    public static EditorAction ViewInProgress() => InProgress;
    public static int ViewCurActionIdx() => CurActionIndex;

    private static void TriggerChange() => OnChange?.Invoke();
}