using UnityEditor;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

[System.Serializable]
public class Primitive
{
    public bool hasPhysics;
    public string name;
    public List<Vector3> position;
    public List<Vector3> rotation;
    public List<Vector3> scale;
    public int type;
}

public class Parser
{
    [MenuItem("Editor/Load Scene From File")]
    static void LoadSceneFromFile()
    {
        string filePath = EditorUtility.OpenFilePanel("Open scene file", "", "level");
        
        if (filePath.Length != 0)
        {
            Debug.Log($"Loading: {filePath}");
            StreamReader streamReader = new StreamReader(filePath);
            string json = streamReader.ReadToEnd();

            // Parse all data
            JObject data = JObject.Parse(json);
            
            // Loop through the objects
            for (int i = 0; i < data.Count; i++)
            {
                JObject obj = (JObject)data[i.ToString()];
                
                // Save the primitive data
                Primitive primitiveData = new Primitive
                {
                    hasPhysics = (bool)obj["hasPhysics"],
                    name = (string)obj["name"],
                    position = new List<Vector3>(),
                    rotation = new List<Vector3>(),
                    scale =    new List<Vector3>(),
                    type = (int)data[i.ToString()]["type"]
                };
                primitiveData.position.Add(new Vector3((float)obj["position"][0]["x"], (float)obj["position"][0]["y"], (float)obj["position"][0]["z"]));
                primitiveData.rotation.Add(new Vector3((float)obj["rotation"][0]["x"], (float)obj["rotation"][0]["y"], (float)obj["rotation"][0]["z"]));
                primitiveData.scale.Add(new Vector3((float)obj["scale"][0]["x"], (float)obj["scale"][0]["y"], (float)obj["scale"][0]["z"]));
                // Create a primitive
                GameObject newPrimitive = GameObject.CreatePrimitive((PrimitiveType)primitiveData.type);

                // Set primitive datas
                newPrimitive.transform.position = primitiveData.position[0];
                newPrimitive.transform.eulerAngles = primitiveData.rotation[0];
                newPrimitive.transform.localScale = primitiveData.scale[0];

                if (primitiveData.hasPhysics)
                {
                    newPrimitive.AddComponent<Rigidbody>();
                }
            }
        }
    }
    [MenuItem("Editor/Save Scene")]
    static void SaveScene()
    {
        GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();
        int gameObjectCount = 0;

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);

        string dataToSave = "{";
        // Loop through game objects
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject obj = gameObjects[i];
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null) continue;
            Debug.Log(meshFilter.sharedMesh.name);

            // Verify if game object is primitive
            bool isPrimitive = true;
            int primType = 0;
                
            if (meshFilter.sharedMesh.name == "Sphere Instance")
            {
                primType = 0;
            }
            else if (meshFilter.sharedMesh.name == "Capsule Instance")
            {
                primType = 1;
            }
            else if (meshFilter.sharedMesh.name == "Cylinder Instance")
            {
                primType = 2;
            }
            else if (meshFilter.sharedMesh.name == "Cube Instance")
            {
                primType = 3;
            }
            else if (meshFilter.sharedMesh.name == "Plane Instance")
            {
                primType = 4;
            }
            else
            {
                isPrimitive = false;
            }

            if (!isPrimitive) continue;

                
            Primitive primitive = new Primitive()
            {
                hasPhysics = (obj.GetComponent<Rigidbody>() != null) ? true : false,
                name = obj.name,
                position = new List<Vector3>(),
                rotation = new List<Vector3>(),
                scale = new List<Vector3>(),
                type = primType
            };

            primitive.position.Add(obj.transform.position);
            primitive.rotation.Add(obj.transform.rotation.eulerAngles);
            primitive.scale.Add(obj.transform.localScale);

            string primitiveData = JsonUtility.ToJson(primitive);
            dataToSave = dataToSave + $"\"{gameObjectCount.ToString()}\" : " + primitiveData + ",";

            gameObjectCount++;
        }
        dataToSave = dataToSave + "}";
        string filePath = EditorUtility.OpenFolderPanel("Save data location", "", "");
        File.WriteAllText(filePath + "/testUnitySave.level", dataToSave);
    }
}
