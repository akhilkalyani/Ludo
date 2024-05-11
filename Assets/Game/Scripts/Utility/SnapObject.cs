using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapObject : MonoBehaviour
{
    [SerializeField] private Vector2 gridSize=Vector2.one;
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying && this.transform.hasChanged)
        {
            SnapToGrid2();
        }
    }
    private void SnapToGrid2()
    {
        var position = new Vector2(
            SnapTo(this.transform.position.x,this.gridSize.x),
            SnapTo(this.transform.position.y,this.gridSize.y)
            );
        this.transform.position = position;
    }
    public float SnapTo(float a, float snap)
    {
        return Mathf.Round(a / snap) * snap;
    }
}
