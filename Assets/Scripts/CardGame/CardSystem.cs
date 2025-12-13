using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame
{
  
    [CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card")]
    public class Card : ScriptableObject
    {
        [Header("Identity")]
        public string cardName;
        public Sprite artwork;
        [TextArea] public string description;

        [Header("Gameplay")]
        public CardCategory category;      // Attack / Defense / Utility / Tactical
        public int staminaCost = 1;
        public TargetType targetType;      // Who this card can target

        [Header("Effects")]
        public List<CardEffectData> effects = new List<CardEffectData>();

        [Header("Usage Limits")]
        [Tooltip("-1 = infinite uses; any positive value makes the card disappear after that many plays.")]
        public int maxUses = -1;

        [Header("Deckbuilding & Rewards")]
        public CardRarity rarity = CardRarity.Common;

        [Tooltip("Maximum number of copies of this card allowed in a single deck.")]
        public int maxCopiesInDeck = 1;
        
        [Tooltip("If false, this card will never be offered as a post‑combat reward.")]
        public bool canAppearAsReward = true;

        [Tooltip("If false, this card will never appear in starting decks.")]
        public bool canAppearInStartingDecks = true;

        [Tooltip("If true, this card is exhausted (removed from the combat deck) when played.")]
        public bool exhaustOnPlay = false;

        [Tooltip("If true, this card is part of at least one starter deck.")]
        public bool isStarterCard = false;

        [Tooltip("If false, this card must be unlocked before it can be used or offered.")]
        public bool unlockedByDefault = true;

        [Tooltip("Level or progression value required before this card can be unlocked.")]
        public int unlockLevelRequired = 0;

        [Tooltip("Tags used for synergies and filtering (e.g. 'bleed', 'ranged', 'defense').")]
        public List<string> tags = new List<string>();

        [Header("Leveling")]
        [Tooltip("Per‑level configuration. Runtime instances track current level and uses.")]
        public List<CardLevelData> levels = new List<CardLevelData>();
    }

    // High-level type, for logic & UI filters
    public enum CardCategory
    {
        Attack,
        Defense,
        Utility,
        Tactical
    }

    // For targeting system
    public enum TargetType
    {
        Self,
        SingleEnemy,
        AllEnemies,

        SingleAlly,
        AllAllies,
        None        // e.g. global buffs, draw cards, etc.
    }

    // Card rarity for rewards and shops
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }


    // What an effect actually DOES
    public enum EffectType
    {
        Damage,             // direct damage
        ApplyBleed,         // bleed over time
        ApplyBlock,         // add shield / block
        Heal,               // restore HP
        ApplyWeak,          // reduce damage dealt
        GainStamina,        // +stamina now
        GainNextTurnStamina,// +stamina next turn
        DrawCards,          // draw extra cards
        PreventAttack,      // enemy can't act (Invisibility)
        DodgeNextAttack,    // next incoming attack = 0
        RevealIntent,       // show enemy damage for N turns
        DamageBuff,         // increase our damage
        DamageReduction,    // flat/percent reduction on enemy damage
        RemoveDebuffs,     // remove negative status effects

        ReflectDamage,            // reflect damage back to attacker

    }

    // For damage types / resistances
    public enum DamageSchool
    {
        Physical,
        Bleed,
        Poison,
        Magic,
        True,
        None
    }

    [Serializable]
    public class CardEffectData
    {
        public EffectType effectType;

        [Tooltip("Main value (damage, block, heal, etc.)")]
        public int amount = 0;

        [Tooltip("For over-time effects like bleed/weak. 0 = instant.")]
        public int durationTurns = 0;

        [Tooltip("For damage / bleed / poison etc.")]
        public DamageSchool damageSchool = DamageSchool.None;

        [Tooltip("Does this effect apply to the card user instead of the target?")]
        public bool applyToSelf = false;

        // If you still want min/max damage rolls:
        public bool useRandomRange = false;
        public int minAmount = 0;
        public int maxAmount = 0;
    }

    [Serializable]
    public class CardLevelData
    {
        [Tooltip("Uses required to reach this level from the previous one.")]
        public int requiredUses = 0;

        [Tooltip("Effects applied when the card is at this level. If empty, the base card effects are used.")]
        public List<CardEffectData> effects = new List<CardEffectData>();

        [TextArea]
        public string levelDescription;
    }
}