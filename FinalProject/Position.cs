using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourAITesting
{
    public class Position
    {
        public const int Width = 7;
        public const int Height = 6;
        public const int MIN_SCORE = -(Width * Height) / 2 + 3;
        public const int MAX_SCORE = (Width * Height + 1) / 2 - 3;

        int moves = 0;
        public ulong current_position;
        public ulong mask;
        Stack<ulong> maskChanges = new Stack<ulong>();
        // 0, p1, p2
        //public int[,] board = new int[Width, Height];
        //public int[] height = new int[Width];

        public Position()
        {
            current_position = 0;
            mask = 0;
            moves = 0;
        }

        public Position(string boardstate)
        {
            current_position = 0;
            mask = 0;
            moves = 0;
            SetBoard(boardstate);
        }

        public Position GetPos()
        {
            Position p = new Position();
            p.moves = this.moves;
            p.current_position = this.current_position;
            p.mask = this.mask;
            return p;
        }

        public ulong bottom()
        {
            ulong bottom = 1;
            bottom |= (ulong)Math.Pow(2, 7);
            bottom |= (ulong)Math.Pow(2, 14);
            bottom |= (ulong)Math.Pow(2, 21);
            bottom |= (ulong)Math.Pow(2, 28);
            bottom |= (ulong)Math.Pow(2, 35);
            bottom |= (ulong)Math.Pow(2, 42);
            return bottom;
        }

        public int nbMoves() { return moves; }

        public ulong key()
        {
            return current_position + mask;
        }

        public void SetBoard(string boardState)
        {
            for (int i = 0; i < boardState.Length; i++)
            {
                int col = (int)Char.GetNumericValue(boardState[i]);
                col -= 1;
                if (CanPlay(col)) Play(col);
                else throw new Exception();
            }
        }

        public bool CanPlay(int col)
        {
            return (mask & top_mask_col(col)) == 0;
        }

        public void Play(int col)
        {
            current_position ^= mask;
            ulong oldmask = mask;
            mask |= mask + bottom_mask_col(col);
            maskChanges.Push(oldmask ^ mask);
            moves++;
        }

        public void Play(ulong move)
        {
            current_position ^= mask;
            ulong oldmask = mask;
            mask |= move;
            maskChanges.Push(oldmask ^ mask);
            moves++;
        }


        public void Undo()
        {
            mask -= maskChanges.Pop();
            current_position ^= mask;
            moves--;
            // Remove by 1 and AND it out
        }

        public bool canWinNext()
        {
            return (winning_position() & possible()) != 0;
        }

        public ulong possibleNonLoosingMoves() {
            //if(canWinNext())throw new Exception();
            ulong possible_mask = possible();
            ulong opponent_win = opponent_winning_position();
            ulong forced_moves = possible_mask & opponent_win;
            if(forced_moves != 0) {
                if((forced_moves & (forced_moves - 1)) != 0) // check if there is more than one forced move
                    return 0;                           // the opponnent has two winning moves and you cannot stop him
                else possible_mask = forced_moves;    // enforce to play the single forced move
            }
            return possible_mask & ~(opponent_win >> 1);  // avoid to play below an opponent winning spot
        }

        public int moveScore(ulong move)
        {
            return popcount(compute_winning_position(current_position | move, mask));
        }

        public bool IsWinningMove(int col)
        {
            return (winning_position() & possible() & column_mask(col)) != 0;
        }

        /*
       * Return a bitmask of the possible winning positions for the current player
       */
        ulong winning_position()
        {
            return compute_winning_position(current_position, mask);
        }

        /*
         * Return a bitmask of the possible winning positions for the opponent
         */
        ulong opponent_winning_position()
        {
            return compute_winning_position(current_position ^ mask, mask);
        }

        ulong possible()
        {
            return (mask + bottom_mask) & board_mask;
        }

        static int popcount(ulong m)
        {
            int c = 0;
            for (c = 0; m !=0; c++) m &= m - 1;
            return c;
        }

        static ulong compute_winning_position(ulong position, ulong mask)
        {
            // vertical;
            ulong r = (position << 1) & (position << 2) & (position << 3);

            //horizontal
            ulong p = (position << (Height + 1)) & (position << 2 * (Height + 1));
            r |= p & (position << 3 * (Height + 1));
            r |= p & (position >> (Height + 1));
            p = (position >> (Height + 1)) & (position >> 2 * (Height + 1));
            r |= p & (position << (Height + 1));
            r |= p & (position >> 3 * (Height + 1));

            //diagonal 1
            p = (position << Height) & (position << 2 * Height);
            r |= p & (position << 3 * Height);
            r |= p & (position >> Height);
            p = (position >> Height) & (position >> 2 * Height);
            r |= p & (position << Height);
            r |= p & (position >> 3 * Height);

            //diagonal 2
            p = (position << (Height + 2)) & (position << 2 * (Height + 2));
            r |= p & (position << 3 * (Height + 2));
            r |= p & (position >> (Height + 2));
            p = (position >> (Height + 2)) & (position >> 2 * (Height + 2));
            r |= p & (position << (Height + 2));
            r |= p & (position >> 3 * (Height + 2));

            return r & (board_mask ^ mask);
        }

        public static ulong bottom(int width, int height)
        {
            return width == 0 ? 0 : bottom(width - 1, height) | (ulong)1 << (width - 1) * (height + 1);
        }

        static ulong bottom_mask = bottom(Width, Height);
        static ulong board_mask = bottom_mask * (((ulong)1 << Height)-1);

          // return a bitmask containg a single 1 corresponding to the top cel of a given column
        static ulong top_mask_col(int col)
        {
            return (ulong)1 << ((Height - 1) + col * (Height + 1));
        }

        // return a bitmask containg a single 1 corresponding to the bottom cell of a given column
        static ulong bottom_mask_col(int col)
        {
            return (ulong)1 << col * (Height + 1);
        }


        // return a bitmask 1 on all the cells of a given column
        public static ulong column_mask(int col)
        {
            return (((ulong)1 << Height) - 1) << col * (Height + 1);
        }

        //public bool CanPlay(int col)
        //{
        //    return height[col] < Height;
        //}
        //
        //public void Play(int col)
        //{
        //    board[col,height[col]] = 1 + moves % 2;
        //    height[col]++;
        //    moves++;
        //}
        //
        //public void Undo(int col)
        //{
        //    board[col, height[col] - 1] = 0;
        //    height[col]--;
        //    moves--;
        //}
        //
        //public bool IsWinningMove(int col)
        //{
        //    int current_player = 1 + moves % 2;
        //    // check for vertical alignments
        //    if (height[col] >= 3
        //        && board[col,height[col] - 1] == current_player
        //        && board[col,height[col] - 2] == current_player
        //        && board[col,height[col] - 3] == current_player)
        //        return true;
        //
        //    for (int dy = -1; dy <= 1; dy++)
        //    {    // Iterate on horizontal (dy = 0) or two diagonal directions (dy = -1 or dy = 1).
        //        int nb = 0;                       // counter of the number of stones of current player surronding the played stone in tested direction.
        //        for (int dx = -1; dx <= 1; dx += 2) // count continuous stones of current player on the left, then right of the played column.
        //            for (int x = col + dx, y = height[col] + dx * dy; x >= 0 
        //                && x < Width && y >= 0 && y < Height && board[x,y] == current_player; nb++)
        //            {
        //                x += dx;
        //                y += dx * dy;
        //            }
        //        if (nb >= 3) return true;
        //    }
        //    return false;
        //}

    }
}
