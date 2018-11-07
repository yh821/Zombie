using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;

/*******************************
 *  Author: Zhou Xing
 *  Time:   2015.09.09
 *  Function: Gzip压缩解压帮助类
 * *****************************/

public static class GzipHelper
{
    /// <summary>
    /// Gzip压缩
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipCompress(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (GZipOutputStream gzip = new GZipOutputStream(ms))
            {
                gzip.Write(bytes, 0, bytes.Length);
                gzip.Close();

                byte[] press = ms.ToArray();

                return press;
            }
        }
        
    }

    /// <summary>
    /// Gzip解压
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static int maxSize = 0;
    public static int GzipDecompress(string filePath, byte[] readBuff)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (GZipInputStream gzip = new GZipInputStream(fs))
            {
                return gzip.Read(readBuff, 0, readBuff.Length);
            }
        }
    }

    /// <summary>
    /// Gzip解压
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipDecompress(byte[] bytes)
    {
        using (GZipInputStream gzip = new GZipInputStream(new MemoryStream(bytes)))
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int count = 0;

                byte[] data = new byte[bytes.Length];
                while ((count = gzip.Read(data, 0, data.Length)) != 0)
                {
                    ms.Write(data, 0, count);
                }

                return ms.ToArray();
            }
        }

    }
    
    #region BZip
    //使用BZIP压缩<--单个-->文件的方法
    public static bool BZipFile(string sourcefilename, string zipfilename)
    {
        bool blResult;//表示压缩是否成功的返回结果
                      //为源文件创建文件流实例，作为压缩方法的输入流参数
        using (FileStream srcFile = File.OpenRead(sourcefilename))
        {
            //为压缩文件创建文件流实例，作为压缩方法的输出流参数
            using (FileStream zipFile = File.Open(zipfilename, FileMode.Create))
            {
                try
                {
                    //以4096字节作为一个块的方式压缩文件
                    BZip2.Compress(srcFile, zipFile, true, 9);
                    blResult = true;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.Message);
                    blResult = false;
                }
                srcFile.Close();//关闭源文件流
                zipFile.Close();//关闭压缩文件流
                return blResult;
            }
        }
    }

    //使用BZIP解压<--单个-->文件的方法
    public static bool UnBzipFile(string zipfilename, string unzipfilename)
    {
        bool blResult;//表示解压是否成功的返回结果
                      //为压缩文件创建文件流实例，作为解压方法的输入流参数
        using (FileStream zipFile = File.OpenRead(zipfilename))
        {
            //为目标文件创建文件流实例，作为解压方法的输出流参数
            using (FileStream destFile = File.Open(unzipfilename, FileMode.Create))
            {
                try
                {
                    BZip2.Decompress(zipFile, destFile, true);//解压文件
                    blResult = true;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.Message);
                    blResult = false;
                }
                destFile.Close();//关闭目标文件流
                zipFile.Close();//关闭压缩文件流
                return blResult;
            }
        }
        
    }
    
    #endregion

    #region GZip
    //使用GZIP压缩文件的方法
    public static void GZipFile(string sourcefilename, string zipfilename)
    {
        using (FileStream srcFile = File.OpenRead(sourcefilename))
        {
            byte[] FileData = new byte[srcFile.Length];
            srcFile.Read(FileData, 0, (int)srcFile.Length);
            srcFile.Close();

            using (GZipOutputStream zipFile = new GZipOutputStream(File.Open(zipfilename, FileMode.Create, FileAccess.Write)))
            {
                zipFile.Write(FileData, 0, FileData.Length);
                zipFile.Close();
            }
        }
    }

    //使用GZIP解压文件的方法
    public static void UnGzipFile(string zipfilename, string unzipfilename)
    {
        using (GZipInputStream zipFile = new GZipInputStream(File.OpenRead(zipfilename)))
        {
            byte[] FileData = new byte[zipFile.Length];
            zipFile.Read(FileData, 0, (int)zipFile.Length);
            zipFile.Close();

            using (FileStream destFile = File.Open(unzipfilename, FileMode.Create))
            {
                destFile.Write(FileData, 0, FileData.Length);
                destFile.Close();
            }
        }
    }
    #endregion
}