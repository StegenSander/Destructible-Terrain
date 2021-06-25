using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour
{
    [SerializeField] private World _WorldToEdit;
    [SerializeField] [Tooltip("This shape will be added/removed from the terrain")] private Collider _MouseTarget;
    [SerializeField] [Tooltip("Provides an offset the the shape you remove/add")] private Vector3 _Offset;
    // Start is called before the first frame update
    void Start()
    {
        if (!_WorldToEdit)
            Debug.LogError("TerrainEditor > _WorldToEdit not set to an instance of a object");
        if (!_MouseTarget)
            Debug.LogError("TerrainEditor > _MouseTarget not set to an instance of a object");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMouseTarget();

        if(Input.GetMouseButtonDown(0))
        {
            TerrainModifier.ModifyTerrain(_WorldToEdit,_MouseTarget,TerrainModifier.TerrainChange.Add);
        }
    }

    void UpdateMouseTarget()
    {
        if (Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000.0f))
        {
            _MouseTarget.transform.position = hitInfo.point + _Offset;
        }
        else
        {
            ray.origin = ray.origin + 1000.0f * ray.direction;
            ray.direction = -ray.direction;

            if (_WorldToEdit.Boundaries.IntersectRay(ray, out float distance))
            {
                Vector3 pointOnBoudingBox = ray.origin + distance * ray.direction;
                _MouseTarget.transform.position = pointOnBoudingBox + _Offset;
            }
        }
    }
}
