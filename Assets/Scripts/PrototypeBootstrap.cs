using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    [Min(24)] public int worldSize = 44;
    public Material groundMaterial;
    public Material waterMaterial;

    private void Start()
    {
        if (GetComponent<AfghanistanOld.Settings.QualityProfileController>() == null)
            gameObject.AddComponent<AfghanistanOld.Settings.QualityProfileController>();

        if (FindObjectOfType<EnvironmentBuilder>() == null)
            gameObject.AddComponent<EnvironmentBuilder>().Build(worldSize, groundMaterial, waterMaterial);

        // Load progression and the saved building world before the village
        // spawns its citizens and guards, so restored barracks/academy levels
        // affect the units created at the beginning of this session.
        if (GetComponent<EmpireProgressionSystem>() == null)
            gameObject.AddComponent<EmpireProgressionSystem>();

        if (!BuildingWorldSaveSystem.Load(out string buildingLoadError) &&
            !string.IsNullOrEmpty(buildingLoadError))
            Debug.LogWarning("Initial building world load failed: " + buildingLoadError);

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

        if (GetComponent<EnemyRaidSystem>() == null)
            gameObject.AddComponent<EnemyRaidSystem>();

        if (GetComponent<GridBuildingSystem>() == null)
        {
            GridBuildingSystem builder = gameObject.AddComponent<GridBuildingSystem>();
            builder.gridSize = 1f;
            builder.buildRadius = 17f;
        }

        if (GetComponent<BuildingUpgradeSystem>() == null)
            gameObject.AddComponent<BuildingUpgradeSystem>();

        if (GetComponent<TechnologyResearchPanel>() == null)
            gameObject.AddComponent<TechnologyResearchPanel>();

        if (GetComponent<LocalSaveCoordinator>() == null)
            gameObject.AddComponent<LocalSaveCoordinator>();
    }
}
