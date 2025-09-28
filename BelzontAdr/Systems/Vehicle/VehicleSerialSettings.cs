using Belzont.Utils;
using Colossal;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Hash128 = Colossal.Hash128;


namespace BelzontAdr
{
    public class VehicleSerialSettings : ISerializable
    {
        private const string ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NUM = "0123456789";
        private const string ALPHA_NUM = NUM + ALPHA;

        private const uint CURRENT_VERSION = 0;
        private string[] m_lettersAllowed;
        private uint m_flagsOwnSerial;
        private uint m_flagsCarNumber;
        private bool? m_buildingIdOnStart;
        private Hash128 checksum;


        public bool IsDirty { get; private set; }
        public void Clear() => IsDirty = false;

        public uint FlagsOwnSerial
        {
            get => m_flagsOwnSerial; set
            {
                m_flagsOwnSerial = value;
                UpdateChecksum();
            }
        }
        public uint FlagsCarNumber
        {
            get => m_flagsCarNumber; set
            {
                m_flagsCarNumber = value;
                UpdateChecksum();
            }
        }

        public Hash128 Checksum => checksum;

        public string[] LettersAllowed
        {
            get => m_lettersAllowed;
            set
            {
                m_lettersAllowed = value;
                UpdateChecksum();
            }
        }

        private void UpdateChecksum()
        {
            checksum = GuidUtils.Create(default, m_lettersAllowed.SelectMany(x => x.SelectMany(y => y.ToBytes())).Union(m_flagsOwnSerial.ToBytes()).Union(m_flagsCarNumber.ToBytes()).ToArray());
            IsDirty = true;
        }

        //One generator for each VehicleServiceType
        public static VehicleSerialSettings CreateBusSerialSettings()
        {
            var result = new VehicleSerialSettings
            {
                m_flagsCarNumber = 0b0,
                m_buildingIdOnStart = true,
                m_flagsOwnSerial = 0b1111,
                m_lettersAllowed = new string[]
                {
                    " ",
                    NUM,
                    NUM,
                    NUM,
                    NUM
                },
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehicleSerialSettings CreateTaxiSerialSettings()
        {
            var result = new VehicleSerialSettings
            {
                m_flagsCarNumber = 0b0,
                m_buildingIdOnStart = true,
                m_flagsOwnSerial = 0b1111,
                m_lettersAllowed = new string[]
                {
                    "-",
                    ALPHA,
                    ALPHA,
                    NUM,
                    NUM
                },
            };
            result.UpdateChecksum();
            return result;
        }


        public static VehicleSerialSettings CreateCityServicesSerialSettings()
        {
            var result = new VehicleSerialSettings
            {
                m_flagsCarNumber = 0b0,
                m_buildingIdOnStart = true,
                m_flagsOwnSerial = 0b111,
                m_lettersAllowed = new string[]
                {
                    "-",
                    ALPHA,
                    NUM,
                    NUM,
                    NUM
                },
            };
            result.UpdateChecksum();
            return result;
        }




        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out m_flagsOwnSerial);
            reader.Read(out m_flagsCarNumber);
            reader.Read(out int bldgIdOnStart);
            m_buildingIdOnStart = bldgIdOnStart switch
            {
                1 => true,
                0 => false,
                _ => null
            };
            reader.Read(out int lettersCount);
            m_lettersAllowed = new string[lettersCount];
            for (int i = 0; i < lettersCount; i++) reader.Read(out m_lettersAllowed[i]);

            UpdateChecksum();
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_flagsOwnSerial);
            writer.Write(m_flagsCarNumber);
            writer.Write(m_buildingIdOnStart.HasValue ? m_buildingIdOnStart.Value ? 1 : 0 : -1);
            writer.Write(m_lettersAllowed.Length);
            foreach (var s in m_lettersAllowed) writer.Write(s);
        }

        public SafeStruct ForBurstJob => new()
        {
            Checksum = checksum,
            m_flagsCarNumber = m_flagsCarNumber,
            m_flagsLocal = m_flagsOwnSerial,
            m_buildingIdOnStart = m_buildingIdOnStart.HasValue ? (m_buildingIdOnStart.Value ? 1 : 0) : -1,
            m_charZeroPos = new NativeArray<int>(m_lettersAllowed.Select((x, i) => m_lettersAllowed.Take(i).Sum(y => y.Length)).ToArray(), Allocator.TempJob),
            m_lettersAllowed = string.Join("", m_lettersAllowed).ToUshortNativeArray(Allocator.TempJob),
        };

        public unsafe struct SafeStruct
        {
            internal uint m_flagsLocal;
            internal uint m_flagsCarNumber;
            internal int m_buildingIdOnStart;
            internal NativeArray<ushort> m_lettersAllowed;
            public NativeArray<int> m_charZeroPos;

            public Hash128 Checksum;
            public readonly FixedString32Bytes GetSerialFor(FixedString32Bytes buildingId, ulong localSerial, int compositionNumber = 1)
            {
                var output = new NativeArray<Unicode.Rune>(m_charZeroPos.Length, Allocator.Temp);
                uint currentFlag = 1;
                int currentIdx = m_charZeroPos.Length - 1;
                do
                {
                    var charZeroPos = (ulong)m_charZeroPos[currentIdx];
                    var numberChars = (ulong)(currentIdx == m_charZeroPos.Length - 1 ? m_lettersAllowed.Length : m_charZeroPos[currentIdx + 1]) - charZeroPos;
                    if ((m_flagsCarNumber & currentFlag) != 0)
                    {
                        output[currentIdx] = new Unicode.Rune(m_lettersAllowed[(int)((ulong)compositionNumber % numberChars + charZeroPos)]);
                        compositionNumber /= (int)numberChars;
                    }
                    else
                    {
                        output[currentIdx] = new Unicode.Rune(m_lettersAllowed[(int)((localSerial % numberChars) + charZeroPos)]);
                        localSerial /= numberChars;
                    }
                    currentFlag <<= 1;
                } while (--currentIdx >= 0);
                var result = new FixedString32Bytes();
                if (m_buildingIdOnStart == 1 && buildingId.Length > 0)
                {
                    result.Append(buildingId);
                }
                for (int i = 0; i < output.Length; i++)
                {
                    if (output[i].value != 0) result.Append(output[i]);
                }
                if (m_buildingIdOnStart == 0 && buildingId.Length > 0)
                {
                    result.Append(buildingId);
                }
                output.Dispose();
                return result;
            }

            internal void Dispose(JobHandle dependency)
            {
                m_lettersAllowed.Dispose(dependency);
                m_charZeroPos.Dispose(dependency);
            }
        }
    }
}