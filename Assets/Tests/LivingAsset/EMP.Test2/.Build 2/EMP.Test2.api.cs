 // This a a generated file. Do not modify.

using UnityEngine;
using System;

namespace EMP.Test2
{
    public class Api
    {
        private const string LIVING_ASSET_NAME = "EMP.Test2";

        private static Type FindType(string name) {
            EMP.LivingAsset.LivingAsset livingAsset = EMP.LivingAsset.LivingAsset.GetRegistry().Get(LIVING_ASSET_NAME);
            Type type = livingAsset.FindType(name);
            if (type == null) {
                throw new Exception("Type '" + name + "' is not available in LivingAsset '" + LIVING_ASSET_NAME + "'.");
            }
            return type;
        }

        public static void AddRotateComponent(GameObject gameObject) {
            Type type = FindType("Rotate");
            gameObject.AddComponent(type);
        }

        public static MonoBehaviour GetRotateComponent(GameObject gameObject) {
            Type type = FindType("Rotate");
            return gameObject.GetComponent(type) as MonoBehaviour;
        }

    }
}
