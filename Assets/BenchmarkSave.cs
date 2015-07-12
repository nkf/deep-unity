using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BenchmarkSave {
    public enum Game {
        Pong, Grid, Doll
    }

    public const string TestFolder = "Benchmarks";

    //--------------Change these accordingly when doing new tests------------
    //Name of test
    public const string CurrentTestID = "test";
    //Number of runs (learn -> test -> reset = 1 run)
    public const int Runs = 1;
    //The Game
    public const Game CurrentGame = Game.Pong;
    //-----------------------------------------------------------------------

    public static bool SaveBenchmarks = true;
    public static int TestN = 1;
    public static string ModelPath { get { return Path.Combine(TestFolder, CurrentTestID+"-"+TestN) + ".xml"; } }

    public static Dictionary<Game, string[]> Header = new Dictionary<Game, string[]> {
        {Game.Pong, new []{"Runtime", "Paddle Hits", "Victories", "Avg. miss distance"}},
        {Game.Grid, new []{"Runtime", "!!!!!!!!!!!!!!!!!!   TODO   !!!!!!!!!!!!!!!!!!"}},
        {Game.Doll, new []{"Runtime", "!!!!!!!!!!!!!!!!!!   TODO   !!!!!!!!!!!!!!!!!!"}},
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
        EditorApplication.Beep();
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
    public static void WriteGridResult(int jewsdidnineeleven) {
        if(!SaveBenchmarks) return;
        if(CurrentGame != Game.Grid) throw new Exception("Current Test game was set to <" + CurrentGame + "> but we are running Grid");
        throw new NotImplementedException();
        using(var writer = GetSaveFile()) {
            var line = string.Format("") + Environment.NewLine;
            writer.Write(line);
        }
    }

    //Must be called second
    public static void WriteDollResult(int jewsdidnineeleven) {
        if(!SaveBenchmarks) return;
        if(CurrentGame != Game.Doll) throw new Exception("Current Test game was set to <" + CurrentGame + "> but we are running Doll");
        throw new NotImplementedException();
        using(var writer = GetSaveFile()) {
            var line = string.Format("") + Environment.NewLine;
            writer.Write(line);
        }
    }
}
