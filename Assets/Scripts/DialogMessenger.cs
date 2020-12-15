// using UnityEngine;
// using Doozy.Engine;
// using Cradle;
//
// public class DialogMessenger : MonoBehaviour
// {
//     public string passageName;  // Indicates which twine story passage to initiate
//     private Story story;
//
//     void Start()
//     {
//         if (story == null)
//             // TODO Story Object discovery should be available after multiple scene loads
//             story = FindObjectOfType<Story>();
//         if (story == null)
//         {
//             Debug.LogError($"{gameObject.name} does not have a story to play, add a story script to DialogListener.");
//             return;
//         }
//     }
//
//     void OnTriggerEnter(Collider other)
//     {
//         story.GoTo(passageName);
//         GameEventMessage.SendEvent("OPEN_DIALOG");
//     }
//     
//     void OnTriggerExit(Collider other)
//     {
//         story.GoTo("Empty");
//         GameEventMessage.SendEvent("EXIT_DIALOG");
//     }
// }