using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using static Shadster.AvatarTools.Helper;

namespace Shadster.AvatarTools
{
    public class Scenes : Editor
    {

        public static void CleanUp()
        {
            CleanUp(false);
        }

        public static void CleanUp(bool includefixes)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string originalScenePath = SceneManager.GetActiveScene().path;
            string path = originalScenePath.Substring(0, originalScenePath.LastIndexOf("/"));
            string name = path.Substring(path.IndexOf("/") + 1);

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { path });
            DeleteFile(Application.dataPath + "/" + name + "/blender.txt");

            foreach (string guid in guids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(scenePath);
                Debug.Log("Opening scene... " + scene.name + ": " + scenePath);

                //Update Scene
                var startPlayModePrefabExists = Checkboxes.GetStartPlayModeInSceneView();
                var ignorePhysImmobilePrefabExists = Checkboxes.GetIgnorePhysImmobile();
                var testPhysbonesPrefabExists = Checkboxes.GetTestPhysbones();

                //Cleanup
                Debug.Log("Cleaning scene... ");
                var avatarDescriptors = Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>();
                foreach (var avatarDescriptor in avatarDescriptors)
                {
                    Common.ClearAvatarBlueprintID(avatarDescriptor.gameObject);
                    //Include Fixes?
                    if (includefixes)
                    {
                        Debug.Log("Implementing fixes...");
                        Common.FixAvatarDescriptor(avatarDescriptor);
                        Common.SetAvatarMeshBounds(avatarDescriptor.gameObject);
                        Common.SetAvatarAnchorProbes(avatarDescriptor.gameObject);
                        Common.FixMissingSFXPrefab(avatarDescriptor.gameObject);
                    }
                }
                if (startPlayModePrefabExists) { Checkboxes.SetStartPlayModeInSceneView(false); }
                if (ignorePhysImmobilePrefabExists) { Checkboxes.SetIgnorePhysImmobile(false); }
                if (testPhysbonesPrefabExists) { Checkboxes.SetTestPhysbones(false); }
                if (GameObject.Find("GestureManager") != null)
                {
                    var temp = GameObject.Find("GestureManager");
                    DestroyImmediate(temp, true);
                }

                // Save the updated scene
                Debug.Log("Saving scene... ");
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(originalScenePath);
            AssetDatabase.Refresh();
        }

        public static bool WholesomeExists()
        {
            if (!(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID("Assets/!Wholesome/SPS Configurator/2.0.11/SFX/SFX.prefab"))))
            {
                return true;
            }
            return false;
        }

        public static void Export()
        {
            //Setup
            List<string> paths = new List<string>();
            //Export
            var scenePath = SceneManager.GetActiveScene().path;
            var sceneDirectory = Path.GetDirectoryName(scenePath);
            var lastFolderName = Path.GetFileName(sceneDirectory);
            paths.Add(scenePath.Substring(0, scenePath.LastIndexOf("/")));
            var name = lastFolderName;
            if (GogoLoco.GogoLocoExist())
            {
                paths.Add("Assets/GoGo");
            }
            if (WholesomeExists())
            {
                paths.Add("Assets/!Wholesome");
            }
            AssetDatabase.ExportPackage(paths.ToArray(), name + ".unitypackage", ExportPackageOptions.Recurse);
            //Open
            EditorUtility.RevealInFinder(Application.dataPath);
        }

        public static float GetAvatarHeight(GameObject vrcAvatar)
        {
            Animator anim = vrcAvatar.GetComponent<Animator>();
            Transform shoulderL = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            float height = shoulderL.position.y - vrcAvatar.transform.position.y;
            Debug.Log(height);
            return height;
        }

        public static void CloneAndRegenerateGUIDs(string contextPath)
        {
            Dictionary<string, string> guidDictionary = new Dictionary<string, string>();
            string clonedPath = contextPath + " 1";
            string contextName = contextPath.Split('/')[1];
            if (AssetDatabase.IsValidFolder(clonedPath))
            {
                Debug.LogError("Cloned Folder already exists!" + clonedPath);
                return;
            }
            string[] assetPaths = Directory.GetFiles(contextPath, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta")) //&& !path.EndsWith(".fbx") && !path.EndsWith(".wav"))
            .ToArray();

            foreach (string originalAssetPath in assetPaths)
            {
                string assetPath = originalAssetPath.Replace("\\", "/");
                string relativePath = assetPath.Substring(contextPath.Length + 1);
                string clonedAssetPath = Path.Combine(clonedPath, relativePath).Replace("\\", "/");
                string directoryPath = Path.GetDirectoryName(clonedAssetPath);
                Directory.CreateDirectory(directoryPath);
                if (AssetDatabase.CopyAsset(assetPath, clonedAssetPath))
                {
                    guidDictionary[AssetDatabase.AssetPathToGUID(assetPath)] = AssetDatabase.AssetPathToGUID(clonedAssetPath);
                }
            }
            AssetDatabase.Refresh();
            string[] clonedAssetPaths = Directory.GetFiles(clonedPath, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta")) //&& !path.EndsWith(".fbx") && !path.EndsWith(".wav"))
            .ToArray();
            foreach (string clonedAsset in clonedAssetPaths)
            {
                UpdateGuidsInFile(clonedAsset, guidDictionary);
            }
            AssetDatabase.Refresh();
        }

        public static void UpdateGuidsInFile(string path, Dictionary<string, string> guidDictionary)
        {
            if (!File.Exists(path) || String.IsNullOrEmpty(path))
            {
                Debug.LogError($"File at {path} does not exist or is NULL/EMPTY");
                return;
            }

            string[] lines = File.ReadAllLines(path);

            if (lines.Length > 0)
            {
                if (lines[0].Contains("%YAML"))
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        foreach (var pair in guidDictionary)
                        {
                            if (lines[i].Contains(pair.Key))
                            {
                                // Replace the old GUID with the new one
                                lines[i] = lines[i].Replace(pair.Key, pair.Value);
                            }
                        }
                    }
                    File.WriteAllLines(path, lines);
                }
            }
        }

    }
}