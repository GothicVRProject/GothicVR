using System.Collections.Generic;
using System.Linq;
using GVR.Manager;
using GVR.World;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToWp : AbstractWalkAnimationAction
    {
        private string destination => Action.String0;

        private Stack<DijkstraWaypoint> route;
            
        public GoToWp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            /*
             * 1. AI_StartState() can get called multiple times until it won't share the WP. (e.g. ZS_SLEEP -> ZS_StandAround())
             * 2. Happens (e.g.) during spawning. As we spawn NPCs onto their current WayPoints, they don't need to walk there from entrance of OC.
             */
            if (destination == "" || Props.currentWayPoint.Name == destination)
            {
                isFinished = true;
                return;
            }
            
            route = new Stack<DijkstraWaypoint>(WayNetHelper.FindFastestPath(Props.currentWayPoint.Name, destination));
        }
        
        public override void OnTriggerEnter(Collider coll)
        {
            if (walkState != WalkState.Walk)
                return;

            if (coll.gameObject.name != route.First().Name)
                return;

            // FIXME - get current waypoint object
            // props.currentWayPoint = coll.gameObject.

            route.Pop();

            if (route.Count == 0)
            {
                walkState = WalkState.Done;
                isFinished = true;
            }
            else
                walkState = WalkState.Initial;
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
