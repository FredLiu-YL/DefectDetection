using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public abstract class CogParameter
    {
        public CogParameter(int id)
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
        /// 由指定的檔案路徑資料夾載入 Recipe。 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>

        public static CogParameter Load(string drectoryPath, int id)
        {

            // string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);          
            // string path = $"{systemPath}\\Recipe\\{recipeName}";
            //一包 Cognex的檔案裏面  包含  Commom.json  與 vpp檔
            string filename = drectoryPath + $"\\Commom{id}.json";

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
                    //  recipe.LoadRecipe(path, id);
                    recipe.LoadCogRecipe(drectoryPath, id);
                    return recipe;
                }
            }
            catch (JsonReaderException) {
                throw;
            }
        }
        /// <summary>
        /// 存檔分成 Commom 檔 (一般參數)跟 Vision檔( cognex的檔案不能用json方式序列化)
        /// </summary>
        /// <param name="recipeName"></param>
        /// <param name="converters"></param>
        public void Save(string drectoryPath, IList<JsonConverter> converters)
        {
            try {

                //string fileName = CreateFolder(recipeName) + $"\\Commom{Id}.json";
                string fileName = drectoryPath + $"\\Commom{Id}.json";
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
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// 儲存 cognex 檔案
        /// </summary>
        /// <param name="directoryPath"></param>
        protected abstract void SaveCogRecipe(string directoryPath);

        /// <summary>
        /// 讀取 cognex 檔案
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="id"></param>
        protected abstract void LoadCogRecipe(string directoryPath, int id);
    }

    public class CogResult
    {
        public Point EndPoint { get; protected set; }
        public Point BeginPoint { get; protected set; }
        public Point CenterPoint { get; protected set; }

    }

    public class VisionResult
    {
        public OutputOption ResultOutput { get; set; }

        /// <summary>
        /// 距離
        /// </summary>
        public double Distance { get; set; }
        public Point EndPoint { get;   set; }
        public Point BeginPoint { get;   set; }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        public BlobDetectorResult[] BlobResult { get; set; } = null;
        public CaliperResult CaliperResult { get; set; } = null;
        public MatchResult[] MatchResult { get; set; } = null;
        public LineCaliperResult LineResult { get; set; } = null;

        public bool Judge { get; set; }
    }

    public class CombineOptionOutput
    {
        public OutputOption Option { get; set; }

        public string SN1 { get; set; }
        public string SN2 { get; set; }
        public double ThresholdMin { get; set; }
        public double ThresholdMax { get; set; }
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

        GapMeansure,
        LineMeansure,
        CircleMeansure,
        BlobDetector,
        PatternMatch,

    }

    public enum OutputOption
    {
        None,
        Distance,
        Angle,
    }


    public class MeansureRecipe : AbstractRecipe, INotifyPropertyChanged
    {
        /// <summary>
        /// 定位用參數
        /// </summary>
        [JsonIgnore]
        public PatmaxParams LocateParams { get; set; } = new PatmaxParams(0);

        /// <summary>
        /// 量測演算法集合
        /// </summary>
        [JsonIgnore]
        public List<CogParameter> MethodParams { get; set; } = new List<CogParameter>();

        /// <summary>
        /// 輸出搭配設定
        /// </summary>
        public List<CombineOptionOutput> CombineOptionOutputs { get; set; } = new List<CombineOptionOutput>();

        public double PixelSize { get; set; } = 1;

        /// <summary>
        /// 因某些元件無法被正常序列化 所以另外做存檔功能
        /// </summary>
        /// <param name="Path"></param>
        public new void Save(string path)
        {
            //刪除所有Vistiontool 的檔案避免 id重複 寫錯，或是 原先檔案數量5個  後來變更成3個  讀檔會錯誤
            string[] files = Directory.GetFiles(path, "*VsTool_*");
            foreach (string file in files) {
                if (file.Contains("VsTool")) // 如果文件名包含 "VSP"
                {
                    File.Delete(file); // 删除该文件
                }
            }

            LocateParams.Save(path);
            foreach (var param in MethodParams) {
                param.Save(path);
            }
            base.Save(path + "\\Recipe.json");
        }
        /// <summary>
        /// 因某些元件無法被正常序列化 所以另外做讀檔功能
        /// </summary>
        /// <param name="Path"></param>
        public void Load(string path)
        {

            MethodParams.Clear();
            CombineOptionOutputs.Clear();
            //想不到好方法做序列化 ， 如果需要修改 就要用JsonConvert 把不能序列化的屬性都改掉  這樣就能正常做load
            var mRecipe = AbstractRecipe.Load<MeansureRecipe>($"{path}\\Recipe.json");
            CombineOptionOutputs = mRecipe.CombineOptionOutputs; //未來新增不同屬性  這裡都要不斷新增
            PixelSize = mRecipe.PixelSize; //未來新增不同屬性  這裡都要不斷新增


            LocateParams = CogParameter.Load(path, 0) as PatmaxParams;
            string[] files = Directory.GetFiles(path, "*VsTool_*");

            foreach (var file in files) {
                string fileName = Path.GetFileName(file);

                string[] id = fileName.Split(new string[] { "VsTool_", ".tool" }, StringSplitOptions.RemoveEmptyEntries);
                if (id[0] == "0") continue; // 0 是定位用的樣本 所以排除
                var param = CogParameter.Load(path, Convert.ToInt32(id[0]));
                MethodParams.Add(param);

            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            T oldValue = field;
            field = value;
            OnPropertyChanged(propertyName, oldValue, value);
        }
        protected virtual void OnPropertyChanged<T>(string name, T oldValue, T newValue)
        {
            // oldValue 和 newValue 目前沒有用到，代爾後需要再實作。
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
