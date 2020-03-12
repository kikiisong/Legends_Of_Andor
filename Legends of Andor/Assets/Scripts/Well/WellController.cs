﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class WellController : MonoBehaviourPun, TurnManager.IOnMove
{
    public GameObject drinkButton;
    public GameGraph gameGraph;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
         drinkButton = GameObject.Find("drinkButton");
         drinkButton.SetActive(false);
         TurnManager.Register(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMove(Player player, Region currentRegion)
    {
        if (PhotonNetwork.LocalPlayer == player)
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.CustomProperties[K.Player.hero];//photonView.Owner is the Scene

            List<Well> wellOnRegion = gameGraph.FindObjectsOnRegion<Well>(currentRegion);

            if(wellOnRegion.Count > 0)
            {

                if(wellOnRegion[0].isFilled)
                {
                    drinkButton.SetActive(true);
                    drinkButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    drinkButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        //Debug.Log(hero.data.WP);
                        hero.data.WP += 3;
                        string wp = hero.data.WP.ToString();
                        text.text = "WP: " + wp;
                        //Debug.Log(hero.data.WP);

                        photonView.RPC("Empty", RpcTarget.AllBuffered, currentRegion.label);

                        drinkButton.SetActive(false);
                    });
                }

            }
            else
            {
                drinkButton.SetActive(false);
            }
        }

    }

    [PunRPC]
    public void Empty(int currentRegion)
    {
        List<Well> wellOnRegion = gameGraph.FindObjectsOnRegion <Well>(GameGraph.Instance.Find(currentRegion));

        Well temp = wellOnRegion[0];

        temp.drunk();
    }

}
