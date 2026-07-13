using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    // One carpark bay strip. Sits alongside a road, oriented PERPENDICULAR to it:
    // parallel to a vertical road (bays run horizontally) it is a Horizontal strip,
    // parallel to a horizontal road it is a Vertical strip. Vehicles parked here face
    // across the road (see StallFacing).
    public class Carpark
    {
        public Vector2 Start;         // strip centreline start (world coords)
        public Vector2 End;           // strip centreline end
        public float Depth;           // half-extent away from the road edge (full depth = 2*Depth)
        public bool Horizontal;       // strip long-axis is horizontal
        public float StallSpacing = 120f;
        public bool Facing;           // true = stalls face the road on the +ve side

        public Color Surface = new Color((byte)52, (byte)52, (byte)56, (byte)255);
        public Color Line    = new Color((byte)220, (byte)220, (byte)210, (byte)255);

        public Carpark(Vector2 start, Vector2 end, float depth, bool facing)
        {
            Start = start;
            End = end;
            Depth = depth;
            Facing = facing;
            Horizontal = MathF.Abs(end.X - start.X) >= MathF.Abs(end.Y - start.Y);
        }

        float MinX => MathF.Min(Start.X, End.X);
        float MaxX => MathF.Max(Start.X, End.X);
        float MinY => MathF.Min(Start.Y, End.Y);
        float MaxY => MathF.Max(Start.Y, End.Y);

        // Drivable/parkable surface rect (counts as on-road for the vehicle speed effect).
        public Rectangle SurfaceRect => Horizontal
            ? new Rectangle(MinX, Start.Y - Depth, MaxX - MinX, Depth * 2f)
            : new Rectangle(Start.X - Depth, MinY, Depth * 2f, MaxY - MinY);

        // Vehicles parked face across the road: if the strip is horizontal (road vertical),
        // the vehicle faces vertically; if the strip is vertical (road horizontal), it faces horizontally.
        public Vector2 StallFacing => Horizontal ? new Vector2(0f, Facing ? -1f : 1f)
                                                  : new Vector2(Facing ? -1f : 1f, 0f);

        public bool OnSurface(Vector2 p) => PointInRect(p, SurfaceRect);

        static bool PointInRect(Vector2 p, Rectangle r) =>
            p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;

        public void DrawSurface() => Raylib.DrawRectangleRec(SurfaceRect, Surface);

        public void DrawStalls()
        {
            const float inset = 12f;   // stall lines fall short of the far edge
            if (Horizontal)
                for (float x = MinX; x <= MaxX; x += StallSpacing)
                    Raylib.DrawRectangle((int)x, (int)(Start.Y - Depth + inset),
                                         3, (int)(Depth * 2f - inset * 2f), Line);
            else
                for (float y = MinY; y <= MaxY; y += StallSpacing)
                    Raylib.DrawRectangle((int)(Start.X - Depth + inset), (int)y,
                                         (int)(Depth * 2f - inset * 2f), 3, Line);
        }
    }

    public static class CarparkManager
    {
        public static readonly List<Carpark> Carparks = new();

        public static Carpark Add(Vector2 start, Vector2 end, float depth, bool facing)
        {
            var c = new Carpark(start, end, depth, facing);
            Carparks.Add(c);
            return c;
        }

        // Double-sided: drop a carpark on BOTH sides of a road, each perpendicular to it.
        // gap = clearance from the road edge before the bays begin.
        public static void AddDoubleSided(Vector2 start, Vector2 end, float laneWidth,
                                          float depth = 90f, bool horizontal = true)
        {
            float off = laneWidth * 0.5f + depth * 0.5f;   // centre each strip just outside the lane
            if (horizontal)
            {
                float minX = MathF.Min(start.X, end.X);
                float maxX = MathF.Max(start.X, end.X);
                Add(new Vector2(minX, start.Y - off), new Vector2(maxX, start.Y - off), depth, facing: true);
                Add(new Vector2(minX, start.Y + off), new Vector2(maxX, start.Y + off), depth, facing: false);
            }
            else
            {
                float minY = MathF.Min(start.Y, end.Y);
                float maxY = MathF.Max(start.Y, end.Y);
                Add(new Vector2(start.X - off, minY), new Vector2(start.X - off, maxY), depth, facing: true);
                Add(new Vector2(start.X + off, minY), new Vector2(start.X + off, maxY), depth, facing: false);
            }
        }

        // Vehicle on the parkable surface? (feeds the on/off-road speed multiplier)
        public static bool IsOnCarparkSurface(Vector2 pos)
        {
            foreach (var c in Carparks) if (c.OnSurface(pos)) return true;
            return false;
        }

        // Blocks tree/rock spawns landing on bays.
        public static bool IsNearCarpark(Vector2 pos, float buffer = 80f)
        {
            foreach (var c in Carparks)
            {
                Rectangle r = c.SurfaceRect;
                if (pos.X >= r.X - buffer && pos.X <= r.X + r.Width + buffer &&
                    pos.Y >= r.Y - buffer && pos.Y <= r.Y + r.Height + buffer) return true;
            }
            return false;
        }

        // Call INSIDE BeginMode2D(camera), in the ground pass after roads so bays sit on top.
        public static void DrawAll(Vector2 camTarget, int screenW, int screenH, float zoom = 1f)
        {
            float halfW = screenW / zoom, halfH = screenH / zoom;
            float vl = camTarget.X - halfW, vr = camTarget.X + halfW;
            float vt = camTarget.Y - halfH, vb = camTarget.Y + halfH;

            bool Visible(Carpark c)
            {
                Rectangle r = c.SurfaceRect;
                return !(r.X + r.Width < vl || r.X > vr || r.Y + r.Height < vt || r.Y > vb);
            }

            foreach (var c in Carparks) if (Visible(c)) c.DrawSurface();
            foreach (var c in Carparks) if (Visible(c)) c.DrawStalls();
        }
    }
}