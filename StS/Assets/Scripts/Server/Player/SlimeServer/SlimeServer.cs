﻿using SDD.Events;
using UnityEngine;

public class SlimeServer : Slime
{
    // Attributs

    public ulong AssociedClientID { get; set; }


    // Life cycle

    private void Awake()
    {
        SubscribeEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }


    // Méthode

    #region Event subscription
    private void SubscribeEvent()
    {
        // ClientInputsManager
        //EventManager.Instance.AddListener<SwipeUpEvent>(SwipeUp);
        //EventManager.Instance.AddListener<SwipeLeftEvent>(SwipeLeft);
        //EventManager.Instance.AddListener<SwipeRightEvent>(SwipeRight);
        //EventManager.Instance.AddListener<SwipeDownEvent>(SwipeDown);

        //EventManager.Instance.AddListener<DoublePressEvent>(DoublePress);

        //EventManager.Instance.AddListener<TiltLeftRightEvent>(TiltLeftRight);
        //EventManager.Instance.AddListener<TiltTopBottomEvent>(TiltTopBottom);

        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }

    private void UnsubscribeEvent()
    {
        // ClientInputsManager
        //EventManager.Instance.RemoveListener<SwipeUpEvent>(SwipeUp);
        //EventManager.Instance.RemoveListener<SwipeLeftEvent>(SwipeLeft);
        //EventManager.Instance.RemoveListener<SwipeRightEvent>(SwipeRight);
        //EventManager.Instance.RemoveListener<SwipeDownEvent>(SwipeDown);

        //EventManager.Instance.RemoveListener<DoublePressEvent>(DoublePress);

        //EventManager.Instance.RemoveListener<TiltLeftRightEvent>(TiltLeftRight);
        //EventManager.Instance.RemoveListener<TiltTopBottomEvent>(TiltTopBottom);

        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }
    #endregion


    // Outils

    #region EventCallBack
    private void OnClientDisconnected(ServerDisconnectionSuccessEvent e)
    {
        if (e.ClientID == this.AssociedClientID)
        {
            Destroy(this.gameObject);
        }
    }

    //private void SwipeUp(SwipeUpEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        Jump();
    //    }
    //}

    //private void SwipeRight(SwipeRightEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        AttackRight();
    //    }
    //}

    //private void SwipeLeft(SwipeLeftEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        AttackLeft();
    //    }
    //}

    //private void SwipeDown(SwipeDownEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        DownAction();
    //    }
    //}

    //private void DoublePress(DoublePressEvent e)
    //{
    //}

    //private void TiltLeftRight(TiltLeftRightEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        MovementIntensityX = e.intensity * Speed;
    //    }
    //}

    //private void TiltTopBottom(TiltTopBottomEvent e)
    //{
    //    if (e.DoesThisConcernMe(AssociedClientID))
    //    {
    //        MovementIntensityZ = e.intensity * Speed;
    //    }
    //}

    #endregion
}
