using System;
using System.Linq;
using System.Reflection;
using NugetForUnity;
using NugetForUnity.Configuration;
using NugetForUnity.Models;
using UnityEditor;
using UnityEngine;

namespace JamStarter.Editor
{
    internal static class AddNuGetPackages
    {
        private static readonly (string packageId, string version)[] NugetPackages =
        {
            ("ZLinq", "1.5.4"),
        };

        //============================================================================================================//

        [InitializeOnLoadMethod]
        private static void UpdateNugetPackages()
        {
            CheckForInstallLocationUpdate();
            
            var shouldUpdate = false;
            for (int i = 0; i < NugetPackages.Length; i++)
            {
                var (packageId, version) = NugetPackages[i];

                if (ContainsPackage(packageId))
                    continue;

                shouldUpdate = true;

                // Create an identifier
                var identifier = new NugetPackageIdentifier(packageId, version);

                NugetPackageInstaller.InstallIdentifier(identifier, false);
            }

            if (shouldUpdate)
                AssetDatabase.Refresh();

            return;

            bool ContainsPackage(string packageId)
            {
                return InstalledPackagesManager.InstalledPackages.Any(x => x.Id.Equals(packageId));
            }
        }

        private static void CheckForInstallLocationUpdate()
        {
            const int CustomWithinAssets = 0;
            const int InPackagesFolder = 1;
            
            var installLocation = NugetInternalConfig.GetInstallLocation();

            if (installLocation.Equals(InPackagesFolder))
                return;
            
            NugetInternalConfig.SetInstallLocation(InPackagesFolder);
        }

        private static class NugetInternalConfig
        {
            private const string AssemblyName = "NugetForUnity";

            private static readonly Type _installLocationEnumType;
            private static readonly Type _packageContentManagerType;

            private static readonly PropertyInfo _installLocationProp;
            private static readonly MethodInfo _updateInstalledPackagesMethod;
            private static readonly MethodInfo _moveConfigMethod;
            private static readonly MethodInfo _moveInstalledPackagesMethod;

            static NugetInternalConfig()
            {
                var asm = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.FullName.Contains(AssemblyName, StringComparison.OrdinalIgnoreCase));

                if (asm == null)
                {
                    Debug.LogError("[NuGet] Unable to locate NugetForUnity assembly.");
                    return;
                }

                var configType = typeof(NugetConfigFile); //asm.GetType(ConfigTypeName, throwOnError: true);
                _installLocationEnumType = asm.GetTypes()
                    .FirstOrDefault(x => x.FullName.Contains("InstallLocation", StringComparison.OrdinalIgnoreCase));

                _packageContentManagerType = asm.GetTypes().FirstOrDefault(x =>
                    x.FullName.Contains("PackageContentManager", StringComparison.OrdinalIgnoreCase));
                
                // Internal InstallLocation property
                _installLocationProp = configType.GetProperty(
                    "InstallLocation",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                //InstalledPackagesManager.UpdateInstalledPackages();
                _updateInstalledPackagesMethod = typeof(InstalledPackagesManager).GetMethod("UpdateInstalledPackages",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                
                //ConfigurationManager.MoveConfig(newInstallLocation);
                _moveConfigMethod = typeof(ConfigurationManager).GetMethod("MoveConfig",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                _moveInstalledPackagesMethod = _packageContentManagerType.GetMethod("MoveInstalledPackages",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            internal static void SetInstallLocation(int enumValue)
            {
                //Emulates the NugetPreferences.cs change in install location:
                /*
                    var oldRepoPath = ConfigurationManager.NugetConfigFile.RepositoryPath;
                    InstalledPackagesManager.UpdateInstalledPackages(); // Make sure it is initialized before we move files around
                    ConfigurationManager.MoveConfig(newInstallLocation);
                    var newRepoPath = ConfigurationManager.NugetConfigFile.RepositoryPath;
                    PackageContentManager.MoveInstalledPackages(oldRepoPath, newRepoPath);
                 */
                
                var currentInstallLocation = GetInstallLocation();

                if (currentInstallLocation == enumValue)
                    return;

                PackagesConfigFile.Load();
                
                var oldRepoPath = ConfigurationManager.NugetConfigFile.RepositoryPath;

                _updateInstalledPackagesMethod.Invoke(null, null);

                var newEnum = Enum.ToObject(_installLocationEnumType, enumValue);
                _moveConfigMethod.Invoke(null, new[] { newEnum });
                
                var newRepoPath = ConfigurationManager.NugetConfigFile.RepositoryPath;
                _moveInstalledPackagesMethod.Invoke(null, new object[] { oldRepoPath, newRepoPath });
                
                ConfigurationManager.NugetConfigFile.Save(ConfigurationManager.NugetConfigFilePath);

                AssetDatabase.Refresh();
            }

            internal static int GetInstallLocation()
            {
                var value = _installLocationProp.GetValue(ConfigurationManager.NugetConfigFile);
                return (int)value;
            }
        }
    }
}