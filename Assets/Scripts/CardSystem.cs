using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CardGame
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public CardType cardType;
        public int health;

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

    }
}