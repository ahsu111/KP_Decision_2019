/* Unity 3D program that displays multiple interactive instances of the Knapsack Problem.
 * 
 * Optimal resolution 1920x1080
 * 
 * Input files are stored in ./StreamingAssets/Input
 * User responses and other data are stored in ./StreamingAssets/Output
 * 
 * Based on Knapsack and TSP code written by Pablo Franco
 * Modifications (July 2019) by Anthony Hsu include:
 * click "Start" button to begin; items clickable; deleted various
 * unused assets and functions; added StreamingAssets folder.
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
    // Stopwatch to calculate time of events.
    public static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    // Time at which the stopwatch started. Time of each event is 
    // calculated according to this moment.
    public static string initialTimeStamp;

    // Game Manager: It is a singleton (i.e. it is always one and the 
    // same it is nor destroyed nor duplicated)
    public static GameManager gameManager = null;

    // The reference to the script managing the board (interface/canvas).
    public BoardManager boardScript;

    // Current Scene
    public static string escena;

    // Time spent so far on this scene
    public static float tiempo;

    // Some of the following parameters are a default to be used if 
    // they are not specified in the input files.
    // Otherwise they are rewritten (see loadParameters() )
    // Total time for these scene
    public static float totalTime;

    // Time spent at the instance
    public static float timeTaken;

    // Current trial initialization
    public static int trial = 0;

    // The current trial number across all blocks
    public static int TotalTrials = 0;

    // Current block initialization
    public static int block = 0;
    
    private static bool showTimer;

    // Modifiable Variables:
    // Minimum and maximum for randomized interperiod Time
    public static float timeRest1min;
    public static float timeRest1max;

    // InterBlock rest time
    public static float timeRest2;

    // Time given for each trial (The total time the items are shown -With and without the question-)
    public static float timeQuestion;
    public static float timeAnswer;

    // If participant submitted early, must wait X seconds before they are able to enter the memorised number
    public static float WAIT_TIME;
    // To record if the participant had to wait before submitting answer, 1 if yes, 0 otherwise
    public static int Waited;

    public static float timeCostShow;
    public static float timeCostEnter;
    public static float timeReward;

    // IMPORTANT: DECISION or OPTIMISATION KP
    // Game skips answer screen if optimisation is chosen.
    // If Decision, set decision = 1 in x_param2.txt. 
    public static int decision;

    // Variant of the KP; reward, cost or size
    public static int reward;
    public static int cost;
    public static int size;

    // Cost KP: This is the number one should memorise
    public static int RandNum;

    // How many digits do you want the random number to have? It goes from 1000 to 9999.
    public static int RandNumDigits;

    // This is what the user submitted
    public static int SubmittedRandNum;

    //The order of the instances to be presented
    public static double[] reward_amount;

    // Total number of trials in each block
    public static int numberOfTrials;

    // Total number of blocks
    public static int numberOfBlocks;

    //Number of instance file to be considered. From i1.txt to i_.txt..
    public static int numberOfInstances;
    
    // The order of the Instances to be presented
    public static int[] Randomization;

    //The order of the left/right No/Yes randomization
    public static int[] buttonRandomization;

    // To record answer in the decision KP
    // 0 if NO
    // 1 if YES
    // 2 if not selected
    // 100 if not applicable. i.e. optimisation KP.
    public static int answer;

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

    // current value
    public static int valueValue;

    // current weight
    public static int weightValue;

    // A structure that contains the parameters of each instance
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

    // An array of all the instances to be uploaded form .txt files.
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
            IOManager.SaveTimeStamp(escena);
        }
    }


    //Initializes the scene. One scene is setup, other is trial, other is Break....
    void InitGame()
    {
        /*
		Scene Order: escena
		0=setup
        (OPTIONAL 0.5: Reward scene)
        (OPTIONAL 0.5: Cost scene with number)
		1= trial game
        2= trial answer
		3= intertrial rest
        (OPTIONAL 3.5: Cost scene enter memorised number)
		4= interblock rest
		5= end
        6= payment
		*/
        Scene scene = SceneManager.GetActiveScene();

        escena = scene.name;

        //Debug.Log("Current Scene: " + escena);

        if (escena == "SetUp")
        {
            //Only uploads parameters and instances once.
            boardScript.SetupInitialScreen();
        }
        else if (escena == "RewardScene")
        {
            showTimer = false;

            //Debug.Log(reward_amount[0] + "   " + reward_amount[1] + "   " + reward_amount[2] + "   " + reward_amount[3] + "   " + reward_amount[4]);

            GameObject.Find("Text").GetComponent<Text>().text = "You are currently playing for $" + reward_amount[TotalTrials];


            tiempo = timeReward;
            totalTime = timeReward;
            //skipButton = GameObject.Find("Skip").GetComponent<Button>();
            //skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "ShowNumber")
        {
            showTimer = false;

            RandNum = Random.Range((int)Math.Pow(10, RandNumDigits - 1), (int)Math.Pow(10, RandNumDigits) - 1);

            GameObject.Find("Number").GetComponent<Text>().text = "" + RandNum;


            tiempo = timeCostShow;
            totalTime = timeCostShow;
            //skipButton = GameObject.Find("Skip").GetComponent<Button>();
            //skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "EnterNumber")
        {
            showTimer = false;

            //Participant Input
            BoardManager.EnterNum = GameObject.Find("UserNum").GetComponent<InputField>();

            InputField.SubmitEvent se3 = new InputField.SubmitEvent();
            se3.AddListener((value) => SubmitRandNum(value));
            BoardManager.EnterNum.onEndEdit = se3;

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);

            tiempo = timeCostEnter;
            totalTime = timeCostEnter;
        }
        else if (escena == "Trial")
        {
            trial++;
            TotalTrials = trial + (block - 1) * numberOfTrials;
            showTimer = true;
            WAIT_TIME = 0;
            Waited = 0;
            boardScript.SetupTrial();

            tiempo = timeQuestion;
            totalTime = timeQuestion;
        }
        else if (escena == "TrialAnswer")
        {
            showTimer = true;
            answer = 2;

            BoardManager.RandomizeButtons();

            tiempo = timeAnswer;
            totalTime = timeAnswer;
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
            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "End")
        {
            showTimer = false;

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "Payment")
        {
            showTimer = false;

            Text perf = GameObject.Find("PerfText").GetComponent<Text>();
            perf.text = DisplayPerf();

            Text pay = GameObject.Find("PayText").GetComponent<Text>();
            pay.text = "Total Payment: $" + Math.Ceiling(payAmount).ToString();
        }

    }

    public static void SubmitRandNum(string memorised_num)
    {
        try
        {
            int.TryParse(memorised_num, out SubmittedRandNum);
        }
        catch
        {
            SubmittedRandNum = -1;
        }

        Debug.Log("The random number was: " + RandNum + ", user submitted: " + SubmittedRandNum);
        
        ChangeToNextScene(BoardManager.itemClicks, true);
    }

    // Function to display user performance (last scene)
    public static string DisplayPerf()
    {
        string perfText = "Performance: ";

        for (int i = 0; i < numberOfTrials * numberOfBlocks; i++)
        {
            // Payment calculation
            perfText += " $" + paylist[i] + ";";
        }
        return perfText;
    }

    // Update is called once per frame
    void Update()
    {
        if (escena != "SetUp" &&  escena != "End" && 
            escena != "Payment")
        {
            StartTimer();
        }
    }

    //Takes care of changing the Scene to the next one (Except for when in the setup scene)
    public static void ChangeToNextScene(List<BoardManager.Click> itemClicks, bool skipped)
    {
        /*
		Scene Order: escena
		0=setup
        (OPTIONAL 0.5: Reward scene)
        (OPTIONAL 0.5: Cost scene with number)
		1= trial game
        2= trial answer
		3= intertrial rest
        (OPTIONAL 3.5: Cost scene enter memorised number)
		4= interblock rest
		5= end
        6= payment
		*/
        if (escena == "SetUp")
        {
            block++;
            IOManager.LoadGame();
            if (reward == 1)
            {
                SceneManager.LoadScene("RewardScene");
            }
            else if (cost == 1)
            {
                SceneManager.LoadScene("ShowNumber");
            }
            else
            {
                SceneManager.LoadScene("Trial");
            }
        }
        else if (escena == "RewardScene")
        {
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "ShowNumber")
        {
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "EnterNumber")
        {
            SceneManager.LoadScene("InterTrialRest");
        }
        else if (escena == "Trial")
        {
            if (skipped)
            {
                timeTaken = timeQuestion - tiempo;
                WAIT_TIME = WAIT_TIME + tiempo;
            }
            else
            {
                timeTaken = timeQuestion;
            }

            // Load next scene
            if (decision == 1)
            {
                IOManager.SaveTimeStamp("AnswerScreen");
                SceneManager.LoadScene("TrialAnswer");
            }
            else if (decision == 0)
            {
                SceneManager.LoadScene("InterTrialRest");
            }
        }
        else if (escena == "TrialAnswer")
        {
            if (Waited == 0)
            {
                WAIT_TIME = WAIT_TIME + tiempo;
                
                if (answer != 2)
                {
                    IOManager.SaveTimeStamp("ParticipantAnswer");
                }
            }

            if (cost == 1)
            {
                Debug.Log(WAIT_TIME + "    " + escena);
                if (WAIT_TIME > 0)
                {
                    if(Waited == 0)
                    {
                        tiempo = WAIT_TIME;
                        totalTime = WAIT_TIME;
                        Waited = 1;
                        GameObject.Find("Right").SetActive(false);
                        GameObject.Find("Left").SetActive(false);
                        GameObject.Find("Early").GetComponent<Text>().text = "Well done! You finished early, please wait " + WAIT_TIME + " seconds.";
                    }
                    showTimer = true;

                    if (tiempo < 0)
                    {
                        SceneManager.LoadScene("EnterNumber");
                    }
                }
                else
                {
                    SceneManager.LoadScene("EnterNumber");
                }
            }
            else
            {
                SceneManager.LoadScene("InterTrialRest");
            }
        }
        else if (escena == "InterTrialRest")
        {
            // Save participant answer
            // Calc Perf
            performance = 0;

            if (kpinstances[BoardManager.currInstance].solution == answer && 
                (cost != 1 || RandNum == SubmittedRandNum))
            {
                performance = 1;
            }

            perf.Add(performance);

            pay = reward_amount[TotalTrials - 1] * performance;

            paylist.Add(pay);

            payAmount += pay;
            Debug.Log("current pay: $" + payAmount);

            IOManager.SaveTrialInfo(answer, ExtractItemsSelected(itemClicks), timeTaken);
            IOManager.SaveClicks(itemClicks);

            ChangeToNextTrial();
        }
        else if (escena == "InterBlockRest")
        {
            if (reward == 1)
            {
                SceneManager.LoadScene("RewardScene");
            }
            else if (cost == 1)
            {
                SceneManager.LoadScene("ShowNumber");
            }
            else
            {
                SceneManager.LoadScene("Trial");
            }
        }
        else if (escena == "End")
        {
            SceneManager.LoadScene("Payment");
        }
    }

    //Redirects to the next scene depending if the trials or blocks are over.
    private static void ChangeToNextTrial()
    {
        //Debug.Log(trial + "   "+ numberOfTrials);
        //Checks if trials are over
        if (trial < numberOfTrials)
        {
            if (reward == 1)
            {
                SceneManager.LoadScene("RewardScene");
            }
            else if (cost == 1)
            {
                SceneManager.LoadScene("ShowNumber");
            }
            else
            {
                SceneManager.LoadScene("Trial");
            }
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

    // Extracts the items that were finally selected based on the sequence of clicks.
    private static string ExtractItemsSelected(List<BoardManager.Click> itemClicks)
    {
        List<int> itemsIn = new List<int>();
        foreach (BoardManager.Click click in itemClicks)
        {
            if (click.State == 1)
            {
                itemsIn.Add(Convert.ToInt32(click.ItemNumber));
            }
            else if (click.State == 0)
            {
                itemsIn.Remove(Convert.ToInt32(click.ItemNumber));
            }
            else if (click.State == 2)
            {
                itemsIn.Clear();
            }
        }

        string itemsInS = string.Empty;
        foreach (int i in itemsIn)
        {
            itemsInS = itemsInS + i + ",";
        }

        if (itemsInS.Length > 0)
        {
            itemsInS = itemsInS.Remove(itemsInS.Length - 1);
        }

        return itemsInS;
    }

    // Starts the stopwatch. Time of each event is calculated according to this moment.
    // Sets "initialTimeStamp" to the time at which the stopwatch started.
    public static void SetTimeStamp()
    {
        initialTimeStamp = @System.DateTime.Now.ToString("HH-mm-ss-fff");
        stopWatch.Start();
    }

    // Calculates time elapsed
    public static string TimeStamp()
    {
        long milliSec = stopWatch.ElapsedMilliseconds;
        return (milliSec / 1000f).ToString();
    }

    // Updates the timer (including the graphical representation)
    // If time runs out in the trial or the break scene. It switches to the next scene.
    void StartTimer()
    {
        tiempo -= Time.deltaTime;

        if (showTimer)
        {
            boardScript.UpdateTimer();
        }

        // When the time runs out:
        if (tiempo < 0)
        {
            if (escena == "EnterNumber")
            {
                try
                {
                    int.TryParse(GameObject.Find("UserNum").GetComponent<Text>().text, out SubmittedRandNum);
                }
                catch
                {
                    SubmittedRandNum = -1;
                }

                Debug.Log("The random number was: " + RandNum + ", user submitted: " + SubmittedRandNum);
            }
            ChangeToNextScene(BoardManager.itemClicks, false);
        }
    }

    // Change to next scene if the user clicks skip
    static void SkipClicked()
    {
        if (escena == "EnterNumber")
        {
            try
            {
                int.TryParse(GameObject.Find("UserNum").GetComponent<Text>().text, out SubmittedRandNum);
            }
            catch 
            {
                SubmittedRandNum = -1;
            }

            Debug.Log("The random number was: " + RandNum + ", user submitted: " + SubmittedRandNum);
        }

        //Debug.Log("Skip Clicked");
        ChangeToNextScene(BoardManager.itemClicks, true);
    }
}
