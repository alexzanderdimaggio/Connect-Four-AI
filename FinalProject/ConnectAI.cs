using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ConnectFourAITesting
{
    public class ConnectAI
    {
        int[] columnOrder = new int[Position.Width];
        public long nodeCount;

        TranspositionTable transTable;
        public int Seconds = 5;
        //public const int weakTurns = 5;


        public static uint log2(ulong n)
        {
            return n <= 1 ? 0 : log2(n / 2) + 1;
        }

        public ConnectAI()
        {
            transTable = new TranspositionTable(8388593);
            reset();
            for (int i = 0; i < Position.Width; i++)
                columnOrder[i] = Position.Width / 2 + (1 - 2 * (i % 2)) * (i + 1) / 2; // initialize the column exploration order, starting with center columns
        }

        void reset()
        {
            nodeCount = 0;
            transTable.reset();
        }

        public int Solve(Position P, Stopwatch sw)
        {

            if (P.canWinNext()) // check if win in one move as the Negamax function does not support this case.
                return (Position.Width * Position.Height + 1 - P.nbMoves()) / 2;
            int min = -(Position.Width * Position.Height - P.nbMoves()) / 2;
            int max = (Position.Width * Position.Height + 1 - P.nbMoves()) / 2;
            //if (P.nbMoves() <= weakTurns)
            //{
            //    min = -1;
            //    max = 1;
            //}
            while (min < max && (sw.ElapsedMilliseconds / 1000) < Seconds)
            {                    // iteratively narrow the min-max exploration window
                int med = min + (max - min) / 2;
                if (med <= 0 && min / 2 < med) med = min / 2;
                else if (med >= 0 && max / 2 > med) med = max / 2;
                int r = Negamax(P, med, med + 1, sw);   // use a null depth window to know if the actual score is greater or smaller than med
                if (r <= med) max = r;
                else min = r;              
            }
            return min;
        }

        public int AITurn(Position p)
        {
            if (p.canWinNext())
            {
                for (int i = 0; i < Position.Width; i++)
                {
                    if (p.CanPlay(columnOrder[i]) && p.IsWinningMove(columnOrder[i]))
                    {
                        return columnOrder[i];
                    }
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int[] columnVals;
            columnVals = Move(p, sw);
            sw.Stop();
            int maxScore = Position.MIN_SCORE - 1;
            List<int> playList = new List<int>();
            for (int i = 0; i < Position.Width; i++)
            {
                if (columnVals[i] > maxScore)
                {
                    maxScore = columnVals[i];
                }
            }
            for (int i = 0; i < Position.Width; i++)
            {
                if (columnVals[i] == maxScore) playList.Add(i);
            }
            Random rng = new Random();
            int x = rng.Next(playList.Count);
            return playList[x];
        }


        public int[] Move(Position P, Stopwatch sw)
        {
            bool[] colExplored = new bool[Position.Width];
            int[] colVals = new int[Position.Width];

            if (P.nbMoves() == 0)
            {
                for (int i = 0; i < Position.Width; i++)
                {
                    if (!colExplored[columnOrder[i]]) colVals[columnOrder[i]] -= (i + 1) / 2;
                }
                return colVals;
            }
            else if (P.nbMoves() == 1)
            {
                switch (log2(P.mask) / 7)
                {
                    case 0:
                        colVals[0] = -1;
                        colVals[1] = 2;
                        colVals[2] = 1;
                        colVals[3] = 2;
                        colVals[4] = -1;
                        colVals[5] = 1;
                        colVals[6] = -2;
                        break;
                    case 1:
                        colVals[0] = -2;
                        colVals[1] = 0;
                        colVals[2] = 1;
                        colVals[3] = 0;
                        colVals[4] = -2;
                        colVals[5] = -2;
                        colVals[6] = -3;
                        break;
                    case 2:
                        colVals[0] = -2;
                        colVals[1] = -2;
                        colVals[2] = 0;
                        colVals[3] = 0;
                        colVals[4] = 0;
                        colVals[5] = 0;
                        colVals[6] = -3;
                        break;
                    case 3:
                        colVals[0] = -4;
                        colVals[1] = -2;
                        colVals[2] = -2;
                        colVals[3] = -1;
                        colVals[4] = -2;
                        colVals[5] = -2;
                        colVals[6] = -4;
                        break;
                    case 4:
                        colVals[6] = -2;
                        colVals[5] = -2;
                        colVals[4] = 0;
                        colVals[3] = 0;
                        colVals[2] = 0;
                        colVals[1] = 0;
                        colVals[0] = -3;
                        break;
                    case 5:
                        colVals[6] = -2;
                        colVals[5] = 0;
                        colVals[4] = 1;
                        colVals[3] = 0;
                        colVals[2] = -2;
                        colVals[1] = -2;
                        colVals[0] = -3;
                        break;
                    case 6:
                        colVals[6] = -1;
                        colVals[5] = 2;
                        colVals[4] = 1;
                        colVals[3] = 2;
                        colVals[2] = -1;
                        colVals[1] = 1;
                        colVals[0] = -2;
                        break;
                }
                return colVals;
            }
            else if (P.nbMoves() < 12)
            {
                Seconds = 5;
            }
            else
            {
                Seconds = 3;
            }

            if (P.nbMoves() < 16)
            {
                for (int i = 0; i < Position.Width; i++)
                {
                    if (P.CanPlay(i))
                    {
                        P.Play(i);
                        for (int j = 0; j < Position.Width; j++)
                        {
                            if (P.CanPlay(j))
                            {
                                P.Play(j);
                                if (P.possibleNonLoosingMoves() == 0)
                                {
                                    colVals[i] = -(Position.Width * Position.Height - P.nbMoves() - 2) / 2;
                                    colExplored[i] = true;
                                }
                                P.Undo();
                            }
                        }
                        P.Undo();
                    }
                }
            }

            nodeCount = 0;
            
            if (P.canWinNext())
            {
                for (int i = 0; i < Position.Width; i++)
                {
                    if (P.CanPlay(columnOrder[i]) && P.IsWinningMove(columnOrder[i]))
                    {
                        colVals[columnOrder[i]] = (Position.Width * Position.Height - 1 - P.nbMoves()) / 2;
                        colExplored[columnOrder[i]] = true;
                    }
                }
            }

            for (int i = 0; i < Position.Width; i++)
            {
                if (P.CanPlay(columnOrder[i]) && !colExplored[columnOrder[i]] && (sw.ElapsedMilliseconds / 1000) < Seconds)
                {
                    P.Play(columnOrder[i]);

                    int moveVal = -Solve(P, sw);
                    P.Undo();
                    colVals[columnOrder[i]] = moveVal;
                    colExplored[columnOrder[i]] = true;
                }
                else if (!P.CanPlay(columnOrder[i]))
                {
                    colVals[columnOrder[i]] = Position.MIN_SCORE - 1;
                    colExplored[columnOrder[i]] = true;
                }
            }

            for (int i = 0; i < Position.Width; i++)
            {
                if (!colExplored[columnOrder[i]]) colVals[columnOrder[i]] -= (i - 1) / 2;
            }

            return colVals;
        }


        //public int Negamax(Position P)
        //{
        //    if (P.nbMoves() == Position.Width * Position.Height) // check for draw game
        //        return 0;
        //
        //    for (int x = 0; x < Position.Width; x++) // check if current player can win next move
        //        if (P.CanPlay(x) && P.IsWinningMove(x))
        //            return (Position.Width * Position.Height + 1 - P.nbMoves()) / 2;
        //
        //
        //    int bestScore = -Position.Width * Position.Height; // init the best possible score with a lower bound of score.
        //
        //    for (int x = 0; x < Position.Width; x++) // compute the score of all possible next move and keep the best one
        //        if (P.CanPlay(x))
        //        {
        //            Position P2 = new Position(P);
        //            P2.Play(x);               // It's opponent turn in P2 position after current player plays x column.
        //            int score = -Negamax(P2); // If current player plays col x, his score will be the opposite of opponent's score after playing col x
        //            if (score > bestScore) bestScore = score; // keep track of best possible score so far.
        //        }
        //
        //    return bestScore;
        //}

        public int Negamax(Position P, int alpha, int beta, Stopwatch sw)
        {

            if (alpha >= beta) throw (new Exception());
            if (P.canWinNext()) throw (new Exception());
            nodeCount++;

            ulong possible = P.possibleNonLoosingMoves();
            if (possible == 0)     // if no possible non losing move, opponent wins next move
                return -(Position.Width * Position.Height - P.nbMoves()) / 2;

            if (P.nbMoves() >= Position.Width * Position.Height) // check for draw game
                return 0;

            int min = -(Position.Width * Position.Height - 2 - P.nbMoves()) / 2;
            if (alpha < min)
            {
                alpha = min;                     // there is no need to keep beta above our max possible score.
                if (alpha >= beta) return alpha;  // prune the exploration if the [alpha;beta] window is empty.
            }

            int max = (Position.Width * Position.Height - 1 - P.nbMoves()) / 2;   // upper bound of our score as we cannot win immediately
            

            if (beta > max)
            {
                beta = max;                     // there is no need to keep beta above our max possible score.
                if (alpha >= beta) return beta;  // prune the exploration if the [alpha;beta] window is empty.
            }

            ulong key = P.key();
            int val = transTable.get(key);
            if (val != 0)
            {
                if (val > Position.MAX_SCORE - Position.MIN_SCORE + 1)
                { // we have an lower bound
                    min = val + 2 * Position.MIN_SCORE - Position.MAX_SCORE - 2;
                    if (alpha < min)
                    {
                        alpha = min;                     // there is no need to keep beta above our max possible score.
                        if (alpha >= beta) return alpha;  // prune the exploration if the [alpha;beta] window is empty.
                    }
                }
                else
                { // we have an upper bound
                    max = val + Position.MIN_SCORE - 1;
                    if (beta > max)
                    {
                        beta = max;                     // there is no need to keep beta above our max possible score.
                        if (alpha >= beta) return beta;  // prune the exploration if the [alpha;beta] window is empty.
                    }
                }
            }
            //for (int x = 0; x < Position.Width; x++) // compute the score of all possible next move and keep the best one
            //    if ((next & Position.column_mask(columnOrder[x])) != 0)
            //    {
            //        P.Play(columnOrder[x]);               // It's opponent turn in P2 position after current player plays x column.
            //        int score = -Negamax(P, -beta, -alpha); // explore opponent's score within [-beta;-alpha] windows:
            //        P.Undo();                                      
            //            // no need to have good precision for score better than beta (opponent's score worse than -beta)
            //        // no need to check for score worse than alpha (opponent's score worse better than -alpha)
            //
            //        if (score >= beta) return score;  // prune the exploration if we find a possible move better than what we were looking for.
            //        if (score > alpha) alpha = score; // reduce the [alpha;beta] window for next exploration, as we only 
            //                                          // need to search for a position that is better than the best so far.
            //    }

            MoveSorter moves = new MoveSorter();

            ulong move;
            for (int i = Position.Width - 1; i >= 0; i--)
                if ((move = (possible & Position.column_mask(columnOrder[i]))) != 0)
                    moves.add(move, P.moveScore(move));

            ulong next;
            while ((next = moves.getNext()) != 0 && (sw.ElapsedMilliseconds / 1000) < Seconds)
            {
                P.Play(next);  // It's opponent turn in P2 position after current player plays x column.
                int score = -Negamax(P, -beta, -alpha, sw); // explore opponent's score within [-beta;-alpha] windows:
                P.Undo();
                // no need to have good precision for score better than beta (opponent's score worse than -beta)
                // no need to check for score worse than alpha (opponent's score worse better than -alpha)

                if (score >= beta)
                {
                    transTable.put(key, (byte)(score + Position.MAX_SCORE - 2 * Position.MIN_SCORE + 2)); // save the lower bound of the position
                    return score;  // prune the exploration if we find a possible move better than what we were looking for.
                }
                // prune the exploration if we find a possible move better than what we were looking for.
                if (score > alpha) { alpha = score; }// reduce the [alpha;beta] window for next exploration, as we only 
                // need to search for a position that is better than the best so far.
            }
            //if (sw.ElapsedMilliseconds / 1000 < Seconds)
            //{
                transTable.put(key, (byte)(alpha - Position.MIN_SCORE + 1));
            //}

            return alpha;
        }
    }
}
