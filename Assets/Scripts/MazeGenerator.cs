using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private MazeCell _mazeCellPrefab;
    [SerializeField] private int _mazeWidth = 10;
    [SerializeField] private int _mazeDepth = 10;
    [SerializeField] private float _cellSize = 3f; // Spacing between cells - adjust based on your bookshelf size
    
    [Header("Maze Position")]
    [Tooltip("The maze will generate at this GameObject's position. Move this object to position the maze!")]
    [SerializeField] private bool _useTransformAsOrigin = true; // Use this GameObject's position as maze origin
    [SerializeField] private bool _rotateWallsToFaceInward = true; // Auto-rotate bookcases to face inward
    
    [Header("Editor Preview")]
    [SerializeField] private bool _showPreviewInEditor = true;
    [SerializeField] private Color _previewColor = new Color(0, 1, 0, 0.3f); // Green transparent
    [SerializeField] private Color _entranceColor = new Color(1, 1, 0, 0.5f); // Yellow for entrance
    
    [Header("Entrance Settings")]
    [SerializeField] private bool _createFixedEntrance = true;
    [SerializeField] private int _entranceX = 0; // X position of entrance (0 = left side)
    [SerializeField] private int _entranceZ = 0; // Z position of entrance (0 = back side)
    [SerializeField] private EntranceSide _entranceSide = EntranceSide.Back; // Which wall to remove for entrance
    
    [Header("Spawnable Objects")]
    [SerializeField] private GameObject _lightPrefab;
    [SerializeField] private GameObject _blobPrefab;
    [SerializeField] private GameObject _chestPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] [Range(0f, 1f)] private float _lightSpawnChance = 0.3f; // 30% chance per wall
    [SerializeField] private float _lightHeight = 2f; // Height to place lights on walls
    [SerializeField] private int _numberOfChests = 2;
    
    public enum EntranceSide { Left, Right, Front, Back }
    
    private MazeCell[,] _mazeGrid;
    private List<Vector2Int> _deadEndCells = new List<Vector2Int>(); // Track dead ends for blob/chest spawning
    
    // Get the maze origin position
    private Vector3 MazeOrigin => _useTransformAsOrigin ? transform.position : Vector3.zero;
    
    // Get the maze rotation
    private Quaternion MazeRotation => _useTransformAsOrigin ? transform.rotation : Quaternion.identity;
    
    /// <summary>
    /// Convert local grid position to world position (accounting for origin and rotation)
    /// </summary>
    private Vector3 GridToWorld(float x, float z, float y = 0f)
    {
        Vector3 localPos = new Vector3(x * _cellSize, y, z * _cellSize);
        return MazeOrigin + MazeRotation * localPos;
    }
    
    /// <summary>
    /// Convert world position back to grid coordinates
    /// </summary>
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // Remove origin offset and reverse rotation
        Vector3 localPos = Quaternion.Inverse(MazeRotation) * (worldPos - MazeOrigin);
        int x = Mathf.RoundToInt(localPos.x / _cellSize);
        int z = Mathf.RoundToInt(localPos.z / _cellSize);
        return new Vector2Int(x, z);
    }
    
    void Start()
    {
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                // Calculate cell position with rotation
                Vector3 cellPosition = GridToWorld(x, z);
                MazeCell cell = Instantiate(_mazeCellPrefab, cellPosition, MazeRotation, transform);
                cell.GridX = x;  // Store grid position on the cell
                cell.GridZ = z;
                _mazeGrid[x, z] = cell;
            }
        }
        GenerateMaze(null, _mazeGrid[0,0]);
        
        // Rotate all walls to face inward after maze is generated
        if (_rotateWallsToFaceInward)
        {
            RotateAllWallsInward();
        }
        
        // Create fixed entrance
        if (_createFixedEntrance)
        {
            CreateEntrance();
        }
        
        // After maze is generated, find dead ends and spawn objects
        FindDeadEnds();
        SpawnLightsOnWalls();
        SpawnBlobInHiddenLocation();
        SpawnChests();
    }
    
    /// <summary>
    /// Rotate all bookcase walls to face inward toward the cell center
    /// </summary>
    private void RotateAllWallsInward()
    {
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                _mazeGrid[x, z].RotateAllWallsToFaceInward();
            }
        }
        Debug.Log("[MazeGenerator] All walls rotated to face inward");
    }
    
    /// <summary>
    /// Create a fixed entrance at the specified location
    /// </summary>
    private void CreateEntrance()
    {
        // Clamp entrance position to valid range
        int entranceX = Mathf.Clamp(_entranceX, 0, _mazeWidth - 1);
        int entranceZ = Mathf.Clamp(_entranceZ, 0, _mazeDepth - 1);
        
        MazeCell entranceCell = _mazeGrid[entranceX, entranceZ];
        
        // Clear the appropriate wall based on entrance side
        switch (_entranceSide)
        {
            case EntranceSide.Left:
                entranceCell.ClearLeftWall();
                break;
            case EntranceSide.Right:
                entranceCell.ClearRightWall();
                break;
            case EntranceSide.Front:
                entranceCell.ClearFrontWall();
                break;
            case EntranceSide.Back:
                entranceCell.ClearBackWall();
                break;
        }
        
        Debug.Log($"[MazeGenerator] Entrance created at ({entranceX}, {entranceZ}) on {_entranceSide} side");
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);


        MazeCell nextCell;
        do
        {
            
            nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell != null)
            {
                GenerateMaze(currentCell, nextCell);
            }
        }while(nextCell!=null);


    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedcells = GetUnvisitedCells(currentCell);

        return unvisitedcells.OrderBy(_=> Random.Range(1,10)).FirstOrDefault();
    }
    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        // Use stored grid coordinates (no world position conversion needed)
        int x = currentCell.GridX;
        int z = currentCell.GridZ;

        if (x+1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x+1,z];

            if(cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }
        if (x-1 >= 0)
        {
            var cellToLeft = _mazeGrid[x-1,z];
            if(cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }

        }
        if (z+1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x,z+1];
            if(cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }

        }
        if (z-1 >= 0)
        {
            var cellToBack = _mazeGrid[x,z-1];
            if(cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }

        }


    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }
        
        // Use grid coordinates to determine which walls to clear
        int dx = currentCell.GridX - previousCell.GridX;
        int dz = currentCell.GridZ - previousCell.GridZ;
        
        // Moving right (+X in grid)
        if (dx == 1)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }
        // Moving left (-X in grid)
        if (dx == -1)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }
        // Moving forward (+Z in grid)
        if (dz == 1)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }
        // Moving backward (-Z in grid)
        if (dz == -1)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }
    
    /// <summary>
    /// Find all dead-end cells (cells with only one opening) - good spots for hidden items
    /// </summary>
    private void FindDeadEnds()
    {
        _deadEndCells.Clear();
        
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                MazeCell cell = _mazeGrid[x, z];
                int openings = CountOpenings(cell);
                
                // Dead end = only 1 opening (excluding the starting cell at 0,0)
                if (openings == 1 && !(x == 0 && z == 0))
                {
                    _deadEndCells.Add(new Vector2Int(x, z));
                }
            }
        }
        
        // Sort by distance from start (0,0) - furthest dead ends are more hidden
        _deadEndCells = _deadEndCells.OrderByDescending(c => c.x + c.y).ToList();
        
        Debug.Log($"[MazeGenerator] Found {_deadEndCells.Count} dead ends");
    }
    
    /// <summary>
    /// Count how many walls are open (cleared) in a cell
    /// </summary>
    private int CountOpenings(MazeCell cell)
    {
        // Count walls that are NOT active (cleared = opening)
        int openings = 4 - cell.GetActiveWallCount();
        return openings;
    }
    
    /// <summary>
    /// Spawn lights randomly on walls throughout the maze
    /// </summary>
    private void SpawnLightsOnWalls()
    {
        if (_lightPrefab == null) return;
        
        int lightsSpawned = 0;
        
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                MazeCell cell = _mazeGrid[x, z];
                Vector3 cellPos = cell.transform.position;
                
                // Randomly place lights on ACTIVE walls (walls that exist)
                float halfCell = _cellSize / 2f;
                
                // Right wall (+X) - only if wall exists
                if (cell.HasRightWall && Random.value < _lightSpawnChance)
                {
                    Vector3 lightPos = cellPos + new Vector3(halfCell - 0.1f, _lightHeight, 0);
                    Quaternion lightRot = Quaternion.Euler(0, -90, 0); // Face inward (toward cell center)
                    Instantiate(_lightPrefab, lightPos, lightRot, cell.transform);
                    lightsSpawned++;
                }
                
                // Left wall (-X) - only if wall exists
                if (cell.HasLeftWall && Random.value < _lightSpawnChance)
                {
                    Vector3 lightPos = cellPos + new Vector3(-halfCell + 0.1f, _lightHeight, 0);
                    Quaternion lightRot = Quaternion.Euler(0, 90, 0); // Face inward
                    Instantiate(_lightPrefab, lightPos, lightRot, cell.transform);
                    lightsSpawned++;
                }
                
                // Front wall (+Z) - only if wall exists
                if (cell.HasFrontWall && Random.value < _lightSpawnChance)
                {
                    Vector3 lightPos = cellPos + new Vector3(0, _lightHeight, halfCell - 0.1f);
                    Quaternion lightRot = Quaternion.Euler(0, 180, 0); // Face inward
                    Instantiate(_lightPrefab, lightPos, lightRot, cell.transform);
                    lightsSpawned++;
                }
                
                // Back wall (-Z) - only if wall exists
                if (cell.HasBackWall && Random.value < _lightSpawnChance)
                {
                    Vector3 lightPos = cellPos + new Vector3(0, _lightHeight, -halfCell + 0.1f);
                    Quaternion lightRot = Quaternion.Euler(0, 0, 0); // Face inward
                    Instantiate(_lightPrefab, lightPos, lightRot, cell.transform);
                    lightsSpawned++;
                }
            }
        }
        
        Debug.Log($"[MazeGenerator] {lightsSpawned} lights spawned on walls");
    }
    
    /// <summary>
    /// Spawn the blob in the most hidden location (furthest dead end from start)
    /// </summary>
    private void SpawnBlobInHiddenLocation()
    {
        if (_blobPrefab == null) return;
        
        if (_deadEndCells.Count == 0)
        {
            Debug.LogWarning("[MazeGenerator] No dead ends found for blob spawn!");
            return;
        }
        
        // Pick the furthest dead end (first in sorted list)
        Vector2Int blobCell = _deadEndCells[0];
        Vector3 spawnPos = GridToWorld(blobCell.x, blobCell.y, 0.5f);
        
        Instantiate(_blobPrefab, spawnPos, MazeRotation);
        
        // Remove this cell from available spots
        _deadEndCells.RemoveAt(0);
        
        Debug.Log($"[MazeGenerator] Blob spawned at dead end ({blobCell.x}, {blobCell.y})");
    }
    
    /// <summary>
    /// Spawn chests in dead ends or corners
    /// </summary>
    private void SpawnChests()
    {
        if (_chestPrefab == null) return;
        
        int chestsToSpawn = Mathf.Min(_numberOfChests, _deadEndCells.Count);
        
        for (int i = 0; i < chestsToSpawn; i++)
        {
            if (_deadEndCells.Count == 0) break;
            
            // Pick a random dead end from remaining ones
            int randomIndex = Random.Range(0, _deadEndCells.Count);
            Vector2Int chestCell = _deadEndCells[randomIndex];
            Vector3 spawnPos = GridToWorld(chestCell.x, chestCell.y, 0.1f);
            
            Instantiate(_chestPrefab, spawnPos, MazeRotation);
            
            // Remove this cell from available spots
            _deadEndCells.RemoveAt(randomIndex);
            
            Debug.Log($"[MazeGenerator] Chest {i + 1} spawned at dead end ({chestCell.x}, {chestCell.y})");
        }
    }
    
    /// <summary>
    /// Draw preview of maze bounds in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showPreviewInEditor) return;
        
        Vector3 origin = _useTransformAsOrigin ? transform.position : Vector3.zero;
        Quaternion rotation = _useTransformAsOrigin ? transform.rotation : Quaternion.identity;
        float mazeWidthWorld = _mazeWidth * _cellSize;
        float mazeDepthWorld = _mazeDepth * _cellSize;
        
        // Helper to convert local to world with rotation
        System.Func<Vector3, Vector3> toWorld = (local) => origin + rotation * local;
        
        // Draw maze boundary corners
        Gizmos.color = _previewColor;
        
        Vector3 corner00 = toWorld(new Vector3(-_cellSize / 2f, 0.5f, -_cellSize / 2f));
        Vector3 corner10 = toWorld(new Vector3(mazeWidthWorld - _cellSize / 2f, 0.5f, -_cellSize / 2f));
        Vector3 corner01 = toWorld(new Vector3(-_cellSize / 2f, 0.5f, mazeDepthWorld - _cellSize / 2f));
        Vector3 corner11 = toWorld(new Vector3(mazeWidthWorld - _cellSize / 2f, 0.5f, mazeDepthWorld - _cellSize / 2f));
        
        // Draw wire outline
        Gizmos.color = Color.green;
        Gizmos.DrawLine(corner00, corner10);
        Gizmos.DrawLine(corner10, corner11);
        Gizmos.DrawLine(corner11, corner01);
        Gizmos.DrawLine(corner01, corner00);
        
        // Draw grid lines for cells
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        for (int x = 0; x <= _mazeWidth; x++)
        {
            Vector3 start = toWorld(new Vector3(x * _cellSize - _cellSize / 2f, 0.5f, -_cellSize / 2f));
            Vector3 end = toWorld(new Vector3(x * _cellSize - _cellSize / 2f, 0.5f, mazeDepthWorld - _cellSize / 2f));
            Gizmos.DrawLine(start, end);
        }
        for (int z = 0; z <= _mazeDepth; z++)
        {
            Vector3 start = toWorld(new Vector3(-_cellSize / 2f, 0.5f, z * _cellSize - _cellSize / 2f));
            Vector3 end = toWorld(new Vector3(mazeWidthWorld - _cellSize / 2f, 0.5f, z * _cellSize - _cellSize / 2f));
            Gizmos.DrawLine(start, end);
        }
        
        // Draw entrance location
        if (_createFixedEntrance)
        {
            Gizmos.color = _entranceColor;
            int entranceX = Mathf.Clamp(_entranceX, 0, _mazeWidth - 1);
            int entranceZ = Mathf.Clamp(_entranceZ, 0, _mazeDepth - 1);
            Vector3 entrancePos = toWorld(new Vector3(entranceX * _cellSize, 0.5f, entranceZ * _cellSize));
            Gizmos.DrawSphere(entrancePos, _cellSize * 0.3f);
            
            // Draw arrow showing entrance direction (rotated)
            Vector3 arrowDir = Vector3.zero;
            switch (_entranceSide)
            {
                case EntranceSide.Left: arrowDir = Vector3.left; break;
                case EntranceSide.Right: arrowDir = Vector3.right; break;
                case EntranceSide.Front: arrowDir = Vector3.forward; break;
                case EntranceSide.Back: arrowDir = Vector3.back; break;
            }
            arrowDir = rotation * arrowDir; // Apply maze rotation to arrow
            Gizmos.DrawLine(entrancePos, entrancePos + arrowDir * _cellSize);
        }
        
        // Draw origin marker
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origin, 0.3f);
        
        // Draw forward direction arrow (blue)
        Gizmos.color = Color.blue;
        Vector3 forwardArrow = origin + rotation * Vector3.forward * 2f;
        Gizmos.DrawLine(origin, forwardArrow);
        Gizmos.DrawSphere(forwardArrow, 0.2f);
    }
}
