using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
          7 
        1 T 5         TREES AROUND
          3 
*/

namespace FireSimulator
{
    class Tree
    {
        public static int BurningTime = 5; // Duration of burning in frames / Czas palenia się drzewa w klatkach
        private static Random random = new Random();
        private int burningDuration = 0;

        public bool Burning = false;
        public bool Burned = false;
        public float BurningChance = 0f;

        public void CalculateNextFrame(Tree[] treesAround, WindDirection direction)
        {
            if(!Burned) calculateChance(treesAround, direction);
        }

        public void NextFrame()
        {
            if (random.NextDouble() <= BurningChance) Burning = true;
            if (Burning) if (++burningDuration >= BurningTime) Burned = true;
        }

        public void NextFrame(object a)
        {
            NextFrame();
        }

        private void calculateChance(Tree[] treesAround, WindDirection direction)
        {
            BurningChance = 0f;
            for (int i = 0; i < treesAround.Length; ++i)
            {
                if (treesAround[i] != null)
                    if (treesAround[i].Burning)
                        BurningChance += .05f;
            }

            try
            {
                if ((direction == WindDirection.N && treesAround[7].Burning)
                || (direction == WindDirection.S && treesAround[3].Burning)
                || (direction == WindDirection.W && treesAround[1].Burning)
                || (direction == WindDirection.E && treesAround[5].Burning))
                    BurningChance += .5f;
            }
            catch (NullReferenceException ex) { }
        }

        public Char ToChar()
        {
            if (Burned) return '=';
            if (Burning) return 'W';
            else return 'T';
        }
    }
}
