using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Networking;
using System.Linq;
//using System;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;



public class DataGathering : MonoBehaviour
{
    [System.Serializable]
    public class Data //overall class to hold all the data
    {
        public string UserID; //participantID, likely needs to be a unique ID for each user?
        public string SessionsID; //can be randomly generated for that session
        public string TimeStamp; //time when this session started
        public string Location; //user's location
        public List<DataEntry> DataEntries; //list of data entries
        public EndOfSessionReport EndOfSessionReport; //end of session report that is gathered at the end

    }
    [System.Serializable]
    public class DataEntry //class to contain the data for a partiuclar entry
    {
        public string ActionCompleted; //e.g. Quest Started.
        public int UniqueQuestID; //unique ID set up for that quest
        public string TypeOfQuest_ID; //Id
        public float RequiredAmount; //in Meters i assume?
        public float CurrentAmount; //also probably in Meters?
        public string TimeStamp; //as a timestamp
        public bool QuestCompleted; //has this entry been marked as the completion of a quest?
        //public string TimeWhenUserStopped; //thinking about it, is this useful?

        public DataEntry Duplicate()
        {
            DataEntry ToReturn = new DataEntry();
            ToReturn.ActionCompleted = this.ActionCompleted;
            ToReturn.UniqueQuestID = this.UniqueQuestID;
            ToReturn.TypeOfQuest_ID = this.TypeOfQuest_ID;
            ToReturn.RequiredAmount = this.RequiredAmount;
            ToReturn.CurrentAmount = this.CurrentAmount;
            ToReturn.TimeStamp = this.TimeStamp;
            ToReturn.QuestCompleted = this.QuestCompleted;
            return ToReturn;
        }
    }

    [System.Serializable]
    public class EndOfSessionReport //class to contain the data for the end of report session, gets calculated from the rest of the data
    {
        public string TimeFinished = string.Empty;
        public float TotalDistanceTravelled = 0f; //the total distance travelled by the user in this session
        public float TotalCollected = 0f; //the total of the collect quests objects "collected"? by tge user
        public float TimeTotal = 0f;
        public QuestStatistics WalkingQuests_Stats = new QuestStatistics(); //stats data structure of the walking quests
        public QuestStatistics CollectingQuests_Stats = new QuestStatistics(); //stats data structure for the collecting quests

    }

    

    [System.Serializable]
    public class QuestStatistics
    {
        public float TotalTimeSpent_Minutes = 0f; //total time user has spent in this type of task
        public int TotalProgressedOn = 0; //total number of this type of task this user has progressed on
        public int TotalCompleted = 0; //total number of this type of task this user has completed
    }

    [System.Serializable]
    public class EndOfSessionReport_Firebase
    {
        public string TS = string.Empty;
        public string TF = string.Empty;
        public string TT = string.Empty;
        public string TDT = string.Empty;
        public string TCo = string.Empty;

        public EndOfSessionReport_Firebase ConvertToFirebaseVersion(Data ReportToConvert) 
        {
            EndOfSessionReport_Firebase toReturn = new EndOfSessionReport_Firebase();
            toReturn.TS = ReportToConvert.TimeStamp;
            toReturn.TF = ReportToConvert.EndOfSessionReport.TimeFinished;
            toReturn.TT = ReportToConvert.EndOfSessionReport.TimeTotal.ToString();
            toReturn.TDT = ReportToConvert.EndOfSessionReport.TotalDistanceTravelled.ToString();
            toReturn.TCo = ReportToConvert.EndOfSessionReport.TotalCollected.ToString();
            return toReturn;
        }

    }
    [System.Serializable]
    public class QuestStatistics_Firebase
    {
        public float TTSS;
        public float TPO;
        public float TCm;

        public QuestStatistics_Firebase ConvertToFirebaseVersion(QuestStatistics StatsToConvert) 
        {
            QuestStatistics_Firebase toReturn = new QuestStatistics_Firebase();
            toReturn.TTSS = StatsToConvert.TotalTimeSpent_Minutes;
            toReturn.TPO = StatsToConvert.TotalProgressedOn;
            toReturn.TCm = StatsToConvert.TotalCompleted;
            return toReturn;
        }
    }
    // Start is called before the first frame update
    public static DataGathering dataGathering; //static reference for the data gathering script
    
    public bool TelemetryActive; //bool for if the telemetry gathering should be active
    public bool FirebaseAnalyticsTracking;
    [Header("--------------------------------------------------------------------------")]
    public Data dataJson; //structure for storing data
    public string endOfSessionReportOverall_Firebase;
    public string endOfSessionReportWalkingQuests_Firebase;
    public string endOfSessionReportCollectingQuests_Firebase;
    [Header("--------------------------------------------------------------------------")]

    [Header("End of Session")]
    public List<int> ListOfUniqueQuestIDs; //list to store a list of unique quest IDs
    public List<DataEntry> CurrentUsedDataEntry; //list to store data entries for comparison, will be the start and stop points for quests
    public List<int> EntryCheckForThisID_Index = new List<int>();

    public List<int> ListOfNumbers = new List<int>();

    public List<float> Walking_TimeSpent = new List<float>();
    public List<System.DateTime> Walking_TSpent = new List<System.DateTime>();
    public List<System.DateTime> Collecting_TSpent = new List<System.DateTime>();

    [Header("PHP")]
    public string FileName;
    //public string FileName; //desired name of the file
    public string FolderName; //folder name for the file to be saved to
    //public string DataToAdd; //data to be passed thru the php
    public string PHPLocation; //PHP location

    //Input fields etc for the data gathering UI
    [Header("Session Data")]
    public TMP_InputField ParticpantID_IF;
    public TMP_InputField SessionID_IF;
    public TMP_InputField Date_IF;
    public TMP_InputField Time_IF;
    public TMP_InputField Location_IF;

    [Header("Tutorial")]
    public System.DateTime TutorialStartTime;
    public System.DateTime TutorialEndTime;

    [Header("Entry Data")]
    public bool UsingDebugUI = false;
    public TMP_InputField ActionCompleted_IF;
    public TMP_InputField UniqueQuestID_IF;
    public TMP_InputField TypeOfQuestID_IF;
    public TMP_InputField RequiredAmount_IF;
    public TMP_InputField CurrentAmount_IF;
    public TMP_InputField TimeUserStarted_IF;
    public TMP_InputField TimeUserStopped_IF;
    public Toggle QuestCompletedToggle;
    public string CurrentScene_Debug = "VentureMode";
    public Button[] CurrentSceneButtons;
    public TextMeshProUGUI PreviewText;
    public List<string> CropsTemp;
    public List<string> FoodTemp;

    [Header("--------------------------------------------------------------------------")]
    public string DateTimeStamp1;
    public string DateTimeStamp2;
    public void Awake()
    {
        if (!dataGathering) //if the data gathering singleton hasnt been set, then set this instance as it
        {
            Debug.Log("setting singleton");
            dataGathering = this;
            DontDestroyOnLoad(this);
           
            if (FirebaseAnalyticsTracking) 
            {
                /*
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Debug.Log("Analytics Tracking On");
                    TestSetUserID();
                });
                */
            }
            
            
            


        }
        else 
        {
            Debug.Log("singleton already exists, deleting object now");
            Destroy(this.gameObject); //if it already exists, then delete this gameobject
        }

    }

    public void Start()
    {
        SetSceneButtonsColours();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; //https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings generate a random string for this session
        var stringChars = new char[10];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        var finalString = new string(stringChars);
        ParticpantID_IF.text = finalString;
        dataJson.TimeStamp = System.DateTime.Now.ToString();
        //UpdateHeaderData();


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            //TestSetUserID();
        }
    }
    public void SetUserID(string UserID) 
    {
        if (FirebaseAnalyticsTracking) 
        {
            FirebaseAnalytics.SetUserId(UserID);
            Debug.Log("Set user ID");
        }
            
    }
    public void CompileBeginingOfSession(string UniqueUserID, string UserLocation) //generate the header data required for the begining of the session
    {
        dataJson.DataEntries = new List<DataEntry>(); //create a new list of the data entires in the datajson
        dataJson.EndOfSessionReport = new EndOfSessionReport();
        dataJson.UserID = UniqueUserID; //set the UniqueUserID 
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; //https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings generate a random string for this session
        var stringChars = new char[10];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        var finalString = new string(stringChars);
        dataJson.SessionsID = finalString;

        string CurrentDate = System.DateTime.Now.ToString(); //get the current date
        //CurrentDate = CurrentDate.Replace("/", "-");
        dataJson.TimeStamp = CurrentDate;


        dataJson.Location = UserLocation; //get the user's location

    }

    public int ReturnUniqueQuestID() //take in the already used QuestIDs
    {
        int NumberToReturn = 0; //set the number to return as 0
        ListOfNumbers = new List<int>(); //init the list of numbers

        for (int i = 0; i < 100; i++) // this whole thing is slightly overkill but better to be overkill, generate 100 random numbers between 0 and 100,000 and add that number to the list
        {
            int RandomNumber = Random.Range(0, 100000);
            ListOfNumbers.Add(RandomNumber);
            
        }
        ListOfNumbers = ListOfNumbers.Distinct().ToList(); //remove any duplicates from the list


        foreach (DataEntry DE in dataJson.DataEntries) //for each of the data entries in dataJson
        {
            if (ListOfNumbers.Contains(DE.UniqueQuestID)) //if the ListOfNumbers contain the unique ID currently being looked at
            {
                ListOfNumbers.RemoveAll(item => item == DE.UniqueQuestID); //remove all instances of that unique ID from the list of numbers
            }
        }
        NumberToReturn = ListOfNumbers[0]; //get the first number in the ListOfNumbers and return this as the UniqueQuestID
        return NumberToReturn;
    
    
    }

    public IEnumerator DifferenceBetweenTimeStamps() 
    {
        DateTimeStamp1 = System.DateTime.Now.ToString();
        Debug.Log(DateTimeStamp1);
        yield return new WaitForSecondsRealtime(120f);

        DateTimeStamp2 = System.DateTime.Now.ToString();
        Debug.Log(DateTimeStamp2);

        System.TimeSpan TS = System.DateTime.Parse(DateTimeStamp2) - System.DateTime.Parse(DateTimeStamp1);
        Debug.Log(TS.TotalSeconds);
    }

    public void AddLine(string ActionCompleted, int UniqueQuestID, string TypeOfQuest,float RequiredAmount, float CurrentAmount, bool WasQuestCompleted) //add a quest log to the telemetry file
    {
        if (TelemetryActive) //if the telemetry gathering is active
        {
            DataEntry DE = new DataEntry(); //create a new data entry
            DE.ActionCompleted = ActionCompleted; //fill in the action completed section based on what was passed in via parameters
            DE.UniqueQuestID = UniqueQuestID; //fill in the UniqueQuestID based on what was passed in
            DE.TypeOfQuest_ID = TypeOfQuest; //fill in the type of TypeOfQuestID based on what was passed in
            DE.RequiredAmount = RequiredAmount; //fill in the RequiredAmount based on what was passed in
            DE.CurrentAmount = CurrentAmount; //fill in the CurrentAmount based on what was passed in
            DE.QuestCompleted = WasQuestCompleted; //fill in the QuestCompleted based on what was passed in
            string CurrentTime = System.DateTime.Now.ToString(); //get the current time
            DE.TimeStamp = CurrentTime; //Set the time stamp

            //do a check to see if this entry already has 2 entries, then replace the oldest
            int EntryCheckForThisID = 0; //set this counter back to 0
            EntryCheckForThisID_Index = new List<int>(); //reinit the entry check list
            foreach (DataEntry existingDE in dataJson.DataEntries) //foreach of the entries in the list
            {
                if (existingDE.UniqueQuestID == UniqueQuestID) //if the newly created entry's unique quest ID matches an existing unique quest id
                {
                    EntryCheckForThisID++; //increment this counter
                    EntryCheckForThisID_Index.Add(dataJson.DataEntries.IndexOf(existingDE)); //add this index to the list of indexes
                }
            }
            if (EntryCheckForThisID == 2) //if there are 2 entries for a quest
            {
                dataJson.DataEntries[EntryCheckForThisID_Index[EntryCheckForThisID_Index.Count - 1]] = DE; //overwrite the second entry
            }
            else 
            {
                dataJson.DataEntries.Add(DE.Duplicate()); //add the newly completed entry to the the list of DataEntries on the dataJson
            }
            
            if (UsingDebugUI) 
            {
                PreviewText.text = JsonUtility.ToJson(dataJson, true); //update the preview text
            }
            


        }
    
    }

    public EndOfSessionReport CompileEndOfSessionReport(float DistanceTravelledFromVentureMode) //generate the data for the end of session report 
    {
        EndOfSessionReport Report = new EndOfSessionReport(); //create a new end of session report and add the calculated variables into it
        
        //Unqiue list of Quest IDs
        ListOfUniqueQuestIDs = new List<int>(); //init the list of UniqueQuestIDs
        float DistanceTravelled = DistanceTravelledFromVentureMode; //create a float to track the distance travelled by the user
        float Collected = 0f;
        Walking_TSpent = new List<System.DateTime>();
        Walking_TimeSpent = new List<float>();
        foreach (DataEntry DE in dataJson.DataEntries) //for each of the data entries collected, add each one to the list of unique quest IDs
        {
            ListOfUniqueQuestIDs.Add(DE.UniqueQuestID);
        }
        ListOfUniqueQuestIDs =  ListOfUniqueQuestIDs.Distinct().ToList(); //make the list unique. In theory, the list should contain 2 of each type of quest entry
        foreach (int ID in ListOfUniqueQuestIDs) //for each of these unique quest IDs
        {
            CurrentUsedDataEntry = new List<DataEntry>(); //reinit the current used data entry list
            for (int i = 0; i <= dataJson.DataEntries.Count; i++) //iterate through the whole list of data entries
            {
                if (dataJson.DataEntries[i].UniqueQuestID == ID) //if the current iterated entry's Unique Quest ID is equal to the ID that is currently being looked for
                {
                    CurrentUsedDataEntry.Add(dataJson.DataEntries[i]); //add that entry to the current used data entry list
                }
                if (CurrentUsedDataEntry.Count == 2) //if the CurrentUsedDataEntry count is equal to, then we have the quest starting entry and the quest ending entry, so we can break and skip the rest of the list
                {
                    break;
                }
            }
            //Debug.Log("Data Entries Found for - " + ID);
            if (CurrentUsedDataEntry.Count == 2) //this needs to be equal to 2 so that that the relevant data can be calculated
            {
                if (CurrentUsedDataEntry[0].TypeOfQuest_ID == "Walk") //if this quest is a walking quest, then calculate the distance travelled
                {
                    //float DistTravelledForThisQuest = (CurrentUsedDataEntry[1].CurrentAmount - CurrentUsedDataEntry[0].CurrentAmount); //calculate the distance travelled for this quest by taking the current amount entry in the "quest stopped" entry and the current amount entry in the "quest started" entry
                    //DistanceTravelled += DistTravelledForThisQuest; //add the float calculated above to the overall distance travelled float
                    //System.TimeSpan TimeSpentOnThisQuest = System.DateTime.Parse(CurrentUsedDataEntry[1].TimeStamp) - System.DateTime.Parse(CurrentUsedDataEntry[0].TimeStamp); //calculate how much time the user spent on this quest, in minutes
                    //Report.WalkingQuests_Stats.TotalTimeSpent_Minutes += (float)TimeSpentOnThisQuest.TotalMinutes; //add the float calculated above to the overall time spent

                    //total time spent walking
                    //System.TimeSpan TimeSpentOnThisQuest = System.DateTime.Parse(CurrentUsedDataEntry[1].TimeStamp) - System.DateTime.Parse(CurrentUsedDataEntry[0].TimeStamp); //calculate how much time the user spent on this quest, in Minutes
                    //Walking_TimeSpent.Add((float)TimeSpentOnThisQuest.TotalMinutes);

                    //Debug.Log(CurrentUsedDataEntry[0].TimeStamp + " " + CurrentUsedDataEntry[1].TimeStamp);
                    System.DateTime s = System.DateTime.Parse(CurrentUsedDataEntry[0].TimeStamp); //create a new datestamp called S and save this datestamp to it
                    Walking_TSpent.Add(s); //add s to the Walking_Tspent list
                    s = System.DateTime.Parse(CurrentUsedDataEntry[1].TimeStamp); //save this timestamp to s
                    Walking_TSpent.Add(s);//add s to the Walking_Tspent list


                    Report.WalkingQuests_Stats.TotalProgressedOn++;
                    if (CurrentUsedDataEntry[0].QuestCompleted | CurrentUsedDataEntry[1].QuestCompleted) //if this data entry is reported as a "finished quest" then increment the finished quest integer
                    {
                        Report.WalkingQuests_Stats.TotalCompleted++;
                    }
                }
                if (CurrentUsedDataEntry[0].TypeOfQuest_ID == "Collect") 
                {
                    float CollectedForThisQuest = (CurrentUsedDataEntry[1].CurrentAmount - CurrentUsedDataEntry[0].CurrentAmount);//calculate the amount collected for this quest using the same logic as above
                    Collected += CollectedForThisQuest; //add the float calculated above to the overall collected for this quest
                    //System.TimeSpan TimeSpentOnThisQuest = System.DateTime.Parse(CurrentUsedDataEntry[1].TimeStamp) - System.DateTime.Parse(CurrentUsedDataEntry[0].TimeStamp); //calculate how much time the user spent on this quest, in Minutes
                    //Report.CollectingQuests_Stats.TotalTimeSpent_Minutes += (float)TimeSpentOnThisQuest.TotalMinutes; //add the float calculated above to the overall time spent

                    System.DateTime s = System.DateTime.Parse(CurrentUsedDataEntry[0].TimeStamp);
                    Collecting_TSpent.Add(s);
                    s = System.DateTime.Parse(CurrentUsedDataEntry[1].TimeStamp);
                    Collecting_TSpent.Add(s);

                    Report.CollectingQuests_Stats.TotalProgressedOn++;
                    if (CurrentUsedDataEntry[0].QuestCompleted | CurrentUsedDataEntry[1].QuestCompleted) //if this data entry is reported as a "finished quest" then increment the finished quest integer
                    {
                        Report.CollectingQuests_Stats.TotalCompleted++;
                    }
                }
                

                
            }
            
        }
        //QuestsStarted = ListOfUniqueQuestIDs.Count(); //set the quests started integer to the number of Unique quest IDs


        string CurrentTime = System.DateTime.Now.ToString(); ; //get the current date
        Debug.Log(CurrentTime);
        //CurrentTime = CurrentTime.Replace("/", "-");

        //Debug.Log(ReturnHighestNumberInList(Walking_TimeSpent));

        //calculate time spent on walking quests
        if (Walking_TSpent.Count >= 2) //if the walking quest count is higher than 2
        {
            List<System.DateTime> W_list = new List<System.DateTime>(); //create a new list of date time
            W_list = ReturnHightestAndLowestDateTimes(Walking_TSpent); //get the highest and lowest datetimes of the walking quests
            System.TimeSpan W_TS = W_list[1] - W_list[0]; //find the timespan between the highest and lowest entries of the walking quests
            Report.WalkingQuests_Stats.TotalTimeSpent_Minutes = (float)W_TS.TotalMinutes; //set the total time spent in walking quests in minutes
        }
        else 
        {
            Report.WalkingQuests_Stats.TotalTimeSpent_Minutes = 0f; //if the condition is false, then theres no walking quests tracked so no data
        }

        if (Collecting_TSpent.Count >= 2) //if the collecting quest count is higher than 2
        {
            //calculate time spent on collecting quests
            List<System.DateTime> C_list = new List<System.DateTime>(); //create a new list of datetime
            C_list = ReturnHightestAndLowestDateTimes(Collecting_TSpent); //get the highest and lowest datetimes of the collecting quests
            System.TimeSpan C_TS = C_list[1] - C_list[0]; //find the timespan between the highest and lowest of the collecting quests
            Report.CollectingQuests_Stats.TotalTimeSpent_Minutes = (float)C_TS.TotalMinutes; //set the total time spent in walking quests in minutes
        }
        else 
        {
            Report.CollectingQuests_Stats.TotalTimeSpent_Minutes = 0f; //if the condition is false, then theres no collecting quests tracked so no data
        }


        

        //Report.WalkingQuests_Stats.TotalTimeSpent = (float)TimeSpent_WalkingQuests;
        //Report.TotalQuestsProgressedOn = QuestsStarted;
        //Report.QuestsCompleted = QuestsCompletedInSession;
        
        Report.TimeFinished = System.DateTime.Now.ToString();
        System.TimeSpan SessionTimeTotal = System.DateTime.Parse(Report.TimeFinished) - System.DateTime.Parse(dataJson.TimeStamp);
        Report.TimeTotal = (float)SessionTimeTotal.TotalMinutes;
        Report.TimeTotal = RoundMinutes(Report.TimeTotal);
        Report.TotalDistanceTravelled = DistanceTravelled;
        Report.TotalCollected = Collected;

        Report.WalkingQuests_Stats.TotalTimeSpent_Minutes = RoundMinutes(Report.WalkingQuests_Stats.TotalTimeSpent_Minutes);
        Report.CollectingQuests_Stats.TotalTimeSpent_Minutes = RoundMinutes(Report.CollectingQuests_Stats.TotalTimeSpent_Minutes);

        return Report;
        //dataJson.EndOfSessionReport = Report;
        //dataJson.EndOfSessionReport.TotalTimeSpentInMinutes = (float)TimeSpent;

    }


    public float ReturnHighestNumberInList(List<float> ChosenList) //dont think i need this anymore??
    {
        float ToReturn;
        ToReturn = ChosenList[0];
        foreach (int num in ChosenList) 
        {
            if (num > ToReturn) 
            {
                ToReturn = num;
            }
        }

        return ToReturn;
    }

    public List<System.DateTime> ReturnHightestAndLowestDateTimes(List<System.DateTime> listToParse) //return the highest and lowset datetimes from the passed in list
    {
        List<System.DateTime> list = new List<System.DateTime>(); //create a new list of datetimes
        //get the lowest of the time spans
        System.DateTime lowest = listToParse[0]; //create a new datetime to hold the lowest datetime and give it the first entry in listToParse
        foreach (System.DateTime STS in listToParse) //for each of the datetime in listToParse
        {
            int result = System.DateTime.Compare(STS, lowest); //compare the current lowest to the current DateTime to check
            //Debug.Log(lowest + " - " + STS + " - " + result);
            switch (result) 
            {
                case (-1): //if sts is lower than the lowest, then set the lowest as the STS
                    lowest = STS;
                    break;

                case (0):

                    break;

                case (1):

                    break;


            }
        }

        //get the highest of the time spans
        System.DateTime highest = listToParse[0];//create a new datetime to hold the highest datetime from the passed in list
        foreach (System.DateTime STS in listToParse) //foreach of the datetimes in listToParse
        {
            int result = System.DateTime.Compare(STS, highest); //compare the STS to the current highest date time
            //Debug.Log(highest + " - " + STS + " - " + result);
            switch (result)
            {
                case (-1):
                    ;
                    break;

                case (0):

                    break;

                case (1):
                    highest = STS;//if the STS is higher than the current highest, then replace the highest
                    break;


            }
        }
        //Debug.Log("Lowest TS - " + lowest);
        //Debug.Log("Highest HS - " + highest);
        list.Add(lowest); //add the lowest to the list
        list.Add(highest); //add the highest to the list
        return list; //return this list
    
    }


    public void SaveToFile(float DistanceTravelled) //still temporary, wont need most of the lines in this function
    {
        dataJson.EndOfSessionReport = CompileEndOfSessionReport(DistanceTravelled); //generate an end of report session and add it do the dataJson
        string DataToPush = JsonUtility.ToJson(dataJson,true); //convert the dataJson into a json string with pretty print enabled
        //Debug.Log(DataToPush);
        FileName = dataJson.UserID + "_" + dataJson.TimeStamp + "_"  + dataJson.SessionsID + ".json";
        FileName = FileName.Replace(":", "-");
        FileName = FileName.Replace("/", "-");

#if UNITY_EDITOR
        string path = Application.streamingAssetsPath + "/" + dataJson.UserID + "/" +  FileName; //temporary: generate a path to save the data to

        //create folder
        if (!Directory.Exists(Application.streamingAssetsPath + "/" + dataJson.UserID)) 
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/" + dataJson.UserID);
        }
        StreamWriter SW = new StreamWriter(path); //temporary: create a new streamwriter with the path
        SW.WriteLine(DataToPush); //temporary: write the data to the streamwriter
        SW.Close(); //close the streamwriter
#endif
        //StartCoroutine(TestLocalPHP()); //run the TestLocalPHP coroutuine, may need to rename this
        

        Firebase_UploadEndOfSessionData();
        if (UsingDebugUI)
        {
            PreviewText.text = JsonUtility.ToJson(dataJson, true);
        }
        
    }

    //firebase tracking
    public void Firebase_QuestAccepted(string TypeOfQuestStarted) 
    {
        if (FirebaseAnalyticsTracking)  //if the firebase analytics are tracking
        {
            Parameter P = new Parameter("QuestType", TypeOfQuestStarted); //add the "TypeOfQuestStarted" under the "QuestType" parameter 
            FirebaseAnalytics.LogEvent("QuestStarted", P); //log a "QuestStarted" event taking in "P" as the paraemter
            
        }
        Debug.Log("FB - Quest Started - " + TypeOfQuestStarted);


    }
    public void Firebase_QuestCompleted(string TypeOfQuestEnded) 
    {
        if (FirebaseAnalyticsTracking) 
        {
            Parameter P = new Parameter("QuestType", TypeOfQuestEnded); //add the "TypeOfQuestEnded" under the "QuestType" parameter 
            FirebaseAnalytics.LogEvent("QuestEnded", P);//log a "QuestEnded" event taking in "P" as the paraemter
            Debug.Log("FB - Quest Ended - " + TypeOfQuestEnded);

        }
            
    }
    public void Firebase_QuestLapsed(string TypeOfQuestLapsed,float MaxAmount, float ProgressedAmount) //progress should be calculated 
    {
        if (FirebaseAnalyticsTracking)
        {
            string QuestProgress_String = TypeOfQuestLapsed + "-" + ProgressedAmount.ToString() + "/" + MaxAmount.ToString(); //get the quest progress as a string i.e. 12/89
            Parameter P = new Parameter("QuestType", TypeOfQuestLapsed); //add the "TypeOfQuestLapsed" under the "QuestType" parameter
            Parameter P2 = new Parameter("Progress", QuestProgress_String); //add the "QuestProgress_String" under the "Progress" parameter
            FirebaseAnalytics.LogEvent("QuestLapsed", P, P2); //log a "QuestLapsed" event taking in P and P2 as parameters
            Debug.Log("FB - Quest Lapsed - " + TypeOfQuestLapsed + " - " + QuestProgress_String);
        }
            
    }

    public void Firebase_CropPlanted(string TypeOfCropPlanted) 
    {
        if (FirebaseAnalyticsTracking) 
        {
            Parameter P = new Parameter("CropType", TypeOfCropPlanted); //add the type of crop planted under "crop type"
            FirebaseAnalytics.LogEvent("CropPlanted", P); //log a "CropPlanted" event taking in P as the parameter
            Debug.Log("FB - CropPlanted - " + TypeOfCropPlanted);
        }
    }

    public void Firebase_CropHarvested(string TypeOfCropHarvested)
    {
        if (FirebaseAnalyticsTracking)
        {
            Parameter P = new Parameter("CropType", TypeOfCropHarvested); //add the type of crop harvested under "crop type"
            FirebaseAnalytics.LogEvent("CropHarvested", P); //log a "CropHarvested" event taking in P as the parameter
            Debug.Log("FB - CropHarvested- " + TypeOfCropHarvested);
        }
    }

    public void Firebase_FoodMade(string TypeOfFoodMade)
    {
        if (FirebaseAnalyticsTracking)
        {
            Parameter P = new Parameter("FoodType", TypeOfFoodMade); //add the type of food made under "food type"
            FirebaseAnalytics.LogEvent("FoodMade", P); //log a "FoodMade" event taking in P as the parameter
            Debug.Log("FB - FoodMade - " + TypeOfFoodMade);

        }
    }


    public void Firebase_ChangingGameMode(string CurrentGameMode, string DestinationGameMode)  //takes in the current gamemode and the destination game mode// split into entering level and exiting level //old but still being used in my UI
    {
        
        //debuging
        CurrentScene_Debug = DestinationGameMode; //set the current scene debug as the DestinationGameMode
        SetSceneButtonsColours(); //set the scene buttons colours in Ethan's testing scene
        if (FirebaseAnalyticsTracking) 
        {
            //level ending
            Parameter Param_CurrentGameMode_Name = new Parameter(FirebaseAnalytics.ParameterLevelName, CurrentGameMode); //create a new LevelName parameter and take in the CurrentGameMode
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, Param_CurrentGameMode_Name); //log an "EventLevelEnd" event taking in Param_CurrentGameMode_Name

            //level starting
            Parameter Param_DestinationGameMode_Name = new Parameter(FirebaseAnalytics.ParameterLevelName, DestinationGameMode); //create a new LevelName parameter and take in the DestinationGameMode
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, Param_DestinationGameMode_Name); //log an "EventLevelStart" taking in Param_DestinationGameMode_Name
        }
       
    }

    public void Firebase_LeavingGameMode(string GameMode) //takes in the game mode of the scene
    {
        if (FirebaseAnalyticsTracking) 
        { 
            Parameter param_CurrentGameMode_Name  = new Parameter(FirebaseAnalytics.ParameterLevelName, GameMode); //create a new level name paraemter and take in the game moode
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, param_CurrentGameMode_Name); //log a level end event taking in the param_currentgamemode_name parameter
            Debug.Log("FB - LeavingGameMode - " + GameMode);
        }
        
    }

    public void Firebase_EnteringGameMode(string GameMode) //takes in the game mode of the scene
    {
        if (FirebaseAnalyticsTracking)
        {
            Parameter param_CurrentGameMode_Name = new Parameter(FirebaseAnalytics.ParameterLevelName, GameMode); //create a new level name parameter taking in the game mode
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, param_CurrentGameMode_Name); //log a level start event taking in in param_currentgamemode_name
            Debug.Log("FB - EnteringGameMode - " + GameMode);
        }
        
    }

    public void Firebase_TutorialStarted() 
    {
        if (FirebaseAnalyticsTracking) 
        {
            FirebaseAnalytics.LogEvent("TutorialStarted");
            Debug.Log("FB - Tutorial Stared");
        }
    }
    public void Firebase_TutorialCompleted(float TimeSpentInTutorial)
    {
        if (FirebaseAnalyticsTracking)
        {
            Parameter param_TimeSpentInTutorial = new Parameter("TimeSpentInTutorial", TimeSpentInTutorial);
            FirebaseAnalytics.LogEvent("TutorialCompleted", param_TimeSpentInTutorial);
            Debug.Log("FB - Tutorial Completed - " + TimeSpentInTutorial);
        }
    }

    public void Firebase_TutorialOpened()
    {
        if (FirebaseAnalyticsTracking)
        {
            FirebaseAnalytics.LogEvent("TutorialOpened");
            TutorialStartTime = System.DateTime.Now;
            Debug.Log("FB - Tutorial Guidebook Opened");
        }
    }
    public void Firebase_TutorialClosed()
    {
        if (FirebaseAnalyticsTracking)
        {
            TutorialEndTime = System.DateTime.Now;
            System.TimeSpan totalTimeSpent = TutorialEndTime - TutorialStartTime;
            float Minutes = RoundMinutes((float)totalTimeSpent.TotalMinutes);
            Parameter param_GuideBookOpenTimeMinutes = new Parameter("GuidebookOpenTimeMinutes", Minutes);
            FirebaseAnalytics.LogEvent("TutorialClosed", param_GuideBookOpenTimeMinutes);
            Debug.Log("FB - Tutorial Guidebook Closed" + Minutes);
        }
    }




    public void Firebase_UploadEndOfSessionData() //uploads a clipped down version of the end of session report
    {
        if (FirebaseAnalyticsTracking)
        {
            //Debug.Log("Gathered data to be uploaded to Firebase"); 


            //OLD//
            //EndOfSessionReport_Firebase ESRFB = new EndOfSessionReport_Firebase().ConvertToFirebaseVersion(dataJson); //create a new EndOfSessionReport_Firebase and convert the EndOfSessionReport from the dataJson to it
            //endOfSessionReportOverall_Firebase = JsonUtility.ToJson(ESRFB); //Convert the above created to a JsonString

            //QuestStatistics_Firebase QSFB_Walking = new QuestStatistics_Firebase().ConvertToFirebaseVersion(dataJson.EndOfSessionReport.WalkingQuests_Stats);//create a new QuestStatistics_Firebase and convert the collecting quest stats from the end of session repor from the dataJson to it
            //endOfSessionReportWalkingQuests_Firebase = JsonUtility.ToJson(QSFB_Walking);//Convert the above created to a JsonString

            //QuestStatistics_Firebase QSFB_Collecting = new QuestStatistics_Firebase().ConvertToFirebaseVersion(dataJson.EndOfSessionReport.CollectingQuests_Stats);//create a new QuestStatistics_Firebase and convert the collecting quest stats from the end of session repor from the dataJson to it
            //endOfSessionReportCollectingQuests_Firebase = JsonUtility.ToJson(QSFB_Collecting);//Convert the above created to a JsonString
            //END OLDS


            //end of session report events
            Parameter ESR_TimeStarted = new Parameter("ESR_TimeStarted", dataJson.TimeStamp); //create a bunch of new parameters for the various varibales stored in the end of session reports
            Parameter ESR_TimeFinished = new Parameter("ESR_TimeFinished", dataJson.EndOfSessionReport.TimeFinished);//
            Parameter ESR_TimeTotal = new Parameter("ESR_TimeTotal", dataJson.EndOfSessionReport.TimeTotal); //
            Parameter ESR_TotalDistanceTravelled = new Parameter("ESR_TotalDistanceTravelled", dataJson.EndOfSessionReport.TotalDistanceTravelled); //
            Parameter ESR_TotalCollected = new Parameter("ESR_TotalCollected", dataJson.EndOfSessionReport.TotalCollected);//

            // walking quest stats
            Parameter ESR_W_TotalTimeSpentMinutes = new Parameter("ESR_W_TotalTimeSpentMinutes", dataJson.EndOfSessionReport.WalkingQuests_Stats.TotalTimeSpent_Minutes); //
            Parameter ESR_W_TotalProgressedOn = new Parameter("ESR_W_TotalProgressedOn", dataJson.EndOfSessionReport.WalkingQuests_Stats.TotalProgressedOn); //
            Parameter ESR_W_TotalCompleted = new Parameter("ESR_W_TotalCompleted", dataJson.EndOfSessionReport.WalkingQuests_Stats.TotalCompleted); //

            // walking quest stats
            Parameter ESR_C_TotalTimeSpentMinutes = new Parameter("ESR_C_TotalTimeSpentMinutes", dataJson.EndOfSessionReport.CollectingQuests_Stats.TotalTimeSpent_Minutes);
            Parameter ESR_C_TotalProgressedOn = new Parameter("ESR_C_TotalProgressedOn", dataJson.EndOfSessionReport.CollectingQuests_Stats.TotalProgressedOn);
            Parameter ESR_C_TotalCompleted = new Parameter("ESR_C_TotalCompleted", dataJson.EndOfSessionReport.CollectingQuests_Stats.TotalCompleted);




            //Parameter Param_Overall = new Parameter("GatheredSessionData_Overall", endOfSessionReportOverall_Firebase); //add the "endOfSessionReportOverall_Firebase" under the "GatheredSessionData_Overall" 
            //Parameter Param_Walking = new Parameter("GatheredSessionData_Walking", endOfSessionReportWalkingQuests_Firebase);//add the "endOfSessionReportWalkingQuests_Firebase" under the "GatheredSessionData_Walking"
            //Parameter Param_Collecting = new Parameter("GatheredSessionData_Collecting", endOfSessionReportCollectingQuests_Firebase);//add the "endOfSessionReportCollectingQuests_Firebase" under the "GatheredSessionData_Collecting"


            //push a big final event which contains the end of session data
            FirebaseAnalytics.LogEvent("SessionData", ESR_TimeStarted, ESR_TimeFinished, ESR_TimeTotal, ESR_TotalDistanceTravelled, ESR_TotalCollected, ESR_W_TotalTimeSpentMinutes, ESR_W_TotalCompleted, ESR_W_TotalProgressedOn, ESR_C_TotalTimeSpentMinutes, ESR_C_TotalProgressedOn, ESR_C_TotalCompleted); //log a session data event taking in the above  parameters
            Debug.Log("FB - SessionData - too much to put here");
        }
            
    }
    public void OnApplicationPause(bool pause)
    {

        Debug.Log("OnApplicationPause - " + pause);
        /*
        if (pause) 
        {
            SaveToFile(7584);
        }
        */
        
    }
    public void OnApplicationFocus(bool focus)
    {
        Debug.Log("OnApplicationFocus - " + focus);
    }
    public void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
    }

    public void CloseApplication() 
    {

        Application.Quit();
    }

    public float RoundMinutes(float MinutesToRound) 
    {
        float output = 0f;
        output = Mathf.Round(MinutesToRound * 1000f) / 1000f;
        return output;
    }
    public IEnumerator TestLocalPHP()
    {

        WWWForm form = new WWWForm(); //create a new www form
        form.AddField("Filepath", dataJson.UserID); //add the desired foldername as the "Filepath" field
        form.AddField("Data", JsonUtility.ToJson(dataJson, true)); //convert the dataJson to a string and add it to the "Data" field
        //Debug.Log(FileName);
        form.AddField("FileName", FileName); //add the desired filename as the "FileName" field
        UnityWebRequest www = UnityWebRequest.Post(PHPLocation, form); //run the PHP script, taking in the form
        yield return www.SendWebRequest(); //send web request

        if (www.result == UnityWebRequest.Result.ConnectionError) //if the connection doesnt work
        {
            Debug.Log(www.error); //return the type of error
            www.Dispose();
            StartCoroutine(TestLocalPHP()); //try and resend 
        }
        else
        {
            Debug.Log("pushed"); //else, return a confirmation debug
        }
        www.Dispose(); //dispose of the www

    }

    

    #region Stuff for the UI Demo

    public void GenerateStartOfSession_UI() 
    {
        CompileBeginingOfSession(ParticpantID_IF.text, Location_IF.text);
        if (UsingDebugUI)
        {
            PreviewText.text = JsonUtility.ToJson(dataJson, true); //update the preview text
        }
        

    }

    public void GenerateUniqueQuestID_UI() 
    {
        UniqueQuestID_IF.text = ReturnUniqueQuestID().ToString();
    
    }
    public void UpdateHeaderData() 
    {
        dataJson.UserID = ParticpantID_IF.text;
        dataJson.SessionsID = SessionID_IF.text;
        dataJson.TimeStamp = Date_IF.text;
        //dataJson.Time = Time_IF.text;
        dataJson.Location = Location_IF.text;
        if (UsingDebugUI)
        {
            PreviewText.text = JsonUtility.ToJson(dataJson, true);
        }
        
    }

    public void SetDate() 
    {

        string CurrentDate = System.DateTime.Now.ToShortDateString(); //get the current date
        CurrentDate = CurrentDate.Replace("/", "-");
        Date_IF.text = CurrentDate;
        UpdateHeaderData();
    }

    public void SetTime()
    {

        string CurrentTime = System.DateTime.Now.ToLongTimeString(); //get the current date
        CurrentTime = CurrentTime.Replace("/", "-");
        Time_IF.text = CurrentTime;
        UpdateHeaderData();
    }

    public void SetUserStartTime()
    {

        string CurrentTime = System.DateTime.Now.ToLongTimeString(); //get the current date
        CurrentTime = CurrentTime.Replace("/", "-");
        TimeUserStarted_IF.text = CurrentTime;
    }

    public void SetUserEndTime()
    {

        string CurrentTime = System.DateTime.Now.ToLongTimeString(); //get the current date
        CurrentTime = CurrentTime.Replace("/", "-");
        TimeUserStopped_IF.text = CurrentTime;

    }

    public void ClearDataEntries() 
    {
        dataJson.DataEntries = new List<DataEntry>();
        dataJson.EndOfSessionReport = new EndOfSessionReport();
        if (UsingDebugUI)
        {
            PreviewText.text = JsonUtility.ToJson(dataJson, true);
        }
        


    }

    public void AddLine_UI() 
    {
        //DE.ActionCompleted = ActionCompleted_IF.text;
        //DE.UniqueQuestID = int.Parse(UniqueQuestID_IF.text);
        //int ParseResult_Int;
        //int.TryParse(TypeOfQuestID_IF.text, out ParseResult_Int);
        //DE.TypeOfQuest_ID = ParseResult_Int;
        //DE.RequiredAmount = float.Parse(RequiredAmount_IF.text);
        //DE.CurrentAmount = float.Parse(CurrentAmount_IF.text);
        //DE.TimeThisUserStarted = TimeUserStarted_IF.text;
        //DE.TimeWhenUserStopped = TimeUserStopped_IF.text;
        AddLine(ActionCompleted_IF.text, int.Parse(UniqueQuestID_IF.text), TypeOfQuestID_IF.text, float.Parse(RequiredAmount_IF.text), float.Parse(CurrentAmount_IF.text), QuestCompletedToggle.isOn);
    }

    public void SetSceneButtonsColours() 
    {
        if (UsingDebugUI)
        {
            switch (CurrentScene_Debug)
            {
                case ("VentureMode"):
                    CurrentSceneButtons[0].image.color = Color.green;
                    CurrentSceneButtons[1].image.color = Color.red;
                    break;

                case ("FarmMode"):
                    CurrentSceneButtons[1].image.color = Color.green;
                    CurrentSceneButtons[0].image.color = Color.red;
                    break;
            }
        }
    }

    public void SetToFarmMode() 
    {
        Firebase_ChangingGameMode(CurrentScene_Debug, "FarmMode");
    }

    public void SetToVentureMode() 
    {
        Firebase_ChangingGameMode(CurrentScene_Debug, "VentureMode");
    }

    public void QuestLapsed_UI(string WalkingCollecting) 
    {
        float MaxNumber_Temp = Random.Range(30, 50);
        float QuestProgress = Random.Range(1, MaxNumber_Temp - 5);
        Firebase_QuestLapsed(WalkingCollecting, MaxNumber_Temp, QuestProgress);
    }

    public void CropPlanted_UI() 
    {
        string ChosenWord = CropsTemp[Random.Range(0, CropsTemp.Count)];
        Firebase_CropPlanted(ChosenWord);
        Debug.Log(ChosenWord);
    }

    public void CropHarvested_UI()
    {
        string ChosenWord = CropsTemp[Random.Range(0, CropsTemp.Count)];
        Firebase_CropHarvested(ChosenWord);
        Debug.Log(ChosenWord);

    }

    public void FoodMade_UI()
    {
        string ChosenWord = FoodTemp[Random.Range(0, CropsTemp.Count)];
        Firebase_FoodMade(ChosenWord);
        Debug.Log(ChosenWord);

    }


    public void TriggerAllFirebaseEvents() 
    {
        Firebase_QuestAccepted("AllTrigger");
        Firebase_QuestCompleted("AllTrigger");
        float MaxNumber_Temp = Random.Range(30, 50);
        float QuestProgress = Random.Range(1, MaxNumber_Temp - 5);
        Firebase_QuestLapsed("AllTrigger", MaxNumber_Temp, QuestProgress);
        Firebase_CropPlanted("AllTrigger");
        Firebase_CropHarvested("AllTrigger");
        Firebase_FoodMade("AllTrigger");
        Firebase_LeavingGameMode("AllTrigger");
        Firebase_EnteringGameMode("AllTrigger");
        //Firebase_TutorialStarted();
        //Firebase_TutorialCompleted(Random.Range(2, 8));
        Firebase_TutorialStarted();
        Firebase_TutorialClosed();
        dataJson.EndOfSessionReport = CompileEndOfSessionReport(Random.Range(0, 100));
        Firebase_UploadEndOfSessionData();

    }

    #endregion

    //WalkingQuests ID = 5
    //CollectingQuests ID = 3

    /*
     * Farming:
    - Crop planted
    - Crop harvest
    - Food Made


    Oversight in calculating distance walked
    end of session report, add parameter for "DistanceWalked" - Done
    remove the list for the uniqueID
    unity debugs for quest accepting to make sure only new accepted quest events are pushed
     * 
     * 
     */

}
