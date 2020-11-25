﻿using MLAPI;
using SDD.Events;
using System.Collections;
using UnityEngine;
using CommonVisibleManager;

namespace ServerManager
{
    public class ServerNetworkManager : ServerManager<ServerNetworkManager>
    {
        // Constante

        public static readonly int MAX_PLAYER_CONNECTED = 16;


        // Attributs

        [SerializeField] private LiteNetLibTransport.LiteNetLibTransport TransportSystem;
        private int NumberOfPlayerConnected;

        private bool IsStoppingServer;


        // Manager Implémentation

        protected override IEnumerator InitCoroutine()
        {
            if (TransportSystem == null)
                Debug.LogError("Client Manager mal paramétré");

            NumberOfPlayerConnected = 0;
            Setup(); // On initialise le serveur

            yield break;
        }


        // Event Subscription

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            if (NetworkingManager.Singleton != null)
            {
                NetworkingManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

                NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

                NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }


        // GameManager event call back

        protected override void GameMainMenu(GameMainMenuEvent e)
        {
            base.GameMainMenu(e);

            StartCoroutine("StopServer");
        }

        protected override void GameRoomMenu(GameRoomMenuEvent e)
        {
            base.GameRoomMenu(e);

            StartCoroutine("LaunchServer");
        }

        protected override void GameOptionsMenu(GameOptionsMenuEvent e)
        {
            base.GameOptionsMenu(e);

            StartCoroutine("StopServer");
        }

        protected override void GameCreditsMenu(GameCreditsMenuEvent e)
        {
            base.GameCreditsMenu(e);

            StartCoroutine("StopServer");
        }

        protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
        {
            base.GameMusicSelectionMenu(e);

            // On averti les téléphone connecté que l'on rentre dans la zone de music selection.
            MessagingManager.Instance.RaiseNetworkedEventOnAllClient(new ServerEnterInGameMusicSelectionEvent());
        }

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            MessagingManager.Instance.RaiseNetworkedEventOnAllClient(new GameStartedEvent());
        }


        // Outils

        private void Setup()
        {
            NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientConnected(ulong ClientID)
        {
            ServerConnectionSuccessEvent e = new ServerConnectionSuccessEvent();
            e.ClientID = ClientID;

            EventManager.Instance.Raise(e);

            ++NumberOfPlayerConnected;
        }

        private void OnClientDisconnected(ulong ClientID)
        {
            ServerDisconnectionSuccessEvent e = new ServerDisconnectionSuccessEvent();
            e.ClientID = ClientID;

            EventManager.Instance.Raise(e);

            --NumberOfPlayerConnected;
        }

        /// <summary>
        /// On accepte les joueurs que si nous somme dans un lobby
        /// </summary>
        /// <param name="connectionData"></param>
        /// <param name="clienID"></param>
        /// <param name="callback"></param>
        private void ApprovalCheck(byte[] connectionData, ulong clienID, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
        {
            bool approve = NumberOfPlayerConnected + 1 <= MAX_PLAYER_CONNECTED && ServerGameManager.Instance.GetGameState == GameState.gameLobby;
            bool createPlayerObject = false;

            callback(createPlayerObject, null, approve, null, null);
        }

        private IEnumerator LaunchServer()
        {
            if (IsStoppingServer)
            {
                yield break;
            }

            if (NetworkingManager.Singleton.IsServer)
            {
                throw new System.Exception("Serveur déjà créé");
            }

            TransportSystem.Address = IPManager.GetIP(ADDRESSFAM.IPv4);
            NetworkingManager.Singleton.StartServer();
        }

        private IEnumerator StopServer()
        {
            if (IsStoppingServer)
            {
                yield break;
            }

            if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer)
            {
                IsStoppingServer = true;

                NetworkingManager.Singleton.StopServer();
                TransportSystem.Address = "";

                IsStoppingServer = false;
            }
        }
    }
}


