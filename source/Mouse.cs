using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry;

public static class Mouse {
    public static bool InBounds { get; internal set; }
    public static Vector2 Screen { get; internal set; }
    public static Vector2 ScreenLast { get; internal set; }

    public static Vector2 World { get; internal set; }
    public static Vector2 WorldLast { get; internal set; }

    public static Vector2 PendingWarp { get; internal set; }
    public static Vector2 ScreenAfterWarp {
        get => Screen + PendingWarp;
        set => PendingWarp = value - Screen;
    }

    public static Vector2 Warp(Vector2 screenPos) {
        var delta = screenPos - Screen;
        PendingWarp += delta;
        return delta;
    }

    public static Vector2 Wrap(Rectangle screenRect, int padding = 5) {
        if (screenRect.Width <= padding * 2 || screenRect.Height <= padding * 2)
            return Vector2.Zero;

        var dest = ScreenAfterWarp;

        dest.X = Util.Wrap(dest.X, screenRect.Left + padding, screenRect.Right - padding);
        dest.Y = Util.Wrap(dest.Y, screenRect.Top + padding, screenRect.Bottom - padding);

        return Warp(dest);
    }

    public static bool IsFocused { get; internal set; }

    public static DateTime LastClick { get; internal set; }
    public static bool IsDoubleClick => IsFocused && MInput.Mouse.PressedLeftButton && DateTime.Now < LastClick.AddMilliseconds(200);
}