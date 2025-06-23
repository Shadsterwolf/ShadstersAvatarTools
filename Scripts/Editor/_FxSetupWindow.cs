using Shadster.AvatarTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.VersionControl;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using YamlDotNet.Core.Tokens;
using static Shadster.AvatarTools.Helper;
using static Shadster.AvatarTools.Menus;
using static Shadster.AvatarTools.Params;
using static Shadster.AvatarTools.Animation;
using static Shadster.AvatarTools.AnimatorControl;

namespace Shadster.AvatarTools.FxSetup
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
        [SerializeReference] private AnimatorController vrcFx;
        [SerializeReference] private GameObject tempObject;

        private List<SkinnedMeshRenderer> _multiList = new List<SkinnedMeshRenderer>();
        private SkinnedMeshRenderer[] _meshRenderers;
        private bool[] _showShapekeys;
        private int[] _selectedKeys;

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
        [MenuItem("ShadsterWolf/FX Setup", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow<_FxMergeWindow>();
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("FX Setup");
                toolWindow.minSize = new Vector2(500, 200);
            }
            toolWindow.Show();
        }

        private void OnInspectorUpdate()
        {
            if (vrcAvatar != null && vrcAvatarDescriptor == null) //because play mode likes to **** with me and clear the descriptor
                vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            if (vrcAvatar == null && vrcAvatarDescriptor == null)
            {
                ResetAll();
            }
        }

        public void AutoDetect()
        {
            vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
            vrcAvatar = vrcAvatarDescriptor.gameObject;
            vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcFx = GetFxController(vrcAvatarDescriptor);

            _meshRenderers = vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (_showShapekeys == null || _showShapekeys.Length != _meshRenderers.Length)
            {
                _showShapekeys = new bool[_meshRenderers.Length];
                _selectedKeys = new int[_meshRenderers.Length];
            }
        }

        public void ResetAll()
        {
            vrcAvatarDescriptor = null;
            vrcAvatar = null;
            vrcMenu = null;
            vrcParameters = null;
            vrcFx = null;
            _multiList.Clear();
        }

        public void DrawFxLayers()
        {
            vrcFx = GetFxController(vrcAvatarDescriptor);
            for (int i = 0; i < vrcFx.layers.Length; i++)
            {
                var layer = vrcFx.layers[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(layer.name);
                if (GUILayout.Button("Delete", GUILayout.Width(110)))
                {
                    vrcFx.RemoveLayer(i);
                    //vrcFx.AddLayer("New Layer");
                    //break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawFxObjects()
        {
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _showShapekeys[i] = EditorGUILayout.Foldout(_showShapekeys[i], _meshRenderers[i].name);
                if (GUILayout.Button("Add Toggle", GUILayout.Width(110)))
                {
                    var clips = GenerateAnimationToggle(_meshRenderers[i], vrcAvatar);
                    //Debug.Log(clips[0] + " " + clips[0].GetInstanceID());
                    //Debug.Log(clips[1]);
                    //Debug.Log(_meshRenderers[i].name + " = " + _meshRenderers[i].enabled);
                    if (_meshRenderers[i].enabled && _meshRenderers[i].gameObject.activeInHierarchy)
                    {
                        CreateToggleLayer(vrcAvatarDescriptor, _meshRenderers[i].name, _meshRenderers[i].name, clips[0], clips[1], 1f);
                    }
                    else
                    {
                        CreateToggleLayer(vrcAvatarDescriptor, _meshRenderers[i].name, _meshRenderers[i].name, clips[0], clips[1], 0f);
                    }
                    CreateMenuControl(vrcMenu, _meshRenderers[i].name, VRCExpressionsMenu.Control.ControlType.Toggle, _meshRenderers[i].name);

                    break;
                }
                //using (new EditorGUI.DisabledScope(true))
                //{
                //    if (GUILayout.Button("Multi + TBD", GUILayout.Width(110)))
                //    {
                //        _multiList.Add(_meshRenderers[i]);
                //        break;
                //    }
                //}
                EditorGUILayout.EndHorizontal();

                if (_showShapekeys[i])
                {
                    EditorGUILayout.BeginHorizontal();
                    var shapeKeyDropdown = new string[0];
                    if (_meshRenderers[i].GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        var mesh = _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().sharedMesh;
                        if (mesh.blendShapeCount > 0)
                        {
                            shapeKeyDropdown = new string[mesh.blendShapeCount];
                            for (int j = 0; j < mesh.blendShapeCount; j++)
                            {
                                shapeKeyDropdown[j] = mesh.GetBlendShapeName(j);
                            }
                        }
                    }
                    if (shapeKeyDropdown.Length > 0)
                    {
                        string layerName;
                        _selectedKeys[i] = EditorGUILayout.Popup("Shape keys", _selectedKeys[i], shapeKeyDropdown);
                        string key = shapeKeyDropdown[_selectedKeys[i]];
                        if (GUILayout.Button("Add Slider", GUILayout.Width(110)))
                        {
                            layerName = key;
                            vrcFx.AddLayer(layerName);
                            var clips = GenerateShapekeyToggle(_meshRenderers[i].GetComponent<SkinnedMeshRenderer>(), key, vrcAvatar);
                            CreateBlendTreeLayer(vrcAvatarDescriptor, layerName, key, clips[0], clips[1]);
                            CreateMenuControl(vrcMenu, key, VRCExpressionsMenu.Control.ControlType.RadialPuppet, key);
                            //break;
                        }
                        if (GUILayout.Button("Add Toggle", GUILayout.Width(110)))
                        {
                            layerName = key;
                            vrcFx.AddLayer(layerName);
                            var clips = GenerateShapekeyToggle(_meshRenderers[i].GetComponent<SkinnedMeshRenderer>(), key, vrcAvatar);
                            CreateToggleLayer(vrcAvatarDescriptor, layerName, key, clips[0], clips[1]);
                            CreateMenuControl(vrcMenu, key, VRCExpressionsMenu.Control.ControlType.Toggle, key);
                            //break;
                        }
                        
                        //EditorGUILayout.LabelField("Value: " + _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(selectedKey));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(24);
                }
            }
        }

        public static void DrawMenuItems(VRCExpressionsMenu vrcMenu, VRCExpressionParameters vrcParameters)
        {
            for (int i = 0; i < vrcMenu.controls.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(vrcMenu.controls[i].name, GUILayout.Width(110));
                    EditorGUILayout.LabelField(vrcMenu.controls[i].type.ToString(), GUILayout.Width(110));
                    EditorGUILayout.LabelField(vrcMenu.controls[i].parameter.name, GUILayout.Width(110));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete", GUILayout.Width(110)))
                    {
                        DeleteVrcParameter(vrcParameters, vrcMenu.controls[i].parameter.name);
                        vrcMenu.controls.RemoveAt(i);
                        //break;
                    }
                }
            }
        }

        public void DrawFxMulti()
        {
            if (_multiList != null)
            {
                for (int i = 0; i < _multiList.Count; i++)
                {
                    var multiMesh = _multiList[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(multiMesh.name);
                    if (GUILayout.Button("Remove", GUILayout.Width(110)))
                    {
                        _multiList.RemoveAt(i);
                        AssetDatabase.Refresh();
                        //break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                using (new EditorGUI.DisabledScope(_multiList.Count < 2))
                {
                    GUILayout.Button("Create ");
                }
            }
        }

        public void DrawFxChildObjects()
        {
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(vrcFx));
            var explicitObjects = new List<Object>();

            foreach (var asset in allSubAssets)
            {
                // Ignore null, the controller itself, and internal Unity objects
                if (asset is BlendTree && asset.name.Contains("BlendTree"))
                {
                    explicitObjects.Add(asset);
                }
            }
            for (int i = 0; i < explicitObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(explicitObjects[i].name);
                if (GUILayout.Button("Remove", GUILayout.Width(110)))
                {
                    DestroyImmediate(explicitObjects[i], true);
                    EditorUtility.SetDirty(vrcFx);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    //break;
                }
                EditorGUILayout.EndHorizontal();
            }
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
                vrcFx = (AnimatorController)EditorGUILayout.ObjectField(vrcFx, typeof(AnimatorController), true, GUILayout.Height(24));
            }
            
            if (vrcAvatar != null && vrcFx != null)
            {
                //if (GUILayout.Button("BackUp Current FX Controller", GUILayout.Height(24)))
                //{
                //    BackupController(vrcFx);
                //}
                if (vrcFx.layers.Length > 0)
                {
                    DrawFxLayers();
                }
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
                vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                tempObject = (GameObject)EditorGUILayout.ObjectField(tempObject, typeof(GameObject), true, GUILayout.Height(24));
                if (GUILayout.Button("Generate Toggle Object", GUILayout.Height(24)))
                {
                    if (tempObject != null)
                    {
                        var clips = GenerateAnimationToggle(tempObject, vrcAvatar);
                        CreateToggleLayer(vrcAvatarDescriptor, (tempObject.name + " Toggle"), tempObject.name, clips[0], clips[1]);
                        CreateMenuControl(vrcMenu, tempObject.name + " Toggle", VRCExpressionsMenu.Control.ControlType.Toggle, tempObject.name);
                    }
                }
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcMenu != null)
            {
                DrawMenuItems(vrcMenu, vrcParameters);
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null)
            {
                DrawFxObjects();
            }
            //GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            //if (vrcAvatar != null)
            //{
            //    DrawFxMulti();
            //}
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null)
            {
                DrawFxChildObjects();
            }
            EditorGUILayout.EndScrollView();
        } //End GUI
    }
}

