using TMPro;
using UnityEngine;


public class DialoguePanelUI : MonoBehaviour
{
 [Header("Components")] 
 [SerializeField] private GameObject contentParent;

 [SerializeField] private TextMeshProUGUI dialogueText;

 private void Awake()
 {
  contentParent.SetActive(false);
  ResetPanel();
 }

 private void OnEnable()
 {
     //GameEventsManager.instance.dialogueEvents.onDialogueStarted += DialogueStarted();
     //GameEventsManager.instance.dialogueEvents.onDialogueFinished += DialogueFinished();
     //GameEventsManager.instance.dialogueEvents.onDisplayDialogue += DisplayDialogue();
 }

 private void DialogueStarted()
 {
   contentParent.SetActive(true);
 }

 private void DialogueFinished()
 {
   contentParent.SetActive(false);
   ResetPanel();
 }

 private void DisplayingDialogue(string dialogueLine)
 {
     dialogueText.text = dialogueLine;
 }

 private void ResetPanel()
 {
     dialogueText.text = "";
 }
 
}
