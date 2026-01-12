#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using CardGame;

public static class StarterCardGenerator
{
    private const string BasePath = "Assets/Resources/Cards";

    [MenuItem("Tools/Card Game/Generate Starter Cards")]
    public static void GenerateStarterCards()
    {
        // Ensure folder structure
        EnsureFolder(BasePath);
        EnsureFolder(Path.Combine(BasePath, "Attack"));
        EnsureFolder(Path.Combine(BasePath, "Defense"));
        EnsureFolder(Path.Combine(BasePath, "Utility"));
        EnsureFolder(Path.Combine(BasePath, "Tactical"));

        CreateOrUpdateAttackCards();
        CreateOrUpdateDefenseCards();
        CreateOrUpdateUtilityCards();
        CreateOrUpdateTacticalCards();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Starter cards generated/updated.");
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

    private static void FinishStarterCommon(Card c, params string[] tags)
    {
        c.rarity = CardRarity.Common;
        c.maxCopiesInDeck = 4;
        c.canAppearAsReward = true;
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

    // ---------- ATTACK CARDS ----------

    private static void CreateOrUpdateAttackCards()
    {
        string folder = Path.Combine(BasePath, "Attack");

        // Slash
        {
            var c = GetOrCreate(folder, "Slash");
            BaseSetup(c, "Slash", "Deal 2 damage.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee");
        }

        // Stab
        {
            var c = GetOrCreate(folder, "Stab");
            BaseSetup(c, "Stab", "Deal 1 damage. Apply 2 Bleed.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 2, 2, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "bleed", "melee");
        }

        // Punch
        {
            var c = GetOrCreate(folder, "Punch");
            BaseSetup(c, "Punch", "Deal 1 damage.", CardCategory.Attack, 0, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee", "free");
        }

        // Kick
        {
            var c = GetOrCreate(folder, "Kick");
            BaseSetup(c, "Kick", "Deal 2 damage.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee");
        }

        // Throw Arrow
        {
            var c = GetOrCreate(folder, "Throw Arrow");
            BaseSetup(c, "Throw Arrow", "Deal 1 damage. Apply 1 Bleed.", CardCategory.Attack, 1, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 1, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 1, 2, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "ranged", "bleed");
        }

        // Crossbow Bolt
        {
            var c = GetOrCreate(folder, "Crossbow Bolt");
            BaseSetup(c, "Crossbow Bolt", "Deal 3 damage. Apply 2 Bleed.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 3, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 2, 2, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "ranged", "bleed");
        }

        // Lunging Attack
        {
            var c = GetOrCreate(folder, "Lunging Attack");
            BaseSetup(c, "Lunging Attack", "Deal 5 damage.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Damage, 5, 0, DamageSchool.Physical);
            FinishStarterCommon(c, "attack", "melee", "heavy");
        }

        // Slash and Stab
        {
            var c = GetOrCreate(folder, "Slash and Stab");
            BaseSetup(c, "Slash and Stab", "Deal 3 damage. Apply 3 Bleed.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.Damage, 3, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 3, 2, DamageSchool.Bleed);
            FinishStarterCommon(c, "attack", "bleed", "combo");
        }

        // Poison Arrow
        {
            var c = GetOrCreate(folder, "Poison Arrow");
            BaseSetup(c, "Poison Arrow", "Deal 2 damage. Apply 2 Bleed. Apply Weak (15%) for 2 turns.", CardCategory.Attack, 2, TargetType.SingleEnemy);
            ClearEffects(c, 3);
            SetEffect(c, 0, EffectType.Damage, 2, 0, DamageSchool.Physical);
            SetEffect(c, 1, EffectType.ApplyBleed, 2, 3, DamageSchool.Bleed);
            SetEffect(c, 2, EffectType.ApplyWeak, 15, 2, DamageSchool.None);
            FinishStarterCommon(c, "attack", "ranged", "bleed", "debuff");
        }
    }

    // ---------- DEFENSE CARDS ----------

    private static void CreateOrUpdateDefenseCards()
    {
        string folder = Path.Combine(BasePath, "Defense");

        // Block
        {
            var c = GetOrCreate(folder, "Block");
            BaseSetup(c, "Block", "Gain 3 Block.", CardCategory.Defense, 0, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.ApplyBlock, 3, 0, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block", "free");
        }

        // Invisibility
        {
            var c = GetOrCreate(folder, "Invisibility");
            BaseSetup(c, "Invisibility", "Become untargetable for 1 turn. Lose 2 HP.", CardCategory.Defense, 2, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.PreventAttack, 0, 1, DamageSchool.None);
            SetEffect(c, 1, EffectType.Damage, 2, 0, DamageSchool.True, applyToSelf: true);
            FinishStarterCommon(c, "defense", "evasion", "self-damage");
        }

        // Shield Block
        {
            var c = GetOrCreate(folder, "Shield Block");
            BaseSetup(c, "Shield Block", "Gain 10 Block.", CardCategory.Defense, 2, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.ApplyBlock, 10, 0, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block");
        }

        // Parry
        {
            var c = GetOrCreate(folder, "Parry");
            BaseSetup(c, "Parry", "Gain 3 Block. Reflect 1 damage for 1 turn.", CardCategory.Defense, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.ApplyBlock, 3, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.ReflectDamage, 1, 1, DamageSchool.Physical);
            FinishStarterCommon(c, "defense", "block", "reflect");
        }

        // Dodge
        {
            var c = GetOrCreate(folder, "Dodge");
            BaseSetup(c, "Dodge", "Gain 2 Block. Become untargetable for 1 turn.", CardCategory.Defense, 1, TargetType.Self);
            ClearEffects(c, 2);
            SetEffect(c, 0, EffectType.ApplyBlock, 2, 0, DamageSchool.None);
            SetEffect(c, 1, EffectType.PreventAttack, 0, 1, DamageSchool.None);
            FinishStarterCommon(c, "defense", "block", "evasion");
        }
    }

    // ---------- UTILITY CARDS ----------

    private static void CreateOrUpdateUtilityCards()
    {
        string folder = Path.Combine(BasePath, "Utility");

        // Heal
        {
            var c = GetOrCreate(folder, "Heal");
            BaseSetup(c, "Heal", "Restore 5 HP.", CardCategory.Utility, 2, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.Heal, 5, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "heal");
        }

        // Energize
        {
            var c = GetOrCreate(folder, "Energize");
            BaseSetup(c, "Energize", "Restore 3 stamina.", CardCategory.Utility, 0, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.GainStamina, 3, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "stamina", "free");
        }

        // Remove Debuff
        {
            var c = GetOrCreate(folder, "Remove Debuff");
            BaseSetup(c, "Remove Debuff", "Remove all debuffs.", CardCategory.Utility, 1, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.RemoveDebuffs, 0, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "cleanse");
        }

        // Draw Card
        {
            var c = GetOrCreate(folder, "Draw Card");
            BaseSetup(c, "Draw Card", "Draw 1 card.", CardCategory.Utility, 1, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.DrawCards, 1, 0, DamageSchool.None);
            FinishStarterCommon(c, "utility", "draw");
        }
    }

    // ---------- TACTICAL CARDS ----------

    private static void CreateOrUpdateTacticalCards()
    {
        string folder = Path.Combine(BasePath, "Tactical");

        // Inspire
        {
            var c = GetOrCreate(folder, "Inspire");
            BaseSetup(c, "Inspire", "Increase allies' damage by 2 for 2 turns.", CardCategory.Tactical, 2, TargetType.AllAllies);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.DamageBuff, 2, 2, DamageSchool.None);
            FinishStarterCommon(c, "tactical", "buff");
        }

        // Disarm
        {
            var c = GetOrCreate(folder, "Disarm");
            BaseSetup(c, "Disarm", "Enemies cannot attack for 1 turn.", CardCategory.Tactical, 2, TargetType.AllEnemies);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.PreventAttack, 0, 1, DamageSchool.None);
            FinishStarterCommon(c, "tactical", "debuff");
        }

        // Scout
        {
            var c = GetOrCreate(folder, "Scout");
            BaseSetup(c, "Scout", "Reveal enemy intents for 2 turns.", CardCategory.Tactical, 1, TargetType.Self);
            ClearEffects(c, 1);
            SetEffect(c, 0, EffectType.RevealIntent, 0, 2, DamageSchool.None);
            FinishStarterCommon(c, "tactical", "utility");
        }
    }
}
#endif
