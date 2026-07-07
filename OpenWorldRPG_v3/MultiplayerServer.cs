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
        private Player _drawPlayer;
        private float _lastX, _lastY;
        private bool  _hasLastPos = false;
        public string HairStyle        = "";
        public string FacialHair       = "None";
        public string EquippedWeapon   = "";
        public bool   IsTwoHanded      = false;
        public float  ChatTimer  = 0f;
        public bool  IsTyping   = false;
        public float TypingTimer = 0f;
        public bool PendingSwing = false;
        public string MountType = "";   // "" = not riding/driving anything
        public bool   IsVehicle  = false; // true = vehicle, false = rideable

        private Rideable _ghostRideable;
        private Vehicle  _ghostVehicle;
        public Color  SkinColor       = Color.Beige;
        public Color  HairColor       = new Color((byte)80,(byte)50,(byte)20,(byte)255);
        public Color  FacialHairColor = new Color((byte)80,(byte)50,(byte)20,(byte)255);
        public string ArmorHelmet = "";
        public string ArmorBody   = "";
        public string ArmorLegs   = "";
        public string ArmorBoots  = "";
        public string ArmorGloves = "";
        public string ArmorCape   = "";
        public string ArmorShield = "";
        public Color ShirtColor = Color.Blue;
        public Color PantsColor = Color.Black;
        public string HeldItem  = "";

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
            DrawAt("World");
        }

        public void DrawAt(string requiredSceneTag)
        {
            if (!Active || Scene != requiredSceneTag) return;

            // ── GHOST MOUNT (no collision, visual only) ───────────────────────────
            if (!string.IsNullOrEmpty(MountType))
            {
                if (IsVehicle)
                {
                    if (System.Enum.TryParse<Vehicle.VehicleType>(MountType, out var vType))
                        {
                            if (_ghostVehicle == null) _ghostVehicle = new Vehicle(new Vector2(X, Y), Color.Gray, 0f, vType);
                            _ghostVehicle.Position = new Vector2(X, Y);
                            _ghostVehicle.Type = vType;
                            _ghostVehicle.Facing = Facing switch
                            {
                                "Up"    => Vehicle.FacingDirection.Up,
                                "Left"  => Vehicle.FacingDirection.Left,
                                "Right" => Vehicle.FacingDirection.Right,
                                _       => Vehicle.FacingDirection.Down
                            };
                            _ghostVehicle.Driving = true; // some vehicle draw art may gate exhaust/animation on this
                            _ghostVehicle.Draw();
                        }
                }
                else
                {
                    if (System.Enum.TryParse<Rideable.RideableType>(MountType, out var rType))
                    {
                        if (_ghostRideable == null) _ghostRideable = new Rideable(new Vector2(X, Y), rType, Color.White);
                        _ghostRideable.Position = new Vector2(X, Y);
                        _ghostRideable.Type     = rType;
                        _ghostRideable.Riding   = true; // forces the rider silhouette to draw
                        _ghostRideable.RiderSkinOverride  = SkinColor;
                        _ghostRideable.RiderShirtOverride = ShirtColor;
                        _ghostRideable.RiderPantsOverride = PantsColor;
                        _ghostRideable.Facing = Facing switch
                        {
                            "Up"    => Rideable.FacingDirection.Up,
                            "Left"  => Rideable.FacingDirection.Left,
                            "Right" => Rideable.FacingDirection.Right,
                            _       => Rideable.FacingDirection.Down
                        };
                        _ghostRideable.Draw();
                    }
                }
            }

            // lazily create a throwaway Player instance to reuse all existing armor/draw code
            if (_drawPlayer == null) _drawPlayer = new Player(new Vector2(X, Y));

            _drawPlayer.Position   = new Vector2(X, Y);
            _drawPlayer.SkinColor  = SkinColor;
            _drawPlayer.ShirtColor = ShirtColor;
            _drawPlayer.PantsColor = PantsColor;
            _drawPlayer.UseHeldItemOverride = true;
            _drawPlayer.HeldItemOverride = string.IsNullOrEmpty(HeldItem) ? null : HeldItem;
            if (PendingSwing)
            {
                _drawPlayer.TriggerSwing();
                PendingSwing = false;
            }
            _drawPlayer.Facing     = Facing switch
            {
                "Up"    => Player.FacingDirection.Up,
                "Left"  => Player.FacingDirection.Left,
                "Right" => Player.FacingDirection.Right,
                _       => Player.FacingDirection.Down
            };
            
            _drawPlayer.Hidden   = !string.IsNullOrEmpty(MountType);

            // derive movement from position delta since last draw call (proxy for isMoving)
            bool moving = false;
            if (_hasLastPos)
            {
                float dx = X - _lastX;
                float dy = Y - _lastY;
                moving = (dx * dx + dy * dy) > 0.25f; // moved more than ~0.5px since last frame
            }
            _lastX = X; _lastY = Y; _hasLastPos = true;

            _drawPlayer.DriveAnimation(moving, Raylib.GetFrameTime());
            _drawPlayer.TickSwing(Raylib.GetFrameTime());

            // swap the global armor/hair statics to this remote player's gear, draw, then restore
            string savedHelmet = Program.armorHelmet, savedBody = Program.armorBody, savedLegs = Program.armorLegs,
                   savedBoots  = Program.armorBoots,  savedGloves = Program.armorGloves, savedCape = Program.armorCape,
                   savedShield = Program.armorShield, savedWeapon = Program.armorWeapon, savedHairStyle = Program.playerHairStyle;
            string savedFacialHair = Program.playerFacialHair;
            Color  savedHairColor  = Program.playerHairColor, savedFacialColor = Program.playerFacialHairColor;

            Program.armorHelmet = string.IsNullOrEmpty(ArmorHelmet) ? null : ArmorHelmet;
            Program.armorBody   = string.IsNullOrEmpty(ArmorBody)   ? null : ArmorBody;
            Program.armorLegs   = string.IsNullOrEmpty(ArmorLegs)   ? null : ArmorLegs;
            Program.armorBoots  = string.IsNullOrEmpty(ArmorBoots)  ? null : ArmorBoots;
            Program.armorGloves = string.IsNullOrEmpty(ArmorGloves) ? null : ArmorGloves;
            Program.armorCape   = string.IsNullOrEmpty(ArmorCape)   ? null : ArmorCape;
            Program.armorShield = string.IsNullOrEmpty(ArmorShield) ? null : ArmorShield;
            Program.armorWeapon = string.IsNullOrEmpty(EquippedWeapon) ? null : EquippedWeapon;
            Program.playerHairStyle = HairStyle;
            Program.playerFacialHair = FacialHair;
            Program.playerHairColor = HairColor;
            Program.playerFacialHairColor = FacialHairColor;

            _drawPlayer.Draw();

            Program.armorHelmet = savedHelmet; Program.armorBody = savedBody; Program.armorLegs = savedLegs;
            Program.armorBoots  = savedBoots;  Program.armorGloves = savedGloves; Program.armorCape = savedCape;
            Program.armorShield = savedShield; Program.armorWeapon = savedWeapon; Program.playerHairStyle = savedHairStyle;
            Program.playerFacialHair = savedFacialHair;
            Program.playerHairColor = savedHairColor; Program.playerFacialHairColor = savedFacialColor;

            // ── name tag, chat bubble, HP bar ──
            int px = (int)X;
            int py = (int)Y;

            int nameW = Program.MeasureTextUI(Name, 14);
            Raylib.DrawRectangle(px + 20 - nameW / 2 - 4, py - 22, nameW + 8, 18,
                new Color((byte)0,(byte)0,(byte)0,(byte)160));
            Program.DrawTextUI(Name, px + 20 - nameW / 2, py - 21, 14,
                new Color((byte)80,(byte)240,(byte)220,(byte)255));

            if (ChatTimer   > 0f) ChatTimer   -= Raylib.GetFrameTime();
            if (TypingTimer > 0f) TypingTimer -= Raylib.GetFrameTime();

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
                int  tw2      = Program.MeasureTextUI(bubbleText, fontSize);
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
                Program.DrawTextUI(display, bx2 + 8, by2 + 7, fontSize, dotCol);
            }

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
        public int    MyId      => IsHost ? 0 : _myId;
        public string StatusText { get; private set; } = "";
        public List<RemotePlayer> RemotePlayers { get; } = new List<RemotePlayer>();
        public List<ChatMessage>  ChatLog       { get; } = new List<ChatMessage>();

        // ── config ────────────────────────────────────────────────────────────
        public const int  PORT        = 9999;
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
        public Action<string> CardStateReceived;

        public void BroadcastCardTableState(string serializedState)
        {
            if (!IsHost) return;
            string msg = $"CARDSTATE|{serializedState}";
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, msg);
        }

        public void SendOwnHandTo(int targetId, int seat, string serializedHand)
        {
            if (!IsHost) return;
            TrySendToId(targetId, $"YOURHAND|{seat}|{serializedHand}");
        }
        public void SendCardAward(int targetId, int xp, bool won, int gameType)
                {
                    TrySendToId(targetId, $"CARDAWARD|{xp}|{(won ? 1 : 0)}|{gameType}");
                }

        public Action<int, bool, int> CardAwardReceived; // (xp, won, gameType)
        public Action<int, string> OwnHandReceived; // (seat, serializedHand)

        /// <summary>Client sends a requested action (bid, play card, etc) for their own seat.</summary>
        public Action<int, string> CardActionReceived; // (fromId, action)

        public void SendCardAction(string action)
        {
            if (!Connected || IsHost) return; // host applies its own actions directly, no need to "send" to itself
            TrySendToServer($"CARDACTION|{action}");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Host a game on the local network. Call from your Host button.</summary>
        /// 
        /// <summary>
        /// Called by ANY client (or the host itself, for consistency) when their local
        /// hit-detection lands a blow on a world boss. The host applies real damage;
        /// non-host clients send this purely as a request and never apply damage locally.
        /// </summary>
        public void SendBossHit(bool isSuperBoss, int damage, int killerId)
        {
            if (!Connected) return;
            string payload = $"BOSSHIT|{(isSuperBoss ? "1" : "0")}|{damage}|{killerId}";

            if (IsHost)
            {
                // host is authoritative — apply immediately via the callback
                BossHitReceived?.Invoke(isSuperBoss, damage, killerId);
            }
            else
            {
                TrySendToServer(payload);
            }
        }

        /// <summary>Host subscribes to this to apply real damage when any client (including itself) reports a hit.</summary>
        public Action<bool, int, int> BossHitReceived;

        /// <summary>Host calls this after applying damage, to broadcast the new authoritative state to all clients.</summary>
        public void BroadcastBossState(bool isSuperBoss, float health, float maxHealth, bool dead, float posX, float posY)
        {
            if (!IsHost) return;
            string msg = $"BOSSSTATE|{(isSuperBoss ? "1" : "0")}" +
                        $"|{health.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                        $"|{maxHealth.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                        $"|{(dead ? "1" : "0")}" +
                        $"|{posX.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                        $"|{posY.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, msg);
        }
        public void SendEnemyProjectile(float startX, float startY, float velX, float velY, float life, string kind, int damage)
        {
            if (!Connected || !IsHost) return;   // only the host fires enemy projectiles
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            string payload = $"EPROJ|{startX.ToString(ci)}|{startY.ToString(ci)}" +
                             $"|{velX.ToString(ci)}|{velY.ToString(ci)}" +
                             $"|{life.ToString(ci)}|{kind}|{damage.ToString(ci)}";
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, payload);
        }
        public void BroadcastWorldClock(float timeOfDay, int dayOfWeek, int dayOfMonth, int month, bool raining)
        {
            if (!IsHost) return;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            string msg = $"CLOCK|{timeOfDay.ToString(ci)}|{dayOfWeek}|{dayOfMonth}|{month}|{(raining ? "1" : "0")}";
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, msg);
        }
        public Action<float, int, int, int, bool> WorldClockReceived;

        public void BroadcastEnemyState(int id, float posX, float posY, int health, bool dead, bool aggro)
        {
            if (!IsHost) return;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            string msg = $"ENEMYSTATE|{id}|{posX.ToString(ci)}|{posY.ToString(ci)}" +
                        $"|{health}|{(dead ? "1" : "0")}|{(aggro ? "1" : "0")}";
            lock (_clientLock)
                foreach (var cc in _clients)
                    TrySendToClient(cc, msg);
        }
        public Action<int, float, float, int, bool, bool> EnemyStateReceived;

        // NEW: a client tells the host it hit an enemy (host applies the damage).
        public void SendEnemyHit(int id, int damage, int killerId)
        {
            if (!Connected || IsHost) return;   // host applies its own hits directly
            TrySendToServer($"ENEMYHIT|{id}|{damage}|{killerId}");
        }
        public Action<int, int, int> EnemyHitReceived;
        public void SendEnemyKill(int targetId, string enemyType)
        {
            if (!IsHost) return;
            TrySendToId(targetId, $"ENEMYKILL|{enemyType}");
        }
        public Action<string> EnemyKillReceived;
        public void SendBossKillReward(int targetId, bool isSuper)
        {
            if (!IsHost) return;
            TrySendToId(targetId, $"BOSSKILL|{(isSuper ? "1" : "0")}");
        }
        public Action<bool> BossKillRewardReceived;

        
        public Action<bool, float, float, bool, float, float> BossStateReceived;
        public void StartHost()
        {
            if (_running) Stop();
            IsHost  = true;
            _running = true;
            StatusText = $"Hosting on port {PORT}…";

            try
            {
                _listener = new TcpListener(IPAddress.IPv6Any, PORT);
                _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
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
                string facing = player.Facing.ToString();
                string payload = $"PLAYER|{player.Position.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"|{player.Position.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"|{player.Health}|{facing}|{scene}|{playerName}";

                if (IsHost)
                    BroadcastWorldState(playerName, player, scene);
                else
                    TrySendToServer(payload);
            }
        }

        public void SendAppearance(Color skin, Color hair, Color facialHair, Color shirt, Color pants,
            string helmet, string body, string legs, string boots, string gloves, string cape, string shield,
            string heldItem, string hairStyle, string facialHairStyle, string weapon, bool isTwoHanded)
        {
            if (!Connected) return;
            string payload = $"APPEAR|{skin.R}|{skin.G}|{skin.B}" +
                            $"|{hair.R}|{hair.G}|{hair.B}" +
                            $"|{facialHair.R}|{facialHair.G}|{facialHair.B}" +
                            $"|{shirt.R}|{shirt.G}|{shirt.B}" +
                            $"|{pants.R}|{pants.G}|{pants.B}" +
                            $"|{helmet}|{body}|{legs}|{boots}|{gloves}|{cape}|{shield}|{heldItem}" +
                            $"|{hairStyle}|{facialHairStyle}|{weapon}|{(isTwoHanded ? "1" : "0")}";

            if (IsHost)
            {
                lock (_clientLock)
                    foreach (var cc in _clients)
                        TrySendToClient(cc, $"APPEAR|0|{payload.Substring(7)}");
            }
            else
            {
                TrySendToServer(payload);
            }
        }

public void SendMount(string mountType, bool isVehicle)
{
    if (!Connected) return;
    string payload = $"MOUNT|{mountType}|{(isVehicle ? "1" : "0")}";

    if (IsHost)
    {
        lock (_clientLock)
            foreach (var cc in _clients)
                TrySendToClient(cc, $"MOUNT|0|{mountType}|{(isVehicle ? "1" : "0")}");
    }
    else
    {
        TrySendToServer(payload);
    }
}

 public void SendSwing()
        {
            if (!Connected) return;

            if (IsHost)
            {
                lock (_clientLock)
                    foreach (var cc in _clients)
                        TrySendToClient(cc, "SWING|0");
            }
            else
            {
                TrySendToServer("SWING");
            }
        } 

public void SendLootDrop(float x, float y, string itemType, int ownerId)
{
    if (!IsHost) return;
    var ci = System.Globalization.CultureInfo.InvariantCulture;
    string msg = $"LOOTDROP|{x.ToString(ci)}|{y.ToString(ci)}|{itemType}|{ownerId}";
    lock (_clientLock)
        foreach (var cc in _clients)
            TrySendToClient(cc, msg);
}
public Action<float, float, string, int> LootDropReceived;

// NEW: a client tells the host it picked up a drop so the host removes it for everyone
public void SendLootPickup(float x, float y, string itemType)
{
    if (!Connected || IsHost) return;
    var ci = System.Globalization.CultureInfo.InvariantCulture;
    TrySendToServer($"LOOTPICK|{x.ToString(ci)}|{y.ToString(ci)}|{itemType}");
}
public Action<float, float, string> LootPickupReceived; 

public void SendProjectile(float startX, float startY, float velX, float velY, float life, string kind, bool isSpell)
{
    if (!Connected) return;
    string spellFlag = isSpell ? "1" : "0";
    string payload = $"PROJ|{startX.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"|{startY.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"|{velX.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"|{velY.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"|{life.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"|{kind}|{spellFlag}";

    if (IsHost)
    {
        lock (_clientLock)
            foreach (var cc in _clients)
                TrySendToClient(cc, payload);
    }
    else
    {
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

            int w = Program.MeasureTextUI(StatusText, 14) + 16;
            Raylib.DrawRectangle(1280 - w - 4, 4, w, 22, new Color((byte)0, (byte)0, (byte)0, (byte)160));
            Raylib.DrawRectangleLines(1280 - w - 4, 4, w, 22, col);
            Program.DrawTextUI(StatusText, 1280 - w + 4, 8, 14, col);

            if (Connected)
            {
                int count;
                lock (_clientLock) count = IsHost ? _clients.Count + 1 : RemotePlayers.Count + 1;
                string players = $"{count}/{MAX_PLAYERS} players";
                int pw = Program.MeasureTextUI(players, 12);
                Program.DrawTextUI(players, 1280 - pw - 8, 28, 12,
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
                int w = Program.MeasureTextUI(line, 15) + 12;
                Raylib.DrawRectangle(4, y - 2, w, 19, new Color((byte)0, (byte)0, (byte)0, (byte)(120 * fade)));
                Program.DrawTextUI(line, 8, y, 15,
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

                    case "SWING":
                        BroadcastExcept(fromId, $"SWING|{fromId}");
                        lock (RemotePlayers)
                        {
                            var rp = GetOrCreateRemote(fromId);
                            rp.PendingSwing = true;
                        }
                        break;

                    case "PROJ":
                        {
                            string rest = string.Join("|", parts, 1, parts.Length - 1);
                            BroadcastExcept(fromId, $"PROJ|{rest}");
                        }
                        break;

                    case "ENEMYHIT":
                        if (parts.Length >= 4 &&
                            int.TryParse(parts[1], out int ehId) &&
                            int.TryParse(parts[2], out int ehDmg) &&
                            int.TryParse(parts[3], out int ehKiller))
                        {
                            EnemyHitReceived?.Invoke(ehId, ehDmg, ehKiller);
                        }
                        break;

                    case "LOOTPICK":
                        if (parts.Length >= 4 &&
                            float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float lpX) &&
                            float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float lpY))
                        {
                            LootPickupReceived?.Invoke(lpX, lpY, parts[3]);
                        }
                        break;

                    case "MOUNT":
                        {
                            string rest = string.Join("|", parts, 1, parts.Length - 1);
                            BroadcastExcept(fromId, $"MOUNT|{fromId}|{rest}");
                            var rpMount = GetOrCreateRemote(fromId);
                            rpMount.MountType = parts.Length > 1 ? parts[1] : "";
                            rpMount.IsVehicle  = parts.Length > 2 && parts[2] == "1";
                        }
                        break;

                    case "APPEAR":
                        {
                            string rest = string.Join("|", parts, 1, parts.Length - 1);
                            BroadcastExcept(fromId, $"APPEAR|{fromId}|{rest}");
                            ApplyAppearance(GetOrCreateRemote(fromId), parts);
                        }
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

                    case "BOSSHIT":
                        if (parts.Length >= 4 &&                          
                            int.TryParse(parts[2], out int hitDmg) &&
                            int.TryParse(parts[3], out int hitKiller))    
                        {
                            bool isSuper = parts[1] == "1";
                            BossHitReceived?.Invoke(isSuper, hitDmg, hitKiller);   
                        }
                        break;

                    case "CARDACTION":
                        {
                            string action = string.Join("|", parts, 1, parts.Length - 1);
                            CardActionReceived?.Invoke(fromId, action);
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

                case "SWING":
                    if (p.Length >= 2 && int.TryParse(p[1], out int swingId))
                    {
                        lock (RemotePlayers)
                        {
                            var rp = RemotePlayers.Find(r => r.Id == swingId);
                            if (rp != null) rp.PendingSwing = true;
                        }
                    }
                    break;

                case "PROJ":
                    if (p.Length >= 8 &&
                        float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float px2) &&
                        float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float py2) &&
                        float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pvx) &&
                        float.TryParse(p[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pvy) &&
                        float.TryParse(p[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float plife))
                    {
                        Program.SpawnRemoteVisualProjectile(px2, py2, pvx, pvy, plife, p[6], p[7] == "1");
                    }
                    break;

                case "MOUNT":
                    if (p.Length >= 4 && int.TryParse(p[1], out int mountId))
                    {
                        var rp = GetOrCreateRemote(mountId);
                        rp.MountType = p[2];
                        rp.IsVehicle  = p[3] == "1";
                    }
                    break;

                case "APPEAR":
                    if (p.Length >= 17 && int.TryParse(p[1], out int appearId))
                    {
                        var rp = GetOrCreateRemote(appearId);
                        ApplyAppearance(rp, p, 1);
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

                case "BOSSSTATE":
                    if (p.Length >= 7 &&
                        float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bHealth) &&
                        float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bMaxHealth) &&
                        float.TryParse(p[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bPosX) &&
                        float.TryParse(p[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bPosY))
                    {
                        bool isSuper = p[1] == "1";
                        bool isDead  = p[4] == "1";
                        BossStateReceived?.Invoke(isSuper, bHealth, bMaxHealth, isDead, bPosX, bPosY);
                    }
                    break;

                case "ENEMYSTATE":
                    if (p.Length >= 7 &&
                        int.TryParse(p[1], out int esId) &&
                        float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float esX) &&
                        float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float esY) &&
                        int.TryParse(p[4], out int esHp))
                    {
                        EnemyStateReceived?.Invoke(esId, esX, esY, esHp, p[5] == "1", p[6] == "1");
                    }
                    break;

                case "CLOCK":
                    if (p.Length >= 6 &&
                        float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ctod) &&
                        int.TryParse(p[2], out int cdow) && int.TryParse(p[3], out int cdom) && int.TryParse(p[4], out int cmon))
                    {
                        WorldClockReceived?.Invoke(ctod, cdow, cdom, cmon, p[5] == "1");
                    }
                    break;

                case "EPROJ":
                    if (p.Length >= 8 &&
                        float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float epx) &&
                        float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float epy) &&
                        float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float epvx) &&
                        float.TryParse(p[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float epvy) &&
                        float.TryParse(p[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float eplife) &&
                        int.TryParse(p[7], out int epdmg))
                    {
                        Program.SpawnNetworkEnemyProjectile(epx, epy, epvx, epvy, eplife, p[6], epdmg);
                    }
                    break;

            case "ENEMYKILL":
                    if (p.Length >= 2)
                        EnemyKillReceived?.Invoke(p[1]);
                    break;

            case "BOSSKILL":
                    if (p.Length >= 2)
                        BossKillRewardReceived?.Invoke(p[1] == "1");
                    break;

            case "LOOTDROP":
                    if (p.Length >= 5 &&
                        float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ldX) &&
                        float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ldY) &&
                        int.TryParse(p[4], out int ldOwner))
                    {
                        LootDropReceived?.Invoke(ldX, ldY, p[3], ldOwner);
                    }
                    break;

                case "CARDSTATE":
                    {
                        string state = string.Join("|", p, 1, p.Length - 1);
                        CardStateReceived?.Invoke(state);
                    }
                    break;
                
                case "YOURHAND":
                    if (p.Length >= 3 && int.TryParse(p[1], out int handSeat))
                    {
                        string handData = p[2];
                        OwnHandReceived?.Invoke(handSeat, handData);
                    }
                    break;
                
                case "CARDAWARD":
                    if (p.Length >= 4 && int.TryParse(p[1], out int awXp)
                        && int.TryParse(p[3], out int awGame))
                    {
                        bool awWon = p[2] == "1";
                        CardAwardReceived?.Invoke(awXp, awWon, awGame);
                    }
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
            sb.Append($"|{hostPlayer.Health}|{hostPlayer.Facing}|{hostScene}|{hostName}");;

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

        private void ApplyAppearance(RemotePlayer rp, string[] parts, int offset = 0)
        {
            int i = offset == 0 ? 1 : 2;
            try
            {
                rp.SkinColor       = new Color(byte.Parse(parts[i]),    byte.Parse(parts[i+1]),  byte.Parse(parts[i+2]),  (byte)255);
                rp.HairColor       = new Color(byte.Parse(parts[i+3]),  byte.Parse(parts[i+4]),  byte.Parse(parts[i+5]),  (byte)255);
                rp.FacialHairColor = new Color(byte.Parse(parts[i+6]),  byte.Parse(parts[i+7]),  byte.Parse(parts[i+8]),  (byte)255);
                rp.ShirtColor      = new Color(byte.Parse(parts[i+9]),  byte.Parse(parts[i+10]), byte.Parse(parts[i+11]), (byte)255);
                rp.PantsColor      = new Color(byte.Parse(parts[i+12]), byte.Parse(parts[i+13]), byte.Parse(parts[i+14]), (byte)255);
                rp.ArmorHelmet = parts[i+15];
                rp.ArmorBody   = parts[i+16];
                rp.ArmorLegs   = parts[i+17];
                rp.ArmorBoots  = parts[i+18];
                rp.ArmorGloves = parts[i+19];
                rp.ArmorCape   = parts[i+20];
                rp.ArmorShield = parts[i+21];
                rp.HeldItem    = parts.Length > i+22 ? parts[i+22] : "";
                rp.HairStyle   = parts.Length > i+23 ? parts[i+23] : "";
                rp.FacialHair  = parts.Length > i+24 ? parts[i+24] : "None";
                rp.EquippedWeapon = parts.Length > i+25 ? parts[i+25] : "";
                rp.IsTwoHanded    = parts.Length > i+26 && parts[i+26] == "1";
            }
            catch { /* malformed packet, ignore */ }
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
