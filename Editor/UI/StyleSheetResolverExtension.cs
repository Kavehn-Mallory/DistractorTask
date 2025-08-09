using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public static class StyleSheetResolverExtension
    {
        public static void ResolveCustomStyleSheetProperty<T>(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<T> customStyleProperty, ref T property) where T : Object
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<float> customStyleProperty, ref float property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<int> customStyleProperty, ref int property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<bool> customStyleProperty, ref bool property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<Color> customStyleProperty, ref Color property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<Color> customStyleProperty, ref Color property, bool wasColorSet)
        {
            if (wasColorSet)
            {
                return;
            }
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<Texture2D> customStyleProperty, ref Texture2D property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<Sprite> customStyleProperty, ref Sprite property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<VectorImage> customStyleProperty, ref VectorImage property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
        public static void ResolveCustomStyleSheetProperty(this CustomStyleResolvedEvent evt,
            CustomStyleProperty<string> customStyleProperty, ref string property)
        {
            if (evt.customStyle.TryGetValue(customStyleProperty, out var resolvedValue))
            {
                property = resolvedValue;
            }
        }
        
    }
}