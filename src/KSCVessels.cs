using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSCVessels
{
    public class KSCVessels : VesselModule
    {

        int rootIdx => vessel.protoVessel.rootIndex;
        ProtoVessel protoVessel => vessel.protoVessel;

        GameObject rootPart;

        static SpaceCenterCamera2 spaceCenterCam;

        List<GameObject> allObjects = new List<GameObject>();

        new public void Start()
        {
            // only work in the spaceCenter Scene
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            { 
                return;
            }

            // ignore asteroids and other spaceObjects
            if (vessel.vesselType == VesselType.SpaceObject || vessel.vesselType == VesselType.Unknown)
            {
                return;
            }
            // fetch the SpaceCenterCam only once
            if (spaceCenterCam == null)
            {
                spaceCenterCam = Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>().FirstOrDefault();
            }
            // only load vessels that are near
            if (Vector3.Distance(vessel.transform.position, spaceCenterCam.transform.position) < 8000)
            {
                LoadRootPart();
                LoadOtherParts();
            }

        }


        internal void LoadRootPart()
        {
            ProtoPartSnapshot rootProtoPart = protoVessel.protoPartSnapshots[rootIdx];
            rootPart = GameObject.Instantiate(rootProtoPart.partPrefab.gameObject, vessel.transform.position, vessel.transform.rotation);
            rootPart.gameObject.SetActive(true);
            allObjects.Add(rootPart);
        }

        internal void LoadOtherParts()
        {
            for (int i = 0 ; i < protoVessel.protoPartSnapshots.Count; i++) 
            {
                // ignore the root part
                if (i == rootIdx)
                {
                    continue;
                }
                ProtoPartSnapshot protoPart = protoVessel.protoPartSnapshots[i];
                GameObject part = GameObject.Instantiate(protoPart.partPrefab.gameObject, vessel.transform.position, vessel.transform.rotation);
                part.transform.parent = rootPart.transform;
                part.transform.localPosition = protoPart.position;
                part.transform.localRotation = protoPart.rotation;
                part.gameObject.SetActive(true);
                allObjects.Add(part);
            }
        }

        /// <summary>
        /// remove parts when a vessel is recovered
        /// </summary>
        public void OnDestroy()
        {
            foreach (GameObject obj in allObjects)
            {
                obj.DestroyGameObject();
            }
        }
    }
}
