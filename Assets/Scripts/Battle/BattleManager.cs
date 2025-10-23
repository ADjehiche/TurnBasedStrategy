using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeReturn = 1.5f;
    
    private void Awake()
    {
        BattleState.Reset();
    }
    
    private void OnEnable()
    {
        BattleState.OnBattleOverChanged += HandleBattleStateChanged;
    }

    private void OnDisable()
    {
        BattleState.OnBattleOverChanged -= HandleBattleStateChanged;
    }

    private void HandleBattleStateChanged(bool isOver)
    {
        if (isOver)
        {
            Debug.Log("Battle ended - returning to LevelOne after delay");
            Invoke("ReturnToLevelOne", delayBeforeReturn);
        }
    }
    
    private void ReturnToLevelOne()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToLevelOne();
        }
        else
        {
            Debug.LogWarning("GameManager instance not found, using direct scene loading");
            SceneManager.LoadScene("LevelOne");
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}