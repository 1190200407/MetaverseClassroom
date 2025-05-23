using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;

public class SceneLoader : UnitySingleton<SceneLoader>
{   
    [Header("加载xml路径")]
    public string xmlPath = "smallroom";
    //public Dictionary<string, SceneElement> NameToElement {get; private set;} = new Dictionary<string, SceneElement>();
    public Dictionary<string, SceneElement> IdToElement {get; private set;} = new();
    public Dictionary<string, GameObject> PathToSceneObject {get; private set;} = new();
    private GameObject curActiveObject = null;
    public bool isLoading = false;
    public float loadingProgress = 0;

    private void OnEnable() 
    {
        EventHandler.Register<ReloadEvent>(OnReload);
    }
    private void OnDisable()
    {
        EventHandler.Unregister<ReloadEvent>(OnReload);
    }

    private void OnReload(ReloadEvent @event){
        IdToElement.Clear();
    }

    private void ChangeSceneObject(GameObject newSceneObject)
    {
        if (curActiveObject == newSceneObject) return;
        curActiveObject?.SetActive(false);
        newSceneObject.SetActive(true);
        curActiveObject = newSceneObject;
    }

    public void LoadSceneFromXml(string xmlPath, bool changeScene = true)
    {
        if (PathToSceneObject.ContainsKey(xmlPath))
        {
            GameObject newSceneObject = PathToSceneObject[xmlPath];
            if (changeScene) ChangeSceneObject(newSceneObject);
            return;
        }
        else
        {
            StartCoroutine(LoadSceneFromXmlAsync(xmlPath, changeScene));
        }
    }

    public IEnumerator LoadSceneFromXmlAsync(string xmlPath, bool changeScene = true) {
        isLoading = true;
        //如果已经加载过了就直接打开原来加载过的场景
        GameObject newSceneObject;
        newSceneObject = new GameObject(xmlPath);
        //将多个场景错开来
        newSceneObject.transform.position = Vector3.right * PathToSceneObject.Count * 10000;
        PathToSceneObject.Add(xmlPath, newSceneObject);

        TextAsset xmlFile = Resources.Load<TextAsset>("SceneXmls/" + xmlPath); //TODO 目前通过Resources.Load读取，后面换成在库中读取
        XmlDocument document = new XmlDocument(); 
        document.LoadXml(xmlFile.text);

        XmlNodeList nodeList = document.SelectSingleNode("Elements").ChildNodes;
        string path = "", name = "", tag, interactionType, interactionContent;
        Vector3 position = Vector3.zero;
        Vector3 rotation = Vector3.zero;
        Vector3 scale = Vector3.zero;
        float? xVector, yVector, zVector;
        int id, parent_id = 0; 

        for(int i = 0; i < nodeList.Count; i++) {
            XmlElement element = (XmlElement)nodeList[i];

            id = int.Parse(element.GetAttribute("id", element.NamespaceURI).Trim());
            tag = "Untagged";
            interactionType = "";
            interactionContent = "";

            foreach (XmlElement node in element.ChildNodes) {    //遍历所有Element节点，读取对应属性
                switch (node.Name)
                {
                    case "name":
                        name = node.InnerText.Trim();
                        break;
                    case "web_path":    
                        //TODO 目前阶段直接通过Resources.Load读取，后面会换成读取网络路径
                        path = node.InnerText.Trim();
                        break;
                    case "position":
                        xVector = float.Parse(node.GetAttribute("x", node.NamespaceURI).Trim());
                        yVector = float.Parse(node.GetAttribute("y", node.NamespaceURI).Trim());
                        zVector = float.Parse(node.GetAttribute("z", node.NamespaceURI).Trim());
                        position = new Vector3(xVector.GetValueOrDefault(), yVector.GetValueOrDefault(), zVector.GetValueOrDefault());
                        break;
                    case "rotation":
                        xVector = float.Parse(node.GetAttribute("x", node.NamespaceURI).Trim());
                        yVector = float.Parse(node.GetAttribute("y", node.NamespaceURI).Trim());
                        zVector = float.Parse(node.GetAttribute("z", node.NamespaceURI).Trim());
                        rotation = new Vector3(xVector.GetValueOrDefault(), yVector.GetValueOrDefault(), zVector.GetValueOrDefault());
                        break;
                    case "scale":
                        xVector = float.Parse(node.GetAttribute("x", node.NamespaceURI).Trim());
                        yVector = float.Parse(node.GetAttribute("y", node.NamespaceURI).Trim());
                        zVector = float.Parse(node.GetAttribute("z", node.NamespaceURI).Trim());
                        scale = new Vector3(xVector.GetValueOrDefault(), yVector.GetValueOrDefault(), zVector.GetValueOrDefault());
                        break;
                    case "parent_id":
                        parent_id = int.Parse(node.InnerText.Trim());
                        break;
                    case "tag":
                        tag = node.InnerText.Trim();
                        break;
                    case "interaction":
                        // interactionType表示交互组件的类名，interactionContent表示交互相关的内容，根据交互类型不同，内容的格式和意义也不同
                        // 例如Browse交互会使玩家在指向该物体的时候，出现说明框，框内的文字即Content的内容
                        // 在View交互中，Content为一段路径，指向授课PPT所在的位置并读取PPT内容
                        interactionType = node.GetAttribute("type", node.NamespaceURI).Trim();
                        interactionContent = node.InnerText.Trim();
                        break;
                }
            }

            GameObject elementObject = null;
            
            // 创建物体
            if(path != "" && path != "null") {
                //TODO 目前阶段直接通过Resources.Load读取，后面会换成读取网络路径
                ResourceRequest request = Resources.LoadAsync<GameObject>(path);
                yield return request;
                elementObject = GameObject.Instantiate(request.asset as GameObject);
            } else {
                elementObject = new GameObject();
            }
                
            SceneElement sceneElement = elementObject.AddComponent<SceneElement>();
            sceneElement.LoadData(xmlPath + id, name, path);
            if(IdToElement.ContainsKey(xmlPath + parent_id))
                sceneElement.transform.SetParent(SceneLoader.instance.IdToElement[xmlPath + parent_id].transform);
            else
                sceneElement.transform.SetParent(newSceneObject.transform);
                
            elementObject.tag = tag;
            elementObject.transform.localPosition = position;
            elementObject.transform.eulerAngles = rotation;
            elementObject.transform.localScale = scale;
            sceneElement.SetInteactionType(interactionType, interactionContent);

            // 每加载10个物体让出一帧，避免卡顿
            if (i % 10 == 0) {
                yield return null;
            }

            loadingProgress = (float)i / nodeList.Count;
        }

        if (changeScene) ChangeSceneObject(newSceneObject);
        else
        {
            newSceneObject.SetActive(false);
        }
        isLoading = false;
    }
}
