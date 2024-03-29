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
using Bag;
using System.Text.RegularExpressions;
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
    FightTurnManager.IOnSunrise, FightTurnManager.IOnLeave

// FightTurnManager.IOnMove
{
    public static Fight Instance;
    public FightState fightstate;

    [Header("Monster")]
    public GameObject Gor;
    public GameObject Skral;
    public GameObject Wardrak;
    //public Transform monsterStation;

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
    public GameObject archerPrefabsfemale;
    public GameObject archerPrefabsmale;
    public GameObject warriorPrefabsfemale;
    public GameObject warriorPrefabsmale;
    public GameObject dwarfPrefabsfemale;
    public GameObject dwarfPrefabsmale;
    public GameObject wizardPrefabsfemale;
    public GameObject wizardPrefabsmale;
    public GameObject herbPrefab;

    public MonsterMoveController mc;

    public bool princeInFight = false;
    public Monster aMonster;
    public GameObject prince;
    public int currentWP;
    public int damage;
    public bool magicUsed;
    bool usedhelm = false;
    // Use this for initialization
    void Start()
    {
        if (Instance == null) Instance = this;

        foreach (MonsterMoveController monsterC in GameObject.FindObjectsOfType<MonsterMoveController>())
        {
            if (monsterC.isFighted)
            {
                mc = monsterC;
                aMonster = mc.m;
                currentWP = monsterC.m.maxWP;

            }
        }

        prince.SetActive(false);
        archerPrefabsfemale.SetActive(false);
        archerPrefabsmale.SetActive(false);
        warriorPrefabsfemale.SetActive(false);
        warriorPrefabsmale.SetActive(false);
        dwarfPrefabsfemale.SetActive(false);
        dwarfPrefabsmale.SetActive(false);
        wizardPrefabsfemale.SetActive(false);
        wizardPrefabsmale.SetActive(false);
        Gor.SetActive(false);
        Skral.SetActive(false);
        Wardrak.SetActive(false);

        switch (mc.type)
        {
            case Monsters.MonsterType.Gor:
                Gor.SetActive(true);
                break;
            case Monsters.MonsterType.Skral:
                Skral.SetActive(true);
                break;
            case Monsters.MonsterType.Wardrak:
                Wardrak.SetActive(true);
                break;

        }

        if (Prince.Instance != null && Prince.Instance.inFight)
        {
            //print("Prince is in fight");
            princeInFight = true;
            prince.SetActive(true);
        }
        fightstate = FightState.START;
        FightTurnManager.Register(this);

        StartCoroutine(setUpBattle());

    }


    //--------START--------//
    void plotCharacter()
    {

        //print("Plot");
        foreach (Player p in PhotonNetwork.PlayerList)
        {

            if (p.CustomProperties.ContainsKey(P.K.isFight))
            {
                bool fight = (bool)p.CustomProperties[P.K.isFight];
                if (fight)
                {
                    Debug.Log(p.NickName + "in fight");
                    Hero hero = (Hero)p.GetHero();

                    switch (hero.type)
                    {
                        case Hero.Type.ARCHER:
                            if (hero.ui.gender) archerPrefabsmale.SetActive(true);
                            else archerPrefabsfemale.SetActive(true);
                            break;
                        case Hero.Type.WARRIOR:
                            if (hero.ui.gender) warriorPrefabsmale.SetActive(true);
                            else warriorPrefabsfemale.SetActive(true);
                            break;
                        case Hero.Type.DWARF:
                            if (hero.ui.gender) dwarfPrefabsmale.SetActive(true);
                            else dwarfPrefabsfemale.SetActive(true);
                            break;
                        case Hero.Type.WIZARD:
                            if (hero.ui.gender) wizardPrefabsmale.SetActive(true);
                            else wizardPrefabsfemale.SetActive(true);
                            break;
                    }

                    //GameObject go5 = PhotonNetwork.
                    //}
                }
                else
                {
                    Hero hero = (Hero)p.GetHero();
                    switch (hero.type)
                    {
                        case Hero.Type.ARCHER:
                            archerPrefabsfemale.SetActive(false);
                            archerPrefabsmale.SetActive(false);
                            break;
                        case Hero.Type.WARRIOR:
                            warriorPrefabsfemale.SetActive(false);
                            warriorPrefabsmale.SetActive(false);
                            break;
                        case Hero.Type.DWARF:
                            dwarfPrefabsfemale.SetActive(false);
                            dwarfPrefabsmale.SetActive(false);
                            break;
                        case Hero.Type.WIZARD:
                            wizardPrefabsfemale.SetActive(false);
                            wizardPrefabsmale.SetActive(false);
                            break;
                    }
                }
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
        hHUD.setHeroHUD(hero);
        mHUD.setMonsterHUD(aMonster, currentWP);
        playerTurn();
        damage = 0;
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
        hero.data.rollResult = 0;
        hero.data.attackNum = 0;
        damage = 0;
        magicUsed = false;
        usedhelm = false;


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
            print("MyTUrn" + !FightTurnManager.IsMyTurn());
            print("photonView" + !photonView.IsMine);
            print("Fight" + !FightTurnManager.CanFight());

            return;

        }
        //print("rolling");
        HeroRoll();
        string s;
        if (hero.type == Hero.Type.ARCHER)
        {
            s = "Value:" + hero.data.diceNum + " Left B/R:" + hero.data.btimes + "/" + hero.data.times;
        }
        else
        {
            s = dice.printArrayList() + "Max:" + hero.data.diceNum;
        }
        Instance.photonView.RPC("HeroRoll", RpcTarget.All, player, s);
    }

    public void HeroRoll()
    {
        if (hero.type == Hero.Type.ARCHER)
        {
            if (hero.data.btimes > 0)
            {
                hero.data.diceNum = dice.getOne(true);
                hero.data.btimes -= 1;
            }
            else if (hero.data.times > 0)
            {
                hero.data.diceNum = dice.getOne(false);
                hero.data.times -= 1;
            }
        }
        //TODO: did not consider how black dice is used
        else if (hero.data.times > 0)
        {
            dice.rollDice(GetDiceNum(), hero.data.blackDice);
            hero.data.diceNum = dice.getMax();
            hero.data.times = 0;
        }
    }

    //Fight
    public int GetDiceNum()
    {
        switch (hero.type)
        {
            case (Hero.Type.ARCHER):
                if (hero.data.WP > 13)
                {
                    return 5;
                }
                else if (hero.data.WP > 6)
                {
                    return 4;
                }
                else
                {
                    return 3;
                }
            case (Hero.Type.DWARF):
                if (hero.data.WP > 13)
                {
                    return 3;
                }
                else if (hero.data.WP > 6)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            case (Hero.Type.WARRIOR):
                if (hero.data.WP > 13)
                {
                    return 4;
                }
                else if (hero.data.WP > 6)
                {
                    return 3;
                }
                else
                {
                    return 2;
                }
            case (Hero.Type.WIZARD):
                {
                    return 1;
                }
            default:

                print("Problem");
                return 0;
        }
    }

    //--------ROLL--------//
    [PunRPC]
    public void HeroRoll(Player player, string s)
    {
        Hero rolledhero = player.GetHero();
        //print("heroroll running");
        if (rolledhero.type == Hero.Type.ARCHER)
        {
            if (rolledhero == hero)
            {

                if (hero.data.btimes > 0 || hero.data.times > 0)
                {
                    myArcherYesButton.gameObject.SetActive(true);
                }
                else
                {
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
    public void OnSkillCompleted(Player currentplayer, int diceNum)
    {


        Hero CurrentHero = (Hero)currentplayer.GetHero();
        print(hero.name + "get changed" + diceNum);
        hero.data.rollResult += diceNum;
        print("Roll" + hero.data.rollResult);
        hero.data.attackNum += CurrentHero.data.SP;

    }

    [PunRPC]
    public void displayRollResult(Player actplayer, int diceNum)
    {
        print("Act:" + actplayer.NickName + "Player" + player.NickName + "D " + diceNum);
        print(FightTurnManager.IsMyTurn());
        //if(magicUsed) {
        //    if (FightTurnManager.IsMyTurn()) {
        //        actplayer.GetHero().data.diceNum += diceNum;
        //    }
        //}
        //else {
        if (!actplayer.NickName.Equals(player.NickName))
        {
            print("indes");
            actplayer.GetHero().data.diceNum = diceNum;
        }
        //}


        print("Noice " + actplayer.GetHero().data.diceNum);
        if (actplayer.NickName.Equals(player.NickName))
        {
            fHUD.rollResult(player.NickName + "finished roll");
        }
        else
        {
            fHUD.rollResult(player.NickName + "in queue");
        }


    }


    //--------ATTACK--------//

    public void OnMonsterTurn()
    {
        print("Total sum of Attack" + hero.data.attackNum);
        hero.data.attackNum += hero.data.rollResult;
        print("Total Damage " + hero.data.attackNum);
        if (princeInFight)
        {
            print("Prince helps to add 4");
            hero.data.attackNum += 4;
        }
        fHUD.rollResult("HeroAttack " + hero.data.attackNum);
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
        if (FightTurnManager.IsMyProtectedTurn())
        {
            print("only run once");

            dice.rollDice(aMonster.redDice, 0);
            if (dice.CheckRepet())
            {
                damage = dice.getSum();
            }
            else
            {
                damage = dice.getMax();
            }
            //damage = aMonster.MonsterRoll();
            string s = dice.printArrayList();
            print(s.ToString());
            fHUD.rollResult(s + "Max:" + damage);
            Instance.photonView.RPC("setNumber", RpcTarget.Others, s);
            StartCoroutine(MonsterRoll());
            return;
        }

        print("should not be run here");


    }

    public void SetDice(string a)
    {
        char[] seperator = { ' ' };
        string[] array = a.Split(seperator);
        List<int> l = new List<int>();
        foreach (string s in array)
        {
            if (Regex.IsMatch(s, @"^\d+$"))
            {
                print(s);
                l.Add(int.Parse(s));
            }
        }
        dice.setResult(l);
        if (dice.CheckRepet())
        {
            damage = dice.getSum();
        }
        else
        {
            damage = dice.getMax();
        }
        print(this.damage);
    }

    [PunRPC]
    public void setNumber(string result)
    {
        print("others");
        SetDice(result);
        StartCoroutine(MonsterRoll());
    }

    IEnumerator MonsterRoll()
    {
        damage += aMonster.maxSP;
        yield return new WaitForSeconds(2f);
        //fHUD.rollResult( "Damage:" + aMonster.damage);
        //yield return new WaitForSeconds(2f);
        mySkillYesButton.gameObject.SetActive(true);
        fightstate = FightState.CHECK;
        fHUD.setFightHUD_CHECK(hero.data.attackNum, damage);
        StartCoroutine(CheckOnShield());
        yield return new WaitForSeconds(4f);
    }


    public void Attacked(int damage)
    {
        currentWP -= damage;
        print("CurrentWP" + currentWP);
    }

    //--------CHECK--------//
    IEnumerator CheckOnShield()
    {
        fHUD.rollResult("Attack By Hero: " + hero.data.attackNum + " Attack By Monster: " + damage);
        yield return new WaitForSeconds(2f);
        if (damage > hero.data.attackNum)
        {
            //go thouth everything to check if want to use sheild
            fHUD.setFightHUD_SHIELD();

        }
        else if (damage <= hero.data.attackNum)
        {

            Attacked(hero.data.attackNum - damage);
            mHUD.basicInfo(aMonster, currentWP);
            yield return new WaitForSeconds(2f);
        }

    }

    public void OnShield(Player player)
    {
        if (damage - hero.data.attackNum > 0)
        {
            hero.data.WP -= damage - hero.data.attackNum;
        }
        else
        {
            print("Damage" + damage);
            print(hero.data.attackNum);
            print("why less than zero");
        }

        hHUD.basicInfoUpdate(hero);
    }

    IEnumerator Check()
    {
        yield return new WaitForSeconds(2f);
        mySkillYesButton.gameObject.SetActive(false);
        print("MonsterWP" + currentWP);
        if (currentWP <= 0)
        {
            print("WIN");
            fightstate = FightState.WIN;
            fHUD.setFightHUD_WIN();
            yield return new WaitForSeconds(2f);
            if (mc.hasHerb)
            {
                PhotonNetwork.Instantiate(herbPrefab.name, GameGraph.Instance.Find(mc.CurrentRegion.label).position, Quaternion.identity);
            }


            int rewardc = aMonster.rewardc;
            int rewardw = aMonster.rewardw;
            print("Reward" + rewardc);
            print("Reward" + rewardw);

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(mc.gameObject);

            }
            Debug.Log(mc);

            if (aMonster.isTower)
            {
                //  photonView.RPC("tellCastle", RpcTarget.AllBuffered);
                GameObject.FindObjectOfType<Castle>().GetComponent<Castle>().tellCastle();
            }
            StartFightManager.Instance.fightStart = false;
            StartFightManager.Instance.isFight = false;
            SceneManager.UnloadSceneAsync("FightScene");
            foreach (Player player in FightTurnManager.Instance.players)
            {
                print(player.NickName);
            }
            if (PhotonNetwork.IsMasterClient)
            {
                //TODO:test
                TurnManager.TriggerEvent_EndTurn();
                DistributionManager.DistributeWinFight(FightTurnManager.Instance.players, (ItemType.Coin, rewardc), (ItemType.WillPower, rewardw));
            }
            Leave();
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
            TurnManager.TriggerEvent_EndTurn();
            StartFightManager.Instance.fightStart = false;
            StartFightManager.Instance.isFight = false;
            SceneManager.UnloadSceneAsync("FightScene");

        }
        else
        {
            fightstate = FightState.DECISION;
            fHUD.setFightHUD_DICISION();
            yield return new WaitForSeconds(2f);
        }

    }



    public void Leave()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                        {
                            { P.K.isFight, false }
                        });
        //not possible to pass a Monster 
        FightTurnManager.TriggerRemove(player);


    }


    public void OnLastLeave()
    {
        mc.isFighted = false;
        //currentWP = aMonster.maxWP;
    }
    public void OnYesClick()
    {
        myArcherYesButton.gameObject.SetActive(false);
        mySkillYesButton.gameObject.SetActive(true);
    }

    [PunRPC]
    public void showSkillResult(Player actplayer, string skill, int result, int resultNum)
    {
        if (skill.Equals("magic"))
        {
            actplayer.GetHero().data.diceNum = result;
            print("?" + hero.data.diceNum);
        }
        else if (skill.Equals("Helm"))
        {
            actplayer.GetHero().data.helm = resultNum;
            actplayer.GetHero().data.diceNum = result;
        }
        else if (skill.Equals("HerbStrength"))
        {
            actplayer.GetHero().data.herb = resultNum;
            actplayer.GetHero().data.diceNum = result;
        }
        else if (skill.Equals("HerbWill"))
        {
            actplayer.GetHero().data.herb = resultNum;
            actplayer.GetHero().data.diceNum = result;
        }
        else if (skill.Equals("Brew"))
        {
            actplayer.GetHero().data.brew = resultNum;
            actplayer.GetHero().data.diceNum = result;
        }

        print(player.NickName + " update " + result + "" + resultNum);
        fHUD.rollResult("Applied:" + skill + result);
    }

    public void onMagicClick()
    {
        if (fightstate != FightState.HERO || !hero.GetMagic())
        {
            return;
        }
        print(FightTurnManager.CurrentHero.name + FightTurnManager.CurrentHero.type);
        Instance.photonView.RPC("AppliedMagic", RpcTarget.All, player);


    }
    [PunRPC]
    public void AppliedMagic(Player actPlayer)
    {
        //FightTurnManager.IsMyTurn()
        print(FightTurnManager.IsMyTurn());
        print(FightTurnManager.CurrentHero.type);
        this.magicUsed = true;

        if (FightTurnManager.IsMyTurn())
        {
            int diceNum = FightTurnManager.CurrentHero.data.diceNum;
            int temp;
            if (diceNum < 7 && diceNum > 0)
            {
                temp = 7 - diceNum;
                //FightTurnManager.CurrentHero.data.diceNum = temp;
                hero.data.diceNum = temp;
                print("Should only turn one applied magic with value" + temp);
                Instance.photonView.RPC("showSkillResult", RpcTarget.All, player, "magic", temp, 0);
            }
            else
            {
                print("error");
                return;
            }



        }
    }



    public void onSheildClick()
    {
        if (fightstate != FightState.CHECK || !hero.HasShield() || usedhelm || !FightTurnManager.IsMyProtectedTurn())
        {
            return;
        }
        hero.data.shield -= 1;
        damage = 0;
        fHUD.rollResult("Applied Sheild");

    }

    public void onHelmClick()
    {
        if (fightstate != FightState.HERO || !hero.HasHelm() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = dice.getSum();
        hero.data.diceNum = temp;

        usedhelm = true;
        hero.data.helm -= 1;
        int changedNum = hero.data.helm;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, player, "Helm", temp, changedNum);

    }

    public void onHerbSClick()
    {
        if (fightstate != FightState.HERO || !hero.HasHerb() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.diceNum + hero.data.herb;
        hero.data.diceNum = temp;
        hero.data.herb = 0;
        int changedNum = 0;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, player, "HerbStrength", temp, changedNum);
    }

    public void onHerbWClick()
    {
        if (fightstate != FightState.HERO || !hero.HasHerb() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.WP + hero.data.herb;
        hero.data.WP = temp;
        hero.data.herb = 0;
        int changedNum = 0;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, player, "HerbWill", temp, changedNum);
        hHUD.basicInfoUpdate(hero);

    }

    public void onBrewClick()
    {
        if (fightstate != FightState.HERO || !hero.HasBrew() || !FightTurnManager.IsMyTurn())
        {
            return;
        }
        int temp = hero.data.diceNum * 2;
        hero.data.diceNum = temp;
        hero.data.brew -= 1;
        int changedNum = hero.data.brew;
        Instance.photonView.RPC("showSkillResult", RpcTarget.All, player, "Brew", temp, changedNum);
    }

    public void onSkillClick()
    {

        if (fightstate == FightState.HERO && FightTurnManager.IsMyTurn())
        {
            int resultNum = hero.data.diceNum;
            Instance.photonView.RPC("displayRollResult", RpcTarget.All, player, resultNum);
            mySkillYesButton.gameObject.SetActive(false);
            FightTurnManager.TriggerEvent_Fight();
            FightTurnManager.TriggerEvent_NewFightRound(player);
        }
        else if (fightstate == FightState.CHECK && FightTurnManager.IsMyProtectedTurn())
        {
            FightTurnManager.TriggerEvent_OnShield(player);
        }

        else
        {
            print("error");
        }

    }

    /*
           LEAVE, CONTINUE, FALCON, TRADE
         */
    public void OnLeaveClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }

        //Initialize the mosnter
        Leave();

        StartFightManager.Instance.fightStart = false;
        StartFightManager.Instance.isFight = false;
        TurnManager.TriggerEvent_EndTurn();
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
        if (!FightTurnManager.enoughTime())
        {

            fHUD.rollResult("You dont have enough hour, you can leave fight");
            return;
        }

        playerTurn();
    }

    public void OnFalconClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }

    }

    public void OnTradeClick()
    {
        if (fightstate != FightState.DECISION)
        {
            return;
        }

    }

    public void OnSunrise()
    {
        StartCoroutine(Check());
    }
}