using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Context.Controls
{
    public interface IInteractionAdapter
    {
        GameObject CreatePlayerController(Scene scene);
        void AddClimbingComponent(GameObject go);
        void AddItemComponent(GameObject go, bool isLab = false);
    }
}
