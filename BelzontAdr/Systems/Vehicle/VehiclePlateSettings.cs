using Belzont.Utils;
using Colossal;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;
using System;
using System.Linq;
using Unity.Collections;
using Hash128 = Colossal.Hash128;


namespace BelzontAdr
{
    public struct VehiclePlateSettings : ISerializable
    {
        private const string ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NUM = "0123456789";
        private const string ALPHA_NUM = NUM + ALPHA;

        private const uint CURRENT_VERSION = 0;
        private NativeList<NativeArray<ushort>> m_lettersAllowed;
        private uint m_flagsLocal;
        private uint m_flagsCarNumber;
        private uint m_randomSeed;
        private uint m_flagsRandomized;
        private int m_monthsFromEpochOffset;
        private uint m_serialIncrementEachMonth;
        private Hash128 checksum;
        private NativeList<NativeArray<ushort>> m_lettersAllowedProcessed;


        public bool IsDirty { get; private set; }
        public void Clear() => IsDirty = false;

        public int MonthsFromEpochOffset
        {
            readonly get => m_monthsFromEpochOffset; set
            {
                m_monthsFromEpochOffset = value;
                IsDirty = true;
            }
        }
        public uint SerialIncrementEachMonth
        {
            readonly get => m_serialIncrementEachMonth; set
            {
                m_serialIncrementEachMonth = value;
                IsDirty = true;
            }
        }

        public uint FlagsLocal
        {
            readonly get => m_flagsLocal; set
            {
                m_flagsLocal = value;
                UpdateChecksum();
            }
        }
        public uint FlagsCarNumber
        {
            readonly get => m_flagsCarNumber; set
            {
                m_flagsCarNumber = value;
                UpdateChecksum();
            }
        }
        public uint FlagsRandomized
        {
            readonly get => m_flagsRandomized; set
            {
                m_flagsRandomized = value;
                UpdateChecksum();
            }
        }

        public readonly Hash128 Checksum => checksum;

        public string[] LettersAllowed
        {
            readonly get => m_lettersAllowed.IsCreated ? m_lettersAllowed.AsArray().ToArray().Select(x => string.Join("", x.ToArray().Select(x => char.ConvertFromUtf32(x)))).ToArray() : new string[0];
            set
            {
                SetDigitsQuantity_internal(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    SetCharSequenceAtPosition_internal(i, value[i]);
                }
                UpdateChecksum();
            }
        }

        private void UpdateChecksum()
        {
            var listArr = m_lettersAllowed.ToArray(Allocator.Temp);
            checksum = GuidUtils.Create(default, listArr.ToArray().SelectMany(x => x.SelectMany(y => y.ToBytes())).Union(m_flagsLocal.ToBytes()).Union(m_flagsCarNumber.ToBytes()).Union(m_randomSeed.ToBytes()).ToArray());
            if (m_lettersAllowedProcessed.IsCreated)
            {
                for (int i = 0; i < m_lettersAllowedProcessed.Length; i++)
                {
                    m_lettersAllowedProcessed[i].Dispose();
                }
                m_lettersAllowedProcessed.Dispose();
            }
            m_lettersAllowedProcessed = new NativeList<NativeArray<ushort>>(Allocator.Persistent);
            for (int i = 0; i < m_lettersAllowed.Length; i++)
            {
                if (((1 << i) & m_flagsRandomized) != 0)
                {
                    m_lettersAllowedProcessed.Add(new NativeArray<ushort>(Shuffle(m_lettersAllowed[i].ToArray(), m_randomSeed + (uint)i), Allocator.Persistent));
                }
                else
                {
                    m_lettersAllowedProcessed.Add(m_lettersAllowed[i]);
                }
            }
            listArr.Dispose();
            IsDirty = true;
        }

        public int SetDigitsQuantity(int quantity)
        {
            var result = SetDigitsQuantity_internal(quantity);
            if (result == 0) UpdateChecksum();
            return result;
        }

        private int SetDigitsQuantity_internal(int quantity)
        {
            if (quantity < 1 || quantity > 24) return 1;
            if (quantity == m_lettersAllowed.Length) return -1;
            else if (quantity < m_lettersAllowed.Length)
            {
                for (int i = quantity; i < m_lettersAllowed.Length; i++)
                {
                    m_lettersAllowed[i].Dispose();
                }
            }
            else
            {
                var oldCap = m_lettersAllowed.Length;
                m_lettersAllowed.SetCapacity(quantity);

                for (int i = oldCap; i < m_lettersAllowed.Length; i++)
                {
                    m_lettersAllowed[i] = ALPHA.ToUshortNativeArray();
                }
            }
            return 0;
        }

        public int SetCharSequenceAtPosition(int position, string chars)
        {
            var result = SetCharSequenceAtPosition_internal(position, chars);
            if (result == 0) UpdateChecksum();
            return result;
        }

        private int SetCharSequenceAtPosition_internal(int position, string chars)
        {
            if (position < 0 || position > m_lettersAllowed.Length) return 1;
            var effectiveOrder = string.Join("", chars.ToCharArray().GroupBy(x => x).Select(x => x.Key));
            if (effectiveOrder.Length > 60) return 2;
            m_lettersAllowed[position] = effectiveOrder.ToUshortNativeArray();
            return 0;
        }

        public void GenerateNewSeed()
        {
            m_randomSeed = (uint)new Random().Next();
            UpdateChecksum();
        }

        public FixedString32Bytes GetPlateFor(ulong regionalCode, ulong localSerial, int monthsFromEpoch, int compositionNumber = 1)
        {
            var output = new NativeArray<Unicode.Rune>(m_lettersAllowed.Length, Allocator.Temp);
            uint currentFlag = 1;
            int currentIdx = m_lettersAllowed.Length - 1;
            unchecked
            {
                localSerial += (ulong)((monthsFromEpoch - m_monthsFromEpochOffset) * m_serialIncrementEachMonth);
            }
            do
            {
                var currentArr = m_lettersAllowedProcessed[currentIdx];
                if ((m_flagsCarNumber & currentFlag) != 0)
                {
                    output[currentIdx] = new Unicode.Rune(currentArr[compositionNumber % currentArr.Length]);
                    compositionNumber /= currentArr.Length;
                }
                else if ((m_flagsLocal & currentFlag) != 0)
                {
                    output[currentIdx] = new Unicode.Rune(currentArr[(int)(localSerial % (ulong)currentArr.Length)]);
                    localSerial /= (ulong)currentArr.Length;
                }
                else
                {
                    output[currentIdx] = new Unicode.Rune(currentArr[(int)(regionalCode % (ulong)currentArr.Length)]);
                    regionalCode /= (ulong)currentArr.Length;
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
        private static T[] Shuffle<T>(T[] list, uint seed) where T : unmanaged
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


        public static VehiclePlateSettings CreateRoadVehicleDefault()
        {
            var alpha = ALPHA.ToUshortNativeArray();
            var num = NUM.ToUshortNativeArray();
            var result = new VehiclePlateSettings
            {
                m_lettersAllowed = new NativeList<NativeArray<ushort>>(7, Allocator.Persistent)
                    {
                        alpha, alpha, alpha, num, num, num, num
                    },
                m_flagsLocal = 0b1111111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateAirVehicleDefault()
        {
            var alpha_num = ALPHA_NUM.ToUshortNativeArray();
            var result = new VehiclePlateSettings
            {
                m_lettersAllowed = new NativeList<NativeArray<ushort>>(7, Allocator.Persistent)
                    {
                        alpha_num,alpha_num,"-".ToUshortNativeArray(),alpha_num,alpha_num,alpha_num,alpha_num
                    },
                m_flagsLocal = 0b001111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateWaterVehicleDefault()
        {
            var alpha = ALPHA.ToUshortNativeArray();
            var alphaOpt = (" " + ALPHA).ToUshortNativeArray();
            var num = NUM.ToUshortNativeArray();
            var result = new VehiclePlateSettings
            {
                m_lettersAllowed = new NativeList<NativeArray<ushort>>(7, Allocator.Persistent)
                    {
                        alpha,alpha,alphaOpt,alphaOpt,"-".ToUshortNativeArray(),num,num,num,num
                    },
                m_flagsLocal = 0b1111,
                m_randomSeed = (uint)new Random().Next()
            };
            result.UpdateChecksum();
            return result;
        }
        public static VehiclePlateSettings CreateRailVehicleDefault()
        {
            var alpha = ALPHA.ToUshortNativeArray();
            var alphaNum = ALPHA_NUM.ToUshortNativeArray();
            var result = new VehiclePlateSettings
            {
                m_lettersAllowed = new NativeList<NativeArray<ushort>>(7, Allocator.Persistent)
                    {
                        alpha,alphaNum,alphaNum,alphaNum,"-".ToUshortNativeArray(),alphaNum
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
            if (m_lettersAllowed.IsCreated) m_lettersAllowed.Dispose();
            m_lettersAllowed = new(length, Allocator.Persistent);
            for (int i = 0; i < length; i++)
            {
                NativeArray<ushort> letters = default;
                reader.Read(letters);
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
    }
}