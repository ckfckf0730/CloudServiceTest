using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;


public class RecordMap
{
	class ObjectRecord
	{
		public string name;
		public string model;
		public System.Numerics.Vector3 position;
		public System.Numerics.Vector3 rotation;
		public System.Numerics.Vector3 scale;
	}


	[MenuItem("CustomTools/RecordMap")]
	public static void StartRecord()
	{

		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		List<GameObject> objects = new List<GameObject>();

		var rootObj = scene.GetRootGameObjects();
		foreach (var obj in rootObj)
		{
			FindAllChildren(obj, objects);
		}

		List<ObjectRecord> objectRecords = new List<ObjectRecord>();

		foreach (var obj in objects)
		{
			RecordObject(obj,objectRecords);
		}

		var json = JsonConvert.SerializeObject(objectRecords, Formatting.Indented);

		Debug.Log(json);


		string fullPath = Application.dataPath + "/SceneJson/" + scene.name + ".scenejson";
		Debug.Log(fullPath);
		System.IO.File.WriteAllText(fullPath, json);
	}

	private static void RecordObject(GameObject obj, List<ObjectRecord> list)
	{
		var mesh = obj.GetComponent<MeshFilter>();
		if (mesh != null)
		{
			ObjectRecord record = new ObjectRecord();

			record.name = obj.name;
			string modelName = mesh.sharedMesh.name;
			var index = modelName.LastIndexOf(" Instance");
			if(index > -1)
			{
				modelName = modelName.Remove(index);
			}
			record.model = modelName;
			record.position.X = obj.transform.position.x;
			record.position.Y = obj.transform.position.y;
			record.position.Z = obj.transform.position.z;
			record.rotation.X = obj.transform.eulerAngles.x * Mathf.Deg2Rad;
			record.rotation.Y = obj.transform.eulerAngles.y * Mathf.Deg2Rad;
			record.rotation.Z = obj.transform.eulerAngles.z * Mathf.Deg2Rad;
			record.scale.X = obj.transform.localScale.x;
			record.scale.Y = obj.transform.localScale.z;
			record.scale.Z = obj.transform.localScale.z;

			list.Add(record);
		}
	}


	private static void FindAllChildren(GameObject obj, List<GameObject> list)
	{
		list.Add(obj);

		foreach (Transform child in obj.transform)
		{
			FindAllChildren(child.gameObject, list);
		}
	}
}
