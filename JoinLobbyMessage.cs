using System;
using System.IO;
using System.Collections.Generic; // Required for IMatchmakingCallbacks
using BepInEx;
using Photon.Pun;          // Required for PhotonNetwork
using Photon.Realtime;     // Required for Player and IMatchmakingCallbacks
using UnityEngine;
using Utilla;              // Import Utilla's namespace

namespace JoinLobbyMessage
{
    // Add BepInDependency to make sure Utilla loads first
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    // Add ModdedGamemode to tell Utilla this mod only works in modded rooms
    [ModdedGamemode]
    [BepInPlugin("com.yourname.joinlobbymessage", "Join Lobby Message", "1.2.0")]
    // We NEED IMatchmakingCallbacks to detect when other players join
    public class JoinLobbyMessage : BaseUnityPlugin, IMatchmakingCallbacks
    {
        private string logFilePath;
        private float popupTime;
        private string popupText;
        private Color popupColor; // Store the color for the popup
        private bool isInModdedRoom = false; // Track if we are in a modded room

        // BepInEx's built-in logger
        private BepInEx.Logging.ManualLogSource pluginLogger;

        private void Awake()
        {
            pluginLogger = Logger; // Get the logger instance from BaseUnityPlugin
            
            // Note: Paths.GameRootPath is for BepInEx 5. 
            // If you use BepInEx 6, change this to Paths.GameRoot
            logFilePath = Path.Combine(Paths.GameRootPath, "JoinLobbyMessage.log");
            Log("JoinLobbyMessage plugin loaded and waiting for Utilla.");
            
            // We MUST register for Photon callbacks to hear when other players join
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            // Always good practice to remove the callback target on destroy
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        // ===== Utilla Callbacks =====

        /// <summary>
        /// This method is AUTOMATICALLY called by Utilla when YOU join a modded room.
        /// </summary>
        [ModdedGamemodeJoin]
        private void OnModdedRoomJoined(string gamemode)
        {
            isInModdedRoom = true; // Set our flag
            string message = $"[{DateTime.Now:HH:mm:ss}] Joined a MODDED ROOM (Queue: {gamemode})";
            Log(message);
            // Show a green message for our own join
            ShowPopup("Joined a Modded Room!", Color.green);
        }

        /// <summary>
        /// This method is AUTOMATICALLY called by Utilla when YOU leave a modded room.
        /// </summary>
        [ModdedGamemodeLeave]
        private void OnModdedRoomLeft()
        {
            isInModdedRoom = false; // Unset our flag
            Log("Left the modded room.");
        }

        // ===== Photon Callbacks =====

        /// <summary>
        /// This is called by Photon when ANY player joins the room (including you).
        /// </summary>
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            // We only care if we are in a modded room AND the new player is NOT us
            if (isInModdedRoom && !newPlayer.IsLocal)
            {
                string message = $"{newPlayer.NickName} has joined the lobby";
                Log(message);
                // Show a purple message for other players
                ShowPopup(message, Color.magenta); // Color.magenta is Unity's purple
            }
        }
        
        // --- Other callbacks we must implement to satisfy IMatchmakingCallbacks ---
        public void OnPlayerLeftRoom(Player otherPlayer) { /* We could add a leave message here */ }
        public void OnFriendListUpdate(List<FriendInfo> friendList) { /* ignore */ }
        public void OnCreatedRoom() { /* ignore */ }
        public void OnCreateRoomFailed(short returnCode, string message) { /* ignore */ }
        public void OnJoinedRoom() { /* Utilla handles this for us */ }
        public void OnJoinRoomFailed(short returnCode, string message) { /* ignore */ }
        public void OnJoinRandomFailed(short returnCode, string message) { /* ignore */ }
        public void OnLeftRoom() { /* Utilla handles this for us */ }


        // ===== Popup Display (Now accepts a color) =====

        private void ShowPopup(string text, Color color)
        {
            popupText = text;
            popupColor = color; // Store the desired color
            popupTime = Time.time + 3f;
        }

        private void OnGUI()
        {
            if (Time.time < popupTime && !string.IsNullOrEmpty(popupText))
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 30;
                style.normal.textColor = popupColor; // Use the stored color
                style.alignment = TextAnchor.MiddleCenter;

                // Made the box wider (500) to fit long player names
                GUI.Label(new Rect(Screen.width / 2 - 250, 50, 500, 60), popupText, style);
            }
        }

        // ===== Logging (No changes) =====

        private void Log(string message)
        {
            try
            {
                string fullMessage = $"[JoinLobbyMessage] {message}";
                pluginLogger.LogInfo(fullMessage); // Use BepInEx logger
                File.AppendAllText(logFilePath, fullMessage + Environment.NewLine);
            }
            catch (Exception e)
            {
                pluginLogger.LogError($"[JoinLobbyMessage] Failed to write to log file: {e}");
            }
        }
    }
}