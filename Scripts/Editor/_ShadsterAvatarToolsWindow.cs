 //Made by Shadsterwolf, some code inspired by the VRCSDK, Av3Creator, and PumpkinTools
using Shadster.AvatarTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static Shadster.AvatarTools.Helper;
using static Shadster.AvatarTools.Menus;
using static Shadster.AvatarTools.Params;
using static Shadster.AvatarTools.Checkboxes;
using static Shadster.AvatarTools.Common;
using static Shadster.AvatarTools.Animation;
using static Shadster.AvatarTools.AnimatorControl;
using static Shadster.AvatarTools.Textures;
using static Shadster.AvatarTools.Bones;
using static Shadster.AvatarTools.GogoLoco;
using static Shadster.AvatarTools.Scenes;
using static Shadster.AvatarTools.Setup;
using static Shadster.AvatarTools.Materials;


namespace Shadster.AvatarTools.ShadsterAvatarToolsWindow
{
    [System.Serializable]
    public class _ShadsterAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static _ShadsterAvatarToolsWindow _tools;

        static EditorWindow toolWindow;
        Vector2 scrollPos;
        private bool startInSceneView;
        private bool useExperimentalPlayMode;
        private bool ignorePhysImmobile;
        private bool testPhysbones;
        private string toolVersion = "1.2.0";

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMenu;


        [SerializeReference] private AnimationClip clipA;
        [SerializeReference] private AnimationClip clipB;
        [SerializeReference] private string layerName;
        [SerializeReference] private string paramName;
        [SerializeReference] private int selectedParamType = 0;
        [SerializeReference] private int selectedControlType = 0;
        [SerializeReference] private string menuControlName;
        [SerializeReference] private bool createControlChecked;
        [SerializeReference] private string version;
        [SerializeReference] private string author;
        [SerializeReference] private string blenderZipPath;
        [SerializeReference] private string lastContext;

        [SerializeReference] private bool commonFoldView;
        [SerializeReference] private bool bonesFoldView;
        [SerializeReference] private bool animationFoldView;
        [SerializeReference] private bool materialFoldView;
        [SerializeReference] private bool texturesFoldView;
        [SerializeReference] private bool scenesFoldView;
        [SerializeReference] private bool subScenesFoldView;
        [SerializeReference] private bool gogoFoldView;

        [SerializeReference] private Transform breastBoneL;
        [SerializeReference] private Transform breastBoneR;
        [SerializeReference] private Transform buttBoneL;
        [SerializeReference] private Transform buttBoneR;
        [SerializeReference] private Transform earBoneR;
        [SerializeReference] private Transform earBoneL;
        [SerializeReference] private Transform tailBone;

        public static _ShadsterAvatarToolsWindow ToolsWindow
        {
            get
            {
                if (!_tools)
                    _tools = FindObjectOfType(typeof(_ShadsterAvatarToolsWindow)) as _ShadsterAvatarToolsWindow ?? CreateInstance<_ShadsterAvatarToolsWindow>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }

        [MenuItem("ShadsterWolf/Shadster Tools", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow<_ShadsterAvatarToolsWindow>();
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("Shadster Tools");
                toolWindow.minSize = new Vector2(400, 200);
            }
            toolWindow.Show();
        }

        private void OnInspectorUpdate()
        {
            if (vrcAvatar != null && vrcAvatarDescriptor == null) //because play mode likes to **** with me and clear the descriptor
                vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            useExperimentalPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            startInSceneView = GetStartPlayModeInSceneView();
            ignorePhysImmobile = GetIgnorePhysImmobile();
        }        

        public void ResetAll()
        {
            vrcAvatarDescriptor = null;
            vrcAvatar = null;
            vrcMenu = null;
            vrcParameters = null;

            breastBoneL = null;
            breastBoneR = null;
            buttBoneL = null;
            buttBoneR = null;
            clipA = null;
            clipB = null;
            layerName = "";
            paramName = "";
            selectedParamType = 0;
        }

        public void AutoDetect()
        {
            vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
            vrcAvatar = vrcAvatarDescriptor.gameObject;
            vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            
            breastBoneL = GetAvatarBone(vrcAvatar, "Breast", "_L");
            breastBoneR = GetAvatarBone(vrcAvatar, "Breast", "_R");
            buttBoneL = GetAvatarBone(vrcAvatar, "Butt", "_L");
            buttBoneR = GetAvatarBone(vrcAvatar, "Butt", "_R");
        }

        public bool Prompt(string banner)
        {
            bool result = EditorUtility.DisplayDialog(banner, "Are you sure?", "Yes", "No");
            if (result)
            {
                return true;
            }
            return false;
        }

        private static string ReadTxtFile(string filePath)
        {
            string txt = "";
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    txt = reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading file: " + e.Message);
            }
            return txt;
        }

        private static void SaveStringToFile(string content, string filePath)
        {
            try
            {
                // Write the content to the file
                File.WriteAllText(filePath, content);
                Debug.Log(content + " saved to " + filePath + " successfully.");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("Error saving string to file: " + e.Message);
            }
        }

        

        private void ZipFiles(string[] paths, string zipPath)
        {
            try
            {
                // Create a new ZIP archive
                using (FileStream zipToCreate = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                    {
                        foreach (var sourceFilePath in paths)
                        {
                            // Get the file name for the entry
                            string entryFileName = Path.GetFileName(sourceFilePath);

                            // Create a new entry in the zip archive
                            ZipArchiveEntry entry = archive.CreateEntry(entryFileName);

                            // Open the source file and copy its content to the entry in the zip archive
                            using (Stream entryStream = entry.Open())
                            {
                                using (FileStream sourceFileStream = File.OpenRead(sourceFilePath))
                                {
                                    sourceFileStream.CopyTo(entryStream);
                                }
                            }

                            Debug.Log("File zipped successfully: " + sourceFilePath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error zipping files: " + e.Message);
            }
        }

        public void DrawCommonFoldout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                //Color currentColor = GUI.color;
                //GUI.color = new Color(0.4f, 1f, 0.4f);
                GUI.Box(subBoxRect, "");

                //GUI.color = currentColor;
                commonFoldView = EditorGUILayout.Foldout(commonFoldView, "Common");
                if (commonFoldView)
                {
                    if (GUILayout.Button("Fix Avatar Descriptor (Missing Face/Body)", GUILayout.Height(24)))
                    {
                        FixAvatarDescriptor(vrcAvatarDescriptor);
                    }
                    if (GUILayout.Button("Set All Mesh Bounds to 2.5sq", GUILayout.Height(24)))
                    {
                        SetAvatarMeshBounds(vrcAvatar);
                    }
                    if (GUILayout.Button("Set All Anchor Probes to Hip", GUILayout.Height(24)))
                    {
                        SetAvatarAnchorProbes(vrcAvatar);
                    }
                    if (GUILayout.Button("Clear Avatar Blueprint ID", GUILayout.Height(24)))
                    {
                        ClearAvatarBlueprintID(vrcAvatar);
                    }
                    if (GUILayout.Button("Fix Missing Wholesome SFX prefab", GUILayout.Height(24)))
                    {
                        FixMissingSFXPrefab(vrcAvatar);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        } //End Common Foldout

        public void DrawTexturesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                texturesFoldView = EditorGUILayout.Foldout(texturesFoldView, "Textures");
                if (texturesFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1.6f, 0.6f, 0.6f);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Enable All Mip Maps", GUILayout.Height(24)))
                        {
                            UpdateAvatarTextureMipMaps(vrcAvatar, true);
                        }
                        if (GUILayout.Button("Disable All Mip Maps", GUILayout.Height(24)))
                        {
                            if (Prompt("Disabling All Mip Maps is useful only if your avatar has texture seam issues! \n This will force others to render your avatar at full vram despite being far away..."))
                            {
                                UpdateAvatarTextureMipMaps(vrcAvatar, false);
                            }
                        }
                    }
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set All 256px", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesMaxSize(vrcAvatar, 256);
                        }
                        if (GUILayout.Button("Set All 512px", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesMaxSize(vrcAvatar, 512);
                        }
                        if (GUILayout.Button("Set All 1k", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesMaxSize(vrcAvatar, 1024);
                        }
                        if (GUILayout.Button("Set All 2k", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesMaxSize(vrcAvatar, 2048);
                        }
                        if (GUILayout.Button("Set All 4k", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesMaxSize(vrcAvatar, 4096);
                        }
                    }

                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set Compression LQ", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.CompressedLQ);
                        }
                        if (GUILayout.Button("Set Compression NQ", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.Compressed);
                        }
                        if (GUILayout.Button("Set Compression HQ", GUILayout.Height(24)))
                        {
                            SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.CompressedHQ);
                        }

                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawMaterialFoldout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0f, 1.5f, 1.5f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                materialFoldView = EditorGUILayout.Foldout(materialFoldView, "Material");
                if (materialFoldView)
                {
                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0f, 1.8f, 1.8f);
                    List<Material> materials = new List<Material>();
                    materials = GetUniqueMaterials(vrcAvatar).ToList();


                    if (GUILayout.Button("Convert PC Materials to Quest Toon Standard", GUILayout.Height(24)))
                    {
                        CreateAvatarMaterialsToQuestToon(vrcAvatar);
                        //GenerateAnimationLightingModes(vrcAvatar);
                        //GenerateAnimationLightingDirection(vrcAvatar);
                        //GenerateAnimationShadingCutoff(vrcAvatar);
                        //GeneratePoiRimLightCutoff(vrcAvatar);
                        //GeneratePoiMenus(vrcAvatarDescriptor);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(new GUIContent("Update Avatar Quest Toon Lit to Toon Standard", "Update selected Avatar's shader ToonLit to Toon Standard"), GUILayout.Height(24)))
                        {
                            ConvertAvatarToonLitToToonStandard(vrcAvatar);
                        }
                        
                        if (GUILayout.Button(new GUIContent("Update ALL Quest Toon Lit to Toon Standard", "Update ENTIRE UNITY PROJECT ToonLit to Toon Standard"), GUILayout.Height(24)))
                        {
                            if (Prompt("This will update the ENTIRE UNITY PROJECT that has \'ToonLit\' to \'ToonStandard\'! "))
                            {
                                ConvertAllToonLitToToonStandard();
                            }
                        }
                    }
                    //if (GUILayout.Button("Update Toon Standard", GUILayout.Height(24)))
                    //{
                    //    UpdateExistingToonStandard(vrcAvatar);
                    //}
                    GUI.backgroundColor = currentBackgroundColor;
                }
            }
            EditorGUILayout.EndVertical();
        }

        public void DrawBonesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.4f, 1f, 0.4f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                bonesFoldView = EditorGUILayout.Foldout(bonesFoldView, "Bones");
                if (bonesFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 1.5f, 0.6f);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set Default Humanoid Arm Twist", GUILayout.Height(24)))
                        {
                            UpdateHumanoidArmTwist(vrcAvatar, 0.5f, 0.5f);
                        }
                        if (GUILayout.Button("Reduce Humanoid Arm Twist", GUILayout.Height(24)))
                        {
                            UpdateHumanoidArmTwist(vrcAvatar, 0.2f, 0.2f);
                        }
                    }
                    //EditorGUILayout.LabelField("Breast Bones");
                    //using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    //{
                    //    breastBoneL = (Transform)EditorGUILayout.ObjectField(breastBoneL, typeof(Transform), true, GUILayout.Height(24));
                    //    breastBoneR = (Transform)EditorGUILayout.ObjectField(breastBoneR, typeof(Transform), true, GUILayout.Height(24));
                    //}
                    //EditorGUILayout.LabelField("Butt Bones");
                    //using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    //{
                    //    buttBoneL = (Transform)EditorGUILayout.ObjectField(buttBoneL, typeof(Transform), true, GUILayout.Height(24));
                    //    buttBoneR = (Transform)EditorGUILayout.ObjectField(buttBoneR, typeof(Transform), true, GUILayout.Height(24));
                    //}
                    //if (GUILayout.Button("Auto Add PhysBones", GUILayout.Height(24)))
                    //{
                    //    ShadstersAvatarTools.AddPhysBones(breastBoneL);
                    //    ShadstersAvatarTools.AddPhysBones(breastBoneR);
                    //    ShadstersAvatarTools.AddButtPhysBones(buttBoneL);
                    //    ShadstersAvatarTools.AddButtPhysBones(buttBoneR);
                    //}
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Move PhysBones from Armature", GUILayout.Height(24)))
                        {
                            MovePhysBonesFromArmature(vrcAvatar);
                        }
                        if (GUILayout.Button("Move Colliders from Armature", GUILayout.Height(24)))
                        {
                            MovePhysCollidersFromArmature(vrcAvatar);
                        }
                    }
                    if (GUILayout.Button("Set All Grab Movement to 1", GUILayout.Height(24)))
                    {
                        SetAllGrabMovement(vrcAvatar);
                    }
                    if (GUILayout.Button("Delete End Bones", GUILayout.Height(24)))
                    {
                        DeleteEndBones(vrcAvatar);
                    }
                    if (GUILayout.Button("Repair Missing PhysBone Transforms", GUILayout.Height(24)))
                    {
                        RepairMissingPhysboneTransforms(vrcAvatar);
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawAnimationFoldout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.4f, 0.4f, 1f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                animationFoldView = EditorGUILayout.Foldout(animationFoldView, "Animation");
                if (animationFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 1.6f);


                    if (GUILayout.Button("Generate Animation Render Toggles", GUILayout.Height(24)))
                    {
                        GenerateAnimationRenderToggles(vrcAvatar);
                        CombineOutfitShapekeys(vrcAvatar);
                    }
                    if (GUILayout.Button("Generate Animation Shapekeys", GUILayout.Height(24)))
                    {
                        CombineAnimationShapekeys(vrcAvatar);
                        CombineEmoteShapekeys(vrcAvatar);
                    }
                    if (GUILayout.Button("Generate Animation Physbone Colliders", GUILayout.Height(24)))
                    {
                        if (Prompt("Generate Animation Physbones, it will add a FX Layer"))
                        {
                            GenerateAnimationPhysbones(vrcAvatar, vrcAvatarDescriptor);
                        }
                    }
                    //if (GUILayout.Button("Generate Animation Poi Hues", GUILayout.Height(24)))
                    //{
                    //    ShadstersAvatarTools.GenerateAnimationHueShaders(vrcAvatar);
                    //}
                    if (GUILayout.Button("Generate Emote Override Menu", GUILayout.Height(24)))
                    {
                        GenerateEmoteOverrideMenu(vrcAvatarDescriptor);
                    }
                    if (GUILayout.Button("Cleanup Unused Generated Animations", GUILayout.Height(24)))
                    {
                        if (Prompt("Cleanup Unused Generated Animations"))
                        {
                            CleanupUnusedGeneratedAnimations();
                        }
                    }
                    if (GUILayout.Button("Uncheck All Write Defaults states", GUILayout.Height(24)))
                    {
                        if (Prompt("Uncheck All Write Defaults states"))
                        {
                            UncheckAllWriteDefaults(vrcAvatarDescriptor);
                        }
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        } //End Animation Foldout

        public void DrawGogoFoldout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(1.0f, 0.65f, 0f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                gogoFoldView = EditorGUILayout.Foldout(gogoFoldView, "Gogo Loco");
                if (gogoFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1.6f, 1f, 0f);
                    using (new EditorGUI.DisabledScope(!GogoLocoExist() && GetGogoLocoVersion("1.8.6")))
                    {
                        GUILayout.Label("Beyond Setup:", GUILayout.Height(24));
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(new GUIContent("Setup Prefab", ""), GUILayout.Height(24)))
                            {
                                SetupGogoBeyondPrefab(vrcAvatar);
                            }
                            if (GUILayout.Button("Setup Layers", GUILayout.Height(24)))
                            {
                                SetupGogoBeyondLayers(vrcAvatarDescriptor);
                            }
                            if (GUILayout.Button("Setup Menu", GUILayout.Height(24)))
                            {
                                SetupGogoBeyondMenu(vrcMenu);
                            }
                            if (GUILayout.Button("Setup Params", GUILayout.Height(24)))
                            {
                                SetupGogoBeyondParams(vrcParameters);
                            }
                            if (GUILayout.Button("Setup FX", GUILayout.Height(24)))
                            {
                                SetupGogoBeyondFX(vrcAvatarDescriptor);
                            }
                        }
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawScenesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.2f, 0.2f, 0.2f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                scenesFoldView = EditorGUILayout.Foldout(scenesFoldView, "Scenes");
                if (scenesFoldView)
                {
                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);

                    string rootAssetsPath = Path.GetDirectoryName(Application.dataPath);
                    string originalScenePath = SceneManager.GetActiveScene().path;
                    string context = originalScenePath.Substring(0, originalScenePath.LastIndexOf("/"));
                    string name = context.Substring(context.LastIndexOf("/") + 1);
                    string versionTxtPath = context + "/version.txt";
                    string authorTxtPath = context + "/author.txt";
                    string blenderTxtPath = context + "/blender.txt";
                    string packagePath = rootAssetsPath + "/" + name + ".unitypackage";

                    if (context != lastContext) //If we changed our avatar folder, clear the info data
                    {
                        lastContext = context;
                        version = "";
                        author = "";
                        blenderZipPath = "";
                    }
                    if (File.Exists(versionTxtPath) && string.IsNullOrEmpty(version))
                    {
                        version = ReadTxtFile(versionTxtPath);
                    }
                    if (File.Exists(authorTxtPath) && string.IsNullOrEmpty(author))
                    {
                        author = ReadTxtFile(authorTxtPath);
                    }
                    if (File.Exists(blenderTxtPath) && string.IsNullOrEmpty(blenderZipPath))
                    {
                        blenderZipPath = ReadTxtFile(blenderTxtPath);
                    }

                    GUIStyle boldCenteredStyle = new GUIStyle(GUI.skin.label);
                    boldCenteredStyle.alignment = TextAnchor.MiddleCenter;
                    boldCenteredStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Label("Context: " + context, boldCenteredStyle);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Setup Menus/Params", GUILayout.Height(24)))
                        {
                            SetupVRCMenus();
                        }
                        if (GUILayout.Button("Setup FX Controller", GUILayout.Height(24)))
                        {
                            SetupVRCController();
                        }
                    }
                    if (GUILayout.Button("Regenerate context with new GUIDs", GUILayout.Height(24)))
                    {
                        if (Prompt("Regenerate context with new GUIDs - This can cause missing references!"))
                        {
                            CloneAndRegenerateGUIDs(context);
                        }
                    }
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(new GUIContent("Cleanup", "Remove leftover prefabs and clear blueprint ID"), GUILayout.Height(24)))
                        {
                            if (Prompt("Cleanup All Scenes in " + context))
                            {
                                CleanUp();
                            }
                        }
                        if (GUILayout.Button(new GUIContent("Cleanup & Fix", "Remove leftovers and run all fixes in Common"), GUILayout.Height(24)))
                        {
                            if (Prompt("Cleanup & Fix All Scenes in " + context))
                            {
                                CleanUp(true);
                            }
                        }
                        if (GUILayout.Button(new GUIContent("Cleanup & Fix & Export", "Remove leftovers, run fixes, export package context folder with GogoLoco and Wholesome folders"), GUILayout.Height(24)))
                        {
                            CleanUp(true);
                            Export();
                        }
                        if (GUILayout.Button(new GUIContent("Export Only", "Export package within context folder with GogoLoco and Wholesome folders"), GUILayout.Height(24)))
                        {
                            Export();
                        }
                    }
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Version:", GUILayout.Height(24));
                        GUILayout.Label("Author:", GUILayout.Height(24));
                    }
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        version = EditorGUILayout.TextField(version, GUILayout.Height(24));
                        author = EditorGUILayout.TextField(author, GUILayout.Height(24));
                    }   
                    GUILayout.Label("Blender Zip Path:", GUILayout.Height(24));
                    blenderZipPath = EditorGUILayout.TextField(blenderZipPath, GUILayout.Height(24));
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Save Info Txt", GUILayout.Height(24)))
                        {
                            Save(versionTxtPath, authorTxtPath, blenderTxtPath);
                        }
                        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(blenderZipPath) || !File.Exists(blenderZipPath)))
                        {
                            if (GUILayout.Button(new GUIContent("Compile", "Take exported package and blender zip path and compile into one zip file"), GUILayout.Height(24)))
                            {
                                string[] filePaths = {blenderZipPath, packagePath};
                                ZipFiles(filePaths, rootAssetsPath + "/" + name + "_" + version + "_" + author + ".zip");
                                EditorUtility.RevealInFinder(Application.dataPath);
                            }
                            if (GUILayout.Button("Cleanup & Fix & Export & Save & Compile", GUILayout.Height(24)))
                            {
                                Save(versionTxtPath, authorTxtPath, blenderTxtPath);
                                CleanUp(true);
                                Export();
                                Save(versionTxtPath, authorTxtPath, blenderTxtPath);
                                string[] filePaths = { blenderZipPath, packagePath };
                                ZipFiles(filePaths, rootAssetsPath + "/" + name + "_" + version + "_" + author + ".zip");
                                EditorUtility.RevealInFinder(Application.dataPath);
                            }
                        }
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawAllScenesView()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.1f, 0.1f, 0.1f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                subScenesFoldView = EditorGUILayout.Foldout(subScenesFoldView, "All Scenes");
                if (subScenesFoldView)
                {
                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
                    string rootAssetsPath = Path.GetDirectoryName(Application.dataPath);
                    GUIStyle boldCenteredStyle = new GUIStyle(GUI.skin.label);
                    boldCenteredStyle.alignment = TextAnchor.MiddleCenter;
                    boldCenteredStyle.fontStyle = FontStyle.Bold;
                    GUILayout.Label("Context: " + rootAssetsPath, boldCenteredStyle);


                    GUI.backgroundColor = currentBackgroundColor;
                }

                EditorGUILayout.EndVertical();
            }
        }

        

        private void Save(string versionTxtPath, string authorTxtPath, string blenderTxtPath)
        {
            if (!string.IsNullOrEmpty(version))
            {
                SaveStringToFile(version, versionTxtPath);
            }
            if (!string.IsNullOrEmpty(author))
            {
                SaveStringToFile(author, authorTxtPath);
            }
            if (!string.IsNullOrEmpty(blenderZipPath))
            {
                blenderZipPath = RemoveQuotes(blenderZipPath);
                SaveStringToFile(blenderZipPath, blenderTxtPath);
            }
            this.Repaint();
        }

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(vrcAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));

                if (GUILayout.Button("Auto-Detect", GUILayout.Height(24)))
                {
                    AutoDetect();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatar = (GameObject)EditorGUILayout.ObjectField(vrcAvatar, typeof(GameObject), true, GUILayout.Height(24));


                if (GUILayout.Button("Reset-All", GUILayout.Height(24)))
                {
                    ResetAll();
                }

            }
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
                vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var sceneToggleState = GUILayout.Toggle(startInSceneView, new GUIContent("Start Play Mode in Scene View", "Loads prefab that will start play mode to Scene view instead of starting in Game View"), GUILayout.Height(24), GUILayout.Width(250));
                if (sceneToggleState != startInSceneView)
                {
                    Checkboxes.SetStartPlayModeInSceneView(sceneToggleState);
                    startInSceneView = Checkboxes.GetStartPlayModeInSceneView();
                }
                var playModeToggleState = GUILayout.Toggle(useExperimentalPlayMode, new GUIContent("Use Experimental Play Mode", "Instantly loads entering play mode, save often and disable if issues occur"), GUILayout.Height(24));
                if (playModeToggleState != useExperimentalPlayMode)
                {
                    Checkboxes.UseExperimentalPlayMode(playModeToggleState);
                    useExperimentalPlayMode = playModeToggleState;

                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                var ignorePhysToggleState = GUILayout.Toggle(ignorePhysImmobile, new GUIContent("Ignore Physbone Immobile", "When in Play Mode, updates all physbones with Immobile World Type to zero"), GUILayout.Height(24), GUILayout.Width(250));
                if (ignorePhysToggleState != ignorePhysImmobile)
                {
                    Checkboxes.SetIgnorePhysImmobile(ignorePhysToggleState);
                    ignorePhysImmobile = Checkboxes.GetIgnorePhysImmobile();
                }
                var testPhysbonesState = GUILayout.Toggle(testPhysbones, new GUIContent("Test All Avatar Physbones", "When in Play Mode, automatically moves the avatar to check behaviour of physbones"), GUILayout.Height(24));
                if (testPhysbonesState != testPhysbones)
                {
                    Checkboxes.SetTestPhysbones(testPhysbonesState);
                    testPhysbones = Checkboxes.GetTestPhysbones();
                }
            }

            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            using (new EditorGUI.DisabledScope(vrcAvatarDescriptor == null && vrcAvatar == null))
            {
                

                DrawCommonFoldout();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawTexturesWindow();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawMaterialFoldout();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawBonesWindow();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawAnimationFoldout();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawGogoFoldout();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
            } // Using Disable Scope
            DrawScenesWindow();
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE
            //Footnote
            GUILayout.FlexibleSpace();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Made by Shadsterwolf  v" + toolVersion);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndScrollView();
        } // GUI
    }
}

