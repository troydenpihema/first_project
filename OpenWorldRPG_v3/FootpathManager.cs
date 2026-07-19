using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    /// <summary>
    /// A single footpath segment — narrow dirt/stone trail.
    /// Horizontal or vertical, biome-themed surface colour, optional trail markers.
    /// </summary>
    public class Footpath
    {
        public Vector2 Start;
        public Vector2 End;
        public float Width;           // half-extent from centreline (full path = 2*Width)
        public bool Horizontal;
        public bool TrailMarkers = true;
        public float MarkerSpacing = 3000f;

        // Biome-themed defaults — override per path
        public Color Surface = new Color((byte)120, (byte)95, (byte)60, (byte)255);   // dirt brown
        public Color Edge    = new Color((byte)90,  (byte)70, (byte)45, (byte)180);   // darker edge
        public Color MarkerCol = new Color((byte)160, (byte)140, (byte)100, (byte)255);

        public Footpath(Vector2 start, Vector2 end, float width = 40f)
        {
            Start = start;
            End = end;
            Width = width;
            Horizontal = MathF.Abs(end.X - start.X) >= MathF.Abs(end.Y - start.Y);
        }

        float MinX => MathF.Min(Start.X, End.X);
        float MaxX => MathF.Max(Start.X, End.X);
        float MinY => MathF.Min(Start.Y, End.Y);
        float MaxY => MathF.Max(Start.Y, End.Y);

        public Rectangle SurfaceRect => Horizontal
            ? new Rectangle(MinX, Start.Y - Width, MaxX - MinX, Width * 2f)
            : new Rectangle(Start.X - Width, MinY, Width * 2f, MaxY - MinY);

        // Edge rect is slightly wider for the border effect
        public Rectangle EdgeRect
        {
            get
            {
                float pad = Width + 8f;
                return Horizontal
                    ? new Rectangle(MinX, Start.Y - pad, MaxX - MinX, pad * 2f)
                    : new Rectangle(Start.X - pad, MinY, pad * 2f, MaxY - MinY);
            }
        }

        public bool OnSurface(Vector2 p) => PointInRect(p, SurfaceRect);
        public bool OnFootprint(Vector2 p) => PointInRect(p, EdgeRect);

        static bool PointInRect(Vector2 p, Rectangle r) =>
            p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;

        public void DrawEdge()    => Raylib.DrawRectangleRec(EdgeRect, Edge);
        public void DrawSurface() => Raylib.DrawRectangleRec(SurfaceRect, Surface);

        public void DrawMarkers()
        {
            if (!TrailMarkers) return;
            if (Horizontal)
            {
                for (float x = MinX + MarkerSpacing; x < MaxX; x += MarkerSpacing)
                {
                    // small wooden post on each side
                    Raylib.DrawRectangle((int)x - 2, (int)(Start.Y - Width - 12), 4, 14, MarkerCol);
                    Raylib.DrawRectangle((int)x - 2, (int)(Start.Y + Width - 2),  4, 14, MarkerCol);
                }
            }
            else
            {
                for (float y = MinY + MarkerSpacing; y < MaxY; y += MarkerSpacing)
                {
                    Raylib.DrawRectangle((int)(Start.X - Width - 12), (int)y - 2, 14, 4, MarkerCol);
                    Raylib.DrawRectangle((int)(Start.X + Width - 2),  (int)y - 2, 14, 4, MarkerCol);
                }
            }
        }
    }

    public static class FootpathManager
    {
        public static readonly List<Footpath> Paths = new();

        // ── Preset colour palettes per biome ──
        static readonly Color SnowSurface  = new Color((byte)200, (byte)210, (byte)220, (byte)255);
        static readonly Color SnowEdge     = new Color((byte)180, (byte)195, (byte)210, (byte)180);
        static readonly Color SnowMarker   = new Color((byte)140, (byte)155, (byte)175, (byte)255);

        static readonly Color VolcanoSurface = new Color((byte)65, (byte)45, (byte)35, (byte)255);
        static readonly Color VolcanoEdge    = new Color((byte)50, (byte)30, (byte)20, (byte)180);
        static readonly Color VolcanoMarker  = new Color((byte)160, (byte)80, (byte)30, (byte)255);

        static readonly Color ForestSurface = new Color((byte)75, (byte)65, (byte)40, (byte)255);
        static readonly Color ForestEdge    = new Color((byte)55, (byte)50, (byte)30, (byte)180);
        static readonly Color ForestMarker  = new Color((byte)90, (byte)120, (byte)60, (byte)255);

        static readonly Color MountainSurface = new Color((byte)130, (byte)125, (byte)115, (byte)255);
        static readonly Color MountainEdge    = new Color((byte)110, (byte)105, (byte)95,  (byte)180);
        static readonly Color MountainMarker  = new Color((byte)160, (byte)155, (byte)145, (byte)255);

        static readonly Color SwampSurface = new Color((byte)70, (byte)80, (byte)50, (byte)255);
        static readonly Color SwampEdge    = new Color((byte)55, (byte)65, (byte)40, (byte)180);
        static readonly Color SwampMarker  = new Color((byte)100, (byte)110, (byte)70, (byte)255);

        static readonly Color DesertSurface = new Color((byte)185, (byte)165, (byte)110, (byte)255);
        static readonly Color DesertEdge    = new Color((byte)165, (byte)145, (byte)95,  (byte)180);
        static readonly Color DesertMarker  = new Color((byte)200, (byte)180, (byte)130, (byte)255);

        static readonly Color BeachSurface = new Color((byte)195, (byte)185, (byte)150, (byte)255);
        static readonly Color BeachEdge    = new Color((byte)175, (byte)165, (byte)135, (byte)180);
        static readonly Color BeachMarker  = new Color((byte)160, (byte)140, (byte)100, (byte)255);

        /// <summary>Add a plain footpath and return it for tweaking.</summary>
        public static Footpath Add(Vector2 start, Vector2 end, float width = 40f)
        {
            var p = new Footpath(start, end, width);
            Paths.Add(p);
            return p;
        }

        /// <summary>Add a biome-themed footpath with preset colours.</summary>
        public static Footpath AddBiome(Vector2 start, Vector2 end, string biome, float width = 40f)
        {
            var p = new Footpath(start, end, width);
            switch (biome)
            {
                case "SNOW":    p.Surface = SnowSurface;    p.Edge = SnowEdge;    p.MarkerCol = SnowMarker;    break;
                case "VOLCANO": p.Surface = VolcanoSurface; p.Edge = VolcanoEdge; p.MarkerCol = VolcanoMarker; break;
                case "FOREST":  p.Surface = ForestSurface;  p.Edge = ForestEdge;  p.MarkerCol = ForestMarker;  break;
                case "MOUNTAIN":p.Surface = MountainSurface;p.Edge = MountainEdge;p.MarkerCol = MountainMarker;break;
                case "SWAMP":   p.Surface = SwampSurface;   p.Edge = SwampEdge;   p.MarkerCol = SwampMarker;   break;
                case "DESERT":  p.Surface = DesertSurface;  p.Edge = DesertEdge;  p.MarkerCol = DesertMarker;  break;
                case "BEACH":   p.Surface = BeachSurface;   p.Edge = BeachEdge;   p.MarkerCol = BeachMarker;   break;
            }
            Paths.Add(p);
            return p;
        }

        // ── Queries (mirror RoadManager API) ──

        public static bool IsOnSurface(Vector2 pos)
        {
            foreach (var p in Paths) if (p.OnSurface(pos)) return true;
            return false;
        }

        public static bool IsOnFootprint(Vector2 pos)
        {
            foreach (var p in Paths) if (p.OnFootprint(pos)) return true;
            return false;
        }

        public static bool IsNearPath(Vector2 pos, float buffer = 60f)
        {
            foreach (var p in Paths)
            {
                Rectangle fp = p.EdgeRect;
                if (pos.X >= fp.X - buffer && pos.X <= fp.X + fp.Width + buffer &&
                    pos.Y >= fp.Y - buffer && pos.Y <= fp.Y + fp.Height + buffer) return true;
            }
            return false;
        }

        // ── Drawing (culled, call inside BeginMode2D) ──

        public static void DrawAll(Vector2 camTarget, int screenW, int screenH, float zoom = 1f)
        {
            float halfW = screenW / zoom, halfH = screenH / zoom;
            float vl = camTarget.X - halfW, vr = camTarget.X + halfW;
            float vt = camTarget.Y - halfH, vb = camTarget.Y + halfH;

            bool Visible(Footpath p)
            {
                Rectangle fp = p.EdgeRect;
                return !(fp.X + fp.Width < vl || fp.X > vr || fp.Y + fp.Height < vt || fp.Y > vb);
            }

            foreach (var p in Paths) if (Visible(p)) p.DrawEdge();
            foreach (var p in Paths) if (Visible(p)) p.DrawSurface();
            foreach (var p in Paths) if (Visible(p)) p.DrawMarkers();
        }

        // ── Minimap & World Map (mirror RoadManager) ──

        public static void DrawOnWorldMap(int cx, int cy, float scale, int mapX, int mapY, int mapW, int mapH)
        {
            var col = new Color((byte)120, (byte)100, (byte)70, (byte)200);
            foreach (var p in Paths)
            {
                Rectangle fp = p.SurfaceRect;
                int x = cx + (int)(fp.X * scale);
                int y = cy + (int)(fp.Y * scale);
                int w = Math.Max(1, (int)(fp.Width  * scale));
                int h = Math.Max(1, (int)(fp.Height * scale));
                int cxp = Math.Clamp(x, mapX, mapX + mapW);
                int cyp = Math.Clamp(y, mapY, mapY + mapH);
                Raylib.DrawRectangle(cxp, cyp,
                    Math.Clamp(x + w, mapX, mapX + mapW) - cxp,
                    Math.Clamp(y + h, mapY, mapY + mapH) - cyp, col);
            }
        }

        public static void DrawOnMinimap(int cx, int cy, float scale, Vector2 playerPos)
        {
            var col = new Color((byte)120, (byte)100, (byte)70, (byte)200);
            foreach (var p in Paths)
            {
                Rectangle fp = p.SurfaceRect;
                int x = cx + (int)((fp.X - playerPos.X) * scale);
                int y = cy + (int)((fp.Y - playerPos.Y) * scale);
                Raylib.DrawRectangle(x, y,
                    Math.Max(1, (int)(fp.Width * scale)),
                    Math.Max(1, (int)(fp.Height * scale)), col);
            }
        }
    }
}
