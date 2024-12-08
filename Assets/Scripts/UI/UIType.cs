public class UIType{
    private string path;
    public string Path {get => path;}

    private string name;
    public string Name {get => name;}

    public UIType(string ui_path, string ui_name){
        name = ui_name;
        path = ui_path;
    }
}
