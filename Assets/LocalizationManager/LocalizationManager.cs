using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Localization
{
    [AddComponentMenu("Localization/Localization Manager")]
    public class LocalizationManager : MonoBehaviour
    {
        public List<ObjectSelector> objectSelectorList = new List<ObjectSelector>();
        public Transform rootObject;
        public static string DIVISION_CHAR = "=";

        public List<TextAsset> languageFiles;
        public int selectedLanguage = 0;

        public bool loadLanguageOnStart = true;

        public List<string> groups = new List<string>();

        private static readonly object locker = new object();
        private static LocalizationManager instance;

        public static LocalizationManager Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<LocalizationManager>();
                        if (instance == null)
                        {
                            Debug.LogError("Instance of '" + typeof(LocalizationManager) + "' does not exist.");
                        }
                    }

                    return instance;
                }
            }
        }

        [System.Serializable]
        public class ObjectSelector
        {
            public enum AssignationType
            {
                GameObjectChild,
                SpecificText,
                SpecificTextMeshPro,
                SpecificTextMesh
            };

            public AssignationType assignationType;

            public string key;
            public string text;
            public int group;

            public bool multipleObjectAllowed = true;
            public bool findChildByKeyName = true;
            public string childName;
            public bool generalChildsRootObject = true;
            public Transform customChildsRootObject;

            public bool textComponentAllowed = true;
            public bool textMeshComponentAllowed = true;
            public bool textMeshProComponentAllowed = true;


            public GameObject[] foundObjects;
            public Text textObject;
            public TextMesh textMeshbject;
            public TextMeshProUGUI textMeshProObject;
        }

        private void Awake()
        {
            if (!Instance.Equals(this))
            {
                enabled = false;
                Debug.LogWarning("Duplicate instance of '" + typeof(LocalizationManager) + "' has been enabled.");
            }

            LoadAssignationTypes();
            if (loadLanguageOnStart)
                LoadLanguage(selectedLanguage);
        }

        public void LoadLanguage(int languageIndex)
        {
            if (languageFiles.Count <= languageIndex)
            {
                Debug.LogError(
                    "Localization: The index for language selection is incorrect! Languages count:" +
                    $" {languageFiles.Count} Your index: {languageIndex}");
                return;
            }
            if (languageFiles[languageIndex] == null)
            {
                Debug.LogError("Localization: The language that you've selected with your index is empty!");
                return;
            }

            foreach (ObjectSelector sel in objectSelectorList)
            {
                sel.text = ConvertAndReturnText(sel, languageFiles[languageIndex].text.Split('\n'))
                    .Replace(@"\n", System.Environment.NewLine);

                if (sel.assignationType == ObjectSelector.AssignationType.GameObjectChild && sel.foundObjects != null)
                {
                    foreach (var foundObject in sel.foundObjects)
                    {
                        if (foundObject.GetComponent<TextMeshProUGUI>() && sel.textMeshProComponentAllowed)
                            foundObject.GetComponent<TextMeshProUGUI>().text = sel.text;
                        else if (foundObject.GetComponent<Text>() && sel.textComponentAllowed)
                            foundObject.GetComponent<Text>().text = sel.text;
                        else if (foundObject.GetComponent<TextMesh>() && sel.textMeshComponentAllowed)
                            foundObject.GetComponent<TextMesh>().text = sel.text;
                    }
                }
                else if (sel.assignationType == ObjectSelector.AssignationType.SpecificTextMeshPro &&
                         sel.textMeshProObject)
                    sel.textMeshProObject.text = sel.text;
                else if (sel.assignationType == ObjectSelector.AssignationType.SpecificTextMesh &&
                         sel.textMeshbject)
                    sel.textMeshbject.text = sel.text;
                else if (sel.assignationType == ObjectSelector.AssignationType.SpecificText &&
                         sel.textObject)
                    sel.textObject.text = sel.text;
            }
        }

        public string GetText(string key) =>
            objectSelectorList.Find((x) => x.key.Equals(key)).text;
        
        #region PRIVATE METHODS

        private string ConvertAndReturnText(ObjectSelector selector, string[] lines)
        {
            if (lines.Length > 1)
            {
                List<string> storedFilelines = new List<string>();
                for (int i = 1; i < lines.Length; i++)
                    storedFilelines.Add(lines[i]);

                foreach (string categories in groups)
                {
                    if (GetGroup(categories) == selector.group)
                    {
                        foreach (string s in storedFilelines)
                        {
                            if (string.IsNullOrEmpty(s))
                                continue;
                            if (s.StartsWith(DIVISION_CHAR))
                                continue;
                            string key = s.Substring(0, s.IndexOf(DIVISION_CHAR, StringComparison.Ordinal));
                            if (string.IsNullOrEmpty(key))
                                continue;
                            if (key == selector.key)
                            {
                                if (s.Length < key.Length + 1)
                                    continue;
                                selector.text = s.Substring(key.Length + 1, s.Length - key.Length - 1);
                                return selector.text;
                            }
                        }
                    }
                }
            }

            return "";
        }

        private int GetGroup(string entry)
        {
            int c = 0;
            foreach (string g in groups)
            {
                if (g == entry)
                    return c;
                c++;
            }

            return 0;
        }


        /// <summary>
        /// Load and Refresh all text objects by the selected options
        /// </summary>
        private void LoadAssignationTypes()
        {
            foreach (ObjectSelector sel in objectSelectorList)
            {
                switch (sel.assignationType)
                {
                    case ObjectSelector.AssignationType.GameObjectChild:
                        string childName = sel.childName;
                        if (sel.findChildByKeyName)
                            childName = sel.key;
                        List<GameObject> foundChild = new List<GameObject>();
                        if (sel.generalChildsRootObject)
                            foreach (Transform t in rootObject.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == childName)
                                {
                                    if (sel.textMeshProComponentAllowed && t.GetComponent<TextMeshProUGUI>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                    else if (sel.textComponentAllowed && t.GetComponent<Text>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                    else if (sel.textMeshComponentAllowed && t.GetComponent<TextMesh>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                }
                            }
                        else if (sel.customChildsRootObject)
                            foreach (Transform t in sel.customChildsRootObject.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == childName)
                                {
                                    if (sel.textMeshProComponentAllowed && t.GetComponent<TextMeshProUGUI>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                    else if (sel.textComponentAllowed && t.GetComponent<Text>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                    else if (sel.textMeshComponentAllowed && t.GetComponent<TextMesh>())
                                    {
                                        foundChild.Add(t.gameObject);
                                        if (!sel.multipleObjectAllowed) break;
                                    }
                                }
                            }
                        else
                            Debug.Log("Localization: The key '" + sel.key +
                                      "' has empty variable [customChildsRootObject].");

                        if (foundChild.Count > 0)
                            sel.foundObjects = foundChild.ToArray();
                        else
                            Debug.Log(
                                "Localization: The key '" + sel.key + "' couldn't find its object in the root object.");
                        break;
                }
            }
        }
        #endregion

        
    }
}