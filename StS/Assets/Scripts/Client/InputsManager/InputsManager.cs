namespace ClientManager
{
    using CommonVisibleManager;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class InputsManager : ClientManager<InputsManager>
    {
        // Constante

        // Les Swipes
        [SerializeField] private float SWIPE_THRESHOLD = 20f;

        // L'inclinaison
        //[SerializeField] private float TILT_THRESHOLD = 0.05f;
        //[SerializeField] private float REFRESH_TILT_DELAY = 0.75f;
        //[SerializeField] private float DEFAULT_INCLINAISON = 0.3f;

        // Les appuies sur écran
        [SerializeField] private float MAX_DELAI_2PRESS = 1;


        // Attributs

        private Vector2 fingerDown;
        private Vector2 CurrentFingerPosition;

        private int NbrPress;
        private float TimeFirstPress;

        // TO DO : Remplacer par un arbre binaire de recherche ?
        private List<int> DisableSwipeTouchId;

        private ulong ClientID;


        // Méthodes

        protected override IEnumerator InitCoroutine()
        {
            DisableSwipeTouchId = new List<int>();
            NbrPress = 0;

            yield break;
        }

        #region Events' subscription
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
        }
        #endregion

        #region Callback to Event Manager
        protected override void MobileGamePlay(MobileGamePlayEvent e)
        {
            base.MobileGamePlay(e);

            // On récupére l'id du joueur
            ClientID = ClientNetworkManager.Instance.GetPlayerID().Value;

            StartCoroutine("ListenInputs");
            //StartCoroutine("CheckTilt");
        }
        #endregion


        // Coroutine

        private IEnumerator ListenInputs()
        {
            while (ClientGameManager.Instance.GetGameState == GameState.gamePlay)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        fingerDown = touch.position;
                        CurrentFingerPosition = touch.position;

                        NbrPress += 1; // On enregistre un clique.
                        CheckPress();

                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        CurrentFingerPosition = touch.position;
                        if (checkSwipe(touch))
                        {
                            NbrPress = 0;
                        }

                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        DisableSwipeTouchId.Remove(touch.fingerId);
                    }
                }

                yield return new CoroutineTools.WaitForFrames(1);
            }
        }

        //private IEnumerator CheckTilt()
        //{

        //    while (IsPlaying)
        //    {
        //        if (Mathf.Abs(Input.acceleration.x) > TILT_THRESHOLD)
        //        {
        //            OnTiltLeftRight(Input.acceleration.x);
        //        }
        //        else
        //        {
        //            OnTiltLeftRight(0);
        //        }

        //        float z = Input.acceleration.z + DEFAULT_INCLINAISON;
        //        if (Mathf.Abs(z) > TILT_THRESHOLD)
        //        {
        //            OnTiltTopBottom(z); // Permet de tenir le telephone légerement incliné de base
        //        }
        //        else
        //        {
        //            OnTiltTopBottom(0);
        //        }
        //        yield return new WaitForSeconds(REFRESH_TILT_DELAY);
        //    }

        //}


        // Outils


        private void CheckPress()
        {
            if (NbrPress == 1)
            {
                // On enregistre le time du premier press
                TimeFirstPress = Time.time;
            }
            else
            {
                if (TimeBeetween(Time.time, TimeFirstPress) < MAX_DELAI_2PRESS)
                {
                    NbrPress = 0;
                    OnDoublePress();
                }
                else
                {
                    TimeFirstPress = Time.time;
                    NbrPress = 1;
                }
            }
        }

        /**
         * SWIPE_THRESHOLD correspond à la distance parcourut vers gauche/droite ou haut/bas et non pas à la distance parcourut.
         * Pourquoi ? Car on fait juste une soustraction au lieu de 2 multiplications et une addition.
         * D'autant que cela ne change pas grand chose sur le confort de jeu.
         * 
         * @post if (swipe) => DisableSwipeTouchId.Contains(touch.fingerId)
         *                  => return true
         *       else
         *          return false
         */
        private bool checkSwipe(Touch touch)
        {
            if (DisableSwipeTouchId.Contains(touch.fingerId))
            {
                return false;
            }

            float vMove = verticalMove();
            float hMove = horizontalValMove();

            //Check if Vertical swipe
            if (vMove > SWIPE_THRESHOLD && vMove > hMove)
            {
                if (fingerDown.y - CurrentFingerPosition.y < 0) // up swipe
                {
                    OnSwipeUp();
                }
                else // Down swipe
                {
                    OnSwipeDown();
                }
                DisableSwipeTouchId.Add(touch.fingerId);
                return true;
            }

            //Check if Horizontal swipe
            else if (horizontalValMove() > SWIPE_THRESHOLD && horizontalValMove() > verticalMove())
            {
                if (fingerDown.x - CurrentFingerPosition.x < 0) //Right swipe
                {
                    OnSwipeRight();
                }
                else //Left swipe
                {
                    OnSwipeLeft();
                }
                DisableSwipeTouchId.Add(touch.fingerId);
                return true;
            }
            else
            {
                // No swipe
                return false;
            }
        }

        private float verticalMove()
        {
            return Mathf.Abs(fingerDown.y - CurrentFingerPosition.y);
        }

        private float horizontalValMove()
        {
            return Mathf.Abs(fingerDown.x - CurrentFingerPosition.x);
        }

        private float TimeBeetween(float t1, float t2)
        {
            return Mathf.Abs(t1 - t2);
        }

        #region CALLBACK FUNCTIONS
        private void OnSwipeUp()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new SwipeUpEvent(ClientID));
        }

        private void OnSwipeDown()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new SwipeDownEvent(ClientID));
        }

        private void OnSwipeLeft()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new SwipeLeftEvent(ClientID));
        }

        private void OnSwipeRight()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new SwipeRightEvent(ClientID));
        }

        private void OnDoublePress()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new DoublePressEvent(ClientID));
        }

        //private void OnTiltLeftRight(float intensity)
        //{

        //    MessagingManager.Instance.RaiseNetworkedEventOnServer(new TiltLeftRightEvent(ClientID, intensity));
        //}

        //private void OnTiltTopBottom(float intensity)
        //{
        //    MessagingManager.Instance.RaiseNetworkedEventOnServer(new TiltTopBottomEvent(ClientID, intensity));

        //}
        #endregion
    }
}