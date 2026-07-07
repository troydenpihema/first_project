using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace OpenWorldRPG
{
    partial class Program
    {
        static void GenerateNZPlaceCardSet(CardSet set)
        {
            set.Pool.Clear();
            for (int i = 0; i < nzPlaceNames.Length; i++)
            {
                CardRarity r = i switch { < 10 => CardRarity.Common, < 20 => CardRarity.Uncommon, < 27 => CardRarity.Rare, _ => CardRarity.Holo };
                set.Pool.Add(new TradingCard { Name = nzPlaceNames[i], Dex = i + 1, Rarity = r });
            }
            string[] nzUltra = { "Milford Sound", "Fiordland", "Aoraki / Mount Cook", "Cathedral Cove", "Hobbiton" };
            foreach (var n in nzUltra) set.Pool.Add(new TradingCard { Name = n, Dex = 0, Rarity = CardRarity.UltraRare });
            set.Pool.Add(new TradingCard { Name = "Cape Reinga — Secret Rare", Dex = 0, Rarity = CardRarity.SecretRare });
        }
        static void GenerateCarCardSet(CardSet set)
        {
            set.Pool.Clear();
            for (int i = 0; i < carNames.Length; i++)
            {
                CardRarity r = i switch { < 10 => CardRarity.Common, < 20 => CardRarity.Uncommon, < 27 => CardRarity.Rare, _ => CardRarity.Holo };
                set.Pool.Add(new TradingCard { Name = carNames[i], Dex = i + 1, Rarity = r });
            }
            string[] carUltra = { "Bugatti Chiron", "Lamborghini Aventador", "Ferrari LaFerrari", "McLaren P1", "Koenigsegg Jesko" };
            foreach (var n in carUltra) set.Pool.Add(new TradingCard { Name = n, Dex = 0, Rarity = CardRarity.UltraRare });
            set.Pool.Add(new TradingCard { Name = "Prototype X — Secret Rare", Dex = 0, Rarity = CardRarity.SecretRare });
        }
        static void BuildDBZBinderSlots()
        {
            dbzBinderSlots.Clear();
            var baseCards = dbzCards.Where(c => c.Rarity != CardRarity.UltraRare && c.Rarity != CardRarity.SecretRare);
            foreach (var c in baseCards) dbzBinderSlots.Add(new BinderSlot { Card = c, IsReverse = false });
            foreach (var c in baseCards.Where(c => c.Dex > 0)) dbzBinderSlots.Add(new BinderSlot { Card = c, IsReverse = true });
            foreach (var c in dbzCards.Where(c => c.Rarity == CardRarity.UltraRare)) dbzBinderSlots.Add(new BinderSlot { Card = c, IsReverse = false });
            var secret = dbzCards.FirstOrDefault(c => c.Rarity == CardRarity.SecretRare);
            if (secret != null) dbzBinderSlots.Add(new BinderSlot { Card = secret, IsReverse = false });
        }
        static void GenerateDBZCardSet()   
        {
            dbzCards.Clear();
            int dex = 1;
            for (int ci = 0; ci < crewCharacters.Length; ci++)
            {
                var (name, titles) = crewCharacters[ci];
                for (int t = 0; t < 12; t++)
                {
                    CardRarity r = t < 5 ? CardRarity.Common : t < 8 ? CardRarity.Uncommon
                                 : t < 10 ? CardRarity.Rare : t == 10 ? CardRarity.UltraRare : CardRarity.SecretRare;
                    int power = r switch
                    {
                        CardRarity.Common    => 200 + t * 150 + ci * 10,        // ~200–1000
                        CardRarity.Uncommon  => 1500 + (t - 5) * 400 + ci * 25, // ~1500–2800
                        CardRarity.Rare      => 3500 + (t - 8) * 1500 + ci * 50,// ~3500–5900
                        CardRarity.UltraRare => 9001 + ci * 100,                // had to
                        _                    => 25000 + ci * 500
                    };
                    dbzCards.Add(new TradingCard {
                        Name = $"{name} {titles[t]}",
                        Dex = (r == CardRarity.UltraRare || r == CardRarity.SecretRare) ? 0 : dex++,
                        Rarity = r, Power = power, Franchise = CardFranchise.DragonBallZ
                    });
                }
            }
        }
        static void GenerateCardSet()
        {
            allCards.Clear();
            for (int i = 0; i < pokemonNames.Length; i++)
            {
                CardRarity r = (i + 1) switch
                {
                    6 or 9 or 12 or 25 or 59 or 94 or 130 or 131 or 143 or 144 or 145 or 146 or 149 or 150 or 151 => CardRarity.Holo,
                    <= 20 => CardRarity.Common,
                    <= 60 => CardRarity.Uncommon,
                    _ => CardRarity.Rare
                };
                allCards.Add(new TradingCard { Name = pokemonNames[i], Dex = i + 1, Rarity = r });
            }
            // trainer + energy filler cards, not tied to a Pokémon
            string[] trainers = { "Professor Oak", "Bill", "Potion", "Pokédex", "Super Energy Removal" };
            string[] energies = { "Grass Energy", "Fire Energy", "Water Energy", "Lightning Energy", "Psychic Energy", "Fighting Energy" };
            foreach (var t in trainers) allCards.Add(new TradingCard { Name = t, Dex = 0, Rarity = CardRarity.Trainer });
            foreach (var e in energies) allCards.Add(new TradingCard { Name = e, Dex = 0, Rarity = CardRarity.Energy });

            string[] ultraRareNames = {
        "Charizard GX","Blastoise GX","Venusaur GX","Pikachu VMAX","Mewtwo EX","Mew EX","Gyarados EX",
        "Dragonite EX","Snorlax EX","Gengar EX","Alakazam EX","Machamp EX","Lapras EX","Articuno EX",
        "Zapdos EX","Moltres EX","Arcanine EX","Nidoking EX","Rapidash EX","Golem EX"
            };
            foreach (var name in ultraRareNames)
                allCards.Add(new TradingCard { Name = name, Dex = 0, Rarity = CardRarity.UltraRare });

            allCards.Add(new TradingCard { Name = "Mew  Secret", Dex = 151, Rarity = CardRarity.SecretRare });
        }
        static CardBinder personalBinder = new();
        static CardBinder masterSetBinder = new();
static void DrawWrappedCardText(string text, int x, int y, int maxW, int fontSize, Color col)
    {
        string line = "";
        foreach (var w in text.Split(' '))
        {
            string test = line.Length == 0 ? w : line + " " + w;
            if (Program.MeasureTextUI(test, fontSize) > maxW && line.Length > 0)
            { Program.DrawTextUI(line, x, y, fontSize, col); y += fontSize + 3; line = w; }
            else line = test;
        }
        if (line.Length > 0) Program.DrawTextUI(line, x, y, fontSize, col);
    }
        
static List<TradingCard> OpenPack(CardSet set)
{
    var pack = new List<TradingCard>();
    var commons   = set.Pool.Where(c => c.Rarity == CardRarity.Common).ToList();
    var uncommons = set.Pool.Where(c => c.Rarity == CardRarity.Uncommon).ToList();
    var rares     = set.Pool.Where(c => c.Rarity == CardRarity.Rare || c.Rarity == CardRarity.Holo).ToList();
    var ultras    = set.Pool.Where(c => c.Rarity == CardRarity.UltraRare).ToList();
    var secrets   = set.Pool.Where(c => c.Rarity == CardRarity.SecretRare).ToList();

    TradingCard Clone(List<TradingCard> pool, bool allowReverse = true)
    {
        if (pool.Count == 0) pool = commons.Count > 0 ? commons : set.Pool;   // safety for thin sets
        var c = pool[Raylib.GetRandomValue(0, pool.Count - 1)];
        bool rev = allowReverse && c.Dex > 0 && Raylib.GetRandomValue(1, 3) == 1;
        return new TradingCard { Name = c.Name, Dex = c.Dex, Rarity = c.Rarity, Power = c.Power, ReverseHolo = rev };
    }

    // ── GOD PACK — 1 in 1000 per pack, any set ──
    lastPackWasGod = Raylib.GetRandomValue(1, 1000) == 1;
    if (lastPackWasGod)
    {
        pack.Add(Clone(commons));
        pack.Add(Clone(uncommons));
        pack.Add(Clone(rares));
        pack.Add(Clone(ultras, false));
        pack.Add(Raylib.GetRandomValue(1, 5) == 1 && secrets.Count > 0
            ? Clone(secrets, false)
            : Clone(ultras, false));                     // missed the 1-in-20 → defaults to ultra rare
        return pack;
    }

    // ── NORMAL PACK — 3 commons, 1 uncommon, then the chase slot ──
    for (int i = 0; i < 3; i++) pack.Add(Clone(commons));
    pack.Add(Clone(uncommons));

    if      (Raylib.GetRandomValue(1, 50) == 1 && secrets.Count > 0) pack.Add(Clone(secrets, false));
    else if (Raylib.GetRandomValue(1, 10)   == 1 && ultras.Count  > 0) pack.Add(Clone(ultras, false));
    else if (Raylib.GetRandomValue(1, 5)   == 1 && rares.Count   > 0) pack.Add(Clone(rares));
    else pack.Add(Clone(Raylib.GetRandomValue(1, 2) == 1 ? commons : uncommons));   // missed rare → common/uncommon
    return pack;
}
    static void InitAllCardSets()
    {
        cardSets.Clear();

        var pokemon = new CardSet { SetName = "Pokemon", CoverTitle = "151", CoverColor = new Color((byte)40,(byte)30,(byte)70,(byte)255) };
        GenerateCardSet(); pokemon.Pool = allCards; BuildSlotsFor(pokemon);
        pokemon.MasterSet = masterSetBinder;   
        pokemon.Personal = personalBinder;    
        cardSets.Add(pokemon);

        var dbz = new CardSet { SetName = "DragonBallZ", CoverTitle = "THE CREW", CoverColor = new Color((byte)70,(byte)25,(byte)20,(byte)255) };
        GenerateDBZCardSet(); dbz.Pool = dbzCards; BuildSlotsFor(dbz);
        cardSets.Add(dbz);

        var cars = new CardSet { SetName = "Cars", CoverTitle = "TOP GEAR", CoverColor = new Color((byte)20,(byte)40,(byte)70,(byte)255) };
        GenerateCarCardSet(cars); BuildSlotsFor(cars);
        cardSets.Add(cars);

        var nz = new CardSet { SetName = "NZPlaces", CoverTitle = "AOTEAROA", CoverColor = new Color((byte)20,(byte)60,(byte)45,(byte)255) };
        GenerateNZPlaceCardSet(nz); BuildSlotsFor(nz);
        cardSets.Add(nz);
    }
        static bool hobbiesShopOpen = false;
        static int hobbiesShopSetIndex = 0;
        static List<TradingCard> lastPackOpened = null;
        static float packRevealTimer = 0f;
        static bool placementModeActive = false;
        static string placementCardName = null;
        static bool placementIsReverse = false;
        static int hoveredCardSlot = -1;
        static (int slot, string cardName, bool isReverse) cardPopupOpen = (-1, null, false);
        static void BuildSlotsFor(CardSet set)
        {
            set.Slots.Clear();
            var baseCards = set.Pool.Where(c => c.Rarity != CardRarity.UltraRare && c.Rarity != CardRarity.SecretRare);
            foreach (var c in baseCards) set.Slots.Add(new BinderSlot { Card = c, IsReverse = false });
            foreach (var c in baseCards.Where(c => c.Dex > 0)) set.Slots.Add(new BinderSlot { Card = c, IsReverse = true });
            foreach (var c in set.Pool.Where(c => c.Rarity == CardRarity.UltraRare)) set.Slots.Add(new BinderSlot { Card = c, IsReverse = false });
            foreach (var c in set.Pool.Where(c => c.Rarity == CardRarity.SecretRare))
                set.Slots.Add(new BinderSlot { Card = c, IsReverse = false });
        }
        static void BuildBinderSlots()
        {
            binderSlots.Clear();
            // 1. normal cards: 151 Pokémon + 5 Trainers + 6 Energy = 162, in original generation order
            var baseCards = allCards.Where(c => c.Rarity != CardRarity.UltraRare && c.Rarity != CardRarity.SecretRare);
            foreach (var c in baseCards) binderSlots.Add(new BinderSlot { Card = c, IsReverse = false });

            // 2. reverse holo variants — Pokémon only (Dex > 0), 151 slots
            foreach (var c in baseCards.Where(c => c.Dex > 0)) binderSlots.Add(new BinderSlot { Card = c, IsReverse = true });

            // 3. Ultra Rares (20), then Secret Rare (1) — at the very end
            foreach (var c in allCards.Where(c => c.Rarity == CardRarity.UltraRare)) binderSlots.Add(new BinderSlot { Card = c, IsReverse = false });
            var secret = allCards.FirstOrDefault(c => c.Rarity == CardRarity.SecretRare);
            if (secret != null) binderSlots.Add(new BinderSlot { Card = secret, IsReverse = false });
        }
    }
}
