using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberUpHelper : MonoBehaviour
{
    public TextMesh myText;
    public SpriteRenderer myicon;
    
    public Vector3 moveDirection;
    public float maxMoveDist = 0;
    public float moveSpeed = 1;

    private bool isStart;
    private float movedDist = 0;
    private Vector3 org_pos;

    private bool isDisplayHP;

    void Start()
    {
        myText.color = new Color(1, 1, 1, 0);
        if (myicon) myicon.color = new Color(1, 1, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isStart) {
            if (movedDist < maxMoveDist)
            {
                movedDist += Time.deltaTime * moveSpeed;

                UpdateMoveStyle_1();

                if (movedDist >= maxMoveDist)
                {
                    DoDestory();
                }
            }
        }
        
    }

    private void UpdateMoveStyle_1() {
        Color n_color;
        if (movedDist > maxMoveDist * 0.6f)
        {
            float dif = maxMoveDist * 0.4f;
            float dist = movedDist - maxMoveDist * 0.6f;
            float alpa = 1 - (dist / dif);

            n_color = isDisplayHP ? new Color(1, .1f, .1f, alpa) : new Color(1, 1, 1, alpa);
        }
        else {
            n_color = isDisplayHP ? new Color(1, .1f, .1f) :  new Color(1, 1, 1)  ;
        }

        //Update 
        myText.color = n_color;
        if(myicon) myicon.color = n_color;
        
        transform.position = new Vector3(
            org_pos.x + moveDirection.x * movedDist,
            org_pos.y + moveDirection.y * movedDist,
            org_pos.z + moveDirection.z * movedDist);
    }

    private void DoDestory() {
        GameObject.Destroy(gameObject);
    }

    /// -------------- Public Function -------------- /// 

    public void DoAnimForHPWith(string in_text)
    {
        isDisplayHP = true;
        DoAnimStartWith(in_text);
    }

    public void DoAnimStartWith(string in_text) {
        DoAnimStartWith(in_text, null);
    }

    public void DoAnimStartWith(string in_text , Sprite in_sprite)
    {
        org_pos = transform.position;
        myText.text = in_text;

        if (in_sprite != null)
        {
            myicon.sprite = in_sprite;
        }
        else {
            GameObject.Destroy(myicon.gameObject);
        }

        isStart = true;
    }

}
