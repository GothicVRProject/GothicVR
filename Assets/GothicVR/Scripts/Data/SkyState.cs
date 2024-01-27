using UnityEngine;

namespace GVR.Data
{
    /// <summary>
    /// This class is mostly a copy of the data from the original game, but with some changes as to make transitions cleaner.
    /// (E.g textures for transitional layers such as dawn and evening and night1, day1)
    /// </summary>
    public class SkyState
    {
        public float time;
        public Vector3 polyColor;
        public Vector3 fogColor;
        public Vector3 domeColor1;
        public Vector3 domeColor0;
        public float fogDist;
        public int sunOn = 1;
        public int cloudShadowOn;
        public SkyLayerData[] layer;

        // how long the transition should take
        // 0.05 = 1 hour and 12 minutes
        public float lerpDuration = 0.05f;

        public SkyState()
        {
            layer = new SkyLayerData[2];
            layer[0] = new SkyLayerData();
            layer[1] = new SkyLayerData();
        }

        public void PresetDawn()
        {
            time = 0.7f; // 4:48 am

            polyColor = new Vector3(190.0f, 160.0f, 255.0f); // ambient light
            fogColor = new Vector3(80.0f, 60.0f, 105.0f); // fog
            domeColor0 = new Vector3(80.0f, 60.0f, 105.0f); // dome color
            domeColor1 = new Vector3(255.0f, 255.0f, 255.0f);

            layer[0].texName = "SKYNIGHT_LAYER0_A0.TGA";
            layer[1].texName = "SKYDAY_LAYER0_A0.TGA";

            layer[0].texAlpha = 128.0f;
            layer[1].texAlpha = 128.0f;

            layer[0].texSpeed.y = 0.0f;
            layer[0].texSpeed.x = 0.0f;

            fogDist = 0.5f;
            sunOn = 1;
        }

        public void PresetDay0()
        {
            time = 0.75f; // 6:00 am

            polyColor = new Vector3(255.0f, 250.0f, 235.0f);
            fogColor = new Vector3(120.0f, 140.0f, 180.0f);
            domeColor0 = fogColor;
            domeColor1 = new Vector3(255.0f, 255.0f, 255.0f);

            layer[0].texName = "SKYDAY_LAYER0_A0.TGA";
            layer[1].texName = "SKYDAY_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 0.0f;

            layer[1].texSpeed *= 0.2f;

            fogDist = 0.2f;
            sunOn = 1;
        }

        public void PresetDay1()
        {
            time = 0f; // 12:00 pm

            polyColor = new Vector3(255.0f, 250.0f, 235.0f);
            fogColor = new Vector3(120.0f, 140.0f, 180.0f);
            domeColor0 = fogColor;
            domeColor1 = new Vector3(255.0f, 255.0f, 255.0f);

            layer[0].texName = "SKYDAY_LAYER0_A0.TGA";
            layer[1].texName = "SKYDAY_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 215.0f;

            fogDist = 0.05f;
            sunOn = 1;
        }

        public void PresetDay2()
        {
            time = 0.25f; // 6:00 pm

            polyColor = new Vector3(255.0f, 250.0f, 235.0f);
            fogColor = new Vector3(120.0f, 140.0f, 180.0f);
            domeColor0 = new Vector3(120.0f, 140.0f, 180.0f);
            domeColor1 = new Vector3(255.0f, 255.0f, 255.0f);

            layer[0].texName = "SKYDAY_LAYER0_A0.TGA";
            layer[1].texName = "SKYDAY_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 0.0f;

            fogDist = 0.05f;
            sunOn = 1;
        }

        public void PresetEvening()
        {
            time = 0.3f; // 7:12 pm

            polyColor = new Vector3(255.0f, 185.0f, 170.0f);
            fogColor = new Vector3(170.0f, 70.0f, 50.0f);
            domeColor0 = new Vector3(170.0f, 70.0f, 50.0f);
            domeColor1 = new Vector3(255.0f, 255.0f, 255.0f);

            layer[0].texName = "SKYNIGHT_LAYER0_A0.TGA";
            layer[1].texName = "SKYDAY_LAYER0_A0.TGA";

            layer[0].texAlpha = 128.0f;
            layer[1].texAlpha = 128.0f;

            layer[0].texSpeed.x = 0.0f;
            layer[0].texSpeed.y = 0.0f;

            sunOn = 1;
            fogDist = 0.2f;
        }

        public void PresetNight0()
        {
            time = 0.35f; // 8:24 pm

            polyColor = new Vector3(105.0f, 105.0f, 195.0f);
            fogColor = new Vector3(20.0f, 20.0f, 60.0f);
            domeColor0 = fogColor;
            domeColor1 = new Vector3(255.0f, 55.0f, 155.0f);

            layer[0].texName = "SKYNIGHT_LAYER0_A0.TGA";
            layer[1].texName = "SKYNIGHT_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 0.0f;

            layer[0].texScale *= 4.0f;

            layer[0].texSpeed.x = 0.0f;
            layer[0].texSpeed.y = 0.0f;

            fogDist = 0.1f;
            sunOn = 0;
            cloudShadowOn = 0;
        }

        public void PresetNight1()
        {
            time = 0.5f; // 12:00 am

            polyColor = new Vector3(40.0f, 60.0f, 210.0f);
            fogColor = new Vector3(5.0f, 5.0f, 20.0f);
            domeColor0 = fogColor;
            domeColor1 = new Vector3(55.0f, 55.0f, 155.0f);

            layer[0].texName = "SKYNIGHT_LAYER0_A0.TGA";
            layer[1].texName = "SKYNIGHT_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 215.0f;

            layer[0].texSpeed.y = 0.0f;
            layer[0].texSpeed.x = 0.0f;

            fogDist = 0.05f;
            sunOn = 0;
        }

        public void PresetNight2()
        {
            time = 0.65f; // 3:36 am

            polyColor = new Vector3(40.0f, 60.0f, 210.0f);
            fogColor = new Vector3(5.0f, 5.0f, 20.0f);
            domeColor0 = fogColor;
            domeColor1 = new Vector3(55.0f, 55.0f, 155.0f);

            layer[0].texName = "SKYNIGHT_LAYER0_A0.TGA";
            layer[1].texName = "SKYNIGHT_LAYER1_A0.TGA";

            layer[0].texAlpha = 255.0f;
            layer[1].texAlpha = 0.0f;

            layer[0].texSpeed.y = 0.0f;
            layer[0].texSpeed.x = 0.0f;

            fogDist = 0.2f;
            sunOn = 0;
        }
    }
}