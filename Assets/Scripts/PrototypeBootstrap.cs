using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    public int worldSize = 44;
    public Material groundMaterial;
    public Material waterMaterial;

    private void Start()
    {
        gameObject.AddComponent<EnvironmentBuilder>()
            .Build(worldSize, groundMaterial, waterMaterial);

        GameObject cameraObject = Camera.main != null
            ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera cam = cameraObject.GetComponent<Camera>();
        if (cam == null) cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = false;
        cam.fieldOfView = 55f;
        cameraObject.transform.position = new Vector3(0, 17, -20);
        cameraObject.transform.LookAt(new Vector3(0, 0, 0));

        if (cameraObject.GetComponent<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();
        if (cameraObject.GetComponent<FreeCityCameraController>() == null)
            cameraObject.AddComponent<FreeCityCameraController>();

        GridBuildingSystem buildingSystem = gameObject.AddComponent<GridBuildingSystem>();
        buildingSystem.gridSize = 1f;
        buildingSystem.buildRadius = 17f;
    }
}