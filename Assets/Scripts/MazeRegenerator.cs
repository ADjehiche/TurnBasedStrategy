using UnityEngine;

/// <summary>
/// Simple utility script to regenerate mazes for testing purposes
/// </summary>
public class MazeRegenerator : MonoBehaviour
{
    [SerializeField] private MazeGenerator _mazeGenerator;
    [SerializeField] private KeyCode _regenerateKey = KeyCode.R;
    [SerializeField] private bool _enableKeyboardControl = true;

    private void Update()
    {
        if (_enableKeyboardControl && Input.GetKeyDown(_regenerateKey))
        {
            RegenerateMaze();
        }
    }

    [ContextMenu("Regenerate Maze")]
    public void RegenerateMaze()
    {
        if (_mazeGenerator == null)
        {
            _mazeGenerator = FindObjectOfType<MazeGenerator>();
            if (_mazeGenerator == null)
            {
                Debug.LogError("[MazeRegenerator] No MazeGenerator found in scene!");
                return;
            }
        }

        // Clear existing maze
        ClearExistingMaze();
        
        // Wait a frame then regenerate
        StartCoroutine(RegenerateMazeCoroutine());
    }

    private void ClearExistingMaze()
    {
        // Find and destroy all child objects of the maze generator
        for (int i = _mazeGenerator.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_mazeGenerator.transform.GetChild(i).gameObject);
        }
        
        Debug.Log("[MazeRegenerator] Cleared existing maze");
    }

    private System.Collections.IEnumerator RegenerateMazeCoroutine()
    {
        yield return null; // Wait one frame
        
        // Trigger Start() method again by enabling/disabling the MazeGenerator
        _mazeGenerator.enabled = false;
        yield return null;
        _mazeGenerator.enabled = true;
        
        Debug.Log("[MazeRegenerator] Maze regenerated!");
    }

    private void OnValidate()
    {
        if (_mazeGenerator == null)
        {
            _mazeGenerator = GetComponent<MazeGenerator>();
        }
    }
}