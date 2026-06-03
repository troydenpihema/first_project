using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG;

    public class FenceManager
{
    public class Fence
    {
        public Vector2 Origin;           // top-left of first segment
        public int Segments;
        public int SegmentWidth = 12;
        public int SegmentHeight = 30;
        public int Spacing = 108;        // space between segment X positions (useful for your art)
        public Color PlankColor = new Color(100,60,20,255);
        public Color PostColor  = new Color(120,80,40,255);
        // orientation: true = horizontal (x increasing), false = vertical (y increasing)
        public bool Horizontal = true;
                // Correctly-typed list
        public List<Rectangle> CollisionRects = new List<Rectangle>();

        public Fence(Vector2 origin, int segments, bool horizontal = true)
        {
            Origin = origin;
            Segments = Math.Max(0, segments);
            Horizontal = horizontal;
            Rebuild();
        }

        public void Rebuild()
{
    CollisionRects.Clear();

    if (Segments == 0) return;

    if (Horizontal)
    {
        // one solid rectangle spanning the full fence length
        float totalW = Segments * (SegmentWidth + Spacing);
        CollisionRects.Add(new Rectangle(Origin.X, Origin.Y, totalW, SegmentHeight));
    }
    else
    {
        // one solid rectangle spanning the full fence height
        float totalH = Segments * (SegmentHeight + Spacing);
        CollisionRects.Add(new Rectangle(Origin.X, Origin.Y, SegmentWidth, totalH));
    }
}

        public void Extend(int delta)
        {
            Segments = Math.Max(0, Segments + delta);
            Rebuild();
        }

        public Fence Duplicate(Vector2 newOrigin)
        {
            var f = new Fence(newOrigin, Segments, Horizontal)
            {
                SegmentWidth = this.SegmentWidth,
                SegmentHeight = this.SegmentHeight,
                Spacing = this.Spacing,
                PlankColor = this.PlankColor,
                PostColor = this.PostColor
            };
            f.Rebuild();
            return f;
        }

 public void Draw()
{
    if (Segments == 0) return;

    if (Horizontal)
    {
        float totalW = Segments * (SegmentWidth + Spacing);
        float startX = Origin.X;
        float startY = Origin.Y;

        // two horizontal rails
        Raylib.DrawRectangle((int)startX, (int)startY,      (int)totalW, 5, PlankColor);
        Raylib.DrawRectangle((int)startX, (int)startY + 16, (int)totalW, 5, PlankColor);

        // vertical pickets using actual Spacing value
        for (int i = 0; i < Segments; i++)
        {
            float px = startX + i * (SegmentWidth + Spacing) + Spacing / 2f;
            Raylib.DrawRectangle((int)px, (int)startY - 2, SegmentWidth, SegmentHeight + 4, PlankColor);
        }

        // end posts
        Raylib.DrawRectangle((int)startX - 8,             (int)startY - 4, 8, SegmentHeight + 8, PostColor);
        Raylib.DrawRectangle((int)(startX + totalW),      (int)startY - 4, 8, SegmentHeight + 8, PostColor);
    }
    else
    {
        float totalH = Segments * (SegmentHeight + Spacing);
        float startX = Origin.X;
        float startY = Origin.Y;

        // two vertical rails
        Raylib.DrawRectangle((int)startX,      (int)startY, 5, (int)totalH, PlankColor);
        Raylib.DrawRectangle((int)startX + 16, (int)startY, 5, (int)totalH, PlankColor);

        // horizontal pickets using actual Spacing value
        for (int i = 0; i < Segments; i++)
        {
            float py = startY + i * (SegmentHeight + Spacing) + Spacing / 2f;
            Raylib.DrawRectangle((int)startX - 2, (int)py, SegmentHeight + 4, SegmentWidth, PlankColor);
        }

        // end posts
        Raylib.DrawRectangle((int)startX - 4, (int)startY - 8,           SegmentHeight + 8, 8, PostColor);
        Raylib.DrawRectangle((int)startX - 4, (int)(startY + totalH),    SegmentHeight + 8, 8, PostColor);
    }
}
    }

    // All fences
    public List<Fence> Fences = new List<Fence>();

    // Combined cache for collision queries
    private List<Rectangle> combinedCollisionCache = new List<Rectangle>();
    private bool cacheDirty = true;

    public FenceManager() { }

  public Fence SpawnAt(Vector2 origin, int segments, bool horizontal = true, int segmentWidth = 12, int segmentHeight = 30, int spacing = 108)
{
    var f = new Fence(origin, segments, horizontal)
    {
        SegmentWidth = segmentWidth,
        SegmentHeight = segmentHeight,
        Spacing = spacing
    };
    f.Rebuild(); // rebuild again now that correct values are set
    Fences.Add(f);
    cacheDirty = true;
    return f;
}

    public Fence SpawnLine(Vector2 start, Vector2 end, string axis = "horizontal", int segmentWidth = 12, int segmentHeight = 30, int spacing = 108)
    {
        bool horizontal = axis.ToLower().StartsWith("h");
        float length = horizontal ? MathF.Abs(end.X - start.X) : MathF.Abs(end.Y - start.Y);
        int segSize = horizontal ? (segmentWidth + spacing) : (segmentHeight + spacing);
        int segments = Math.Max(1, (int)MathF.Floor(length / segSize));
        return SpawnAt(start, segments, horizontal, segmentWidth, segmentHeight, spacing);
    }

    public void RemoveFence(Fence f)
    {
        if (Fences.Remove(f)) cacheDirty = true;
    }

    public void Clear()
    {
        Fences.Clear();
        cacheDirty = true;
    }

    private void RebuildCacheIfNeeded()
    {
        if (!cacheDirty) return;
        combinedCollisionCache.Clear();
        foreach (var f in Fences)
            combinedCollisionCache.AddRange(f.CollisionRects);
        cacheDirty = false;
    }

    // Call during drawing (inside BeginMode2D)
    public void Draw()
    {
        foreach (var f in Fences) f.Draw();
    }

    // Collision query
    public bool CheckCollision(Rectangle rect)
    {
        RebuildCacheIfNeeded();
        foreach (var r in combinedCollisionCache)
            if (Raylib.CheckCollisionRecs(r, rect)) return true;
        return false;
    }

    public List<Rectangle> GetCollisionRects()
    {
        RebuildCacheIfNeeded();
        return new List<Rectangle>(combinedCollisionCache);
    }

    public void MarkDirty() => cacheDirty = true;
}

        