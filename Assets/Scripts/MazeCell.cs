using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _rightWall;
    [SerializeField] private GameObject _frontWall;
    [SerializeField] private GameObject _backWall;
    [SerializeField] private GameObject _unvisitedBlock;
    public bool IsVisited { get; private set; }
    
    // Grid position (set by MazeGenerator)
    public int GridX { get; set; }
    public int GridZ { get; set; }
    
    // Expose wall states for checking
    public bool HasLeftWall => _leftWall != null && _leftWall.activeSelf;
    public bool HasRightWall => _rightWall != null && _rightWall.activeSelf;
    public bool HasFrontWall => _frontWall != null && _frontWall.activeSelf;
    public bool HasBackWall => _backWall != null && _backWall.activeSelf;
    
    // Get wall transforms for placing lights
    public Transform LeftWallTransform => _leftWall?.transform;
    public Transform RightWallTransform => _rightWall?.transform;
    public Transform FrontWallTransform => _frontWall?.transform;
    public Transform BackWallTransform => _backWall?.transform;

    public void Visit()
    {
        IsVisited = true;
        _unvisitedBlock.SetActive(false);
    }
    public void ClearLeftWall()
    {
        _leftWall.SetActive(false);
    }
    public void ClearRightWall()
    {
        _rightWall.SetActive(false);
    }
    public void ClearFrontWall()
    {
        _frontWall.SetActive(false);
    }
    public void ClearBackWall()
    {
        _backWall.SetActive(false);
    }
    
    /// <summary>
    /// Rotate a specific wall to face inward (toward cell center)
    /// </summary>
    public void RotateWallToFaceInward(string wallName)
    {
        switch (wallName.ToLower())
        {
            case "left":
                if (_leftWall != null) _leftWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
                break;
            case "right":
                if (_rightWall != null) _rightWall.transform.localRotation = Quaternion.Euler(0, -90, 0);
                break;
            case "front":
                if (_frontWall != null) _frontWall.transform.localRotation = Quaternion.Euler(0, 180, 0);
                break;
            case "back":
                if (_backWall != null) _backWall.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }
    
    /// <summary>
    /// Rotate all walls to face inward
    /// </summary>
    public void RotateAllWallsToFaceInward()
    {
        RotateWallToFaceInward("left");
        RotateWallToFaceInward("right");
        RotateWallToFaceInward("front");
        RotateWallToFaceInward("back");
    }
    
    /// <summary>
    /// Count how many walls are still active (not cleared)
    /// </summary>
    public int GetActiveWallCount()
    {
        int count = 0;
        if (HasLeftWall) count++;
        if (HasRightWall) count++;
        if (HasFrontWall) count++;
        if (HasBackWall) count++;
        return count;
    }
}
