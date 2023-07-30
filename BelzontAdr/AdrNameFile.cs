using Colossal;
using System;
using System.IO;
using System.Linq;

namespace BelzontAdr
{
    public class AdrNameFile
    {
        private static readonly Guid baseGuid = new(214, 657, 645, 54, 54, 45, 45, 45, 45, 45, 45);

        internal readonly Guid Id;
        public readonly string Name;
        public readonly string[] Values;
        public string IdString => Id.ToString();

        public AdrNameFile(string name, string[] values)
        {
            Id = GuidUtils.Create(baseGuid, name);
            Name = name;
            Values = values?.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray() ?? new string[0];
        }
    }
}
