using System;
using Celeste;

namespace Snowberry;

public static class Events {

    public static event Action<Editor.Map, MapData> OnImportMap;
    internal static void ImportMap(Editor.Map map, MapData data)
        => OnImportMap.Invoke(map, data);

    public static event Action<Editor.Map, BinaryPacker.Element> OnExportMap;
    internal static void ExportMap(Editor.Map map, BinaryPacker.Element data)
        => OnExportMap.Invoke(map, data);

    public static event Action<Editor.Map, Editor.Editor.BufferCamera> OnRenderMap;
    internal static void RenderMap(Editor.Map map, Editor.Editor.BufferCamera camera)
        => OnRenderMap.Invoke(map, camera);

    public static event Action<Editor.Map> OnPostRenderMap;
    internal static void PostRenderMap(Editor.Map map)
        => OnPostRenderMap.Invoke(map);

    public static event Action<Editor.Map, Editor.Editor.BufferCamera> OnHQRenderMap;
    internal static void HQRenderMap(Editor.Map map, Editor.Editor.BufferCamera camera)
        => OnHQRenderMap.Invoke(map, camera);

}
