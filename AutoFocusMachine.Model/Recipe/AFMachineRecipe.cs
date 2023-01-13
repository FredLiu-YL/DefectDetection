using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.ImageProcess.Blob;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model.Recipe
{
    public class AFMachineRecipe : AbstractRecipe
    {

        public Wafer WaferData { get; set; }
        
        public Point[] FiducialMarkGrabPos { get; set; }

        public Point[] LayoutPos { get; set; }

        public PatmaxParams PMParams { get; set; }

        public CogFindLine LineAParam { get; set; }
        public CogFindLine LineBParam { get; set; }


        public BlobParam DefectParam { get; set; }


        /*    public static AFMachineRecipe Load(string filename)
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
                        var recipe = (AFMachineRecipe)serializer.Deserialize(jr);
                        recipe.FilePath = filename;
                        recipe.BeenSaved = true;
                        recipe.Name = Path.GetFileNameWithoutExtension(filename);
                       // recipe.CreationTime = File.GetCreationTime(filename).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                       // recipe.LastWriteTime = File.GetLastWriteTime(filename).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return recipe;
                    }
                }
                catch (JsonReaderException)
                {
                    throw;
                }
            }*/
    }


    public class LocateData : AbstractRecipe
    {


        public Point[] FiducialMarkPos { get; set; }

        public Point[] LayoutPos { get; set; }

        public  Point[]  LayoutIndex { get; set; }
    }

}
