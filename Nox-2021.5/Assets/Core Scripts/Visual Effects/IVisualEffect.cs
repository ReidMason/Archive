using UnityEngine;

namespace NoxCore.Effects
{
    public interface IVisualEffect
    {
        bool getIsRunning();
        void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0);
        void setSortingLayerOrder(Transform prefabRoot, int sortingOrderOffset = 0);
        void startVFX();
        void stopVFX();
        void updateVFX();
    }
}