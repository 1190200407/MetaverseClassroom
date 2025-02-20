using Microsoft.Office.Interop.PowerPoint;
using System;
using System.IO;
using System.Runtime.InteropServices;

public class PPTtoImage
{
    public static void ConvertPPTToImages(string pptFilePath, string outputDir)
    {
        // 创建PowerPoint应用实例
        Application pptApplication = new Application();

        // 打开PPT文件
        Presentations presentations = pptApplication.Presentations;
        Presentation presentation = presentations.Open(null, default, default, default);

        // 遍历所有幻灯片，导出为图片
        for (int slideIndex = 1; slideIndex <= presentation.Slides.Count; slideIndex++)
        {
            Slide slide = presentation.Slides[slideIndex];
            string outputFilePath = Path.Combine(outputDir, $"Slide_{slideIndex}.png");

            // 导出为PNG图片
            slide.Export(outputFilePath, "PNG");
        }

        // 关闭PPT文件
        presentation.Close();
        pptApplication.Quit();

        // 释放PowerPoint的资源
        Marshal.ReleaseComObject(pptApplication);
    }
}
