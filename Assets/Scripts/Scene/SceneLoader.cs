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

    public void LoadSceneFromXml(string xmlPath) {
        //如果已经加载过了就直接打开原来加载过的场景
        GameObject newSceneObject;
        if (PathToSceneObject.ContainsKey(xmlPath))
        {
            newSceneObject = PathToSceneObject[xmlPath];
            ChangeSceneObject(newSceneObject);
            return;
        }
        newSceneObject = new GameObject(xmlPath);
        //将多个场景错开来
        newSceneObject.transform.position = Vector3.right * PathToSceneObject.Count * 10000;
        PathToSceneObject.Add(xmlPath, newSceneObject);

        TextAsset xmlFile = Resources.Load<TextAsset>(xmlPath); //TODO 目前通过Resources.Load读取，后面换成在库中读取
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
            if(path != "" && path != "null")
                //TODO 目前阶段直接通过Resources.Load读取，后面会换成读取网络路径
                elementObject = GameObject.Instantiate(Resources.Load<GameObject>(path)); 
            else
                elementObject = new GameObject();
                
            SceneElement sceneElement = elementObject.AddComponent<SceneElement>();
            sceneElement.LoadData(xmlPath + id, name, path);
            if(IdToElement.ContainsKey(xmlPath + parent_id))
                sceneElement.transform.SetParent(SceneLoader.instance.IdToElement[xmlPath + parent_id].transform);
            else
                sceneElement.transform.SetParent(newSceneObject.transform);
            sceneElement.SetInteactionType(interactionType, interactionContent);
            
            elementObject.tag = tag;
            elementObject.transform.localPosition = position;
            elementObject.transform.eulerAngles = rotation;
            elementObject.transform.localScale = scale;
        }

        ChangeSceneObject(newSceneObject);
    }

    public void SaveSceneAsXml(string xmlPath) {
        // 在Edit Mode下将场景保存为xml（Play Mode应该也行）
        // 方法比较土，就是提前把场景里的物体都挂上SceneElement脚本，然后填上path（id自动填充）
        // 然后获取属性生成xml

        // 创建XmlDocument并构建XML结构
        int currentId = 1;
        XmlDocument xml = new XmlDocument();
        XmlDeclaration xmldecl = xml.CreateXmlDeclaration("1.0", "UTF-8", "");
        XmlElement root = xml.CreateElement("Elements");

        // 遍历场景中的所有SceneElement组件
        foreach (var gameObject in GameObject.FindObjectsOfType<SceneElement>())
        {
            SceneElement element = gameObject.GetComponent<SceneElement>();
            XmlElement info = xml.CreateElement("element");

            // 自动递增id
            element.id = currentId++.ToString();
            info.SetAttribute("id", element.id.ToString());

            // 添加name
            XmlElement name = xml.CreateElement("name");
            name.InnerText = element.name;
            info.AppendChild(name);

            // 添加path
            XmlElement path = xml.CreateElement("path");
            path.InnerText = element.path;
            info.AppendChild(path);

            // 添加position
            XmlElement position = xml.CreateElement("position");
            position.SetAttribute("x", element.transform.localPosition.x.ToString());
            position.SetAttribute("y", element.transform.localPosition.y.ToString());
            position.SetAttribute("z", element.transform.localPosition.z.ToString());
            info.AppendChild(position);

            // 添加rotation
            XmlElement rotation = xml.CreateElement("rotation");
            rotation.SetAttribute("x", element.transform.eulerAngles.x.ToString());
            rotation.SetAttribute("y", element.transform.eulerAngles.y.ToString());
            rotation.SetAttribute("z", element.transform.eulerAngles.z.ToString());
            info.AppendChild(rotation);

            // 添加scale
            XmlElement scale = xml.CreateElement("scale");
            scale.SetAttribute("x", element.transform.localScale.x.ToString());
            scale.SetAttribute("y", element.transform.localScale.y.ToString());
            scale.SetAttribute("z", element.transform.localScale.z.ToString());
            info.AppendChild(scale);

            // 添加parent_id
            XmlElement parent_id = xml.CreateElement("parent_id");
            if (element.transform.parent != null && element.transform.parent.TryGetComponent(out SceneElement parentElement))
                parent_id.InnerText = parentElement.id.ToString();
            else
                parent_id.InnerText = "0";
            info.AppendChild(parent_id);

            // 添加tag
            XmlElement tag = xml.CreateElement("tag");
            tag.InnerText = element.gameObject.tag; // 获取GameObject的tag
            info.AppendChild(tag);

            // 添加interaction
            if (element.interactionScript != null)
            {
                XmlElement interaction = xml.CreateElement("interaction");
                interaction.SetAttribute("type", element.interactType);
                interaction.InnerText = element.interactionContent;
                info.AppendChild(interaction);
            }

            // 将该element添加到根节点
            root.AppendChild(info);
        }

        // 将根节点添加到文档并保存
        xml.AppendChild(root);
        xml.Save(xmlPath);

        Debug.Log("Scene saved as XML!");
    }
}
