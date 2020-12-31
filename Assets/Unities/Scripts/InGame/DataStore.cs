using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGameDataBase
{
    public class Character
    {
        public enum ClassType
        {
           Warrior,
           Hunter,
           Knight,
           Witch
        }

        public enum ClassRank
        {
            Normal,
            Common,
            Uncommon,
            Rare,
            VeryRare,
            SuperRare,
            UltraRare
        }

        public enum ClassElement
        {
            Fire,
            Water,
            Wind,
            Dirt
        }

        public string id;
        public string name;
        public string description;
        public ClassType classType;
        public ClassRank rarity;
        public ClassElement classElement;
        public int startingLevel;
        public int healthPoint;
        public int attackPoint;
        public int defencePoint;
        public int attackSpeed;  //DPS
        public Skill skill;
        public Equipment equipment;
        public int onLeaveTime;
        public int itemQuality;
        public bool isAtk;
        public string itemPack;

        //Not From Data, in this moment
        public RuntimeAnimatorController act_controller;
        public string act_controller_path;

        public void initWith( string in_id )
        {
            id = in_id;
            {
                string path_prefix = "ImportedPack/Actor_Anims/";
                string path_subfix = "/Animations/";
                string characterFolderName = "";

                if (int.Parse(id) <= 100)
                {
                    characterFolderName = "actor_" + id;
                }
                else if (int.Parse(id) > 100 && (int.Parse(id) <= 200))
                {//Monster
                    // do other cases
                    characterFolderName = "m_" + (int.Parse(id) - 100).ToString();
                }
                else
                {//Stuff

                }

                List<string> animateNames = new List<string>();
                List<RuntimeAnimatorController> animates = new List<RuntimeAnimatorController>();
                {//do set up all animator controller
                    animateNames.Add("AnimationsController");

                    animates.Add(act_controller);

                    //set AnimatorOverrideControllers
                    foreach (string name in animateNames) {
                        int idx = animateNames.IndexOf(name);
                        act_controller_path = path_prefix + characterFolderName + path_subfix + name;
                        Debug.Log("["+ idx.ToString() + "]RuntimeAnimatorController filepath[" + act_controller_path + "] is now load ");
                        RuntimeAnimatorController rac = Resources.Load<RuntimeAnimatorController>(act_controller_path);
                        if (rac == null) {
                            Debug.LogWarning("RuntimeAnimatorController not found");
                        }
                        else {
                            animates[idx] = rac;
                        }
                    }
                }
                
                // init other properties from the CSV
                Dictionary<string, string> data = CommonUtil.InGameCoreDataStore.instance.GetCharactorDataById(in_id);
                if (data != null) {
                    //Debug.Log(" data.Values : \n " + data.Values.Count);
                    //foreach (string val in data.Values) {
                    //    Debug.Log(" data.Value " + val) ;
                    //}
                    const string nullVal = "NaN";
                    foreach (string key in data.Keys) {
                        string val;
                        data.TryGetValue(key, out val);

                        if (val != null && !val.Contains(nullVal)) {
                            switch (key)
                            {
                                case "name":  name = val; break;
                                case "description":  description = val;   break;
                                case "classType":  classType = (ClassType)int.Parse(val); break;
                                case "rarity": rarity = (ClassRank)int.Parse(val); break;
                                case "classElement":  classElement = (ClassElement)int.Parse(val); break;
                                case "startingLevel": startingLevel = int.Parse(val); break;
                                case "healthPoint":  healthPoint = int.Parse(val); break;
                                case "attackPoint":  attackPoint = int.Parse(val); break;
                                case "defencePoint": defencePoint = int.Parse(val); break;
                                case "attackSpeed":  attackSpeed = int.Parse(val); break;
                                case "skill_id":
                                    {
                                        skill = new Skill();
                                        skill.initWith(val);
                                    }
                                    break;
                                case "equipment_id":
                                    {
                                        int eqID = int.Parse(val);
                                        //Cyrus : To do
                                        //do init equipment
                                    }
                                    break;
                                    default: Debug.Log("unknown key[" + key + "] & value[" + val + "] not set");  break;
                                case "onLeaveTime": onLeaveTime = int.Parse(val); break;
                                case "isAtk": isAtk = (int.Parse(val) > 0 ); break;
                                case "itemPack_id": itemPack = val; break;
                                case "itemQuality": itemQuality = int.Parse(val); break;
                            }
                        }
                        else {
                            Debug.LogWarning("key[" + key + "] is null value");
                        }
                    }
                }
            }
        }
    }

    public class Skill
    {
        public enum Type
        {
            Phyical,
            Magic,
            Buff
        }

        public enum Element
        {
            Fire,
            Water,
            Wind,
            Dirt
        }

        public string id;
        public string name;
        public Type type;
        public int level;
        public int coolDownTime;
        public int power;
        public string classSkill;

        public void initWith(string in_id)
        {
            id = in_id;
            {
                const string nullVal = "NaN";
                // init other properties from the CSV
                Dictionary<string, string> data = CommonUtil.InGameCoreDataStore.instance.GetSkillDataById(id);
                if (data != null)
                {
                    foreach (string key in data.Keys)
                    {
                        string val;
                        data.TryGetValue(key, out val);

                        if (val != null && !val.Contains(nullVal))
                        {
                            //id	name	type	level	coolDownTime	power	classSkill
                            switch (key)
                            {
                                case "id": id = val; break;
                                case "name": name = val; break;
                                //case "description": description = val; break;
                                case "type": type = (Type)int.Parse(val); break;
                                case "level": level = int.Parse(val); break;
                                case "coolDownTime": coolDownTime = int.Parse(val); break;
                                case "power": coolDownTime = int.Parse(val); break;
                                case "classSkill": classSkill = val; break;

                                default: Debug.LogWarning("unknown key[" + key + "] & value[" + val + "] not set"); break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("key[" + key + "] is null value");
                        }
                    }
                }
            }
        }
    }

    public class Equipment
    {
        public enum Type
        {
            Ammor,
            Weapon,
            Ring
        }

        public string id;
        public string name;
        public string description;
        public Type type;
        public int attackPoint;
        public int defencePoint;
    }

    public class DataStore
    {
        

        void init()
        {
            

        }
    }
}


