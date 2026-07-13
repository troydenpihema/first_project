using Raylib_cs;
using System;
using System.Numerics;

namespace OpenWorldRPG
{
    // Layered, tintable LPC character renderer.
    //
    // Draw order is z-depth (back to front): skin body -> clothing -> boots ->
    // gloves -> hair -> hat -> held item. All layers share ONE source rectangle,
    // so they stay in sync every frame as long as every sheet is exported with the
    // same layout (64px cells, same row order, same frame counts).
    //
    // Greyscale masks are recoloured by passing a tint Color to DrawTexturePro
    // (multiply blend). Author masks in a mid-to-light grey range (~100-230) so the
    // tint keeps visible shading. Pre-coloured sheets (e.g. weapons) pass White.
    //
    // This is a RENDERER. Your existing player-logic class should OWN one of these
    // and feed it position + animation state each frame, rather than replacing your
    // movement/inventory code.
    class PlayerCharacter
    {
        // ── sizing ────────────────────────────────────────────────
        public const int Cell = 64;              // LPC base cell size
        public float Scale = 1.5f;               // on-screen scale (match your current player)
        public bool  Oversize;
        public const int OversizeCell = 128;
        public const int OversizeY    = 3456;

        // ── LPC row groups (row = BaseRow + Dir) ──────────────────
        // Dir: 0 = North(Up), 1 = West(Left), 2 = South(Down), 3 = East(Right)
        public const int RowSpellcast = 0;
        public const int RowThrust    = 4;       // pickaxe / axe hit
        public const int RowWalk      = 8;       // walk + idle(frame 0)
        public const int RowSlash     = 12;      // weapon swing (64px variant)
        public const int RowShoot     = 16;      // bow / fishing-cast style
        public const int RowHurt      = 20;
        public const int RowMining = 54;
        public int FallbackRow = RowSlash;      
        public int FallbackFrames = 6; 
        float _actionT; 
        public Vector2 DrawOffset = new(-28f, -26f); 

        // ── layers: fixed skin base (never tinted) ────────────────
        public Texture2D BodyTexture;

        // ── layers: greyscale masks, each with its own tint ───────
        public Texture2D ClothingTexture; public Color ClothingColor = Color.White;
        public Texture2D BootsTexture;    public Color BootsColor    = Color.White;
        public Texture2D GlovesTexture;   public Color GlovesColor   = Color.White;
        public Texture2D HairTexture;     public Color HairColor     = Color.Gold;
        public Texture2D HatTexture;      public Color HatColor      = Color.White;

        // ── held item: tool OR weapon (exclusive slot) ────────────
        // Tool masks may be tinted; pre-coloured weapon sheets pass White.
        public Texture2D HeldTexture;     public Color HeldColor     = Color.White;

        // When a covering hat/helmet is on, skip the hair layer so it isn't
        // drawn poking through the helmet.
        public bool HatCoversHair = false;

        // ── animation state (driven by your player logic each frame) ──
        public int BaseRow = RowWalk;   // which animation group
        public int Dir = 2;             // facing (see Dir table above)
        public int Frame = 0;           // current column on the sheet
        

        // Advance a looping walk cycle. Call while moving; pass the per-frame dt.
        // frameCount is how many columns your walk row uses (LPC classic = 9,
        // ULPC-expanded 13-wide sheets = 9 walk frames in columns 0-8).
        private float _walkTimer;
        public void TickWalk(float dt, int frameCount = 9, float fps = 8f)
        {
            Oversize = false;
            _walkTimer += dt * fps;
            if (_walkTimer >= 1f)
            {
                _walkTimer -= 1f;
                Frame = (Frame + 1) % frameCount;
            }
        }

        // Reset to a standing idle pose (walk row, first column).
        public void SetIdle()
        {
            Oversize = false; 
            BaseRow = RowWalk;
            Frame = 0;
            _walkTimer = 0f;
        }

        // Map a 0..1 action progress onto an action row's frames (thrust/slash/shoot).
        // Use with your existing swingTimer/chopAnimAngle progress values.
        public void SetAction(int baseRow, float progress01, int frameCount = 8, bool oversize = false)
        {
            _actionT = Math.Clamp(progress01, 0f, 1f);
            Oversize = oversize; 
            BaseRow = baseRow;
            Frame = Math.Clamp((int)(progress01 * frameCount), 0, frameCount - 1);
        }

        // ── draw the full stack at a world position ───────────────
        public int OversizeBlock = 0;

        public void Draw(Vector2 pos)
        {
            pos += DrawOffset;
            // normal 64px addressing
            Rectangle nSrc = new(Frame * Cell, (BaseRow + Dir) * Cell, Cell, Cell);
            Rectangle nDst = new(pos.X, pos.Y, Cell * Scale, Cell * Scale);

            // oversized 128px addressing (only used while Oversize)
            int oY = OversizeY + (OversizeBlock * 4 + Dir) * OversizeCell;
            Rectangle oSrc = new(Frame * OversizeCell, oY, OversizeCell, OversizeCell);
            float grow = (OversizeCell - Cell) * Scale / 2f;
            Rectangle oDst = new(pos.X - grow, pos.Y - grow, OversizeCell * Scale, OversizeCell * Scale);

            // 64px fallback for layers missing the oversize block: thrust row, progress-mapped
            int fbFrame = Math.Clamp((int)(_actionT * FallbackFrames), 0, FallbackFrames - 1);
            Rectangle fSrc = new(fbFrame * Cell, (FallbackRow + Dir) * Cell, Cell, Cell);

            Vector2 zero = Vector2.Zero;

            void Blit(Texture2D t, Color c)
            {
                if (t.Id == 0) return;
                if (!Oversize)
                {
                    if ((BaseRow + Dir + 1) * Cell <= t.Height)
                        Raylib.DrawTexturePro(t, nSrc, nDst, zero, 0f, c);
                    return;
                }
                if (oY + OversizeCell <= t.Height)                 // sheet has the smash block → use it
                    Raylib.DrawTexturePro(t, oSrc, oDst, zero, 0f, c);
                else                                               // no block → keep animating on thrust
                    Raylib.DrawTexturePro(t, fSrc, nDst, zero, 0f, c);
            }

            Blit(BodyTexture,     Color.White);
            Blit(ClothingTexture, ClothingColor);
            Blit(BootsTexture,    BootsColor);
            Blit(GlovesTexture,   GlovesColor);
            if (!HatCoversHair) Blit(HairTexture, HairColor);
            Blit(HatTexture,      HatColor);
            Blit(HeldTexture,     HeldColor);
        }

    }
}
