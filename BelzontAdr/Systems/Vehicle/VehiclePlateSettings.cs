using Belzont.Utils;
using Colossal;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;
using Game.Simulation;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Hash128 = Colossal.Hash128;


namespace BelzontAdr
{
    public class VehiclePlateSettings : ISerializable
    {
        private const string ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NUM = "0123456789";
        private const string ALPHA_NUM = NUM + ALPHA;

        private const uint CURRENT_VERSION = 0;
        private string[] m_lettersAllowed;
        private uint m_flagsLocal;
        private uint m_flagsCarNumber;
        private uint m_randomSeed;
        private uint m_flagsRandomized;
        private int m_monthsFromEpochOffset;
        private uint m_serialIncrementEachMonth;
        private Hash128 checksum;
        private string[] m_lettersAllowedProcessed;


        public bool IsDirty { get; private set; }
        public void Clear() => IsDirty = false;

        public int MonthsFromEpochOffset
        {
            get => m_monthsFromEpochOffset; set
            {
                m_monthsFromEpochOffset = value;
                IsDirty = true;
            }
        }
        public uint SerialIncrementEachMonth
        {
            get => m_serialIncrementEachMonth; set
            {
                m_serialIncrementEachMonth = value;
                IsDirty = true;
            }
        }

        public uint FlagsLocal
        {
            get => m_flagsLocal; set
            {
                m_flagsLocal = value;
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
        public uint FlagsRandomized
        {
            get => m_flagsRandomized; set
            {
                m_flagsRandomized = value;
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
            checksum = GuidUtils.Create(default, m_lettersAllowed.SelectMany(x => x.SelectMany(y => y.ToBytes())).Union(m_flagsLocal.ToBytes()).Union(m_flagsCarNumber.ToBytes()).Union(m_randomSeed.ToBytes()).ToArray());
            m_lettersAllowedProcessed = new string[m_lettersAllowed.Length];
            for (int i = 0; i < m_lettersAllowed.Length; i++)
            {
                m_lettersAllowedProcessed[i] = ((1 << i) & m_flagsRandomized) != 0
                    ? string.Join("", Shuffle(m_lettersAllowed[i].SplitIntoCharacters().ToArray(), m_randomSeed + (uint)i))
                    : m_lettersAllowed[i];
            }
            IsDirty = true;
        }

        public void GenerateNewSeed()
        {
            m_randomSeed = (uint)new Random().Next();
            UpdateChecksum();
        }

        private static T[] Shuffle<T>(T[] list, uint seed)
        {
            var rng = Unity.Mathematics.Random.CreateFromIndex(seed);
            int n = list.Length;

            while (--n > 0)
            {
                int k = rng.NextInt(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }

            return list;
        }


        public static VehiclePlateSettings CreateRoadVehicleDefault(TimeSystem timeSystem)
        {
            var alpha = ALPHA;
            var num = NUM;
            var result = new VehiclePlateSettings
            {
                m_monthsFromEpochOffset = timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                m_serialIncrementEachMonth = 10000,
                m_lettersAllowed = new string[]
                    {
                        alpha, alpha, alpha, num, num, num, num
                    },
                m_flagsLocal = 0b1111111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateAirVehicleDefault(TimeSystem timeSystem)
        {
            var alpha_num = ALPHA_NUM;
            var result = new VehiclePlateSettings
            {
                m_monthsFromEpochOffset = timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                m_serialIncrementEachMonth = 36 * 36 * 36,
                m_lettersAllowed = new string[]
                    {
                        alpha_num,alpha_num,"-",alpha_num,alpha_num,alpha_num,alpha_num
                    },
                m_flagsLocal = 0b001111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateWaterVehicleDefault(TimeSystem timeSystem)
        {
            var alpha = ALPHA;
            var alphaOpt = (" " + ALPHA);
            var num = NUM;
            var result = new VehiclePlateSettings
            {
                m_monthsFromEpochOffset = timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                m_serialIncrementEachMonth = 10000,
                m_lettersAllowed = new string[]
                {
                    alpha,
                    alpha,
                    alphaOpt,
                    alphaOpt,
                    "-",
                    num,
                    num,
                    num,
                    num
                },
                m_flagsLocal = 0b1111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateRailVehicleDefault(TimeSystem timeSystem)
        {
            var alpha = ALPHA;
            var alphaNum = ALPHA_NUM;
            var result = new VehiclePlateSettings
            {
                m_monthsFromEpochOffset = timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                m_serialIncrementEachMonth = 36 * 36,
                m_lettersAllowed = new string[]
                    {
                        alpha,alphaNum,alphaNum,alphaNum,alphaNum,"-",alphaNum
                    },
                m_flagsLocal = 0b11100,
                m_flagsCarNumber = 0b1,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out m_flagsLocal);
            reader.Read(out m_flagsCarNumber);
            reader.Read(out m_flagsRandomized);
            reader.Read(out m_randomSeed);
            reader.Read(out m_monthsFromEpochOffset);
            reader.Read(out m_serialIncrementEachMonth);
            reader.Read(out int length);
            m_lettersAllowed = new string[length];
            for (int i = 0; i < length; i++)
            {
                reader.Read(out string letters);
                m_lettersAllowed[i] = letters;
            }
            UpdateChecksum();
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_flagsLocal);
            writer.Write(m_flagsCarNumber);
            writer.Write(m_flagsRandomized);
            writer.Write(m_randomSeed);
            writer.Write(m_monthsFromEpochOffset);
            writer.Write(m_serialIncrementEachMonth);
            writer.Write(m_lettersAllowed.Length);
            for (int i = 0; i < m_lettersAllowed.Length; i++)
            {
                writer.Write(m_lettersAllowed[i]);
            }
        }

        public SafeStruct ForBurstJob => new()
        {
            Checksum = checksum,
            m_flagsCarNumber = m_flagsCarNumber,
            m_flagsLocal = m_flagsLocal,
            m_monthsFromEpochOffset = m_monthsFromEpochOffset,
            m_serialIncrementEachMonth = m_serialIncrementEachMonth,
            m_charZeroPos = new NativeArray<int>(m_lettersAllowedProcessed.Select((x, i) => m_lettersAllowedProcessed.Take(i).Sum(y => y.Length)).ToArray(), Allocator.TempJob),
            m_lettersAllowedProcessed = string.Join("", m_lettersAllowedProcessed).ToUshortNativeArray(Allocator.TempJob),

        };

        public unsafe struct SafeStruct
        {
            internal uint m_flagsLocal;
            internal uint m_flagsCarNumber;
            internal NativeArray<ushort> m_lettersAllowedProcessed;
            internal int m_monthsFromEpochOffset;
            internal uint m_serialIncrementEachMonth;
            public NativeArray<int> m_charZeroPos;

            public Hash128 Checksum;
            public readonly FixedString32Bytes GetPlateFor(ulong regionalCode, ulong localSerial, int monthsFromEpoch, int compositionNumber = 1)
            {
                var output = new NativeArray<Unicode.Rune>(m_charZeroPos.Length, Allocator.Temp);
                uint currentFlag = 1;
                int currentIdx = m_charZeroPos.Length - 1;
                unchecked
                {
                    localSerial += (ulong)(Math.Max(0, monthsFromEpoch - m_monthsFromEpochOffset) * m_serialIncrementEachMonth);
                }
                do
                {
                    var charZeroPos = (ulong)m_charZeroPos[currentIdx];
                    var numberChars = (ulong)(currentIdx == m_charZeroPos.Length - 1 ? m_lettersAllowedProcessed.Length : m_charZeroPos[currentIdx + 1]) - charZeroPos;
                    if ((m_flagsCarNumber & currentFlag) != 0)
                    {
                        output[currentIdx] = new Unicode.Rune(m_lettersAllowedProcessed[(int)((ulong)compositionNumber % numberChars + charZeroPos)]);
                        compositionNumber /= (int)numberChars;
                    }
                    else if ((m_flagsLocal & currentFlag) != 0)
                    {
                        output[currentIdx] = new Unicode.Rune(m_lettersAllowedProcessed[(int)((localSerial % numberChars) + charZeroPos)]);
                        localSerial /= numberChars;
                    }
                    else
                    {
                        output[currentIdx] = new Unicode.Rune(m_lettersAllowedProcessed[(int)(regionalCode % numberChars + charZeroPos)]);
                        regionalCode /= numberChars;
                    }
                    currentFlag <<= 1;
                } while (--currentIdx >= 0);
                var result = new FixedString32Bytes();
                for (int i = 0; i < output.Length; i++)
                {
                    if (output[i].value != 0) result.Append(output[i]);
                }
                output.Dispose();
                return result;
            }

            internal void Dispose(JobHandle dependency)
            {
                m_lettersAllowedProcessed.Dispose(dependency);
                m_charZeroPos.Dispose(dependency);
            }
        }
    }
}