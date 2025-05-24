using System.Text;

namespace WFC
{
    internal class Program
    {
        enum EDirection
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        private static readonly int[] Masks = [0b_1000_1000_1000_1000, 0b_0100_0100_0100_0100, 0b_0010_0010_0010_0010, 0b_0001_0001_0001_0001];

        private const int NullTile = 0b_1111;
        private static readonly int[] Tiles =
            [
            0b_0100_1011_0000, 0b_1000_0111_0000, 0b_0001_1110_0000, 0b_0010_1101_0000,
            0b_0101_1010_0000, 0b_1001_0110_0000, 0b_0110_1001_0000, 0b_1010_0101_0000,
            0b_0011_1100_0000, 0b_1100_0011_0000, 0b_0000_1111_0000,
            0b_0111_1000_0000, 0b_1011_0100_0000, 0b_1101_0010_0000, 0b_1110_0001_0000,
            0b_1111_0000_0000
            ];


        private const char NullChar = ' ';
        private static readonly char[] Chars =
            [
            '╩', '╦', '╣', '╠',
            '╝', '╗', '╚', '╔',
            '║', '═', '╬',
            '╨','╥','╡', '╞',
            ' '
            ];

        private static int[,]? map;

        private static Random rng = new Random();

        static void Main(string[] args)
        {
            string[] size = args[0].Split(',');

            if (!int.TryParse(size[0], out int sizeX) || !int.TryParse(size[1], out int sizeY))
                return;

            map = new int[sizeY, sizeX];

            Dictionary<int, char> tileCharPairs = new Dictionary<int, char>
            {
                { NullTile, NullChar }
            };

            for (int i = 0; i < Tiles.Length; i++)
            {
                tileCharPairs.Add(Tiles[i], Chars[i]);
            }

            Console.OutputEncoding = Encoding.UTF8;

            int rounds = 0;

            top:

            Console.CursorVisible = false;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    map[y, x] = NullTile;
                }
            }

            int firstTileY = rng.Next(sizeY);
            int firstTileX = rng.Next(sizeX);
            map[firstTileY, firstTileX] = 0b_1000_0111_0000;
            Console.SetCursorPosition(firstTileX, firstTileY);
            Console.Write('╦');

            while (true)
            {
                List<(int, int, int)> leastEntropyTiles = [(int.MaxValue, -1, -1)];

                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        //check if collapsed
                        if ((NullTile & map[y, x]) == 0)
                            continue;

                        //check up if not on y 0
                        int tileUpCheck = 0;
                        if (y != 0)
                        {
                            tileUpCheck = map[y - 1, x] << 1;
                            tileUpCheck &= Masks[(int)EDirection.UP];
                        }

                        //check down if not on y max
                        int tileDownCheck = 0;
                        if (y != sizeY - 1)
                        {
                            tileDownCheck = map[y + 1, x] >> 1;
                            tileDownCheck &= Masks[(int)EDirection.DOWN];
                        }

                        //check left if not on x 0 
                        int tileLeftCheck = 0;
                        if (x != 0)
                        {
                            tileLeftCheck = map[y, x - 1] << 1;
                            tileLeftCheck &= Masks[(int)EDirection.LEFT];
                        }

                        //check right if not on x max
                        int tileRightCheck = 0;
                        if (x != sizeX - 1)
                        {
                            tileRightCheck = map[y, x + 1] >> 1;
                            tileRightCheck &= Masks[(int)EDirection.RIGHT];
                        }

                        int result = tileUpCheck ^ tileDownCheck ^ tileLeftCheck ^ tileRightCheck;

                        map[y, x] = result;

                        result -= NullTile & result;

                        int entropyNumber = 0;

                        foreach (var tile in tileCharPairs)
                        {
                            if ((tile.Key & result) == result)
                            {
                                entropyNumber++;
                            }
                        }

                        if (entropyNumber < leastEntropyTiles[0].Item1)
                            leastEntropyTiles = [(entropyNumber, y, x)];
                        else if (entropyNumber == leastEntropyTiles[0].Item1)
                            leastEntropyTiles.Add((entropyNumber, y, x));
                    }
                }

                if (leastEntropyTiles[0].Item1 == int.MaxValue)
                    break;

                if (leastEntropyTiles.Count > 1)
                {
                    int y = leastEntropyTiles[rng.Next(1, leastEntropyTiles.Count)].Item2;
                    int x = leastEntropyTiles[rng.Next(1, leastEntropyTiles.Count)].Item3;
                    leastEntropyTiles = [(0, y, x)];
                }

                var leastEntropy = map[leastEntropyTiles[0].Item2, leastEntropyTiles[0].Item3];

                leastEntropy -= NullTile & leastEntropy;

                List<int> possibleTiles = new List<int>();

                foreach (var tile in tileCharPairs)
                {
                    if ((tile.Key & leastEntropy) == leastEntropy)
                    {
                        possibleTiles.Add(tile.Key);
                    }
                }

                int randomPick = NullTile;

                while (randomPick == NullTile)
                    randomPick = possibleTiles[rng.Next(possibleTiles.Count)];

                map[leastEntropyTiles[0].Item2, leastEntropyTiles[0].Item3] = randomPick;

                tileCharPairs.TryGetValue(randomPick, out char print);
                Console.SetCursorPosition(leastEntropyTiles[0].Item3, leastEntropyTiles[0].Item2);
                Console.Write(print);
            }

            Console.ReadLine();
            
            int color = rounds % 15;
            rounds++;
            if (color == 0)
            {
                color = 1;
                rounds++;
            }
            Console.ForegroundColor = (ConsoleColor)color;

            goto top;
        }
    }
}
