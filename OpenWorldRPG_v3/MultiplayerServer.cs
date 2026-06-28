// ============================================================================
//  MULTIPLAYER LAN SERVER — OpenWorldRPG
//  Local network co-op: up to 4 players, same shared world.
//
//  ARCHITECTURE
//  ─────────────────────────────────────────────────────────────────────────
//  One player clicks "Host Game" → becomes the server (still plays normally).
//  Other players click "Join Game" → enter the host IP and connect.
//  The server broadcasts the world state to all clients every tick.
//  Each client sends their player state to the server every tick.
//  The server merges all player states and broadcasts the full picture.
//
//  PROTOCOL (plain text over TCP, one line per message, newline-terminated)
//  ─────────────────────────────────────────────────────────────────────────
//  Client → Server:
//    HELLO|<name>                          (initial handshake)
//    PLAYER|<x>|<y>|<hp>|<facing>|<scene> (sent every frame)
//    CHAT|<message>                        (optional chat)
//    BYE                                   (clean disconnect)
//
//  Server → Client:
//    WELCOME|<assignedId>                  (reply to HELLO)
//    WORLD|<id>|<x>|<y>|<hp>|<facing>|<scene>|<name>|... (all players)
//    CHAT|<fromId>|<name>|<message>
//    FULL                                  (server is full, reject)
//    BYE                                   (server shutting down)
//
//  ─────────────────────────────────────────────────────────────────────────
//  HOOK-IN CHECKLIST  (minimal changes to Program.cs)
//  ─────────────────────────────────────────────────────────────────────────
//
//  1. Add to the top of Program.cs usings:
//       using OpenWorldRPG;
//
//  2. Add static fields near your other static vars:
//       static MultiplayerManager multiplayer = new MultiplayerManager();
//       static List<RemotePlayer> remotePlayers => multiplayer.RemotePlayers;
//
//  3. In your Update loop (World scene), add ONE call:
//       multiplayer.Update(player, playerName, currentScene.ToString());
//
//  4. In DrawWorld(), after drawing your player, add:
//       foreach (var rp in remotePlayers) rp.Draw();
//
//  5. In DrawHUD() or your main menu, call:
//       multiplayer.DrawStatusOverlay();
//
//  6. In your main menu, add two buttons that call:
//       multiplayer.StartHost();          // Host Game button
//       multiplayer.StartClient(ipInput); // Join Game button (pass the IP string)
//       multiplayer.Stop();               // Disconnect button
//
//  7. In DrawWorld() for chat display, call:
//       multiplayer.DrawChat();
//
//  8. When the player sends a chat (e.g. pressing Enter on a chat line):
//       multiplayer.SendChat(chatInput);
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using Raylib_cs;

namespace OpenWorldRPG
{
    // ─────────────────────────────────────────────────────────────────────────
    //  REMOTE PLAYER — one entry per connected peer, rendered in the world
    // ─────────────────────────────────────────────────────────────────────────
    public class RemotePlayer
    {
        public int    Id;
        public string Name      = "Player";
        public float  X, Y;
        public int    HP;
        public string Facing    = "Down";
        public string Scene     = "World";
        public float  LastSeen  = 0f;   // seconds since last update
        public bool   Active    => LastSeen < 5f;
        public string ChatBubble = "";
        public float  ChatTimer  = 0f;
        public bool  IsTyping   = false;
        public float TypingTimer = 0f;

        // Simple walk animation
        private float _animTimer = 0f;
        private int   _animFrame = 0;

        public void UpdateAnim(float dt)
        {
            _animTimer += dt;
            if (_animTimer > 0.18f) { _animTimer = 0f; _animFrame = (_animFrame + 1) % 4; }
        }

        public void Draw()
{
    if (!Active || Scene != "World") return;

    int px = (int)X;
    int py = (int)Y;

    // Shadow
    Raylib.DrawEllipse(px + 20, py + 48, 18, 6, new Color((byte)0,(byte)0,(byte)0,(byte)50));

    // Body
    Raylib.DrawRectangle(px + 8, py + 20, 24, 28, new Color((byte)20,(byte)160,(byte)180,(byte)255));

    // Head — matches player head at y+12, centre x+20
    Raylib.DrawCircle(px + 20, py + 12, 12, new Color((byte)220,(byte)185,(byte)140,(byte)255));

    // Legs
    int legOff = (_animFrame % 2 == 0) ? 3 : -3;
    Raylib.DrawRectangle(px + 8,  py + 48, 9, 14, new Color((byte)60,(byte)80,(byte)140,(byte)255));
    Raylib.DrawRectangle(px + 19, py + 48 + legOff, 9, 14, new Color((byte)60,(byte)80,(byte)140,(byte)255));

    // Name tag — above head
    int nameW = Raylib.MeasureText(Name, 14);
    Raylib.DrawRectangle(px + 20 - nameW / 2 - 4, py - 22, nameW + 8, 18,
        new Color((byte)0,(byte)0,(byte)0,(byte)160));
    Raylib.DrawText(Name, px + 20 - nameW / 2, py - 21, 14,
        new Color((byte)80,(byte)240,(byte)220,(byte)255));

// age timers
if (ChatTimer   > 0f) ChatTimer   -= Raylib.GetFrameTime();
if (TypingTimer > 0f) TypingTimer -= Raylib.GetFrameTime();

// decide what to show
string bubbleText  = "";
float  bubbleAlpha = 0f;
bool   showBubble  = false;

if (IsTyping && TypingTimer > 0f)
{
    bubbleText  = "typing...";
    bubbleAlpha = 1f;
    showBubble  = true;
}
else if (ChatTimer > 0f && ChatBubble.Length > 0)
{
    bubbleText  = ChatBubble;
    bubbleAlpha = Math.Clamp(ChatTimer / 5f, 0f, 1f);
    showBubble  = true;
}

if (showBubble)
{
    byte alpha    = (byte)(230 * bubbleAlpha);
    byte txtAlpha = (byte)(255 * bubbleAlpha);
    int  fontSize = 13;
    int  tw2      = Raylib.MeasureText(bubbleText, fontSize);
    int  bw       = Math.Min(tw2 + 16, 260);
    int  bh       = fontSize + 14;
    int  bx2      = px + 20 - bw / 2;
    int  by2      = py - 72 - bh;

    Raylib.DrawRectangle(bx2, by2, bw, bh,
        new Color((byte)255,(byte)255,(byte)255,(byte)alpha));
    Raylib.DrawRectangleLines(bx2, by2, bw, bh,
        new Color((byte)80,(byte)80,(byte)80,(byte)alpha));
    Raylib.DrawTriangle(
        new Vector2(px + 14, by2 + bh),
        new Vector2(px + 26, by2 + bh),
        new Vector2(px + 20, by2 + bh + 8),
        new Color((byte)255,(byte)255,(byte)255,(byte)alpha));

    string display = IsTyping ? bubbleText
        : (tw2 > 240 ? bubbleText[..Math.Min(bubbleText.Length, 22)] + "..." : bubbleText);
    Color dotCol = IsTyping
        ? new Color((byte)100,(byte)100,(byte)100,(byte)txtAlpha)
        : new Color((byte)20,(byte)20,(byte)20,(byte)txtAlpha);
    Raylib.DrawText(display, bx2 + 8, by2 + 7, fontSize, dotCol);
}

    // HP bar
    float hpPct = Math.Clamp(HP / 100f, 0f, 1f);
    Raylib.DrawRectangle(px + 2, py - 30, 36, 5, new Color((byte)80,(byte)0,(byte)0,(byte)200));
    Raylib.DrawRectangle(px + 2, py - 30, (int)(36 * hpPct), 5,
        hpPct > 0.5f ? Color.Green : hpPct > 0.25f ? Color.Orange : Color.Red);
}
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CHAT MESSAGE
    // ─────────────────────────────────────────────────────────────────────────
    public class ChatMessage
    {
        public string Name;
        public string Text;
        public float  Timer = 8f;   // seconds before it fades
        public Color  Col;

        public ChatMessage(string name, string text, bool isSystem = false)
        {
            Name = name;
            Text = text;
            Col  = isSystem
                ? new Color((byte)180, (byte)220, (byte)100, (byte)255)
                : new Color((byte)240, (byte)240, (byte)240, (byte)255);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MULTIPLAYER MANAGER — single object you keep on Program
    // ─────────────────────────────────────────────────────────────────────────
    public class MultiplayerManager
    {
        // ── public state ──────────────────────────────────────────────────────
        public bool   IsHost    { get; private set; }
        public bool   IsClient  { get; private set; }
        public bool   Connected => IsHost || (IsClient && _clientConnected);
        public string StatusText { get; private set; } = "";
        public List<RemotePlayer> RemotePlayers { get; } = new List<RemotePlayer>();
        public List<ChatMessage>  ChatLog       { get; } = new List<ChatMessage>();

        // ── config ────────────────────────────────────────────────────────────
        public const int  PORT        = 7777;
        public const int  MAX_PLAYERS = 4;
        public const float SEND_RATE  = 0.05f;   // send update 20× per second

        // ── server-side ───────────────────────────────────────────────────────
        private TcpListener                         _listener;
        private List<ConnectedClient>               _clients     = new();
        private Thread                              _acceptThread;
        private readonly object                     _clientLock  = new();

        // ── client-side ───────────────────────────────────────────────────────
        private TcpClient                           _tcpClient;
        private StreamReader                        _reader;
        private StreamWriter                        _writer;
        private Thread                              _readThread;
        private bool                                _clientConnected;
        private int                                 _myId        = -1;
        private readonly object                     _writeLock   = new();

        // ── shared ────────────────────────────────────────────────────────────
        private ConcurrentQueue<string>             _incomingMessages = new();
        private float                               _sendTimer        = 0f;
        private bool                                _running          = false;
        private string _lastPlayerName = "Host";

        // ─────────────────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Host a game on the local network. Call from your Host button.</summary>
        public void StartHost()
        {
            if (_running) Stop();
            IsHost  = true;
            _running = true;
            StatusText = $"Hosting on port {PORT}…";

            try
            {
                _listener = new TcpListener(IPAddress.Any, PORT);
                _listener.Start();

                // print local IPs so friends know what to type
                string ips = GetLocalIPAddresses();
                StatusText = $"Hosting — your IP: {ips}";
                AddChat("SERVER", $"Server started. Share your IP: {ips}", true);

                _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "MP-Accept" };
                _acceptThread.Start();
            }
            catch (Exception ex)
            {
                StatusText = $"Host failed: {ex.Message}";
                IsHost = _running = false;
            }
        }

        /// <summary>Connect to a host. Pass the IP string from your input field.</summary>
        public void StartClient(string hostIp)
        {
            if (_running) Stop();
            if (string.IsNullOrWhiteSpace(hostIp)) { StatusText = "Enter a host IP first."; return; }

            IsClient   = true;
            _running   = true;
            StatusText = $"Connecting to {hostIp}:{PORT}…";

            Thread t = new Thread(() => ClientConnectLoop(hostIp)) { IsBackground = true, Name = "MP-Connect" };
            t.Start();
        }

        /// <summary>Disconnect cleanly.</summary>
        public void Stop()
        {
            _running = false;

            if (IsHost)
            {
                lock (_clientLock)
                    foreach (var c in _clients) TrySendToClient(c, "BYE");
                _listener?.Stop();
            }
            else if (IsClient)
            {
                TrySendToServer("BYE");
                _tcpClient?.Close();
            }

            IsHost = IsClient = _clientConnected = false;
            _myId  = -1;
            lock (_clientLock) _clients.Clear();
            RemotePlayers.Clear();
            StatusText = "Disconnected.";
        }

        /// <summary>
        /// Call once per frame from Program.Update().
        /// Flushes incoming messages, sends our state outbound.
        /// </summary>
        internal void Update(Player player, string playerName, string scene)
        {
            if (!Connected) return;
            _lastPlayerName = playerName;

            // drain incoming queue
            while (_incomingMessages.TryDequeue(out string msg))
                HandleMessage(msg);

            // age remote players
            float dt = Raylib.GetFrameTime();
            lock (RemotePlayers)
                foreach (var rp in RemotePlayers)
                {
                    rp.LastSeen += dt;
                    rp.UpdateAnim(dt);
                }

            // age chat
            for (int i = ChatLog.Count - 1; i >= 0; i--)
            {
                ChatLog[i].Timer -= dt;
                if (ChatLog[i].Timer <= 0) ChatLog.RemoveAt(i);
            }

            // send our state on a timer
            _sendTimer -= dt;
            if (_sendTimer <= 0f)
            {
                _sendTimer = SEND_RATE;
                string facing = "Down"; // replace with your player.Facing.ToString() if accessible
                string payload = $"PLAYER|{player.Position.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"|{player.Position.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"|{player.Health}|{facing}|{scene}|{playerName}";

                if (IsHost)
                    BroadcastWorldState(playerName, player, scene);
                else
                    TrySendToServer(payload);
            }
        }

        /// <summary>Send a chat message.</summary>
        public void SendChat(string text)
{
    if (!Connected || string.IsNullOrWhiteSpace(text)) return;

    if (IsHost)
    {
        // host handles chat directly — no server to send to
        string hostName = _lastPlayerName; // store name from Update
        AddChat(hostName, text);
        // broadcast to all clients with id 0 (host)
        lock (_clientLock)
            foreach (var cc in _clients)
                TrySendToClient(cc, $"CHAT|0|{hostName}|{text}");
    }
    else
    {
        TrySendToServer($"CHAT|{text}");
    }
}

        public void SendTyping(bool isTyping)
{
    if (!Connected) return;

    if (IsHost)
    {
        // broadcast typing state to all clients as id 0
        lock (_clientLock)
            foreach (var cc in _clients)
                TrySendToClient(cc, $"TYPING|0|{(isTyping ? "1" : "0")}");
    }
    else
    {
        TrySendToServer($"TYPING|{(isTyping ? "1" : "0")}");
    }
}

        // ─────────────────────────────────────────────────────────────────────
        //  DRAW HELPERS (call from inside BeginDrawing / EndDrawing)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Small status bar in top-right corner.</summary>
        public void DrawStatusOverlay()
        {
            if (!Connected && string.IsNullOrEmpty(StatusText)) return;

            Color col = Connected
                ? new Color((byte)40,  (byte)200, (byte)100, (byte)200)
                : new Color((byte)200, (byte)80,  (byte)40,  (byte)200);

            int w = Raylib.MeasureText(StatusText, 14) + 16;
            Raylib.DrawRectangle(1280 - w - 4, 4, w, 22, new Color((byte)0, (byte)0, (byte)0, (byte)160));
            Raylib.DrawRectangleLines(1280 - w - 4, 4, w, 22, col);
            Raylib.DrawText(StatusText, 1280 - w + 4, 8, 14, col);

            if (Connected)
            {
                int count;
                lock (_clientLock) count = IsHost ? _clients.Count + 1 : RemotePlayers.Count + 1;
                string players = $"{count}/{MAX_PLAYERS} players";
                int pw = Raylib.MeasureText(players, 12);
                Raylib.DrawText(players, 1280 - pw - 8, 28, 12,
                    new Color((byte)180, (byte)180, (byte)180, (byte)200));
            }
        }

        /// <summary>Chat log in bottom-left. Call from DrawWorld or DrawHUD.</summary>
        public void DrawChat()
        {
            if (ChatLog.Count == 0) return;

            int y = 720 - 40;
            for (int i = ChatLog.Count - 1; i >= 0 && i >= ChatLog.Count - 6; i--)
            {
                var cm = ChatLog[i];
                float fade = Math.Min(1f, cm.Timer);
                byte alpha = (byte)(200 * fade);
                string line = $"{cm.Name}: {cm.Text}";
                int w = Raylib.MeasureText(line, 15) + 12;
                Raylib.DrawRectangle(4, y - 2, w, 19, new Color((byte)0, (byte)0, (byte)0, (byte)(120 * fade)));
                Raylib.DrawText(line, 8, y, 15,
                    new Color(cm.Col.R, cm.Col.G, cm.Col.B, alpha));
                y -= 22;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SERVER — accept loop + per-client read loop
        // ─────────────────────────────────────────────────────────────────────

        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    TcpClient incoming = _listener.AcceptTcpClient();
                    lock (_clientLock)
                    {
                        if (_clients.Count >= MAX_PLAYERS - 1)   // -1 because host counts
                        {
                            var tmp = new StreamWriter(incoming.GetStream(), Encoding.UTF8) { AutoFlush = true };
                            tmp.WriteLine("FULL");
                            incoming.Close();
                            continue;
                        }

                        int id = AllocateId();
                        var cc = new ConnectedClient(id, incoming);
                        _clients.Add(cc);

                        Thread rt = new Thread(() => ClientReadLoop(cc))
                            { IsBackground = true, Name = $"MP-Read-{id}" };
                        rt.Start();
                    }
                }
                catch (SocketException) { break; }
                catch (Exception ex) { Console.WriteLine($"[MP] AcceptLoop error: {ex.Message}"); }
            }
        }

        private void ClientReadLoop(ConnectedClient cc)
        {
            try
            {
                while (_running)
                {
                    string line = cc.Reader.ReadLine();
                    if (line == null) break;
                    _incomingMessages.Enqueue($"FROM:{cc.Id}|{line}");
                }
            }
            catch { /* client disconnected */ }
            finally { RemoveClient(cc); }
        }

        private void RemoveClient(ConnectedClient cc)
        {
            lock (_clientLock) _clients.Remove(cc);
            cc.Tcp.Close();

            lock (RemotePlayers)
                RemotePlayers.RemoveAll(rp => rp.Id == cc.Id);

            string name = cc.Name ?? $"Player {cc.Id}";
            AddChat("SERVER", $"{name} disconnected.", true);
            StatusText = $"Hosting — {_clients.Count + 1}/{MAX_PLAYERS} players";
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CLIENT — connect + read loop
        // ─────────────────────────────────────────────────────────────────────

        private void ClientConnectLoop(string hostIp)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(hostIp, PORT);
                _reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
                _writer = new StreamWriter(_tcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
                _clientConnected = true;
                StatusText = $"Connected to {hostIp}";

                // handshake — name will be sent on next Update tick via HELLO
                TrySendToServer("HELLO|Player");   // replaced with real name in first Update

                _readThread = new Thread(ClientReadFromServerLoop)
                    { IsBackground = true, Name = "MP-ClientRead" };
                _readThread.Start();
            }
            catch (Exception ex)
            {
                StatusText    = $"Connection failed: {ex.Message}";
                IsClient      = _running = _clientConnected = false;
            }
        }

        private void ClientReadFromServerLoop()
        {
            try
            {
                while (_running)
                {
                    string line = _reader.ReadLine();
                    if (line == null) break;
                    _incomingMessages.Enqueue(line);
                }
            }
            catch { /* disconnected */ }
            finally
            {
                _clientConnected = false;
                StatusText = "Disconnected from host.";
                AddChat("SERVER", "Lost connection to host.", true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  MESSAGE HANDLER (runs on main thread via queue)
        // ─────────────────────────────────────────────────────────────────────

        private void HandleMessage(string raw)
        {
            // ── server receiving from a client ─────────────────────────────
            if (IsHost && raw.StartsWith("FROM:"))
            {
                int pipe = raw.IndexOf('|');
                if (pipe < 0) return;
                int fromId = int.Parse(raw.Substring(5, pipe - 5));
                string msg = raw.Substring(pipe + 1);
                string[] parts = msg.Split('|');

                switch (parts[0])
                {
                    case "HELLO":
                        string name = parts.Length > 1 ? parts[1] : $"Player{fromId}";
                        lock (_clientLock)
                        {
                            var cc = _clients.Find(c => c.Id == fromId);
                            if (cc != null) cc.Name = name;
                        }
                        TrySendToId(fromId, $"WELCOME|{fromId}");
                        AddChat("SERVER", $"{name} joined!", true);
                        StatusText = $"Hosting — {_clients.Count + 1}/{MAX_PLAYERS} players";
                        break;

                    case "PLAYER":
                        if (parts.Length >= 7)
                        {
                            float px = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                            float py = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                            int   hp = int.Parse(parts[3]);
                            string playerName2 = parts[6];
                            // also update the client's stored name
                            lock (_clientLock)
                            {
                                var cc2 = _clients.Find(c => c.Id == fromId);
                                if (cc2 != null && !string.IsNullOrEmpty(playerName2))
                                    cc2.Name = playerName2;
                            }
                            UpdateRemotePlayer(fromId, px, py, hp, parts[4], parts[5], playerName2);
                        }
                        break;

                    case "TYPING":
                        bool isTyping = parts.Length > 1 && parts[1] == "1";
                        lock (RemotePlayers)
                        {
                            var rp = GetOrCreateRemote(fromId);
                            rp.IsTyping   = isTyping;
                            rp.TypingTimer = isTyping ? 10f : 0f;
                        }
                        BroadcastExcept(fromId, $"TYPING|{fromId}|{parts[1]}");
                        break;

                    case "CHAT":
                        if (parts.Length > 1)
                        {
                            string sender = GetClientName(fromId);
                            string text   = string.Join("|", parts, 1, parts.Length - 1);
                            AddChat(sender, text);
                            BroadcastExcept(fromId, $"CHAT|{fromId}|{sender}|{text}");
                            // set bubble on server's own remote player entry
                            lock (RemotePlayers)
                            {
                                var rp = GetOrCreateRemote(fromId);
                                rp.ChatBubble  = text;
                                rp.ChatTimer   = 5f;
                                rp.IsTyping    = false;
                                rp.TypingTimer = 0f;
                            }
                        }
                        break;

                    case "BYE":
                        lock (_clientLock)
                        {
                            var cc = _clients.Find(c => c.Id == fromId);
                            if (cc != null) RemoveClient(cc);
                        }
                        break;
                }
                return;
            }

            // ── client receiving from server ───────────────────────────────
            string[] p = raw.Split('|');
            switch (p[0])
            {
                case "WELCOME":
                    _myId = int.Parse(p[1]);
                    StatusText = $"Connected (you are player {_myId})";
                    break;

                case "WORLD":
                    // format: WORLD|id|x|y|hp|facing|scene|name|id|x|y...
                    ParseWorldMessage(p);
                    break;

                case "CHAT":
                    if (p.Length >= 4 && int.TryParse(p[1], out int chatFromId))
                    {
                        string fromName = p[2];
                        string text     = string.Join("|", p, 3, p.Length - 3);
                        AddChat(fromName, text);
                        lock (RemotePlayers)
                        {
                            var rp = RemotePlayers.Find(r => r.Id == chatFromId);
                            if (rp != null)
                            {
                                rp.ChatBubble  = text;
                                rp.ChatTimer   = 5f;
                                rp.IsTyping    = false;
                                rp.TypingTimer = 0f;
                            }
                        }
                    }
                    break;

                case "TYPING":
                    if (p.Length >= 3 && int.TryParse(p[1], out int typingId))
                    {
                        bool typing = p[2] == "1";
                        lock (RemotePlayers)
                        {
                            var rp = RemotePlayers.Find(r => r.Id == typingId);
                            if (rp != null) { rp.IsTyping = typing; rp.TypingTimer = typing ? 10f : 0f; }
                        }
                    }
                    break;

                case "FULL":
                    StatusText       = "Server is full!";
                    IsClient         = _running = _clientConnected = false;
                    break;

                case "BYE":
                    StatusText       = "Server shut down.";
                    IsClient         = _running = _clientConnected = false;
                    RemotePlayers.Clear();
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  HOST — broadcast world state to all clients every send tick
        // ─────────────────────────────────────────────────────────────────────

        private void BroadcastWorldState(string hostName, Player hostPlayer, string hostScene)
        {
            // build WORLD message: all remote players + host itself
            var sb = new StringBuilder("WORLD");

            // host entry (id 0 = host)
            sb.Append($"|0|{hostPlayer.Position.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            sb.Append($"|{hostPlayer.Position.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            sb.Append($"|{hostPlayer.Health}|Down|{hostScene}|{hostName}");

            lock (_clientLock)
                foreach (var cc in _clients)
                {
                    var rp = GetOrCreateRemote(cc.Id);
                    sb.Append($"|{cc.Id}|{rp.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    sb.Append($"|{rp.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    sb.Append($"|{rp.HP}|{rp.Facing}|{rp.Scene}|{cc.Name ?? $"Player{cc.Id}"}");
                }

            string world = sb.ToString();
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, world);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private void ParseWorldMessage(string[] p)
        {
            // WORLD|id|x|y|hp|facing|scene|name|id|x|y|hp|facing|scene|name|...
            // each player is 7 fields: id, x, y, hp, facing, scene, name
            int i = 1;
            while (i + 6 < p.Length)
            {
                int   id     = int.Parse(p[i]);
                float px     = float.Parse(p[i+1], System.Globalization.CultureInfo.InvariantCulture);
                float py     = float.Parse(p[i+2], System.Globalization.CultureInfo.InvariantCulture);
                int   hp     = int.Parse(p[i+3]);
                string facing = p[i+4];
                string scene  = p[i+5];
                string name   = p[i+6];
                i += 7;

                if (id == _myId) continue;   // skip ourselves
                UpdateRemotePlayer(id, px, py, hp, facing, scene, name);
            }
        }

        private void UpdateRemotePlayer(int id, float x, float y, int hp,
                                         string facing, string scene, string name)
        {
            lock (RemotePlayers)
            {
                var rp = RemotePlayers.Find(r => r.Id == id);
                if (rp == null)
                {
                    rp = new RemotePlayer { Id = id };
                    RemotePlayers.Add(rp);
                }
                rp.X        = x;
                rp.Y        = y;
                rp.HP       = hp;
                rp.Facing   = facing;
                rp.Scene    = scene;
                rp.LastSeen = 0f;
                if (!string.IsNullOrEmpty(name)) rp.Name = name;
            }
        }

        private RemotePlayer GetOrCreateRemote(int id)
        {
            lock (RemotePlayers)
            {
                var rp = RemotePlayers.Find(r => r.Id == id);
                if (rp == null) { rp = new RemotePlayer { Id = id }; RemotePlayers.Add(rp); }
                return rp;
            }
        }

        private void TrySendToServer(string msg)
        {
            if (_writer == null) return;
            lock (_writeLock)
            {
                try { _writer.WriteLine(msg); }
                catch { _clientConnected = false; }
            }
        }

        private void TrySendToClient(ConnectedClient cc, string msg)
        {
            lock (cc.WriteLock)
            {
                try { cc.Writer.WriteLine(msg); }
                catch { /* will be cleaned up by read loop */ }
            }
        }

        private void TrySendToId(int id, string msg)
        {
            lock (_clientLock)
            {
                var cc = _clients.Find(c => c.Id == id);
                if (cc != null) TrySendToClient(cc, msg);
            }
        }

        private void BroadcastExcept(int excludeId, string msg)
        {
            lock (_clientLock)
                foreach (var cc in _clients)
                    if (cc.Id != excludeId) TrySendToClient(cc, msg);
        }

        private void AddChat(string name, string text, bool isSystem = false)
        {
            ChatLog.Add(new ChatMessage(name, text, isSystem));
            if (ChatLog.Count > 50) ChatLog.RemoveAt(0);
        }

        private string GetClientName(int id)
        {
            lock (_clientLock)
            {
                var cc = _clients.Find(c => c.Id == id);
                return cc?.Name ?? $"Player{id}";
            }
        }

        private int AllocateId()
        {
            // ids 1–MAX_PLAYERS (0 = host)
            for (int i = 1; i <= MAX_PLAYERS; i++)
                if (!_clients.Exists(c => c.Id == i)) return i;
            return _clients.Count + 1;
        }

        private static string GetLocalIPAddresses()
        {
            var ips = new List<string>();
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork
                            && !IPAddress.IsLoopback(addr.Address))
                            ips.Add(addr.Address.ToString());
                    }
                }
            }
            catch { }
            return ips.Count > 0 ? string.Join(", ", ips) : "127.0.0.1";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CONNECTED CLIENT — server-side wrapper for one TCP connection
    // ─────────────────────────────────────────────────────────────────────────
    internal class ConnectedClient
    {
        public int         Id;
        public string      Name;
        public TcpClient   Tcp;
        public StreamReader Reader;
        public StreamWriter Writer;
        public readonly object WriteLock = new();

        public ConnectedClient(int id, TcpClient tcp)
        {
            Id     = id;
            Tcp    = tcp;
            Reader = new StreamReader(tcp.GetStream(), Encoding.UTF8);
            Writer = new StreamWriter(tcp.GetStream(), Encoding.UTF8) { AutoFlush = true };
        }
    }
}
