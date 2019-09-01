using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IOManager : MonoBehaviour
{
    // This is the string that will be used as the file name where
    // the data is stored. Currently the date-time is used.
    public static string participantID;

    // This is the randomisation number (#_param2.txt that is to be used
    // for order of instances for this participant)
    public static string randomisationID;

    // Current time, used in output file names
    public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

    // Starting string of the output file names
    public static string Identifier;
    
    //Input and Outout Folders with respect to the Application.dataPath;
    public static string inputFolder = "/StreamingAssets/Input/";
    public static string inputFolderKPInstances = "/StreamingAssets/Input/KPInstances/";
    public static string outputFolder = "/StreamingAssets/Output/";

    // Complete folder path of inputs and ouputs
    public static string folderPathLoad = Application.dataPath + inputFolder;
    public static string folderPathLoadInstances = Application.dataPath + inputFolderKPInstances;
    public static string folderPathSave = Application.dataPath + outputFolder;

    public static Dictionary<string, string> dict;

    /*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt
	 * Structure of each file is the following:
	 * weights:[2,5,8,10,11,12]
	 * values:[10,8,3,9,1,4]
	 * capacity:15
	 * profit:16
	 *
	 * The instances are stored as kpinstances structures in the array of structures: kpinstances
	 */
    public static void LoadGame()
    {
        Identifier = "KP_" + participantID + "_" + randomisationID + "_" + dateID + "_";

        dict = LoadParameters();

        // Process time & randomisation parameters
        AssignVariables(dict);

        // Load and process all instance parameters
        LoadInstances(GameManager.numberOfInstances);

        // Create output file headers
        SaveHeaders();


    }

    // Reads all instances from .txt files.
    // The instances are stored as tspinstances structs in an array called "tspinstances"
    private static void LoadInstances(int numberOfInstances)
    {
        GameManager.kpinstances = new GameManager.KPInstance[GameManager.numberOfInstances];

        for (int k = 0; k < GameManager.numberOfInstances; k++)
        {
            var dict = new Dictionary<string, string>();

            // Open the text file using a stream reader.
            using (StreamReader sr = new StreamReader(folderPathLoadInstances + "i" + (k + 1) + ".txt"))
            {
                ReadToDict(sr, dict);
            }

            dict.TryGetValue("weights", out string weightsS);
            dict.TryGetValue("values", out string valuesS);
            dict.TryGetValue("capacity", out string capacityS);
            dict.TryGetValue("profit", out string profitS);
            dict.TryGetValue("solution", out string solutionS);

            GameManager.kpinstances[k].weights = 
                Array.ConvertAll(weightsS.Substring(1, weightsS.Length - 2).Split(','), int.Parse);
            GameManager.kpinstances[k].values = 
                Array.ConvertAll(valuesS.Substring(1, valuesS.Length - 2).Split(','), int.Parse);
            GameManager.kpinstances[k].capacity = int.Parse(capacityS);
            GameManager.kpinstances[k].profit = int.Parse(profitS);
            GameManager.kpinstances[k].solution = int.Parse(solutionS);

            dict.TryGetValue("problemID", out GameManager.kpinstances[k].id);
            dict.TryGetValue("instanceType", out GameManager.kpinstances[k].type);
        }
    }

    // Loads the parameters from the text files: param.txt
    private static Dictionary<string, string> LoadParameters()
    {
        // Store parameters in a dictionary
        var dict = new Dictionary<string, string>();
        using (StreamReader sr = new StreamReader(folderPathLoad + "layoutParam.txt"))
        {
            ReadToDict(sr, dict);
        }
        // Reading time_param.txt
        using (StreamReader sr1 = new StreamReader(folderPathLoad + "time_param.txt"))
        {
            ReadToDict(sr1, dict);
        }
        // Reading param2.txt within the Input folder
        using (StreamReader sr2 = new StreamReader(folderPathLoadInstances + randomisationID + "_param2.txt"))
        {
            ReadToDict(sr2, dict);
        }

        return dict;
    }

    // Store an input file "sr" in a dictionary "dict"
    private static void ReadToDict(StreamReader sr, Dictionary<string, string> dict)
    {
        string line;
        // (This loop reads every line until EOF or the first blank line.)
        while (!string.IsNullOrEmpty(line = sr.ReadLine()))
        {
            string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            // Add the key-value pair to the dictionary:
            if (!dict.ContainsKey(tmp[0]))
            {
                dict.Add(tmp[0], tmp[1]);
            }
        }
        sr.Close();
    }

    //Assigns the parameters in the dictionary to variables
    private static void AssignVariables(Dictionary<string, string> dictionary)
    {
        //Assigns Parameters
        dictionary.TryGetValue("timeRest1min", out string timeRest1minS);
        dictionary.TryGetValue("timeRest1max", out string timeRest1maxS);
        dictionary.TryGetValue("timeRest2", out string timeRest2S);
        dictionary.TryGetValue("timeQuestion", out string timeQuestionS);
        dictionary.TryGetValue("timeAnswer", out string timeAnswerS);
        dictionary.TryGetValue("timeCostShow", out string timeCostShowS);
        dictionary.TryGetValue("timeCostEnter", out string timeCostEnterS);
        dictionary.TryGetValue("timeReward", out string timeRewardS);

        dictionary.TryGetValue("decision", out string decisionS);
        dictionary.TryGetValue("cost", out string costS);
        dictionary.TryGetValue("cost_digits", out string cost_digitsS);
        dictionary.TryGetValue("reward", out string rewardS);
        dictionary.TryGetValue("reward_amount", out string reward_amountS);
        dictionary.TryGetValue("size", out string sizeS);
        dictionary.TryGetValue("numberOfTrials", out string numberOfTrialsS);
        dictionary.TryGetValue("numberOfBlocks", out string numberOfBlocksS);
        dictionary.TryGetValue("numberOfInstances", out string numberOfInstancesS);
        dictionary.TryGetValue("instanceRandomization", out string instanceRandomizationS);
        
        GameManager.timeRest1min = Convert.ToSingle(timeRest1minS);
        GameManager.timeRest1max = Convert.ToSingle(timeRest1maxS);
        GameManager.timeRest2 = Convert.ToSingle(timeRest2S);
        GameManager.timeQuestion = int.Parse(timeQuestionS);
        GameManager.timeAnswer = int.Parse(timeAnswerS);
        GameManager.timeCostShow = int.Parse(timeCostShowS);
        GameManager.timeCostEnter = int.Parse(timeCostEnterS); 
        GameManager.timeReward = int.Parse(timeRewardS);

        GameManager.decision = int.Parse(decisionS);
        GameManager.cost = int.Parse(costS);
        GameManager.RandNumDigits = int.Parse(cost_digitsS);
        GameManager.reward = int.Parse(rewardS);
        //Debug.Log(reward_amountS.Substring(1, reward_amountS.Length - 2));
        GameManager.reward_amount = Array.ConvertAll(reward_amountS.Substring(1,
            reward_amountS.Length - 2).Split(','), Double.Parse);
        GameManager.size = int.Parse(sizeS);
        GameManager.numberOfTrials = int.Parse(numberOfTrialsS);
        GameManager.numberOfBlocks = int.Parse(numberOfBlocksS);
        GameManager.numberOfInstances = int.Parse(numberOfInstancesS);
        
        int[] instanceRandomizationNo0 = 
            Array.ConvertAll(instanceRandomizationS.Substring(1, 
            instanceRandomizationS.Length - 2).Split(','), int.Parse);

        //Debug.Log(instanceRandomizationNo0.Length);
        GameManager.Randomization = new int[instanceRandomizationNo0.Length];

        for (int i = 0; i < instanceRandomizationNo0.Length; i++)
        {
            GameManager.Randomization[i] = instanceRandomizationNo0[i] - 1;
        }
        
        ////Assigns LayoutParameters
        dictionary.TryGetValue("columns", out string columnsS);
        dictionary.TryGetValue("rows", out string rowsS);
        
        BoardManager.columns = int.Parse(columnsS);
        BoardManager.rows = int.Parse(rowsS);
    }

    /// <summary>
    /// Saves the headers for both files (Trial Info and Time Stamps)
    /// In the trial file it saves:  1. The participant ID. 2. Instance details.
    /// In the TimeStamp file it saves: 1. The participant ID. 
    /// 2.The time onset of the stopwatch from which the time stamps are measured. 
    /// 3. the event types description.
    /// </summary>
    private static void SaveHeaders()
    {
        //Saves InstanceInfo
        //an array of string, a new variable called lines3
        string[] lines3 = new string[4 + GameManager.numberOfInstances];
        lines3[0] = "PartcipantID:" + participantID;
        lines3[1] = "RandID:" + randomisationID;
        lines3[2] = "InitialTimeStamp:" + GameManager.initialTimeStamp;
        lines3[3] = "instanceNumber;capacity;profit;weights;values;id;type;sol";
        int l = 4;
        int ksn = 1;
        foreach (GameManager.KPInstance ks in GameManager.kpinstances)
        {
            //With instance type and problem ID
            lines3[l] = ksn + ";" + ks.capacity + ";" + ks.profit + ";" + 
                string.Join(",", ks.weights.Select(p => p.ToString()).ToArray()) + 
                ";" + string.Join(",", ks.values.Select(p => p.ToString()).ToArray())
                + ";" + ks.id + ";" + ks.type + ";" + ks.solution;
            l++;
            ksn++;
        }
        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "InstancesInfo.txt", true))
        {
            foreach (string line in lines3)
                outputFile.WriteLine(line);
        }


        // Trial Info file headers
        string[] lines = new string[4];
        lines[0] = "PartcipantID:" + participantID;
        lines[1] = "RandID:" + randomisationID;
        lines[2] = "InitialTimeStamp:" + GameManager.initialTimeStamp;
        lines[3] = "block;trial;answer;correct;timeSpent;itemsSelected;" +
            "finalvalue;finalweight;ReverseButtons;instanceNumber;pay;" +
            "xyCoordinates;";

        if(GameManager.cost == 1)
        {
            lines[3] = lines[3] + "RandomNumber;" + "SubmittedNumber";
        }

        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }

        // Time Stamps file headers
        string[] lines1 = new string[4];
        lines1[0] = "PartcipantID:" + participantID;
        lines1[1] = "RandID:" + randomisationID;
        lines1[2] = "InitialTimeStamp:" + GameManager.initialTimeStamp;
        lines1[3] = "block;trial;eventType;elapsedTime";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "TimeStamps.txt", true))
        {
            foreach (string line in lines1)
                outputFile.WriteLine(line);
        }

        // Headers for Clicks file
        string[] lines2 = new string[4];
        lines2[0] = "PartcipantID:" + participantID;
        lines2[1] = "RandID:" + randomisationID;
        lines2[2] = "InitialTimeStamp:" + GameManager.initialTimeStamp;
        lines2[3] = "block;trial;itemnumber(100=Reset);Out(0)/In(1)/Reset(2)/Other;time";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + Identifier + "Clicks.txt", true))
        {
            WriteToFile(outputFile, lines2);
        }
    }

    // Saves the data of a trial to a .txt file with the participants ID as 
    // filename using StreamWriter.
    // If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
    // Each line in the File has the following structure: "trial;answer;timeSpent".
    // itemsSelected in the final solutions (irrespective if it was submitted); 
    // xycorrdinates; Error message if any.".
    public static void SaveTrialInfo(int answer, string itemsSelected, float timeSpent)
    {
        string xyCoordinates = BoardManager.GetItemCoordinates();

        // Get the instance n umber for this trial and add 1 because the 
        // instanceRandomization is linked to array numbering in C#, which starts at 0;
        int instanceNum = GameManager.Randomization[GameManager.TotalTrials - 1] + 1;

        int solutionQ = GameManager.kpinstances[instanceNum - 1].solution;

        //"block;trial;answer;correct;timeSpent;itemsSelected;" +
        //"finalvalue;finalweight;ReverseButtons;instanceNumber;pay;" +
        //";xyCoordinates";

        // Reverse buttons is 1 if no/yes; 0 if yes/no
        string dataTrialText = GameManager.block + ";" + GameManager.trial + ";" + answer + ";" + GameManager.performance 
             + ";"+ timeSpent + ";" + itemsSelected + ";" + GameManager.valueValue + ";" 
            + GameManager.weightValue + ";" + BoardManager.ReverseButtons + ";" + instanceNum + ";" + GameManager.pay + ";"
            + xyCoordinates;

        if (GameManager.cost == 1)
        {
            dataTrialText = dataTrialText + ";" + GameManager.RandNum + ";" + GameManager.SubmittedRandNum;
        }
        // This location can be used by unity to save a file if u open the 
        // game in any platform/computer: Application.persistentDataPath;

        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "TrialInfo.txt", true))
        {
            outputFile.WriteLine(dataTrialText);
        }

        //Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
    }

    /// <summary>
    /// Saves the time stamp for a particular event type to the "TimeStamps" File
    /// </summary>
    /// Event type: 1=ItemsWithQuestion;2=AnswerScreen;3=InterTrialScreen;
    /// 4=InterBlockScreen;5=EndScreen
    public static void SaveTimeStamp(string eventType)
    {
        string dataTrialText = GameManager.block + ";" + GameManager.trial + 
            ";" + eventType + ";" + GameManager.TimeStamp();
        
        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "TimeStamps.txt", true))
        {
            outputFile.WriteLine(dataTrialText);
        }
    }


    // Saves the time stamp of every click made on the items 
    // block ; trial ; clicklist (i.e. item number ; itemIn? 
    // (1: selecting; 0:deselecting; 2: reset)
    // time of the click with respect to the begining of the trial)
    public static void SaveClicks(List<BoardManager.Click> itemClicks)
    {
        string[] lines = new string[itemClicks.Count];
        int i = 0;

        foreach (BoardManager.Click click in itemClicks)
        {
            lines[i] = GameManager.block + ";" + GameManager.trial + 
                ";" + click.ItemNumber + ";" + click.State + ";" + click.time;
            i++;
        }

        using (StreamWriter outputFile = new StreamWriter(folderPathSave +
            Identifier + "Clicks.txt", true))
        {
            WriteToFile(outputFile, lines);
        }

    }

    // Helper function to write lines to an outputfile
    private static void WriteToFile(StreamWriter outputFile, string[] lines)
    {
        foreach (string line in lines)
        {
            outputFile.WriteLine(line);
        }

        outputFile.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
