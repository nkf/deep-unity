using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BenchmarkSave {
    public enum Game {
        Pong, Grid, Slot
    }

    public const string TestFolder = "Benchmarks";

    //--------------Change these accordingly when doing new tests------------
    //Name of test
    public static string CurrentTestID;
    //Number of runs (learn -> test -> reset = 1 run)
    public static int Runs;
    //The Game
    public static Game CurrentGame = Game.Slot;
    //-----------------------------------------------------------------------

    public static bool SaveBenchmarks = false;
    public static int TestN = 1;
	private static string _modelPath = null;
    public static string ModelPath { get { return _modelPath != null && !_modelPath.Equals("") ? 
										   		  _modelPath : 
												  Path.Combine(TestFolder, Path.Combine(CurrentTestID, CurrentTestID+"-"+TestN)) + ".xml"; }
									 set { _modelPath = value; }}

    public static Dictionary<Game, string[]> Header = new Dictionary<Game, string[]> {
        {Game.Pong, new []{"Runtime", "Paddle Hits", "Victories", "Avg. miss distance"}},
        {Game.Grid, new []{"Runtime", "Accuracy", "Avg. Distance Score"}},
        {Game.Slot, new []{"Runtime", "LapTime", "Crashes", "Distance"}},
    };

    public static bool HaveRunsLeft {
        get { return TestN < Runs; }
    }

    private static StreamWriter GetSaveFile() {
        var filepath = Path.Combine(TestFolder, CurrentTestID + ".csv");
        Directory.CreateDirectory(TestFolder);
        if(File.Exists(filepath)) 
            return new StreamWriter(File.Open(filepath, FileMode.Append));
        var writer = new StreamWriter(File.Open(filepath, FileMode.CreateNew));
        //Write header
        writer.Write(string.Join(";", Header[CurrentGame]) + Environment.NewLine); 
        return writer;
    }

    public static void NextRun() {
        TestN++;
    }

    //Must be called first
    public static void WriteRunTime(double time) {
        if(!SaveBenchmarks) return;
        using (var writer = GetSaveFile()) {
            writer.Write("{0:F};", time);
        }
    }

    //Must be called second
    public static void WritePongResult(int hits, int victories, double avgMissDist) {
        if(!SaveBenchmarks) return;
        if(CurrentGame != Game.Pong) throw new Exception("Current Test game was set to <" + CurrentGame + "> but we are running Pong");
        using (var writer = GetSaveFile()) {
            var line = string.Format("{0};{1};{2:F3}", hits, victories, avgMissDist) + Environment.NewLine;
            writer.Write(line);
        }
    }

    //Must be called second
    public static void WriteGridResult(double accuracy, double distScore) {
        if(!SaveBenchmarks) return;
        if(CurrentGame != Game.Grid) throw new Exception("Current Test game was set to <" + CurrentGame + "> but we are running Grid");
        using(var writer = GetSaveFile()) {
            var line = string.Format("{0:F};{1:F}", accuracy, distScore) + Environment.NewLine;
            writer.Write(line);
        }
    }

    //Must be called second
    public static void WriteSlotResult(float time, int crashes, float distance) {
        if(!SaveBenchmarks) return;
        if(CurrentGame != Game.Slot) throw new Exception("Current Test game was set to <" + CurrentGame + "> but we are running Doll");
        using(var writer = GetSaveFile()) {
			var line = string.Format("{0:F};{1:D};{2:F}", time, crashes, distance) + Environment.NewLine;
            writer.Write(line);
        }
    }
}
