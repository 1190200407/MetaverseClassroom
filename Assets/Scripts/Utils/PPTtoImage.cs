using System.Collections;
using Spire.Presentation;
using System.Drawing;
using System.IO;
using UnityEngine;

public class PPTToImageConverter : UnitySingleton<PPTToImageConverter>
{
    /// <summary>
    /// 将tiff文件转换为jpg文件，因为Spire.Presentation.FileFormat的枚举中没有jpg
    /// </summary>
    /// <param name="tiffFilePath"></param>
    /// <param name="jpgFilePath"></param>
    private void ConvertTiffToJpg(string tiffFilePath, string jpgFilePath)
    {
        using (Bitmap tiffImage = new Bitmap(tiffFilePath))
        {
            // 保存为 JPG 格式，质量设定为 100
            tiffImage.Save(jpgFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }

    // 将 PPT 文件转换为图像，并保存在 PPT 所在路径的子文件夹中
    public Texture2D[] ConvertPPTToImage(string pptFilePath, string outputDir)
    {
        try
        {
            // 确保文件存在
            if (!File.Exists(pptFilePath))
            {
                Debug.LogError("PPT 文件未找到: " + pptFilePath);
                return null;
            }
            Texture2D[] textures;

            // 确保输出目录存在
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            // 如果输出目录缓存了图片则直接导出
            else
            {
                string[] paths = Directory.GetFiles(outputDir);
                if (paths.Length > 0)
                {
                    textures = new Texture2D[paths.Length];

                    // 加载 JPG 文件为 Texture2D
                    for (int i = 0; i < paths.Length; i++)
                    {
                        byte[] imageData = File.ReadAllBytes(paths[i]);
                        Texture2D texture = new Texture2D(1024, 512); // 临时尺寸
                        texture.LoadImage(imageData); // 将图片数据加载到 Texture2D 中
                        // 存储 Texture2D 到数组
                        textures[i] = texture;
                    }

                    return textures;
                }
            }

            // 创建 Presentation 实例并加载 PPT 文件
            Presentation presentation = new Presentation();
            presentation.LoadFromFile(pptFilePath);

            // 获取幻灯片总数
            int slideCount = presentation.Slides.Count;
            Debug.Log($"Total Slides: {slideCount}");

            // 创建 Texture2D 数组来存储幻灯片的图像
            textures = new Texture2D[slideCount];
            // 遍历幻灯片并保存
            for (int i = 0; i < slideCount; i++)
            {
                string tiffFilePath = Path.Combine(outputDir, $"Slide_{i + 1}.tif");

                // 保存为tiff
                presentation.Slides[i].SaveToFile(tiffFilePath, FileFormat.Tiff);
                
                string jpgFilePath = Path.Combine(outputDir, $"Slide_{i + 1}.jpeg");
                //转换为jpg
                ConvertTiffToJpg(tiffFilePath, jpgFilePath);
                // 加载 JPG 文件为 Texture2D
                byte[] imageData = File.ReadAllBytes(jpgFilePath);
                Texture2D texture = new Texture2D(1024, 512); // 临时尺寸
                texture.LoadImage(imageData); // 将图片数据加载到 Texture2D 中
                // 存储 Texture2D 到数组
                textures[i] = texture;
                // 删除临时tiff文件
                File.Delete(tiffFilePath);
            }

            Debug.Log($"所有幻灯片已保存至: {outputDir}");
            return textures;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("转换 PPT 失败: " + ex.Message);
        }
        return null;
    }
}