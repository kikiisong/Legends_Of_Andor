﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class WellManager : MonoBehaviourPun, TurnManager.IOnMove, TurnManager.IOnTurnCompleted, TurnManager.IOnEndDay
{
    public Button drinkButton;
    public GameObject wellPrefab;

    // Start is called before the first frame update
    void Start()
    {
         drinkButton.gameObject.SetActive(false);
         TurnManager.Register(this);
        if (!Room.IsSaved)
        {
            initPlacement();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void initPlacement()
    {
        Instantiate(wellPrefab, GameGraph.Instance.Find(5).position, Quaternion.identity);
        Instantiate(wellPrefab, GameGraph.Instance.Find(35).position, Quaternion.identity);
        Instantiate(wellPrefab, GameGraph.Instance.Find(45).position, Quaternion.identity);
        Instantiate(wellPrefab, GameGraph.Instance.Find(55).position, Quaternion.identity);

    }

    public void OnMove(Player player, Region currentRegion)
    {
        if (PhotonNetwork.LocalPlayer == player)
        {
            drinkButton.gameObject.SetActive(false);
        }

    }

    public void OnTurnCompleted(Player player)
    {
        if (PhotonNetwork.LocalPlayer == player)
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.GetHero();//photonView.Owner is the Scene
            var r = player.GetCurrentRegion();
            List<Well> wellOnRegion = GameGraph.Instance.FindObjectsOnRegion<Well>(r.label);

            if (wellOnRegion.Count > 0)
            {

                if (wellOnRegion[0].IsFilled)
                {
                    drinkButton.gameObject.SetActive(true);
                    drinkButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    drinkButton.GetComponent<Button>().onClick.AddListener(() =>
                    {

                        photonView.RPC("Empty", RpcTarget.AllBuffered, r.label, (int)hero.type);

                        drinkButton.gameObject.SetActive(false);
                    });
                }

            }
            else
            {
                drinkButton.gameObject.SetActive(false);
            }
        }
    }

    public void OnEndDay(Player player)
    {
        if (PhotonNetwork.LocalPlayer == player)
        {
            Hero hero = (Hero)PhotonNetwork.LocalPlayer.GetHero();//photonView.Owner is the Scene
            var r = player.GetCurrentRegion();
            List<Well> wellOnRegion = GameGraph.Instance.FindObjectsOnRegion<Well>(r.label);

            if (wellOnRegion.Count > 0)
            {

                if (wellOnRegion[0].IsFilled)
                {
                    drinkButton.gameObject.SetActive(true);
                    drinkButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    drinkButton.GetComponent<Button>().onClick.AddListener(() =>
                    {

                        photonView.RPC("Empty", RpcTarget.AllBuffered, r.label, (int)hero.type);

                        drinkButton.gameObject.SetActive(false);
                    });
                }

            }
            else
            {
                drinkButton.gameObject.SetActive(false);
            }
        }
    }


    [PunRPC]
    public void Empty(int currentRegion ,int heroType)
    {

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Hero hero = (Hero)players[i].GetHero();
            if ((int)hero.type == heroType)
            {
                if (hero.type == Hero.Type.WARRIOR)
                {
                    hero.data.WP += 5;
                }
                else
                {
                    hero.data.WP += 3;
                }
                break;
            }
        }

        List<Well> wellOnRegion = GameGraph.Instance.FindObjectsOnRegion <Well>(GameGraph.Instance.Find(currentRegion));

        Well temp = wellOnRegion[0];

        temp.Drunk();
    }

}
