using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Characher {

    public class Buff {
        static Buff my_Buff;

        public enum Type {
            defence,
            attack,
            health,
            speed,
            skill
        }

        public float timeLast;
    }

    public class State {


        public int level;
        public int hp;
        public int ap;
        public int dp;
        public int speed;
        public int expRequesting;
        private InGameDataBase.Character info;
        private List<Buff> exisitingBuffs;

        private List<int> expList;
        private List<Dictionary<string,int>> lvStateList;

        public void SetBuff( Buff.Type in_type , float in_value , float timeLenght ) {
            
        }

        public void UpdateStateWithLevel(int in_level) {

            if (info != null)
            {
                level = in_level;

                if (lvStateList != null)
                {
                    hp = info.healthPoint;
                    ap = info.attackPoint;
                    dp = info.defencePoint;
                    speed = info.attackSpeed;
                }
                else {
                    hp = info.healthPoint + (in_level * 10);
                    ap = info.attackPoint + in_level;
                    dp = info.defencePoint + in_level;
                    speed = info.attackSpeed;
                }

                if (expList != null)
                {
                    //TODO : unknown usage of exp...
                    /*
                     * 1. game end , exp calculation
                     * 2. store something.... 
                     */

                    if (expList.Count > level + 1)
                    {
                        expRequesting = expList[level + 1];
                    }
                    else
                    {
                        //cant level up
                        expRequesting = -99999999;
                    }
                }
                else {
                    Debug.LogWarning("UpdateStateWithLevel expList is null ");
                }
            }
            else {
                Debug.LogWarning("character state cant set without value info ! please check");
            }
        }

        public void grantExp() {
            
        }

        public void InitWith(InGameDataBase.Character in_info , int inGame_level) {
            if (in_info != null) {
                info = in_info;

                expList = CommonUtil.InGameCoreDataStore.instance.GetCharacterLevelList((int)info.rarity);
                lvStateList = CommonUtil.InGameCoreDataStore.instance.GetCharacterClassLevelStateList((int)info.classType);

                UpdateStateWithLevel(inGame_level);
            }
        }
    }

}

public class InGameCharacterController : MonoBehaviour
{

    public enum Type
    {
        None,
        Player,
        Enemy,
        Stuff
    }

    public enum Status
    {
        Deactive,
        Active,
        Death,
        Run
    }

    public bool testWithSimple;
    public bool testWithSimple_enemy;
    public bool testDeath;
    private bool isReseting;
    private bool isBeingReset;
    private bool isForWaveReset;
    private bool isResetAnimActionCalled;

    private List<GameObject> coinList = new List<GameObject>();
    public GameObject coinsPrefab;
    public GameObject countingPrefab;

    public SpriteRenderer marker;
    public Animator animator;
    public ParticleSystem buffEffect_1;

    private InGameDataBase.Character info;
    private Characher.State myState;
    private int cur_HP;
    private int cur_DPS;
    private Status cur_status = Status.Deactive;
    private Type my_type;

    //Delegate
    public delegate void ReadyToStartWave(InGameCharacterController self);
    private ReadyToStartWave waitToStart_delegate;
    public delegate void ReadyToAttack(InGameCharacterController self);
    private ReadyToAttack action_delegate;
    public delegate void AlreadyDead(InGameCharacterController self);
    private AlreadyDead death_delegate;
    public delegate void AlreadyRun(InGameCharacterController self);
    private AlreadyRun run_delegate;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RunDebugTestCase();

        UpdateEnemyResetState();

        UpdatePlayerResetState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    /// ----------- Test Methods ----------- ///

    void RunDebugTestCase()
    {
        //Test load Player
        if (testWithSimple)
        {
            testWithSimple = false;

            //set
            InGameDataBase.Character demoCharacter = new InGameDataBase.Character();
            demoCharacter.initWith("2");
            SetCharacterInfo(demoCharacter, InGameCharacterController.Type.Player);
        }

        //Test load Enemy
        if (testWithSimple_enemy)
        {
            testWithSimple_enemy = false;

            //set
            InGameDataBase.Character demoCharacter = new InGameDataBase.Character();
            demoCharacter.initWith("101");
            SetCharacterInfo(demoCharacter, InGameCharacterController.Type.Enemy);
        }

        //Test Action
        if (testDeath)
        {
            testDeath = false;
            cur_HP = 0;
            DoDeath();
        }
    }

    /// ----------- Private Methods ------------ ///

    private void UpdateEnemyResetState()
    {
        if (my_type == Type.Enemy) {

            if (isForWaveReset)
            {
                if (isReseting)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle_Ready"))
                    {
                        Debug.Log(GetCharacterName() + " Current Animator StateInfo is in idle_Ready");
                        DoResetAnimAction();
                        isReseting = false;
                        isBeingReset = true;
                    }
                }

                if (isBeingReset)
                {
                    //Catch the target animation end
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Settle_enemy") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Settle_player"))
                    {
                        isForWaveReset = false;
                        isBeingReset = false;
                        cur_status = Status.Active;
                        ResetDPSCoins();
                        waitToStart_delegate?.Invoke(this); // call back
                    }
                    else
                    {
                        Debug.Log(GetCharacterName() + " Current Animator StateInfo is in " + animator.GetCurrentAnimatorStateInfo(0).ToString());
                    }
                }
            }
            else {

                if (isReseting)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle_Ready"))
                    {
                        Debug.Log(GetCharacterName() + " Current Animator StateInfo is in idle_Ready");
                        DoResetAnimAction();
                    }
                    else {
                        isReseting = false;
                        cur_status = Status.Active;
                        ResetDPSCoins();
                        waitToStart_delegate?.Invoke(this); // call back
                    }
                }

            }
        }
    }

    private void UpdatePlayerResetState()
    {
        if (my_type == Type.Player)
        {
            if (isReseting)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle_Ready"))
                {
                    Debug.Log(GetCharacterName() + " Current Animator StateInfo is in Death");
                    DoResetAnimAction();
                }
                else
                {
                    isReseting = false;
                    cur_status = Status.Active;
                    ResetDPSCoins();
                    waitToStart_delegate?.Invoke(this); // call back
                }
            }
        }
    }


        /// ----------- Public Methods ----------- ///

        public string GetCharacterName() => info.name;
    public Type GetCharacterType() => my_type;
    public Status GetCurStatus() => cur_status;
    public int GetCurHealthPoint() => cur_HP;
    public int GetBaseHealthPoint() => myState.hp;  //state
    public int GetAttackPoint() => myState.ap;  //state
    public int GetDefencePoint() => myState.dp;  //state
    public int GetSpeed() => myState.speed;  //state
    public int GetNextLevelExpReq() => myState.expRequesting;  //state
    public InGameDataBase.Skill GetSkillInfo() => info.skill;
    public string GetItemPackID() => info.itemPack;

    public void SetCharacterInfoForReset(InGameDataBase.Character in_characterInfo, Type in_playerType)
    {
        isForWaveReset = true;
        SetCharacterInfo(in_characterInfo, in_playerType);
    }
    public void SetCharacterInfo(InGameDataBase.Character in_characterInfo, Type in_playerType)
    {
        if (myState == null)
        {
            SetCharacterInfo(in_characterInfo, in_playerType, 1);
        }
        else {
            SetCharacterInfo(in_characterInfo, in_playerType, myState.level);
        }

        
    }

    public void SetCharacterInfo(InGameDataBase.Character in_characterInfo, Type in_playerType , int start_level)
    {
        cur_status = Status.Deactive;

        //do init this character setup
        if (in_characterInfo != null) {

            bool isSameCharacterInfo = false;
            if (info != null) {
                if (info.id == in_characterInfo.id)
                {
                    Debug.Log("info.id ? "+ info.id + " ->> in_characterInfo.id ? "+ in_characterInfo.id + " ");
                    info = null;
                    //isSameCharacterInfo = true;
                }
                else {
                    
                    Debug.Log("clean the old info");
                }
            }

            if (!isSameCharacterInfo)
            {
                info = in_characterInfo;
                Debug.Log("" + name + " Change Character --> " + info.name);

                //Set Level ready character informations
                SetLevel(start_level);

                if (animator)
                {
                    if (info.act_controller != null)
                    {
                        animator.runtimeAnimatorController = info.act_controller;
                    }
                    else
                    {
                        Debug.LogWarning("info.act_controller have nothing !! ");
                        // do Load the resource path

                        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(info.act_controller_path);
                    }

                    //
                    animator.SetBool("isEnemy", in_playerType == Type.Enemy);
                    //DoNeedDisapperForStart();

                    //Config Type
                    my_type = in_playerType;

                    //Config Details
                    cur_HP = myState.hp;
                    cur_DPS = myState.speed;

                    if (in_playerType == Type.Enemy || in_playerType == Type.Stuff)
                    {
                        //Change target cur_DPS
                        cur_DPS = info.onLeaveTime;
                    }
                    else {
                        if (in_playerType == Type.Player)
                        {
                            //Do other cases config
                            cur_status = Status.Active; //Set to active as Player have no more wave.
                            ResetDPSCoins();
                        }
                        else
                        {

                        }
                    }

                }
            }
            else {
                Debug.LogWarning(" info == in_characterInfo ! please check ");
                SetLevel(start_level);
            }
        }
        else
        {/// Reset as null
            info = null;

            
            cur_HP = 0;
            my_type = Type.None; // for empty

            if (animator)
            {//Do remove the Controller
                animator.runtimeAnimatorController = null;
            }
        }
    }

    public void SetLevel(int in_level) {
        if (info != null) {
            if (myState == null)
            {
                myState = new Characher.State();
                myState.InitWith(info, in_level);
            }
            else
            {
                myState.UpdateStateWithLevel(in_level);
            }
        }
    }

    public void ResetDPSCoins()
    {// Config DFP counter

        if (info != null)
        {
            //do reset
            cur_DPS = myState.speed;
            if (my_type == Type.Enemy)
            {
                cur_DPS = info.onLeaveTime; //Change target cur_DPS
            }

            if (coinList.Count < cur_DPS && coinsPrefab)
            {
                for (int c_idx = coinList.Count; c_idx < cur_DPS; c_idx++)
                {
                    if (coinList.Count < cur_DPS)
                    {
                        GameObject n_coin = GameObject.Instantiate(coinsPrefab, this.transform);
                        n_coin.transform.rotation = new Quaternion(0, 180, 180, 0);
                        n_coin.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                        float pos_X = 0.45f - (0.12f * (c_idx / 20));
                        if (my_type == Type.Player)
                        {
                            pos_X = -0.45f + (0.12f * (c_idx / 20));
                        }

                        float pos_y = 0.032f * (c_idx % 20);
                        //set
                        n_coin.transform.localPosition = new Vector3(pos_X, pos_y, 0);

                        coinList.Add(n_coin);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        else {
            Debug.LogWarning("Character Info is missing");
        }
    }

    private void RemoveAllDPSCoins()
    {
        cur_DPS = 0;
        if (coinList.Count > 0) {
            Debug.Log("RemoveAllDPSCoins -- start Remove All DPF Coins : count ? " + coinList.Count);
            for (int i = coinList.Count - 1; i >= 0; i--)
            {
                GameObject del = coinList[i];
                coinList.RemoveAt(i);
                Destroy(del);
            }
            Debug.Log("RemoveAllDPSCoins --  Remove Coins done : count ? " + coinList.Count);
        }
        
    }


    /// ----------- Public Animation Action Call Methods ----------- ///


    public void DoCountDownDPS(int in_coins)
    {
        if (cur_status == Status.Death)
        {
            Debug.Log("DoCountDownDPS -- " + GetCharacterName() + " is Death ");
        }
        else if (cur_status == Status.Active)
        {
            if (my_type == Type.Player || my_type == Type.Enemy)
            {
                if (info != null)
                {
                    if (cur_DPS > 0)
                    {
                        cur_DPS -= in_coins;

                        //remove coin
                        if (coinList.Count > 0)
                        {
                            GameObject rm_coin = coinList[coinList.Count - 1];
                            coinList.Remove(rm_coin);
                            GameObject.Destroy(rm_coin);
                        }

                        if (cur_DPS <= 0)
                        {
                            if (my_type == Type.Enemy && !info.isAtk)
                            {
                                DoRun();
                            }
                            else
                            {
                                ResetDPSCoins();
                                //do call attack event , wait GameMain to handle target
                                action_delegate?.Invoke(this);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log(info.name + " -> DoCountDownDPS   cur_DPS ? " + cur_DPS + "\n in_coins ? " + in_coins);
                    }
                }
                else
                {
                    Debug.LogWarning(gameObject.name + " Missing Character Info");
                }

            }
        }
        else {
            
            
        }
    }

    public void DoAttackDebug()
    {
        DoAttack();
    }

    public int DoAttack()
    {
        if (info != null && animator) {
            animator.SetTrigger("DoAttack");

            Debug.Log(info.name + " -> DoAttack " );
        }

        return myState.ap;
    }

    public void DoHit(int in_Damage)
    {
        if (info != null && animator)
        {
            animator.SetTrigger("GetHitted");
        }

        int receivedDamage = in_Damage - myState.dp;
        if (cur_HP - receivedDamage <= 0)
        {
            cur_HP = 0;
            DoDeath();
        }
        else {
            cur_HP -= receivedDamage;
        }

        {
            GameObject temp_counter = GameObject.Instantiate(countingPrefab);
            temp_counter.transform.position = transform.position;
            temp_counter.GetComponent<NumberUpHelper>().DoAnimForHPWith("-"+ in_Damage + " HP");
        }
    }

    public void DoDeath()
    {
        if (info != null && animator)
        {
            animator.SetTrigger("IsDead");

            cur_status = Status.Death;
            //do call back
            death_delegate?.Invoke(this);

            RemoveAllDPSCoins();
        }
    }

    public void DoRun() {
        if (info != null && animator)
        {
            animator.SetTrigger("IsRun");

            cur_status = Status.Death;
            //do call back
            run_delegate?.Invoke(this);
        }
    }

    public void DoJoin()
    {
        if (info != null && animator)
        {
            animator.SetTrigger("DoJoin");
        }
    }

    public void DoNeedDisapperForStart()
    {
        if (info != null && animator)
        {
            animator.SetTrigger("NeedDisapperForStart");
            Debug.Log("DoNeedDisapperForStart called");
        }
    }

    public void DoResetFromOutSide()
    {
        isReseting = true;
        isResetAnimActionCalled = false;
        //Cyrus : and wait to update
        Debug.Log("DoResetFromOutSide called");
    }

    private void DoResetAnimAction() {
        if (info != null && animator && !isResetAnimActionCalled)
        {
            animator.SetTrigger("isReadyResetFrom");

            Debug.Log("DoResetAnimAction called");
            isResetAnimActionCalled = true;
        }
    }

    /// ----------- Public Delegate Methods ----------- ///
    public void SetWaitingToStartDelegate(ReadyToStartWave in_method)
    {
        if (waitToStart_delegate != in_method)
        {
            waitToStart_delegate = in_method;
        }
    }

    public void SetActionDelegate(ReadyToAttack in_method)
    {
        if (action_delegate != in_method)
        {
            action_delegate = in_method;
        }
    }

    public void SetDeathDelegate(AlreadyDead in_method)
    {
        if (death_delegate != in_method)
        {
            death_delegate = in_method;
        }
    }

    public void SetRunDelegate(AlreadyRun in_method)
    {
        if (run_delegate != in_method)
        {
            run_delegate = in_method;
        }
    }
    



}
