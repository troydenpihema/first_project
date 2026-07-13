using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    // One road segment. Horizontal or vertical, defined by a centreline + lane width.
    // Sidewalks and centre markings are derived automatically from that geometry.
    public class Road
    {
        public Vector2 Start;         // centreline start (world coords)
        public Vector2 End;           // centreline end
        public float LaneWidth;       // drivable half-extent from centreline (so full road = 2*LaneWidth)
        public float SidewalkWidth;   // walkable strip outside each drivable edge
        public bool Horizontal;       
        public bool Streetlights = true;      
        public float LightSpacing = 2000f;
        public IEnumerable<Vector2> LightPositions()
        {
            if (!Streetlights) yield break;
            float off = LaneWidth + SidewalkWidth * 0.5f;   // sit in the sidewalk strip
            if (Horizontal)
            {
                for (float x = MinX; x <= MaxX; x += LightSpacing)
                {
                    yield return new Vector2(x, Start.Y - off);
                    yield return new Vector2(x + LightSpacing * 0.5f, Start.Y + off);
                }
            }
            else
            {
                for (float y = MinY; y <= MaxY; y += LightSpacing)
                {
                    yield return new Vector2(Start.X - off, y);
                    yield return new Vector2(Start.X + off, y + LightSpacing * 0.5f);
                }
            }
        }

        // Colours (sensible defaults; override per-road if you like)
        public Color Surface  = new Color((byte)52, (byte)52, (byte)56, (byte)255);
        public Color Sidewalk = new Color((byte)155, (byte)155, (byte)140, (byte)255);
        public Color Marking  = new Color((byte)235, (byte)225, (byte)120, (byte)255);
        public Road(Vector2 start, Vector2 end, float laneWidth = 100f, float sidewalkWidth = 40f)
        {
            Start = start;
            End = end;
            LaneWidth = laneWidth;
            SidewalkWidth = sidewalkWidth;
            Horizontal = MathF.Abs(end.X - start.X) >= MathF.Abs(end.Y - start.Y);
        }

        // ── Bounds helpers (axis-aligned; roads are straight H or V) ──
        float MinX => MathF.Min(Start.X, End.X);
        float MaxX => MathF.Max(Start.X, End.X);
        float MinY => MathF.Min(Start.Y, End.Y);
        float MaxY => MathF.Max(Start.Y, End.Y);

        // Drivable rectangle (surface only — used for the on-road speed effect)
        public Rectangle SurfaceRect => Horizontal
            ? new Rectangle(MinX, Start.Y - LaneWidth, MaxX - MinX, LaneWidth * 2f)
            : new Rectangle(Start.X - LaneWidth, MinY, LaneWidth * 2f, MaxY - MinY);

        // Full footprint including sidewalks (used for the "safe zone" / walkable query)
        public Rectangle FootprintRect
        {
            get
            {
                float pad = LaneWidth + SidewalkWidth;
                return Horizontal
                    ? new Rectangle(MinX, Start.Y - pad, MaxX - MinX, pad * 2f)
                    : new Rectangle(Start.X - pad, MinY, pad * 2f, MaxY - MinY);
            }
        }

        public bool OnSurface(Vector2 p)  => PointInRect(p, SurfaceRect);
        public bool OnFootprint(Vector2 p) => PointInRect(p, FootprintRect);

        static bool PointInRect(Vector2 p, Rectangle r) =>
            p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;

        // Draw sidewalks first, then surface on top, then dashed centreline.
        public void DrawSidewalk() => Raylib.DrawRectangleRec(FootprintRect, Sidewalk);
        public void DrawSurface()  => Raylib.DrawRectangleRec(SurfaceRect, Surface);
        public void DrawMarkings()
        {
            const float dash = 40f, gap = 30f;
            if (Horizontal)
                for (float x = MinX; x < MaxX; x += dash + gap)
                    Raylib.DrawRectangle((int)x, (int)(Start.Y - 4), (int)MathF.Min(dash, MaxX - x), 8, Marking);
            else
                for (float y = MinY; y < MaxY; y += dash + gap)
                    Raylib.DrawRectangle((int)(Start.X - 4), (int)y, 8, (int)MathF.Min(dash, MaxY - y), Marking);
        }
        public void DrawStreetlights(bool night)
    {
        var post = new Color((byte)45, (byte)45, (byte)50, (byte)255);
        var head = night ? new Color((byte)255, (byte)225, (byte)140, (byte)255)
                         : new Color((byte)120, (byte)120, (byte)110, (byte)255);
        foreach (var p in LightPositions())
        {
            Raylib.DrawRectangle((int)p.X - 3, (int)p.Y - 22, 6, 22, post);   // pole
            if (night)
                Raylib.DrawCircle((int)p.X, (int)p.Y - 24, 26, new Color((byte)255, (byte)230, (byte)150, (byte)60)); // glow
            Raylib.DrawCircle((int)p.X, (int)p.Y - 24, 6, head);              // lamp head
        }
    }
    }

    public static class RoadManager
    {
        public static readonly List<Road> Roads = new();

        // One-liner to drop a road. Returns it so you can tweak colours if wanted.
        public static Road Add(Vector2 start, Vector2 end, float laneWidth = 100f, float sidewalkWidth = 40f)
        {
            var r = new Road(start, end, laneWidth, sidewalkWidth);
            Roads.Add(r);
            return r;
        }

        // On the drivable surface? (drives the speed multiplier)
        public static bool IsOnRoadSurface(Vector2 pos)
        {
            foreach (var r in Roads) if (r.OnSurface(pos)) return true;
            return false;
        }

        // On road OR sidewalk? (safe zone / walkable + suppresses off-road grass)
        public static bool IsOnFootprint(Vector2 pos)
        {
            foreach (var r in Roads) if (r.OnFootprint(pos)) return true;
            return false;
        }
        public static bool IsNearRoad(Vector2 pos, float buffer = 80f)
        {
            foreach (var r in Roads)
            {
                Rectangle fp = r.FootprintRect;
                if (pos.X >= fp.X - buffer && pos.X <= fp.X + fp.Width + buffer &&
                    pos.Y >= fp.Y - buffer && pos.Y <= fp.Y + fp.Height + buffer) return true;
            }
            return false;
        }

        public static void DrawOnWorldMap(int cx, int cy, float scale, int mapX, int mapY, int mapW, int mapH)
        {
            var col = new Color((byte)70, (byte)70, (byte)70, (byte)255);
            foreach (var r in Roads)
            {
                Rectangle fp = r.FootprintRect;
                int x = cx + (int)(fp.X * scale);
                int y = cy + (int)(fp.Y * scale);
                int w = Math.Max(2, (int)(fp.Width  * scale));
                int h = Math.Max(2, (int)(fp.Height * scale));
                int cxp = Math.Clamp(x, mapX, mapX + mapW);
                int cyp = Math.Clamp(y, mapY, mapY + mapH);
                Raylib.DrawRectangle(cxp, cyp,
                    Math.Clamp(x + w, mapX, mapX + mapW) - cxp,
                    Math.Clamp(y + h, mapY, mapY + mapH) - cyp, col);
            }
        }

        // Minimap: player-centred transform (already inside its scissor region).
        public static void DrawOnMinimap(int cx, int cy, float scale, Vector2 playerPos)
        {
            var col = new Color((byte)70, (byte)70, (byte)70, (byte)255);
            foreach (var r in Roads)
            {
                Rectangle fp = r.FootprintRect;
                int x = cx + (int)((fp.X - playerPos.X) * scale);
                int y = cy + (int)((fp.Y - playerPos.Y) * scale);
                Raylib.DrawRectangle(x, y,
                    Math.Max(1, (int)(fp.Width * scale)),
                    Math.Max(1, (int)(fp.Height * scale)), col);
            }
        }

        // Draw every road, culled to the view. Call INSIDE BeginMode2D(camera),
        // early in the ground pass so world objects render on top.
        public static void DrawAll(Vector2 camTarget, int screenW, int screenH, float zoom = 1f)
        {
            float halfW = screenW / zoom, halfH = screenH / zoom;
            float vl = camTarget.X - halfW, vr = camTarget.X + halfW;
            float vt = camTarget.Y - halfH, vb = camTarget.Y + halfH;

            bool Visible(Road r)
            {
                Rectangle fp = r.FootprintRect;
                return !(fp.X + fp.Width < vl || fp.X > vr || fp.Y + fp.Height < vt || fp.Y > vb);
            }

            foreach (var r in Roads) if (Visible(r)) r.DrawSidewalk();
            bool night = Program.GetCurrentHour() >= 19f || Program.GetCurrentHour() < 6f; 
            foreach (var r in Roads) if (Visible(r)) r.DrawStreetlights(night);
            foreach (var r in Roads) if (Visible(r)) r.DrawSurface();
            foreach (var r in Roads) if (Visible(r)) r.DrawMarkings();
              
             
        }
    }
}
