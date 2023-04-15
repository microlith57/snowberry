namespace Snowberry.Editor.Triggers;

[Plugin("eventTrigger")]
[Plugin("creditsTrigger")]
public class Plugin_EventTrigger : Trigger {
    [Option("event")] public string Event = "";

    public new static void AddPlacements() {
        Placements.Create("Event Trigger", "eventTrigger");
        Placements.Create("Credits Trigger", "creditsTrigger");
    }
}