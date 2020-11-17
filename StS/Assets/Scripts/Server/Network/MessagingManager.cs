namespace CommonVisibleManager
{
    using MLAPI;
    using MLAPI.Messaging;
    using MLAPI.Serialization;
    using SDD.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using UnityEngine;

    /**
     * Permet d'envoyer des events et  sur le réseau
     */
    public class MessagingManager : NetworkedBehaviour
    {
        #region Singleton
        private static MessagingManager _instance;
        public static MessagingManager Instance
        {
            get { return _instance; }
        }

        [Header("Singleton")]
        [SerializeField]
        private bool m_DoNotDestroyGameObjectOnLoad;

        protected virtual void Awake()
        {
            if (_instance != null)
                Destroy(gameObject);

            _instance = this as MessagingManager;

            if (m_DoNotDestroyGameObjectOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        #endregion


        // Méthode

        public void RaiseNetworkedEventOnServer(NetworkedEvent e)
        {

            // La généricité ne fonctionnant pas avec InvokeServerRpc, je suis obligé d'envoyé le type en argument.
            MessageData msg = new MessageData(e);

            InvokeServerRpc(RaiseEventOnServer, msg);
        }

        public void RaiseNetworkedEventOnClient(NetworkedEvent e)
        {
            MessageData msg = new MessageData(e);

            if (e.PlayerID == null)
            {
                throw new Exception("Player ID null");
            }
            else
            {
                InvokeClientRpcOnClient(RaiseEventOnClient, e.PlayerID.Value, msg);
            }
        }

        public void RaiseNetworkedEventOnAllClient(NetworkedEvent e)
        {
            MessageData msg = new MessageData(e);

            InvokeClientRpcOnEveryone(RaiseEventOnClient, msg);
        }


        // Outils

        [ServerRPC(RequireOwnership = false)]
        private void RaiseEventOnServer(MessageData e)
        {
            EventManager.Instance.Raise(e.GetEventInstance());
        }

        [ClientRPC]
        private void RaiseEventOnClient(MessageData e)
        {
            EventManager.Instance.Raise(e.GetEventInstance());
        }


        // Classe imbriquée

        /**
         * @cons
         *  @pre typeof(e.GetType()) : MobileInputs
         */
        [Serializable]
        private class MessageData : IBitWritable
        {
            // Attribut

            private Type Type { get; set; }
            private ulong? PlayerID;
            private Argument[] args;


            // Constructeur

            // La généricité ne fonctionnant pas avec InvokeServerRpc, je suis obligé d'envoyé le type en argument.
            public MessageData(NetworkedEvent m)
            {
                Type = m.GetType();
                args = m.args;
                PlayerID = m.PlayerID;
            }

            public MessageData()
            {
                Type = null;
                args = null;
                PlayerID = null;
            }


            // Méthodes

            public void Read(Stream stream)
            {
                byte[] TamponSize = new byte[sizeof(int)];
                byte[] TamponResult;

                long sizeRead = 0;
                int sizeofInt = sizeof(int);

                int sizeToRead;

                int NbrObjectRead = 0;

                bool hasPlayerID = false;

                int checkResult = 0;
                int argumentInformation = 2;

                // Variable pouvant stocker les potentiels arguments.

                List<Argument> arguments = new List<Argument>();
                Type CurrentTypeArg = null;
                object CurrentArg = null;

                while ((sizeRead = stream.Read(TamponSize, 0, sizeofInt)) > 0 && stream.Position + 1 < stream.Length) // On lit 2 par 2
                {
                    if (sizeRead != sizeofInt)
                    {
                        throw new Exception("Erreur de lecture 1");
                    }

                    sizeToRead = BitConverter.ToInt32(TamponSize, 0);

                    TamponResult = new byte[sizeToRead];
                    if (stream.Read(TamponResult, 0, sizeToRead) != sizeToRead)
                    {
                        throw new Exception("Erreur de lecture 2");
                    }
                    else
                    {
                        NbrObjectRead += 1;

                        if (NbrObjectRead == 1) // On recoit le type en premier
                        {
                            Type = (Type)ByteArrayToObject(TamponResult);

                        }
                        else if (NbrObjectRead == 2) // Ensuite on recoit le boolean indiquant si il y a un player ID ou non
                        {
                            hasPlayerID = BitConverter.ToBoolean(TamponResult, 0);

                        }
                        else if (NbrObjectRead == 3)
                        {
                            // Soit il y a un player ID
                            if (hasPlayerID)
                            {
                                PlayerID = BitConverter.ToUInt32(TamponResult, 0);
                                checkResult = 0;
                            }
                            else // Soit il n'y en a pas et dans ce cas, on écoute les arguments.
                            {
                                checkResult = 1;
                                if (NbrObjectRead % argumentInformation == checkResult)
                                {
                                    CurrentTypeArg = (Type)ByteArrayToObject(TamponResult);
                                }
                            }
                        }
                        else // Enfin, on recoit les arguments.
                        {
                            if (NbrObjectRead % argumentInformation == checkResult)
                            {
                                CurrentTypeArg = (Type)ByteArrayToObject(TamponResult);
                            }
                            else
                            {
                                CurrentArg = Convert.ChangeType(ByteArrayToObject(TamponResult), CurrentTypeArg);
                                arguments.Add(new Argument()
                                {
                                    Arg = CurrentArg,
                                    Type = CurrentTypeArg
                                });
                            }
                        }
                    }
                }

                if (arguments.Count > 0)
                {
                    args = arguments.ToArray();
                }
            }

            /**
             * Type == null => throw Exception
             */
            public void Write(Stream stream)
            {
                if (Type == null)
                {
                    throw new Exception("Message vide");
                }

                int size;
                int sizeofInt = sizeof(int);

                // On écrit le type de l'objet

                byte[] tampon = ObjectToByteArray(Type);
                size = tampon.Length;

                stream.Write(BitConverter.GetBytes(size), 0, sizeofInt);
                stream.Write(tampon, 0, size);

                // On écrit si il existe un player ID ou non.

                tampon = BitConverter.GetBytes(PlayerID.HasValue);
                size = tampon.Length;

                stream.Write(BitConverter.GetBytes(size), 0, sizeofInt);
                stream.Write(tampon, 0, size);

                // On écrit le player ID si il existe

                if (PlayerID.HasValue)
                {
                    tampon = BitConverter.GetBytes(PlayerID.Value);
                    size = tampon.Length;

                    stream.Write(BitConverter.GetBytes(size), 0, sizeofInt);
                    stream.Write(tampon, 0, size);
                }

                // On écrit les arguments si il en existe.

                if (args != null)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        // On écrit le type

                        tampon = ObjectToByteArray(args[i].Type);
                        size = tampon.Length;

                        stream.Write(BitConverter.GetBytes(size), 0, sizeofInt);
                        stream.Write(tampon, 0, size);

                        // On écrit l'objet arg

                        tampon = ObjectToByteArray(args[i].Arg);
                        size = tampon.Length;

                        stream.Write(BitConverter.GetBytes(size), 0, sizeofInt);
                        stream.Write(tampon, 0, size);
                    }
                }
            }

            /**
             * Type == null => Throw Exception
             */
            public NetworkedEvent GetEventInstance()
            {
                if (Type == null)
                {
                    throw new Exception("Message vide");
                }

                if (PlayerID == null && args == null)
                {
                    return (NetworkedEvent)Activator.CreateInstance(Type);

                }
                else if (PlayerID != null && args == null)
                {
                    return (NetworkedEvent)Activator.CreateInstance(Type, PlayerID);

                }
                else if (PlayerID == null && args != null)
                {
                    object[] tampon = new object[args.Length];

                    // On ajoute les arguments
                    for (int i = 0; i < args.Length; ++i)
                    {
                        tampon[i] = args[i].Arg;
                    }

                    return (NetworkedEvent)Activator.CreateInstance(Type, tampon);

                }
                else // PlayerID != null && args != null
                {
                    object[] tampon = new object[args.Length + 1];

                    // On ajoute d'abord le PlayerID

                    tampon[0] = PlayerID;

                    // Puis on ajoute les autres arguments.
                    for (int i = 0; i < args.Length; ++i)
                    {
                        tampon[i + 1] = args[i].Arg;
                    }

                    return (NetworkedEvent)Activator.CreateInstance(Type, tampon);
                }
            }


            // Outils

            private byte[] ObjectToByteArray(object obj)
            {
                if (obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }

            private object ByteArrayToObject(byte[] arrBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                object obj = binForm.Deserialize(memStream);

                return obj;
            }
        }
    }

}
