using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Constants
{
    public struct ItemShaderProperties
    {
        public static readonly int SuggestHighlight = Shader.PropertyToID("_Suggest_Highlight");
        public static readonly int HighlightAmount = Shader.PropertyToID("_Highlight_Amount");
    }
}
