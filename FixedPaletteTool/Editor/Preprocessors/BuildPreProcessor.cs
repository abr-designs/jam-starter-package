using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace FixedColorPaletteTool.Preprocessors
{
    public class BuildPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<FixedPaletteSettings>(FixedPaletteSettings.AssetPath);
            
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            
            var allSettings = preloadedAssets
                .Where(x => x.GetType() == typeof(FixedPaletteSettings))
                .ToList();
            if (allSettings.Count > 1)
                throw new BuildFailedException($"More than one {nameof(FixedPaletteSettings)} set in preloaded assets!");
            if(allSettings.Count == 1 && allSettings.First() != settings)
                throw new BuildFailedException($"Included {nameof(FixedPaletteSettings)} is not the one expected!");

            //If the previous checks passed, then we can assume that we're ready to build
            //Otherwise, make sure that the asset is included with the build
            if (allSettings.Count != 0) 
                return;
            
            preloadedAssets.Add(settings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}