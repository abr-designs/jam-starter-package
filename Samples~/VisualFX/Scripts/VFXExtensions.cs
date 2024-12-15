using UnityEngine;
using UnityEngine.Assertions;

namespace VisualFX
{
    public static class VFXExtension
    {
        public static GameObject PlayAtLocation(this VFX vfx, Vector3 worldPosition, float scale = 1f, bool keepAlive = false)
        {
            var vfxManager = VFXManager;
            
            Assert.IsNotNull(vfxManager, $"Missing the {nameof(VFXManager)} in the Scene!!");
            return vfxManager.PlayAtLocation(vfx, worldPosition, scale, keepAlive);
        }

        private static VFXManager VFXManager => VFXManager.Instance;
    }
}