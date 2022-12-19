using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Interface
{


    public abstract class AbstractRecipe : INotifyPropertyChanged
    {
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

        [JsonIgnore, Browsable(false)]
        public bool IsPropertyChanged => changedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 由指定的檔案路徑載入 Recipe。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TRecipe Load<TRecipe>(string filename, IList<JsonConverter> converters) where TRecipe : AbstractRecipe
        {
            string extension = Path.GetExtension(filename);
            if (!File.Exists(filename)) throw new FileNotFoundException($"Not found recipe file", filename);

            try
            {
                string dirPath = new DirectoryInfo(filename).FullName;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                if (converters != null && converters.Any()) settings.Converters = converters;

                using (FileStream fs = File.Open(Path.GetFullPath(filename), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    var recipe = serializer.Deserialize<TRecipe>(jr);
                    recipe.FilePath = filename;
                    recipe.BeenSaved = true;
                    return recipe;
                }
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }

        /// <summary>
        /// 由指定的檔案路徑載入 Recipe。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TRecipe Load<TRecipe>(string filename, JsonConverter converter) where TRecipe : AbstractRecipe
        {
            string extension = Path.GetExtension(filename);
            if (!File.Exists(filename)) throw new FileNotFoundException($"Not found recipe file", filename);

            try
            {
                string dirPath = new DirectoryInfo(filename).FullName;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                if (converter != null) settings.Converters = new List<JsonConverter>() { converter };

                using (FileStream fs = File.Open(Path.GetFullPath(filename), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    var recipe = serializer.Deserialize<TRecipe>(jr);
                    recipe.FilePath = filename;
                    recipe.BeenSaved = true;
                    return recipe;
                }
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }

        public static TRecipe Load<TRecipe>(string filename) where TRecipe : AbstractRecipe
        {
            string extension = Path.GetExtension(filename);
            if (!File.Exists(filename)) throw new FileNotFoundException($"Not found recipe file", filename);

            try
            {
                string dirPath = new DirectoryInfo(filename).FullName;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                using (FileStream fs = File.Open(Path.GetFullPath(filename), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    var recipe = serializer.Deserialize<TRecipe>(jr);
                    recipe.FilePath = filename;
                    recipe.BeenSaved = true;
                    return recipe;
                }
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }

        public static AbstractRecipe Load(string filename)
        {
            string extension = Path.GetExtension(filename);
            if (!File.Exists(filename)) throw new FileNotFoundException($"Not found recipe file", filename);

            try
            {
                string dirPath = new DirectoryInfo(filename).FullName;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                using (FileStream fs = File.Open(Path.GetFullPath(filename), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    var recipe = (AbstractRecipe)serializer.Deserialize(jr);
                    recipe.FilePath = filename;
                    recipe.BeenSaved = true;
                    recipe.Name = Path.GetFileNameWithoutExtension(filename);
                    recipe.CreationTime = File.GetCreationTime(filename).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    recipe.LastWriteTime = File.GetLastWriteTime(filename).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    return recipe;
                }
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }

        public void Save(string fileName, IList<JsonConverter> converters)
        {
            try
            {
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
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(jw, this);
                }

                BeenSaved = true;
                FilePath = fileName;
                changedProperties.Clear(); // 存檔後清空紀錄變動值屬性的列表。
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Save recipe failed.", ex);
            }
        }

        public void Save(JsonConverter converter, string fileName)
        {
            try
            {
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

                if (converter != null) settings.Converters = new List<JsonConverter>() { converter };

                using (FileStream fs = File.Open(fileName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(jw, this);
                }

                BeenSaved = true;
                FilePath = fileName;
                changedProperties.Clear(); // 存檔後清空紀錄變動值屬性的列表。
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Save recipe failed.", ex);
            }
        }

        /// <summary>
        /// 儲存當前 Recpie 內容至指定路徑。
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName) => Save(fileName, null);

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

            // 紀錄變動值的屬性。
            if (!changedProperties.Contains(name))
                changedProperties.Add(name);
        }
    }

}
