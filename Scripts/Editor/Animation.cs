using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine.SceneManagement;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
using static Shadster.AvatarTools.Menus;
using static Shadster.AvatarTools.Helper;
using static Shadster.AvatarTools.Scenes;
using static Shadster.AvatarTools.AnimatorControl;
using static Shadster.AvatarTools.Materials;
using UnityEditor.Animations;

namespace Shadster.AvatarTools
{
    public class Animation : Editor
    {

        public static AnimationClip[] GenerateAnimationToggle(SkinnedMeshRenderer mRender, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipOn = new AnimationClip();
            aClipOff.name = mRender.name + " OFF";
            aClipOn.name = mRender.name + " ON";
            var path = AnimationUtility.CalculateTransformPath(mRender.transform, vrcAvatar.transform);
            aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
            aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
            clips[0] = SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            clips[1] = SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

            return clips;
        }

        public static AnimationClip[] GenerateAnimationToggle(GameObject go, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipOn = new AnimationClip();
            aClipOff.name = go.name + " OFF";
            aClipOn.name = go.name + " ON";
            var path = AnimationUtility.CalculateTransformPath(go.transform, vrcAvatar.transform);
            aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
            aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
            clips[0] = SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            clips[1] = SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

            return clips;
        }

        public static void GenerateAnimationRenderToggles(GameObject vrcAvatar)
        {
            AnimationClip allOff = new AnimationClip();
            AnimationClip allOn = new AnimationClip();
            allOff.name = "all OFF";
            allOn.name = "all ON";
            foreach (var r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                //Debug.Log(r.name);
                AnimationClip aClipOff = new AnimationClip();
                AnimationClip aClipOn = new AnimationClip();
                aClipOff.name = r.name + " OFF";
                aClipOn.name = r.name + " ON";
                var path = AnimationUtility.CalculateTransformPath(r.transform, vrcAvatar.transform);
                aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
                AddToggleShapekeys(vrcAvatar, r, aClipOff, false);
                aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
                AddToggleShapekeys(vrcAvatar, r, aClipOn, true);
                allOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
                allOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));

                SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
                SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            }
            SaveAnimation(allOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            SaveAnimation(allOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

        }

        public static void AddToggleShapekeys(GameObject vrcAvatar, Renderer r, AnimationClip clip, bool toggle)
        {

            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                    var splitString = smr.sharedMesh.GetBlendShapeName(i).Split('_');
                    string prefix = string.Join("_", splitString.Take(splitString.Length - 1));
                    string suffix = splitString[splitString.Length - 1].ToLower();
                    //Debug.Log("Prefix: " + prefix);
                    //Debug.Log("Suffix: " + suffix);
                    if (prefix == r.name && suffix == "on")
                    {
                        if (toggle)
                            clip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                        else
                            clip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                    }
                }
            }
        }

        public static AnimationClip[] GenerateShapekeyToggle(SkinnedMeshRenderer smr, string shapeKeyName, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            //Debug.Log(smr.name);

            var dir = new DirectoryInfo(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
            var minClipPath = dir + "/" + shapeKeyName + " MIN.anim";
            var maxClipPath = dir + "/" + shapeKeyName + " MAX.anim";

            if (File.Exists(minClipPath) && File.Exists(maxClipPath))
            {
                Debug.Log("Using animation files that already exist for" + shapeKeyName);
                clips[0] = AssetDatabase.LoadAssetAtPath<AnimationClip>(minClipPath);
                clips[1] = AssetDatabase.LoadAssetAtPath<AnimationClip>(maxClipPath);
            }
            else
            {
                AnimationClip aClipMin = new AnimationClip();
                AnimationClip aClipMax = new AnimationClip();
                var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + shapeKeyName, new AnimationCurve(new Keyframe(0, 0)));
                //aClipMin.name = smr.name + "_" + shapeKeyName + " MIN";
                aClipMin.name = shapeKeyName + " MIN";
                aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + shapeKeyName, new AnimationCurve(new Keyframe(0, 100)));
                //aClipMax.name = smr.name + "_" + shapeKeyName + " MAX";
                aClipMax.name = shapeKeyName + " MAX";

                clips[0] = SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                clips[1] = SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
            }

            return clips;

        }

        public static void GenerateAnimationShapekeys(GameObject vrcAvatar)
        {
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    //Debug.Log(smr.sharedMesh.GetBlendShapeName(i));
                    AnimationClip aClipMin = new AnimationClip();
                    AnimationClip aClipMax = new AnimationClip();
                    var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                    aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                    aClipMin.name = smr.name + "_" + smr.sharedMesh.GetBlendShapeName(i) + " MIN";
                    aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                    aClipMax.name = smr.name + "_" + smr.sharedMesh.GetBlendShapeName(i) + " MAX";

                    SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                }
            }
        }

        public static void GenerateAnimationPhysbones(GameObject vrcAvatar, VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            //AnimationClip aClipOn = new AnimationClip();
            //AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipCollisonOn = new AnimationClip();
            AnimationClip aClipCollisonOff = new AnimationClip();
            AnimationClip aClipCollisionOthers = new AnimationClip();

            foreach (var pbone in vrcAvatar.GetComponentsInChildren<VRC_PhysBone>(true))
            {

                var path = AnimationUtility.CalculateTransformPath(pbone.transform, vrcAvatar.transform);
                aClipCollisonOff.SetCurve(path, typeof(VRC_PhysBone), "m_Enabled", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));
                aClipCollisonOn.SetCurve(path, typeof(VRC_PhysBone), "m_Enabled", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));
                //aClipCollisionOthers.SetCurve(path, typeof(VRC_PhysBone), "m_Enabled", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));

                aClipCollisonOff.SetCurve(path, typeof(VRC_PhysBone), "collisionFilter.allowSelf", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 0)));
                aClipCollisonOn.SetCurve(path, typeof(VRC_PhysBone), "collisionFilter.allowSelf", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));

                //aClipCollisonOff.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 0)));
                //aClipCollisonOn.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.1f, 1)));
                //aClipCollisionOthers.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 2), new Keyframe(0.1f, 2)));

                aClipCollisonOn.name = "Physbone Collision Self ON";
                aClipCollisonOff.name = "Physbone Collision Self OFF";
                //aClipCollisionOthers.name = "Physbone Collision Self OFF";

            }
            //SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            //SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            var clip1 = SaveAnimation(aClipCollisonOn, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            var clip2 = SaveAnimation(aClipCollisonOff, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            //var clip2 = SaveAnimation(aClipCollisionOthers, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            string paramName = "PhysCollision";
            CreateToggleLayer(vrcAvatarDescriptor, "PhysCollision Settings", paramName, clip2, clip1);
            //CreateFxParameter(vrcAvatarDescriptor, "PhysCollision", AnimatorControllerParameterType.Bool);
            var menu = CreateNewMenu("Menu_Physbones");
            CreateMenuControl(menu, "Enable Self Colliders", VRCExpressionsMenu.Control.ControlType.Toggle, paramName);

        }

        public static void CombineAnimationShapekeys(GameObject vrcAvatar)
        {
            List<string> blendPaths = new List<string>();
            AnimationClip allMin = new AnimationClip();
            AnimationClip allMax = new AnimationClip();
            allMin.name = "all MIN";
            allMax.name = "all MAX";
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    blendPaths.Add(AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform)); //First blend path
                    foreach (var smr2 in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true)) //check other skinned mesh renders
                    {
                        if (smr.name != smr2.name)
                        {
                            for (int j = 0; j < smr2.sharedMesh.blendShapeCount; j++) //check those render's shapekeys
                            {
                                if (smr.sharedMesh.GetBlendShapeName(i) == smr2.sharedMesh.GetBlendShapeName(j)) //Matching shapes found
                                {
                                    blendPaths.Add(AnimationUtility.CalculateTransformPath(smr2.transform, vrcAvatar.transform));
                                }
                            }
                        }
                    }
                    AnimationClip aClipMin = new AnimationClip();
                    AnimationClip aClipMax = new AnimationClip();
                    aClipMin.name = smr.sharedMesh.GetBlendShapeName(i) + " MIN";
                    aClipMax.name = smr.sharedMesh.GetBlendShapeName(i) + " MAX";
                    foreach (var path in blendPaths)
                    {
                        //var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                        allMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                        aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                        allMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                    }
                    SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    blendPaths.Clear();
                }
            }
            SaveAnimation(allMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
            SaveAnimation(allMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
        }

        public static void CombineOutfitShapekeys(GameObject vrcAvatar)
        {
            // Dictionary to store outfit GameObjects by outfit number
            Dictionary<string, List<GameObject>> outfitGameObjects = new Dictionary<string, List<GameObject>>();

            // Dictionary to store outfit shapekeys by outfit number
            Dictionary<string, List<(string path, SkinnedMeshRenderer smr, string shapeName)>> outfitShapekeys =
                new Dictionary<string, List<(string path, SkinnedMeshRenderer smr, string shapeName)>>();

            // Collect all GameObjects with outfit names
            foreach (var obj in vrcAvatar.GetComponentsInChildren<Transform>(true))
            {
                if (obj.name.StartsWith("Outfit") && obj.name.Length > 6) // "Outfit" + number
                {
                    string outfitNumber = obj.name.Substring(6); // Extract number part
                    if (!outfitGameObjects.ContainsKey(outfitNumber))
                    {
                        outfitGameObjects[outfitNumber] = new List<GameObject>();
                    }
                    outfitGameObjects[outfitNumber].Add(obj.gameObject);
                }
            }

            // Collect all shapekeys with outfit names
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh == null) continue;

                string smrPath = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);

                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    string shapeName = smr.sharedMesh.GetBlendShapeName(i);

                    // Look for shapekeys like "Outfit1_On", "Outfit2_On", etc.
                    if (shapeName.StartsWith("Outfit") && shapeName.Contains("_On"))
                    {
                        // Extract outfit number (e.g., "1" from "Outfit1_On")
                        string[] parts = shapeName.Split('_');
                        if (parts.Length >= 2 && parts[0].StartsWith("Outfit"))
                        {
                            string outfitNumber = parts[0].Substring(6); // Extract number part
                            if (!outfitShapekeys.ContainsKey(outfitNumber))
                            {
                                outfitShapekeys[outfitNumber] = new List<(string, SkinnedMeshRenderer, string)>();
                            }
                            outfitShapekeys[outfitNumber].Add((smrPath, smr, shapeName));
                        }
                    }
                }
            }

            // Get all unique outfit numbers from both GameObjects and shapekeys
            HashSet<string> allOutfitNumbers = new HashSet<string>();
            foreach (var key in outfitGameObjects.Keys) allOutfitNumbers.Add(key);
            foreach (var key in outfitShapekeys.Keys) allOutfitNumbers.Add(key);

            if (allOutfitNumbers.Count > 0)
            {
                // Create "All Off" animation clip
                AnimationClip allOffClip = new AnimationClip();
                allOffClip.name = "Outfit_None";

                // Set all outfit GameObjects to inactive
                foreach (var outfitEntry in outfitGameObjects)
                {
                    foreach (var obj in outfitEntry.Value)
                    {
                        string objPath = AnimationUtility.CalculateTransformPath(obj.transform, vrcAvatar.transform);
                        allOffClip.SetCurve(
                            objPath,
                            typeof(GameObject),
                            "m_IsActive",
                            new AnimationCurve(new Keyframe(0, 0))
                        );
                    }
                }

                // Set all outfit shapekeys to 0
                foreach (var shapeEntry in outfitShapekeys)
                {
                    foreach (var shapeData in shapeEntry.Value)
                    {
                        allOffClip.SetCurve(
                            shapeData.path,
                            typeof(SkinnedMeshRenderer),
                            "blendShape." + shapeData.shapeName,
                            new AnimationCurve(new Keyframe(0, 0))
                        );
                    }
                }

                SaveAnimation(allOffClip, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles/Outfits");

                // Create individual outfit animation clips
                foreach (string currentOutfit in allOutfitNumbers)
                {
                    AnimationClip outfitClip = new AnimationClip();
                    outfitClip.name = "Outfit" + currentOutfit;

                    // Handle GameObjects - activate current outfit, deactivate others
                    foreach (var outfitEntry in outfitGameObjects)
                    {
                        foreach (var obj in outfitEntry.Value)
                        {
                            string objPath = AnimationUtility.CalculateTransformPath(obj.transform, vrcAvatar.transform);
                            float activeValue = (outfitEntry.Key == currentOutfit) ? 1 : 0;
                            outfitClip.SetCurve(
                                objPath,
                                typeof(GameObject),
                                "m_IsActive",
                                new AnimationCurve(new Keyframe(0, activeValue))
                            );
                        }
                    }

                    // Handle shapekeys - set current outfit to 100, others to 0
                    foreach (var shapeEntry in outfitShapekeys)
                    {
                        foreach (var shapeData in shapeEntry.Value)
                        {
                            float shapeValue = (shapeEntry.Key == currentOutfit) ? 100 : 0;
                            outfitClip.SetCurve(
                                shapeData.path,
                                typeof(SkinnedMeshRenderer),
                                "blendShape." + shapeData.shapeName,
                                new AnimationCurve(new Keyframe(0, shapeValue))
                            );
                        }
                    }

                    SaveAnimation(outfitClip, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles/Outfits");
                }
            }
        }

        public static void CombineEmoteShapekeys(GameObject vrcAvatar)
        {
            // Create data structure to hold all emote blendshapes across all SMRs
            Dictionary<string, List<(string path, SkinnedMeshRenderer smr, int index)>> emoteBlendshapes =
                new Dictionary<string, List<(string path, SkinnedMeshRenderer smr, int index)>>();
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string blendPath = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);

                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    string blendShapeName = smr.sharedMesh.GetBlendShapeName(i);

                    if (blendShapeName.Contains("Emote"))
                    {
                        // Add this blendshape to our collection
                        if (!emoteBlendshapes.ContainsKey(blendShapeName))
                        {
                            emoteBlendshapes[blendShapeName] = new List<(string, SkinnedMeshRenderer, int)>();
                        }

                        emoteBlendshapes[blendShapeName].Add((blendPath, smr, i));
                    }
                }
            }
            if (emoteBlendshapes.Count > 0)
            {
                AnimationClip emoteIdle = new AnimationClip();
                emoteIdle.name = "Emote_Idle";
                foreach (var entry in emoteBlendshapes)
                {
                    foreach (var blendData in entry.Value)
                    {
                        emoteIdle.SetCurve(
                            blendData.path,
                            typeof(SkinnedMeshRenderer),
                            "blendShape." + entry.Key,
                            new AnimationCurve(new Keyframe(0, 0))
                        );
                    }
                }
                foreach (var emoteEntry in emoteBlendshapes)
                {
                    string emoteName = emoteEntry.Key;
                    AnimationClip emoteClip = new AnimationClip();
                    emoteClip.name = emoteName;
                    foreach (var blendData in emoteEntry.Value) // Set this emote to 100 in all places it appears
                    {
                        emoteClip.SetCurve(
                            blendData.path,
                            typeof(SkinnedMeshRenderer),
                            "blendShape." + emoteName,
                            new AnimationCurve(new Keyframe(0, 100))
                        );
                    }
                    foreach (var otherEntry in emoteBlendshapes) // Set all other emotes to 0
                    {
                        if (otherEntry.Key != emoteName)
                        {
                            foreach (var blendData in otherEntry.Value)
                            {
                                emoteClip.SetCurve(
                                    blendData.path,
                                    typeof(SkinnedMeshRenderer),
                                    "blendShape." + otherEntry.Key,
                                    new AnimationCurve(new Keyframe(0, 0))
                                );
                            }
                        }
                    }
                    SaveAnimation(emoteClip, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
                }
                SaveAnimation(emoteIdle, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
            }
        }

        public static void GenerateEmoteOverrideMenu(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            if (Directory.Exists(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes"))
            {
                var dir = new DirectoryInfo(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
                var emoteFiles = dir.GetFiles("*.anim");
                string paramName = "EmoteOverride";
                string layerName = "Emote Override Control";
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Int);
                var menu = CreateNewMenu("Menu_EmoteOverride");
                var fx = GetFxController(vrcAvatarDescriptor);
                DeleteExistingFxLayer(fx, layerName);
                fx.AddLayer(layerName);
                var fxLayers = fx.layers;
                var newLayer = fxLayers[fx.layers.Length - 1];
                newLayer.defaultWeight = 1f;
                var emptyState = newLayer.stateMachine.AddState("Empty", new Vector3(250, 220));
                emptyState.writeDefaultValues = true; //Reset defaults as we don't want to override anymore
                EditorUtility.SetDirty(emptyState);
                int emoteCount = emoteFiles.Length;
                if (emoteCount > 8) { emoteCount = 8; }
                for (int i = 0; i < emoteCount; i++)
                {
                    var emoteAsset = "Assets" + emoteFiles[i].FullName.Substring(Application.dataPath.Length);
                    var emote = AssetDatabase.LoadAssetAtPath(emoteAsset, typeof(AnimationClip)) as AnimationClip;
                    //var emote = Resources.Load<AnimationClip>(emoteFiles[i].FullName);

                    var emoteState = newLayer.stateMachine.AddState(emote.name, new Vector3(650, 20 + (i * 50)));
                    emoteState.writeDefaultValues = false;
                    emoteState.motion = emote;
                    EditorUtility.SetDirty(emoteState);

                    emptyState.AddTransition(emoteState);
                    emptyState.transitions[i].hasFixedDuration = true;
                    emptyState.transitions[i].duration = 0f;
                    emptyState.transitions[i].exitTime = 0f;
                    emptyState.transitions[i].hasExitTime = false;
                    emptyState.transitions[i].AddCondition(AnimatorConditionMode.Equals, i + 1, paramName);

                    emoteState.AddTransition(emptyState);
                    emoteState.transitions[0].hasFixedDuration = true;
                    emoteState.transitions[0].duration = 0f;
                    emoteState.transitions[0].exitTime = 0f;
                    emoteState.transitions[0].hasExitTime = false;
                    emoteState.transitions[0].AddCondition(AnimatorConditionMode.NotEqual, i + 1, paramName);

                    CreateMenuControl(menu, emote.name, VRCExpressionsMenu.Control.ControlType.Toggle, paramName, i + 1);
                }
                fx.layers = fxLayers; //fixes save for default weight for some reason
                //EditorUtility.SetDirty(fx);
                //AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        public static void GenerateBreastSlider(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            if (Directory.Exists(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys"))
            {
                var dir = new DirectoryInfo(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                var maxClipPath = dir + "/BreastSize MAX.anim";
                var minClipPath = dir + "/BreastSize MIN.anim";
                string paramName = "BreastSize";
                string layerName = "BreastSize";
                //CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);
                var menu = vrcAvatarDescriptor.expressionsMenu;
                var fx = GetFxController(vrcAvatarDescriptor);
                //DeleteExistingFxLayer(fx, layerName);
                //fx.AddLayer(layerName);
                //var fxLayers = fx.layers;
                //var newLayer = fxLayers[fx.layers.Length - 1];
                //newLayer.defaultWeight = 1f;
                var maxClip = AssetDatabase.LoadAssetAtPath(maxClipPath, typeof(AnimationClip)) as AnimationClip;
                var minClip = AssetDatabase.LoadAssetAtPath(minClipPath, typeof(AnimationClip)) as AnimationClip;
                ////Debug.Log(minClipPath);
                ////Debug.Log(maxClipPath);
                CreateBlendTreeLayer(vrcAvatarDescriptor, layerName, paramName, minClip, maxClip, 0);
                //CreateMenuControl(menu, "Breast Size", VRCExpressionsMenu.Control.ControlType.RadialPuppet, paramName, 1);

                //AssetDatabase.SaveAssets();
            }
        }

        public static void GenerateAnimationHueShaders(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            AnimationClip aClipLightLimitMin = new AnimationClip();
            AnimationClip aClipLightLimitMax = new AnimationClip();
            AnimationClip aClipGlitterLimitMin = new AnimationClip();
            AnimationClip aClipGlitterLimitMax = new AnimationClip();
            AnimationClip aClipOutlineOn = new AnimationClip();
            AnimationClip aClipOutlineOff = new AnimationClip();
            AnimationClip[] aClipMainHueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainHueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainSatMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainSatMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainBrightMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainBrightMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal0HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal0HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal1HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal1HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal2HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal2HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal3HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal3HueMax = new AnimationClip[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                aClipMainHueMin[i] = new AnimationClip();
                aClipMainHueMax[i] = new AnimationClip();
                aClipMainSatMin[i] = new AnimationClip();
                aClipMainSatMax[i] = new AnimationClip();
                aClipMainBrightMin[i] = new AnimationClip();
                aClipMainBrightMax[i] = new AnimationClip();
                aClipDecal0HueMin[i] = new AnimationClip();
                aClipDecal0HueMax[i] = new AnimationClip();
                aClipDecal1HueMin[i] = new AnimationClip();
                aClipDecal1HueMax[i] = new AnimationClip();
                aClipDecal2HueMin[i] = new AnimationClip();
                aClipDecal2HueMax[i] = new AnimationClip();
                aClipDecal3HueMin[i] = new AnimationClip();
                aClipDecal3HueMax[i] = new AnimationClip();
            }


            var menuPoi = CreateNewMenu("Menu_Poi");
            var menuMain = CreateNewMenu("Menu_Poi_Main");
            var menuDecal0 = CreateNewMenu("Menu_Poi_Decal0");
            var menuDecal1 = CreateNewMenu("Menu_Poi_Decal1");
            var menuDecal2 = CreateNewMenu("Menu_Poi_Decal2");
            var menuDecal3 = CreateNewMenu("Menu_Poi_Decal3");

            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                foreach (var mat in smr.sharedMaterials)
                {
                    //Debug.Log(mat.name);
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) })
                        .Where(property => property.FloatValue == 1);

                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        float floatValue = property.FloatValue;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        if (propertyName == "_EnableOutlines" && floatValue == 1)
                        {
                            aClipOutlineOff.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_EnableOutlines", new AnimationCurve(new Keyframe(0, 0)));
                            aClipOutlineOn.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_EnableOutlines", new AnimationCurve(new Keyframe(0, 1)));
                            aClipOutlineOff.name = "Outline Off";
                            aClipOutlineOn.name = "Outline On";
                        }
                        if (propertyName == "_LightingCapEnabled" && floatValue == 1)
                        {
                            aClipLightLimitMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_LightingMinLightBrightness", new AnimationCurve(new Keyframe(0, 0f)));
                            aClipLightLimitMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_LightingMinLightBrightness", new AnimationCurve(new Keyframe(0, 1f)));
                            aClipLightLimitMin.name = "LightLimit MIN";
                            aClipLightLimitMax.name = "LightLimit MAX";
                        }
                        if (propertyName == "_GlitterEnable" && floatValue == 1)
                        {
                            aClipGlitterLimitMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_GlitterBrightness", new AnimationCurve(new Keyframe(0, 0f)));
                            aClipGlitterLimitMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_GlitterBrightness", new AnimationCurve(new Keyframe(0, 2f)));
                            aClipGlitterLimitMin.name = "GlitterLimit MIN";
                            aClipGlitterLimitMax.name = "GlitterLimit MAX";
                        }
                        if (propertyName == "_MainHueShiftToggle" && floatValue == 1)
                        {
                            aClipMainHueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainHueShift", new AnimationCurve(new Keyframe(0, 0)));
                            aClipMainHueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainHueShift", new AnimationCurve(new Keyframe(0, 1)));
                            aClipMainSatMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_Saturation", new AnimationCurve(new Keyframe(0, -1)));
                            aClipMainSatMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_Saturation", new AnimationCurve(new Keyframe(0, 1)));
                            aClipMainBrightMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainBrightness", new AnimationCurve(new Keyframe(0, -1)));
                            aClipMainBrightMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainBrightness", new AnimationCurve(new Keyframe(0, 1)));

                            aClipMainHueMin[i].name = mat.name + "Main Hue" + " MIN";
                            aClipMainHueMax[i].name = mat.name + "Main Hue" + " MAX";
                            aClipMainSatMin[i].name = mat.name + "Main Saturation" + " MIN";
                            aClipMainSatMax[i].name = mat.name + "Main Saturation" + " MAX";
                            aClipMainBrightMin[i].name = mat.name + "Main Brightness" + " MIN";
                            aClipMainBrightMax[i].name = mat.name + "Main Brightness" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled" && floatValue == 1)
                        {
                            aClipDecal0HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal0HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal0HueMin[i].name = mat.name + "Decal0 Hue" + " MIN";
                            aClipDecal0HueMax[i].name = mat.name + "Decal0 Hue" + " MAX";

                        }
                        if (propertyName == "_DecalHueShiftEnabled1" && floatValue == 1)
                        {
                            aClipDecal1HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift1", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal1HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift1", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal1HueMin[i].name = mat.name + "Decal1 Hue" + " MIN";
                            aClipDecal1HueMax[i].name = mat.name + "Decal1 Hue" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled2" && floatValue == 1)
                        {
                            aClipDecal2HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift2", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal2HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift2", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal2HueMin[i].name = mat.name + "Decal2 Hue" + " MIN";
                            aClipDecal2HueMax[i].name = mat.name + "Decal2 Hue" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled3" && floatValue == 1)
                        {
                            aClipDecal3HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift3", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal3HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift3", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal3HueMin[i].name = mat.name + "Decal3 Hue" + " MIN";
                            aClipDecal3HueMax[i].name = mat.name + "Decal3 Hue" + " MAX";
                        }
                    }
                }//end foreach mat
            }// end foreach smr
            if (aClipOutlineOn != null)
            {
                var clip1 = SaveAnimation(aClipOutlineOff, savePath);
                var clip2 = SaveAnimation(aClipOutlineOn, savePath);
                CreateBlendTreeLayer(vrcAvatarDescriptor, "Poi Outline", "Outline", clip1, clip2, 0f);
                CreateMenuControl(menuPoi, "Outline", VRCExpressionsMenu.Control.ControlType.Toggle, "Outline");

            }
            if (aClipLightLimitMax != null)
            {
                var clip1 = SaveAnimation(aClipLightLimitMin, savePath);
                var clip2 = SaveAnimation(aClipLightLimitMax, savePath);
                CreateBlendTreeLayer(vrcAvatarDescriptor, "Poi LightLimit", "LightLimit", clip1, clip2, 0.1f);
                CreateMenuControl(menuPoi, "Min Light Limit", VRCExpressionsMenu.Control.ControlType.RadialPuppet, "LightLimit");
            }
            if (aClipGlitterLimitMax != null)
            {
                var clip1 = SaveAnimation(aClipGlitterLimitMin, savePath);
                var clip2 = SaveAnimation(aClipGlitterLimitMax, savePath);
                CreateBlendTreeLayer(vrcAvatarDescriptor, "Poi Glitter Limit", "GlitterLimit", clip1, clip2);
                CreateMenuControl(menuPoi, "Glitter Brightness", VRCExpressionsMenu.Control.ControlType.RadialPuppet, "GlitterLimit");
            }

            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                Debug.Log(mat.name);
                var menuMat = CreateNewMenu("Menu_Poi_" + mat.name);
                CreateMenuControl(menuPoi, mat.name + " Settings", VRCExpressionsMenu.Control.ControlType.SubMenu, menuMat);
                if (aClipMainHueMax != null)
                {
                    var clip1 = SaveAnimation(aClipMainHueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipMainHueMax[i], savePath);
                    var clip5 = SaveAnimation(aClipMainSatMin[i], savePath);
                    var clip6 = SaveAnimation(aClipMainSatMax[i], savePath);
                    var clip7 = SaveAnimation(aClipMainBrightMin[i], savePath);
                    var clip8 = SaveAnimation(aClipMainBrightMax[i], savePath);

                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Main Hue", mat.name + "MainHue", clip1, clip2);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Main Saturation", mat.name + "MainSat", clip5, clip6, 0.5f);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Main Brightness", mat.name + "MainBright", clip7, clip8, 0.5f);

                    CreateMenuControl(menuMat, "Main Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainHue");
                    CreateMenuControl(menuMat, "Main Saturation", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainSat");
                    CreateMenuControl(menuMat, "Main Brightness", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainBright");

                }
                if (aClipDecal0HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal0HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal0HueMax[i], savePath);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Decal0 Hue", mat.name + "Decal0Hue", clip1, clip2);
                    //CreateMenuControl(menuPoi, "Decal0 Settings", VRCExpressionsMenu.Control.ControlType.SubMenu, menuMat);
                    CreateMenuControl(menuMat, "Decal0 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal0Hue");
                }
                if (aClipDecal1HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal1HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal1HueMax[i], savePath);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Decal1 Hue", mat.name + "Decal1Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal1 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal1Hue");
                }
                if (aClipDecal2HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal2HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal2HueMax[i], savePath);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Decal2 Hue", mat.name + "Decal2Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal2 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal2Hue");
                }
                if (aClipDecal3HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal3HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal3HueMax[i], savePath);
                    CreateBlendTreeLayer(vrcAvatarDescriptor, mat.name + "Decal3 Hue", mat.name + "Decal3Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal3 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal3Hue");
                }
            }
        }

        public static void GenerateAnimationLightingModes(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            AnimationClip aClipLightingMode0 = new AnimationClip();
            AnimationClip aClipLightingMode1 = new AnimationClip();
            AnimationClip aClipLightingMode2 = new AnimationClip();


            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var mat in smr.sharedMaterials)
                {
                    //Debug.Log(mat.name);
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) })
                        .Where(property => property.FloatValue == 1);

                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        float floatValue = property.FloatValue;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        //Debug.Log(propertyName);
                        if (propertyName == "_LightingDirectionMode")
                        {
                            // Animation for Lighting Mode 0
                            aClipLightingMode0.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingDirectionMode",
                                new AnimationCurve(new Keyframe(0, 0)));
                            aClipLightingMode0.name = "LightingModePoi";

                            // Animation for Lighting Mode 1
                            aClipLightingMode1.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingDirectionMode",
                                new AnimationCurve(new Keyframe(0, 1)));
                            aClipLightingMode1.name = "LightingModeForcedLocal";

                            // Animation for Lighting Mode 2
                            aClipLightingMode2.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingDirectionMode",
                                new AnimationCurve(new Keyframe(0, 2)));
                            aClipLightingMode2.name = "LightingModeForcedWorld";
                        }
                    }
                } //end mat
            } // end smr
            SaveAnimation(aClipLightingMode0, savePath);
            SaveAnimation(aClipLightingMode1, savePath);
            SaveAnimation(aClipLightingMode2, savePath);
        }

        public static void GenerateAnimationLightingDirection(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            // Create animation clips for each axis of _LightingForcedDirection
            AnimationClip aClipLightingDirXMin = new AnimationClip();
            AnimationClip aClipLightingDirXMax = new AnimationClip();
            AnimationClip aClipLightingDirYMin = new AnimationClip();
            AnimationClip aClipLightingDirYMax = new AnimationClip();
            AnimationClip aClipLightingDirZMin = new AnimationClip();
            AnimationClip aClipLightingDirZMax = new AnimationClip();

            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var mat in smr.sharedMaterials)
                {
                    //Debug.Log(mat.name);
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) })
                        .Where(property => property.FloatValue == 1);

                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);

                        // X-axis animation (-1 to 0)
                        aClipLightingDirXMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.x",
                            new AnimationCurve(new Keyframe(0, -1)));
                        aClipLightingDirXMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.x",
                            new AnimationCurve(new Keyframe(0, 0)));
                        aClipLightingDirXMin.name = "Lighting Direction X MIN";
                        aClipLightingDirXMax.name = "Lighting Direction X MAX";

                        // Y-axis animation (-1 to 0)
                        aClipLightingDirYMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.y",
                            new AnimationCurve(new Keyframe(0, -1)));
                        aClipLightingDirYMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.y",
                            new AnimationCurve(new Keyframe(0, 0)));
                        aClipLightingDirYMin.name = "Lighting Direction Y MIN";
                        aClipLightingDirYMax.name = "Lighting Direction Y MAX";

                        // Z-axis animation (-1 to 0)
                        aClipLightingDirZMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.z",
                            new AnimationCurve(new Keyframe(0, -1)));
                        aClipLightingDirZMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingForcedDirection.z",
                            new AnimationCurve(new Keyframe(0, 0)));
                        aClipLightingDirZMin.name = "Lighting Direction Z MIN";
                        aClipLightingDirZMax.name = "Lighting Direction Z MAX";
                    }
                } //end mat
            }// end smr
            SaveAnimation(aClipLightingDirXMin, savePath);
            SaveAnimation(aClipLightingDirXMax, savePath);
            SaveAnimation(aClipLightingDirYMin, savePath);
            SaveAnimation(aClipLightingDirYMax, savePath);
            SaveAnimation(aClipLightingDirZMin, savePath);
            SaveAnimation(aClipLightingDirZMax, savePath);
        }

        public static void GenerateAnimationShadingCutoff(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            // Create animation clips for _LightingAdditiveGradientStart and _LightingAdditiveGradientEnd
            AnimationClip aClipLightingCutoffMin = new AnimationClip();
            AnimationClip aClipLightingCutoffMax = new AnimationClip();

            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var mat in smr.sharedMaterials)
                {
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) });


                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        //Debug.Log(propertyName);
                        if (propertyName == "_LightingAdditiveGradientStart")
                        {
                            aClipLightingCutoffMin.name = "Shading Cutoff MIN";
                            aClipLightingCutoffMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._BaseColor_Step",
                                new AnimationCurve(new Keyframe(0, 0f)));
                            aClipLightingCutoffMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingAdditiveGradientStart",
                                new AnimationCurve(new Keyframe(0, 0f)));
                            aClipLightingCutoffMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingAdditiveGradientEnd",
                                new AnimationCurve(new Keyframe(0, 0.01f)));
                            aClipLightingCutoffMax.name = "Shading Cutoff MAX";
                            aClipLightingCutoffMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._BaseColor_Step",
                                new AnimationCurve(new Keyframe(0, 1f)));
                            aClipLightingCutoffMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingAdditiveGradientStart",
                                new AnimationCurve(new Keyframe(0, 0.99f)));
                            aClipLightingCutoffMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._LightingAdditiveGradientEnd",
                                new AnimationCurve(new Keyframe(0, 1f)));
                        }
                    }
                }//end mat
            }//end smr
            SaveAnimation(aClipLightingCutoffMin, savePath);
            SaveAnimation(aClipLightingCutoffMax, savePath);
        }

        public static void GeneratePoiRimLightCutoff(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            // Create animation clips for _LightingAdditiveGradientStart and _LightingAdditiveGradientEnd
            AnimationClip aClipRimLightMin = new AnimationClip();
            AnimationClip aClipRimLightMax = new AnimationClip();

            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var mat in smr.sharedMaterials)
                {
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) });


                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        //Debug.Log(propertyName);
                        if (propertyName == "_RimWidth")
                        {
                            aClipRimLightMin.name = "Rim Light MIN";
                            aClipRimLightMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material._RimWidth",
                                new AnimationCurve(new Keyframe(0, 0f)));
                            aClipRimLightMax.name = "Rim Light MAX";
                            aClipRimLightMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material._RimWidth",
                                new AnimationCurve(new Keyframe(0, 1f)));
                        }
                    }
                }//end mat
            }//end smr
            SaveAnimation(aClipRimLightMin, savePath);
            SaveAnimation(aClipRimLightMax, savePath);
        }

        public static void GeneratePoiMenus(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            string poiPath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            if (Directory.Exists(poiPath))
            {
                var dir = new DirectoryInfo(poiPath);
                //var menu = CreateNewMenu("Menu_PoiLighting");
                var poiFiles = dir.GetFiles("*.anim");
                string poiModeParamName = "PoiMode";
                string poiModeLayerName = "Poi Lighting Mode";
                string poiDirectionParamName = "PoiDirection";
                string poiDirectionLayerName = "Poi Lighting Direction";
                string poiCutoffParamName = "PoiCutoff";
                string poiCutoffLayerName = "Poi Lighting Cutoff";
                string poiRimLightParamName = "PoiRimLight";
                string poiRimLightLayerName = "Poi Rim Light";
                CreateFxParameter(vrcAvatarDescriptor, poiModeParamName, AnimatorControllerParameterType.Int);
                CreateFxParameter(vrcAvatarDescriptor, poiDirectionLayerName, AnimatorControllerParameterType.Float);
                //CreateFxParameter(vrcAvatarDescriptor, poiCutoffParamName, AnimatorControllerParameterType.Float);
                var fx = GetFxController(vrcAvatarDescriptor);
                //DeleteExistingFxLayer(fx, poiModeLayerName);
                //DeleteExistingFxLayer(fx, poiDirectionLayerName);
                DeleteExistingFxLayer(fx, poiCutoffLayerName);
                AnimationClip shadingCutoffMin = new AnimationClip();
                AnimationClip shadingCutoffMax = new AnimationClip();
                AnimationClip rimLightMin = new AnimationClip();
                AnimationClip rimLightMax = new AnimationClip();
                foreach (var file in poiFiles)
                {
                    if (file.Name == "Shading Cutoff MIN.anim")
                    {
                        shadingCutoffMin = AssetDatabase.LoadAssetAtPath<AnimationClip>(poiPath + "/" + file.Name);
                    }
                    else if (file.Name == "Shading Cutoff MAX.anim")
                    {
                        shadingCutoffMax = AssetDatabase.LoadAssetAtPath<AnimationClip>(poiPath + "/" + file.Name);
                    }
                    else if (file.Name == "Rim Light MIN.anim")
                    {
                        rimLightMin = AssetDatabase.LoadAssetAtPath<AnimationClip>(poiPath + "/" + file.Name);
                    }
                    else if (file.Name == "Rim Light MAX.anim")
                    {
                        rimLightMax = AssetDatabase.LoadAssetAtPath<AnimationClip>(poiPath + "/" + file.Name);
                    }
                }
                if (shadingCutoffMin != null && shadingCutoffMax != null)
                    CreateBlendTreeLayer(vrcAvatarDescriptor, poiCutoffLayerName, poiCutoffParamName, shadingCutoffMin, shadingCutoffMax, 0.5f);
                if (rimLightMin != null && rimLightMax != null)
                    CreateBlendTreeLayer(vrcAvatarDescriptor, poiRimLightLayerName, poiRimLightParamName, rimLightMin, rimLightMax, 0.6f);
            }
        }

        public static List<AnimationClip> GetAllGeneratedAnimationClips()
        {
            string folderPath = GetCurrentSceneRootPath() + "/Animations/Generated";
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new string[] { folderPath });
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                clips.Add(clip);
            }
            if (clips.Count == 0)
            {
                return null;
            }
            else
            {
                return clips;
            }
        }

        public static void CleanupUnusedGeneratedAnimations()
        {
            AnimatorController[] aControllers = Resources.FindObjectsOfTypeAll<AnimatorController>();
            List<AnimationClip> generatedClips = GetAllGeneratedAnimationClips();
            foreach (AnimatorController aController in aControllers)
            {
                AnimationClip[] controllerClips = aController.animationClips;
                foreach (AnimationClip controllerClip in controllerClips)
                {
                    if (generatedClips.Contains(controllerClip))
                    {
                        //Debug.Log(controllerClip.name + " does exist in " + aController.name);
                        generatedClips.Remove(controllerClip); //clip is used, remove from list
                    }
                }
                //Debug.Log(generatedClips.Count);
            }
            foreach (AnimationClip unusedClip in generatedClips)
            {
                var unusedClipPath = AssetDatabase.GetAssetPath(unusedClip);
                AssetDatabase.DeleteAsset(unusedClipPath);
            }

        }

        public static AnimationClip SaveAnimation(AnimationClip anim, string savePath)
        {
            if (anim != null && anim.name != "")
            {
                if (!(AssetDatabase.IsValidFolder(savePath)))
                    Directory.CreateDirectory(savePath);
                savePath = savePath + "/" + anim.name + ".anim";
                Helper.CreateOrReplaceAsset<AnimationClip>(anim, savePath);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
                return clip;
            }
            return null;
        }


        public static AnimationCurve LinearAnimationCurve()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
            curve.preWrapMode = WrapMode.Default;
            curve.postWrapMode = WrapMode.Default;

            return curve;
        }

    }
}
