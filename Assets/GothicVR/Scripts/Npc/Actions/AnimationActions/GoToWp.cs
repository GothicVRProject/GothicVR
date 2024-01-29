using System.Collections.Generic;
using System.Linq;
using GVR.Manager;
using GVR.Vob.WayNet;
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
            if (Props.CurrentWayPoint != null && (destination == "" || Props.CurrentWayPoint.Name == destination))
            {
                IsFinishedFlag = true;
                return;
            }

            WayPoint waypoint = Props.CurrentWayPoint != null ? Props.CurrentWayPoint : WayNetHelper.FindNearestWayPoint(Props.transform.position);
            string finalDestination = destination;

            if (destination == "OCR_OUSIDE_HUT_77_INSERT")
                finalDestination = "OCR_OUTSIDE_HUT_77_INSERT";

            route = new Stack<DijkstraWaypoint>(WayNetHelper.FindFastestPath(waypoint.Name, finalDestination));
            
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
                IsFinishedFlag = true;
            }
            else
            {
                // A new waypoint is destination, we therefore rotate NPC again.
                walkState = WalkState.WalkAndRotate;
            }
        }

        protected override Vector3 GetWalkDestination()
        {
            return route.Peek().Position;
        }

        public override void AnimationEndEventCallback()
        {
            base.AnimationEndEventCallback();

            IsFinishedFlag = false;
        }
    }
}
