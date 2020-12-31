using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
//using Newtonsoft.Json;

namespace CommonUtil
{
    public class CVSHelper
    {
        public static List<Dictionary<string , string>> ConvertCsvFileToJsonObject(string path)
        {
            //const string spaceChar= " ";
            //const string spaceReplace = "_";
            string[] lines = GetTextAsset(path);
            //File.ReadAllLines(fullPath);
            if (lines != null) {
                List<string[]> csv = new List<string[]>();
                foreach (string line in lines) {
                    string line_exc = line;
                    //if (line.Contains(spaceChar)) {
                    //    line_exc = line.Replace(spaceChar , spaceReplace);
                    //}
                    string[] splitVals;
                    if (line_exc.Contains("\""))
                    {
                        string finText = ReplaceStringContentWithQuoteMark(line_exc);
                        
                        Debug.Log(""+ finText);
                        splitVals = finText.Split(',');
                    }
                    else {
                        splitVals = line_exc.Split(',');
                    }

                    csv.Add(splitVals);

                }
                    

                string[] properties = lines[0].Split(',');

                List<Dictionary<string, string>> listObjResult = new List<Dictionary<string, string>>();

                for (int i = 1; i < lines.Length; i++)
                {
                    Dictionary<string, string> objResult = new Dictionary<string, string>();
                    for (int j = 0; j < properties.Length; j++)
                    {
                        objResult.Add(properties[j], csv[i][j]);
                    }

                    listObjResult.Add(objResult);
                }

                if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
                {
//                    Debug.Log("listObjResult result " + JsonConvert.SerializeObject(listObjResult));
                }

                return listObjResult;
            }

            return null;
        }

        private static string ReplaceStringContentWithQuoteMark(string in_text) {

            string[] contentTexts = in_text.Split('\"'); 

            List<string> comb = new List<string>();
            for (int i = 0; i < (contentTexts).Length; i ++) {
                if (i > 0 && i % 2 == 1)
                {
                    comb.Add(contentTexts[i].Replace(",", "_"));
                }
                else
                {
                    comb.Add(contentTexts[i]);
                }
            }

            return string.Join("", comb);
        }

        public static string GetStreamongAssetPath(string in_path)
        {
            /*
             * reference path : https://docs.unity3d.com/Manual/StreamingAssets.html
             * Most platforms (Unity Editor, Windows, Linux players, PS4, Xbox One, Switch) use Application.dataPath + "/StreamingAssets",
             * macOS player uses Application.dataPath + "/Resources/Data/StreamingAssets",
             * iOS uses Application.dataPath + "/Raw",
             * Android uses files inside a compressed APK JAR file, "jar:file://" + Application.dataPath + "!/assets".
             */

            //appPath = Application.dataPath;

            //Debug.Log("dataPath : " + appPath);

            //if (Application.platform == RuntimePlatform.Android)
            //{
            //    appPath =  "jar:file://" + Application.dataPath + "!/assets";
            //}

            //if (Application.platform == RuntimePlatform.IPhonePlayer)
            //{
            //    appPath = Application.dataPath + "/Raw";
            //}

            string appPath;


            appPath = Application.persistentDataPath;
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
            {
                appPath = "Assets/Resources";
            }


            return appPath + "/";
        }

        public static string[] GetTextAsset(string in_path) {

            const string csvExtention = ".csv";
            string[] fileLine = null;
            string appPath = GetStreamongAssetPath(in_path) + in_path  + csvExtention;
            
            if (File.Exists(appPath))
            {
                fileLine = File.ReadAllLines(appPath);
            }
            else {
                Debug.Log("File [" + appPath + "] not Exist , change to Read build-in files");
                appPath = in_path;
                TextAsset SourceFile = (TextAsset)Resources.Load(in_path, typeof(TextAsset));
                if (SourceFile)
                {
                    string text = SourceFile.ToString();
                    if (text != null && text.Length > 0)
                    {
                        text = text.Replace("\r", "");
                        Debug.Log(" [file : " + appPath + "] got text ? \n" + text);
                        fileLine = text.ToString().Split("\n".ToCharArray());
                        Debug.Log(" txt line count " + fileLine.Length + "\n last line ? " + fileLine[fileLine.Length - 1]);
                    }
                    else
                    {
                        Debug.LogWarning(" [file : " + appPath + "] got nothing !");
                    }
                }
                else
                {
                    Debug.LogWarning(" [file : " + appPath + "] got nothing ! SourceFile is null");
                }
            }

            return fileLine;
        }

    }

    public class InGameCoreDataStore : MonoBehaviour
    {
        //for Game scene setup
        private string target_stageID;
        private List<string> target_playerTeam;

        /// <summary>
        /// Key : rarity
        /// Value : array Exp_require
        /// </summary>
        private Dictionary<string, List<int>> characterLevelTableCoreData;
        /// <summary>
        /// Key : classType
        /// Value : array of dict for each state with key
        /// </summary>
        private Dictionary<string, List<Dictionary<string,int>>> characterClassLevelStateTableCoreData;
        private List<Dictionary<string, string>> charactersCoreData;
        private List<Dictionary<string, string>> stageLevelCoreData;
        private List<Dictionary<string, string>> skillListCoreData;
        

        private static InGameCoreDataStore _instance = null;
        public static InGameCoreDataStore instance
        {
            get { return _instance ?? (_instance = InGameCoreDataStore.Create()); }
        }
        private static InGameCoreDataStore Create()
        {

            string key = "_CommonUtil_";
            GameObject myUtils = GameObject.Find(key);
            if (myUtils == null)
            {
                myUtils = new GameObject(key);
                myUtils.AddComponent<InGameCoreDataStore>();
            }

            _instance = myUtils.GetComponent<InGameCoreDataStore>();

            return _instance;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            //init list of CoreData
            DoInitLoadingTask();
        }

        private void Update()
        {
             
        }

        //------- Threading ----- ///
        private System.Threading.Thread t = null;
        private void DoInitLoadingTask()
        {
            //t = new Thread(Run);
            //t.Start();

            LoadCharacterCoreData();
            LoadStageLevelCoreData();
            LoadSkillListCoreData();
            LoadCharacterLevelTable();
            LoadCharacteClassrLevelStateTable();
        }

        private void LoadCharacterCoreData() {
            if (charactersCoreData == null)
            {
                charactersCoreData = CVSHelper.ConvertCsvFileToJsonObject("Game Specification Items lists Excel - Character List");
            }
        }

        private void LoadStageLevelCoreData()
        {
            if (stageLevelCoreData == null)
            {
                stageLevelCoreData = CVSHelper.ConvertCsvFileToJsonObject("Game Specification Items lists Excel - Stage List");
            }
        }

        private void LoadSkillListCoreData()
        {
            if (skillListCoreData == null)
            {
                skillListCoreData = CVSHelper.ConvertCsvFileToJsonObject("Game Specification Items lists Excel - Skill List");
            }
        }

        private void LoadCharacterLevelTable() {
            if (characterLevelTableCoreData == null) {
                List<Dictionary<string, string>> datas = CVSHelper.ConvertCsvFileToJsonObject("Game Specification Items lists Excel - Character Level");

                if (datas != null)
                {
                    characterLevelTableCoreData = new Dictionary<string, List<int>>();

                    //format the data
                    foreach (Dictionary<string, string> rowDic in datas)
                    {
                        string key_rarity = rowDic["rarity"];
                        int level_index = int.Parse(rowDic["level_index"]);
                        int exp = int.Parse(rowDic["exp_require"]);

                        //Debug.Log("key_rarity ["+ key_rarity + "]" +
                        //    "level_index [" + level_index + "]" +
                        //    "exp [" + exp + "]");

                        if (characterLevelTableCoreData.ContainsKey(key_rarity))
                        {
                            List<int> lv_list = characterLevelTableCoreData[key_rarity];
                            lv_list.Add(exp);
                        }
                        else
                        {
                            List<int> n_lv_list = new List<int>();
                            n_lv_list.Add(exp);
                            characterLevelTableCoreData.Add(key_rarity, n_lv_list);
                        }
                    }
                }
                else {
                    Debug.LogWarning("Cant load Character Level CVS or missing , Please check!");
                }                
            }
        }

        private void LoadCharacteClassrLevelStateTable()
        {
            if (characterClassLevelStateTableCoreData == null)
            {
                //TODO
                List<Dictionary<string, string>> datas = CVSHelper.ConvertCsvFileToJsonObject("Game Specification Items lists Excel - Character Class Level state");

                if (datas != null)
                {
                    ////format the data
                    //foreach (Dictionary<string, string> rowDic in datas)
                    //{
                    //    string key_rarity = rowDic["rarity"];
                    //    int level_index = int.Parse(rowDic["level_index"]);
                    //    int exp = int.Parse(rowDic["exp_require"]);
                    //    if (characterLevelTableCoreData.ContainsKey(key_rarity))
                    //    {
                    //        List<int> lv_list = characterLevelTableCoreData[key_rarity];
                    //        lv_list.Add(exp);
                    //    }
                    //    else
                    //    {
                    //        List<int> n_lv_list = new List<int>();
                    //        n_lv_list.Add(exp);
                    //        characterLevelTableCoreData.Add(key_rarity, n_lv_list);
                    //    }
                    //}
                }
            }
        }


        private void Run()
        {
            ThreadFunc();
        }

        protected virtual void ThreadFunc()
        {

        }


        public List<Dictionary<string, string>> GetAllStageData() {

            LoadStageLevelCoreData();
            return stageLevelCoreData;
        }
        
        public Dictionary<string, string> GetStageDataById(string in_id)
        {
            LoadStageLevelCoreData();

            if (stageLevelCoreData != null)
            {
                foreach (Dictionary<string, string> dic in stageLevelCoreData)
                {
                    string val;
                    string key = "id";
                    dic.TryGetValue(key, out val);
                    if (val.CompareTo(in_id) == 0)
                    {
                        Debug.Log("Stage data found :: dic ? " + dic);
                        return dic;
                    }
                }
            }

            Debug.Log("Stage data not found");

            return null;
        }

        public Dictionary<string, string> GetCharactorDataById(string in_id)
        {
            if (charactersCoreData == null)
            {
                LoadCharacterCoreData();
            }

            if (charactersCoreData != null)
            {
                foreach (Dictionary<string, string> dic in charactersCoreData)
                {
                    string val;
                    string key = "id";
                    dic.TryGetValue(key, out val);
                    if (val.CompareTo(in_id) == 0)
                    {
                        Debug.Log("Character data found :: dic ? " + dic);
                        return dic;
                    }
                }
            }

            Debug.Log("Character data not found");

            return null;
        }

        public Dictionary<string, string> GetSkillDataById(string in_id)
        {
            if (skillListCoreData == null)
            {
                LoadSkillListCoreData();
            }

            if (skillListCoreData != null)
            {
                foreach (Dictionary<string, string> dic in skillListCoreData)
                {
                    string val;
                    string key = "id";
                    dic.TryGetValue(key, out val);
                    if (val.CompareTo(in_id) == 0)
                    {
                        Debug.Log("Skill data found :: dic ? " + dic);
                        return dic;
                    }
                }
            }

            Debug.Log("Skill data not found");

            return null;
        }

        public List<int> GetCharacterLevelList(int in_rarity) {

            if (characterLevelTableCoreData == null) {
                LoadCharacterLevelTable();
            }

            if (characterLevelTableCoreData != null) {
                if (characterLevelTableCoreData.ContainsKey(in_rarity.ToString())) {
                    return characterLevelTableCoreData[in_rarity.ToString()];
                }
            }

            return null;
        }

        public List<Dictionary<string,int>> GetCharacterClassLevelStateList(int classTypeID)
        {

            if (characterClassLevelStateTableCoreData == null)
            {
                LoadCharacteClassrLevelStateTable();
            }

            if (characterClassLevelStateTableCoreData != null)
            {
                if (characterClassLevelStateTableCoreData.ContainsKey(classTypeID.ToString()))
                {
                    return characterClassLevelStateTableCoreData[classTypeID.ToString()];
                }
            }

            return null;
        }

        public void SetSelectedIngameContent(string in_stageID , List<string> in_playerTeam)
        {
            target_stageID = in_stageID;
            target_playerTeam = in_playerTeam;
        }

        public string GetSelectedStage()
        {
            return target_stageID;
        }

        public List<string> GetTeam()
        {
            return target_playerTeam;
        }
    }

}


