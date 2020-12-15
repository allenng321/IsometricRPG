// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using TMPro;
//
// public class GameConsole : MonoBehaviour
// {
//     public Dictionary<string, string> snippets;
//     public Doozy.Engine.UI.UIButton[] buttons;
//     public TextMeshProUGUI backScroll;
//     public TextAsset textFromFile;
//
//     void Start()
//     {
//         backScroll = gameObject.GetComponent<TextMeshProUGUI>();
//         snippets = new Dictionary<string, string>();
//         textFromFile = Resources.Load("MenuLog") as TextAsset;
//         backScroll.text = TextAssetToList(textFromFile)[0];
//         buttons = FindObjectsOfType<Doozy.Engine.UI.UIButton>();
//
//         for (int i = 0; i < buttons.Length; ++i)
//         {
//             snippets.Add(buttons[i].name, System.String.Format("{0} notepad.exe", buttons[i].name));
//         }
//     }
//
//     private List<string> TextAssetToList(TextAsset ta)
//     {
//         return new List<string>(ta.text.Split('\n'));
//     }
//
//     public void AddTextToBackScroll()
//     {
//         backScroll.text += EventSystem.current.currentSelectedGameObject.name;
//     }
// }