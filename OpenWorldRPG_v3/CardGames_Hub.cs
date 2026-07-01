// ============================================================================
//  CARD GAMES HUB  —  Menu, ratings, tutorials, per-game XP
//  Covers: Euchre, Five Hundred, Sequence
//
//  HOOK-IN CHECKLIST (all edits are small inline snippets):
//
//  ── A. Player class (in Program.cs) — add these fields: ──
//    public int EuchreRating = 200;
//    public int FiveHundredRating = 200;
//    public int SequenceRating = 200;
//    public int EuchreWins = 0;
//    public int FiveHundredWins = 0;
//    public int SequenceWins = 0;
//
//  ── B. Replace StartCardGame(type) calls in Casino with: ──
//    OpenCardsHub(CardGameType.Euchre);     // euchre table
//    OpenCardsHub(CardGameType.FiveHundred);// 500 table
//
//  ── C. Replace StartSequenceGame() call in MY HOUSE with: ──
//    OpenCardsHub(CardGameType.Sequence);
//
//  ── D. Save (before GEAR_START line): ──
//    lines.Add(player.PlayingCardsLevel.ToString());
//    lines.Add(player.PlayingCardsXP.ToString());
//    lines.Add(player.EuchreRating.ToString());
//    lines.Add(player.FiveHundredRating.ToString());
//    lines.Add(player.SequenceRating.ToString());
//    lines.Add(player.EuchreWins.ToString());
//    lines.Add(player.FiveHundredWins.ToString());
//    lines.Add(player.SequenceWins.ToString());
//
//  ── E. Load (after tutorial tasks block, before GEAR_START marker search): ──
//    player.PlayingCardsLevel = NextInt();
//    player.PlayingCardsXP    = NextInt();
//    player.EuchreRating      = NextInt();
//    player.FiveHundredRating = NextInt();
//    player.SequenceRating    = NextInt();
//    player.EuchreWins        = NextInt();
//    player.FiveHundredWins   = NextInt();
//    player.SequenceWins      = NextInt();
//
//  ── F. Skills UI — UpdateSkillsUI: add after elementalBtn hover check: ──
//    hoverCards = Raylib.CheckCollisionPointRec(mouse, cardsSkillBtn);
//    (also add: static bool hoverCards = false; to the skill hover vars block)
//    (and in the else/false block: hoverCards = false;)
//
//  ── G. Skills UI — DrawSkillsUI: add after the Elemental block: ──
//    Rectangle cardsSkillBtn = new Rectangle(ScreenWidth - 320, ScreenHeight - 380, 140, 40);
//    Color cardsColor = hoverCards ? Color.Gold : Color.White;
//    Raylib.DrawRectangleRec(cardsSkillBtn, new Color((byte)0,(byte)0,(byte)0,(byte)200));
//    Raylib.DrawRectangleLinesEx(cardsSkillBtn, 2, cardsColor);
//    Raylib.DrawText($"Cards Lv {player.PlayingCardsLevel}", ScreenWidth - 315, ScreenHeight - 368, 20, cardsColor);
//    if (!hoverCards) {
//        int req = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
//        float prog = (float)player.PlayingCardsXP / req;
//        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 343, 140, 8, new Color((byte)40,(byte)40,(byte)40,(byte)255));
//        Raylib.DrawRectangle(ScreenWidth - 320, ScreenHeight - 343, (int)(140 * prog), 8, Color.Gold);
//    } else {
//        Raylib.DrawText($"Euchre:  {player.EuchreRating}", ScreenWidth - 315, ScreenHeight - 368, 14, Color.LightGray);
//        Raylib.DrawText($"500:     {player.FiveHundredRating}", ScreenWidth - 315, ScreenHeight - 352, 14, Color.LightGray);
//        Raylib.DrawText($"Seq:     {player.SequenceRating}", ScreenWidth - 315, ScreenHeight - 336, 14, Color.LightGray);
//    }
//
//  ── H. Also update the skillsOpen panel height guard in UpdateSkillsUI: ──
//    Change: new Rectangle(ScreenWidth - 160, ScreenHeight - 390, 140, 335)
//    To:     new Rectangle(ScreenWidth - 320, ScreenHeight - 430, 320, 375)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
partial class Program
{
    // ─────────────────────────────────────────────────────────────────────
    //  HUB STATE
    // ─────────────────────────────────────────────────────────────────────

    enum HubScreen { GameSelect, ModeSelect, Tutorial, TutorialHelper, SeatSelect, Playing }

    static bool hubActive = false;
    static HubScreen hubScreen = HubScreen.GameSelect;
    static CardGameType hubGame = CardGameType.Euchre;
    static bool hubPracticeMode = false;   // practice = no rating change

    // tutorial page state
    static int tutPage = 0;
    static int tutHelperStep = 0;

    // ─────────────────────────────────────────────────────────────────────
    //  RATING HELPERS
    // ─────────────────────────────────────────────────────────────────────
    static int GetRating(CardGameType g) => g switch
    {
        CardGameType.Euchre       => player.EuchreRating,
        CardGameType.FiveHundred  => player.FiveHundredRating,
        CardGameType.Sequence     => player.SequenceRating,
        _ => 200
    };

    static void SetRating(CardGameType g, int v)
    {
        int clamped = Math.Clamp(v, 200, 1000);
        switch (g)
        {
            case CardGameType.Euchre:      player.EuchreRating      = clamped; break;
            case CardGameType.FiveHundred: player.FiveHundredRating = clamped; break;
            case CardGameType.Sequence:    player.SequenceRating    = clamped; break;
        }
    }

    // Call at end of each rated game with win=true/false
    static void RecordGameResult(CardGameType g, bool win)
    {
        if (hubPracticeMode) return;
        if (win)
        {
            SetRating(g, GetRating(g) + 10);
            switch (g)
            {
                case CardGameType.Euchre:      player.EuchreWins++;      break;
                case CardGameType.FiveHundred: player.FiveHundredWins++; break;
                case CardGameType.Sequence:    player.SequenceWins++;    break;
            }
            AddPlayingCardsXP(20);
            ShowNotification($"Rating up! {g}: {GetRating(g)}");
        }
        else
        {
            SetRating(g, GetRating(g) - 5);   // small loss penalty
            AddPlayingCardsXP(5);
        }
    }

    static int seatSelectPickedSeat = -1;

static void DrawHubSeatSelect()
{
    string gameName = hubGame switch
    {
        CardGameType.Euchre      => "EUCHRE",
        CardGameType.FiveHundred => "FIVE HUNDRED",
        CardGameType.Sequence    => "SEQUENCE",
        _ => ""
    };
    Raylib.DrawText($"{gameName} — CHOOSE YOUR SEAT", ScreenWidth/2 - 220, 80, 32, Color.Gold);
    Raylib.DrawText("Empty seats are filled by AI.", ScreenWidth/2 - 130, 124, 18, Color.LightGray);

    Vector2 mouse = Raylib.GetMousePosition();

    string[] seatLabels = { "SOUTH (You/Host)", "WEST", "NORTH (Partner)", "EAST" };

    for (int seat = 0; seat < 4; seat++)
    {
        int bx = ScreenWidth/2 - 260;
        int by = 170 + seat * 90;
        Rectangle btn = new Rectangle(bx, by, 520, 75);
        bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

        bool isHostSeat = seat == 0; // host always anchors seat 0 by convention
        bool taken = cardSeatOwner[seat] > 0; // occupied by a different remote player already
        bool isThisLocalChoice = seatSelectPickedSeat == seat;

        Color baseCol = isThisLocalChoice
            ? new Color((byte)60,(byte)100,(byte)60,(byte)255)
            : taken
                ? new Color((byte)60,(byte)30,(byte)30,(byte)255)
                : new Color((byte)25,(byte)45,(byte)25,(byte)255);

        Raylib.DrawRectangleRec(btn, baseCol);
        Raylib.DrawRectangleLinesEx(btn, 2,
            isThisLocalChoice ? Color.Gold
            : taken ? Color.Red
            : hover ? Color.SkyBlue
            : new Color((byte)80,(byte)120,(byte)80,(byte)255));

        Raylib.DrawText(seatLabels[seat], bx + 20, by + 12, 22, Color.White);

        string status = isThisLocalChoice ? "YOU"
            : taken ? $"Taken (Player {cardSeatOwner[seat]})"
            : "Open — AI or click to claim";
        Raylib.DrawText(status, bx + 20, by + 42, 16,
            isThisLocalChoice ? Color.Gold : taken ? Color.Red : Color.LightGray);

        // host cannot click a taken seat; non-host players can't claim host's seat 0
        bool clickable = !taken && (multiplayer.IsHost || seat != 0);

        if (hover && clickable && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            // release any seat we previously picked
            if (seatSelectPickedSeat >= 0)
                cardSeatOwner[seatSelectPickedSeat] = -1;

            seatSelectPickedSeat = seat;
            int myNetId = multiplayer.IsHost ? 0 : multiplayer.MyId;
            cardSeatOwner[seat] = myNetId;

            // tell the host (or, if we ARE the host, just update locally — host is authoritative)
            if (!multiplayer.IsHost)
                multiplayer.SendCardAction($"SEAT|{seat}");
        }
    }

    // START button — only the host can actually start the game
    if (multiplayer.IsHost)
    {
        Rectangle startBtn = new Rectangle(ScreenWidth/2 - 110, 560, 220, 50);
        bool hoverStart = Raylib.CheckCollisionPointRec(mouse, startBtn);
        Raylib.DrawRectangleRec(startBtn, hoverStart ? new Color((byte)50,(byte)100,(byte)50,(byte)255) : new Color((byte)30,(byte)60,(byte)30,(byte)255));
        Raylib.DrawRectangleLinesEx(startBtn, 2, hoverStart ? Color.Gold : Color.White);
        Raylib.DrawText("START GAME", (int)startBtn.X + 30, (int)startBtn.Y + 14, 22, hoverStart ? Color.Gold : Color.White);

        if (hoverStart && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            cardSeatOwner[0] = 0; // host always owns seat 0 if unclaimed
            hubPracticeMode = false;
            cardGameStarted = true;

            // tell every client which game to launch, before sending state
            if (multiplayer.Connected)
                multiplayer.BroadcastCardTableState($"HUBSTART|{(int)hubGame}|" +
                    $"{cardSeatOwner[0]}|{cardSeatOwner[1]}|{cardSeatOwner[2]}|{cardSeatOwner[3]}");

            LaunchGame(hubGame);
            BroadcastCardTableState();
        }
    }
    else
    {
        Raylib.DrawText("Waiting for host to start...", ScreenWidth/2 - 130, 560, 20, Color.LightGray);
    }

    Raylib.DrawText("ESC = Back", ScreenWidth/2 - 40, ScreenHeight - 60, 20, Color.LightGray);
    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        if (seatSelectPickedSeat >= 0) cardSeatOwner[seatSelectPickedSeat] = -1;
        seatSelectPickedSeat = -1;
        hubScreen = HubScreen.ModeSelect;
    }
}

    // Scale AI difficulty from rating: 200→0.25 skill, 1000→0.95 skill
    static float AiSkillForRating(CardGameType g)
    {
        float t = (GetRating(g) - 200f) / 800f;   // 0..1
        return Math.Clamp(0.25f + t * 0.70f, 0.25f, 0.95f);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  ENTRY POINT  — replaces direct StartCardGame / StartSequenceGame calls
    // ─────────────────────────────────────────────────────────────────────
    static void OpenCardsHub(CardGameType game)
    {
        hubActive  = true;
        hubGame    = game;
        hubScreen  = HubScreen.GameSelect;
        tutPage    = 0;
        tutHelperStep = 0;
        currentScene = SceneState.CardGame;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UPDATE
    // ─────────────────────────────────────────────────────────────────────
    static void UpdateCardsHub(float dt)
    {
        if (!hubActive) return;

        // route to active game if we've left the hub
        if (hubScreen == HubScreen.Playing)
        {
            if (seqActive)          { UpdateSequenceGame(dt); return; }
            UpdateCardGame_Inner(dt);
            return;
        }

        if (hubScreen == HubScreen.TutorialHelper)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
                tutHelperStep++;
            if (Raylib.IsKeyPressed(KeyboardKey.Left)  || Raylib.IsKeyPressed(KeyboardKey.A))
                tutHelperStep = Math.Max(0, tutHelperStep - 1);
            if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Q))
                hubScreen = HubScreen.ModeSelect;
            return;
        }

        if (hubScreen == HubScreen.Tutorial)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
                tutPage++;
            if (Raylib.IsKeyPressed(KeyboardKey.Left)  || Raylib.IsKeyPressed(KeyboardKey.A))
                tutPage = Math.Max(0, tutPage - 1);
            if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Q))
                hubScreen = HubScreen.GameSelect;
            return;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DRAW  — main hub router
    // ─────────────────────────────────────────────────────────────────────
    static void DrawCardsHub()
    {
        if (!hubActive) return;

        if (hubScreen == HubScreen.Playing)
        {
            if (seqActive) { DrawSequenceGame(); return; }
            DrawCardGame_Inner();
            return;
        }

        Raylib.ClearBackground(new Color((byte)15,(byte)20,(byte)30,(byte)255));

        // felt table background
        Raylib.DrawRectangle(60, 50, ScreenWidth - 120, ScreenHeight - 100,
            new Color((byte)18,(byte)60,(byte)35,(byte)255));
        Raylib.DrawRectangleLines(60, 50, ScreenWidth - 120, ScreenHeight - 100,
            new Color((byte)120,(byte)90,(byte)40,(byte)255));

        switch (hubScreen)
        {
            case HubScreen.GameSelect:  DrawHubGameSelect(); break;
            case HubScreen.ModeSelect:  DrawHubModeSelect(); break;
            case HubScreen.Tutorial:    DrawHubTutorial();   break;
            case HubScreen.TutorialHelper: DrawTutorialHelper(); break;
            case HubScreen.SeatSelect:  DrawHubSeatSelect();  break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SCREEN: GAME SELECT
    // ─────────────────────────────────────────────────────────────────────
    static void DrawHubGameSelect()
    {
        Raylib.DrawText("CARD GAMES", ScreenWidth/2 - 100, 80, 36, Color.Gold);
        Raylib.DrawText($"Cards Level {player.PlayingCardsLevel}", ScreenWidth/2 - 70, 124, 20, Color.LightGray);

        Vector2 mouse = Raylib.GetMousePosition();

        (CardGameType game, string label, string desc, int rating, int wins)[] games =
        {
            (CardGameType.Euchre,
             "EUCHRE",
             "Trick-taking, 2v2. First to 10 points wins.",
             player.EuchreRating, player.EuchreWins),
            (CardGameType.FiveHundred,
             "FIVE HUNDRED",
             "Bid & trick game, 2v2. First to 500 points wins.",
             player.FiveHundredRating, player.FiveHundredWins),
            (CardGameType.Sequence,
             "SEQUENCE",
             "Board card game, 2v2. Get 2 sequences of 5 to win.",
             player.SequenceRating, player.SequenceWins),
        };

        for (int i = 0; i < games.Length; i++)
        {
            var g = games[i];
            int bx = ScreenWidth/2 - 300;
            int by = 180 + i * 140;
            Rectangle btn = new Rectangle(bx, by, 600, 110);
            bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

            Raylib.DrawRectangleRec(btn, hover
                ? new Color((byte)50,(byte)80,(byte)50,(byte)255)
                : new Color((byte)25,(byte)45,(byte)25,(byte)255));
            Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : new Color((byte)80,(byte)120,(byte)80,(byte)255));

            Raylib.DrawText(g.label, bx + 20, by + 14, 26, hover ? Color.Gold : Color.White);
            Raylib.DrawText(g.desc,  bx + 20, by + 46, 17, Color.LightGray);

            // rating bar
            float ratingT = (g.rating - 200f) / 800f;
            Raylib.DrawRectangle(bx + 20, by + 74, 300, 10, new Color((byte)40,(byte)40,(byte)40,(byte)255));
            Raylib.DrawRectangle(bx + 20, by + 74, (int)(300 * ratingT), 10, RatingColor(g.rating));
            Raylib.DrawText($"Rating: {g.rating}  |  Wins: {g.wins}", bx + 330, by + 72, 16, Color.LightGray);

            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                hubGame   = g.game;
                hubScreen = HubScreen.ModeSelect;
                tutPage   = 0;
                tutHelperStep = 0;
            }
        }

        // Q to exit back to building
        Raylib.DrawText("Q = Exit", ScreenWidth/2 - 30, ScreenHeight - 60, 20, Color.LightGray);
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            hubActive = false;
            currentScene = SceneState.Building;
        }
    }

    static Color RatingColor(int r)
    {
        if (r < 400) return new Color((byte)100,(byte)180,(byte)100,(byte)255);
        if (r < 600) return new Color((byte)180,(byte)180,(byte)60,(byte)255);
        if (r < 800) return new Color((byte)220,(byte)120,(byte)40,(byte)255);
        return new Color((byte)220,(byte)60,(byte)60,(byte)255);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SCREEN: MODE SELECT  (Play / Practice / Tutorial Helper)
    // ─────────────────────────────────────────────────────────────────────
    static void DrawHubModeSelect()
    {
        string gameName = hubGame switch
        {
            CardGameType.Euchre      => "EUCHRE",
            CardGameType.FiveHundred => "FIVE HUNDRED",
            CardGameType.Sequence    => "SEQUENCE",
            _ => ""
        };
        Raylib.DrawText(gameName, ScreenWidth/2 - Raylib.MeasureText(gameName,36)/2, 80, 36, Color.Gold);
        Raylib.DrawText($"Your rating: {GetRating(hubGame)}",
            ScreenWidth/2 - 70, 128, 20,  RatingColor(GetRating(hubGame)));

        Vector2 mouse = Raylib.GetMousePosition();

        // Three option buttons
        var optionsList = new List<(string label, string sublabel, Action action)>
        {
            ("PLAY",
            "Rated game — win to increase your rating",
            () => { hubPracticeMode = false; LaunchGame(hubGame); }),
            ("PRACTICE",
            "No rating change — play casually",
            () => { hubPracticeMode = true;  LaunchGame(hubGame); }),
            ("TUTORIAL HELPER",
            "Step-by-step walkthrough of the game",
            () => { hubScreen = HubScreen.TutorialHelper; tutHelperStep = 0; }),
        };

        if (multiplayer.Connected)
        {
            optionsList.Add((
                "MULTIPLAYER",
                "Play with connected players — choose seats",
                () => { hubScreen = HubScreen.SeatSelect; }
            ));
        }

        var options = optionsList.ToArray();

        for (int i = 0; i < options.Length; i++)
        {
            int bx = ScreenWidth/2 - 260;
            int by = 200 + i * 130;
            Rectangle btn = new Rectangle(bx, by, 520, 100);
            bool hover = Raylib.CheckCollisionPointRec(mouse, btn);

            Raylib.DrawRectangleRec(btn, hover
                ? new Color((byte)50,(byte)80,(byte)50,(byte)255)
                : new Color((byte)25,(byte)45,(byte)25,(byte)255));
            Raylib.DrawRectangleLinesEx(btn, 2, hover ? Color.Gold : new Color((byte)80,(byte)120,(byte)80,(byte)255));
            Raylib.DrawText(options[i].label,    bx + 20, by + 18, 28, hover ? Color.Gold : Color.White);
            Raylib.DrawText(options[i].sublabel, bx + 20, by + 56, 16, Color.LightGray);

            if (hover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                options[i].action();
        }

        Raylib.DrawText("ESC = Back", ScreenWidth/2 - 40, ScreenHeight - 60, 20, Color.LightGray);
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            hubScreen = HubScreen.GameSelect;
    }

    static void LaunchGame(CardGameType g)
    {
        hubScreen = HubScreen.Playing;
        if (g == CardGameType.Sequence)
            StartSequenceGame();
        else
            StartCardGame_Internal(g == CardGameType.Euchre
                ? CardGameType.Euchre : CardGameType.FiveHundred);
    }

    // thin wrapper so we can call the original StartCardGame logic
    static void StartCardGame_Internal(CardGameType type)
    {
        cardGameType = type;
        returnFromCardsPos = player.Position;
        teamScore[0] = teamScore[1] = 0;
        dealer = cardRng.Next(4);
        targetScore = type == CardGameType.Euchre ? 10 : 500;
        StartNewHand();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SCREEN: TUTORIAL  (how-to-play text pages)
    // ─────────────────────────────────────────────────────────────────────
    static void DrawHubTutorial()
    {
        var pages = TutorialPages(hubGame);
        tutPage = Math.Clamp(tutPage, 0, pages.Length - 1);

        Raylib.DrawText($"HOW TO PLAY — {hubGame.ToString().ToUpper()}",
            ScreenWidth/2 - 200, 70, 28, Color.Gold);

        // page content
        int ty = 130;
        foreach (string line in pages[tutPage])
        {
            bool isHeading = line.StartsWith("##");
            string text = isHeading ? line.Substring(2).Trim() : line;
            int size = isHeading ? 22 : 17;
            Color col = isHeading ? Color.Gold : Color.White;
            Raylib.DrawText(text, 120, ty, size, col);
            ty += isHeading ? 32 : 24;
        }

        // pager
        Raylib.DrawText($"Page {tutPage+1} / {pages.Length}", ScreenWidth/2 - 40, ScreenHeight - 80, 18, Color.LightGray);
        if (tutPage > 0)
            Raylib.DrawText("← A/Left", 120, ScreenHeight - 80, 18, Color.LightGray);
        if (tutPage < pages.Length - 1)
            Raylib.DrawText("D/Right →", ScreenWidth - 200, ScreenHeight - 80, 18, Color.LightGray);
        Raylib.DrawText("Q = Back", ScreenWidth/2 - 30, ScreenHeight - 55, 18, Color.LightGray);
    }

    static string[][] TutorialPages(CardGameType g)
    {
        if (g == CardGameType.Euchre) return new[]
        {
            new[]{ "## GOAL",
                "Be the first team to 10 points.",
                "You are partnered with Rala (North). Joy and Tipene are your opponents." },
            new[]{ "## THE DECK",
                "Euchre uses 24 cards: 9, 10, J, Q, K, A in all 4 suits.",
                "Each player gets 5 cards. One card is turned face-up — this is the",
                "proposed trump suit." },
            new[]{ "## BIDDING",
                "Starting left of dealer, each player passes or orders up the turned card.",
                "If ordered up, dealer picks it up and discards one card.",
                "If everyone passes, players can name any other suit as trump or pass again.",
                "If everyone passes twice, the hand is redealt." },
            new[]{ "## JACKS — THE BOWERS",
                "The Jack of the trump suit = Right Bower (highest card in the game).",
                "The Jack of the same colour suit = Left Bower (second highest trump).",
                "Example: Spades trump → J♠ is right bower, J♣ is left bower." },
            new[]{ "## PLAYING TRICKS",
                "The player left of dealer leads the first card.",
                "You MUST follow the led suit if you can. Trump beats all other suits.",
                "Highest card of the led suit wins unless trump is played.",
                "Winner of each trick leads the next." },
            new[]{ "## SCORING",
                "Making the contract (winning 3-4 tricks): 1 point.",
                "Winning all 5 tricks (March): 2 points.",
                "Going alone and marching: 4 points.",
                "Getting euchred (maker wins fewer than 3 tricks): 2 points to defenders." },
        };

        if (g == CardGameType.FiveHundred) return new[]
        {
            new[]{ "## GOAL",
                "Be the first team to reach 500 points (or force opponents to -500).",
                "You partner with Rala. Joy and Tipene are opponents." },
            new[]{ "## THE DECK",
                "500 uses a 43-card deck: 4♦ 4♥, 5-A in all suits, plus a Joker.",
                "Each player receives 10 cards. 3 cards go to a face-down kitty." },
            new[]{ "## BIDDING",
                "Players bid how many tricks (6–10) they'll win and which suit is trump.",
                "Highest bid wins. The winner takes the 3-card kitty, adds them to hand,",
                "then discards 3 cards to bring their hand back to 10." },
            new[]{ "## THE JOKER & BOWERS",
                "Joker = absolute highest card (always trump).",
                "Right bower = Jack of trump suit (second highest).",
                "Left bower = Jack of same colour suit (third highest trump).",
                "No Trump bids: Joker is still highest but it can be led as any suit." },
            new[]{ "## PLAYING TRICKS",
                "Winner of bid leads first. Must follow suit if able.",
                "Trump beats all other suits. Joker is always trump unless No Trump bid." },
            new[]{ "## SCORING",
                "Making your bid: score the bid value (6♠=40, 7♥=160, 10NT=520 etc).",
                "Failing your bid: subtract the bid value from your score.",
                "Defending team: 10 points per trick won, regardless of outcome." },
        };

        // Sequence
        return new[]
        {
            new[]{ "## GOAL",
                "Be the first team to complete TWO sequences.",
                "A sequence is 5 chips in a row — horizontal, vertical, or diagonal.",
                "You partner with Jake (North). Shack and C Ride are your opponents." },
            new[]{ "## THE BOARD",
                "The board is a 10×10 grid. Each card appears twice (except Jacks).",
                "The 4 corner squares are FREE spaces — they count as chips for everyone.",
                "Blue chips = your team (You + Jake). Red chips = opponents." },
            new[]{ "## HOW TO PLAY",
                "On your turn, play a card from your hand.",
                "Place one of your chips on any matching square on the board.",
                "Then draw a card from the deck to replace it.",
                "First team with 2 completed sequences wins the game." },
            new[]{ "## THE JACKS",
                "Two-eyed Jacks (♣J ♦J) = WILD: place your chip on any empty square.",
                "One-eyed Jacks (♥J ♠J) = ANTI-WILD: remove one of your opponent's chips.",
                "One-eyed Jacks cannot remove chips that are part of a completed sequence." },
            new[]{ "## DEAD CARDS",
                "If your card matches a square already occupied (by anyone), it is a dead card.",
                "Press D to discard a dead card and draw a new one.",
                "You do not get to place a chip on a dead card turn." },
            new[]{ "## TIPS",
                "Build toward the corners — free spaces count as your chips.",
                "Block your opponents early if they start a sequence.",
                "Use two-eyed Jacks to complete your own sequence in one move.",
                "Save one-eyed Jacks to remove a chip that threatens to complete a sequence." },
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SCREEN: TUTORIAL HELPER  (interactive step-by-step visual guide)
    // ─────────────────────────────────────────────────────────────────────
    static void DrawTutorialHelper()
    {
        var steps = TutorialHelperSteps(hubGame);
        tutHelperStep = Math.Clamp(tutHelperStep, 0, steps.Length - 1);
        var step = steps[tutHelperStep];

        Raylib.DrawText("TUTORIAL HELPER", ScreenWidth/2 - 130, 72, 30, Color.Gold);
        Raylib.DrawText($"Step {tutHelperStep+1} of {steps.Length}", ScreenWidth/2 - 50, 110, 18, Color.LightGray);

        // big step title
        int tw = Raylib.MeasureText(step.title, 26);
        Raylib.DrawText(step.title, ScreenWidth/2 - tw/2, 148, 26, Color.White);

        // visual demo panel
        Raylib.DrawRectangle(120, 188, ScreenWidth - 240, 280,
            new Color((byte)20,(byte)50,(byte)20,(byte)255));
        Raylib.DrawRectangleLines(120, 188, ScreenWidth - 240, 280,
            new Color((byte)100,(byte)140,(byte)60,(byte)255));

        step.drawDemo(120, 188, ScreenWidth - 240, 280);

        // explanation text
        int ey = 488;
        foreach (string line in step.lines)
        {
            Raylib.DrawText(line, 130, ey, 17, Color.White);
            ey += 24;
        }

        // nav
        if (tutHelperStep > 0)
        {
            Rectangle prevBtn = new Rectangle(130, ScreenHeight - 70, 120, 40);
            bool hp = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), prevBtn);
            Raylib.DrawRectangleRec(prevBtn, new Color((byte)30,(byte)50,(byte)30,(byte)255));
            Raylib.DrawRectangleLinesEx(prevBtn, 2, hp ? Color.Gold : Color.White);
            Raylib.DrawText("← PREV", 145, ScreenHeight - 58, 18, hp ? Color.Gold : Color.White);
            if (hp && Raylib.IsMouseButtonPressed(MouseButton.Left)) tutHelperStep--;
        }

        if (tutHelperStep < steps.Length - 1)
        {
            Rectangle nextBtn = new Rectangle(ScreenWidth - 250, ScreenHeight - 70, 120, 40);
            bool hn = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), nextBtn);
            Raylib.DrawRectangleRec(nextBtn, new Color((byte)30,(byte)50,(byte)30,(byte)255));
            Raylib.DrawRectangleLinesEx(nextBtn, 2, hn ? Color.Gold : Color.White);
            Raylib.DrawText("NEXT →", ScreenWidth - 238, ScreenHeight - 58, 18, hn ? Color.Gold : Color.White);
            if (hn && Raylib.IsMouseButtonPressed(MouseButton.Left)) tutHelperStep++;
        }

        Raylib.DrawText("ESC = Back", ScreenWidth/2 - 40, ScreenHeight - 58, 18, Color.LightGray);
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) hubScreen = HubScreen.ModeSelect;
    }

    // Each step has: title, explanation lines, and a demo draw lambda
    record TutStep(string title, string[] lines, Action<int,int,int,int> drawDemo);

    static TutStep[] TutorialHelperSteps(CardGameType g)
    {
        if (g == CardGameType.Sequence) return new[]
        {
            new TutStep("The Board",
                new[]{ "The 10×10 board has every card (except Jacks) placed twice.",
                       "The 4 corners are FREE spaces — they count as chips for everyone." },
                (bx,by,bw,bh) => {
                    // mini board sketch
                    int cs = 22;
                    for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++)
                    {
                        bool corner = (r==0||r==5)&&(c==0||c==5);
                        Raylib.DrawRectangle(bx+20+c*cs, by+20+r*cs, cs-1, cs-1,
                            corner ? new Color((byte)180,(byte)180,(byte)60,(byte)200)
                                   : new Color((byte)30,(byte)55,(byte)30,(byte)255));
                        Raylib.DrawRectangleLines(bx+20+c*cs, by+20+r*cs, cs-1, cs-1,
                            new Color((byte)60,(byte)100,(byte)60,(byte)255));
                        if (corner) Raylib.DrawText("F", bx+20+c*cs+6, by+20+r*cs+4, 12, Color.Gold);
                    }
                    Raylib.DrawText("← FREE corner", bx+160, by+24, 15, Color.Gold);
                }),

            new TutStep("Placing a Chip",
                new[]{ "Select a card from your hand (press 1–7).",
                       "Click the matching square on the board to place your chip.",
                       "Blue = your team, Red = opponents." },
                (bx,by,bw,bh) => {
                    // show a card + matching board cell + chip
                    DrawCardFace(new Card(2, 9), bx+40, by+80, true);
                    Raylib.DrawText("→", bx+120, by+112, 28, Color.Gold);
                    Raylib.DrawRectangle(bx+160, by+82, 54, 76, new Color((byte)60,(byte)90,(byte)20,(byte)255));
                    Raylib.DrawRectangleLines(bx+160, by+82, 54, 76, Color.Yellow);
                    DrawCardFace(new Card(2, 9), bx+161, by+83, true);
                    Raylib.DrawCircle(bx+187, by+120, 18, new Color((byte)60,(byte)130,(byte)255,(byte)220));
                    Raylib.DrawText("Chip placed!", bx+230, by+112, 16, Color.White);
                }),

            new TutStep("Two-Eyed Jack — Wild",
                new[]{ "Clubs J or Diamonds J = two-eyed jack.",
                       "Play it to place your chip on ANY empty square.",
                       "Great for completing or extending a sequence." },
                (bx,by,bw,bh) => {
                    DrawCardFace(new Card(0, 11), bx+40, by+80, true);
                    Raylib.DrawText("♣J = WILD", bx+40, by+175, 16, Color.Green);
                    Raylib.DrawText("→ Place anywhere", bx+140, by+112, 17, Color.White);
                    // show several cells highlighted
                    for (int i = 0; i < 4; i++)
                        Raylib.DrawRectangle(bx+280+i*60, by+88, 52, 74,
                            new Color((byte)60,(byte)90,(byte)20,(byte)200));
                    Raylib.DrawText("Any empty square", bx+280, by+172, 15, Color.LightGray);
                }),

            new TutStep("One-Eyed Jack — Remove",
                new[]{ "Hearts J or Spades J = one-eyed jack.",
                       "Play it to REMOVE an opponent's chip from any square.",
                       "Cannot remove chips that form part of a completed sequence." },
                (bx,by,bw,bh) => {
                    DrawCardFace(new Card(2, 11), bx+40, by+80, true);
                    Raylib.DrawText("♥J = REMOVE", bx+40, by+175, 16, Color.Red);
                    // show an opponent chip being removed
                    Raylib.DrawCircle(bx+260, by+120, 22, new Color((byte)255,(byte)70,(byte)70,(byte)220));
                    Raylib.DrawText("✗", bx+250, by+105, 32, Color.Red);
                    Raylib.DrawText("Opponent chip removed", bx+210, by+155, 14, Color.LightGray);
                }),

            new TutStep("Building a Sequence",
                new[]{ "Get 5 of your team's chips in a row — horizontal, vertical or diagonal.",
                       "FREE corner squares count as your chip.",
                       "Complete TWO sequences to win the game." },
                (bx,by,bw,bh) => {
                    int cs = 36;
                    for (int c = 0; c < 5; c++)
                    {
                        Raylib.DrawRectangle(bx+60+c*cs, by+100, cs-2, cs-2,
                            new Color((byte)30,(byte)55,(byte)30,(byte)255));
                        Raylib.DrawRectangleLines(bx+60+c*cs, by+100, cs-2, cs-2, Color.Gold);
                        Raylib.DrawCircle(bx+60+c*cs+17, by+118, 14,
                            new Color((byte)60,(byte)130,(byte)255,(byte)220));
                    }
                    Raylib.DrawText("← 5 in a row = ONE SEQUENCE", bx+60, by+148, 16, Color.Gold);
                    Raylib.DrawText("Need 2 sequences to WIN", bx+60, by+172, 16, Color.White);
                }),
        };

        if (g == CardGameType.Euchre) return new[]
        {
            new TutStep("Your Hand & Trump",
                new[]{ "You hold 5 cards. One card is turned up — this is the proposed trump.",
                       "The Jack of trump (Right Bower) is the highest card.",
                       "The Jack of the same colour (Left Bower) is second highest." },
                (bx,by,bw,bh) => {
                    Card[] hand = { new Card(3,11), new Card(3,14), new Card(3,10), new Card(0,11), new Card(1,12) };
                    for (int i = 0; i < hand.Length; i++) DrawCardFace(hand[i], bx+30+i*66, by+80, true);
                    Raylib.DrawText("Spades trump: J♠=Right Bower  J♣=Left Bower", bx+30, by+176, 15, Color.Gold);
                }),
            new TutStep("Bidding",
                new[]{ "Starting left of dealer, each player decides to Order Up or Pass.",
                       "Ordering up means the dealer takes the turned card as trump.",
                       "If all pass, each player may name a different suit as trump." },
                (bx,by,bw,bh) => {
                    Raylib.DrawRectangle(bx+bw/2-80, by+60, 160, 50,
                        new Color((byte)30,(byte)30,(byte)45,(byte)255));
                    Raylib.DrawRectangleLinesEx(new Rectangle(bx+bw/2-80, by+60, 160, 50), 2, Color.Gold);
                    Raylib.DrawText("ORDER UP", bx+bw/2-50, by+77, 20, Color.Gold);
                    Raylib.DrawRectangle(bx+bw/2-80, by+122, 160, 50,
                        new Color((byte)30,(byte)30,(byte)45,(byte)255));
                    Raylib.DrawRectangleLinesEx(new Rectangle(bx+bw/2-80, by+122, 160, 50), 2, Color.White);
                    Raylib.DrawText("PASS", bx+bw/2-26, by+139, 20, Color.White);
                }),
            new TutStep("Winning Tricks",
                new[]{ "Must follow the led suit if you can. Trump beats everything.",
                       "Highest card of the led suit wins unless trump is played.",
                       "Win 3 tricks to make your contract. Win all 5 for a March (+2 pts)." },
                (bx,by,bw,bh) => {
                    // show a trick
                    (int,int)[] spots = { (bw/2-25, bh-80), (80, bh/2), (bw/2-25, 30), (bw-130, bh/2) };
                    Card[] played = { new Card(3,14), new Card(2,10), new Card(3,11), new Card(0,9) };
                    for (int i = 0; i < 4; i++) DrawCardFace(played[i], bx+spots[i].Item1, by+spots[i].Item2, false);
                    Raylib.DrawText("J♠ wins! (Right Bower)", bx+bw/2-70, by+bh/2-10, 15, Color.Gold);
                }),
        };

        // 500 steps
        return new[]
        {
            new TutStep("Bidding in 500",
                new[]{ "Highest bidder wins the kitty (3 extra cards).",
                       "They add kitty to hand, then discard 3 to return to 10 cards.",
                       "Bid = how many tricks you promise to win this hand." },
                (bx,by,bw,bh) => {
                    string[] bids = { "6♠", "7♥", "8♦", "Pass" };
                    for (int i = 0; i < 4; i++)
                    {
                        bool last = i == 3;
                        Raylib.DrawRectangle(bx+60+i*120, by+80, 100, 50,
                            new Color((byte)30,(byte)30,(byte)45,(byte)255));
                        Raylib.DrawRectangleLinesEx(new Rectangle(bx+60+i*120,by+80,100,50), 2,
                            last ? Color.White : Color.Gold);
                        int tw2 = Raylib.MeasureText(bids[i], 22);
                        Raylib.DrawText(bids[i], bx+60+i*120+50-tw2/2, by+93, 22,
                            last ? Color.LightGray : Color.Gold);
                    }
                }),
            new TutStep("The Joker",
                new[]{ "The Joker is the highest card — it always wins the trick.",
                       "It belongs to the trump suit unless No Trump was bid.",
                       "In No Trump, the player who leads the Joker calls its suit." },
                (bx,by,bw,bh) => {
                    DrawCardFace(new Card(-1,15), bx+bw/2-30, by+60, true);
                    Raylib.DrawText("Always wins!", bx+bw/2-42, by+160, 17, Color.Gold);
                }),
            new TutStep("Scoring",
                new[]{ "Make your bid = score the bid value.",
                       "Fail your bid = subtract the bid value (can go negative).",
                       "Defending team always scores 10 per trick they win." },
                (bx,by,bw,bh) => {
                    Raylib.DrawText("Made 7♥ (bid value 160): +160", bx+40, by+80, 17, Color.Green);
                    Raylib.DrawText("Failed 8♦ (bid value 220): -220", bx+40, by+112, 17, Color.Red);
                    Raylib.DrawText("Defenders won 4 tricks: +40",    bx+40, by+144, 17, Color.LightGray);
                }),
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    //  WRAPPERS  so the Draw/Update switch just calls one method each
    // ─────────────────────────────────────────────────────────────────────

    // Call these from DrawCardGame() and UpdateCardGame() at the very top:
    //
    //   static void DrawCardGame()  { if (hubActive) { DrawCardsHub();    return; } ... }
    //   static void UpdateCardGame(float dt) { if (hubActive) { UpdateCardsHub(dt); return; } ... }

    // Inner methods (renamed so hub can call them without recursion)
   static void DrawCardGame_Inner()
    {
        // announce turn changes once
        if (cardPhase == CardPhase.Playing && currentPlayer != lastAnnouncedTurn)
        {
            lastAnnouncedTurn = currentPlayer;
            bool mine = currentPlayer == MyViewSeat();
            ShowNotification(mine ? "Your turn!" : $"{PlayerName(currentPlayer)}'s turn");
        }
        
        Raylib.ClearBackground(new Color((byte)15,(byte)60,(byte)35,(byte)255));
        Raylib.DrawCircle(ScreenWidth/2, ScreenHeight/2, 280, new Color((byte)20,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircleLines(ScreenWidth/2, ScreenHeight/2, 280, new Color((byte)120,(byte)90,(byte)40,(byte)255));
        string title = cardGameType == CardGameType.Euchre ? "EUCHRE" : "500";
        Raylib.DrawText(title, 20, 16, 30, Color.Gold);
        Raylib.DrawText($"{TeamLabel(0)}: {teamScore[0]}", 20, 56, 22, new Color((byte)120,(byte)200,(byte)255,(byte)255));
        Raylib.DrawText($"{TeamLabel(1)}: {teamScore[1]}", 20, 82, 22, new Color((byte)255,(byte)140,(byte)140,(byte)255));
        Raylib.DrawText($"Playing Cards Lv {player.PlayingCardsLevel}", 20, 112, 18, Color.LightGray);
        if (trumpSuit >= 0)
            Raylib.DrawText($"Trump: {SuitNames[trumpSuit]}", 20, 138, 20, SuitColor(trumpSuit));
        Raylib.DrawText($"Tricks  You: {tricksWon[0]}  Them: {tricksWon[1]}", 20, 164, 18, Color.White);
        Raylib.DrawText("Q = Leave", ScreenWidth - 130, 16, 20, Color.LightGray);
        DrawSeatLabels();
        DrawOpponentHands();
        DrawCurrentTrick();
        DrawHumanHand();
        Raylib.DrawText($"MyId={multiplayer.MyId} host={multiplayer.IsHost} cur={currentPlayer} " +
            $"myView={MyViewSeat()} owners=[{cardSeatOwner[0]},{cardSeatOwner[1]},{cardSeatOwner[2]},{cardSeatOwner[3]}]",
            20, ScreenHeight - 160, 18, Color.Yellow);
        if (cardPhase == CardPhase.Bidding && currentPlayer == MyViewSeat())
            DrawBiddingUI();
        if (cardMessageTimer > 0 && cardMessage.Length > 0)
        {
            int w = Raylib.MeasureText(cardMessage, 26);
            Raylib.DrawRectangle(ScreenWidth/2 - w/2 - 14, 200, w + 28, 42, new Color((byte)0,(byte)0,(byte)0,(byte)180));
            Raylib.DrawText(cardMessage, ScreenWidth/2 - w/2, 208, 26, Color.Gold);
        }
        if (cardPhase == CardPhase.HandOver)
            Raylib.DrawText("SPACE = next hand", ScreenWidth/2 - 110, ScreenHeight - 40, 24, Color.White);
        if (cardPhase == CardPhase.GameOver)
        {
            int winTeam = teamScore[0] > teamScore[1] ? 0 : 1;
            string msg = TeamLabel(winTeam) + " WIN!";
            int w = Raylib.MeasureText(msg, 40);
            Raylib.DrawRectangle(ScreenWidth/2 - w/2 - 20, ScreenHeight/2 - 40, w + 40, 80, new Color((byte)0,(byte)0,(byte)0,(byte)220));
            Raylib.DrawText(msg, ScreenWidth/2 - w/2, ScreenHeight/2 - 24, 40, Color.Gold);
            Raylib.DrawText("SPACE = exit", ScreenWidth/2 - 70, ScreenHeight/2 + 30, 22, Color.White);
        }
        Raylib.DrawRectangle(10, ScreenHeight - 70, 230, 56, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Raylib.DrawRectangleLines(10, ScreenHeight - 70, 230, 56, Color.Gold);
        Raylib.DrawText("DEALER THIS HAND", 20, ScreenHeight - 64, 16, Color.LightGray);
        Raylib.DrawText(PlayerName(dealer), 20, ScreenHeight - 42, 24, Color.Gold);
    }

    static void UpdateCardGame_Inner(float dt)
    {
        if (cardMessageTimer > 0) cardMessageTimer -= dt;
        if (trickPending)
        {
            // only the host resolves tricks; clients wait for the broadcast
            if (!multiplayer.Connected || multiplayer.IsHost)
            {
                trickResolveDelay -= dt;
                if (trickResolveDelay <= 0f)
                {
                    trickPending = false;
                    ResolveTrick();
                    if (multiplayer.Connected) BroadcastCardTableState();
                }
            }
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            player.Position = returnFromCardsPos;
            hubActive = false;
            hubScreen = HubScreen.GameSelect;
            currentScene = SceneState.Building;
            return;
        }
        if (cardPhase == CardPhase.GameOver)
        {
            // exiting is local — either player can leave their own screen
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                player.Position = returnFromCardsPos;
                hubActive = false;
                hubScreen = HubScreen.GameSelect;
                currentScene = SceneState.Building;
            }
            return;
        }
        if (cardPhase == CardPhase.HandOver)
        {
            // only the host advances to the next hand; clients wait for the broadcast
            bool amHostHand = !multiplayer.Connected || multiplayer.IsHost;
            if (amHostHand && Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (teamScore[0] >= targetScore || teamScore[1] >= targetScore
                    || (cardGameType == CardGameType.Euchre && (teamScore[0] <= -targetScore || teamScore[1] <= -targetScore)))
                    cardPhase = CardPhase.GameOver;
                else
                {
                    dealer = (dealer + 1) % 4;
                    StartNewHand();
                }
                if (multiplayer.Connected) BroadcastCardTableState();
            }
            return;
        }
        bool amHost = !multiplayer.Connected || multiplayer.IsHost;
        if (amHost && CardSeatIsAI(currentPlayer))
        {
            aiThinkTimer -= dt;
            if (aiThinkTimer <= 0f)
            {
                aiThinkTimer = 0.7f;
                if (cardPhase == CardPhase.Bidding) AiBid(currentPlayer);
                else if (cardPhase == CardPhase.Playing) AiPlay(currentPlayer);
                if (multiplayer.Connected) BroadcastCardTableState();
            }
        }
    }
}
}
