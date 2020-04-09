﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class FogManager : MonoBehaviourPun, TurnManager.IOnMove
{
    
    // Start is called before the first frame update
    void Start()
    {
        TurnManager.Register(this);
        int[] regions = { 8, 11, 12, 13, 16, 32, 46, 44, 42, 64, 63, 56, 47, 48, 49 };
        Shuffle(regions);
        //TODO assign types to fogs
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("Typing", RpcTarget.AllBuffered, regions);
        }


    }

    void Shuffle(int[] list)
    {
        System.Random rand = new System.Random();
        int n = list.Length;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            int temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMove(Player player, Region currentRegion)
    {
        if (PhotonNetwork.LocalPlayer == player)
        {

            List<Fog> fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(currentRegion);

            if (fogOnRegion.Count > 0)
            {
                Hero hero = (Hero)player.GetHero();
                Uncover(currentRegion.label, (int)hero.type);
                
                //photonView.RPC("Encounter", RpcTarget.AllBuffered, currentRegion.label);

            }
            
        }

    }

    public void Uncover(int currentRegion, int heroType)
    {
        List<Fog> fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(currentRegion));
        Fog curr = fogOnRegion[0];

        if (curr.type == FogType.SP)//SP+1
        {
            photonView.RPC("SP", RpcTarget.AllBuffered, currentRegion, heroType);

        }
        /*else if (curr.type == FogType.TwoWP)//WP+2
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.GetHero();
            hero.data.WP += 2;
        }
        else if (curr.type == FogType.ThreeWP)//WP+3
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.GetHero();
            hero.data.WP += 3;
        }
        else if (curr.type == FogType.Gold)//Gold
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.GetHero();
            hero.data.gold += 1;
        }
        else if (curr.type == FogType.Event)//Event
        {
            myEvents.flipped();
        }
        else if (curr.type == FogType.Wineskin)//Wineskin
        {

        }
        else if (curr.type == FogType.Witch)//Witch
        {
            PhotonNetwork.Instantiate("Witch", transform.position, transform.rotation);
        }
        else if (curr.type == FogType.Monster)//Gor
        {
            PhotonNetwork.Instantiate("Gor", transform.position, transform.rotation);
        }
        fogIcon.enabled = false;*/
    }

    [PunRPC]
    public void SP(int currentRegion, int whichHero)
    {
        List<Fog> fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(currentRegion));
        Fog curr = fogOnRegion[0];

        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i<players.Length;i++)
        {
            Hero hero = (Hero)players[i].GetHero();
            if ((int)hero.type == whichHero)
            {
                hero.data.SP += 1;
                break;
            }
        }
        //make sure fog is removed
        curr.fogIcon.enabled = false;
        Destroy(curr);
    }



    /*[PunRPC]
    public void Encounter(int currentRegion)
    {
        List<Fog> fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(currentRegion));

        Fog curr = fogOnRegion[0];
        curr.uncover();

        //make sure fog is removed
        Destroy(curr);
    }*/





    [PunRPC]
    public void Typing(int[] whereTo)
    {
        int i;
        Fog curr;
        List<Fog> fogOnRegion;

        for (i=0; i<5;i++)
        {
            fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[i]));
            curr = fogOnRegion[0];
            curr.type = FogType.Event;
        }
        //5th is the SP default one

        fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[6]));
        curr = fogOnRegion[0];
        curr.type = FogType.TwoWP;

        fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[7]));
        curr = fogOnRegion[0];
        curr.type = FogType.ThreeWP;

        for (i=8; i < 11; i++)
        {
            fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[i]));
            curr = fogOnRegion[0];
            curr.type = FogType.Gold;
        }

        for (; i < 13; i++)
        {
            fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[i]));
            curr = fogOnRegion[0];
            curr.type = FogType.Monster;
        }

        fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[13]));
        curr = fogOnRegion[0];
        curr.type = FogType.Wineskin;

        fogOnRegion = GameGraph.Instance.FindObjectsOnRegion<Fog>(GameGraph.Instance.Find(whereTo[14]));
        curr = fogOnRegion[0];
        curr.type = FogType.Witch;

    }


}
