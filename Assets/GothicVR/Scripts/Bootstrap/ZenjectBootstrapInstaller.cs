using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GVR.ZenjectTest
{
    public class ZenjectBootstrapInstaller : MonoBehaviour
    {
        private static readonly List<string> INSTALLERS = new() {
            "GVR.HVR.HVRInstaller",
            "GVR.Flat.FlatInstaller"
        };

        private void Awake()
        {
            var installers = new List<MonoInstaller>();

            foreach (var installer in INSTALLERS)
            {
                var type = Type.GetType(installer);

                if (type == null)
                    continue;

                var component = (MonoInstaller)gameObject.AddComponent(type);

                installers.Add(component);
            }

            GetComponent<SceneContext>().Installers = installers;
        }
    }
}
