namespace ClientManager
{
	using MLAPI;
	using SDD.Events;
	using System;
	using System.Collections;
	using UnityEngine;

	public class ClientNetworkManager : ClientManager<ClientNetworkManager>
	{
		// Attributs

		// Transport du NetworkingManager
		[SerializeField] private LiteNetLibTransport.LiteNetLibTransport transport;

		private ulong? PlayerID; // L'id du player pour le serveur.


		// Requete

		public ulong? GetPlayerID()
		{
			return PlayerID;
		}


		// Manager Implementation

		#region Implementation
		protected override IEnumerator InitCoroutine()
		{
			if (transport == null)
				Debug.LogError("Client Manager mal paramétré");

			Setup();

			yield break;
		}
		#endregion


		// Life Cycle

		protected override void OnDestroy()
		{
			base.OnDestroy();

			DisconnectionClient();
		}


		// Event sub

		#region Events' subscription
		public override void SubscribeEvents()
		{
			base.SubscribeEvents();

			EventManager.Instance.AddListener<ServerConnectionEvent>(Connect);
		}

		public override void UnsubscribeEvents()
		{
			base.UnsubscribeEvents();

			EventManager.Instance.RemoveListener<ServerConnectionEvent>(Connect);

			// Networking Manager

			if (NetworkingManager.Singleton != null)
			{
				NetworkingManager.Singleton.OnClientConnectedCallback -= ConnectionSuccess;

				NetworkingManager.Singleton.OnClientDisconnectCallback -= ConnectionStopped;
			}
		}
		#endregion


		// Outils

		private void Setup()
		{
			NetworkingManager.Singleton.OnClientConnectedCallback += ConnectionSuccess;

			NetworkingManager.Singleton.OnClientDisconnectCallback += ConnectionStopped;
		}

		private void Connect(ServerConnectionEvent e)
		{
			if (NetworkingManager.Singleton.IsClient)
			{
				throw new Exception("Client déjà connecté");
			}

			if (IPManager.ValidateIPv4(e.Adress))
			{
				transport.Address = e.Adress;
				NetworkingManager.Singleton.StartClient();
			}
			else
			{
				throw new Exception("AdresseIP Invalide");
			}

		}

		private void ConnectionSuccess(ulong ClientID)
		{
			PlayerID = ClientID;

			ServerConnectionSuccessEvent e = new ServerConnectionSuccessEvent();
			e.ClientID = ClientID;

			EventManager.Instance.Raise(e);
		}

		private void ConnectionStopped(ulong ClientID)
		{
			EventManager.Instance.Raise(new ServerClosedEvent());
		}

		private void DisconnectionClient()
		{
			if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
			{
				NetworkingManager.Singleton.StopClient();
				transport.Address = "";
			}

			PlayerID = null;
		}


		// GameManager Event call back

		protected override void MobileMainMenu(MobileMainMenuEvent e)
		{
			base.MobileMainMenu(e);

			DisconnectionClient();
		}

		protected override void MobileJoinRoom(MobileJoinRoomEvent e)
		{
			base.MobileJoinRoom(e);

			DisconnectionClient();
		}
	}

}

