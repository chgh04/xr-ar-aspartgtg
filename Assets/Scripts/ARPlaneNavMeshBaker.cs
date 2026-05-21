using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.UI;

public class ARPlaneNavMeshBaker : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    private ARPlaneManager arPlaneManager;

    void Awake()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        arPlaneManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void OnDisable()
    {
        arPlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        if (args.added.Count > 0 || args.updated.Count > 0)
        {
            foreach (var plane in args.added)
            {
                plane.transform.parent = navMeshSurface.gameObject.transform;
            }

            BakeNavMesh();
        }
    }

    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}
