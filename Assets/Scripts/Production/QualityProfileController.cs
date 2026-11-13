using System;
using System.Reflection;
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
        [Range(0.5f, 3f)] public float lodBias = 1f;
        [Range(0f, 150f)] public float shadowDistance = 55f;
        public bool enableSoftShadows = true;
        public bool enablePostProcessing = true;
    }

    /// <summary>
    /// Applies a safe graphics profile even when no ScriptableObject assets have
    /// been authored yet. The defaults are selected for the detected device and
    /// can be overridden later by profile assets or the in-game settings UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QualityProfileController : MonoBehaviour
    {
        [SerializeField] private QualityProfile low;
        [SerializeField] private QualityProfile balanced;
        [SerializeField] private QualityProfile high;

        private const string PreferenceKey = "afg.graphics-tier";
        private static readonly BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private void Awake()
        {
            int fallback = (int)DetectRecommendedTier();
            int saved = PlayerPrefs.GetInt(PreferenceKey, fallback);
            Apply((GraphicsTier)Mathf.Clamp(saved, 0, 2), false);
        }

        public void SetTier(int tier) =>
            Apply((GraphicsTier)Mathf.Clamp(tier, 0, 2), true);

        public GraphicsTier CurrentTier { get; private set; } = GraphicsTier.Balanced;

        private static GraphicsTier DetectRecommendedTier()
        {
            if (SystemInfo.graphicsMemorySize > 0 && SystemInfo.graphicsMemorySize < 1800)
                return GraphicsTier.Low;
            if (SystemInfo.systemMemorySize > 0 && SystemInfo.systemMemorySize < 5000)
                return GraphicsTier.Low;
            if (SystemInfo.processorCount <= 4)
                return GraphicsTier.Low;
            if (SystemInfo.graphicsMemorySize >= 5000 && SystemInfo.processorCount >= 8)
                return GraphicsTier.High;
            return GraphicsTier.Balanced;
        }

        private void Apply(GraphicsTier tier, bool persist)
        {
            QualityProfile profile = GetProfile(tier);
            int cascades = profile != null ? profile.shadowCascades : tier == GraphicsTier.Low ? 0 : 2;
            int antiAliasing = profile != null ? profile.antiAliasing : tier == GraphicsTier.High ? 4 : tier == GraphicsTier.Balanced ? 2 : 0;
            float lodBias = profile != null ? profile.lodBias : tier == GraphicsTier.High ? 1.8f : tier == GraphicsTier.Balanced ? 1.1f : .7f;
            float shadowDistance = profile != null ? profile.shadowDistance : tier == GraphicsTier.High ? 85f : tier == GraphicsTier.Balanced ? 48f : 22f;
            float renderScale = profile != null ? profile.renderScale : tier == GraphicsTier.High ? 1.1f : tier == GraphicsTier.Balanced ? 1f : .8f;
            int frameRate = profile != null ? profile.targetFrameRate : tier == GraphicsTier.High ? 60 : 45;
            bool softShadows = profile != null ? profile.enableSoftShadows : tier != GraphicsTier.Low;

            QualitySettings.shadows = cascades == 0 ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowCascades = cascades;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowResolution = tier == GraphicsTier.High
                ? ShadowResolution.VeryHigh : tier == GraphicsTier.Balanced
                    ? ShadowResolution.Medium : ShadowResolution.Low;
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.lodBias = lodBias;
            QualitySettings.pixelLightCount = tier == GraphicsTier.High ? 4 : tier == GraphicsTier.Balanced ? 2 : 1;
            QualitySettings.softParticles = tier != GraphicsTier.Low;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameRate;
            SetUniversalRenderScale(renderScale);

            CurrentTier = tier;
            if (persist)
            {
                PlayerPrefs.SetInt(PreferenceKey, (int)tier);
                PlayerPrefs.Save();
            }
        }

        private QualityProfile GetProfile(GraphicsTier tier)
        {
            switch (tier)
            {
                case GraphicsTier.Low: return low;
                case GraphicsTier.High: return high;
                default: return balanced;
            }
        }

        // Avoid a hard assembly dependency: the project can still compile if
        // the render pipeline package is changed, while URP receives its scale.
        private static void SetUniversalRenderScale(float scale)
        {
            try
            {
                object pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                if (pipeline == null) return;
                PropertyInfo property = pipeline.GetType().GetProperty("renderScale", InstanceFlags);
                if (property != null && property.CanWrite)
                    property.SetValue(pipeline, Mathf.Clamp(scale, .5f, 2f), null);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not apply render scale: " + exception.Message);
            }
        }
    }
}
