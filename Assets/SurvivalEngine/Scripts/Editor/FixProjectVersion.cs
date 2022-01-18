using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace SurvivalEngine.EditorTool
{
    /// <summary>
    /// Check if can apply any automatic fixes to issues that could be caused from changing asset version
    /// </summary>

    public class FixProjectVersion : ScriptableWizard
    {
        [MenuItem("Survival Engine/Fix Project Version", priority = 400)]
        static void ScriptableWizardMenu()
        {
            ScriptableWizard.DisplayWizard<FixProjectVersion>("Fix Project Version", "Fix");
        }

        void OnWizardCreate()
        {
            string[] allPrefabs = GetAllPrefabs();
            foreach (string prefab_path in allPrefabs)
            {
                GameObject prefab = (GameObject) AssetDatabase.LoadMainAssetAtPath(prefab_path);
                if (prefab != null)
                {
                    //Add buildable to constructions
                    if (prefab.GetComponent<Construction>() != null && prefab.GetComponent<Buildable>() == null)
                    {
                        prefab.AddComponent<Buildable>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added Buildable Component to: " + prefab_path);
                    }

                    //Add buildable to plants
                    if (prefab.GetComponent<Plant>() != null && prefab.GetComponent<Buildable>() == null)
                    {
                        prefab.AddComponent<Buildable>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added Buildable Component to: " + prefab_path);
                    }

                    //Add character to animals
                    if (prefab.GetComponent<AnimalWild>() != null && prefab.GetComponent<Character>() == null)
                    {
                        prefab.AddComponent<Character>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added Character Component to: " + prefab_path);
                    }

                    //Add character to birds
                    if (prefab.GetComponent<Bird>() != null && prefab.GetComponent<Character>() == null)
                    {
                        prefab.AddComponent<Character>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added Character Component to: " + prefab_path);
                    }

                    //Add PlayerCharacterCombat
                    if (prefab.GetComponent<PlayerCharacter>() != null && prefab.GetComponent<PlayerCharacterCombat>() == null)
                    {
                        prefab.AddComponent<PlayerCharacterCombat>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added PlayerCharacterCombat Component to: " + prefab_path);
                    }

                    //Add PlayerCharacterAttribute
                    if (prefab.GetComponent<PlayerCharacter>() != null && prefab.GetComponent<PlayerCharacterAttribute>() == null)
                    {
                        prefab.AddComponent<PlayerCharacterAttribute>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added PlayerCharacterAttribute Component to: " + prefab_path);
                    }

                    //Add PlayerCharacterAttribute
                    if (prefab.GetComponent<PlayerCharacter>() != null && prefab.GetComponent<PlayerCharacterInventory>() == null)
                    {
                        prefab.AddComponent<PlayerCharacterInventory>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added PlayerCharacterInventory Component to: " + prefab_path);
                    }

                    //Add PlayerCharacterCraft
                    if (prefab.GetComponent<PlayerCharacter>() != null && prefab.GetComponent<PlayerCharacterCraft>() == null)
                    {
                        prefab.AddComponent<PlayerCharacterCraft>();
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Added PlayerCharacterCraft Component to: " + prefab_path);
                    }

                    //Remove Anywhere mode in Buildable
                    Buildable buildable = prefab.GetComponent<Buildable>();
                    if (buildable != null)
                    {
                        if ((int)buildable.type == 5) //Anywhere
                        { 
                            buildable.type = (BuildableType)0; //Default
                            EditorUtility.SetDirty(prefab);
                            Debug.Log("Changed buildable type to " + buildable.type + " on: " + prefab_path);
                        }
                        if ((int)buildable.type == 15) //AnywhereGrid
                        { 
                            buildable.type = (BuildableType)10; //Grid
                            EditorUtility.SetDirty(prefab);
                            Debug.Log("Changed buildable type to " + buildable.type + " on: " + prefab_path);
                        }
                    }

                    //Add event trigger in UISlot
                    GameObject ui_parent = null;
                    ActionSelector actionselect = prefab.GetComponent<ActionSelector>();
                    TheUI theui = prefab.GetComponent<TheUI>();
                    if (actionselect != null)
                        ui_parent = actionselect.gameObject;
                    if (theui != null)
                        ui_parent = theui.gameObject;
                    if (ui_parent != null)
                    {
                        foreach (UISlot slot in ui_parent.GetComponentsInChildren<UISlot>())
                        {
                            if (slot.GetComponent<UnityEngine.UI.Button>() == null && slot.GetComponent<UnityEngine.EventSystems.EventTrigger>() == null)
                            {
                                slot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                                EditorUtility.SetDirty(prefab);
                                Debug.Log("Added Event Trigger to " + slot.gameObject.name + " on: " + prefab_path);
                            }
                        }
                    }

                    //Add gameplay ui
                    if (theui != null && theui.GetComponentInChildren<PlayerUI>() == null)
                    {
                        for (int i = 0; i < theui.transform.childCount; i++)
                        {
                            Transform child = theui.transform.GetChild(i);
                            if (child.gameObject.name == "Gameplay")
                            {
                                child.gameObject.AddComponent<PlayerUI>();
                                EditorUtility.SetDirty(prefab);
                                Debug.Log("Added PlayerUI on: " + prefab_path);
                            }
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        public static string[] GetAllPrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (s.Contains(".prefab")) result.Add(s);
            }
            return result.ToArray();
        }

        void OnWizardUpdate()
        {
            helpString = "Use this tool after updating Survival Engine version, to fix any prefabs that should be updated to match the new verison.";
        }
    }
}