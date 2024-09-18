using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.DataStructs;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Product Database", menuName = "Scriptable Objects/Databases/Product Database")]
    public class ProductDatabase : ScriptableObject
    {
        [SerializeField] public List<ProductInfo> ProductInfos;

        public Dictionary<string, ProductInfo> ProductCollection { get; private set; }

        public void Initialize()
        {
            ProductCollection = ProductInfos.ToDictionary(key => key.ProductID, value => value);
        }
    }
}
