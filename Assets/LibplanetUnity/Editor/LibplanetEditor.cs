﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace LibplanetUnity.Editor
{
    public static class LibplanetEditor
    {
        [MenuItem("Tools/Libplanet/Delete All(Editor)")]
        public static void DeleteAllEditor()
        {
            var path = Path.Combine(Application.persistentDataPath, "planetarium_dev.ldb");
            DeleteAll(path);
        }
        
        [MenuItem("Tools/Libplanet/Delete All(Player)")]
        public static void DeleteAllPlayer()
        {
            var path = Path.Combine(Application.persistentDataPath, "planetarium.ldb");
            DeleteAll(path);
        }

        private static void DeleteAll(string path)
        {
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                return;
            }
            info.Delete();
        }
    }   
}
