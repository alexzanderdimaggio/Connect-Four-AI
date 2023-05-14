using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourAITesting
{
    public class MoveSorter
    {
        public uint size;

        struct E
        {
            public ulong move { get; set; }
            public int score { get; set; }
        }
        E[] entries = new E[Position.Width];

        public MoveSorter()
        {
            size = 0;
        }

        public void add(ulong move, int score)
        {
            int pos = (int)size++;
            for (; pos != 0 && entries[pos - 1].score > score; --pos)
                entries[pos] = entries[pos - 1];
            E e = new E();
            e.move = move;
            e.score = score;
            entries[pos] = e;
        }

        /*
         * Get next move 
         * @return next remaining move with max score and remove it from the container.
         * If no more move is available return 0
         */
        public ulong getNext()
        {
            if (size != 0)
                return entries[--size].move;
            else
                return 0;
        }

        /*
         * reset (empty) the container
         */
        public void reset()
        {
            size = 0;
        }

        /*
         * Build an empty container
         */
        
    }
}
