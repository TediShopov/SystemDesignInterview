using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Timers;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

//Works more of a frame conter as Unity already provides
//a fixed timestep.
public class FrameLimiter : MonoBehaviour
{
    public static FrameLimiter Instance { get; set; }

    //Configure unity fixed update
    public double FPSLimit;

    public bool UnpauseGameScheduled = true;
    public double unpauseGameAt = -1;

    public Int32 FramesInPlay;

    [Header("Debug")]
    public bool DebugOutputUpdateSteps = false;

    public bool DebugScaledTimeSinceStart = false;
    public float DebugScaleTimeSet = 1.0f;

    //Set when game was unpaused after syncing clocks
    private double timeFightingSimulationRunning = 0;

    public Stopwatch Stopwatch;
    private void Awake()
    {
        Instance = this;
        Stopwatch = new Stopwatch();
        Stopwatch.Start();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        //Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        double sToWait = (1.0 / FPSLimit);
        Time.fixedDeltaTime = (float)sToWait;
    }

    private void FixedUpdate()
    {
        if (ClientData.GameState == GameState.Finished)
        {
            var sceneLevel = SceneManager.GetActiveScene();
            if (SocketComunication.Instance != null)
            {
                //Socket communication on destroy will handle the
                // gracefull close of socketsReceiver
                SocketComunication.Instance.RestartSockets();
                DestroyImmediate(ChatSocketCommunication.Instance);
            }
            SceneManager.LoadScene(0);
        }

        if (ClientData.GameState == GameState.Runing)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);
            FramesInPlay++;
        }
        if (DebugOutputUpdateSteps)
            UnityEngine.Debug.Log("FixedUpdate time :" + Time.deltaTime);
        if (DebugScaledTimeSinceStart)
            UnityEngine.Debug.Log("Scaled time since start :" + GetScaledTimeSinceFightingSimulationStart());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClientData.IsDebugModeEnabled = !ClientData.IsDebugModeEnabled;
        }
        if (UnpauseGameScheduled && ClientData.GameState == GameState.NotYetStarted && this.unpauseGameAt > 0 && this.GetUncaledTicksSinceSceneStarted() >= this.unpauseGameAt)
        {
            SetRelativeTimeForFightingSimulation();
            ClientData.GameState = GameState.Runing;
            UnpauseGameScheduled = false;
            UnityEngine.Debug.Log($"Game unpaused due to timer");
        }
        if (DebugOutputUpdateSteps)
            UnityEngine.Debug.Log("FixedUpdate time :" + Time.deltaTime);
        Time.timeScale = DebugScaleTimeSet;
    }

    //Utilit Methods

    #region UtilityMethods

    public void SetRelativeTimeForFightingSimulation()
    {
        timeFightingSimulationRunning = Time.timeAsDouble;
    }
    public double GetScaledTimeSinceFightingSimulationStart()
    {
        return Time.timeAsDouble - timeFightingSimulationRunning;
    }

    public long GetUncaledTicksSinceSceneStarted()
    {
        return Stopwatch.ElapsedTicks;
    }
    public long TickToMilliseconds(double ticks)
    {
        double seconds = ticks / (double)Stopwatch.Frequency;
        return (long)((seconds) * 1000);
    }

    #endregion UtilityMethods
}