using System.Collections.Generic;
using System.Linq;
using GVR.Extensions;
using GVR.Manager;
using GVR.Properties;
using GVR.World;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToWp : AbstractWalkAnimationAction
    {
        private string destination => action.str0;

        private Queue<DijkstraWaypoint> route;
            
        public GoToWp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // Happens (e.g.) during spawning. As we spawn NPCs onto their current WayPoints, they don't need to walk there from entrance of OC.
            if (props.currentWayPoint.Name.EqualsIgnoreCase(destination))
            {
                isFinished = true;
                return;
            }
            
            route = new Queue<DijkstraWaypoint>(WayNetHelper.FindFastestPath(props.currentWayPoint.Name, destination));
        }
        
        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject.name != route.First().Name)
                return;

            // FIXME - get current waypoint object
            // props.currentWayPoint = coll.gameObject.

            route.Dequeue();
            
            if (route.Count == 0)
            {
                walkState = WalkState.Done;
                isFinished = true;
            }
        }

        protected override Vector3 GetWalkDestination()
        {
            return route.Peek().Position;
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            isFinished = false;
        }
    }
}
