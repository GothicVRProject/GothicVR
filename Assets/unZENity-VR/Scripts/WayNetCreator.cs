using UnityEngine;

namespace UZVR
{
    public class WayNetCreator
    {
        public void Create(GameObject root, PCBridge_World world)
        {
            var wayNetObj = new GameObject(string.Format("WayNet"));
            wayNetObj.transform.parent = root.transform;


            foreach (var waypoint in world.waypoints)
            {
                var wpobject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                wpobject.name = waypoint.name;
                wpobject.transform.position= waypoint.position / 100;

                wpobject.transform.parent = wayNetObj.transform;
            }

        }
    }
}
