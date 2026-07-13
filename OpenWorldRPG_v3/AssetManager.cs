using Raylib_cs;
using System.Collections.Generic;

namespace OpenWorldRPG
{
    public static class AssetManager
    {
        static readonly Dictionary<string, Texture2D> _textures = new();

        // Call AFTER Raylib.InitWindow(), on the main thread only.
        public static void Load(string key, string path)
        {
            if (_textures.ContainsKey(key))
            {
                System.Console.WriteLine($"[ASSET] '{key}' already loaded — SKIPPING {path}");   // NEW
                return;
            }

            _textures[key] = Raylib.LoadTexture(path);
            if (_textures[key].Id == 0)                                                        
                System.Console.WriteLine($"[ASSET] *** FAILED to load '{key}' — check path: {path}");   

        }

        public static Texture2D Get(string key)
        {
            if (_textures.TryGetValue(key, out var t)) return t;
            if (_textures.TryGetValue("villager", out var v)) return v;
            return default;   // empty texture; draws nothing rather than crashing
        }

        public static bool Has(string key) => _textures.ContainsKey(key);

        // Call ONCE at shutdown, before Raylib.CloseWindow().
        public static void UnloadAll()
        {
            foreach (var tex in _textures.Values)
                Raylib.UnloadTexture(tex);
            _textures.Clear();
        }
    }
}