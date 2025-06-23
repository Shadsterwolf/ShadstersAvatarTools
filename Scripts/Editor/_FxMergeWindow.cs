using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static Shadster.AvatarTools.Helper;
using static Shadster.AvatarTools.Animation;
using static Shadster.AvatarTools.AnimatorControl;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using System.IO;

namespace Shadster.AvatarTools.FxMerge
{
    [System.Serializable]
    public class _FxMergeWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static _FxMergeWindow _tools;

        static EditorWindow toolWindow;
        Vector2 scrollPos;

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMenu;
        [SerializeReference] private AnimatorController sharedFx;
        [SerializeReference] private List<AnimatorController> recieveFx = new List<AnimatorController>();
        [SerializeReference] private GameObject tempObject;


        [System.Serializable]
        private class FxMergeConfig
        {
            public AnimatorController sharedFx;
            public List<AnimatorController> recieveFx = new List<AnimatorController>();
        }
        public static _FxMergeWindow ToolsWindow
        {
            get
            {
                if (!_tools)
                    _tools = FindObjectOfType(typeof(_FxMergeWindow)) as _FxMergeWindow ?? CreateInstance<_FxMergeWindow>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }
        [MenuItem("ShadsterWolf/FX Merge", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow<_FxMergeWindow>();
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("FX Merge");
                toolWindow.minSize = new Vector2(500, 200);
            }
            toolWindow.Show();
        }

        private void OnInspectorUpdate()
        {

        }

        private void SaveFxConfig(string path)
        {
            FxMergeConfig config = new FxMergeConfig
            {
                sharedFx = sharedFx,
                recieveFx = recieveFx
            };

            string json = EditorJsonUtility.ToJson(config, true);
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log("FX Merge config saved.");
        }

        private void LoadFxConfig(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning("FX Merge config not found.");
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            FxMergeConfig config = new FxMergeConfig();
            EditorJsonUtility.FromJsonOverwrite(json, config);
            sharedFx = config.sharedFx;
            recieveFx = config.recieveFx;
            Debug.Log("FX Merge config loaded.");
        }

        public void OnGUI()
        {
            string rootAssetsPath = Path.GetDirectoryName(Application.dataPath);
            string originalScenePath = SceneManager.GetActiveScene().path;
            string context = originalScenePath.Substring(0, originalScenePath.LastIndexOf("/"));
            string savePath = context + "/FxMergeConfig.json";

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.LabelField("Shared/Source Controller", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Config"))
                    SaveFxConfig(savePath);

                if (GUILayout.Button("Load Config"))
                    LoadFxConfig(savePath);
            }
            EditorGUILayout.Space();
            sharedFx = (AnimatorController)EditorGUILayout.ObjectField(sharedFx, typeof(AnimatorController), true, GUILayout.Height(24));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recieve FX Controllers", EditorStyles.boldLabel);

            if (recieveFx == null)
                recieveFx = new List<AnimatorController>();

            int removeIndex = -1;

            for (int i = 0; i < recieveFx.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                recieveFx[i] = (AnimatorController)EditorGUILayout.ObjectField($"Controller {i}", recieveFx[i], typeof(AnimatorController), true);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ Add Controller"))
            {
                recieveFx.Add(null);
            }
            if (removeIndex >= 0)
            {
                recieveFx.RemoveAt(removeIndex);
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Merge controller source to recievers"))
                {
                    foreach (var controller in recieveFx)
                    {
                        if (controller == null)
                        {
                            Debug.LogWarning("Null controller found, skipping merge.");
                            continue;
                        }
                        Debug.Log("Merging " + controller.name);
                        CopyControllerParams(sharedFx, controller);
                        VRLabs.AV3Manager.AnimatorCloner.CopyControllersLayers(sharedFx, controller);
                    }
                }
            }


            EditorGUILayout.EndScrollView();

        } //End GUI
    }
}