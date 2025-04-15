using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SceneSaver : EditorWindow
{
    private Dictionary<GameObject, int> gameObjectToId = new();
    // 创建一个字符串变量来保存输入的 XML 路径
    private string xmlPath = "Assets/Resources/SceneXmls/Cafe.xml";  // 默认路径

    [MenuItem("Tools/Save Scene As XML")]
    public static void ShowWindow()
    {
        // 打开窗口
        GetWindow<SceneSaver>("Save Scene As XML");
    }

    // 绘制窗口 UI
    private void OnGUI()
    {
        // 绘制一个标题
        GUILayout.Label("Save Scene As XML", EditorStyles.boldLabel);

        // 绘制一个输入框，用于输入文件路径
        xmlPath = EditorGUILayout.TextField("XML Path", xmlPath);

        // 添加一个按钮，当点击时调用 SaveSceneAsXml 方法
        if (GUILayout.Button("Save Scene"))
        {
            SaveSceneAsXml(xmlPath);
        }
    }

    public void SaveSceneAsXml(string xmlPath)
    {
        // 在 Edit Mode 下将场景保存为 XML（Play Mode 也应该可以）
        // 遍历场景中的所有 GameObject，并获取其路径信息

        // 创建 XmlDocument 并构建 XML 结构
        int currentId = 1;
        gameObjectToId.Clear();
        XmlDocument xml = new();
        XmlDeclaration xmldecl = xml.CreateXmlDeclaration("1.0", "UTF-8", "");
        xml.AppendChild(xmldecl);

        XmlElement root = xml.CreateElement("Elements");

        // 获取场景中的所有根 GameObject
        var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // 遍历场景中的所有 GameObject，按层级顺序深度优先遍历
        foreach (var rootGameObject in rootGameObjects)
        {
            TraverseAndSave(rootGameObject, ref currentId, xml, root);
        }

        // 将根节点添加到文档并保存
        xml.AppendChild(root);
        xml.Save(xmlPath);

        Debug.Log("Scene saved as XML!");
    }

    private void TraverseAndSave(GameObject gameObject, ref int currentId, XmlDocument xml, XmlElement root)
    {
        // 先检查是否为 Prefab 实例，如果是，跳过其子对象
        string prefabPath = "null";
        if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
        {
            prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject); // 获取 Prefab 路径
            prefabPath = prefabPath.Replace("Assets/Resources/", "");
            prefabPath = Regex.Replace(prefabPath, @"\.[^/\\]+$", "");
        }

        // 创建并填充 element 节点
        XmlElement info = xml.CreateElement("element");

        // 自动递增 id
        gameObjectToId.Add(gameObject, currentId);
        info.SetAttribute("id", currentId++.ToString());

        // 添加 name
        XmlElement name = xml.CreateElement("name");
        name.InnerText = gameObject.name;
        info.AppendChild(name);

        // 获取 prefab 路径
        XmlElement path = xml.CreateElement("web_path");
        path.InnerText = prefabPath;
        info.AppendChild(path);

        // 添加 position
        XmlElement position = xml.CreateElement("position");
        position.SetAttribute("x", gameObject.transform.localPosition.x.ToString());
        position.SetAttribute("y", gameObject.transform.localPosition.y.ToString());
        position.SetAttribute("z", gameObject.transform.localPosition.z.ToString());
        info.AppendChild(position);

        // 添加 rotation
        XmlElement rotation = xml.CreateElement("rotation");
        rotation.SetAttribute("x", gameObject.transform.eulerAngles.x.ToString());
        rotation.SetAttribute("y", gameObject.transform.eulerAngles.y.ToString());
        rotation.SetAttribute("z", gameObject.transform.eulerAngles.z.ToString());
        info.AppendChild(rotation);

        // 添加 scale
        XmlElement scale = xml.CreateElement("scale");
        scale.SetAttribute("x", gameObject.transform.localScale.x.ToString());
        scale.SetAttribute("y", gameObject.transform.localScale.y.ToString());
        scale.SetAttribute("z", gameObject.transform.localScale.z.ToString());
        info.AppendChild(scale);

        // 添加 parent_id
        XmlElement parent_id = xml.CreateElement("parent_id");
        if (gameObject.transform.parent != null)
            parent_id.InnerText = gameObjectToId[gameObject.transform.parent.gameObject].ToString();
        else
            parent_id.InnerText = "0";
        info.AppendChild(parent_id);

        // 添加 tag
        XmlElement tag = xml.CreateElement("tag");
        tag.InnerText = gameObject.tag; // 获取 GameObject 的 tag
        info.AppendChild(tag);

        // 添加 interaction
        if (gameObject.TryGetComponent(out SceneElement sceneElement))
        {
            XmlElement interaction = xml.CreateElement("interaction");
            interaction.SetAttribute("type", sceneElement.interactType);
            interaction.InnerText = sceneElement.interactionContent;
            info.AppendChild(interaction);
        }

        // 将该 element 添加到根节点
        root.AppendChild(info);


        // 如果该物体是 Prefab 实例，且该物体有 Prefab 路径，不继续遍历其子物体
        if (!prefabPath.Equals("null"))
            return;
            
        // 递归遍历所有子物体
        foreach (Transform childTransform in gameObject.transform)
        {
            TraverseAndSave(childTransform.gameObject, ref currentId, xml, root);
        }
    }
}
#endif