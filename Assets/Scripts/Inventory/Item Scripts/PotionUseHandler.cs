using System.Collections;
using UnityEngine;

public class PotionUseHandler : MonoBehaviour
{
    public void UsePotion(InventoryItemData item)
    {
        if (item == null) return;
        switch (item.potionEffectType)
        {
            case PotionEffectType.Speed:
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.ApplySpeedBoost(item.speedMultiplier > 0 ? item.speedMultiplier : 1.2f, item.effectDuration > 0 ? item.effectDuration : 180f);
                }
                break;
            case PotionEffectType.Stamina:
                if (PlayerStamina.Instance != null)
                {
                    PlayerStamina.Instance.maxStamina += (item.staminaIncrease > 0 ? item.staminaIncrease : 1);
                    PlayerStamina.Instance.Refill();
                }
                break;
            default:
                Debug.Log("[PotionUseHandler] No effect for this item.");
                break;
        }
        // Remove from inventory after use (implement as needed)
    }
}