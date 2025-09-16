
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    public static GameData _gameData;
    [SerializeField] PlayerUpgradesStats _stats;
    [SerializeField] private string _fileName;
    private FileDataHandler _dataHandler;
    private List<IDataPersistence> _dataPersistenceObjects;
    public static DataPersistenceManager Instance { get; private set; }
    [SerializeField] public bool DebugOnLoad = true;

    public void Awake()
    {
        _dataHandler = new FileDataHandler(Application.persistentDataPath, _fileName);
        this._dataPersistenceObjects = FindAllDataPersistenceObejcts();
        Instance = this;
        Instance.LoadGame();

        //if (Instance != null)
        //{

        //    //Data was initialized in another scene
        //    Instance.LoadGame();
        //    Destroy(this.gameObject);
        //}
        //else
        //{
        //    //DontDestroyOnLoad(this);
        //    Instance = this;
        //    LoadGame();

        //}

     
    }

    //void Start()
    //{_dataHandler = new FileDataHandler(Application.persistentDataPath, _fileName);
    //    this._dataPersistenceObjects = FindAllDataPersistenceObejcts();


    //    LoadGame();
    //}

    //void OnApplicationQuit()
    //{
    //    SaveGame();
    //}

    public void NewGame()
    {
        _gameData = new GameData(_stats);
       // _gameData.Init();
    }

    public void LoadGame()
    {
        //TODO deserialzie data
        _gameData = this._dataHandler.Load();
       
        // this._dataPersistenceObjects.Add(_gameData.PlayerProgressionTracker);

        if (_gameData==null)
        {
            Debug.Log("Initializing Default Values");
            NewGame();
        }
        //TODO find a way to initialize the player upgrades stats in another script
        _gameData.PlayerProgressionTracker.SetPlayerProgressionStats(_stats);
        foreach (var dataPersistenceObject in _dataPersistenceObjects)
        {
            dataPersistenceObject.LoadData(_gameData);
            if (dataPersistenceObject.OnLoaded != null)
            {
                dataPersistenceObject.OnLoaded.Invoke(_gameData);
            }
        }

        if (DebugOnLoad)
        {
            Debug.Log(_gameData);
        }

    }

    public void SaveGame()
    {
         if (_gameData != null)
        {
            foreach (var dataPersistenceObject in _dataPersistenceObjects)
            {
                dataPersistenceObject.SaveData(ref _gameData);
            }

            this._dataHandler.Save(_gameData);
        }

    }

    private List<IDataPersistence> FindAllDataPersistenceObejcts()
    {
        return FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>().ToList();
    }


}
