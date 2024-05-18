using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Context.Controls
{
    public interface IPlayerControllerAdapter
    {
        GameObject CreatePlayerController(Scene scene);
    }
}
