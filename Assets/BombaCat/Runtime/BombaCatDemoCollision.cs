using UnityEngine;
using UnityEngine.SceneManagement;
public static class BombaCatDemoCollision
{
 public static void Install(Scene scene)
 {
  if(scene.name!="Level2") return;
  foreach(var root in scene.GetRootGameObjects()) {
   if(root.name!="Buildings") continue;
   foreach(var mesh in root.GetComponentsInChildren<MeshFilter>(true)) {
    if(mesh.sharedMesh==null || mesh.GetComponent<Renderer>()==null || mesh.GetComponent<Collider>()!=null || mesh.GetComponentInParent<Rigidbody>()!=null) continue;
    mesh.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;
   }
  }
  Physics.SyncTransforms();
 }
}
