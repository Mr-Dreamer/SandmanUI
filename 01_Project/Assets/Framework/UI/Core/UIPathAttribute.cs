using System;

namespace Framework.UI.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UIPathAttribute : Attribute
    {
        public string Path {get; private set;}
        
        public UIPathAttribute(string path)
        {
            Path = path;
        }
    }
}