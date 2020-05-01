using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Localization
{
    [CustomEditor(typeof(LocalizationManager))]
    [CanEditMultipleObjects]
    public class LocalizationManagerInspector : Editor
    {
        private SerializedProperty _languageFiles, _selectedLanguage;
        private SerializedProperty _loadLanguageOnStart;

        private SerializedProperty _categories;
        private SerializedProperty _localizationSelector;

        private SerializedProperty _gameObjectChildsRoot;

        LocalizationManager l;

        bool addKey = false;
        int _group = -1;


        private bool isShowLang = false;
        private bool isShowContent = true;
        private GUIStyle blockStyle;

        private void OnEnable()
        {
            l = (LocalizationManager) target;

            _languageFiles = serializedObject.FindProperty("languageFiles");
            _selectedLanguage = serializedObject.FindProperty("selectedLanguage");
            _loadLanguageOnStart = serializedObject.FindProperty("loadLanguageOnStart");
            _categories = serializedObject.FindProperty("groups");
            _localizationSelector = serializedObject.FindProperty("objectSelectorList");
            _gameObjectChildsRoot = serializedObject.FindProperty("rootObject");

            blockStyle = new GUIStyle(LocLayout.BACKGROUND);
            blockStyle.padding = new RectOffset(10, 10, 10, 10);
        }

        #region PRIVATE METHODS

        private void AddKey(string key)
        {
            foreach (LocalizationElement a in LocalizationWindow
                .ElementsList)
            {
                if (a.Key == key)
                {
                    l.objectSelectorList.Add(
                        new LocalizationManager.ObjectSelector() {key = a.Key, text = a.Text, group = a.Group});
                    return;
                }
            }
        }

        private void Refresh()
        {
            l.groups.Clear();
            l.groups.AddRange(LocalizationController.LOCALIZATION_GROUPS);
        }

        #endregion


        public override void OnInspectorGUI()
        {
            if (target == null)
                return;
            serializedObject.Update();
            DrawHeader();

            LocLayout.Space();

            //LANGUAGE SETTINGS

            if (LocLayout.Bar("Show language settings", ref isShowLang, _selectedLanguage.intValue == -1))
            {
                GUILayout.BeginVertical(blockStyle);

                LocLayout.List(_languageFiles, "Language Files");
                LocLayout.Space(5);
                serializedObject.ApplyModifiedProperties();

                if (DrawItemSelector(_selectedLanguage, _languageFiles, "Selected Language") &&
                    _selectedLanguage.intValue != -1)
                    l.LoadLanguage(_selectedLanguage.intValue);

                GUILayout.EndVertical();
            }

            LocLayout.Space();
            serializedObject.ApplyModifiedProperties();

            // CONTENT SETTINGS
            LocLayout.Bar("Show content settings", ref isShowContent);
            if (!isShowContent) return;

            GUILayout.BeginVertical(blockStyle);
            {
                LocLayout.Property(_gameObjectChildsRoot, "GameObject Root",
                    "Starting find root for keys containing 'GameObjectChild' assignation type", isColor: true,
                    isNullColor: true);
                LocLayout.Space(5);
                GUILayout.BeginHorizontal();
                {

                    LocLayout.ToggleLabel(_loadLanguageOnStart, "Load language On Start",
                        "Update all GameObjects on scene load");
                    LocLayout.Space(3);
                    if(_selectedLanguage.intValue!=-1 && LocLayout.Button("Load selected language")) 
                        l.LoadLanguage(_selectedLanguage.intValue);
                }
                GUILayout.EndHorizontal();
                LocLayout.Space();
                serializedObject.ApplyModifiedProperties();

                //ADD NEW KEY
                if (LocLayout.Bar("Add Key", ref addKey, false, 20))
                {
                    LocalizationWindow.Init();
                    Refresh();
                    DrawAddKeyMenu();
                }

                LocLayout.Space();
                serializedObject.Update();

                //KEY LIST
                GUILayout.BeginVertical();
                {
                    if (l.objectSelectorList.Count > 0)
                        DrawKeyList(_localizationSelector);
                    else
                        LocLayout.Label("- - Key list is empty - -");
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        #region LAYOUT

        private void DrawAddKeyMenu()
        {
            bool addAll = false;

            GUILayout.BeginVertical(blockStyle);
            {
                LocLayout.Label("Choose Group:");
                LocLayout.Space();
                GUILayout.BeginHorizontal();
                {
                    //TODO too much elements
                    for (int i = 0; i < l.groups.Count; i++)
                    {
                        if (LocLayout.Button(l.groups[i]))
                        {
                            _group = i;
                        }

                        LocLayout.Space();
                    }
                }
                GUILayout.EndHorizontal();
                if (_group != -1)
                {
                    LocLayout.Space(40);

                    GUILayout.BeginVertical(blockStyle);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            LocLayout.Label("Choose Key:");
                            GUILayout.FlexibleSpace();
                            LocLayout.Button("Add All", ref addAll, 120);
                        }
                        GUILayout.EndHorizontal();
                        LocLayout.Space();
                        GUILayout.BeginVertical();
                        {
                            ushort counter = 0;
                            LocalizationWindow.ElementsList.Sort((x, y)
                                => String.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
                            for (var i = 0; i < LocalizationWindow.ElementsList.Count; i++)
                            {
                                var el = LocalizationWindow.ElementsList[i];
                                if (el.Group != _group) continue;
                                bool passed = true;
                                foreach (LocalizationManager.ObjectSelector sel in l.objectSelectorList)
                                {
                                    if (sel.key == el.Key)
                                    {
                                        passed = false;
                                        break;
                                    }
                                }

                                if (!passed) continue;
                                if (addAll)
                                {
                                    AddKey(el.Key);
                                }

                                //DRAW ELEMENTS
                                if (counter % 3 == 0) GUILayout.BeginHorizontal();

                                if (LocLayout.Button(el.Key, width: 100, color: LocLayout.BACK_COLOR))
                                {
                                    AddKey(el.Key);
                                    addKey = false;
                                    _group = -1;
                                    return;
                                }

                                GUILayout.FlexibleSpace();
                                counter++;
                                if (counter % 3 == 0)
                                {
                                    GUILayout.EndHorizontal();
                                    LocLayout.Space();
                                }
                            }

                            if (counter % 3 != 0) GUILayout.EndHorizontal();
                            if (counter == 0) LocLayout.Label("- - Empty - -");
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKeyList(SerializedProperty p)
        {
            for (int i = 0; i < l.objectSelectorList.Count; i++)
            {
                GUILayout.BeginVertical(blockStyle);
                SerializedProperty item = _localizationSelector.GetArrayElementAtIndex(i);
                var foundObjectsProperty = item.FindPropertyRelative("foundObjects");

                GUILayout.BeginHorizontal();
                {
                    LocLayout.Property(item, l.objectSelectorList[i].key);

                    if (Application.isPlaying)
                    {
                        GUILayout.BeginVertical();
                        {
                            for (int j = 0; j < foundObjectsProperty.arraySize; j++)
                                LocLayout.Property(foundObjectsProperty.GetArrayElementAtIndex(j));
                        }
                        GUILayout.EndVertical();
                    }

                    if (LocLayout.Button("Delete", 30, 40, LocLayout.BACK_COLOR))
                    {
                        l.objectSelectorList.RemoveAt(i);
                        return;
                    }
                }
                GUILayout.EndHorizontal();
                if (!item.isExpanded)
                {
                    GUILayout.EndVertical();
                    continue;
                }

                LocalizationManager.ObjectSelector sec = l.objectSelectorList[i];

                LocLayout.Space(5);


                EditorGUI.indentLevel += 1;
                GUILayout.BeginHorizontal("Box");
                LocLayout.Label("Key: " + sec.key);
                LocLayout.Label("Group: " + l.groups[sec.@group]);
                GUILayout.EndHorizontal();
                LocLayout.Space();


                LocLayout.Property(item.FindPropertyRelative("assignationType"), "Assignation Type", isColor: true);

                switch (sec.assignationType)
                {
                    case LocalizationManager.ObjectSelector.AssignationType.GameObjectChild:

                        if (!LocLayout.ToggleLabel(item.FindPropertyRelative("refindOnStart"), "Refind On Start"))
                        {
                            LocLayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {

                                if(LocLayout.Button("Search"))
                                {
                                    sec.foundObjects = l.FindGameObject(sec);
                                }
                                LocLayout.Space(3);

                                LocLayout.List(foundObjectsProperty, "Objects");

                            }
                            GUILayout.EndHorizontal();

                        }
                        LocLayout.Space(5);

                        LocLayout.ToggleLabel(item.FindPropertyRelative("findChildByKeyName"),
                            "Find Child By Key Name",
                            "If enabled, the system will find the child of the selected component type [below] by the key name");
                        if (sec.findChildByKeyName)
                            LocLayout.Property(item.FindPropertyRelative("childName"), "Child Name");

                        LocLayout.Space(3);

                        LocLayout.ToggleLabel(item.FindPropertyRelative("сhildsRootObject"),
                            "Use Custom Childs Root Object");
                        if (sec.сhildsRootObject)
                            LocLayout.Property(item.FindPropertyRelative("customChildsRootObject"),
                                "Custom Childs Root Object", isColor: true, isNullColor: true);

                        LocLayout.Space(3);

                        LocLayout.ToggleLabel(item.FindPropertyRelative("multipleObjectAllowed"),
                            "Allow Multiple Objects");
                        
                        LocLayout.Space(5);
                        LocLayout.Label("Allow Component Object");
                        GUILayout.BeginHorizontal();
                    {
                        LocLayout.ToggleLabel(item.FindPropertyRelative("textComponentAllowed"),
                            "UIText");
                        LocLayout.ToggleLabel(item.FindPropertyRelative("textMeshComponentAllowed"),
                            "TextMesh");
                        LocLayout.ToggleLabel(item.FindPropertyRelative("textMeshProComponentAllowed"),
                            "UITextMeshPro");
                        LocLayout.Space(3);
                    }
                        GUILayout.EndHorizontal();


                        break;
                    case LocalizationManager.ObjectSelector.AssignationType.SpecificTextMeshPro:
                        LocLayout.Property(item.FindPropertyRelative("textMeshProObject"),
                            "Specific UI TextMeshPro",
                            "Assign specific UI TextMeshPro object", isColor: true, isNullColor: true);
                        break;
                    case LocalizationManager.ObjectSelector.AssignationType.SpecificText:
                        LocLayout.Property(item.FindPropertyRelative("textObject"), "Specific UI Text",
                            "Assign specific UI Text object", isColor: true, isNullColor: true);
                        break;

                    case LocalizationManager.ObjectSelector.AssignationType.SpecificTextMesh:
                        LocLayout.Property(item.FindPropertyRelative("textMeshbject"), "Specific Text Mesh",
                            "Assign specific Text Mesh object", isColor: true, isNullColor: true);
                        break;
                }

                EditorGUI.indentLevel -= 1;
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool DrawItemSelector(SerializedProperty p, SerializedProperty serializedList, string label)
        {
            var temp = p.intValue;
            GUILayout.BeginVertical();
            {
                var size = 14;
                int height = (int) (size * 1.5f);

                List<string> list = new List<string>();
                if (serializedList.arraySize > 0)
                {
                    for (int i = 0; i < serializedList.arraySize; i++)
                    {
                        var el = serializedList.GetArrayElementAtIndex(i);
                        if (!string.IsNullOrEmpty(el?.objectReferenceValue?.name))
                            list.Add(el.objectReferenceValue.name);
                    }
                }

                var backColor = p.intValue != -1 ? LocLayout.BACK_COLOR : LocLayout.EMPTY_COLOR;
                LocLayout.Background(backColor, height);
                LocLayout.Space(-height);
                GUILayout.BeginVertical(GUILayout.Height(size));
                if (list.Count > 0)
                {
                    if (p.intValue == -1)
                        p.intValue = 0;
                    p.intValue = EditorGUILayout.Popup(label, p.intValue, list.ToArray());
                }
                else
                {
                    p.intValue = -1;
                    EditorGUILayout.LabelField("Selected Language \t\t\t - NONE -");
                }

                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            return temp != p.intValue ? true : false;
        }

        private new static void DrawHeader()
        {
            LocLayout.Space();
            GUILayout.Label(GUIContent.none, LocLayout.HEADER, GUILayout.ExpandWidth(true));
            LocLayout.Space(20);
        }

        #endregion
    }
}