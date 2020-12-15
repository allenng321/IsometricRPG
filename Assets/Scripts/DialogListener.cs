
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using Doozy.Engine;
// using Doozy.Engine.UI;
// using Cradle;
// using TMPro;
//
// public class DialogListener : MonoBehaviour
// {
//     public TextMeshProUGUI dialogText;
//     
//     private Story story;
//     private UIButton[] choices;
//
//     private void Awake()
//     {
//         choices = GetComponentsInChildren<UIButton>(true);
//         foreach (UIButton choice in choices)
//         {
//             choice.gameObject.SetActive(false);
//         }
//
//         Message.AddListener<GameEventMessage>("OPEN_DIALOG", OnOpenDialog);
//         
//         if (story == null) story = gameObject.GetComponent<Story>();
//         if (story == null)
//         {
//             Debug.LogError("Text player does not have a story to play, add a story script to the GameObject.");
//             return;
//         }
//         
//         story.OnOutput += Story_OnOutput;
//         story.OnPassageDone += Story_OnPassageDone;
//         story.OnPassageEnter += Story_OnPassageEnter;
//         story.Begin();        
//     }
//
//     private void OnDestroy()
//     {
//         Message.RemoveListener<GameEventMessage>("OPEN_DIALOG", OnOpenDialog);
//     }
//
//     private void OnOpenDialog(GameEventMessage message)
//     {
//     }
//     
//     void Story_OnOutput(StoryOutput output)
//     {
//         if (output is StoryText)
//         {
//             dialogText.text = output.Text;
//         }
//     }
//
//     void Story_OnPassageEnter(StoryPassage passage)
//     {
//         for (var i = 0; i < choices.Length; i++)
//         {
//             choices[i].gameObject.SetActive(false);
//         } 
//     }
//     
//     void Story_OnPassageDone(StoryPassage passage)
//     {
// //        Debug.Log($"On Passage Done: {passage.Name}");
//         IEnumerable<StoryLink> links = story.GetCurrentLinks();
// //        Debug.Log(links.Count());
//         for (var i = 0; i < links.Count(); i++)
//         {
//             StoryLink link = links.ElementAt(i);
//             choices[i].Button.onClick.AddListener(() =>
//             {
//                 story.DoLink(link);
//             });
//             choices[i].gameObject.SetActive(true);
//             choices[i].TextMeshProLabel.text = link.Text;
//         } 
//     }
// }
