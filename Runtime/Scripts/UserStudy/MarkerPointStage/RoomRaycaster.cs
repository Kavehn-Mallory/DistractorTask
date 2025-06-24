using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class RoomRaycaster : MonoBehaviour
    {
        
        public ARRaycastManager manager;
        public ARRaycast raycast;
        
        private void Start()
        {
            //Raycast.plane
            manager.AddRaycast(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), 10f);
            //MixedRealityRaycaster.RaycastSimplePhysicsStep()
        }
        
        protected void PerformRaycast()
        {
            
            /*
            RayStep currentRay = new RayStep();
            // The transform target is the transform of the TrackedTargetType, i.e. Controller Ray, Head or Hand Joint
            Transform transform = SolverHandler.TransformTarget;

            Vector3 origin = transform.position;
            Vector3 endpoint = transform.position + transform.forward;
            currentRay.UpdateRayStep(in origin, in endpoint);

            // Check if the current ray hits a magnetic surface
            var didHitSurface = MixedRealityRaycaster.RaycastSimplePhysicsStep(currentRay, MaxRaycastDistance, MagneticSurfaces, false, out currentHit);
            */

        }
    }
}