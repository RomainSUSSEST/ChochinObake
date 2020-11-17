
using System;

namespace SDD.Events {

    /// <summary>
    /// Base event for all EventManager events. Use for local event
    /// </summary>
    public abstract class Event {
    }

    #region Networked Event
    // Use for Networked Event
    public abstract class NetworkedEvent : Event
    {
        // Attributs

        public ulong? PlayerID { get; }

        public Argument[] args { get; }


        // Constructeur

        public NetworkedEvent() : this(null, null)
        {
        }

        public NetworkedEvent(ulong? playerID) : this(playerID, null)
        {
        }

        public NetworkedEvent(params Argument[] args) : this(null, args)
        {
        }

        public NetworkedEvent(ulong? playerID, params Argument[] args)
        {
            this.args = args;
            this.PlayerID = playerID;
        }


        // Requete

        /**
         * <summary>
         * Renvoie true si l'event concerne le playerId donné en argument.
         * </summary>
         */
        public bool DoesThisConcernMe(ulong playerId)
        {
            if (PlayerID == null)
            {
                return true;
            } else
            {
                return PlayerID.Value == playerId;
            }
        }
    }

    /**
     * L'objet doit être serialisable
     */
    [Serializable] public struct Argument
    {
        public object Arg { get; set; }

        private Type _type;
        public Type Type { get
            {
                return _type;
            }
            set
            {
                if (value.IsSerializable)
                {
                    _type = value;
                } else
                {
                    throw new Exception("Type non serializable");
                }
            }
        }
    }
    #endregion
}
