using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
class Job
{
    public string Title, Employer, Resource;
    public int Target, Pay;
    public bool CompletedToday;
}
class SideTask
{
    public string Title;
    public Func<int> Progress; public int Target, Baseline, Pay;
    public string DeliverTo;   // "Building:NAME" or "NPC:Name"
    public bool Accepted, ReadyToDeliver, DoneToday;
    public string DeliverLabel => DeliverTo.StartsWith("NPC:")
        ? "Deliver to " + DeliverTo[4..] : "Deliver to the " + DeliverTo[9..];
}
class FriendNPC
{
    public NPC Npc;
    public string Name;
    public int Friendship; 
    public string Shop;                 // 0–100
    public string FavoriteGift;            // "Fish", "Logs", "Bones", "Fur"
    public bool TalkedToday, GiftedToday, RewardGiven;
    public string Tier => Friendship >= 90 ? "Best Friend" : Friendship >= 60 ? "Close Friend"
                        : Friendship >= 30 ? "Friend" : "Acquaintance";
    public string TierDialogue => Friendship >= 90 ? "You're family now, mate!"
        : Friendship >= 60 ? "Always good to see you!"
        : Friendship >= 30 ? "Oh hey, it's you again!"
        : "Oh... hello. Do I know you?";
}
class LootEntry { public string Item; public int Min, Max; public int Weight; }
class ArenaOrb { public Vector2 Pos, Vel; public float Life = 6f; }
class ArenaSpike { public Vector2 Pos; public float Telegraph = 1.2f; public float ActiveTime = 0.6f; public bool Fired = false; }
class ArenaMinion { public Vector2 Pos; public float Health = 40; public float Speed = 110f; }
class QuestStage
{
    public string Description;
    public Func<int> Progress;   // cumulative counter; null = "return to giver/spot" stage
    public int Target;
    public int Baseline;         // captured when the stage begins
}
class StoryQuest
{
    public string Title;
    public string GiverName;     // "" = no NPC giver
    public Vector2 TriggerSpot;  // Vector2.Zero = no spot trigger
    public List<QuestStage> Stages = new();
    public int Stage = 0;
    public bool Started, Completed;
    public int Reward;
    public QuestStage Current => Stage < Stages.Count ? Stages[Stage] : null;
}
}
