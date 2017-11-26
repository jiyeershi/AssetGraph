/**
 * AssetBundles-Browser integration
 * 
 * This code will setup the output of the graph tool to be viewable in the browser.
 * 
 * AssetBundles-Browser Available at:
 * https://github.com/Unity-Technologies/AssetBundles-Browser
 */
 
using UnityEditor;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using System;
using System.Collections.Generic;

namespace UnityEngine.AssetBundles.AssetBundleDataSource
{
    public partial struct ABBuildInfo { }
    public partial interface ABDataSource { }
}

namespace UnityEngine.AssetGraph {
    public class GraphToolABDataSource : UnityEngine.AssetBundles.AssetBundleDataSource.ABDataSource
    {
        public static List<UnityEngine.AssetBundles.AssetBundleDataSource.ABDataSource> CreateDataSources()
        {
            var op = new GraphToolABDataSource();
            var retList = new List<UnityEngine.AssetBundles.AssetBundleDataSource.ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name {
			get {
				return "AssetBundles";
			}
		}

		public string ProviderName {
			get {
				return "AssetGraph";
			}
		}

		public string[] GetAssetPathsFromAssetBundle (string assetBundleName) {
			return AssetBundleBuildMap.GetBuildMap ().GetAssetPathsFromAssetBundle (assetBundleName);
		}

		public string GetAssetBundleName(string assetPath) {
			return AssetBundleBuildMap.GetBuildMap ().GetAssetBundleName (assetPath);
		}

		public string GetImplicitAssetBundleName(string assetPath) {
			return AssetBundleBuildMap.GetBuildMap ().GetImplicitAssetBundleName (assetPath);
		}

		public string[] GetAllAssetBundleNames() {
            UpdateBuildMap ();
			return AssetBundleBuildMap.GetBuildMap ().GetAllAssetBundleNames ();
		}

		public bool IsReadOnly() {
			return true;
		}

		public void SetAssetBundleNameAndVariant (string assetPath, string bundleName, string variantName) {
			// readonly. do nothing
		}

		public void RemoveUnusedAssetBundleNames() {
			// readonly. do nothing
		}

		public bool CanSpecifyBuildTarget {
			get { return true; } 
		}
		public bool CanSpecifyBuildOutputDirectory { 
			get { return false; } 
		}

		public bool CanSpecifyBuildOptions { 
			get { return false; } 
		}

        private void UpdateBuildMap() {
            var graphGuid = Model.Settings.UserSettings.DefaultAssetBundleBuildGraphGuid;
            string path = AssetDatabase.GUIDToAssetPath(graphGuid);

            if(string.IsNullOrEmpty(path)) {
                return;
            }

            AssetBundleBuildMap.GetBuildMap ().Clear ();
            AssetGraphUtility.ExecuteGraphSetup (path);
        }

        public bool BuildAssetBundles (UnityEngine.AssetBundles.AssetBundleDataSource.ABBuildInfo info) {
			
            AssetBundleBuildMap.GetBuildMap ().Clear ();

            var graphGuid = Model.Settings.UserSettings.DefaultAssetBundleBuildGraphGuid;

            if (string.IsNullOrEmpty (graphGuid)) {
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(graphGuid);
            if(string.IsNullOrEmpty(path)) {
                return false;
            }

            var graph = AssetDatabase.LoadAssetAtPath<Model.ConfigGraph>(path);

            Type infoType = info.GetType();

            var fieldInfo = infoType.GetField ("buildTarget");
            if (fieldInfo != null) {
                BuildTarget target = (BuildTarget)fieldInfo.GetValue (info);
                var result = AssetGraphUtility.ExecuteGraph(target, graph);
                if (result.IsAnyIssueFound)
                {
                    return false;
                }
            }

			return true;
		}
    }
}
