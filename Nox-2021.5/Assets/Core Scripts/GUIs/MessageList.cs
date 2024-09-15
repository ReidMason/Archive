using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

namespace NoxCore.GUIs
{
	//  MessageList.cs
	//  From the Unity Wiki
	//  Use with TimedFadeText.cs
	//  Attach to an emtpy Game Object
	//  Based on the work of capnbishop
	//  Conversion to csharp by CorrodedSoul
	
	public class MessageList : MonoBehaviour 
	{
		public GameObject messagePrefab;									//The prefab for our text object;
		
		public float lineSize = 20.0f;									//Pixel spacing between lines;
		public Vector3 startingPos = new  Vector3(10, 20, 0);
		public int layerTag = 0;
		public bool insertAbove = true;
		
		public List<GameObject> _messages;								//Using a List<> instead of a JS dynamic array
		private float _directionFactor = 1.0f;

        public Transform messagePanel;
		
		#region Singleton
		/// <summary>
		///   Provide singleton support for this class.
		///   The script must still be attached to a game object, but this will allow it to be called
		///   from anywhere without specifically identifying that game object.
		/// </summary>
		private static MessageList instance;
		
		public static MessageList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (MessageList)FindObjectOfType(typeof(MessageList));
					if (instance == null)
						D.error("Content: {0}", "There needs to be one active MessageList script on a GameObject in your scene.");
				}
				return instance;
			}
		}
		#endregion
		
		void Awake() 
		{
			// First make sure we have a prefab set. If not, disable the script
			if (messagePrefab == null)
			{
				enabled = false;
				D.warn("GUI: {0}", "Must set the message prefab for MessageList");
			}
			
			if (insertAbove) 
			{
				_directionFactor = 1.0f;
			}
			else
			{
				_directionFactor = -1.0f;
			}
			
			_messages = new List<GameObject>();
		}
		
		
		/// <summary>
		/// AddMessage() accepts a text value and adds it as a status message.
		/// All other status messages will be moved along the y axis by a normalized distance of lineSize.
		/// AddMessage() also handles automatic removing of any GUIText objects that automatically destroy
		/// themselves.
		/// </summary>
		public void AddMessage(string messageText, Color? colour = null) 
		{
			// Iterate though the messages, removing any that don't exist anymore, and moving the rest
			for (int i = 0; i < _messages.Count; i++)
			{
				// If this message is null, remove it, drop back the index count, and jump back to the begining of the loop
				if (ObjectPool.IsSpawned(_messages[i].gameObject) == false)
				//if (_messages[i].gameObject.activeSelf == false)
				{
					// Debug.Log("Removing message: " + _messages[i].text);
					_messages.RemoveAt(i);
					i--;
					continue;
				}
				
				_messages[i].transform.position += new Vector3(0,_directionFactor * (lineSize/Screen.height),0);
			}

            //  All the existing messages have been moved, making room for the new one.
            //  Instantiate a new message from the prefab, set it's text value, and add it to the
            //  array of messages.
            if (messagePrefab != null)
            {
                GameObject messageLabel = messagePrefab.Spawn();

                messageLabel.transform.SetParent(messagePanel);

                RectTransform trans = messageLabel.GetComponent<RectTransform>();
                trans.anchoredPosition = new Vector2(startingPos.x / Screen.width, startingPos.y / Screen.height);

                Text newMessage = messageLabel.GetComponent<Text>();
                newMessage.text = messageText;
                newMessage.color = colour.GetValueOrDefault(Color.white);
                // D.log("GUI", "Message: " + newMessage.text);
                newMessage.gameObject.layer = layerTag;
                _messages.Add(messageLabel);
            }
		}
	}
}
