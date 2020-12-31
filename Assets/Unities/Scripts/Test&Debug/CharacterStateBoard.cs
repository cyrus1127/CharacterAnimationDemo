using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class CharacterStateBoard : MonoBehaviour
{
    public Text txt_characterName;
    public Text txt_HP;
    public Text txt_AP;
    public Text txt_DP;
    public Text txt_Speed;

    public InputField input_ID;
    public Text txt_level;
    public Slider slider_level;
    public Text txt_levelExp;

    public InGameCharacterController controller;
    private InGameDataBase.Character demoCharacter;


    // Start is called before the first frame update
    void Start()
    {

        //set
        InGameDataBase.Character demoCharacter = new InGameDataBase.Character();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (txt_level.text != slider_level.value.ToString()) {
            txt_level.text = slider_level.value.ToString();
            UpdateCharacter();
        }
    }

    public void ChangeCharacter()
    {
        if (input_ID != null) {
            if (input_ID.text.Length > 0) {

                if (demoCharacter == null) {
                    demoCharacter = new InGameDataBase.Character();
                }

                demoCharacter.initWith(input_ID.text);
                controller.SetCharacterInfo(demoCharacter, InGameCharacterController.Type.Player , (int) slider_level.value);
                UpdateCharacter();
            }
        }
    }

    private void UpdateCharacter()
    {

        if (controller != null) {
            controller.SetLevel( (int)slider_level.value );

            //set
            txt_levelExp.text = controller.GetNextLevelExpReq().ToString();
            txt_characterName.text = controller.GetCharacterName();
            txt_HP.text = controller.GetBaseHealthPoint().ToString();
            txt_AP.text = controller.GetAttackPoint().ToString();
            txt_DP.text = controller.GetDefencePoint().ToString();
            txt_Speed.text = controller.GetSpeed().ToString();
        }

    }

}
