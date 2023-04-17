using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor;

public class EntitySelection {
    public class Selection {
        public int Index;
        public Entity Entity;

        public Selection(Entity entity, int i) {
            Index = i;
            Entity = entity;
        }

        // -1 = entity itself
        public Rectangle Rect => Entity.SelectionRectangles[Index + 1];
    }

    public readonly Entity Entity;
    public readonly List<Selection> Selections;

    public EntitySelection(Entity entity, List<Selection> selection) {
        Entity = entity;
        Selections = selection;
    }

    public bool Contains(Point p) {
        return Selections.Any(s => s.Rect.Contains(p));
    }

    public void Move(Vector2 amount) {
        foreach (Selection s in Selections) {
            if (s.Index < 0)
                Entity.Move(amount);
            else
                Entity.MoveNode(s.Index, amount);
        }
    }

    public void SetPosition(Vector2 position, int i) {
        if (i < 0)
            Entity.SetPosition(position);
        else
            Entity.SetNode(i, position);
    }

    public void SetWidth(int width) {
        Entity.SetWidth(width);
    }

    public void SetHeight(int height) {
        Entity.SetHeight(height);
    }

    public override bool Equals(object obj) {
        return obj is EntitySelection s && s.Entity.Equals(Entity) && s.Selections.All(it => Selections.Any(x => x.Index == it.Index));
    }

    public override int GetHashCode() {
        return Entity.GetHashCode() ^ Selections.GetHashCode();
    }
}