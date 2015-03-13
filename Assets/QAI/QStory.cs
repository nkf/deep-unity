using System.IO;

public class QStory {
    public string ScenePath { get; set; }
    public string SceneName {
        get {
            return Path.GetFileNameWithoutExtension(ScenePath);
        }
    }
    public int Iterations { get; set; }


}
