using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string _dataDirPath;
    private string _fileName;

    public FileDataHandler(string dataDir, string fileName)
    {
        this._dataDirPath=dataDir;
        this._fileName=fileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(_dataDirPath, _fileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream fs = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        dataToLoad =reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
               // loadedData.Init();
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from " + fullPath + "\n" + e);
                throw;
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {

        string fullPath = Path.Combine(_dataDirPath, _fileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream fs = new FileStream(fullPath,FileMode.Create) )
            {
                using (StreamWriter writer=new StreamWriter(fs))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to write data to " + fullPath + "\n" +e);
            throw;
        }
    }
}
