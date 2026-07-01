// ============================================================================
//  SEQUENCE MODULE  —  Board card game, 2 teams of 2
//  Team 0: You + Jake (North)   — Blue chips
//  Team 1: Shack (West) + C Ride (East) — Red chips
//
//  HOOK-IN CHECKLIST (3 edits in Program.cs):
//    1. enum SceneState → already has CardGame, reused here.
//    2. MY HOUSE interior update block → add table interaction (snippet below).
//    3. MY HOUSE interior draw block   → add table prompt draw (snippet below).
//
//  ── SNIPPET A: inside the MY HOUSE building update block ──
//
//    Vector2 seqTablePos = new Vector2(260, 480);
//    bool nearSeqTable = Vector2.Distance(player.Center, seqTablePos) < 100;
//    if (nearSeqTable && !wardrobeOpen && !chestOpen && !cookingMenuOpen)
//    {
//        if (Raylib.IsKeyPressed(KeyboardKey.E))
//            StartSequenceGame();
//    }
//
//  ── SNIPPET B: inside the MY HOUSE building draw block ──
//
//    Vector2 seqTablePos = new Vector2(260, 480);
//    if (Vector2.Distance(player.Center, seqTablePos) < 100 &&
//        !wardrobeOpen && !chestOpen && !cookingMenuOpen)
//    {
//        Raylib.DrawRectangle(0, 620, 1280, 100, new Color((byte)0,(byte)0,(byte)0,(byte)180));
//        Raylib.DrawText("SEQUENCE TABLE", 20, 630, 28, new Color((byte)80,(byte)180,(byte)255,(byte)255));
//        Raylib.DrawText("E = Play Sequence (2v2)", 20, 668, 22, Color.White);
//    }
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
    //  SEQUENCE: BOARD LAYOUT
    //  The 10×10 grid uses a fixed card layout matching the real Sequence board.
    //  Corners are FREE spaces (suit=-1, rank=-1).
    //  Each card from a standard deck appears exactly twice on the board.
    //  Jacks never appear on the board.
    // ─────────────────────────────────────────────────────────────────────

    // seqBoard[row][col] = (suit, rank)  — (-1,-1) = FREE corner
   static readonly (int suit, int rank)[,] seqBoard = new (int,int)[10,10]
    {
        // row 0
        { (-1,-1),(3,2),(3,3),(3,4),(3,5),(3,6),(3,7),(3,8),(3,9),(-1,-1) },
        // row 1
        { (0, 6),(0,5),(0,4),(0,3),(0,2),(2,1),(2,13),(2,12),(2,10),(3,10) },
        // row 2
        { (0,7),(3,1),(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),(2,9),(3,12) },
        // row 3
        { (0,8),(3,13),(0,6),(0,5),(0,4),(0,3),(0,2),(1,8),(2,8),(3,13) },
        // row 4
        { (0,9),(3,12),(0,7),(2,6),(2,5),(2,4),(2,1),(1,9),(2,7),(3,1) },
        // row 5
        { (0,10),(3,10),(0,8),(2,7),(2,2),(2,3),(2,13),(1,10),(2,6),(1,2) },
        // row 6
        { (0,12),(3,9),(0,9),(2,8),(2,9),(2,10),(2,12),(1,12),(2,5),(1,3) },
        // row 7
        { (0,13),(3,8),(0,10),(0,12),(0,13),(0,1),(1,1),(1,13),(2,4),(1,4) },
        // row 8
        { (0,1),(3,7),(3,6),(3,5),(3,4),(3,3),(3,2),(2,2),(2,3),(1,5) },
        // row 9
        { (-1,-1),(1,1),(1,13),(1,12),(1,10),(1,9),(1,8),(1,7),(1,6),(-1,-1) },
    };

    // chip owner per cell: -1=empty, 0=team0 (blue), 1=team1 (red), 2=free corner
    static int[,] seqChips = new int[10,10];

    // ─────────────────────────────────────────────────────────────────────
    //  SEQUENCE: GAME STATE
    // ─────────────────────────────────────────────────────────────────────
    static bool seqActive = false;
    static int[] seqSequences = new int[2];     // completed sequences per team
    static int seqCurrentPlayer = 0;            // 0=You,1=Shack,2=Jake,3=CRide
    static int lastAnnouncedTurn = -1;
    static int lastAnnouncedSeqTurn = -1;
    static List<Card>[] seqHands = new List<Card>[4];
    static List<Card> seqDeck = new List<Card>();
    static int seqDeckIdx = 0;
    static string seqMessage = "";
    static float seqMessageTimer = 0f;
    static float seqAiTimer = 0f;
    static Vector2 seqReturnPos;
    static int seqSelectedCard = -1;            // index in human hand
    static bool seqGameOver = false;
    static int seqWinTeam = -1;

    // for highlighting valid board cells when a card is selected
    static List<(int row, int col)> seqValidCells = new();

    // dead card discard tracking (can't place — draw replacement)
    static bool seqDeadCardPending = false;

    // ─────────────────────────────────────────────────────────────────────
    //  SEQUENCE: PLAYER NAMES & LOOKS
    //  0=You, 1=Shack(W/female), 2=Jake(N/male), 3=CRide(E/female)
    // ─────────────────────────────────────────────────────────────────────
    static string SeqPlayerName(int p) => p switch
    {
        0 => "You",
        1 => "Shack",
        2 => "Jake",
        3 => "C Ride",
        _ => "?"
    };

    // Multiplayer-aware display name for a Sequence seat.
    static string SeqDisplayName(int p)
    {
        if (p < 0 || p > 3) return "?";
        int owner = cardSeatOwner[p];
        if (owner == -1) return SeqPlayerName(p);              // AI → Shack/Jake/C Ride/You
        if (owner == multiplayer.MyId)
            return multiplayer.Connected ? (playerName ?? "Me") : "Me";
        if (owner > 0)
        {
            lock (multiplayer.RemotePlayers)
            {
                var rp = multiplayer.RemotePlayers.Find(r => r.Id == owner);
                if (rp != null && !string.IsNullOrEmpty(rp.Name)) return rp.Name;
            }
            return $"Player {owner}";
        }
        return "Host";
    }

    static int SeqTeamOf(int p) => (p == 0 || p == 2) ? 0 : 1;   // 0=You+Jake, 1=Shack+CRide

    static string SeqTeamLabel(int t) => t == 0 ? "You & Jake" : "Shack & C Ride";

    static Color SeqTeamColor(int t) =>
        t == 0 ? new Color((byte)80,(byte)160,(byte)255,(byte)255)
               : new Color((byte)255,(byte)80,(byte)80,(byte)255);

    // ─────────────────────────────────────────────────────────────────────
    //  JACK HELPERS
    // ─────────────────────────────────────────────────────────────────────
    // One-eyed jacks (Hearts J + Spades J) = remove opponent chip
    static bool IsOneEyedJack(Card c) =>
        c.Rank == 11 && (c.Suit == 2 || c.Suit == 3);   // Hearts or Spades

    // Two-eyed jacks (Clubs J + Diamonds J) = wild place anywhere
    static bool IsTwoEyedJack(Card c) =>
        c.Rank == 11 && (c.Suit == 0 || c.Suit == 1);   // Clubs or Diamonds

    // ─────────────────────────────────────────────────────────────────────
    //  ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────
    static void StartSequenceGame()
    {

        seqActive = true;
        seqGameOver = false;
        seqWinTeam = -1;
        seqSequences[0] = seqSequences[1] = 0;
        seqReturnPos = player.Position;
        seqCurrentPlayer = 0;
        lastAnnouncedSeqTurn = -1;
        seqMessage = "Sequence! First to 2 sequences wins.";
        seqMessageTimer = 3f;
        seqSelectedCard = -1;
        seqDeadCardPending = false;

        // init chips — corners are free for everyone (team 2 = neutral free)
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                seqChips[r,c] = seqBoard[r,c].suit == -1 ? 2 : -1;

        // host deals; client creates empty hands and waits for the broadcast
        if (!multiplayer.Connected || multiplayer.IsHost)
        {
            SeqBuildAndDeal();
        }
        else
        {
            for (int p = 0; p < 4; p++)
                seqHands[p] = new List<Card>();
        }

        currentScene = SceneState.CardGame;
    }

    static void SeqBuildAndDeal()
    {
        // Two standard 52-card decks shuffled together
        seqDeck.Clear();
        for (int d = 0; d < 2; d++)
            for (int s = 0; s < 4; s++)
                for (int r = 1; r <= 13; r++)
                    seqDeck.Add(new Card(s, r));

        // shuffle
        for (int i = seqDeck.Count - 1; i > 0; i--)
        {
            int j = cardRng.Next(i + 1);
            (seqDeck[i], seqDeck[j]) = (seqDeck[j], seqDeck[i]);
        }
        seqDeckIdx = 0;

        // deal 7 cards each for 4 players
        for (int p = 0; p < 4; p++)
        {
            seqHands[p] = new List<Card>();
            for (int c = 0; c < 7; c++)
                seqHands[p].Add(seqDeck[seqDeckIdx++]);
        }
    }

    static Card SeqDrawCard() =>
        seqDeckIdx < seqDeck.Count ? seqDeck[seqDeckIdx++] : new Card(0, 1); // fallback if deck empty

    // ─────────────────────────────────────────────────────────────────────
    //  UPDATE
    // ─────────────────────────────────────────────────────────────────────
    static void UpdateSequenceGame(float dt)
    {
        if (!seqActive) return;
        if (seqMessageTimer > 0) seqMessageTimer -= dt;

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            seqActive = false;
            player.Position = seqReturnPos;
            if (hubActive)
            {
                hubScreen = HubScreen.GameSelect;
                currentScene = SceneState.CardGame;
            }
            else
            {
                hubActive = false;
                currentScene = SceneState.Building;
            }
            return;
        }

        if (seqGameOver) return;

        // whose turn: AI runs only on the host, only for AI-owned seats
        bool amHost = !multiplayer.Connected || multiplayer.IsHost;
        if (CardSeatIsAI(seqCurrentPlayer))
        {
            if (amHost)
            {
                seqAiTimer -= dt;
                if (seqAiTimer <= 0f)
                {
                    seqAiTimer = 0.9f;
                    SeqAiTurn(seqCurrentPlayer);
                    if (multiplayer.Connected) SeqBroadcastState();
                }
            }
            return;
        }

        // remote human's turn → wait for their network action
        if (!IsMyCardSeat(seqCurrentPlayer))
            return;

        // it's MY turn
        int mySeat = MyViewSeat();

        // don't process input until our hand exists (client waiting for broadcast)
        if (seqHands[mySeat] == null) return;

        // card selection with number keys 1-7 (from my own hand)
        for (int i = 0; i < seqHands[mySeat].Count; i++)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.One + i))
            {
                seqSelectedCard = (seqSelectedCard == i) ? -1 : i;
                SeqUpdateValidCells();
                break;
            }
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            seqSelectedCard = -1;
            seqValidCells.Clear();
        }
    }

    // Called from DrawSequenceGame when human clicks a board cell
    static void SeqHumanClickCell(int row, int col)
    {
        int mySeat = MyViewSeat();
        if (seqSelectedCard < 0 || seqCurrentPlayer != mySeat) return;
        if (seqSelectedCard >= seqHands[mySeat].Count) return;
        Card c = seqHands[mySeat][seqSelectedCard];
        int oppTeam = 1 - SeqTeamOf(mySeat);

        // validate locally so we never send an illegal move
        bool valid;
        if (IsTwoEyedJack(c))
        {
            valid = seqChips[row, col] == -1;                       // wild: any empty cell
        }
        else if (IsOneEyedJack(c))
        {
            valid = seqChips[row, col] == oppTeam                    // anti-wild: remove an opponent chip
                    && !SeqIsChipInSequence(row, col, oppTeam);     // not part of a locked sequence
        }
        else
        {
            var (bs, br) = seqBoard[row, col];                      // normal: match card + empty cell
            valid = (br == c.Rank && bs == c.Suit && seqChips[row, col] == -1);
        }
        if (!valid) return;

        if (!multiplayer.Connected || multiplayer.IsHost)
        {
            SeqApplyMove(mySeat, seqSelectedCard, row, col);        // host/local applies directly
            if (multiplayer.Connected) SeqBroadcastState();
        }
        else
        {
            multiplayer.SendCardAction($"SEQPLAY|{seqSelectedCard}|{row}|{col}");  // client requests it
        }
    }

    static void SeqBroadcastState()
    {
        if (!multiplayer.IsHost || !multiplayer.Connected) return;

        // flatten the 10x10 chip grid into 100 comma-separated ints
        var chips = new System.Text.StringBuilder();
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
            {
                chips.Append(seqChips[r, c]);
                if (!(r == 9 && c == 9)) chips.Append(',');
            }

        string handCounts = $"{seqHands[0]?.Count ?? 0},{seqHands[1]?.Count ?? 0}," +
                            $"{seqHands[2]?.Count ?? 0},{seqHands[3]?.Count ?? 0}";

        string payload = "SEQ|" +
            $"{seqCurrentPlayer}|{seqSequences[0]}|{seqSequences[1]}|" +
            $"{(seqGameOver ? 1 : 0)}|{seqWinTeam}|" +
            $"{cardSeatOwner[0]}|{cardSeatOwner[1]}|{cardSeatOwner[2]}|{cardSeatOwner[3]}|" +
            $"{handCounts}|{chips}|" +
            $"{seqMessage.Replace('|', '/')}|{seqMessageTimer.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        multiplayer.BroadcastCardTableState(payload);

        // send each remote their own private hand
        for (int seat = 0; seat < 4; seat++)
            if (cardSeatOwner[seat] > 0)
                multiplayer.SendOwnHandTo(cardSeatOwner[seat], seat, SerializeCardList(seqHands[seat]));
    }
    static void ApplySeqState(string state)
    {
        if (multiplayer.IsHost) return;   // host owns state

        string[] f = state.Split('|');
        // f[0] = "SEQ"
        int i = 1;
        seqCurrentPlayer = int.Parse(f[i++]);
        seqSequences[0]  = int.Parse(f[i++]);
        seqSequences[1]  = int.Parse(f[i++]);
        seqGameOver      = f[i++] == "1";
        seqWinTeam       = int.Parse(f[i++]);
        cardSeatOwner[0] = int.Parse(f[i++]);
        cardSeatOwner[1] = int.Parse(f[i++]);
        cardSeatOwner[2] = int.Parse(f[i++]);
        cardSeatOwner[3] = int.Parse(f[i++]);

        string[] counts = f[i++].Split(',');
        string[] chipVals = f[i++].Split(',');
        int idx = 0;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                seqChips[r, c] = int.Parse(chipVals[idx++]);

        if (i < f.Length) seqMessage = f[i++].Replace('/', '|');
        if (i < f.Length) float.TryParse(f[i++], System.Globalization.NumberStyles.Float,
                              System.Globalization.CultureInfo.InvariantCulture, out seqMessageTimer);

        // rebuild opponent hand counts as placeholders; my own hand comes via OwnHandReceived
        int mySeat = MyViewSeat();
        for (int seat = 0; seat < 4; seat++)
        {
            if (seat == mySeat) { if (seqHands[seat] == null) seqHands[seat] = new List<Card>(); continue; }
            seqHands[seat] = new List<Card>();
            int cnt = int.Parse(counts[seat]);
            for (int k = 0; k < cnt; k++) seqHands[seat].Add(new Card(-2, -2));
        }

        seqActive = true;   // make sure the client is showing the board
    }

    static void SeqFinishTurn(int p, int cardIdx)
    {
        seqHands[p].RemoveAt(cardIdx);
        if (seqDeckIdx < seqDeck.Count)
            seqHands[p].Add(SeqDrawCard());

        seqSelectedCard = -1;
        seqValidCells.Clear();

        // check sequences
        int newSeqs = SeqCountSequences(p);
        int team = SeqTeamOf(p);
        if (newSeqs > seqSequences[team])
        {
            seqSequences[team] = newSeqs;
            seqMessage = $"{SeqTeamLabel(team)} completed a sequence! ({seqSequences[team]}/2)";
            seqMessageTimer = 2.5f;
            SeqAwardTeam(team, 20, 5, false);
        }

        if (seqSequences[team] >= 2)
        {
            seqGameOver = true;
            seqWinTeam = team;
            seqMessage = $"{SeqTeamLabel(team)} WIN!";
            seqMessageTimer = 10f;
            SeqAwardTeam(team, 50, 0, true);
            return;
        }

        seqCurrentPlayer = (p + 1) % 4;
        seqAiTimer = 0.9f;
    }

    // Awards each human seat based on whether their team scored/won.
    // Local player gets XP directly; remote players get a network award; AI gets nothing.
    static void SeqAwardTeam(int scoringTeam, int winXp, int loseXp, bool recordResult)
    {
        for (int st = 0; st < 4; st++)
        {
            int owner = cardSeatOwner[st];
            if (owner == -1) continue; // AI seat — no skills

            bool onScoringTeam = SeqTeamOf(st) == scoringTeam;
            int xp = onScoringTeam ? winXp : loseXp;

            if (owner == multiplayer.MyId)
            {
                AddPlayingCardsXP(xp);
                if (recordResult) RecordGameResult(CardGameType.Sequence, onScoringTeam);
            }
            else if (multiplayer.IsHost && owner > 0)
            {
                multiplayer.SendCardAward(owner, xp, onScoringTeam, (int)CardGameType.Sequence);
            }
        }
    }

    // Applies a validated move for seat p. Runs on the host (authoritative).
    static void SeqApplyMove(int p, int cardIdx, int row, int col)
    {
        if (cardIdx < 0 || cardIdx >= seqHands[p].Count) return;
        Card c = seqHands[p][cardIdx];
        int myTeam = SeqTeamOf(p);

        if (IsTwoEyedJack(c))
        {
            if (seqChips[row, col] != -1) return;
            seqChips[row, col] = myTeam;
        }
        else if (IsOneEyedJack(c))
        {
            int oppTeam = 1 - myTeam;
            if (seqChips[row, col] != oppTeam || SeqIsChipInSequence(row, col, oppTeam)) return;
            seqChips[row, col] = -1;
            seqMessage = $"{SeqPlayerName(p)} removed a chip!";
            seqMessageTimer = 1.5f;
        }
        else
        {
            var (bs, br) = seqBoard[row, col];
            if (!(br == c.Rank && bs == c.Suit && seqChips[row, col] == -1)) return;
            seqChips[row, col] = myTeam;
        }

        SeqFinishTurn(p, cardIdx);
    }

    static int SeqOpponentTeamMarker(int seat) => 1 - SeqTeamOf(seat);

    // ─────────────────────────────────────────────────────────────────────
    //  AI TURN
    // ─────────────────────────────────────────────────────────────────────
    static void SeqAiTurn(int p)
    {
        int myTeam = SeqTeamOf(p);
        int oppTeam = 1 - myTeam;
        var hand = seqHands[p];
        if (hand.Count == 0) { seqCurrentPlayer = (p + 1) % 4; return; }

        float skill = AiSkill();

        // --- Priority 1: complete a sequence if one move away ---
        var winning = SeqFindCompletingMove(p, myTeam, hand);
        if (winning.HasValue)
        {
            SeqAiPlayMove(p, winning.Value.cardIdx, winning.Value.row, winning.Value.col);
            return;
        }

        // --- Priority 2: block opponent one move from completing ---
        if (skill > 0.4f)
        {
            var blocking = SeqFindBlockingMove(p, oppTeam, hand);
            if (blocking.HasValue)
            {
                SeqAiPlayMove(p, blocking.Value.cardIdx, blocking.Value.row, blocking.Value.col);
                return;
            }
        }

        // --- Priority 3: play best available card strategically ---
        // Score each playable move by how many friendly chips are adjacent
        var allMoves = SeqGetAllMoves(p, hand);
        if (allMoves.Count == 0)
        {
            // dead card — discard and draw
            seqHands[p].RemoveAt(0);
            if (seqDeckIdx < seqDeck.Count) seqHands[p].Add(SeqDrawCard());
            seqMessage = $"{SeqPlayerName(p)} discarded a dead card.";
            seqMessageTimer = 1.2f;
            seqCurrentPlayer = (p + 1) % 4;
            return;
        }

        // pick move with highest adjacency score
        var best = allMoves
            .OrderByDescending(m => SeqAdjacencyScore(m.row, m.col, myTeam))
            .First();
        SeqAiPlayMove(p, best.cardIdx, best.row, best.col);
    }

    static void SeqAiPlayMove(int p, int cardIdx, int row, int col)
    {
        Card c = seqHands[p][cardIdx];
        int myTeam = SeqTeamOf(p);

        if (IsTwoEyedJack(c))
        {
            // find best empty cell
            var empties = new List<(int r, int c2)>();
            for (int r = 0; r < 10; r++)
                for (int c2 = 0; c2 < 10; c2++)
                    if (seqChips[r,c2] == -1) empties.Add((r,c2));
            if (empties.Count == 0) { SeqFinishTurn(p, cardIdx); return; }
            var pick = empties.OrderByDescending(e => SeqAdjacencyScore(e.r, e.c2, myTeam)).First();
            seqChips[pick.r, pick.c2] = myTeam;
            seqMessage = $"{SeqPlayerName(p)} plays a wild Jack!";
            seqMessageTimer = 1.2f;
        }
        else if (IsOneEyedJack(c))
        {
            // find best opponent chip to remove
            var opp = new List<(int r, int c2)>();
            for (int r = 0; r < 10; r++)
                for (int c2 = 0; c2 < 10; c2++)
                    if (seqChips[r,c2] == 1 - myTeam) opp.Add((r,c2));
            if (opp.Count == 0) { SeqFinishTurn(p, cardIdx); return; }
            // remove from opponent's most dangerous position
            var rem = opp
                .Where(e => !SeqIsChipInSequence(e.r, e.c2, 1 - myTeam))
                .OrderByDescending(e => SeqAdjacencyScore(e.r, e.c2, 1-myTeam))
                .FirstOrDefault();
            if (rem == default) { SeqFinishTurn(p, cardIdx); return; }
            seqChips[rem.r, rem.c2] = -1;
            seqMessage = $"{SeqPlayerName(p)} removed a chip!";
            seqMessageTimer = 1.2f;
        }
        else
        {
            seqChips[row, col] = myTeam;
            seqMessage = $"{SeqPlayerName(p)} placed on {SeqCardLabel(c)}.";
            seqMessageTimer = 1f;
        }

        SeqFinishTurn(p, cardIdx);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  AI HELPERS
    // ─────────────────────────────────────────────────────────────────────
    static (int cardIdx, int row, int col)? SeqFindCompletingMove(int p, int team, List<Card> hand)
    {
        for (int ci = 0; ci < hand.Count; ci++)
        {
            var moves = SeqMovesForCard(p, hand[ci], ci);
            foreach (var (cardIdx, row, col) in moves)
            {
                // simulate placing and check if it completes a sequence
                int prev = seqChips[row, col];
                seqChips[row, col] = team;
                int seqs = SeqCountSequences(p);
                seqChips[row, col] = prev;
                if (seqs > seqSequences[team])
                    return (cardIdx, row, col);
            }
        }
        return null;
    }

    static (int cardIdx, int row, int col)? SeqFindBlockingMove(int p, int oppTeam, List<Card> hand)
    {
        // find cells that would complete a sequence for opponent
        // and see if we have a one-eyed jack to remove one of their chips there
        for (int ci = 0; ci < hand.Count; ci++)
        {
            Card c = hand[ci];
            if (!IsOneEyedJack(c)) continue;
            // find opponent's most dangerous chip
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                    if (seqChips[r, col] == oppTeam &&
                        SeqAdjacencyScore(r, col, oppTeam) >= 3)
                        return (ci, r, col);
        }
        return null;
    }

    static List<(int cardIdx, int row, int col)> SeqGetAllMoves(int p, List<Card> hand)
    {
        var moves = new List<(int,int,int)>();
        for (int ci = 0; ci < hand.Count; ci++)
            moves.AddRange(SeqMovesForCard(p, hand[ci], ci));
        return moves;
    }

    static List<(int cardIdx, int row, int col)> SeqMovesForCard(int p, Card c, int ci)
    {
        int myTeam = SeqTeamOf(p);
        var result = new List<(int,int,int)>();
        if (IsTwoEyedJack(c))
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                    if (seqChips[r, col] == -1) result.Add((ci, r, col));
        }
      
        else if (IsOneEyedJack(c))
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                    if (seqChips[r, col] == 1 - myTeam &&
                        !SeqIsChipInSequence(r, col, 1 - myTeam))
                        result.Add((ci, r, col));
        }

        else
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                {
                    var (bs, br) = seqBoard[r, col];
                    if (br == c.Rank && bs == c.Suit && seqChips[r, col] == -1)
                        result.Add((ci, r, col));
                }
        }
        return result;
    }

    static int SeqAdjacencyScore(int row, int col, int team)
    {
        int score = 0;
        int[] dr = { -1,-1,-1,0,0,1,1,1 };
        int[] dc = { -1,0,1,-1,1,-1,0,1 };
        for (int d = 0; d < 8; d++)
        {
            int nr = row + dr[d], nc = col + dc[d];
            if (nr >= 0 && nr < 10 && nc >= 0 && nc < 10 &&
                (seqChips[nr, nc] == team || seqChips[nr, nc] == 2))
                score++;
        }
        return score;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SEQUENCE DETECTION
    //  Returns total number of completed 5-in-a-row sequences for the given
    //  player's team (counts diagonals, horizontals, verticals).
    //  Corners (chip==2) count as wild for both teams.
    // ─────────────────────────────────────────────────────────────────────
    static int SeqCountSequences(int p)
    {
        int team = SeqTeamOf(p);
        bool[,] counted = new bool[10, 10];
        int total = 0;

        // directions: right, down, diagonal-right-down, diagonal-left-down
        int[][] dirs = { new[]{0,1}, new[]{1,0}, new[]{1,1}, new[]{1,-1} };

        foreach (var dir in dirs)
        {
            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 10; c++)
                {
                    // try to start a sequence here
                    if (!SeqCellBelongsToTeam(r, c, team)) continue;

                    // check 5 in a row
                    bool valid = true;
                    var cells = new List<(int,int)>();
                    for (int k = 0; k < 5; k++)
                    {
                        int nr = r + dir[0]*k, nc = c + dir[1]*k;
                        if (nr < 0 || nr >= 10 || nc < 0 || nc >= 10 ||
                            !SeqCellBelongsToTeam(nr, nc, team))
                        { valid = false; break; }
                        cells.Add((nr, nc));
                    }

                    if (valid && !cells.Any(cell => counted[cell.Item1, cell.Item2]))
                    {
                        total++;
                        foreach (var cell in cells) counted[cell.Item1, cell.Item2] = true;
                    }
                }
            }
        }
        return total;
    }

    static bool SeqCellBelongsToTeam(int r, int c, int team) =>
        seqChips[r, c] == team || seqChips[r, c] == 2;   // free corners count for both

    static bool SeqIsChipInSequence(int r, int c, int team)
    {
        int[][] dirs = { new[]{0,1}, new[]{1,0}, new[]{1,1}, new[]{1,-1} };
        foreach (var dir in dirs)
        {
            // walk back to the earliest start of a potential line through (r,c)
            int count = 0;
            int sr = r, sc = c;
            while (sr - dir[0] >= 0 && sr - dir[0] < 10 &&
                   sc - dir[1] >= 0 && sc - dir[1] < 10 &&
                   SeqCellBelongsToTeam(sr - dir[0], sc - dir[1], team))
            { sr -= dir[0]; sc -= dir[1]; }

            // now count forward
            int tr = sr, tc = sc;
            while (tr >= 0 && tr < 10 && tc >= 0 && tc < 10 &&
                   SeqCellBelongsToTeam(tr, tc, team))
            { count++; tr += dir[0]; tc += dir[1]; }

            if (count >= 5) return true;
        }
        return false;
    }

    static void SeqUpdateValidCells()
    {
        seqValidCells.Clear();
        int mySeat = MyViewSeat();
        if (seqSelectedCard < 0 || seqSelectedCard >= seqHands[mySeat].Count) return;
        Card c = seqHands[mySeat][seqSelectedCard];

        if (IsTwoEyedJack(c))
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                    if (seqChips[r, col] == -1) seqValidCells.Add((r, col));
        }
        else if (IsOneEyedJack(c))
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                    if (seqChips[r, col] == 1) seqValidCells.Add((r, col));
        }
        else
        {
            for (int r = 0; r < 10; r++)
                for (int col = 0; col < 10; col++)
                {
                    var (bs, br) = seqBoard[r, col];
                    if (br == c.Rank && bs == c.Suit && seqChips[r, col] == -1)
                        seqValidCells.Add((r, col));
                }
        }

        // dead card check — no valid cells and not a jack
        if (seqValidCells.Count == 0 && !IsOneEyedJack(c) && !IsTwoEyedJack(c))
        {
            seqMessage = "Dead card — press D to discard and draw.";
            seqMessageTimer = 3f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  DRAW
    // ─────────────────────────────────────────────────────────────────────
    static void DrawSequenceGame()
    {
        for (int p = 0; p < 4; p++)
            if (seqHands[p] == null)
            {
                Raylib.DrawText("Waiting for host...", ScreenWidth/2 - 100, ScreenHeight/2, 24, Color.White);
                return;
            }

        if (!seqActive) return;

        if (!seqGameOver && seqCurrentPlayer != lastAnnouncedSeqTurn)
        {
            lastAnnouncedSeqTurn = seqCurrentPlayer;
            bool mine = seqCurrentPlayer == MyViewSeat();
            seqMessage = mine ? "Your turn!" : $"{SeqDisplayName(seqCurrentPlayer)}'s turn";
            seqMessageTimer = 1.8f;
        }

        Raylib.ClearBackground(new Color((byte)15,(byte)35,(byte)15,(byte)255));

        // ── board ──
        int cellSize = 56;
        int boardW = 10 * cellSize;               // 560
        int boardOffX = (ScreenWidth - boardW) / 2;
        int boardOffY = 10;
        Vector2 mouse = Raylib.GetMousePosition();

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                int px = boardOffX + c * cellSize;
                int py = boardOffY + r * cellSize;
                var (bs, br) = seqBoard[r, c];
                bool isCorner = (bs == -1);
                bool isValid = seqValidCells.Contains((r, c));
                bool hovered = mouse.X >= px && mouse.X < px + cellSize &&
                               mouse.Y >= py && mouse.Y < py + cellSize;

                // cell background
                Color cellBg = isCorner
                    ? new Color((byte)60,(byte)60,(byte)60,(byte)255)
                    : new Color((byte)30,(byte)55,(byte)30,(byte)255);
                if (isValid) cellBg = new Color((byte)60,(byte)90,(byte)20,(byte)255);
                if (hovered && isValid) cellBg = new Color((byte)80,(byte)120,(byte)20,(byte)255);
                Raylib.DrawRectangle(px, py, cellSize-2, cellSize-2, cellBg);

                // card label on cell
                // card label on cell
                if (isCorner)
                {
                    Raylib.DrawText("FREE", px+8, py+18, 13, Color.Gold);
                }
                else
                {
                    DrawCardFace(new Card(bs, br), px + 1, py + 1, false);
                }

                // chip drawn ON TOP of card face
                int chip = seqChips[r, c];
                if (chip == 0)
                    Raylib.DrawCircle(px + cellSize/2 - 5, py + cellSize/2 - 1, 18,
                        new Color((byte)60,(byte)130,(byte)255,(byte)220));
                else if (chip == 1)
                    Raylib.DrawCircle(px + cellSize/2 - 5, py + cellSize/2 - 1, 18,
                        new Color((byte)255,(byte)70,(byte)70,(byte)220));
                else if (chip == 2)
                    Raylib.DrawCircle(px + cellSize/2 - 5, py + cellSize/2 - 1, 18,
                        new Color((byte)180,(byte)180,(byte)60,(byte)200));

                // border last so it sits on top of everything
                Color border = isValid ? Color.Yellow : new Color((byte)40,(byte)80,(byte)40,(byte)255);
                Raylib.DrawRectangleLines(px, py, cellSize-2, cellSize-2, border);

                // click handler
                if (hovered && isValid && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    SeqHumanClickCell(r, c);
            }
        }
// ── LEFT PANEL: Me + Jake ──
        int leftX = 10;
        int midY = ScreenHeight / 2;

        // Me (player 0)
        bool turn0 = seqCurrentPlayer == 0 && !seqGameOver;
        Raylib.DrawRectangle(leftX, midY - 160, 170, 60, new Color((byte)20,(byte)20,(byte)40,(byte)200));
        Raylib.DrawRectangleLinesEx(new Rectangle(leftX, midY - 160, 170, 60), turn0 ? 4 : 2,
            turn0 ? Color.Gold : SeqTeamColor(0));
        Raylib.DrawText(SeqDisplayName(0), leftX + 8, midY - 152, 20, Color.White);
        SeqDrawPlayerPortrait(0, leftX + 140, midY - 130);

        // Jake (player 2)
        bool turn2 = seqCurrentPlayer == 2 && !seqGameOver;
        Raylib.DrawRectangle(leftX, midY - 80, 170, 60, new Color((byte)20,(byte)20,(byte)40,(byte)200));
        Raylib.DrawRectangleLinesEx(new Rectangle(leftX, midY - 80, 170, 60), turn2 ? 4 : 2,
            turn2 ? Color.Gold : SeqTeamColor(0));
        Raylib.DrawText(SeqDisplayName(2), leftX + 8, midY - 72, 20, Color.White);
        SeqDrawPlayerPortrait(2, leftX + 140, midY - 50);

        // Tricks (team 0 sequences)
        Raylib.DrawRectangle(leftX, midY + 10, 170, 50, new Color((byte)20,(byte)20,(byte)40,(byte)200));
        Raylib.DrawRectangleLines(leftX, midY + 10, 170, 50, SeqTeamColor(0));
        Raylib.DrawText("Tricks", leftX + 8, midY + 18, 18, SeqTeamColor(0));
        for (int s = 0; s < 2; s++)
        {
            Color filled = s < seqSequences[0] ? SeqTeamColor(0) : new Color((byte)50,(byte)50,(byte)50,(byte)255);
            Raylib.DrawRectangle(leftX + 8 + s * 36, midY + 38, 30, 16, filled);
            Raylib.DrawRectangleLines(leftX + 8 + s * 36, midY + 38, 30, 16, SeqTeamColor(0));
        }

        // Controls
        Raylib.DrawRectangle(leftX, midY + 76, 170, 140, new Color((byte)15,(byte)15,(byte)30,(byte)200));
        Raylib.DrawRectangleLines(leftX, midY + 76, 170, 140, Color.DarkGray);
        int cy2 = midY + 84;
        Raylib.DrawText("Controls", leftX + 8, cy2, 15, Color.LightGray); cy2 += 18;
        Raylib.DrawText("1-7 Select", leftX + 8, cy2, 13, Color.Gray); cy2 += 15;
        Raylib.DrawText("Click Place", leftX + 8, cy2, 13, Color.Gray); cy2 += 15;
        Raylib.DrawText("ESC Deselect", leftX + 8, cy2, 13, Color.Gray); cy2 += 15;
        Raylib.DrawText("D Dead card", leftX + 8, cy2, 13, Color.Gray);
        Raylib.DrawText("Q Quit", leftX + 8, cy2 + 15, 13, Color.Gray);

        // ── RIGHT PANEL: C Ride + Shack ──
        int rightX = boardOffX + boardW + 8;

        // C Ride (player 3)
        bool turn3 = seqCurrentPlayer == 3 && !seqGameOver;
        Raylib.DrawRectangle(rightX + 160, midY - 160, 170, 60, new Color((byte)40,(byte)20,(byte)20,(byte)200));
        Raylib.DrawRectangleLinesEx(new Rectangle(rightX + 160, midY - 160, 170, 60), turn3 ? 4 : 2,
            turn3 ? Color.Gold : SeqTeamColor(1));
        Raylib.DrawText(SeqDisplayName(3), rightX + 168, midY - 152, 20, Color.White);
        SeqDrawPlayerPortrait(3, rightX + 300, midY - 130);

        // Shack (player 1)
        bool turn1 = seqCurrentPlayer == 1 && !seqGameOver;
        Raylib.DrawRectangle(rightX + 160, midY - 80, 170, 60, new Color((byte)40,(byte)20,(byte)20,(byte)200));
        Raylib.DrawRectangleLinesEx(new Rectangle(rightX + 160, midY - 80, 170, 60), turn1 ? 4 : 2,
            turn1 ? Color.Gold : SeqTeamColor(1));
        Raylib.DrawText(SeqDisplayName(1), rightX + 168, midY - 72, 20, Color.White);
        SeqDrawPlayerPortrait(1, rightX + 300, midY - 50);

        // Tricks (team 1 sequences)
        Raylib.DrawRectangle(rightX + 160, midY + 10, 170, 50, new Color((byte)40,(byte)20,(byte)20,(byte)200));
        Raylib.DrawRectangleLines(rightX + 160, midY + 10, 170, 50, SeqTeamColor(1));
        Raylib.DrawText("Tricks", rightX + 168, midY + 18, 18, SeqTeamColor(1));
        for (int s = 0; s < 2; s++)
        {
            Color filled = s < seqSequences[1] ? SeqTeamColor(1) : new Color((byte)50,(byte)50,(byte)50,(byte)255);
            Raylib.DrawRectangle(rightX + 168 + s * 36, midY + 38, 30, 16, filled);
            Raylib.DrawRectangleLines(rightX + 168 + s * 36, midY + 38, 30, 16, SeqTeamColor(1));
        }

        // Card amounts
        Raylib.DrawRectangle(rightX, midY + 76, 170, 140, new Color((byte)15,(byte)15,(byte)30,(byte)200));
        Raylib.DrawRectangleLines(rightX, midY + 76, 170, 140, Color.DarkGray);
        int ry = midY + 84;
        Raylib.DrawText("Card amounts", rightX + 8, ry, 14, Color.LightGray); ry += 20;
        for (int p = 1; p < 4; p++)
        {
            Color pc = SeqTeamColor(SeqTeamOf(p));
            Raylib.DrawText($"{SeqPlayerName(p)}: {seqHands[p].Count}", rightX + 8, ry, 14, pc);
            ry += 18;
        }
        int remaining = seqDeck.Count - seqDeckIdx;
        Raylib.DrawText($"Deck: {remaining}", rightX + 8, ry, 13, Color.Gray);

        // Turn + title top centre
        string turnStr = $"Turn: {SeqPlayerName(seqCurrentPlayer)}";
        int tw = Raylib.MeasureText(turnStr, 20);
        Color turnCol = SeqTeamColor(SeqTeamOf(seqCurrentPlayer));
   Raylib.DrawText("SEQUENCE", leftX + 8, 6, 22, Color.Gold);
        Raylib.DrawText(turnStr, leftX + 8, 30, 18, turnCol);

        // ── human hand ──
        Raylib.DrawRectangle(0, ScreenHeight - 115, ScreenWidth, 115, new Color((byte)10,(byte)10,(byte)25,(byte)220));
        SeqDrawHumanHand(mouse);

        // ── message banner ──
        if (seqMessageTimer > 0 && seqMessage.Length > 0)
        {
            int mw = Raylib.MeasureText(seqMessage, 22);
            int mx = ScreenWidth/2 - mw/2 - 12;
            Raylib.DrawRectangle(mx, ScreenHeight - 145, mw + 24, 36, new Color((byte)0,(byte)0,(byte)0,(byte)200));
            Raylib.DrawText(seqMessage, mx + 12, ScreenHeight - 139, 22, Color.Gold);
        }

        // ── dead card D key ──
        int mySeatDead = MyViewSeat();
        if (seqSelectedCard >= 0 && seqCurrentPlayer == mySeatDead &&
            seqValidCells.Count == 0 && Raylib.IsKeyPressed(KeyboardKey.D))
        {
            if (!multiplayer.Connected || multiplayer.IsHost)
            {
                seqHands[mySeatDead].RemoveAt(seqSelectedCard);
                if (seqDeckIdx < seqDeck.Count) seqHands[mySeatDead].Add(SeqDrawCard());
                seqSelectedCard = -1;
                seqValidCells.Clear();
                seqMessage = "Dead card discarded, drew a new one.";
                seqMessageTimer = 1.5f;
                seqCurrentPlayer = (mySeatDead + 1) % 4;
                seqAiTimer = 0.9f;
                if (multiplayer.Connected) SeqBroadcastState();
            }
            else
            {
                multiplayer.SendCardAction($"SEQDISCARD|{seqSelectedCard}");
            }
        }

        // ── game over overlay ──
        if (seqGameOver)
        {
            Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, new Color((byte)0,(byte)0,(byte)0,(byte)160));
            string winMsg = SeqTeamLabel(seqWinTeam) + " WIN!";
            int wmw = Raylib.MeasureText(winMsg, 48);
            Raylib.DrawRectangle(ScreenWidth/2 - wmw/2 - 24, ScreenHeight/2 - 60, wmw + 48, 100,
                new Color((byte)0,(byte)0,(byte)0,(byte)230));
            Raylib.DrawText(winMsg, ScreenWidth/2 - wmw/2, ScreenHeight/2 - 46, 48,
                SeqTeamColor(seqWinTeam));
            Raylib.DrawText("Q = Exit", ScreenWidth/2 - 46, ScreenHeight/2 + 24, 22, Color.White);
        }
    }

    static void SeqDrawHumanHand(Vector2 mouse)
{
    int mySeat = MyViewSeat();
    var hand = seqHands[mySeat];
    if (hand == null) return;
    int n = hand.Count;
    if (n == 0) return;

    int cw = 60, gap = 66;
    int totalW = n * gap;
    int startX = ScreenWidth / 2 - totalW / 2;
    int y = ScreenHeight - 100;

    for (int i = 0; i < n; i++)
    {
        Card c = hand[i];
        int x = startX + i * gap;
        bool sel = (seqSelectedCard == i);
        int lift = sel ? 18 : 0;
        bool hov = mouse.X >= x && mouse.X < x + cw &&
                   mouse.Y >= y - lift && mouse.Y < y - lift + 84;

        if (hov && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            seqSelectedCard = (seqSelectedCard == i) ? -1 : i;
            SeqUpdateValidCells();
        }

        // reuse DrawCardFace from CardGames_1.cs — handles suit symbols, rank, joker
        DrawCardFace(c, x, y - lift, true, sel);

        // jack type label below card
        if (c.Rank == 11)
        {
            string jlabel = IsOneEyedJack(c) ? "REMOVE" : "WILD";
            Color jcol   = IsOneEyedJack(c) ? Color.Red : Color.Green;
            int lw = Raylib.MeasureText(jlabel, 11);
            Raylib.DrawText(jlabel, x + cw / 2 - lw / 2, y - lift + 88, 11, jcol);
        }

        // number hint
        Raylib.DrawText((i + 1).ToString(), x + cw / 2 - 4, y - lift + 70, 12,
            sel ? Color.Gold : Color.LightGray);
    }
}

    static int boardOffXSeq() => 60;   // matches boardOffX in DrawSequenceGame

    // ─────────────────────────────────────────────────────────────────────
    //  SEAT PORTRAITS  (reuse DrawCardPlayer style)
    //  Laid out vertically in the right panel: You / Shack / Jake / CRide
    // ─────────────────────────────────────────────────────────────────────
    static void SeqDrawSeats(int panelX, int startY)
    {
        // 2 columns of 2: You+Jake (blue) left, Shack+CRide (red) right
        (int p, int ox, int oy)[] layout =
        {
            (0, 0,   0),    // You
            (2, 0,  80),    // Jake  (your partner)
            (1, 110, 0),    // Shack
            (3, 110, 80),   // CRide
        };
        foreach (var (p, ox, oy) in layout)
        {
            int cx = panelX + ox + 28;
            int cy = startY + oy + 28;
            bool isActive = (seqCurrentPlayer == p);
            if (isActive)
                Raylib.DrawCircle(cx, cy, 30, new Color((byte)60,(byte)60,(byte)20,(byte)180));
            SeqDrawPlayerPortrait(p, cx, cy);
            Color nc = SeqTeamColor(SeqTeamOf(p));
            int nw = Raylib.MeasureText(SeqPlayerName(p), 13);
            Raylib.DrawText(SeqPlayerName(p), cx - nw/2, cy + 24, 13, nc);
            if (isActive)
                Raylib.DrawText("▲", cx - 5, cy - 40, 14, Color.Yellow);
        }
    }

    static void SeqDrawPlayerPortrait(int p, int cx, int cy)
    {
        // p=0 You, p=1 Shack(female), p=2 Jake(male), p=3 CRide(female)
        (Color hair, Color shirt, bool female) look = p switch
        {
            0 => (player.SkinColor, player.ShirtColor, false),
            1 => (new Color((byte)20,(byte)15,(byte)10,(byte)255),   // Shack: dark hair
                  new Color((byte)180,(byte)60,(byte)60,(byte)255),   // red shirt
                  true),
            2 => (new Color((byte)60,(byte)35,(byte)10,(byte)255),   // Jake: brown hair
                  new Color((byte)40,(byte)100,(byte)180,(byte)255),  // blue shirt
                  false),
            3 => (new Color((byte)150,(byte)90,(byte)20,(byte)255),  // CRide: auburn hair
                  new Color((byte)60,(byte)160,(byte)100,(byte)255),  // green shirt
                  true),
            _ => (Color.Gray, Color.Gray, false)
        };

        Color skin = p == 0 ? player.SkinColor : new Color((byte)210,(byte)175,(byte)140,(byte)255);

        // body
        Raylib.DrawRectangle(cx - 14, cy + 6, 28, 20, look.shirt);
        // neck
        Raylib.DrawRectangle(cx - 4, cy, 8, 8, skin);
        // head
        Raylib.DrawCircle(cx, cy - 8, 12, skin);

        if (look.female)
        {
            // long hair
            Raylib.DrawCircle(cx, cy - 10, 13, look.hair);
            Raylib.DrawRectangle(cx - 13, cy - 10, 5, 24, look.hair);
            Raylib.DrawRectangle(cx + 8,  cy - 10, 5, 24, look.hair);
            Raylib.DrawCircle(cx, cy - 7, 10, skin);
        }
        else
        {
            // short hair
            Raylib.DrawCircle(cx, cy - 12, 12, look.hair);
            Raylib.DrawRectangle(cx - 12, cy - 14, 24, 8, look.hair);
            Raylib.DrawCircle(cx, cy - 6, 10, skin);
        }

        // eyes
        Raylib.DrawCircle(cx - 4, cy - 9, 2, Color.Black);
        Raylib.DrawCircle(cx + 4, cy - 9, 2, Color.Black);
        // mouth
        Raylib.DrawRectangle(cx - 3, cy - 2, 6, 2, new Color((byte)140,(byte)70,(byte)70,(byte)255));

        // Jake gets a beard
        if (p == 2)
            Raylib.DrawRectangle(cx - 6, cy - 3, 12, 5, new Color((byte)50,(byte)30,(byte)10,(byte)255));
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────────────────────────────────
    static string SeqCardLabel(Card c)
    {
        if (c.IsJoker) return "Joker";
        string r = c.Rank switch { 1=>"A",11=>"J",12=>"Q",13=>"K", _=>c.Rank.ToString() };
        string s = c.Suit switch { 0=>"♣",1=>"♦",2=>"♥",3=>"♠", _=>"?" };
        return r + s;
    }
}
}
