using System.Collections.Generic;
using System.IO;

public class QStory {
    public string ScenePath { get; set; }
    public string SceneName {
        get {
            return Path.GetFileNameWithoutExtension(ScenePath);
        }
    }
    public int Iterations { get; set; }
    public List<QExperience> ImitationExperiences;
}
