#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using CardGame;

public static class StarterCardGenerator
{
    private const string BasePath = "Assets/Resources/Cards";


    [MenuItem("Tools/Card Game/Generate All Cards")]
    public static void GenerateAllCards()
    {
        // Ensure folder structure
        EnsureFolder(BasePath);
        EnsureFolder(Path.Combine(BasePath, "Attack"));
        EnsureFolder(Path.Combine(BasePath, "Defense"));
        EnsureFolder(Path.Combine(BasePath, "Utility"));
        // Merge feature removed: no MergeOnly folder/cards

        // Generate all cards
        CreateStarterAttackCards();      // 1-11: starter attacks
        CreateStarterDefenseCards();     // 12-17: starter defense
        CreateStarterUtilityCards();     // 18-23: ALL utility (includes Battle Focus & Disarm)
        // Merge feature removed: no merge-only cards

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All starter cards generated!");
    }


    // ---------- helpers ----------

    private static void EnsureFolder(string path)
    {
        path = path.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(path)) return;

        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static Card GetOrCreate(string folderPath, string assetName)
    {
        folderPath = folderPath.Replace("\\", "/");
        var assetPath = Path.Combine(folderPath, assetName + ".asset").Replace("\\", "/");
        var card = AssetDatabase.LoadAssetAtPath<Card>(assetPath);
        if (card == null)
        {
            card = ScriptableObject.CreateInstance<Card>();
            AssetDatabase.CreateAsset(card, assetPath);
        }
        card.name = assetName;
        return card;
    }

    private static void BaseSetup(Card c, string name, string description, CardCategory category, int staminaCost, TargetType target)
    {
        c.cardName = name;
        c.description = description;
        c.category = category;
        c.staminaCost = staminaCost;
        c.targetType = target;
        c.maxUses = -1;
    }

    private static void ClearEffects(Card c, int desiredCount)
    {
        if (c.effects == null)
            c.effects = new System.Collections.Generic.List<CardEffectData>();

        while (c.effects.Count < desiredCount)
            c.effects.Add(new CardEffectData());

        while (c.effects.Count > desiredCount)
            c.effects.RemoveAt(c.effects.Count - 1);
    }

    private static void SetEffect(Card c, int index, EffectType type, int amount, int durationTurns, DamageSchool school = DamageSchool.None, bool applyToSelf = false)
    {
        var e = c.effects[index];
        e.effectType = type;
        e.amount = amount;
        e.durationTurns = durationTurns;
        e.damageSchool = school;
        e.applyToSelf = applyToSelf;
        e.useRandomRange = false;
        e.minAmount = 0;
        e.maxAmount = 0;
    }

    // Finish setup for Common Starter cards (all eligible for starting pool)
    private static void FinishStarterCommon(Card c, params string[] tags)
    {
        c.rarity = CardRarity.Common;
        c.maxCopiesInDeck = 4;
        c.canAppearAsReward = false;
        c.canAppearInStartingDecks = true;
        c.exhaustOnPlay = false;
        c.isStarterCard = true;
        c.unlockedByDefault = true;
        c.unlockLevelRequired = 0;

        if (c.tags == null)
            c.tags = new System.Collections.Generic.List<string>();
        c.tags.Clear();
        foreach (var t in tags)
        {
            if (!string.IsNullOrWhiteSpace(t))
                c.tags.Add(t);
        }

        EditorUtility.SetDirty(c);
    }

    // No longer needed - reward cards merged into starter pool
    // Kept for potential future use
    private static void FinishRewardCard(Card c, CardRarity rarity, int maxCopies, bool exhaust, params string[] tags)
    {
        c.rarity = rarity;
        c.maxCopiesInDeck = maxCopies;
        c.canAppearAsReward = false;
        c.canAppearInStartingDecks = true;
        c.exhaustOnPlay = exhaust;
        c.isStarterCard = true;
        c.unlockedByDefault = true;
        c.unlockLevelRequired = 0;

        if (c.tags == null)
            c.tags = new System.Collections.Generic.List<string>();
        c.tags.Clear();
        foreach (var t in tags)
        {
            if (!string.IsNullOrWhiteSpace(t))
                c.tags.Add(t);
        }

        EditorUtility.SetDirty(c);
    }

    // Merge feature removed: no merge-only cards.

    // ---------- STARTER ATTACK CARDS (1-11) ----------

    private static void CreateStarterAttackCards()
    {
        string folder = Path.Combine(BasePath, "Attack");

        // 1) Quick Slash (renamed from Stash)
        {
            var c = GetOrCreate(folder, "Quick Slash");
            BaseSetup(c, "Quick Slash", "Deal 2 damage.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee");
        }

        // 2) Stab
        {
            var c = GetOrCreate(folder, "Stab");
            BaseSetup(c, "Stab", "Deal 1 damage. Apply Bleed 2.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 2, 2, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "melee", "bleed");
        }

        // 3) Brawler's Jab
        {
            var c = GetOrCreate(folder, "Brawler's Jab");
            BaseSetup(c, "Brawler's Jab", "Deal 1 damage.", CardCategory.Attack, 0, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee", "free");
        }

        // 4) Open-Hand Slap
        {
            var c = GetOrCreate(folder, "Open-Hand Slap");
            BaseSetup(c, "Open-Hand Slap", "Deal 1 damage. Apply Weak 10% for 1 turn.", CardCategory.Attack, 0, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyWeak, 10, 1, DamageSchool.None);
            FinishStarterCommon(c, "attack", "melee", "free", "debuff", "weak");
        }

        // 5) Low Sweep
        {
            var c = GetOrCreate(folder, "Low Sweep");
            BaseSetup(c, "Low Sweep", "Deal 2 damage. Apply Weak 15% for 1 turn.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyWeak, 15, 1, DamageSchool.None);
            FinishStarterCommon(c, "attack", "melee", "debuff", "weak");
        }

        // 6) Improvised Bolt
        {
            var c = GetOrCreate(folder, "Improvised Bolt");
            BaseSetup(c, "Improvised Bolt", "Deal 2 damage. Apply Bleed 1.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 1, 1, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "ranged", "bleed");
        }

        // 7) Crossbow Bolt
        {
            var c = GetOrCreate(folder, "Crossbow Bolt");
            BaseSetup(c, "Crossbow Bolt", "Deal 4 damage.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 4, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "ranged");
        }

        // 8) Lunging Thrust
        {
            var c = GetOrCreate(folder, "Lunging Thrust");
            BaseSetup(c, "Lunging Thrust", "Deal 5 damage.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 5, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee", "heavy");
        }

        // 9) Poison Arrow
        {
            var c = GetOrCreate(folder, "Poison Arrow");
            BaseSetup(c, "Poison Arrow", "Deal 2 damage. Apply Bleed 3. Apply Weak 10% for 2 turns.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 3);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 3, 3, DamageSchool.Bleed);
            SetEffect(c, 2, EffectType.ApplyWeak, 10, 2, DamageSchool.None);
            FinishStarterCommon(c, "attack", "ranged", "bleed", "debuff", "weak");
        }

        // 10) Rend
        {
            var c = GetOrCreate(folder, "Rend");
            BaseSetup(c, "Rend", "Deal 1 damage. Apply Bleed 3.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 3, 3, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "melee", "bleed");
        }

        // 11) Aimed Shot
        {
            var c = GetOrCreate(folder, "Aimed Shot");
            BaseSetup(c, "Aimed Shot", "Deal 2 damage.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "ranged");
        }
    }

    // ---------- STARTER DEFENSE CARDS (12-17) ----------

    private static void CreateStarterDefenseCards()
    {
        string folder = Path.Combine(BasePath, "Defense");

        // 12) Block
        {
            var c = GetOrCreate(folder, "Block");
            BaseSetup(c, "Block", "Gain 4 Block.", CardCategory.Defense, 0, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.ApplyBlock, 4, 0, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block", "free");
        }

        // 13) Parry
        {
            var c = GetOrCreate(folder, "Parry");
            BaseSetup(c, "Parry", "Gain 3 Block. Reflect 2 damage for 1 turn.", CardCategory.Defense, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.ApplyBlock, 3, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.ReflectDamage, 2, 1, DamageSchool.Physical);
            FinishStarterCommon(c, "defense", "block", "reflect");
        }

        // 14) Dodge
        {
            var c = GetOrCreate(folder, "Dodge");
            BaseSetup(c, "Dodge", "Gain 2 Block. Dodge the next incoming attack this turn.", CardCategory.Defense, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.ApplyBlock, 2, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.DodgeNextAttack, 0, 1, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block", "evasion");
        }

        // 15) Shield Block
        {
            var c = GetOrCreate(folder, "Shield Block");
            BaseSetup(c, "Shield Block", "Gain 10 Block.", CardCategory.Defense, 2, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.ApplyBlock, 10, 0, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block");
        }

        // 16) Invisibility
        {
            var c = GetOrCreate(folder, "Invisibility");
            BaseSetup(c, "Invisibility", "Become untargetable for 1 turn. Lose 2 HP.", CardCategory.Defense, 2, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.PreventAttack, 0, 1, DamageSchool.None);
            SetEffect(c, 1, EffectType.Damage, 2, 0, DamageSchool.True, applyToSelf: true);
            FinishStarterCommon(c, "defense", "evasion", "self-damage");
        }

        // 17) Brace
        {
            var c = GetOrCreate(folder, "Brace");
            BaseSetup(c, "Brace", "Gain 6 Block. Gain 1 stamina next turn.", CardCategory.Defense, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.ApplyBlock, 6, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.GainNextTurnStamina, 1, 1, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block", "tempo");
        }
    }

    // ---------- STARTER UTILITY CARDS (18-23) ----------
    // ALL utility cards are available in starting pool

    private static void CreateStarterUtilityCards()
    {
        string folder = Path.Combine(BasePath, "Utility");

        // 18) Quick Draw
        {
            var c = GetOrCreate(folder, "Quick Draw");
            BaseSetup(c, "Quick Draw", "Draw 2 cards.", CardCategory.Utility, 1, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.DrawCards, 2, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "draw");
        }

        // 19) Energize
        {
            var c = GetOrCreate(folder, "Energize");
            BaseSetup(c, "Energize", "Gain 2 stamina. Exhaust.", CardCategory.Utility, 0, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.GainStamina, 2, 0, DamageSchool.None);
            c.exhaustOnPlay = true; // Override for this card
            FinishStarterCommon(c, "utility", "stamina", "free", "exhaust");
            c.maxCopiesInDeck = 2; // Override max copies
        }

        // 20) Heal
        {
            var c = GetOrCreate(folder, "Heal");
            BaseSetup(c, "Heal", "Restore 5 HP.", CardCategory.Utility, 2, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Heal, 5, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "heal");
        }

        // 21) Cleanse
        {
            var c = GetOrCreate(folder, "Cleanse");
            BaseSetup(c, "Cleanse", "Remove all debuffs. Gain 2 Block.", CardCategory.Utility, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.RemoveDebuffs, 0, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.ApplyBlock, 2, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "cleanse", "block");
        }

        // 22) Battle Focus
        {
            var c = GetOrCreate(folder, "Battle Focus");
            BaseSetup(c, "Battle Focus", "Gain +2 damage for 1 turn.", CardCategory.Utility, 1, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.DamageBuff, 2, 1, DamageSchool.None);
            FinishStarterCommon(c, "utility", "buff", "damage");
            c.rarity = CardRarity.Uncommon; // Override rarity
            c.maxCopiesInDeck = 2; // Override max copies
        }

        // 23) Disarm
        {
            var c = GetOrCreate(folder, "Disarm");
            BaseSetup(c, "Disarm", "Enemies cannot attack for 1 turn. Exhaust.", CardCategory.Utility, 2, TargetType.AllEnemies);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.PreventAttack, 0, 1, DamageSchool.None);
            c.exhaustOnPlay = true; // Override for this card
            FinishStarterCommon(c, "utility", "control", "exhaust");
            c.rarity = CardRarity.Rare; // Override rarity
            c.maxCopiesInDeck = 1; // Override max copies
        }
    }

    // Merge feature removed: no merge-only cards.
}
#endif
