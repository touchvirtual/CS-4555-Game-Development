using UnityEngine;
[DefaultExecutionOrder(100)]
public sealed class BombaCatConversationView : MonoBehaviour
{
 Camera cameraRef; CameraFollow follow; DialogueUI dialogue;
 bool active, wasEnabled; Vector3 saved, destination, focus; Quaternion rotation;
 void LateUpdate()
 {
  if(dialogue==null) dialogue=GetComponent<DialogueUI>();
  if(dialogue==null || !dialogue.IsTalking || dialogue.Speaker==null) {Restore(); return;}
  if(!active) {
   cameraRef=Camera.main; var player=BombaCatCheckpointService.ActivePlayer;
   if(cameraRef==null || player==null) return;
   active=true; saved=cameraRef.transform.position; rotation=cameraRef.transform.rotation;
   follow=cameraRef.GetComponent<CameraFollow>(); wasEnabled=follow!=null && follow.enabled; if(follow!=null) follow.enabled=false;
   Vector3 a=player.transform.position+Vector3.up*.5f,b=dialogue.Speaker.position+Vector3.up;
   focus=(a+b)*.5f; Vector3 side=Vector3.Cross(Vector3.up,b-a).normalized;
   if(side.sqrMagnitude<.1f) side=-cameraRef.transform.forward;
   if(Vector3.Dot(side,saved-focus)<0) side=-side;
   float distance=Mathf.Max(4f,Vector3.Distance(a,b)*1.2f+2f);
   destination=focus+side*distance+Vector3.up*1.4f;
   if(Physics.Linecast(focus,destination,out var hit,~(1<<5),QueryTriggerInteraction.Ignore) && hit.distance<distance*.7f) destination=focus-side*distance+Vector3.up*1.4f;
  }
  if(Time.timeScale<=0) return;
  float blend=1f-Mathf.Exp(-8f*Time.unscaledDeltaTime);
  cameraRef.transform.position=Vector3.Lerp(cameraRef.transform.position,destination,blend);
  cameraRef.transform.rotation=Quaternion.Slerp(cameraRef.transform.rotation,Quaternion.LookRotation(focus-cameraRef.transform.position),blend);
  var panel=dialogue.dialoguePanel.GetComponent<RectTransform>(); if(!(panel.parent is RectTransform parent)) return;
  var canvas=panel.GetComponentInParent<Canvas>();
  RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,cameraRef.WorldToScreenPoint(dialogue.Speaker.position+Vector3.up*2f),canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var point);
  point.x=Mathf.Clamp(point.x,parent.rect.xMin+220,parent.rect.xMax-220); point.y=Mathf.Clamp(point.y,parent.rect.yMin+200,parent.rect.yMax-155);
  panel.anchoredPosition=point;
 }
 void Restore(){if(!active)return; if(cameraRef!=null)cameraRef.transform.SetPositionAndRotation(saved,rotation);if(follow!=null)follow.enabled=wasEnabled;active=false;}
 void OnDisable(){Restore();}
}
