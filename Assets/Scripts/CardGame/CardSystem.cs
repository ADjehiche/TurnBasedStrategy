using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace CardGame
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;

        public CardType cardType;

        [FormerlySerializedAs("health")]
        public int staminaCost;


        //for future use
        //public TargetMode targetMode = TargetMode.SingleEnemy;

        public int damageMin;
        public int damageMax;
        public DamageType damageType;

        public enum CardType
        {
            melee,
            ranged,
            spell,
            defense,

        }
        public enum DamageType
        {
            melee,
            ranged,
            spell,
            defense,
        }

        // for future use
        // public enum TargetMode
        // {
        //     SingleEnemy,
        //     AllEnemies,
        //     Self,
        //     Ally,
        //     Area,
        // }

    }
}