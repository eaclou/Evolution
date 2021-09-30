using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playcraft
{
    public static class RandomStatics
    {
        #region NEW
        public static T RandomEnumValue<T>()
        {
            var values = Enum.GetValues(typeof(T));
            var random = Random.Range(0, values.Length);
            return (T)values.GetValue(random);
        }
        
        /// Input percent chance of returning true (default is 0.5)
        public static bool CoinToss(float chance = 0.5f) { return Random.Range(0, 1) < chance; }
        
        public static bool RandomFlip(float chance, bool defaultValue) { return CoinToss(chance) ? !defaultValue : defaultValue; }
    
        #endregion
        
        public static int RandomNoRepeat(int min, int max, int prior)
        {
            if (max - min < 1)
                return prior;
            
            int result = prior;
            
            while (result == prior)
                result = Random.Range(min, max);
            
            return result;
        }
        
        public static Vector3 RandomInRectangle(MinMaxVector3 range)
        {
            return RandomInRectangle(range.min, range.max);
        }
        
        public static Vector3 RandomInRectangle(Vector3 min, Vector3 max)
        {
            var x = Random.Range(min.x, max.x);
            var y = Random.Range(min.y, max.y);
            var z = Random.Range(min.z, max.z);
            return new Vector3(x, y, z);
        }
    
        public static Vector3 RandomInHollowCylinder(Vector2 widthRange, Vector3 heightRange, Axis axis = Axis.Y)
        {
            var height = Random.Range(heightRange.x, heightRange.y);
            var width = Random.Range(widthRange.x, widthRange.y);
            var direction = Random.insideUnitCircle.normalized;
            var widthVector = direction * width;
            
            switch (axis)
            {
                case Axis.X: return new Vector3(height, widthVector.x, widthVector.y);
                case Axis.Y: return new Vector3(widthVector.x, height, widthVector.y);
                case Axis.Z: return new Vector3(widthVector.x, widthVector.y, height);
                default: return new Vector3(widthVector.x, height, widthVector.y);
            }
        }
    }

    [Serializable] public struct MinMaxVector3
    {
        public Vector3 min;
        public Vector3 max;
    }
}
