using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ConnectFourAITesting
{
    

    public class TranspositionTable
    {

        struct Entry
        {
            public uint key1 { get; set; } // use 56-bit keys
            public ushort key2 { get; set; }
            public byte key3 { get; set; }
            public byte val { get; set; }       // use 8-bit values
        };

        public static ulong ConvertKeyTo64(uint key1, ushort key2, byte key3)
        {
            ulong key = key1;
            key = (key << 16) + key2;
            key = (key << 8) + key3;
            return key;
        }

        public static uint Convert64ToKey1(ulong key)
        {
            return (uint)(key >> 24);
        }

        public static ushort Convert64ToKey2(ulong key)
        {
            return (ushort)((key << 40) >> 48);
        }

        public static byte Convert64ToKey3(ulong key)
        {
            return (byte)((key << 56) >> 56);
        }

        List<Entry> T = new List<Entry>();

        private int index(ulong key)
        {
            return (int)(key % (ulong)(T.Count()));
        }

        public TranspositionTable(uint size) //: T(size)
        {
            // Setup struct with size
            T = new List<Entry>((int)size);
            for (int i = 0; i < (int)size; i++)
            {
                T.Add(new Entry());
            }
            if(size <= 0) throw new Exception();
        }

        /*
         * Empty the Transition Table.
         */
        public void reset()
        { 
            // fill everything with 0, because 0 value means missing data
            for (int i = 0; i < T.Count; i++)
            {
                Entry e = T[i];
                e.key1 = 0;
                e.key2 = 0;
                e.key3 = 0;
                e.val = 0;
                T[i] = e;
            } 
            //memset(&T[0], 0, T.size()*sizeof(Entry));
        }

        /**
         * Store a value for a given key
         * @param key: 56-bit key
         * @param value: non-null 8-bit value. null (0) value are used to encode missing data.
         */
        public void put(ulong key, byte val)
        {
            if(key >= (1L << 56)) throw new Exception();
            int i = index(key); // compute the index position
            Entry e = T[i];
            e.key1 = Convert64ToKey1(key);
            e.key2 = Convert64ToKey2(key);
            e.key3 = Convert64ToKey3(key);
            e.val = val;
            T[i] = e;
        }


        /** 
         * Get the value of a key
         * @param key
         * @return 8-bit value associated with the key if present, 0 otherwise.
         */
        public byte get(ulong key)
        {
            if (key >= (1L << 56)) throw new Exception();
            int i = index(key);  // compute the index position
            if (ConvertKeyTo64(T[i].key1, T[i].key2, T[i].key3) == key)
                return T[i].val;            // and return value if key matches
            else
                return 0;                   // or 0 if missing entry
        }
    } 
}
