﻿using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Routines;
using System.Collections.Generic;

public enum FightState
    {
        START,
        HERO,
        //hero roll dice
        MONSTER,
        //monster roll dice
        CHECK,
        //check who wins
        COOP,
        //wait for other seletction
        WIN,
        //label if hero wins
        LOSE,
        DECISION

    }


public class Fight : MonoBehaviourPun, FightTurnManager.IOnSkillCompleted
, FightTurnManager.IOnMonsterTurn, FightTurnManager.IOnShield, 
    FightTurnManager.IOnSunrise,FightTurnManager.IOnLeave

// FightTurnManager.IOnMove
{
    public static Fight Instance;
    public FightState fightstate;

    [Header("Monster")]
    public Transform monsterStation;

    [Header("HUD")]
    public MonsterHUD mHUD;
    public HeroHUD hHUD;
    public FightHUD fHUD;

    [Header("ArcherSpecial")]
    public Button myArcherYesButton;
    public Button mySkillYesButton;

    [Header("Dice")]
    public Dice dice;

    [Header("Prefabs")]
    public GameObject archerPrefabs;
    public GameObject warriorPrefabs;
    public GameObject dwarfPrefabs;
    public GameObject wizardPrefabs;
    public Transform[] transforms = new Transform[4];

    public Monster aMonster;
    //public int diceNum;
    //public int damage;

    // Use this for initialization
    void Start()
    {
        if (Instance == null) Instance = this;
        foreach (MonsterMoveController monsterC in GameObject.FindObjectsOfType<MonsterMoveController>())
        {
            if (monsterC.m.isFighted)
            {

                aMonster = monsterC.m;
                Debug.Log(aMonster);
                break;
            }
        }
        Instantiate(aMonster.gameObject, monsterStation);
        //aMonster = monsterGo.GetComponent<Monster>();
        //go5.transform.position = monsterStation.position;

        fightstate = FightState.START;
        FightTurnManager.Register(this);
        StartCoroutine(setUpBattle());
    }



    //--------START--------//
    void plotCharacter() {

        Player player = PhotonNetwork.LocalPlayer;
        if (player.CustomProperties.ContainsKey(P.K.isFight))
        {
            bool fight = (bool)player.CustomProperties[P.K.isFight];
            if (fight) {
                Debug.Log(player.NickName + "in fight");
                Hero hero = (Hero)player.GetHero();

                switch (hero.type)
                {
                    case Hero.Type.ARCHER:
                        GameObject go1 = PhotonNetwork.Instantiate(archerPrefabs);
                        go1.transform.position = transforms[0].position;
                        go1.SetActive(true);
                        break;
                    case Hero.Type.WARRIOR:
                        GameObject go2 = PhotonNetwork.Instantiate(warriorPrefabs);
                        go2.transform.position = transforms[1].position;
                        go2.SetActive(true);
                        break;
                    case Hero.Type.DWARF:
                        GameObject go3 = PhotonNetwork.Instantiate(dwarfPrefabs);
                        go3.transform.position = transforms[2].position;
                        go3.SetActive(true);
                        break;
                    case Hero.Type.WIZARD:
                        GameObject go4 = PhotonNetwork.Instantiate(wizardPrefabs);
                        go4.transform.position = transforms[3].position;
                        go4.SetActive(true);
                        break;
                }
                hero.data.dice = dice;
                hHUD.setHeroHUD(hero);
                mHUD.setMonsterHUD(aMonster);

                //GameObject go5 = PhotonNetwork.


                //}
            }
        }

    }

    Player player;
    Hero hero;

    IEnumerator setUpBattle()
    {
        myArcherYesButton.gameObject.SetActive(false);
        mySkillYesButton.gameObject.SetActive(false);
        fHUD.setFightHUD_START();
        fightstate = FightState.HERO;
        yield return new WaitForSeconds(2f);
        fHUD.setFightHUD_PLAYER();
        plotCharacter();
        player = PhotonNetwork.LocalPlayer;
        hero = (Hero)player.GetHero();
        playerTurn();
        yield return new WaitForSeconds(2f);

    }
    //--------HERO--------//
    //--------MESSAGE--------//


    public void playerTurn()
    {
        //TODO:clean the prefab of existing hero
        hero.data.times = hero.GetDiceNum();
        hero.data.btimes = hero.data.blackDice;
        fHUD.setFightHUD_PLAYER();
        hero.data.diceNum = 0;
        hero.data.attackNum = 0;
        aMonster.damage = 0;

        if (fightstate != FightState.HERO || !FightTurnManager.IsMyTurn()
            || !photonView.IsMine || !FightTurnManager.CanFight())
        {
            print("return");
            return;

        }

        fHUD.rollResult("Player Turn: " + player.NickName);

    }

    public void OnRollDice()
    {
        //roll the dice
        //confirm the action


        if (fightstate != FightState.HERO || !FightTurnManager.IsMyTurn()
            || !FightTurnManager.CanFight())
        {
            //TODO:leave fight
            print("return");

            print("Fight State" + (fightstate != FightState.HERO));
            print("MyTUrn"+ !FightTurnManager.IsMyTurn());
            print("photonView" + !photonView.IsMine);
            print("Fight" + !FightTurnManager.CanFight());

            return;

        }
        print("rolling");
        hero.HeroRoll();
        string s;
        if (hero.type == Hero.Type.ARCHER)
        {
            s = "Value:" + hero.data.diceNum + " Left B/R:" + hero.data.btimes + "/" + hero.data.times;
        }
        else {
            s = hero.data.dice.printArrayList() + "Max:" + hero.data.diceNum;
        }
        Instance.photonView.RPC("HeroRoll", RpcTarget.All, player, s);
    }

    //--------ROLL--------//
    [PunRPC]
    public void HeroRoll(Player player, string s)
    {
        Hero rolledhero = player.GetHero();
        print("heroroll running");
        if (rolledhero.type == Hero.Type.ARCHER)
        {
            if (rolledhero == hero) {

                if (hero.data.btimes > 0 || hero.data.times > 0) {
                    myArcherYesButton.gameObject.SetActive(true);
                }
                else {
                    OnYesClick();
                }

            }

            fHUD.rollResult(s);


        }
        else
        {
            if (rolledhero == hero)
            {
                mySkillYesButton.gameObject.SetActive(true);


            }
            fHUD.rollResult(s);

        }

    }

    //--------ROLLFINISHED--------//
    public void OnSkillCompleted(Player player) {

        Hero CurrentHero = (Hero)player.GetHero();
        hero.data.diceNum = Mathf.Max(hero.data.diceNum, CurrentHero.data.diceNum);
        hero.data.attackNum += CurrentHero.data.SP;
        
    }

    [PunRPC]
    public void displayRollResult(Player player,int result) {
        fHUD.rollResult(player.NickName +"finished roll" );

    }


    //--------ATTACK--------//

    public void OnMonsterTurn() {
        hero.data.attackNum += hero.data.diceNum;
        fHUD.rollResult("HeroAttacl" + hero.data.attackNum);
        print("Monster");
        StartCoroutine(MonsterStart());
    }


    IEnumerator MonsterStart()
    {
        yield return new WaitForSeconds(2f);
        fightstate = FightState.MONSTER;
        fHUD.setFightHUD_MONSTER();
        yield return new WaitForSeconds(2f);
        MonsterAttack();
    }

    public void MonsterAttack()
    {

        if (fightstate != FightState.MONSTER)
        {
            return;

        }
        if (FightTurnManager.IsMyProtectedTurn()) {
            print("only run once");
            aMonster.MonsterRoll();
            string s = aMonster.dice.printArrayList();
            fHUD.rollResult(s + "Max:" + aMonster.damage);
            Instance.photonView.RPC("setNumber", RpcTarget.Others, s);
            StartCoroutine(MonsterRoll());
            print("this"); 
            return;
        }

        print("should not be run here");
        

    }

    [PunRPC]
    public void setNumber(string result)
    {
        print("others");
        aMonster.SetDice(result);
        StartCoroutine(MonsterRoll());
    }

    IEnumerator MonsterRoll()
    {
        aMonster.damage += aMonster.maxSP;
        yield return new WaitForSeconds(2f);
        //fHUD.rollResult( "Damage:" + aMonster.damage);
        //yield return new WaitForSeconds(2f);
        mySkillYesButton.gameObject.SetActive(true);
        fightstate = FightState.CHECK;
        fHUD.setFightHUD_CHECK(hero.data.attackNum, aMonster.damage);
        StartCoroutine(CheckOnShield());
        yield return new WaitForSeconds(4f);
    }

    //--------CHECK--------//
    IEnumerator CheckOnShield()
    { fHUD.rollResult("Attack By Hero: "+ hero.data.attackNum+ " Attack By Monster: " +aMonster.damage);
        yield return new WaitForSeconds(2f);
        if (aMonster.damage > hero.data.attackNum)
        {
            //go thouth everything to check if want to use sheild
            fHUD.setFightHUD_SHIELD();
 
        }
        else if (aMonster.damage <= hero.data.attackNum)
        {
            //TODO:
            aMonster.Attacked(hero.data.attackNum - aMonster.damage);
            mHUD.basicInfo(aMonster);
            yield return new WaitForSeconds(2f);
        }

    }

    public void OnShield(Player player)
    {

        hero.data.WP -= aMonster.damage - hero.data.attackNum;
        hHUD.basicInfoUpdate(hero);
    }

    IEnumerator Check()
    {
        mySkillYesButton.gameObject.SetActive(false);
        if (aMonster.currentWP <= 0)
        {
            fightstate = FightState.WIN;
            fHUD.setFightHUD_WIN();
            yield return new WaitForSeconds(2f);
            Destroy(aMonster);
            print("WIN");
            SceneManager.LoadSceneAsync("Distribution", LoadSceneMode.Additive);

        }
        else if (hero.data.WP <= 0)
        {
            fightstate = FightState.LOSE;
            fHUD.setFightHUD_LOSE();
            yield return new WaitForSeconds(2f);

            //penalty
            hero.data.WP = 3;
            if (hero.data.SP > 1) hero.data.SP -= 1;

            //initialize everything
            Leave();
            SceneManager.UnloadSceneAsync("FightScene");

        }
        else
        {
            fightstate = FightState.DECISION;
            fHUD.setFightHUD_DICISION();
            yield return new WaitForSeconds(2f);
        }

    }

    public void Leave() {

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                        {
                            { P.K.isFight, false }
                        });
        //not possible to pass a Monster 
        FightTurnManager.TriggerRemove(player);

    }


    public void OnLastLeave() {
        aMonster.isFighted = false;
        aMonster.currentWP = aMonster.maxWP;
    }
    public void OnYesClick()
    {
        myArcherYesButton.gameObject.SetActive(false);
        mySkillYesButton.gameObject.SetActive(true);
    }

    [PunRPC]
    public void showSkillResult(Hero h, string skill, int result) {
        fHUD.rollResult("Applied:" +skill + result);
    }

    public void onMagicClick()
    {
        if (fightstate != FightState.HERO || !hero.GetMagic())
        {
            return;
        }
        Instance.photonView.RPC("AppliedMagic", RpcTarget.All);


    }
    [PunRPC]
    public void AppliedMagic() {
        
        if (FightTurnManager.IsMyTurn()) {
            int diceNum = hero.data.diceNum;
            int temp = diceNum;
            if (diceNum < 7)
            {
                temp = 7 - diceNum;
                FightTurnManager.CurrentHero.data.diceNum = temp;
            }

            Instance.photonView.RPC("showSkillResult", RpcTarget.All, hero, "magic", temp);

        }
    }

    bool usedhelm = false;

    public void onSheildClick()
    {
        if (fightstate != FightState.CHECK || !hero.HasShield() || usedhelm || !FightTurnManager.IsMyProtectedTurn())
        {
            return;
        }
        hero.data.shield -= 1;
        aMonster.damage = 0;
        fHUD.rollResult("Applied Sheild" );

    }

    public void onHelmClick()
    {
        if (fightstate != FightState.HERO || !hero.HasHelm() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.dice.getSum();
        hero.data.diceNum = temp;

        usedhelm = true;
        hero.data.helm-=1;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, hero, "Helm",temp);

    }

    public void onHerbSClick()
    {
        if (fightstate != FightState.HERO || !hero.HasHerb() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.diceNum + hero.data.herb;
        hero.data.diceNum =temp;
        
        hero.data.herb = 0;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, hero, "HerbStrength",temp);
    }

    public void onHerbWClick()
    {
        if (fightstate != FightState.HERO||!hero.HasHerb() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.WP + hero.data.herb;
        hero.data.WP = temp;
        hero.data.herb = 0;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, hero, "HerbWill",temp);
        hHUD.basicInfoUpdate(hero);

    }

    public void onBrewClick()
    {
        if (fightstate != FightState.HERO || !hero.HasBrew() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.diceNum * 2;
        hero.data.diceNum =temp;
        hero.data.brew -=1 ;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, hero, "Brew",temp);
    }

    public void onSkillClick()
    {

        if (fightstate == FightState.HERO&& FightTurnManager.IsMyTurn())
        {
            Instance.photonView.RPC("displayRollResult", RpcTarget.All, player, hero.data.diceNum);
            mySkillYesButton.gameObject.SetActive(false);
            FightTurnManager.TriggerEvent_Fight();
            FightTurnManager.TriggerEvent_NewFightRound(player);
        }
        else if (fightstate == FightState.CHECK && FightTurnManager.IsMyProtectedTurn())
        {
            FightTurnManager.TriggerEvent_OnShield(player);
        }

        else {
            print("error");
        }
        
    }

    /*Four button*/
    public void OnLeaveClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }

        //Initialize the mosnter
        Leave();
        
        SceneManager.UnloadSceneAsync("FightScene");

    }

    public void OnConitnueClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }

        fightstate = FightState.HERO;

        if (hero.type == Hero.Type.WIZARD)
        {
            hHUD.backColorMagic();
        }

        playerTurn();
    }

    public void OnFalconClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }
        //Initialize the mosnter
        print("Falcon");
    }

    public void OnTradeClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }
        //Initialize the mosnter
        print("Trade");
    }

    public void OnSunrise()
    {
        StartCoroutine(Check());
    }
}
    

