using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/* This class is used to store data in text files */
public class DataManager
{
/// <summary>
    /// Saves all information regarding the trial in the participant's file in a dedicated row 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="navConfig"></param>
    /// <param name="data"></param>
    public static void WriteDataSummary(string fileName, NavConfig navConfig, float simTiming, HololensTracker hololensTracker, FeetTracker feetTracker)
    {
        string participantName = navConfig.ParticipantName;
        string dataPath = "Assets/Data";
        CreateFolderIfNecessary(dataPath, participantName);
        string participantPath = dataPath + "/" + participantName;

        StreamWriter sw = new StreamWriter(participantPath + "/" + fileName + ".csv", true);
        sw.Write("ID path;Simulation time (s);Distance travelled (m);Mean speed (m/s);Number of steps;Mean step frequency (step/s);Percent of left foot in front;Walked path;Number of areas covered;Average time in area (s);Nbr errors; Nbr wrong areas;Total error time (s);Total error distance (m)");

        sw.Write(navConfig.Path.Name + ";" 
                + simTiming.ToString() + ";" 
                + hololensTracker.DistanceTravelled().ToString() + ";" 
                + hololensTracker.MeanSpeed().ToString() + ";" 
                + feetTracker.StepCount.ToString() + ";"
                + feetTracker.MeanStepFrequency().ToString() + ";"
                + feetTracker.LeftFootInFrontRate().ToString() + ";"
                + hololensTracker.walkedPath.Count().ToString() + ";"
                + hololensTracker.AverageTimeInArea().ToString() + ";"
                + hololensTracker.NumberOfErrors().ToString() + ";"
                + hololensTracker.TotalWrongAreas().ToString() + ";"
                + hololensTracker.TotalErrorTime().ToString() + ";"
                + hololensTracker.TotalErrorDistance().ToString()
            + "\r\n");
        sw.Flush();
        sw.Close();
    }
	
    public static void WriteDataInFile(string fileName, NavConfig navConfig, string data)
    {
        string participantName = navConfig.ParticipantName;
        string dataPath = "Assets/Data";
        CreateFolderIfNecessary(dataPath, participantName);

        string trial = "Path" + navConfig.Path.Name + " (" + navConfig.Advice + ")";
        string participantPath = dataPath + "/" + participantName;
        CreateFolderIfNecessary(participantPath, trial);

        string path =  participantPath + "/" + trial + "/" + fileName + ".txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, fileName + Environment.NewLine);
        }
        File.AppendAllText(path, data + Environment.NewLine);
    }

    private static void CreateFolderIfNecessary(string parentFolder, string newFolderName)
    {
        if (!AssetDatabase.IsValidFolder(parentFolder + "/" + newFolderName))
        {
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }
}
