using System;
using GVR.Context.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using Component = UnityEngine.Component;

namespace GVR.Flat
{
    public class FlatInteractionAdapter : IInteractionAdapter
    {
        public GameObject CreatePlayerController(Scene scene)
        {
            throw new NotImplementedException();
        }

        public void AddClimbingComponent(GameObject go)
        {
            throw new NotImplementedException();
        }

        public void AddItemComponent(GameObject go, bool isLab = false)
        {
            throw new NotImplementedException();
        }
    }
}
