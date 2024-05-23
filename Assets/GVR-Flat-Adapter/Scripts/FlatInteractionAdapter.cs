using System;
using GVR.Context.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Flat
{
    public class FlatInteractionAdapter : IInteractionAdapter
    {
        private const string CONTEXT_NAME = "Flat";

        public string GetContextName()
        {
            return CONTEXT_NAME;
        }

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
