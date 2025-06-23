using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using VRC.SDKBase.Editor.Api;

namespace Shadster.AvatarTools
{
    public class Materials
    {
        public static Material[] GetUniqueMaterials(GameObject obj)
        {
            if (obj != null)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                HashSet<Material> uniqueMaterials = new HashSet<Material>();
                foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    foreach (Material material in materials)
                    {
                        uniqueMaterials.Add(material);
                    }
                }
                Material[] uniqueMaterialArray = new Material[uniqueMaterials.Count];
                uniqueMaterials.CopyTo(uniqueMaterialArray);

                //foreach (Material mat in uniqueMaterialArray) //DEBUG
                //{
                //    Debug.Log(mat.name);
                //}
                return uniqueMaterialArray;
            }
            return new Material[0];
        }

        public static string GetMaterialPath(Material material)
        {
            if (material == null) return null;
            var path = AssetDatabase.GetAssetPath(material);
            //Debug.Log("GetMaterialPath " + path);
            return path;
            
        }

        public static Material FindOrCreateQuestMaterial(Material oriMat)
        {
            if (oriMat == null) return null;

            string originalPath = GetMaterialPath(oriMat);
            if (string.IsNullOrEmpty(originalPath)) return null;

            string directory = Path.GetDirectoryName(originalPath);
            string newMatName = oriMat.name + " 1";
            string newMatPath = Path.Combine(directory, newMatName + ".mat").Replace("\\", "/");

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(newMatPath);

            if (existing != null)
            {
                Debug.Log("Found existing material" + existing.name);
                return existing;
            }

            Material newMat = new Material(Shader.Find("VRChat/Mobile/Toon Standard"));
            newMat.name = newMatName;
            SetupNewToonStandardProperties(newMat);
            TransferAlbedoTexture(oriMat, newMat);
            Debug.Log("Created New Material " + newMat.name);
            AssetDatabase.CreateAsset(newMat, newMatPath);
            AssetDatabase.SaveAssets();
            return newMat;
        }

        public static void TransferAlbedoTexture(Material matA, Material matB)
        {
            if (matA == null || matB == null)
            {
                Debug.LogError("No valid given material");
                return;
            }
            
            Texture texA = matA.GetTexture("_MainTex");
            if (texA == null)
            {
                Debug.LogWarning("No Albedo texture found in " + matA.name);
            }
            else
            {
                matB.SetTexture("_MainTex", texA);
            }
        }

        public static void CreateMaterialsToQuestToon(GameObject obj)
        {
            var materials = new List<Material>();
            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                materials.AddRange(renderer.sharedMaterials);
                Material[] newMaterials = new Material[materials.Count];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = FindOrCreateQuestMaterial(materials[i]);
                }
                renderer.sharedMaterials = newMaterials;
            }
        }

        public static void CreateAvatarMaterialsToQuestToon(GameObject vrcAvatar)
        {
            List<GameObject> objs = Helper.GetRenderersInChildren(vrcAvatar);
            foreach (GameObject obj in objs)
            {
                Undo.RecordObject(obj, "Create Avatar Materials To Quest Toon Standard");
                CreateMaterialsToQuestToon(obj);
                EditorUtility.SetDirty(obj);
            }
        }

        public static void ConvertAvatarToonLitToToonStandard(GameObject vrcAvatar)
        {
            var mats = new List<Material>();
            int convertedCount = 0;
            List<GameObject> objs = Helper.GetRenderersInChildren(vrcAvatar);
            foreach (GameObject obj in objs)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    mats.AddRange(renderer.sharedMaterials);
                    foreach (var mat in mats)
                    {
                        if (mat == null) continue;
                        if (mat.shader.name == "VRChat/Mobile/Toon Lit")
                        {
                            Undo.RecordObject(mat, "Convert Avatar Toon Lit to Toon Standard");
                            mat.shader = Shader.Find("VRChat/Mobile/Toon Standard");
                            SetupNewToonStandardProperties(mat);
                            EditorUtility.SetDirty(mat);
                            convertedCount++;
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Converted {convertedCount} materials to VRChat/Mobile/Toon Standard.");
        }

        public static void ConvertAllToonLitToToonStandard()
        {
            int convertedCount = 0;

            // Find all materials in the project
            string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");

            foreach (string guid in materialGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

                if (mat != null && mat.shader != null && mat.shader.name == "VRChat/Mobile/Toon Lit")
                {
                    Undo.RecordObject(mat, "Convert All Toon Lit to Toon Standard");
                    mat.shader = Shader.Find("VRChat/Mobile/Toon Standard");
                    SetupNewToonStandardProperties(mat);
                    EditorUtility.SetDirty(mat);
                    convertedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Converted {convertedCount} materials to VRChat/Mobile/Toon Standard across the entire project.");
        }

        public static void UpdateExistingToonStandard(GameObject vrcAvatar)
        {
            var mats = new List<Material>();
            int convertedCount = 0;
            List<GameObject> objs = Helper.GetRenderersInChildren(vrcAvatar);
            foreach (GameObject obj in objs)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    mats.AddRange(renderer.sharedMaterials);
                    foreach (var mat in mats)
                    {
                        if (mat == null) continue;
                        if (mat.shader.name == "VRChat/Mobile/Toon Standard")
                        {
                            Undo.RecordObject(mat, "Update Toon Standard");
                            SetupNewToonStandardProperties(mat);
                            EditorUtility.SetDirty(mat);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        static void SetupNewToonStandardProperties(Material mat)
        {
            if (mat == null) return;
            mat.EnableKeyword("USE_RIMLIGHT"); //Hope this works

            mat.SetFloat("_ShadowBoost", 0.5f);
            mat.SetFloat("_ShadowAlbedo", 0.5f);
            mat.SetFloat("_MinBrightness", 0.1f);
            mat.SetColor("_RimColor", new Color(0xDD / 255f, 0xDD / 255f, 0xDD / 255f));
            mat.SetFloat("_RimAlbedoTint", 1f);
            mat.SetFloat("_RimIntensity", 0.4f);
            mat.SetFloat("_RimRange", 0.2f);
            mat.SetFloat("_RimSharpness", 0.1f);
            mat.SetTexture("_EmissionMap", null);
            mat.SetColor("_EmissionColor", Color.black);
            mat.SetFloat("_EmissionStrength", 0.65f);
            Texture2D rampTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.vrchat.base/Runtime/VRCSDK/Sample Assets/Shaders/Mobile/ToonStandard/Resources/VRChat/ShadowRampToon2Band.png");
            //ermahgurd help me plz
            mat.SetTexture("_Ramp", rampTex);
            EditorUtility.SetDirty(mat);
        }


    }
}
