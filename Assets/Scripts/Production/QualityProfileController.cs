using UnityEngine;

namespace AfghanistanOld.Settings
{
    public enum GraphicsTier { Low, Balanced, High }

    [CreateAssetMenu(menuName = "Afghanistan Old/Settings/Quality Profile", fileName = "QualityProfile")]
    public sealed class QualityProfile : ScriptableObject
    {
        [Header("Rendering")]
        [Range(0, 2)] public int shadowCascades = 2;
        [Range(0, 4)] public int antiAliasing = 2;
        [Range(0.5f, 2f)] public float renderScale = 1f;
        [Range(30, 120)] public int targetFrameRate = 60;
        [Range(50, 500)] public float lodBias = 1f;
        public bool enableSoftShadows = true;
        public bool enablePostProcessing = true;
    }

    public sealed class QualityProfileController : MonoBehaviour
    {
        [SerializeField] private QualityProfile low;
        [SerializeField] private QualityProfile balanced;
        [SerializeField] private QualityProfile high;

        private const string PreferenceKey = "afg.graphics-tier";

        private void Awake()
        {
            int saved = PlayerPrefs.GetInt(PreferenceKey, (int)GraphicsTier.Balanced);
            Apply((GraphicsTier)Mathf.Clamp(saved, 0, 2), false);
        }

        public void SetTier(int tier) => Apply((GraphicsTier)Mathf.Clamp(tier, 0, 2), true);

        private void Apply(GraphicsTier tier, bool persist)
        {
            QualityProfile profile = tier switch
            {
                GraphicsTier.Low => low,
                GraphicsTier.High => high,
                _ => balanced
            };
            if (profile == null) return;
            QualitySettings.shadowCascades = profile.shadowCascades;
            QualitySettings.antiAliasing = profile.antiAliasing;
            QualitySettings.lodBias = profile.lodBias;
            QualitySettings.shadows = profile.shadowCascades == 0 ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.softParticles = tier != GraphicsTier.Low;
            Application.targetFrameRate = profile.targetFrameRate;
            if (persist)
            {
                PlayerPrefs.SetInt(PreferenceKey, (int)tier);
                PlayerPrefs.Save();
            }
        }
    }
}