using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public abstract class CogParameter
    {
        public CogParameter(int id  )
        {
            Id = id;

        }
        // 紀錄變動過值的屬性。
        private List<string> changedProperties = new List<string>();

        [Browsable(false)]
        public static string Extension { get; set; } = ".json";

        [Browsable(false)]
        public string Name { get; set; }

        [JsonIgnore, Browsable(false)]
        public string CreationTime { get; private set; }

        [JsonIgnore, Browsable(false)]
        public string LastWriteTime { get; private set; }

        [JsonIgnore, Browsable(false)]
        public bool BeenSaved { get; set; } = false;

        [JsonIgnore, Browsable(false)]
        public string FilePath { get; set; }
        /// <summary>
        /// 取得或設定Patmax 的Id 預設為 = 0, 若一個料號有兩個以上的Patmax參數屬性, 請明確指定Id後再儲存
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 輸出資料的型態 ，FULL :完整輸出 ， 中心點 ， 起點 ， 終點
        /// </summary>
        public ResultSelect ResultOutput { get; set; } = ResultSelect.Full;
        /// <summary>
        /// VisionPro 演算法方法
        /// </summary>
        public MethodName Methodname { get; set; }

        /// <summary>
        /// 由指定的檔案路徑載入 Recipe。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>

        public static CogParameter Load (string recipeName,int id)
        {
           
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = $"{systemPath}\\Recipe\\{recipeName}";

            string filename = path + $"\\Commom{id}.json";

          //  string fileFullPath = Path.GetFullPath(filename);
          //  string dirFullPath = Path.GetDirectoryName(fileFullPath);


            string extension = Path.GetExtension(filename);
            if (!File.Exists(filename)) throw new FileNotFoundException($"Not found recipe file", filename);

            try {
                string dirPath = new DirectoryInfo(filename).FullName;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                using (FileStream fs = File.Open(Path.GetFullPath(filename), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader jr = new JsonTextReader(sr)) {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    var recipe = serializer.Deserialize<CogParameter>(jr);
                    recipe.FilePath = filename;
                    recipe.BeenSaved = true;
                    recipe.LoadRecipe(path, id);
                    return recipe;
                }
            }
            catch (JsonReaderException) {
                throw;
            }
        }

        public void Save(string recipeName, IList<JsonConverter> converters)
        {
            try {

                string fileName = CreateFolder(recipeName) + $"\\Commom{Id}.json";

                string fileFullPath = Path.GetFullPath(fileName);
                string dirFullPath = Path.GetDirectoryName(fileFullPath);

                DirectoryInfo dir = new DirectoryInfo(dirFullPath);
                if (!dir.Exists) throw new DirectoryNotFoundException($"Directory not exists {dir.FullName}");

                Name = Path.GetFileNameWithoutExtension(fileName);

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error,
                    TypeNameHandling = TypeNameHandling.All
                };

                if (converters != null && converters.Any()) settings.Converters = converters;

                using (FileStream fs = File.Open(fileName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw)) {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(jw, this);
                }

                BeenSaved = true;
                FilePath = fileName;
                changedProperties.Clear(); // 存檔後清空紀錄變動值屬性的列表。

                SaveCogRecipe(dirFullPath);
            }
            catch (Exception ex) {
                throw new InvalidOperationException($"Save recipe failed.", ex);
            }
        }
     
       

        /// <summary>
        /// 儲存當前 Recpie 內容至指定路徑。
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName) => Save(fileName, null);
       
        /// <summary>
        /// 在我的文件夾裡面 創造Recipe 的資料夾  再以Recipe名稱創建資料夾  將recipe的檔案集中在裡面
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>    
        protected string CreateFolder(string folderName)
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        

            string path = $"{systemPath}\\Recipe\\{folderName}";
            if (!Directory.Exists(path))  Directory.CreateDirectory(path);

            return path;
        }

        protected abstract void SaveCogRecipe(string directoryPath);

        protected abstract void LoadRecipe(string directoryPath, int id) ;
    }

    public class VisionResult
    {
        public OutputOption ResultOutput { get; set; }  

        /// <summary>
        /// 距離
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        public BlobDetectorResult[] BlobResult { get; set; } = null;
        public CaliperResult[] CaliperResult { get; set; } = null;
        public MatchResult[] MatchResult { get; set; } = null;
    }

    public class CombineOptionOutput
    {
        public OutputOption Option { get; set; }

        public string SN1 { get; set; }
        public string SN2 { get; set; }

    }

    public enum ResultSelect
    {
        Full,
        Center,
        Begin,
        End,
    }
    public enum MethodName
    {
        PatternMatch,
        GapMeansure,
        LineMeansure,
        CircleMeansure
    }

    public enum OutputOption
    {
        Result,
        Distance,
        Angle,
    }


    public class MeansureRecipe 
    {
        public PatmaxParams LocateParams { get; set; } = new PatmaxParams(0);

        public List<CogParameter> MethodParams { get; set; } = new List<CogParameter>();

        public List<CombineOptionOutput> CombineOptionOutputs { get; set; } = new List<CombineOptionOutput>();


    
    }
}
