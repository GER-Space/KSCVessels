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
        Part rootPart;

        private static List<string> blackList = new List<string>();
        private static List<string> delayList = new List<string>();
        private static bool isInitialized = false;

        static SpaceCenterCamera2 spaceCenterCam;

        List<Part> allObjects = new List<Part>();

        new public void Start()
        {
            // only work in the spaceCenter Scene
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            { 
                return;
            }
            if (vessel.protoVessel == null)
            {
                return;
            }

            // ignore asteroids and other spaceObjects
            if (vessel.vesselType == VesselType.SpaceObject || vessel.vesselType == VesselType.Unknown)
            {
                return;
            }

            if (!isInitialized)
            {
                Initialize();
            }

            // fetch the SpaceCenterCam only once
            if (spaceCenterCam == null)
            {
                spaceCenterCam = Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>().FirstOrDefault();
            }


            // only load vessels that are near
            if (Vector3.Distance(vessel.transform.position, spaceCenterCam.transform.position) < 8000)
            {
                allObjects.Clear();
                rootPart = LoadPart(protoVessel.protoPartSnapshots[rootIdx]);
                
                for (int i = 0; i < protoVessel.protoPartSnapshots.Count; i++)
                {
                    if (i == rootIdx)
                    {
                        continue;
                    }
                    LoadPart(protoVessel.protoPartSnapshots[i]);
                }              
            }
        }


        internal void LoadRootPart()
        {
            ProtoPartSnapshot rootProtoPart = protoVessel.protoPartSnapshots[rootIdx];
            rootPart = (Part)GameObject.Instantiate(rootProtoPart.partPrefab, vessel.transform.position, vessel.transform.rotation);
            rootPart.gameObject.SetActive(true);
            allObjects.Add(rootPart);
        }

        internal Part LoadPart(ProtoPartSnapshot protoPart)
        {
            Part newPart = (Part)GameObject.Instantiate(protoPart.partPrefab, vessel.transform.position, vessel.transform.rotation);

            if (rootPart != null)
            {
                newPart.transform.parent = rootPart.transform;
                newPart.setParent(rootPart);
            } else
            {
                newPart.transform.parent = vessel.transform;
            }

            newPart.transform.localPosition = protoPart.position;
            newPart.transform.localRotation = protoPart.rotation;
            newPart.gameObject.SetActive(true);

            newPart.vessel = vessel;

            int index = 0;

            foreach (var module in protoPart.modules)
            {
                //if (!blacklist.Contains(module.moduleName))
                //{
                    //Log.Normal("Loaded Module: " + (module.moduleName));
                    module.Load(newPart, ref index);
                //}
            }

            PartModule[] partModules = newPart.Modules.Cast<PartModule>().ToArray();

            foreach (var module in partModules)
            {

                if (delayList.Contains(module.moduleName))
                {
                    StartCoroutine(CallbackUtil.DelayedCallback(10, DestroyDelayed, module));
                }
                else
                {
                    if (!blackList.Contains(module.moduleName))
                    {
                        Log.Normal("Loaded Module: " + (module.moduleName));
                        module.OnAwake();
                        module.OnStart(PartModule.StartState.PreLaunch);
                    }
                    newPart.Modules.Remove(module);
                    GameObject.DestroyImmediate(module);
                }
            }

            vessel.parts.Add(newPart);

            allObjects.Add(newPart);
            return newPart;
        }


        internal void DestroyDelayed(PartModule module)
        {
            module.part.Modules.Remove(module);
            GameObject.DestroyImmediate(module);
        }


        /// <summary>
        /// remove parts when a vessel is recovered
        /// </summary>
        public void OnDestroy()
        {
            foreach (Part obj in allObjects)
            {
                obj.gameObject.DestroyGameObject();
            }
        }


        private static void Initialize()
        {
            isInitialized = true;
            //Log.Normal("init called");
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KSCVessel_ModuleBlackList"))
            {
                //Log.Normal("cfg Node found");
                foreach (string name in node.GetValues("Entry"))
                {
                    //Log.Normal("added to blacklist: " + name);
                    blackList.Add(name);
                }
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KSCVessel_DelayedModules"))
            {
                //Log.Normal("delay cfg Node found");
                foreach (string name in node.GetValues("Entry"))
                {
                    //Log.Normal("added to delayList: " + name);
                    delayList.Add(name);
                }
            }
        }


    }
}
