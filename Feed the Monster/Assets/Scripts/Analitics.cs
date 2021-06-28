﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Firebase;
using Firebase.Messaging;
using Firebase.Analytics;
using Facebook.Unity;

public class Analitics : MonoBehaviour
{
	public static Analitics Instance;
	private delegate void DeferredEvent();
	private DeferredEvent deferred;
	public UINotificationPopup popup;
	public bool isReady = false;
	private const int MAXUSERPROPERTIES = 25;


        public List<string> ListOfSubskills;

    public int numsessions;
    public float sessionstart, avgsession, totalplaytime;
    public double sessionend;
    public System.DateTime m_StartTime;
    public System.DateTime lastplayed;

    public void TryAddNewSubskill(string SSN)
    {
        string subskillname = SSN.ToLower();
        if (!ListOfSubskills.Contains(subskillname))
        {
            ListOfSubskills.Add(subskillname);
        }
    }


    public void TryImproveSubskill(int levelnum, string SSN, float amt)
    {
        if (!UserInfo.Instance.HasEarnedSubskill(levelnum))
        {
            UserInfo.Instance.SetEarnedSubskill(levelnum);
            ImproveSubskill(SSN, amt);

        }
    }

    public void ImproveSubskill(string SSN, float amt)
    {
        float value = UserInfo.Instance.GetSubskillValue(SSN);
        value = value + amt;
        value = Mathf.Min(value, 100f);
        UserInfo.Instance.SetSubskillValue(SSN, value);
        ReportSubskillIncrease(SSN);
    }


    public void ReportSubskillIncrease(string SSN)
    {
        float value = UserInfo.Instance.GetSubskillValue(SSN);
        Debug.Log("improving " + SSN + " to " + value);
        treckEvent(AnaliticsCategory.SubSkills, AnaliticsAction.SubskillIncrease,SSN,value);

    }



    public void startTimeTracking(int uid)
    {
        if (PlayerPrefs.HasKey(uid + "_numSessions"))
        {

            numsessions = PlayerPrefs.GetInt(uid + "_numSessions");
            totalplaytime = PlayerPrefs.GetFloat(uid + "_totalPlayTime");

        }
        else
        {
            numsessions = 0;
            totalplaytime = 0f;

        }

        m_StartTime = System.DateTime.Now;
        Debug.Log(m_StartTime);

    }

		void Awake()
	    {
	        Instance = this;
			// we need to explicitly exclude the editor to prevent Player crashes
	#if UNITY_ANDROID && !UNITY_EDITOR
			activateFacebook();

	#endif
		}

		// Use this for initialization
		void Start ()
		{

			#if UNITY_ANDROID && !UNITY_EDITOR
			FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
			{
				var dependencyStatus = task.Result;
				if (dependencyStatus == Firebase.DependencyStatus.Available)
				{
					var app = FirebaseApp.DefaultInstance;
					Instance.isReady = true;
					deferred.Invoke(); // log events that were captured before initialization
					Debug.Log("successfully initialized firebase");
				}
				else
				{
					Debug.LogError(System.String.Format(
						"Could not resolve all Firebase dependencies: {0}", dependencyStatus));
				}
			});

			#endif
			/*if (Firebase.Analytics.FirebaseAnalytics != null) {
				Firebase.Analytics.FirebaseAnalytics.StartSession ();
			}*/
		}

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            startTimeTracking(UsersController.Instance.CurrentProfileId);
        }
        else
        {
            dotimetracking();
        }

    }

    private void OnApplicationQuit()
    {
        dotimetracking();
    }

    private void OnApplicationPause(bool pauseStatus)
	    {
	        if (!pauseStatus)
	        {
				activateFacebook();
           
	        }
				
	    }

	    void OnDisable() {
			/*if (Firebase.Analytics.FirebaseAnalytics != null) {
				Firebase.Analytics.FirebaseAnalytics.StopSession ();
			}*/
		}



    public void dotimetracking()
    {
        var uid = UsersController.Instance.CurrentProfileId;
        var timeSpan = System.DateTime.Now.Subtract(m_StartTime);
         Debug.Log("current session (" + numsessions + ")" + (float)timeSpan.TotalSeconds);

        totalplaytime += (float)timeSpan.TotalSeconds;
        numsessions += 1;
         Debug.Log("total playtime: " + totalplaytime);
        avgsession = totalplaytime / numsessions;
          Debug.Log("Average: " + avgsession);
        PlayerPrefs.SetInt(uid + "_numSessions", numsessions);
        PlayerPrefs.SetFloat(uid + "_totalPlayTime", totalplaytime);

        float sincelastdays = 0f;

        if (PlayerPrefs.HasKey(uid + "_LastPlayed"))
        {
            lastplayed = System.DateTime.Parse(PlayerPrefs.GetString(uid + "_LastPlayed"));
            var sincelast = System.DateTime.Now.Subtract(lastplayed);
            sincelastdays = (float)sincelast.TotalDays;
              Debug.Log("since last play:" + sincelastdays + "days");
        }


        PlayerPrefs.SetString(uid + "_LastPlayed", System.DateTime.Now.ToString());

        ReportTimeTracking(uid, avgsession, totalplaytime, sincelastdays);
    }

    public void ReportTimeTracking(int uid, float avgplaytime, float totalplaytime, float dayssincelast)
    {


        treckEvent(AnaliticsCategory.TimeTracking, AnaliticsAction.AvgSession,"average_session" ,avgsession);
        treckEvent(AnaliticsCategory.TimeTracking, AnaliticsAction.TotalPlaytime, "total_playtime" , totalplaytime);
        treckEvent(AnaliticsCategory.TimeTracking, AnaliticsAction.DaysSinceLast, "days_since_last", dayssincelast);
    }


    public void treckScreen (string screenName)
	{
		#if  UNITY_ANDROID && !UNITY_EDITOR
			FirebaseAnalytics.SetCurrentScreen (screenName, null);
		#endif
	}

    public void trackwithuserids(AnaliticsCategory category, AnaliticsAction action, string label, float value, int userid)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(category.ToString(), new Firebase.Analytics.Parameter[] {
            new Firebase.Analytics.Parameter (
                "action", action.ToString()
            ),
            new Firebase.Analytics.Parameter (
                "label", label
            ),
            new Firebase.Analytics.Parameter (
                "value", value
            ),
            new Firebase.Analytics.Parameter(
                "userid", userid
            )

        });
    }


	public void treckEvent (AnaliticsCategory category, AnaliticsAction action, string label, long value = 0)
	{
		treckEvent (category, action.ToString (), label, value);
	}
    public void treckEvent(AnaliticsCategory category, AnaliticsAction action, string label, float value)
    {
        treckEvent(category, action.ToString(), label, value);
    }

    public void treckEvent(AnaliticsCategory category, string action, string label, float value)
    {

        Firebase.Analytics.FirebaseAnalytics.LogEvent(category.ToString(), new Firebase.Analytics.Parameter[] {
            new Firebase.Analytics.Parameter (
                "action", action
            ),
            new Firebase.Analytics.Parameter (
                "label", label
            ),
            new Firebase.Analytics.Parameter (
                "value", value
            )
        });

    }

    public void treckEvent (AnaliticsCategory category, string action, string label, long value = 0)
	{
		#if UNITY_ANDROID
		if(!isReady) //defer events that fire before Firebase is initialized
        {
			deferred += () =>
			{
				treckEvent(category, action, label, value);
			};
			return;
        }
		FirebaseAnalytics.LogEvent (category.ToString (), new Firebase.Analytics.Parameter[] {
			new Firebase.Analytics.Parameter (
				"action", action
			),
			new Firebase.Analytics.Parameter (
				"label", label
			),
			new Firebase.Analytics.Parameter (
				"value", value
			)
		});
		#endif
	}

	public void logNotification(FirebaseMessage message)
    {
		if(!isReady)
        {
			deferred += () =>
			{
				logNotification(message);
			};
			return;
        }
		FirebaseAnalytics.LogEvent("notification_received", new Parameter[]
		{
			new Parameter(
				"message_id", message.MessageId
				),
			new Parameter(
				"message_type", message.MessageType
				),
			new Parameter (
				"notif_opened", message.NotificationOpened.ToString()
				),
		});
    }

	private void activateFacebook()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		if (FB.IsInitialized)
		{
			FB.ActivateApp();
		}
		else
		{
			FB.Init(() =>
			{
				FB.ActivateApp();
				int isFirstOpen = PlayerPrefs.GetInt("isFirst");
				if (isFirstOpen == 0)
				{
					Debug.Log("first open");
					FB.Mobile.FetchDeferredAppLinkData(FbDeepLinkCallback);
					PlayerPrefs.SetInt("isFirst", 1);
				}
				else
				{
					FB.GetAppLink(FbDeepLinkCallback);
				}


			});
		}
		#endif
	}

	void FbDeepLinkCallback(IAppLinkResult result)
    {
		Debug.Log("received result");
		if(!string.IsNullOrEmpty(result.TargetUrl))
        {
            Debug.Log("received Deep link URL: ");
            Debug.Log(result.TargetUrl);
			setDeepLinkUserProperty(result);
        }
    }

	private void setDeepLinkUserProperty(IAppLinkResult result)
    {
		List<string[]> parameters = parseDeepLink(result.TargetUrl);
		for (int i=0; i < parameters.Count; i++)
        {
			if (i > MAXUSERPROPERTIES) break; //Firebase will not accept more properties
			string[] vals = parameters[i];
			FirebaseAnalytics.SetUserProperty(vals[0], vals[1]);
			Debug.Log(string.Format("User Property \"{0}\" set to \"{1}\"", vals[0], vals[1]));
        }
    }

	public void setUserProperty(string prop, string val)
    {
		FirebaseAnalytics.SetUserProperty(prop, val);
    }

    List<string[]> parseDeepLink(string url)
    {
		string prefix = "feedthemonster://";
		char paramParseChar = '/';
		char valParseChar = '=';
		string cleanUrl = url.Replace(prefix, "").TrimStart().TrimEnd();
		string[] split_url = cleanUrl.Split(paramParseChar);
		List<string[]> paramList = new List<string[]>();
		for (int i=0; i < split_url.Length; i++)
        {
			string[] vals = split_url[i].Split(valParseChar);
			if(vals[0] != null && vals[1] != null)
            {
				paramList.Add(vals);
            }
        }
		return paramList;

    }

}
