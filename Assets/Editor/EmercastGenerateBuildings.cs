using System.Collections.Generic;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class EmercastGenerateBuildings : UnityEditor.Editor
    {
        private const string BuildingsPrefabFolder = "Assets/Prefabs/Buildings";
        
        [MenuItem(("Emercast/Generate Buildings"))]
        public static void GenerateBuildings()
        {
            var targetSceneObject = GameObject.FindGameObjectWithTag("GeneratorTargetScene");
            var targetScene = targetSceneObject.GetComponent<SubScene>();
            
            var oldBuildingObjects = GameObject.FindGameObjectsWithTag("GeneratedBuildingObject");
            foreach (var obo in oldBuildingObjects)
            {
                DestroyImmediate(obo);
            }

            List<GameObject> CityParts = new List<GameObject>();
            
            // CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5334"), targetScene, 688000, 5334000));
            // CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5336"), targetScene));
            // CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5338"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5334"), targetScene, 690000, 5334000));
            // CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5336"), targetScene));
            // CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5338"), targetScene));

            var parent = new GameObject { name = "City", tag = "GeneratedBuildingObject", isStatic = true };
            SceneManager.MoveGameObjectToScene(parent, targetScene.EditingScene);
            var center = Vector3.zero;
            foreach (var part in CityParts)
            {
                center += part.transform.position;
            }

            center /= CityParts.Count;
            parent.transform.position = center;

            foreach (var part in CityParts)
            {
                part.transform.parent = parent.transform;
            }
        }
        
        private static GameObject GenerateCityPart(TextAsset gmlResource, SubScene targetScene, int xOffset, int yOffset)
        {
            var gmlFile = gmlResource.text;
            var lines = gmlFile.Split("\r\n");
            Debug.Log("Lines in GML file: " + lines.Length);
            var city = ParserGml.LoadGml(lines, xOffset, yOffset);
            Debug.Log("Parsed " + city.Count + " buildings");
        
            var center = Vector3.zero;
            var parent = new GameObject { name = "CityPart " + gmlResource.name, tag = "GeneratedBuildingObject", isStatic = true};
            SceneManager.MoveGameObjectToScene(parent, targetScene.EditingScene);

            List<GameObject> gos = new List<GameObject>();
            
            var counter = 0;
            var material = Resources.Load<Material>("BuildingMaterial");
            
            Debug.Log("Building to be generated: " + city.Count);
            
            foreach (var b in city)
            {
                var go = b.CreatePrefabAndInstantiate(material, targetScene.EditingScene, "Assets/Prefabs/Buildings/Building" + counter);
                go.transform.position = b.GetFloorCenter();
                center += go.transform.position;
                gos.Add(go);
                counter++;
            }
            AssetDatabase.SaveAssets();

            center /= counter;

            foreach (var go in gos)
            {
                go.transform.position -= center;
                go.transform.position *= 0.5f;
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                go.transform.parent = parent.transform;
            }
                    
            EditorSceneManager.MarkSceneDirty(targetScene.EditingScene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return parent;
        }
    }
}
