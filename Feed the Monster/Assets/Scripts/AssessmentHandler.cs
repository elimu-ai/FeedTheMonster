﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

public class AssessmentHandler : MonoBehaviour
{
    public static AssessmentHandler instance;

    public GameObject webviewprefab;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }


    public void tryAssessment()
    {
        Debug.Log("checking if assessment is needed");
        if (CheckIfNeeded())
        {
            Debug.Log("assessment is needed");
            showAssessment();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void  markneededafterlast()
    {
        var uid = UsersController.Instance.CurrentProfileId;
        PlayerPrefs.SetInt("u" + uid + "assessmentNeeded", 1);
    }

    public bool CheckIfNeeded()
    {
        bool res = false;
        var uid = UsersController.Instance.CurrentProfileId;
        if (PlayerPrefs.HasKey("u" + uid + "assessmentNeeded"))
        {
            if (PlayerPrefs.GetInt("u" + uid + "assessmentNeeded") == 1)
            {
                res = true;
            }
            else
            {
                var lasttested = System.DateTime.Parse(PlayerPrefs.GetString("u" + uid + "_LastTested"));

                var timeSpan = System.DateTime.Now.Subtract(lasttested);

                if (timeSpan.TotalDays > 30)
                {
                    PlayerPrefs.SetInt("u" + uid + "assessmentNeeded", 1);
                    res = true;
                }

                res = false;
            }
        }
        else
        {
            PlayerPrefs.SetInt("u" + uid + "assessmentNeeded", 1);
            res = true;
        }

        return res;
    }


    public void MarkClosedEarly()
    {
        var uid = UsersController.Instance.CurrentProfileId;
        if (PlayerPrefs.HasKey("u" + uid + "assessmentDisconnected"))
        {
            var oldfails = PlayerPrefs.GetInt("u" + uid + "assessmentDisconnected");
            oldfails = oldfails + 1;
            PlayerPrefs.SetInt("u" + uid + "assessmentDisconnected", oldfails);

        }
        else
        {
            PlayerPrefs.SetInt("u" + uid + "assessmentDisconnected", 1);
        }

            
        Parameter[] paramz = {
            new Parameter("uuid", PlayerPrefs.GetString("uuid" + uid)),
          new Parameter("highestUnlockedLevel",  UserInfo.Instance.GetHighestOpenLevel ()),
        };
        FirebaseAnalytics.LogEvent("AssessmentClosedEarly", paramz);
    }

    public void MarkTestStarted()
    {
      
        var uid = UsersController.Instance.CurrentProfileId;
        var oldfails = 0;
        if (PlayerPrefs.HasKey("u" + uid + "assessmentDisconnected"))
        {
             oldfails = PlayerPrefs.GetInt("u" + uid + "assessmentDisconnected");

        }
        PlayerPrefs.SetInt("u" + uid + "assessmentDisconnected", 0);
            Parameter[] paramz = {
          new Parameter("uuid", PlayerPrefs.GetString("uuid" + uid)),
          new Parameter("highestUnlockedLevel",  UserInfo.Instance.GetHighestOpenLevel ()),
          new Parameter("previouslyDisconnected", oldfails)
        };
        FirebaseAnalytics.LogEvent("AssessmentStarted", paramz);
    }

    public void MarkTested()
    {
        var uid = UsersController.Instance.CurrentProfileId;
        PlayerPrefs.SetString("u" + uid + "_LastTested", System.DateTime.Now.ToString());
        PlayerPrefs.SetInt("u" + uid + "assessmentNeeded", 0);
        Parameter[] paramz = {
           new Parameter("uuid", PlayerPrefs.GetString("uuid" + uid)),
          new Parameter("highestUnlockedLevel",  UserInfo.Instance.GetHighestOpenLevel ()),
        };
        FirebaseAnalytics.LogEvent("AssessmentCompleted", paramz);
    }
    public void showAssessment()
    {
        var go = GameObject.Instantiate(webviewprefab,UIController.Instance.MapPanel.transform);
        
    }
}
