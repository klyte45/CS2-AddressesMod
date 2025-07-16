using System;
using System.Collections.Generic;
using System.Reflection;

namespace BridgeWE
{
    public static class WETemplatesManagementBridge
    {
        public static bool RegisterCustomTemplates(Assembly mainAssembly, string rootFolderLayouts) => throw new NotImplementedException("Stub only!");
        public static void RegisterLoadableTemplatesFolder(Assembly mainAssembly, string rootFolder) => throw new NotImplementedException("Stub only!");
        public static Dictionary<string, string> GetMetadatasFromReplacement(Assembly mainAssembly, string layoutName) => throw new NotImplementedException("Stub only!");
    }
}