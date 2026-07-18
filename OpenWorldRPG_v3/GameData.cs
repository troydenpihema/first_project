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
    public string Shop;
    public string FavoriteGift;
    public bool TalkedToday, GiftedToday, RewardGiven;

    // ── PERSONALITY & PREFERENCES ──
    public string[] Likes = Array.Empty<string>();
    public string[] Dislikes = Array.Empty<string>();
    public string[] Fears = Array.Empty<string>();
    public string Personality = "";
    public string FavoriteFood = "";
    public string Opinion = "";           // their worldview in one line

    // ── RELATIONSHIPS ──
    public string Partner = "";           // name of their partner ("" = single)
    public string[] Children = Array.Empty<string>(); // names of their kids
    public string[] Parents = Array.Empty<string>();  // names of their parents
    public bool IsChild = false;

    // ── TIER MILESTONES ──
    public bool Milestone30 = false;      // Friend tier reached
    public bool Milestone60 = false;      // Close Friend tier reached
    public bool Milestone90 = false;      // Best Friend tier reached

    public string Tier => Friendship >= 90 ? "Best Friend" : Friendship >= 60 ? "Close Friend"
                        : Friendship >= 30 ? "Friend" : "Acquaintance";

    public string TierDialogue => Friendship >= 90 ? "You're family now, mate!"
        : Friendship >= 60 ? "Always good to see you!"
        : Friendship >= 30 ? "Oh hey, it's you again!"
        : "Oh... hello. Do I know you?";

    // gift reaction based on likes/dislikes
    public string GiftReaction(string item)
    {
        if (item == FavoriteGift) return $"{Name} absolutely loves this! \"You remembered!\"";
        if (Likes.Any(l => item.Contains(l))) return $"{Name} likes this. \"Oh nice, cheers!\"";
        if (Dislikes.Any(d => item.Contains(d))) return $"{Name} doesn't like this... \"Uh... thanks?\"";
        return $"{Name} accepts the gift. \"Thanks, I guess.\"";
    }

    public int GiftFriendshipGain(string item)
    {
        if (item == FavoriteGift) return 10;
        if (Likes.Any(l => item.Contains(l))) return 5;
        if (Dislikes.Any(d => item.Contains(d))) return -3;
        return 2;
    }

    // ── FAVOR SYSTEM ──
    public NpcFavor ActiveFavor = null;
    public int FavorCooldownDays = 0;   // days until next favor can roll
    public int FavorsCompleted = 0;
}

class NpcFavor
{
    public string Description;       // "Bring me 5 Fish"
    public string ItemNeeded;        // "Fish", "Iron Ore", etc.
    public int    AmountNeeded;
    public int    AmountDelivered;
    public string RewardType;        // "money", "item", "friendship", "reputation"
    public string RewardItem;        // item name if RewardType == "item"
    public int    RewardAmount;
    public int    FriendshipGain;    // bonus friendship on completion
    public string Dialogue;          // what the NPC says when asking
    public bool   Completed;
}

class DailyChallenge
{
    public string Title;
    public string Category;          // "Combat", "Gathering", "Crafting", etc.
    public Func<int> Progress;       // current cumulative value
    public int Baseline;             // captured at roll time
    public int Target;
    public int Reward;               // money
    public bool Completed;
    public int Current => Math.Clamp(Progress() - Baseline, 0, Target);
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
