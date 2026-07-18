
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
    partial class Program
    {
        // ═══════════════════════════════════════════════════════════════
        //  1. LOW HEALTH SCREEN PULSE
        // ═══════════════════════════════════════════════════════════════
        // Red vignette that pulses at screen edges when HP < 25%.
        // Pulse speed increases as HP drops lower.

        static float lowHealthPulse = 0f;

        static void UpdateLowHealthPulse(float dt)
        {
            float hpPercent = (float)player.Health / player.MaxHealth;
            if (hpPercent < 0.25f && player.Health > 0 && !deathScreenActive)
            {
                // pulse faster as HP gets lower
                float urgency = 1f - (hpPercent / 0.25f); // 0 at 25%, 1 at 0%
                float speed = 3f + urgency * 5f;          // 3–8 Hz
                lowHealthPulse += dt * speed;
            }
            else
            {
                lowHealthPulse = 0f;
            }
        }

        static void DrawLowHealthPulse()
        {
            float hpPercent = (float)player.Health / player.MaxHealth;
            if (hpPercent >= 0.25f || player.Health <= 0 || deathScreenActive) return;

            float urgency = 1f - (hpPercent / 0.25f);
            float pulse = (MathF.Sin(lowHealthPulse) + 1f) / 2f; // 0–1 oscillation

            // base intensity grows with urgency, pulse modulates it
            byte alpha = (byte)(40 + urgency * 80 + pulse * 60 * urgency);

            // draw four gradient rectangles at screen edges (vignette)
            int thickness = 80 + (int)(urgency * 60); // 80–140px deep
            Color red = new Color((byte)180, (byte)0, (byte)0, alpha);
            Color clear = new Color((byte)180, (byte)0, (byte)0, (byte)0);

            // top edge
            Raylib.DrawRectangleGradientV(0, 0, ScreenWidth, thickness, red, clear);
            // bottom edge
            Raylib.DrawRectangleGradientV(0, ScreenHeight - thickness, ScreenWidth, thickness, clear, red);
            // left edge
            Raylib.DrawRectangleGradientH(0, 0, thickness, ScreenHeight, red, clear);
            // right edge
            Raylib.DrawRectangleGradientH(ScreenWidth - thickness, 0, thickness, ScreenHeight, clear, red);

            // heartbeat text flash at critical HP (< 10%)
            if (hpPercent < 0.10f && pulse > 0.7f)
            {
                byte ta = (byte)(200 * pulse);
                int tw = MeasureTextUI("LOW HP!", 28);
                DrawTextUI("LOW HP!", ScreenWidth / 2 - tw / 2, 60, 28,
                    new Color((byte)255, (byte)60, (byte)60, ta));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  2. WEAPON SWING ARC
        // ═══════════════════════════════════════════════════════════════
        // Draws a fading arc trail during the player's swing animation.
        // Uses the existing isSwinging / swingTimer / swingDuration.

        static void DrawSwingArc()
        {
            if (!player.isSwinging) return;

            // only draw for weapons, not tools
            string equipped = GetActiveWeapon();
            if (equipped == null) return;
            string slot = GetItemSlot(equipped);
            if (slot != "WEAPON") return;

            float t = player.swingTimer / player.swingDuration; // 0→1
            if (t > 0.9f) return; // fade out at end

            float alpha = (1f - t) * 0.8f; // bright at start, fades
            byte a = (byte)(255 * alpha);

            // arc center is the player center
            Vector2 center = player.Center;
            float reach = 44f; // arc radius

            // arc sweeps based on facing direction
            float startAngle, endAngle;
            switch (player.Facing)
            {
                case Player.FacingDirection.Right:
                    startAngle = -80f; endAngle = 80f; break;
                case Player.FacingDirection.Left:
                    startAngle = 100f; endAngle = 260f; break;
                case Player.FacingDirection.Up:
                    startAngle = 190f; endAngle = 350f; break;
                default: // Down
                    startAngle = 10f; endAngle = 170f; break;
            }

            // the arc "sweeps" — the progress point moves through the arc
            float sweepAngle = startAngle + (endAngle - startAngle) * t;

            // draw trailing arc segments (3 segments fading behind the sweep point)
            Color arcColor = equipped.Contains("Sword") || equipped.Contains("Great Sword")
                ? new Color((byte)220, (byte)230, (byte)255, a)   // silvery white for swords
                : equipped.Contains("War Axe")
                ? new Color((byte)200, (byte)160, (byte)100, a)   // bronze for axes
                : new Color((byte)255, (byte)200, (byte)100, a);  // golden for others

            int segments = 8;
            float trailSpan = (endAngle - startAngle) * Math.Min(t + 0.1f, 0.5f);
            float trailStart = sweepAngle - trailSpan;

            for (int i = 0; i < segments; i++)
            {
                float frac = (float)i / segments;
                float ang1 = trailStart + trailSpan * frac;
                float ang2 = trailStart + trailSpan * (frac + 1f / segments);

                float rad1 = ang1 * MathF.PI / 180f;
                float rad2 = ang2 * MathF.PI / 180f;

                Vector2 p1 = center + new Vector2(MathF.Cos(rad1), MathF.Sin(rad1)) * reach;
                Vector2 p2 = center + new Vector2(MathF.Cos(rad2), MathF.Sin(rad2)) * reach;

                // fade trail: brighter near the sweep point
                byte segA = (byte)(a * frac);
                Raylib.DrawLineEx(p1, p2, 3f - frac * 1.5f,
                    new Color(arcColor.R, arcColor.G, arcColor.B, segA));
            }

            // bright point at the sweep tip
            float tipRad = sweepAngle * MathF.PI / 180f;
            Vector2 tip = center + new Vector2(MathF.Cos(tipRad), MathF.Sin(tipRad)) * reach;
            Raylib.DrawCircleV(tip, 3f, new Color(arcColor.R, arcColor.G, arcColor.B, a));
        }


        // ═══════════════════════════════════════════════════════════════
        //  3. XP POP FLOATING TEXT ENHANCEMENT
        // ═══════════════════════════════════════════════════════════════
        // Replaces the linear fade with: scale-up pop on appear,
        // slight random horizontal drift, and smoother alpha curve.
        //
        // This replaces the existing floating text DRAW code only.
        // The update loop stays the same (timer countdown + Y drift).

        static void DrawEnhancedFloatingTexts()
        {
            foreach (var ft in floatingTexts)
            {
                if (ft.Timer <= 0) continue;

                // normalized lifetime (1 = just spawned, 0 = about to vanish)
                float life = Math.Clamp(ft.Timer / 1.2f, 0f, 1f);

                // pop scale: starts at 1.3x, settles to 1.0x in the first 20% of life
                float popPhase = Math.Clamp((1f - life) / 0.2f, 0f, 1f); // 0→1 in first 0.2 of timer
                float scale = 1.3f - 0.3f * popPhase;
                if (popPhase >= 1f) scale = 1f;

                // smooth alpha: holds solid for 60% of life, then fades
                float alphaF = life > 0.4f ? 1f : life / 0.4f;
                byte alpha = (byte)(255 * alphaF);

                // font size with pop
                int baseSize = 22;
                int fontSize = (int)(baseSize * scale);

                // slight horizontal wobble based on text hash for variety
                int hashDrift = (ft.Text.GetHashCode() & 0xFF) - 128; // -128 to 127
                float driftX = hashDrift * 0.15f * (1f - life); // grows over time

                int drawX = (int)(ft.Position.X + driftX);
                int drawY = (int)ft.Position.Y;

                // shadow for readability
                DrawTextUI(ft.Text, drawX + 1, drawY + 1, fontSize,
                    new Color((byte)0, (byte)0, (byte)0, (byte)(alpha * 0.6f)));
                // main text
                DrawTextUI(ft.Text, drawX, drawY, fontSize,
                    new Color(ft.TextColor.R, ft.TextColor.G, ft.TextColor.B, alpha));
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  4. INTERACT PROMPT ICONS
        // ═══════════════════════════════════════════════════════════════
        // Draws a bouncing key icon [E] / [SPACE] next to interact prompts.

        static float interactIconBob = 0f;

        static void UpdateInteractIcon(float dt)
        {
            interactIconBob += dt * 4f;
        }

        /// Draw a key prompt icon at the given position. Returns the width consumed.
        static int DrawKeyIcon(string key, int x, int y, Color borderColor)
        {
            float bob = MathF.Sin(interactIconBob) * 3f;
            int ky = y + (int)bob;

            int textW = MeasureTextUI(key, 14);
            int boxW = textW + 12;
            int boxH = 22;

            // key cap shape
            Raylib.DrawRectangle(x, ky, boxW, boxH,
                new Color((byte)30, (byte)30, (byte)40, (byte)220));
            Raylib.DrawRectangleLines(x, ky, boxW, boxH, borderColor);
            // slight 3D bevel (bottom/right edge lighter)
            Raylib.DrawLine(x + 1, ky + boxH, x + boxW, ky + boxH,
                new Color((byte)60, (byte)60, (byte)70, (byte)200));
            Raylib.DrawLine(x + boxW, ky + 1, x + boxW, ky + boxH,
                new Color((byte)60, (byte)60, (byte)70, (byte)200));

            DrawTextUI(key, x + 6, ky + 4, 14, Color.White);

            return boxW + 6; // total width + gap
        }

        /// Draw interact prompts near buildings with a key icon
        static void DrawInteractPromptWithIcon(string keyLabel, string message, int x, int y, Color textColor)
        {
            int iconW = DrawKeyIcon(keyLabel, x, y, Color.Gold);
            DrawTextUI(message, x + iconW, y + 2, 18, textColor);
        }


        // ═══════════════════════════════════════════════════════════════
        //  5. SMOOTH HEALTH BAR (LERPED DISPLAY)
        // ═══════════════════════════════════════════════════════════════
        // The displayed HP lags behind the actual HP, creating smooth
        // motion on both damage and healing. Same for survival bars.

        static float displayedHealth   = -1f; // -1 = uninitialized
        static float displayedFood     = -1f;
        static float displayedThirst   = -1f;
        static float displayedStamina  = -1f;

        static void UpdateSmoothedBars(float dt)
        {
            float lerpSpeed = 5f; // higher = snappier

            // initialize on first frame
            if (displayedHealth < 0f)
            {
                displayedHealth  = player.Health;
                displayedFood    = player.Food;
                displayedThirst  = player.Thirst;
                displayedStamina = player.Stamina;
                return;
            }

            displayedHealth  = Lerp(displayedHealth,  player.Health,  lerpSpeed * dt);
            displayedFood    = Lerp(displayedFood,    player.Food,    lerpSpeed * dt);
            displayedThirst  = Lerp(displayedThirst,  player.Thirst,  lerpSpeed * dt);
            displayedStamina = Lerp(displayedStamina, player.Stamina, lerpSpeed * dt);

            // snap when very close to avoid permanent micro-drift
            if (MathF.Abs(displayedHealth - player.Health) < 0.5f) displayedHealth = player.Health;
            if (MathF.Abs(displayedFood   - player.Food)   < 0.5f) displayedFood   = player.Food;
            if (MathF.Abs(displayedThirst - player.Thirst) < 0.5f) displayedThirst = player.Thirst;
            if (MathF.Abs(displayedStamina- player.Stamina)< 0.5f) displayedStamina= player.Stamina;
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);

        /// Replacement health bar draw that uses smoothed values.
        /// Call this INSTEAD of the existing health bar block in DrawHUD.
        static void DrawSmoothedHealthBar()
        {
            int hbWidth = 300;
            int hbX = ScreenWidth / 2 - hbWidth / 2;

            // background
            Raylib.DrawRectangle(hbX, 10, hbWidth, 24,
                new Color((byte)40, (byte)40, (byte)40, (byte)220));

            float healthPercent = displayedHealth / player.MaxHealth;
            float actualPercent = (float)player.Health / player.MaxHealth;

            // "ghost bar" — shows where health WAS (damage trail)
            if (displayedHealth > player.Health)
            {
                int ghostW = (int)(hbWidth * (displayedHealth / player.MaxHealth));
                Raylib.DrawRectangle(hbX, 10, ghostW, 24,
                    new Color((byte)180, (byte)40, (byte)40, (byte)120));
            }

            // "heal glow" — shows where health IS GOING (heal trail)
            if (displayedHealth < player.Health)
            {
                int healW = (int)(hbWidth * actualPercent);
                Raylib.DrawRectangle(hbX, 10, healW, 24,
                    new Color((byte)100, (byte)255, (byte)100, (byte)80));
            }

            // main bar uses displayed (smoothed) value
            Color hpColor = actualPercent > 0.5f ? Color.Green
                          : actualPercent > 0.25f ? Color.Orange : Color.Red;
            int mainW = (int)(hbWidth * Math.Clamp(healthPercent, 0f, 1f));
            Raylib.DrawRectangle(hbX, 10, mainW, 24, hpColor);

            Raylib.DrawRectangleLines(hbX, 10, hbWidth, 24, Color.White);
            DrawTextUI($"HP: {player.Health}/{player.MaxHealth}",
                hbX + hbWidth / 2 - 40, 13, 18, Color.White);

            // survival bars — smoothed versions
            int sbW = 96, sbH = 10, sbY = 40;
            (string label, float displayed, float actual, float max, Color col)[] bars =
            {
                ("FOOD",    displayedFood,    player.Food,    100f, new Color((byte)230,(byte)140,(byte)40,(byte)255)),
                ("THIRST",  displayedThirst,  player.Thirst,  100f, new Color((byte)60,(byte)150,(byte)240,(byte)255)),
                ("STAMINA", displayedStamina, player.Stamina, 100f, new Color((byte)230,(byte)210,(byte)60,(byte)255)),
            };

            for (int sb = 0; sb < bars.Length; sb++)
            {
                int sx = hbX + sb * (sbW + 6);
                Raylib.DrawRectangle(sx, sbY, sbW, sbH,
                    new Color((byte)40, (byte)40, (byte)40, (byte)220));
                int fillW = (int)(sbW * Math.Clamp(bars[sb].displayed / bars[sb].max, 0f, 1f));
                Raylib.DrawRectangle(sx, sbY, fillW, sbH, bars[sb].col);
                Raylib.DrawRectangleLines(sx, sbY, sbW, sbH, Color.White);
                DrawTextUI(bars[sb].label, sx + 2, sbY + 11, 12, Color.LightGray);
            }
        }


        // ═══════════════════════════════════════════════════════════════
        //  6. SCREEN FLASH ON KEY EVENTS
        // ═══════════════════════════════════════════════════════════════
        // Brief full-screen flash for level ups, crits, boss kills, etc.

        static float screenFlashTimer = 0f;
        static Color screenFlashColor = Color.White;

        static void TriggerScreenFlash(Color color, float duration = 0.12f)
        {
            screenFlashColor = color;
            screenFlashTimer = duration;
        }

        static void UpdateScreenFlash(float dt)
        {
            if (screenFlashTimer > 0f) screenFlashTimer -= dt;
        }

        static void DrawScreenFlash()
        {
            if (screenFlashTimer <= 0f) return;

            float t = screenFlashTimer / 0.12f; // normalized, assumes max 0.12
            byte alpha = (byte)(180 * Math.Min(t, 1f));
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight,
                new Color(screenFlashColor.R, screenFlashColor.G, screenFlashColor.B, alpha));
        }


        // ═══════════════════════════════════════════════════════════════
        //  COMBINED UPDATE & DRAW
        // ═══════════════════════════════════════════════════════════════

        static void UpdatePolish3(float dt)
        {
            UpdateLowHealthPulse(dt);
            UpdateInteractIcon(dt);
            UpdateSmoothedBars(dt);
            UpdateScreenFlash(dt);
        }

        /// Call inside BeginMode2D — world-space effects
        static void DrawPolish3World()
        {
            DrawSwingArc();
        }

        // ═══════════════════════════════════════════════════════════════
        //  7. INVENTORY HOVER TOOLTIP
        // ═══════════════════════════════════════════════════════════════
        // Shows item category, description, and sell value on hover.

        static string GetItemCategory(string item)
        {
            if (item == null) return "Unknown";
            if (item == "Money") return "Currency";

            if (IsUsable(item)) return "Consumable";
            if (IsToolItem(item)) return "Tool";

            string slot = GetItemSlot(item);
            if (slot == "WEAPON") return "Weapon";
            if (slot != null) return "Armor";

            if (item == "Arrows" || item == "Bolts" || item == "Arcane Essence") return "Ammo";

            // materials
            if (item.Contains("Logs") || item == "Dead Wood") return "Wood";
            if (item.Contains("Ore") || item.Contains("Bar") || item.Contains("Crystal") || item.Contains("Coal")) return "Ore";
            if (item.Contains("Seed")) return "Seed";
            if (item == "Fish" || item == "Raw Meat" || item == "Cooked Fish" || item == "Cooked Meat") return "Food";
            if (item == "Bones" || item == "Fur" || item == "Stingers" || item == "Pelts"
                || item == "Bear Pelts" || item == "Feathers" || item == "Crab Shell") return "Drop";
            if (item.Contains("Egg")) return "Egg";

            return "Material";
        }

        static string GetItemDescription(string item)
        {
            if (item == null) return "";
            return item switch
            {
                // tools
                "Axe"          => "Chops trees for logs.",
                "Pickaxe"      => "Mines rocks for ore.",
                "Rod"          => "Cast into water to catch fish.",
                "Net"          => "Catches fish in shallow water.",
                "Spade"        => "Tills soil for planting crops.",
                "Watering Can" => "Waters tilled farm plots.",
                "Torch"        => "Lights the way at night.",
                "Stick"        => "A basic melee weapon.",

                // consumables
                "Cooked Fish"  => "Restores health. Grants a food buff.",
                "Cooked Meat"  => "Hearty meal. Restores health.",
                "Apple"        => "A fresh apple. Restores a little health.",
                "Bread"        => "Simple food. Restores health.",
                "Bandage"      => "Heals a small wound.",
                "Health Potion"=> "Restores a large amount of health.",
                "Stamina Potion" => "Restores stamina instantly.",
                "Roast Potato" => "Warm and filling. Restores health.",
                "Bacon & Eggs" => "A hearty breakfast. Restores health.",
                "Sandwich"     => "Quick snack. Restores health.",
                "Fruit Salad"  => "Refreshing. Restores health and thirst.",
                "Vegetable Soup" => "Warming broth. Restores health.",
                "Pancakes"     => "Sweet and fluffy. Restores health.",
                "Homemade Pizza" => "Cheesy goodness. Restores lots of health.",

                // raw materials
                "Logs"         => "Common wood from normal trees.",
                "Birch Logs"   => "Light-coloured wood. Sells for more.",
                "Oak Logs"     => "Sturdy hardwood from oak trees.",
                "Pine Logs"    => "Resinous wood from pine trees.",
                "Arctic Logs"  => "Rare wood from frozen trees.",
                "Dead Wood"    => "Dry, brittle wood. Low value.",
                "Copper Ore"   => "Basic ore. Smelt into bars.",
                "Iron Ore"     => "Sturdy ore for quality gear.",
                "Gold Ore"     => "Precious ore. Very valuable.",
                "Crystal"      => "A gleaming crystal shard.",
                "Coal"         => "Fuel for smelting furnaces.",
                "Fish"         => "Raw fish. Cook or sell it.",
                "Raw Meat"     => "Uncooked meat. Cook before eating.",
                "Bones"        => "Monster remains. Sell at the shop.",
                "Fur"          => "Soft animal fur. Used in crafting.",
                "Stingers"     => "Sharp insect stingers. Sell for profit.",
                "Pelts"        => "Animal pelts. Crafting material.",
                "Feathers"     => "Light feathers. Used for arrows.",
                "Money"        => "Your hard-earned cash.",

                // weapons
                "Sword"        => "A reliable one-handed blade.",
                "Bow"          => "Ranged weapon. Requires arrows.",
                "Crossbow"     => "Powerful ranged weapon. Requires bolts.",
                "Arrows"       => "Ammunition for bows.",
                "Bolts"        => "Ammunition for crossbows.",
                "Arcane Essence" => "Magical ammo for staffs.",

                _ => GetItemSlot(item) != null
                    ? $"Equippable gear ({GetItemSlot(item).ToLower()} slot)."
                    : item.Contains("Seed")
                    ? "Plant in tilled soil to grow crops."
                    : ""
            };
        }

        static int GetItemSellValue(string item)
        {
            // check gear prices first
            int gearPrice = GetGearSellPrice(item);
            if (gearPrice > 0) return gearPrice;

            // material sell prices (matches shop)
            return item switch
            {
                "Logs"       => 5,
                "Birch Logs" => 8,
                "Oak Logs"   => 12,
                "Pine Logs"  => 18,
                "Arctic Logs"=> 25,
                "Dead Wood"  => 3,
                "Fish"       => 10,
                "Bones"      => 8,
                "Fur"        => 15,
                "Stingers"   => 12,
                "Pelts"      => 25,
                "Copper Ore" => 8,
                "Iron Ore"   => 15,
                "Gold Ore"   => 30,
                "Crystal"    => 40,
                "Coal"       => 5,
                "Raw Meat"   => 6,
                "Cooked Fish"=> 15,
                "Cooked Meat"=> 18,
                _ => 0
            };
        }

        static Color GetCategoryColor(string category) => category switch
        {
            "Weapon"     => new Color((byte)255, (byte)100, (byte)100, (byte)255),
            "Armor"      => new Color((byte)100, (byte)160, (byte)255, (byte)255),
            "Tool"       => new Color((byte)200, (byte)200, (byte)100, (byte)255),
            "Consumable" => new Color((byte)100, (byte)230, (byte)100, (byte)255),
            "Ammo"       => new Color((byte)255, (byte)180, (byte)100, (byte)255),
            "Currency"   => Color.Gold,
            _            => new Color((byte)180, (byte)180, (byte)190, (byte)255),
        };

        /// Call from the inventory draw loop when hovering an item.
        /// Pass the mouse position and item name.
        static void DrawInventoryTooltip(string itemName, int count, int mouseX, int mouseY)
        {
            if (itemName == null) return;

            string category = GetItemCategory(itemName);
            string desc = GetItemDescription(itemName);
            int sellVal = GetItemSellValue(itemName);
            Color catCol = GetCategoryColor(category);

            // build lines
            List<(string text, int size, Color color)> lines = new();
            lines.Add((itemName, 16, Color.White));
            lines.Add((category, 13, catCol));
            if (desc.Length > 0) lines.Add((desc, 12, Color.LightGray));
            if (count > 1) lines.Add(($"Quantity: {count}", 12, Color.LightGray));
            if (sellVal > 0) lines.Add(($"Sell: ${sellVal} each", 12, Color.Gold));

            // measure tooltip size
            int padX = 12, padY = 8;
            int lineGap = 4;
            int tooltipW = 0;
            int tooltipH = padY * 2;
            foreach (var line in lines)
            {
                int lw = MeasureTextUI(line.text, line.size);
                if (lw > tooltipW) tooltipW = lw;
                tooltipH += line.size + lineGap;
            }
            tooltipW += padX * 2;

            // position: above and to the right of cursor, clamped to screen
            int tx = mouseX + 16;
            int ty = mouseY - tooltipH - 8;
            if (tx + tooltipW > ScreenWidth - 10) tx = mouseX - tooltipW - 8;
            if (ty < 10) ty = mouseY + 24;

            // background
            Raylib.DrawRectangle(tx, ty, tooltipW, tooltipH,
                new Color((byte)15, (byte)15, (byte)25, (byte)240));
            Raylib.DrawRectangleLines(tx, ty, tooltipW, tooltipH,
                new Color(catCol.R, catCol.G, catCol.B, (byte)160));

            // category color accent bar on the left
            Raylib.DrawRectangle(tx, ty, 3, tooltipH, catCol);

            // draw lines
            int ly = ty + padY;
            foreach (var line in lines)
            {
                DrawTextUI(line.text, tx + padX, ly, line.size, line.color);
                ly += line.size + lineGap;
            }
        }


        /// Call after EndMode2D — screen-space HUD overlays
        static void DrawPolish3HUD()
        {
            DrawLowHealthPulse();
            DrawScreenFlash();
        }
    }
}
