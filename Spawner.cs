using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    // public MeshGenerator mg;
    // public GameObject[] chunks;

    void Awake(){
        // MeshGenerator.loadData();
    }
    // Start is called before the first frame update
    void Start()
    {
        // chunks = new GameObject[9];
        for(int x = 0; x < 3; x++){
            for(int z = 0; z < 3; z++){
                // chunks[z*3 + x] = ;
                GameObject chunk = Instantiate(prefab,new Vector3(x*250,0,z*250), Quaternion.identity, parent.transform);
                if(chunk != null){
                    MeshGenerator mg = chunk.GetComponent<MeshGenerator>();
                    if(mg != null){
                        mg.SetOffset(x*250,z*250);
                    }
                }
                // GameObject.Find(chunk.name).GetComponent<MeshGenerator>().SetOffset(x*250,z*250);
                // chunk.transform.SetParent(gameObject.transform);
                // chunks[z*3 + x] = chunk;

            }
        }
        // Instantiate(prefab,new Vector3(0,0,0), Quaternion.identity).GetComponent<MeshGenerator>().SetOffset(0,0);
        // Instantiate(prefab,new Vector3(250,0,0), Quaternion.identity).GetComponent<MeshGenerator>().SetOffset(250,0);
        // Instantiate(prefab,new Vector3(0,0,250), Quaternion.identity).GetComponent<MeshGenerator>().SetOffset(0,250);
        // Instantiate(prefab,new Vector3(250,0,250), Quaternion.identity).GetComponent<MeshGenerator>().SetOffset(250,250);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Do Something")]
    void instantiate(){
        Instantiate(prefab);
    }
}



// public Transform[] yourPrefabVariations;
// public int instanceCount = 100;
 
// // You might even want to keep track of each instance:
// private List<Transform> _instances = new List<Transform>();
 
// private void CreateInstances() {
//     for (int i = 0; i < instanceCount; ++i) {
//         // Randomly pick prefab variation:
//         var yourPrefab = yourPrefabVariations[ Random.Range(0, yourPrefabVariations.Length) ];
 
//         var instance = Instantiate(yourPrefab) as Transform;
//         _instances.Add(instance);
 
//         // Then adjust your position as needed.
//         instance.localPosition = .....
//     }
// }
