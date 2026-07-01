// ============================================================================
//  CARD GAMES MODULE  —  Euchre & 500  (3 AI opponents, 2 teams of 2)
//  Drop this file into your project alongside Program.cs.
//  It is a `partial class Program`, so it shares all your existing statics
//  (currentScene, player, ScreenWidth, ShowNotification, etc.).
//
//  HOOK-IN CHECKLIST (only 6 tiny edits in Program.cs — see bottom of file):
//    1. enum SceneState  → already has nothing for cards; we reuse Minigame?  NO.
//                          Add: CardGame   (add to your SceneState enum)
//    2. Draw() switch     → case SceneState.CardGame: DrawCardGame(); break;
//    3. Update() switch    → case SceneState.CardGame: UpdateCardGame(dt); break;
//    4. Player class       → add: public int PlayingCardsLevel = 1;
//                                  public int PlayingCardsXP = 0;
//                            and an AddPlayingCardsXP method (provided below as
//                            a helper that lives here if you prefer).
//    5. Casino interior    → add the E-key launcher block (provided below).
//    6. Save/Load          → add PlayingCardsLevel / PlayingCardsXP two lines each.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace OpenWorldRPG
{
partial class Program
{
    // ─────────────────────────────────────────────────────────────────────
    //  CARD MODEL
    // ─────────────────────────────────────────────────────────────────────
    // Suit: 0=Clubs 1=Diamonds 2=Hearts 3=Spades
    // Rank: for Euchre 9,10,J,Q,K,A (9..14). For 500 we add 4..8 + Joker(15).
    public struct Card
    {
        public int Suit;   // 0-3, Joker uses Suit = -1
        public int Rank;   // 9..14 (Euchre), 4..14 + 15 joker (500)
        public Card(int suit, int rank) { Suit = suit; Rank = rank; }
        public bool IsJoker => Suit == -1;
    }

    static readonly string[] SuitNames  = { "Clubs", "Diamonds", "Hearts", "Spades" };
    static readonly string[] SuitGlyphs = { "C", "D", "H", "S" };
    static string RankName(int r) => r switch
    {
        1 => "A", 14 => "A", 13 => "K", 12 => "Q", 11 => "J", 15 => "Jk",
        _  => r.ToString()
    };

    static Color SuitColor(int suit) =>
        (suit == 1 || suit == 2) ? new Color((byte)200,(byte)30,(byte)30,(byte)255)   // red
                                 : new Color((byte)20,(byte)20,(byte)30,(byte)255);   // black

    // ─────────────────────────────────────────────────────────────────────
    //  GAME STATE
    // ─────────────────────────────────────────────────────────────────────
    enum CardGameType { Euchre, FiveHundred, Sequence }
    enum CardPhase { Bidding, Playing, HandOver, GameOver }

    static CardGameType cardGameType = CardGameType.Euchre;
    static CardPhase cardPhase = CardPhase.Bidding;
    static bool cardGameStarted = false;
    static int[] cardSeatOwner = { 0, -1, -1, -1 };
    static bool CardSeatIsLocalHuman(int seat) => cardSeatOwner[seat] == 0;
    static bool CardSeatIsAI(int seat)         => cardSeatOwner[seat] == -1;
    static bool CardSeatIsRemote(int seat)     => cardSeatOwner[seat] > 0;

    // 4 players: index 0 = YOU (south), 1 = West, 2 = North (your partner), 3 = East
    // Teams: {0,2} = You + North,  {1,3} = West + East
    static List<Card>[] hands = new List<Card>[4];
    static int dealer = 0;             // rotates every hand
    static int currentPlayer = 0;      // whose turn
    static int trumpSuit = -1;         // 0-3
    static int makerTeam = -1;         // which team called trump (0 = you/north, 1 = west/east)
    static int maker = -1;             // which player called
    static bool goingAlone = false;    // euchre loner
    static int[] teamScore = new int[2];
    static int[] tricksWon = new int[2];
    static float trickResolveDelay = 0f;   // pause so the last card is visible
    static bool trickPending = false;      // a full trick is waiting to resolve

    // current trick on the table: card + who played it
    static List<(Card card, int player)> currentTrick = new();
    static int trickLeader = 0;

    // bidding (euchre): round 1 = accept/pass upturned card, round 2 = name suit
    static Card upCard;                // euchre turned-up card
    static int euchreBidRound = 1;
    static int euchrePassCount = 0;

    // 500 bidding
    static int fiveHundredBid = 0;     // contract tricks (6-10)
    static int fiveHundredBidSuit = -1;
    static int fiveHundredHighBidder = -1;
    static int fiveHundredBidValue = 0;
    static List<Card> kitty = new();   // 3-card kitty in 500
    static int fiveHundredPassCount = 0;

    // UI / flow
    static string cardMessage = "";
    static float cardMessageTimer = 0f;
    static float aiThinkTimer = 0f;    // delay so AI plays aren't instant
    static Vector2 returnFromCardsPos;
    static int targetScore = 10;       // euchre to 10; 500 to 500 (set on start)
    static Random cardRng = new Random();

    // hovered/clickable card rects for the human
    static List<Rectangle> humanCardRects = new();
    static string SerializeCard(Card c) => $"{c.Suit}:{c.Rank}";
    static Card DeserializeCard(string s)
    {
        var parts = s.Split(':');
        return new Card(int.Parse(parts[0]), int.Parse(parts[1]));
    }
    static string SerializeCardList(List<Card> cards) =>
    cards == null ? "" : string.Join(",", cards.Select(SerializeCard));
    static List<Card> DeserializeCardList(string s) =>
        string.IsNullOrEmpty(s) ? new List<Card>() : s.Split(',').Select(DeserializeCard).ToList();

    // ─────────────────────────────────────────────────────────────────────
    //  XP / SKILL  (PlayingCards)
    //  If you'd rather put this in the Player class, move it there and delete here.
    // ─────────────────────────────────────────────────────────────────────
    static void AddPlayingCardsXP(int xp)
    {
        if (player.PlayingCardsLevel >= 100) return;
        player.PlayingCardsXP += xp;
        int required = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
        while (player.PlayingCardsXP >= required && player.PlayingCardsLevel < 100)
        {
            player.PlayingCardsXP -= required;
            player.PlayingCardsLevel++;
            ShowLevelUp("Playing Cards", player.PlayingCardsLevel);
            required = player.PlayingCardsLevel * player.PlayingCardsLevel * 40;
        }
    }

    // AI skill: how good opponents are, scaled by YOUR level so they keep pace.
    // 0.0 = random, 1.0 = near-optimal. Opponents get tougher as you level.
    static float AiSkill() => Math.Clamp(0.25f + player.PlayingCardsLevel * 0.0075f, 0.25f, 0.95f);

    // ─────────────────────────────────────────────────────────────────────
    //  ENTRY POINTS
    // ─────────────────────────────────────────────────────────────────────
    static void StartCardGame(CardGameType type)
    {
        cardGameType = type;
        returnFromCardsPos = player.Position;
        teamScore[0] = teamScore[1] = 0;
        dealer = cardRng.Next(4);
        targetScore = type == CardGameType.Euchre ? 10 : 500;
        currentScene = SceneState.CardGame;
        StartNewHand();
    }

    static void StartNewHand()
    {
        lastAnnouncedTurn = -1;
        for (int i = 0; i < 4; i++) hands[i] = new List<Card>();
        currentTrick.Clear();
        kitty.Clear();
        tricksWon[0] = tricksWon[1] = 0;
        trumpSuit = -1; makerTeam = -1; maker = -1; goingAlone = false;
        euchreBidRound = 1; euchrePassCount = 0;
        fiveHundredBid = 0; fiveHundredBidSuit = -1; fiveHundredHighBidder = -1;
        fiveHundredBidValue = 0; fiveHundredPassCount = 0;
         trickPending = false;
        trickResolveDelay = 0f;

        if (cardGameType == CardGameType.Euchre) DealEuchre();
        else                                     DealFiveHundred();

        cardPhase = CardPhase.Bidding;
        currentPlayer = (dealer + 1) % 4;   // left of dealer bids/leads first
        aiThinkTimer = 0.6f;
        cardMessage = $"{PlayerName(dealer)} deals.";
        cardMessageTimer = 2f;
    }

    static readonly string[] aiSeatNames = { "You", "Joy", "Rala", "Tipene" };

    static string PlayerName(int p)
    {
        if (p < 0 || p > 3) return "?";

        int owner = cardSeatOwner[p];

        // AI seat → default character name
        if (owner == -1) return aiSeatNames[p];

        // the local player's own seat
        if (owner == multiplayer.MyId)
            return multiplayer.Connected ? (playerName ?? "You") : "You";

        // a remote human's seat → look up their network name
        if (owner > 0)
        {
            lock (multiplayer.RemotePlayers)
            {
                var rp = multiplayer.RemotePlayers.Find(r => r.Id == owner);
                if (rp != null && !string.IsNullOrEmpty(rp.Name)) return rp.Name;
            }
            return $"Player {owner}";
        }

        // owner == 0 and it's not me → the host, when I'm a client
        return "Host";
    }

    static int TeamOf(int p) => (p == 0 || p == 2) ? 0 : 1;

    // Seats

    static int MyViewSeat()
    {
        if (!multiplayer.Connected) return 0;
        for (int s = 0; s < 4; s++)
            if (cardSeatOwner[s] == multiplayer.MyId) return s;
        return 0;
    }

    static bool IsMyCardSeat(int seat) => cardSeatOwner[seat] == multiplayer.MyId;

    static int SeatToScreenSlot(int seat)
    {
        int mySeat = MyViewSeat();
        return (seat - mySeat + 4) % 4;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DEALING
    // ─────────────────────────────────────────────────────────────────────
    static List<Card> BuildDeck(bool fiveHundred)
    {
        var deck = new List<Card>();
        if (!fiveHundred)
        {
            // Euchre: 9,10,J,Q,K,A in 4 suits = 24 cards
            for (int s = 0; s < 4; s++)
                for (int r = 9; r <= 14; r++)
                    deck.Add(new Card(s, r));
        }
        else
        {
            // 500 (4-player, 43 cards): 4..A in 4 suits = 44, drop the black 4s
            // → standard 500 deck of 42 + 1 Joker = 43.
            for (int s = 0; s < 4; s++)
                for (int r = 5; r <= 14; r++)
                    deck.Add(new Card(s, r));
            // add red 4s only (diamonds=1, hearts=2)
            deck.Add(new Card(1, 4));
            deck.Add(new Card(2, 4));
            // joker
            deck.Add(new Card(-1, 15));
        }
        // shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = cardRng.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
        return deck;
    }

    static void DealEuchre()
    {
        var deck = BuildDeck(false);
        int idx = 0;
        for (int p = 0; p < 4; p++)
            for (int c = 0; c < 5; c++)
                hands[p].Add(deck[idx++]);
        upCard = deck[idx++];          // turned-up card
        SortHands();
    }

    static void DealFiveHundred()
    {
        var deck = BuildDeck(true);
        int idx = 0;
        // 10 cards each, 3 to kitty
        for (int p = 0; p < 4; p++)
            for (int c = 0; c < 10; c++)
                hands[p].Add(deck[idx++]);
        for (int k = 0; k < 3; k++) kitty.Add(deck[idx++]);
        SortHands();
    }

    static void SortHands()
    {
        for (int p = 0; p < 4; p++)
            hands[p] = hands[p]
                .OrderBy(c => c.IsJoker ? 99 : c.Suit)
                .ThenBy(c => c.Rank)
                .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CARD STRENGTH  (handles euchre bowers + 500 joker)
    // ─────────────────────────────────────────────────────────────────────
    static bool SameColor(int s1, int s2)
    {
        bool red1 = s1 == 1 || s1 == 2;
        bool red2 = s2 == 1 || s2 == 2;
        return red1 == red2;
    }

    // The "effective suit" of a card given trump (left bower counts as trump).
    static int EffectiveSuit(Card c, int trump)
    {
        if (c.IsJoker) return trump;                       // joker is top trump in 500
        if (c.Rank == 11 && SameColor(c.Suit, trump) && c.Suit != trump)
            return trump;                                   // left bower
        return c.Suit;
    }

    // Strength ranking for comparing within a trick. Higher = stronger.
    static int CardStrength(Card c, int trump, int ledSuit)
    {
        int eff = EffectiveSuit(c, trump);

        // 500 joker = absolute top
        if (c.IsJoker) return 1000;

        if (eff == trump)
        {
            // trump ordering: Right bower > Left bower > A > K > Q > 10 > 9 ...
            if (c.Rank == 11 && c.Suit == trump) return 900;             // right bower
            if (c.Rank == 11 && SameColor(c.Suit, trump)) return 890;    // left bower
            return 700 + c.Rank;                                          // other trump
        }
        if (eff == ledSuit) return 100 + c.Rank;   // following suit
        return c.Rank;                              // off-suit (can't win)
    }

    static int TrickWinner(int trump)
    {
        int ledSuit = EffectiveSuit(currentTrick[0].card, trump);
        int best = 0;
        int bestStr = CardStrength(currentTrick[0].card, trump, ledSuit);
        for (int i = 1; i < currentTrick.Count; i++)
        {
            int s = CardStrength(currentTrick[i].card, trump, ledSuit);
            if (s > bestStr) { bestStr = s; best = i; }
        }
        return currentTrick[best].player;
    }

    // legal moves: must follow led suit if able (left bower belongs to trump!)
    static List<Card> LegalMoves(int p, int trump)
    {
        if (currentTrick.Count == 0) return new List<Card>(hands[p]);
        int led = EffectiveSuit(currentTrick[0].card, trump);
        var follow = hands[p].Where(c => EffectiveSuit(c, trump) == led).ToList();
        return follow.Count > 0 ? follow : new List<Card>(hands[p]);
    }

    static void DrawSuitSymbol(int suit, int cx, int cy, int size, Color col)
    {
        switch (suit)
        {
            case 2: // Hearts
                Raylib.DrawCircle(cx - size/3, cy - size/4, size/2.5f, col);
                Raylib.DrawCircle(cx + size/3, cy - size/4, size/2.5f, col);
                // counter-clockwise: bottom point, right, left
                Raylib.DrawTriangle(
                    new Vector2(cx, cy + size*0.8f),
                    new Vector2(cx + size*0.7f, cy - size/6),
                    new Vector2(cx - size*0.7f, cy - size/6), col);
                break;

            case 1: // Diamonds — two triangles, both counter-clockwise
                // top half: top point, left, right  → CCW
                Raylib.DrawTriangle(
                    new Vector2(cx, cy - size*0.8f),
                    new Vector2(cx - size*0.6f, cy),
                    new Vector2(cx + size*0.6f, cy), col);
                // bottom half: left, bottom point, right → CCW
                Raylib.DrawTriangle(
                    new Vector2(cx - size*0.6f, cy),
                    new Vector2(cx, cy + size*0.8f),
                    new Vector2(cx + size*0.6f, cy), col);
                break;

            case 0: // Clubs
                Raylib.DrawCircle(cx, cy - size/3, size/2.8f, col);
                Raylib.DrawCircle(cx - size/3, cy + size/8, size/2.8f, col);
                Raylib.DrawCircle(cx + size/3, cy + size/8, size/2.8f, col);
                Raylib.DrawRectangle(cx - size/8, cy, size/4, (int)(size*0.6f), col);
                break;

            case 3: // Spades
                // counter-clockwise: top point, left, right
                Raylib.DrawTriangle(
                    new Vector2(cx, cy - size*0.8f),
                    new Vector2(cx - size*0.6f, cy + size/4),
                    new Vector2(cx + size*0.6f, cy + size/4), col);
                Raylib.DrawCircle(cx - size/3, cy + size/6, size/2.8f, col);
                Raylib.DrawCircle(cx + size/3, cy + size/6, size/2.8f, col);
                Raylib.DrawRectangle(cx - size/8, cy, size/4, (int)(size*0.6f), col);
                break;
        }
    }

    // Broadcast table

    static void BroadcastCardTableState()
{
    if (!multiplayer.IsHost || !multiplayer.Connected) return;

    // public info — visible to everyone, no privacy concerns
    string trickStr = string.Join(";", currentTrick.Select(t => $"{SerializeCard(t.card)}:{t.player}"));
    string handCounts = string.Join(",", hands.Select(h => h?.Count ?? 0));

    string payload = $"{(int)cardGameType}|{(int)cardPhase}|{currentPlayer}|{dealer}" +
                  $"|{teamScore[0]}|{teamScore[1]}|{tricksWon[0]}|{tricksWon[1]}" +
                  $"|{trumpSuit}|{cardSeatOwner[0]}|{cardSeatOwner[1]}|{cardSeatOwner[2]}|{cardSeatOwner[3]}" +
                  $"|{handCounts}|{trickStr}" +
                  $"|{maker}|{makerTeam}|{(goingAlone ? 1 : 0)}" +
                  $"|{SerializeCard(upCard)}|{euchreBidRound}" +
                  $"|{fiveHundredBid}|{fiveHundredBidSuit}|{fiveHundredHighBidder}|{fiveHundredBidValue}" +
                  $"|{cardMessage.Replace('|', '/')}|{cardMessageTimer}" +
                  $"|{(cardGameStarted ? 1 : 0)}";

    multiplayer.BroadcastCardTableState(payload);

    // then send each connected client THEIR OWN hand privately
    for (int seat = 0; seat < 4; seat++)
    {
        if (cardSeatOwner[seat] > 0) // a remote player's seat
        {
            string ownHand = SerializeCardList(hands[seat]);
            multiplayer.SendOwnHandTo(cardSeatOwner[seat], seat, ownHand);
        }
    }
}


    // ─────────────────────────────────────────────────────────────────────
    //  UPDATE  (called from your Update switch)
    // ─────────────────────────────────────────────────────────────────────
    static void UpdateCardGame(float dt)
    {
        if (hubActive)  { UpdateCardsHub(dt);  return; }
        if (seqActive) { UpdateSequenceGame(dt); return; }
        if (cardMessageTimer > 0) cardMessageTimer -= dt;
        if (trickPending)
        {
            trickResolveDelay -= dt;
            if (trickResolveDelay <= 0f)
            {
                trickPending = false;
                ResolveTrick();
            }
            return;
        }
        // exit
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            player.Position = returnFromCardsPos;
            currentScene = SceneState.Building;
            return;
        }

        if (cardPhase == CardPhase.GameOver)
        {
            // exiting is local — either player can leave their own screen
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                player.Position = returnFromCardsPos;
                currentScene = SceneState.Building;
            }
            return;
        }

        if (cardPhase == CardPhase.HandOver)
        {
            // only the host advances to the next hand; clients wait for the broadcast
            bool amHost = !multiplayer.Connected || multiplayer.IsHost;
            if (amHost && Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                // check for game over
                if (teamScore[0] >= targetScore || teamScore[1] >= targetScore
                    || (cardGameType == CardGameType.Euchre && (teamScore[0] <= -targetScore || teamScore[1] <= -targetScore)))
                {
                    cardPhase = CardPhase.GameOver;
                }
                else
                {
                    dealer = (dealer + 1) % 4;   // dealer rotates every hand
                    StartNewHand();
                }
                if (multiplayer.Connected) BroadcastCardTableState();
            }
            return;
        }

        // AI turns run on a small timer so they feel natural
        if (multiplayer.Connected && !multiplayer.IsHost)
        {
            return; // joiner: input for their own seat is handled separately in DrawCardGame via network send
        }

        if (CardSeatIsAI(currentPlayer))
        {
            aiThinkTimer -= dt;
            if (aiThinkTimer <= 0f)
            {
                aiThinkTimer = 0.7f;
                if (cardPhase == CardPhase.Bidding) AiBid(currentPlayer);
                else if (cardPhase == CardPhase.Playing) AiPlay(currentPlayer);
                BroadcastCardTableState();
            }
            return;
        }

        if (CardSeatIsRemote(currentPlayer))
        {
            // waiting for that remote player's network input — nothing to do locally
            return;
        }

        // HUMAN turn handled by mouse clicks in DrawCardGame (bidding buttons / cards)
    }

    static void ResolveTrick()
    {
        int winner = TrickWinner(trumpSuit);
        tricksWon[TeamOf(winner)]++;
        cardMessage = $"{PlayerName(winner)} wins the trick.";
        cardMessageTimer = 1.5f;
        currentTrick.Clear();
        currentPlayer = winner;
        trickLeader = winner;

        int totalTricks = cardGameType == CardGameType.Euchre ? 5 : 10;
        if (tricksWon[0] + tricksWon[1] >= totalTricks)
            ScoreHand();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  BIDDING  — EUCHRE
    // ─────────────────────────────────────────────────────────────────────
    static void EuchreOrderUp(int p, bool alone)
    {
        trumpSuit = upCard.Suit;
        maker = p; makerTeam = TeamOf(p); goingAlone = alone;
        // dealer picks up the up-card, discards worst
        hands[dealer].Add(upCard);
        DiscardWorst(dealer, trumpSuit);
        SortHands();
        cardPhase = CardPhase.Playing;
        currentPlayer = (dealer + 1) % 4;
        trickLeader = currentPlayer;
        cardMessage = $"{PlayerName(p)} ordered up {SuitNames[trumpSuit]}" + (alone ? " ALONE!" : ".");
        cardMessageTimer = 2.5f;
        if (alone) SkipPartnerOfLoner();
    }

    static void EuchreNameSuit(int p, int suit, bool alone)
    {
        trumpSuit = suit;
        maker = p; makerTeam = TeamOf(p); goingAlone = alone;
        SortHands();
        cardPhase = CardPhase.Playing;
        currentPlayer = (dealer + 1) % 4;
        trickLeader = currentPlayer;
        cardMessage = $"{PlayerName(p)} called {SuitNames[suit]}" + (alone ? " ALONE!" : ".");
        cardMessageTimer = 2.5f;
        if (alone) SkipPartnerOfLoner();
    }

    static void SkipPartnerOfLoner()
    {
        // In SkipPartnerOfLoner(), replace the body with:
        int partner = (maker + 2) % 4;
        hands[partner].Clear();
        // always start from left-of-dealer, skip partner if needed
        int lead = (dealer + 1) % 4;
        if (lead == partner) lead = (lead + 1) % 4;
        currentPlayer = lead;
        trickLeader = lead;
    }

    static void EuchrePass(int p)
    {
        euchrePassCount++;
        if (euchreBidRound == 1 && euchrePassCount >= 4)
        {
            // everyone passed round 1 → flip up-card, round 2
            euchreBidRound = 2;
            euchrePassCount = 0;
            currentPlayer = (dealer + 1) % 4;
            aiThinkTimer = 0.8f;
            cardMessage = "All passed. Name a suit (round 2).";
            cardMessageTimer = 2f;
            return;
        }
        if (euchreBidRound == 2 && euchrePassCount >= 4)
        {
            // stuck the dealer would go here; simplest: redeal
            cardMessage = "All passed again — redeal.";
            cardMessageTimer = 1.5f;
            dealer = (dealer + 1) % 4;
            StartNewHand();
            return;
        }
        currentPlayer = (currentPlayer + 1) % 4;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  BIDDING  — 500
    // ─────────────────────────────────────────────────────────────────────
    // bid value table: tricks 6-10 × suit rank. Simplified Avondale.
    static int FiveHundredBidValue(int tricks, int suit)
    {
        // suit order value: Spades<Clubs<Diamonds<Hearts<NoTrump
        int suitVal = suit switch { 3 => 0, 0 => 1, 1 => 2, 2 => 3, _ => 4 };
        return (tricks - 6) * 100 + 40 + suitVal * 20;
    }

    static void FiveHundredMakeBid(int p, int tricks, int suit)
    {
        int val = FiveHundredBidValue(tricks, suit);
        if (val > fiveHundredBidValue)
        {
            fiveHundredBidValue = val;
            fiveHundredBid = tricks;
            fiveHundredBidSuit = suit;
            fiveHundredHighBidder = p;
        }
        currentPlayer = (currentPlayer + 1) % 4;
        AdvanceFiveHundredBidding();
    }

    static void FiveHundredPass(int p)
    {
        fiveHundredPassCount++;
        currentPlayer = (currentPlayer + 1) % 4;
        AdvanceFiveHundredBidding();
    }

    static void AdvanceFiveHundredBidding()
    {
        if (fiveHundredPassCount >= 4 && fiveHundredHighBidder < 0)
        {
            cardMessage = "All passed — redeal.";
            cardMessageTimer = 1.5f;
            dealer = (dealer + 1) % 4;
            StartNewHand();
            return;
        }
        // bidding ends when 3 players have passed after a bid exists
        if (fiveHundredHighBidder >= 0 && fiveHundredPassCount >= 3)
        {
            // winner takes kitty
            trumpSuit = fiveHundredBidSuit;
            maker = fiveHundredHighBidder;
            makerTeam = TeamOf(maker);
            hands[maker].AddRange(kitty);
            kitty.Clear();
            // discard down to 10
            while (hands[maker].Count > 10) DiscardWorst(maker, trumpSuit);
            SortHands();
            cardPhase = CardPhase.Playing;
            currentPlayer = maker;
            trickLeader = maker;
            cardMessage = $"{PlayerName(maker)} won the bid: {fiveHundredBid} {(trumpSuit<0?"No Trump":SuitNames[trumpSuit])}.";
            cardMessageTimer = 3f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  PLAYING A CARD
    // ─────────────────────────────────────────────────────────────────────
   static void PlayCard(int p, Card c)
    {
        hands[p].Remove(c);
        currentTrick.Add((c, p));

        int next = (p + 1) % 4;
        while (hands[next].Count == 0 && currentTrick.Count < 4 && goingAlone)
            next = (next + 1) % 4;
        currentPlayer = next;

        int playersThisTrick = goingAlone ? 3 : 4;
        if (currentTrick.Count >= playersThisTrick)
        {
            // don't resolve yet — pause so the final card is visible
            trickPending = true;
            trickResolveDelay = 1.0f;   // 1 second to see the last card
        }
    }

    //Request bid and Seat request
    static void RequestBidAction(string netMsg, Action hostApply)
{
    if (multiplayer.IsHost || !multiplayer.Connected)
    {
        hostApply();
        if (multiplayer.Connected) BroadcastCardTableState();
    }
    else
    {
        multiplayer.SendCardAction(netMsg);
    }
}

static int SeatOfSender(int fromId)
{
    for (int s = 0; s < 4; s++) if (cardSeatOwner[s] == fromId) return s;
    return -1;
}

    // ─────────────────────────────────────────────────────────────────────
    //  SCORING
    // ─────────────────────────────────────────────────────────────────────
static void ScoreHand()
    {
        cardPhase = CardPhase.HandOver;

        if (cardGameType == CardGameType.Euchre)
        {
            int makerTricks = tricksWon[makerTeam];
            int defTeam = 1 - makerTeam;
            string result;
            int pts = 0, team = makerTeam;

            if (makerTricks >= 3)
            {
                if (makerTricks == 5)
                {
                    if (goingAlone) { pts = 4; result = "LOANER MARCH! +4"; }   // loner sweep
                    else            { pts = 2; result = "MARCH! +2"; }           // all 5 tricks
                }
                else { pts = 1; result = "Made it. +1"; }                       // called & made
                team = makerTeam;
            }
            else
            {
                pts = 2; team = defTeam; result = "EUCHRED! Defenders +2";       // makers failed
            }
            teamScore[team] += pts;

            // award every human seat based on whether their team won the hand
            for (int st = 0; st < 4; st++)
            {
                bool seatWon = TeamOf(st) == team;
                AwardSeat(st, seatWon ? pts * 6 : 2, seatWon);
            }

            cardMessage = $"{result}  ({TeamLabel(team)})";
            cardMessageTimer = 4f;
        }
        else // 500
        {
            int makerTricks = tricksWon[makerTeam];
            int defTeam = 1 - makerTeam;
            string result;

            if (makerTricks >= fiveHundredBid)
            {
                int gained = fiveHundredBidValue;
                // all 10 tricks but bid under 10 → still cap unless slam; keep simple
                teamScore[makerTeam] += gained;
                result = $"Contract made! +{gained}";
            }
            else
            {
                teamScore[makerTeam] -= fiveHundredBidValue;
                result = $"Contract FAILED! -{fiveHundredBidValue}";
            }
            // defenders score 10 per trick
            teamScore[defTeam] += tricksWon[defTeam] * 10;
            cardMessage = result;
            cardMessageTimer = 4f;

            // per-hand XP to every human seat (contract made → makers win, else defenders)
            int winningSide = (makerTricks >= fiveHundredBid) ? makerTeam : defTeam;
            for (int st = 0; st < 4; st++)
            {
                bool seatWon = TeamOf(st) == winningSide;
                AwardSeat(st, seatWon ? 12 : 8, seatWon);
            }
        }
    }

    // Awards XP + rating to whoever occupies `seat`, on the correct machine.
// Host calls this for every human seat after scoring.
static void AwardSeat(int seat, int xp, bool won)
{
    int owner = cardSeatOwner[seat];
    if (owner == -1) return; // AI seat — no skills

    if (owner == multiplayer.MyId)
    {
        // this machine's own player
        AddPlayingCardsXP(xp);
        RecordGameResult(cardGameType, won);
    }
    else if (multiplayer.IsHost && owner > 0)
    {
        // a remote human — tell their machine to award itself
        multiplayer.SendCardAward(owner, xp, won, (int)cardGameType);
    }
}

    static string TeamLabel(int t)
    {
        // team 0 = seats 0 & 2, team 1 = seats 1 & 3
        if (t == 0) return $"{PlayerName(0)} & {PlayerName(2)}";
        return $"{PlayerName(1)} & {PlayerName(3)}";
    }

    // ─────────────────────────────────────────────────────────────────────
    //  AI
    // ─────────────────────────────────────────────────────────────────────
    static void AiBid(int p)
    {
        float skill = AiSkill();

        if (cardGameType == CardGameType.Euchre)
        {
            // count trump strength if upCard.Suit (round 1) or best suit (round 2)
            if (euchreBidRound == 1)
            {
                int candidate = upCard.Suit;
                int strength = EuchreHandStrength(p, candidate);
                // dealer's team values the up-card more (they pick it up)
                bool myTeamDealer = TeamOf(p) == TeamOf(dealer);
                int threshold = (int)(11 - skill * 4);   // skilled AI orders up more readily
                if (myTeamDealer) threshold -= 1;
                if (strength >= threshold)
                {
                    bool alone = strength >= threshold + 5 && skill > 0.6f;
                    EuchreOrderUp(p, alone);
                    return;
                }
                EuchrePass(p);
            }
            else
            {
                // round 2: pick best non-upCard suit
                int bestSuit = -1, bestStr = -1;
                for (int s = 0; s < 4; s++)
                {
                    if (s == upCard.Suit) continue;
                    int st = EuchreHandStrength(p, s);
                    if (st > bestStr) { bestStr = st; bestSuit = s; }
                }
                int threshold = (int)(12 - skill * 4);
                if (bestStr >= threshold)
                {
                    bool alone = bestStr >= threshold + 5 && skill > 0.65f;
                    EuchreNameSuit(p, bestSuit, alone);
                    return;
                }
                EuchrePass(p);
            }
        }
        else // 500 bidding
        {
            int bestSuit = -1, bestStr = -1;
            for (int s = 0; s < 4; s++)
            {
                int st = EuchreHandStrength(p, s); // reuse heuristic
                if (st > bestStr) { bestStr = st; bestSuit = s; }
            }
            // map strength → tricks bid
            int tricks = 6 + (int)((bestStr / 20f) * 4f * skill);
            tricks = Math.Clamp(tricks, 6, 10);
            int val = FiveHundredBidValue(tricks, bestSuit);
            // only bid if it beats current and hand justifies
            if (bestStr >= (int)(10 - skill * 3) && val > fiveHundredBidValue)
                FiveHundredMakeBid(p, tricks, bestSuit);
            else
                FiveHundredPass(p);
        }
    }

    // crude hand strength for a given trump suit
    static int EuchreHandStrength(int p, int trump)
    {
        int s = 0;
        foreach (var c in hands[p])
        {
            int eff = EffectiveSuit(c, trump);
            if (c.IsJoker) { s += 10; continue; }
            if (eff == trump)
            {
                if (c.Rank == 11 && c.Suit == trump) s += 9;       // right bower
                else if (c.Rank == 11) s += 7;                     // left bower
                else if (c.Rank == 14) s += 5;
                else s += 3;
            }
            else if (c.Rank == 14) s += 2;                         // off-ace
        }
        return s;
    }

    static void AiPlay(int p)
    {
        var legal = LegalMoves(p, trumpSuit);
        if (legal.Count == 0) { currentPlayer = (p + 1) % 4; return; }

        float skill = AiSkill();
        Card choice;

        // with probability (1-skill) play a random legal card (weak play)
        if (cardRng.NextDouble() > skill)
        {
            choice = legal[cardRng.Next(legal.Count)];
        }
        else
        {
            // smart-ish: if leading, lead a strong trump or an off-ace.
            if (currentTrick.Count == 0)
            {
                choice = legal.OrderByDescending(c => CardStrength(c, trumpSuit, EffectiveSuit(c, trumpSuit))).First();
            }
            else
            {
                int led = EffectiveSuit(currentTrick[0].card, trumpSuit);
                int curWinner = TrickWinner(trumpSuit);
                bool partnerWinning = TeamOf(curWinner) == TeamOf(p);

                if (partnerWinning)
                {
                    // throw lowest (don't waste high cards)
                    choice = legal.OrderBy(c => CardStrength(c, trumpSuit, led)).First();
                }
                else
                {
                    // try to win with the lowest winning card
                    int bestOnTable = CardStrength(currentTrick[TrickIndex(curWinner)].card, trumpSuit, led);
                    var winners = legal.Where(c => CardStrength(c, trumpSuit, led) > bestOnTable)
                                       .OrderBy(c => CardStrength(c, trumpSuit, led)).ToList();
                    choice = winners.Count > 0 ? winners.First()
                                               : legal.OrderBy(c => CardStrength(c, trumpSuit, led)).First();
                }
            }
        }
        PlayCard(p, choice);
    }

    static int TrickIndex(int player)
    {
        for (int i = 0; i < currentTrick.Count; i++)
            if (currentTrick[i].player == player) return i;
        return 0;
    }

    static void DiscardWorst(int p, int trump)
    {
        // In DiscardWorst(), change to:
        Card worst = hands[p]
            .Where(c => !c.IsJoker)   // never discard the Joker
            .OrderBy(c => CardStrength(c, trump, EffectiveSuit(c, trump)))
            .FirstOrDefault();
        // if all remaining are Joker (edge case), fall back
        if (worst.Suit == 0 && worst.Rank == 0 && hands[p].All(c => c.IsJoker)) return;
        hands[p].Remove(worst);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DRAW  (called from your Draw switch)
    // ─────────────────────────────────────────────────────────────────────
    static void DrawCardGame()
    {
        if (hubActive)  { DrawCardsHub();      return; }
        if (seqActive) { DrawSequenceGame(); return; }

        Raylib.ClearBackground(new Color((byte)15,(byte)60,(byte)35,(byte)255)); // felt green

        // table
        Raylib.DrawCircle(ScreenWidth/2, ScreenHeight/2, 280, new Color((byte)20,(byte)80,(byte)45,(byte)255));
        Raylib.DrawCircleLines(ScreenWidth/2, ScreenHeight/2, 280, new Color((byte)120,(byte)90,(byte)40,(byte)255));

        // title + scores
        string title = cardGameType == CardGameType.Euchre ? "EUCHRE" : "500";
        Raylib.DrawText(title, 20, 16, 30, Color.Gold);
        Raylib.DrawText($"{TeamLabel(0)}: {teamScore[0]}", 20, 56, 22, new Color((byte)120,(byte)200,(byte)255,(byte)255));
        Raylib.DrawText($"{TeamLabel(1)}: {teamScore[1]}", 20, 82, 22, new Color((byte)255,(byte)140,(byte)140,(byte)255));
        Raylib.DrawText($"Playing Cards Lv {player.PlayingCardsLevel}", 20, 112, 18, Color.LightGray);
        if (trumpSuit >= 0)
            Raylib.DrawText($"Trump: {SuitNames[trumpSuit]}", 20, 138, 20, SuitColor(trumpSuit));
        Raylib.DrawText($"Tricks  You: {tricksWon[0]}  Them: {tricksWon[1]}", 20, 164, 18, Color.White);
        Raylib.DrawText("Q = Leave", ScreenWidth - 130, 16, 20, Color.LightGray);

        // dealer chip indicator near each seat
        DrawSeatLabels();

        // opponents' hands (face down)
        DrawOpponentHands();

        // the trick in the middle
        DrawCurrentTrick();

        // your hand (face up, clickable)
        DrawHumanHand();

        // bidding UI
        if (cardPhase == CardPhase.Bidding && currentPlayer == 0)
            DrawBiddingUI();

        // messages
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
    // ── DEALER HUD (constant, bottom-left) ──
        Raylib.DrawRectangle(10, ScreenHeight - 70, 230, 56, new Color((byte)0,(byte)0,(byte)0,(byte)200));
        Raylib.DrawRectangleLines(10, ScreenHeight - 70, 230, 56, Color.Gold);
        Raylib.DrawText("DEALER THIS HAND", 20, ScreenHeight - 64, 16, Color.LightGray);
        Raylib.DrawText(PlayerName(dealer), 20, ScreenHeight - 42, 24, Color.Gold);

        // ── CALLER HUD (just above dealer box) ──
        if (maker >= 0 && cardPhase == CardPhase.Playing)
        {
            string calledWhat = cardGameType == CardGameType.Euchre
                ? SuitNames[trumpSuit]
                : $"{fiveHundredBid} {(trumpSuit < 0 ? "No Trump" : SuitNames[trumpSuit])}";
            string callerLine2 = $"{PlayerName(maker)} called {calledWhat}";
            int cw = Raylib.MeasureText(callerLine2, 18);
            int boxW = Math.Max(cw + 20, 230);
            Raylib.DrawRectangle(10, ScreenHeight - 136, boxW, 60, new Color((byte)0,(byte)0,(byte)0,(byte)200));
            Raylib.DrawRectangleLines(10, ScreenHeight - 136, boxW, 60, SuitColor(trumpSuit < 0 ? 0 : trumpSuit));
            Raylib.DrawText("CALLED THIS HAND", 20, ScreenHeight - 130, 16, Color.LightGray);
            Raylib.DrawText(callerLine2, 20, ScreenHeight - 108, 18, SuitColor(trumpSuit < 0 ? 0 : trumpSuit));
        }
    }

    static void DrawCardPlayer(int p, int cx, int cy)
    {
        // colours per character
        (Color hair, Color shirt, bool female) look = p switch
        {
            0 => (player.SkinColor, player.ShirtColor, false),                     // you (use your colors)
            1 => (new Color((byte)90,(byte)50,(byte)20,(byte)255),  new Color((byte)200,(byte)80,(byte)140,(byte)255), true),   // Joy
            2 => (new Color((byte)20,(byte)20,(byte)20,(byte)255),  new Color((byte)140,(byte)60,(byte)180,(byte)255), true),   // Rala
            3 => (new Color((byte)30,(byte)20,(byte)10,(byte)255),  new Color((byte)40,(byte)120,(byte)160,(byte)255), false),  // Tipene
            _ => (Color.Gray, Color.Gray, false)
        };
        Color skin = p == 0 ? player.SkinColor : new Color((byte)225,(byte)185,(byte)150,(byte)255);

        // body / shoulders
        Raylib.DrawRectangle(cx - 18, cy + 8, 36, 26, look.shirt);
        // neck
        Raylib.DrawRectangle(cx - 5, cy + 2, 10, 8, skin);
        // head
        Raylib.DrawCircle(cx, cy - 8, 14, skin);

        // hair — women get longer hair, man gets short
        if (look.female)
        {
            // long hair framing the face
            Raylib.DrawCircle(cx, cy - 10, 15, look.hair);
            Raylib.DrawRectangle(cx - 15, cy - 12, 6, 28, look.hair);   // left length
            Raylib.DrawRectangle(cx + 9,  cy - 12, 6, 28, look.hair);   // right length
            Raylib.DrawCircle(cx, cy - 6, 12, skin);                    // face over hair
        }
        else
        {
            // short hair cap
            Raylib.DrawCircle(cx, cy - 12, 14, look.hair);
            Raylib.DrawRectangle(cx - 14, cy - 14, 28, 8, look.hair);
            Raylib.DrawCircle(cx, cy - 6, 12, skin);                    // face
        }

        // simple face
        Raylib.DrawCircle(cx - 5, cy - 8, 2, Color.Black);
        Raylib.DrawCircle(cx + 5, cy - 8, 2, Color.Black);
        Raylib.DrawRectangle(cx - 3, cy - 1, 6, 2, new Color((byte)150,(byte)80,(byte)80,(byte)255));

        // Tipene gets a small beard
        if (p == 3)
            Raylib.DrawRectangle(cx - 8, cy - 2, 16, 6, new Color((byte)30,(byte)20,(byte)10,(byte)255));
    }

    static void DrawSeatLabels()
    {
        // seat anchor points
        (int x, int y)[] seats =
        {
            (ScreenWidth/2,        ScreenHeight - 150),  // you (south)
            (180,                  ScreenHeight/2),      // west
            (ScreenWidth/2,        150),                 // north
            (ScreenWidth - 180,    ScreenHeight/2),      // east
        };
        for (int p = 0; p < 4; p++)
        {
            Color c = TeamOf(p) == 0 ? new Color((byte)120,(byte)200,(byte)255,(byte)255)
                                     : new Color((byte)255,(byte)140,(byte)140,(byte)255);
             string nm = PlayerName(p);
            int w = Raylib.MeasureText(nm, 18);
            int nx = seats[p].x - w/2;
            int ny = seats[p].y + (p==0?70:p==2?-90:-50);
            bool isTurn = p == currentPlayer && cardPhase != CardPhase.HandOver && cardPhase != CardPhase.GameOver;

            // highlight box around the current player's name
            if (isTurn)
            {
                Raylib.DrawRectangle(nx - 8, ny - 4, w + 16, 26, new Color((byte)60,(byte)50,(byte)10,(byte)200));
                Raylib.DrawRectangleLinesEx(new Rectangle(nx - 8, ny - 4, w + 16, 26), 3, Color.Gold);
            }
            Raylib.DrawText(nm, nx, ny, 18, isTurn ? Color.Gold : c);
            if (p == dealer)
                Raylib.DrawText("[DEALER]", seats[p].x - 38, seats[p].y + (p==0?92:p==2?-68:-28), 14, Color.Gold);
            
            int portraitY = p == 0 ? seats[p].y + 110
                          : p == 2 ? seats[p].y - 130
                          : seats[p].y - 95;
            DrawCardPlayer(p, seats[p].x, portraitY);
        }
    }

    static void DrawOpponentHands()
{
    int mySeat = MyViewSeat();

    // West/North/East screen slots, but populated by whichever ACTUAL seat
    // rotates into that slot relative to the local viewer
    for (int seat = 0; seat < 4; seat++)
    {
        if (seat == mySeat) continue; // that's "my" hand, drawn separately
        int slot = SeatToScreenSlot(seat); // 1=left, 2=top, 3=right

        if (hands[seat] == null) continue;

        if (slot == 1) // west
            for (int i = 0; i < hands[seat].Count; i++)
                DrawCardBack(120, ScreenHeight/2 - 80 + i * 26, true);
        else if (slot == 2) // north
            for (int i = 0; i < hands[seat].Count; i++)
                DrawCardBack(ScreenWidth/2 - 90 + i * 34, 90, false);
        else if (slot == 3) // east
            for (int i = 0; i < hands[seat].Count; i++)
                DrawCardBack(ScreenWidth - 160, ScreenHeight/2 - 80 + i * 26, true);
    }
}

    static void DrawCardBack(int x, int y, bool small)
    {
        int w = small ? 36 : 46, h = small ? 50 : 64;
        Raylib.DrawRectangle(x, y, w, h, new Color((byte)40,(byte)40,(byte)90,(byte)255));
        Raylib.DrawRectangleLines(x, y, w, h, new Color((byte)120,(byte)120,(byte)200,(byte)255));
        Raylib.DrawRectangle(x + 6, y + 6, w - 12, h - 12, new Color((byte)60,(byte)60,(byte)130,(byte)255));
    }

    static void DrawCurrentTrick()
{
    (int x, int y)[] spot =
    {
        (ScreenWidth/2 - 25,  ScreenHeight/2 + 60),  // bottom (you)
        (ScreenWidth/2 - 110, ScreenHeight/2 - 30),  // left
        (ScreenWidth/2 - 25,  ScreenHeight/2 - 120), // top
        (ScreenWidth/2 + 60,  ScreenHeight/2 - 30),  // right
    };
    foreach (var (card, p) in currentTrick)
    {
        int slot = SeatToScreenSlot(p);
        DrawCardFace(card, spot[slot].x, spot[slot].y, false);
    }
}

  static void DrawHumanHand()
{
    int mySeat = MyViewSeat();
    
    if (hands[mySeat] == null) return;

    humanCardRects.Clear();
    var hand = new List<Card>(hands[mySeat]);
    int n = hand.Count;
    if (n == 0) return;
    int cw = 60, gap = 66;
    int totalW = n * gap;
    int startX = ScreenWidth/2 - totalW/2;
    int y = ScreenHeight - 110;

    Vector2 mouse = Raylib.GetMousePosition();
    var legal = (cardPhase == CardPhase.Playing && currentPlayer == mySeat)
                ? LegalMoves(mySeat, trumpSuit) : new List<Card>();

    Card? clicked = null;

    for (int i = 0; i < n; i++)
    {
        Card card = hand[i];
        int x = startX + i * gap;
        var rect = new Rectangle(x, y, cw, 84);
        humanCardRects.Add(rect);
        bool hover = Raylib.CheckCollisionPointRec(mouse, rect);
        bool playable = legal.Any(c => c.Suit == card.Suit && c.Rank == card.Rank && c.IsJoker == card.IsJoker);
        int lift = (hover && playable) ? 18 : 0;
        DrawCardFace(card, x, y - lift, true, playable && currentPlayer == mySeat && cardPhase == CardPhase.Playing);

        if (cardPhase == CardPhase.Playing && currentPlayer == mySeat
            && !trickPending
            && hover && playable && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            clicked = card;
        }
    }

    if (clicked.HasValue)
    {
        if (multiplayer.IsHost || !multiplayer.Connected)
        {
            PlayCard(mySeat, clicked.Value);
            if (multiplayer.Connected) BroadcastCardTableState();
        }
        else
        {
            // joiner: request the play, let the host apply it authoritatively
            multiplayer.SendCardAction($"PLAY|{SerializeCard(clicked.Value)}");
        }
    }
}

    static void DrawCardFace(Card c, int x, int y, bool large, bool highlight = false)
    {
        int w = large ? 60 : 46, h = large ? 84 : 64;
        Raylib.DrawRectangle(x, y, w, h, Color.White);
        Raylib.DrawRectangleLines(x, y, w, h, highlight ? Color.Gold : new Color((byte)80,(byte)80,(byte)80,(byte)255));
        if (highlight) Raylib.DrawRectangleLines(x-1, y-1, w+2, h+2, Color.Gold);

        if (c.IsJoker)
        {
            Raylib.DrawText("JOKER", x + 6, y + h/2 - 8, 14, new Color((byte)150,(byte)20,(byte)150,(byte)255));
            return;
        }
        Color col = SuitColor(c.Suit);
        Raylib.DrawText(RankName(c.Rank), x + 3, y + 3, large ? 22 : 14, col);
        DrawSuitSymbol(c.Suit, x + w/2, y + (large ? 48 : 28), large ? 12 : 10, col);
        // bottom mirrored
        Raylib.DrawText(RankName(c.Rank), x + w - 18, y + h - 24, large ? 18 : 14, col);
    }

    static void DrawBiddingUI()
    {
        int bx = ScreenWidth/2 - 260, by = ScreenHeight - 230;
        Vector2 mouse = Raylib.GetMousePosition();
        int seat = MyViewSeat();

        if (cardGameType == CardGameType.Euchre)
        {
            if (euchreBidRound == 1)
            {
                // show up-card + Order Up / Pass / Alone
                Raylib.DrawText("Up-card:", bx, by - 30, 18, Color.White);
                DrawCardFace(upCard, bx + 80, by - 50, false);
                if (CardButton("Order Up", bx + 150, by, 130, mouse))
                    RequestBidAction($"ORDERUP|{seat}|0", () => EuchreOrderUp(seat, false));
                if (CardButton("Alone",    bx + 290, by, 90,  mouse))
                    RequestBidAction($"ORDERUP|{seat}|1", () => EuchreOrderUp(seat, true));
                if (CardButton("Pass",     bx + 390, by, 100, mouse))
                    RequestBidAction($"PASS|{seat}", () => EuchrePass(seat));
            }
            else
            {
                Raylib.DrawText("Name a suit:", bx, by - 30, 18, Color.White);
                int drawn = 0;
                for (int s = 0; s < 4; s++)
                {
                    if (s == upCard.Suit) continue;
                    int suit = s; // capture for the closure
                    if (CardButton(SuitNames[s], bx + drawn * 110, by, 100, mouse))
                        RequestBidAction($"NAMESUIT|{seat}|{suit}|0", () => EuchreNameSuit(seat, suit, false));
                    drawn++;
                }
                if (CardButton("Pass", bx + drawn * 110, by, 90, mouse))
                    RequestBidAction($"PASS|{seat}", () => EuchrePass(seat));
            }
        }
        else // 500 bidding
        {
            Raylib.DrawText($"Current high bid: " +
                (fiveHundredHighBidder < 0 ? "none" :
                 $"{fiveHundredBid} {(fiveHundredBidSuit<0?"NT":SuitNames[fiveHundredBidSuit])} ({PlayerName(fiveHundredHighBidder)})"),
                bx, by - 56, 18, Color.White);
            Raylib.DrawText("Bid tricks + suit, or pass:", bx, by - 32, 16, Color.LightGray);

            // simple grid: pick a suit then tricks; here we expose a few common bids
            int col = 0;
            foreach (int tricks in new[] {6,7,8})
            {
                foreach (int s in new[] {3,0,1,2})
                {
                    int val = FiveHundredBidValue(tricks, s);
                    if (val <= fiveHundredBidValue) continue;
                    int bidTricks = tricks; // capture for the closure
                    int bidSuit = s;
                    if (CardButton($"{tricks}{SuitGlyphs[s]}", bx + (col%6)*82, by + (col/6)*40, 76, mouse))
                        RequestBidAction($"BID500|{seat}|{bidTricks}|{bidSuit}", () => FiveHundredMakeBid(seat, bidTricks, bidSuit));
                    col++;
                }
            }
            if (CardButton("PASS", bx + (col%6)*82, by + (col/6)*40, 76, mouse))
                RequestBidAction($"PASS|{seat}", () => FiveHundredPass(seat));
        }
    }

    static bool CardButton(string label, int x, int y, int w, Vector2 mouse)
    {
        var r = new Rectangle(x, y, w, 34);
        bool hover = Raylib.CheckCollisionPointRec(mouse, r);
        Raylib.DrawRectangleRec(r, new Color((byte)30,(byte)30,(byte)45,(byte)255));
        Raylib.DrawRectangleLinesEx(r, 2, hover ? Color.Gold : Color.White);
        int tw = Raylib.MeasureText(label, 16);
        Raylib.DrawText(label, x + w/2 - tw/2, y + 9, 16, hover ? Color.Gold : Color.White);
        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }
}
}
