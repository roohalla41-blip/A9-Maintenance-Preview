using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    [Min(24)] public int worldSize = 44;
    public Material groundMaterial;
    public Material waterMaterial;

    private void Start()
    {
        if (FindObjectOfType<EnvironmentBuilder>() == null)
            gameObject.AddComponent<EnvironmentBuilder>().Build(worldSize, groundMaterial, waterMaterial);

        if (GetComponent<VillageSimulation>() == null)
            gameObject.AddComponent<VillageSimulation>();

        GameObject cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.GetComponent<Camera>();
        if (cam == null) cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = false;
        cam.fieldOfView = 55f;

        if (cameraObject.GetComponent<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();

        FreeCityCameraController controller = cameraObject.GetComponent<FreeCityCameraController>();
        if (controller == null) controller = cameraObject.AddComponent<FreeCityCameraController>();
        controller.SetInitialView(Vector3.zero, 23f, 48f, 0f);

        if (GetComponent<GridBuildingSystem>() == null)
        {
            GridBuildingSystem builder = gameObject.AddComponent<GridBuildingSystem>();
            builder.gridSize = 1f;
            builder.buildRadius = 17f;
        }
    }
}