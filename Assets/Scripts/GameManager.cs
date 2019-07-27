/* Unity 3D program that displays multiple interactive instances of the Knapsack Problem.
 * 
 * Optimal resolution 1024x768. Future users of this program should consider updating the code to suit higher resolution displays.
 * 
 * Input files are stored in ./StreamingAssets/Input
 * User responses and other data are stored in ./StreamingAssets/Output
 * 
 * Based on Knapsack and TSP code written by Pablo Franco
 * Modifications (July 2019) by Anthony Hsu include:
 * click "Start" button to begin; items clickable; deleted various unused assets and functions; added StreamingAssets folder.
 * 
 * Honours students should make further changes to suit their projects.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    //Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)
    public static GameManager gameManager = null;

    //The reference to the script managing the board (interface/canvas).
    public BoardManager boardScript;

    //Current Scene
    public static string escena;

    //Time spent so far on this scene
    public static float tiempo;

    //Some of the following parameters are a default to be used if they are not specified in the input files.
    //Otherwise they are rewritten (see loadParameters() )
    //Total time for these scene
    public static float totalTime;

    //Time spent at the instance
    public static float timeTaken;

    //Current trial initialization
    public static int trial = 0;

    //Current block initialization
    public static int block = 0;

    //Total trial (As if no blocks were used)
    public static int TotalTrials;

    private static bool showTimer;

    //Modifiable Variables:
    //Minimum and maximum for randomized interperiod Time
    public static float timeRest1min;
    public static float timeRest1max;

    //InterBlock rest time
    public static float timeRest2;

    //Time given for each trial (The total time the items are shown -With and without the question-)
    public static float timeQuestion;

    //Total number of trials in each block
    private static int numberOfTrials;

    //Total number of blocks
    private static int numberOfBlocks;

    //Number of instance file to be considered. From i1.txt to i_.txt..
    public static int numberOfInstances;

    //The order of the instances to be presented
    public static int[] instanceRandomization;

    //The order of the left/right No/Yes randomization
    public static int[] buttonRandomization;

    // Skip button in case user does not want a break
    public static Button skipButton;

    // A list of floats to record participant performance
    // Performance should always be equal to or greater than 1.
    // Due to the way it's calculated (participant answer/optimal solution), performance closer to 1 is better.
    public static List<double> perf = new List<double>();
    public static double performance;
    public static List<double> paylist = new List<double>();
    public static double pay;

    // Keep track of total payment
    // Default value is the show up fee
    public static double payAmount = 8.00;

    //This is the string that will be used as the file name where the data is stored. DeCurrently the date-time is used.
    public static string participantID;

    //This is the randomisation number (#_param2.txt that is to be used for oder of instances for this participant)
    public static string randomisationID;

    public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

    private static string identifierName;

    //Is the question shown on scene 1?
    //private static int questionOn;

    //Input and Outout Folders with respect to the Application.dataPath;
    private static string inputFolder = "/StreamingAssets/Input/";
    private static string inputFolderKPInstances = "/StreamingAssets/Input/KPInstances/";
    private static string outputFolder = "/StreamingAssets/Output/";

    // Stopwatch to calculate time of events.
    private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    // Time at which the stopwatch started. Time of each event is calculated according to this moment.
    private static string initialTimeStamp;

    // current value
    public static int valueValue;

    // current weight
    public static int weightValue;


    // binary variable to keep track of whether the submission was due to time out or user choice
    public static int timedOut;

    //A structure that contains the parameters of each instance
    public struct KPInstance
    {
        public int capacity;
        public int profit;

        public int[] weights;
        public int[] values;

        public string id;
        public string type;

        public int solution;
    }

    //An array of all the instances to be uploaded form .txt files.
    public static KPInstance[] kpinstances;

    // Use this for initialization
    void Awake()
    {

        //Makes the Game manager a Singleton
        if (gameManager == null)
        {
            gameManager = this;
        }
        else if (gameManager != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        //Initializes the game
        boardScript = gameManager.GetComponent<BoardManager>();

        InitGame();
        if (escena != "SetUp")
        {
            saveTimeStamp(escena);
        }

    }


    //Initializes the scene. One scene is setup, other is trial, other is Break....
    void InitGame()
    {
        /*
		Scene Order: escena
		0=setup
		1=trial game
		2= intertrial rest
		3= interblock rest
		4= end
		*/
        Scene scene = SceneManager.GetActiveScene();

        escena = scene.name;

        Debug.Log("Current Scene" + escena);

        if (escena == "SetUp")
        {
            //Only uploads parameters and instances once.
            block++;
            boardScript.setupInitialScreen();
        }

        else if (escena == "Trial")
        {
            trial++;
            TotalTrials = trial + (block - 1) * numberOfTrials;
            showTimer = true;
            boardScript.SetupScene();

            tiempo = timeQuestion;
            totalTime = timeQuestion;
        }

        else if (escena == "InterTrialRest")
        {
            showTimer = false;
            tiempo = Random.Range(timeRest1min, timeRest1max);
            totalTime = tiempo;
        }

        else if (escena == "InterBlockRest")
        {
            trial = 0;
            block++;
            showTimer = true;
            tiempo = timeRest2;
            totalTime = tiempo;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (escena != "SetUp")
        {
            startTimer();
        }
    }

    //Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
    //If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
    //Each line in the File has the following structure: "trial;answer;timeSpent".
    // itemsSelected in the final solutions (irrespective if it was submitted); xycorrdinates; Error message if any.".
    public static void save(int answer, float timeSpent, string error)
    {
        string xyCoordinates = BoardManager.getItemCoordinates();

        //Get the instance n umber for this trial and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
        int instanceNum = instanceRandomization[TotalTrials - 1] + 1;

        int solutionQ = kpinstances[instanceNum - 1].solution;
        int correct = (solutionQ == answer) ? 1 : 0;

        string dataTrialText = block + ";" + trial + ";" + answer + ";" + correct + ";" + timeSpent + ";" + instanceNum + ";" + xyCoordinates + ";"
            + error;

        string[] lines = { dataTrialText };
        string folderPathSave = Application.dataPath + outputFolder;

        //This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }

        //Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
    }

    /// <summary>
    /// Saves the time stamp for a particular event type to the "TimeStamps" File
    /// </summary>
    /// Event type: 1=ItemsWithQuestion;2=AnswerScreen;3=InterTrialScreen;4=InterBlockScreen;5=EndScreen
    public static void saveTimeStamp(string eventType)
    {

        string dataTrialText = block + ";" + trial + ";" + eventType + ";" + timeStamp();

        string[] lines = { dataTrialText };
        string folderPathSave = Application.dataPath + outputFolder;

        //This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }
    }

    /// <summary>
    /// Saves the headers for both files (Trial Info and Time Stamps)
    /// In the trial file it saves:  1. The participant ID. 2. Instance details.
    /// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
    /// </summary>
    private static void saveHeaders()
    {

        identifierName = participantID + "_" + dateID + "_" + "Dec" + "_";
        string folderPathSave = Application.dataPath + outputFolder;


        //Saves InstanceInfo
        string[] lines3 = new string[numberOfInstances + 2];
        lines3[0] = "PartcipantID:" + participantID;
        lines3[1] = "instanceNumber" + ";c" + ";p" + ";w" + ";v" + ";id" + ";type" + ";sol";
        int l = 2;
        int ksn = 1;
        foreach (KPInstance ks in kpinstances)
        {
            //With instance type and problem ID
            lines3[l] = ksn + ";" + ks.capacity + ";" + ks.profit + ";" + string.Join(",", ks.weights.Select(p => p.ToString()).ToArray()) + ";" + string.Join(",", ks.values.Select(p => p.ToString()).ToArray())
                + ";" + ks.id + ";" + ks.type + ";" + ks.solution;
            l++;
            ksn++;
        }
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "InstancesInfo.txt", true))
        {
            foreach (string line in lines3)
                outputFile.WriteLine(line);
        }


        // Trial Info file headers
        string[] lines = new string[2];
        lines[0] = "PartcipantID:" + participantID;
        lines[1] = "block;trial;answer;correct;timeSpent;instanceNumber;xyCoordinates;error";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }

        // Time Stamps file headers
        string[] lines1 = new string[3];
        lines1[0] = "PartcipantID:" + participantID;
        lines1[1] = "InitialTimeStamp:" + initialTimeStamp;
        lines1[2] = "block;trial;eventType;elapsedTime";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt", true))
        {
            foreach (string line in lines1)
                outputFile.WriteLine(line);
        }
    }

    // Saves the time stamp of every click made on the items 
    // block ; trial ; clicklist (i.e. item number ; itemIn? (1: selcting; 0:deselecting; 2: click invalid; 3: reset) ; time of the click with respect to the begining of the trial)
    public static void SaveClicks(List<BoardManager.Click> itemClicks)
    {
        string folderPathSave = Application.dataPath + outputFolder;

        string[] lines = new string[itemClicks.Count];
        int i = 0;
        foreach (BoardManager.Click click in itemClicks)
        {
            lines[i] = block + ";" + trial + ";" + click.ItemNumber + ";" + click.State + ";" + click.time;
            i++;
        }

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "Clicks.txt", true))
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
    public static void loadKPInstance()
    {
        string folderPathLoad = Application.dataPath + inputFolderKPInstances;
        kpinstances = new KPInstance[numberOfInstances];

        for (int k = 1; k <= numberOfInstances; k++)
        {

            var dict = new Dictionary<string, string>();

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(folderPathLoad + "i" + k + ".txt"))
                {

                    string line;
                    while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                    {
                        string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        // Add the key-value pair to the dictionary:
                        dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                    }
                    // Read the stream to a string, and write the string to the console.
                    //String line = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.Log("The file could not be read:");
                Debug.Log(e.Message);
            }

            string weightsS;
            string valuesS;
            string capacityS;
            string profitS;
            string solutionS;

            dict.TryGetValue("weights", out weightsS);
            dict.TryGetValue("values", out valuesS);
            dict.TryGetValue("capacity", out capacityS);
            dict.TryGetValue("profit", out profitS);
            dict.TryGetValue("solution", out solutionS);

            kpinstances[k - 1].weights = Array.ConvertAll(weightsS.Substring(1, weightsS.Length - 2).Split(','), int.Parse);

            kpinstances[k - 1].values = Array.ConvertAll(valuesS.Substring(1, valuesS.Length - 2).Split(','), int.Parse);

            kpinstances[k - 1].capacity = int.Parse(capacityS);

            kpinstances[k - 1].profit = int.Parse(profitS);

            kpinstances[k - 1].solution = int.Parse(solutionS);

            dict.TryGetValue("problemID", out kpinstances[k - 1].id);
            dict.TryGetValue("instanceType", out kpinstances[k - 1].type);

        }

    }

    //Loads the parameters form the text files: param.txt and layoutParam.txt
    private static void loadParameters()
    {
        //string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
        string folderPathLoad = Application.dataPath + inputFolder;
        string folderPathLoadInstances = Application.dataPath + inputFolderKPInstances;
        var dict = new Dictionary<string, string>();

        try
        {   // Open the text file using a stream reader.
            using (StreamReader sr = new StreamReader(folderPathLoad + "layoutParam.txt"))
            {

                // (This loop reads every line until EOF or the first blank line.)
                string line;
                while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }


            using (StreamReader sr1 = new StreamReader(folderPathLoad + "param.txt"))
            {

                // (This loop reads every line until EOF or the first blank line.)
                string line1;
                while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line1.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
        }


        try
        {
            using (StreamReader sr2 = new StreamReader(folderPathLoadInstances + randomisationID + "_param2.txt"))
            {

                // (This loop reads every line until EOF or the first blank line.)
                string line2;
                while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line2.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("The randomisation file could not be read. Perhaps it doesn't exist.");
            Debug.Log(e.Message);
            //EditorUtility.DisplayDialog ("The randomisation file could not be read.", e.Message,"Got it! I'll restart the game.");

        }

        assignVariables(dict);

    }

    //Assigns the parameters in the dictionary to variables
    private static void assignVariables(Dictionary<string, string> dictionary)
    {

        //Assigns Parameters
        string timeRest1minS;
        string timeRest1maxS;
        string timeRest2S;
        string timeQuestionS;
        string numberOfTrialsS;
        string numberOfBlocksS;
        string numberOfInstancesS;
        string instanceRandomizationS;

        dictionary.TryGetValue("timeRest1min", out timeRest1minS);
        dictionary.TryGetValue("timeRest1max", out timeRest1maxS);
        dictionary.TryGetValue("timeRest2", out timeRest2S);

        dictionary.TryGetValue("timeQuestion", out timeQuestionS);

        dictionary.TryGetValue("numberOfTrials", out numberOfTrialsS);

        dictionary.TryGetValue("numberOfBlocks", out numberOfBlocksS);

        dictionary.TryGetValue("numberOfInstances", out numberOfInstancesS);


        timeRest1min = Convert.ToSingle(timeRest1minS);
        timeRest1max = Convert.ToSingle(timeRest1maxS);
        timeRest2 = Convert.ToSingle(timeRest2S);
        timeQuestion = Int32.Parse(timeQuestionS);
        numberOfTrials = Int32.Parse(numberOfTrialsS);
        numberOfBlocks = Int32.Parse(numberOfBlocksS);
        numberOfInstances = Int32.Parse(numberOfInstancesS);

        dictionary.TryGetValue("instanceRandomization", out instanceRandomizationS);

        int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring(1, instanceRandomizationS.Length - 2).Split(','), int.Parse);
        instanceRandomization = new int[instanceRandomizationNo0.Length];

        for (int i = 0; i < instanceRandomizationNo0.Length; i++)
        {
            instanceRandomization[i] = instanceRandomizationNo0[i] - 1;
        }
        //		}


        ////Assigns LayoutParameters
        string randomPlacementTypeS;
        string columnsS;
        string rowsS;
        string totalAreaBillS;
        string totalAreaWeightS;

        dictionary.TryGetValue("randomPlacementType", out randomPlacementTypeS);

        dictionary.TryGetValue("columns", out columnsS);
        dictionary.TryGetValue("rows", out rowsS);
        dictionary.TryGetValue("totalAreaBill", out totalAreaBillS);
        dictionary.TryGetValue("totalAreaWeight", out totalAreaWeightS);

        BoardManager.randomPlacementType = Int32.Parse(randomPlacementTypeS);
        BoardManager.columns = Int32.Parse(columnsS);
        BoardManager.rows = Int32.Parse(rowsS);
        BoardManager.totalAreaBill = Int32.Parse(totalAreaBillS);
        BoardManager.totalAreaWeight = Int32.Parse(totalAreaWeightS);
    }
    
    //Takes care of changing the Scene to the next one (Except for when in the setup scene)
    public static void changeToNextScene(int answer, bool skipped)
    {
        if (escena == "SetUp")
        {
            loadParameters();
            loadKPInstance();
            saveHeaders();
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "Trial")
        {
            if (skipped == true)
            {
                timeTaken = timeQuestion - tiempo;
            }
            else
            {
                timeTaken = timeQuestion;
            }
            SceneManager.LoadScene("InterTrialRest");
        }
        else if (escena == "InterTrialRest")
        {
            save(answer, timeTaken, "");
            if (answer != 2)
            {
                saveTimeStamp("ParticipantAnswer");
            }
            changeToNextTrial();
        }
        else if (escena == "InterBlockRest")
        {
            SceneManager.LoadScene("Trial");
        }

    }


    //Redirects to the next scene depending if the trials or blocks are over.
    private static void changeToNextTrial()
    {
        //Checks if trials are over
        if (trial < numberOfTrials)
        {
            SceneManager.LoadScene("Trial");
        }
        else if (block < numberOfBlocks)
        {
            SceneManager.LoadScene("InterBlockRest");
        }
        else
        {
            SceneManager.LoadScene("End");
        }
    }


    /// <summary>
    /// In case of an error: Skip trial and go to next one.
    /// Example of error: Not enough space to place all items
    /// </summary>
    /// Receives as input a string with the errorDetails which is saved in the output file.
    public static void errorInScene(string errorDetails)
    {
        Debug.Log(errorDetails);
        
        int answer = 3;
        save(answer, timeQuestion, errorDetails);
        changeToNextTrial();
    }


    /// <summary>
    /// Starts the stopwatch. Time of each event is calculated according to this moment.
    /// Sets "initialTimeStamp" to the time at which the stopwatch started.
    /// </summary>
    public static void setTimeStamp()
    {
        initialTimeStamp = @System.DateTime.Now.ToString("HH-mm-ss-fff");
        stopWatch.Start();
        Debug.Log(initialTimeStamp);
    }

    /// <summary>
    /// Calculates time elapsed
    /// </summary>
    /// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
    private static string timeStamp()
    {
        long milliSec = stopWatch.ElapsedMilliseconds;
        string stamp = milliSec.ToString();
        return stamp;
    }

    //Updates the timer (including the graphical representation)
    //If time runs out in the trial or the break scene. It switches to the next scene.
    void startTimer()
    {
        tiempo -= Time.deltaTime;

        if (showTimer)
        {
            boardScript.updateTimer();
        }

        //When the time runs out:
        if (tiempo < 0)
        {
            changeToNextScene(2, false);
        }
    }
}
