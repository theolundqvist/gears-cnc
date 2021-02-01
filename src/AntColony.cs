using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using TMPro;



// Demo of Ant Colony Optimization (ACO) solving a Traveling Salesman Problem (TSP).
// There are many variations of ACO; this is just one approach.
// The problem to solve has a program defined number of cities. We assume that every
// city is connected to every other city. The distance between cities is artificially
// set so that the distance between any two cities is a random value between 1 and 8
// Cities wrap, so if there are 20 cities then D(0,19) = D(19,0).
// Free parameters are alpha, beta, rho, and Q. Hard-coded constants limit min and max
// values of pheromones.

namespace AntColony
{
    public class AntColonyProgram : MonoBehaviour
    {
        private LineRenderer line;
        private TextMeshProUGUI bestPathText;
        private TextMeshProUGUI logText;
        private GcodeGenerator GcodeGen;

        public void LoadReferenses(TextMeshProUGUI BestPathText, TextMeshProUGUI LogText, LineRenderer line)
        {
            this.line = line;
            bestPathText = BestPathText;
            logText = LogText;
        }

        void Start()
        {
            GcodeGen = gameObject.GetComponent<GcodeGenerator>();

            GcodeGen.LoadingPanel.SetActive(true);
            GcodeGen.LoadingPanel.SetActive(false);

            line.gameObject.SetActive(false);
            alpha = 1;
            beta =  2;
            rho = 0.00002;
            Q = 14;
        }
        private System.Random random = new System.Random(0);
        public int alpha;
        public int beta;
        public double rho;
        public double Q;


        private int[] bestTrail;
        public int[] trail{get{return bestTrail;}}
        [HideInInspector]
        public List<Gear> Gears;
        [HideInInspector]
        public bool calcDone = false;


        public void startCalculation(List<Gear> gears)
        {
            logText.text = "";
            logs.Clear();
            StopAllCoroutines();
            Gears = gears;
            calcDone = false;
            StartCoroutine(Calculate());
        }
        private IEnumerator Calculate()
        {

            if(Gears.Count < 4) yield return null;


            int numCities = Gears.Count;
            int numAnts = (Gears.Count < 60) ? 9 : (Gears.Count < 100) ? 4 : 1;
            int maxTime = 1000;

            Log("Begin Ant Colony Optimization demo\n");
            yield return new WaitForSeconds(0.5f);
            Log("Number of nodes in problem = " + numCities);
            yield return new WaitForSeconds(0.1f);
            Log("Number ants = " + numAnts);
            yield return new WaitForSeconds(0.1f);
            Log("Maximum tries = " + maxTime);
            yield return new WaitForSeconds(0.1f);

            yield return new WaitForSeconds(0.3f);
            Log("Alpha = " + alpha);
            Log("Beta = " + beta);
            Log("Rho = " + rho.ToString("F2"));
            Log("Q = " + Q.ToString("F2"));
            yield return new WaitForSeconds(0.3f);

            Log("\nInitialing graph distances...");
            yield return new WaitForSeconds(1f);

            int[][] dists = MakeGraphDistances(numCities);

            int[][] ants = InitAnts(numAnts, numCities);
            // initialize ants to random trails
            //ShowAnts(ants, dists);
            bestTrail = BestTrail(ants, dists);

            line.gameObject.SetActive(true);
            // determine the best initial trail
            double bestLength = Length(bestTrail, dists);
            // the length of the best trail
            Display(bestTrail, bestLength);
            Log("\nBest initial trail length: " + bestLength.ToString("F1") + "");
            yield return new WaitForSeconds(0.3f);
            Log("\n\nInitializing pheromones on trails");
            yield return new WaitForSeconds(0.1f);
            double[][] pheromones = InitPheromones(numCities);

            int time = 0;

            Log("\nEntering UpdateAnts - UpdatePheromones loop");
            yield return new WaitForSeconds(1f);
            Stopwatch watch = new Stopwatch();
            while (time < maxTime)
            {
                UpdateAnts(ants, pheromones, dists);
                yield return new WaitForEndOfFrame();

                UpdatePheromones(pheromones, ants, dists);
                //if(Gears.Count > 10)yield return new WaitForEndOfFrame();

                int[] currBestTrail = BestTrail(ants, dists);
                double currBestLength = Length(currBestTrail, dists);
                //yield return new WaitForEndOfFrame();

                if (currBestLength < bestLength)
                {
                    bestLength = currBestLength;
                    bestTrail = currBestTrail;
                    Display(bestTrail, bestLength);
                    Log("New best length of " + bestLength.ToString("F1") + " found at time " + time);
                    yield return new WaitForSeconds(0.3f);
                    watch.Reset();
                    watch.Start();
                    
                }
               // progressTime.Value = (watch.ElapsedMilliseconds/1400f)*100;
                if(watch.ElapsedMilliseconds > ((Gears.Count < 60) ? 1400 : 2500)) break;
                time += 1;
                
            }
            Log("");
            Console.WriteLine("Time complete");
            Console.WriteLine("Best trail found:");
            Display(bestTrail, bestLength);
            Console.WriteLine("Length of best trail found: " + bestLength.ToString("F1"));
            yield return new WaitForSeconds(1.5f);

            UnityEngine.Debug.Log("Length: " + bestLength + " Time: " + time + " Ants: " + numAnts + " Gears: " + Gears.Count);

            calcDone = true;
            yield return new WaitForSeconds(3f);
            line.gameObject.SetActive(false);
        }
        // Main

        // --------------------------------------------------------------------------------------------


        List<string> logs = new List<string>();
        int rows = 10;
        private void Log(string s)
        {
            logs.Add(s);
            if(logs.Count > rows) logs.RemoveAt(0);
            string text = "";
            for (int i = 0; i < logs.Count; i++)
            {
                text += logs[i] + Environment.NewLine;
            }
            
            logText.text = text;
            
            UnityEngine.Debug.Log(s);
        }


        private int[][] InitAnts(int numAnts, int numCities)
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++)
            {
                int start = random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }

        private int[] RandomTrail(int start, int numCities)
        {
            // helper for InitAnts
            int[] trail = new int[numCities];

            // sequential
            for (int i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (int i = 0; i <= numCities - 1; i++)
            {
                int r = random.Next(i, numCities);
                int tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start);
            // put start at [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private int IndexOfTarget(int[] trail, int target)
        {
            // helper for RandomTrail
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception("Target not found in IndexOfTarget");
        }

        private double Length(int[] trail, int[][] dists)
        {
            // total length of a trail
            double result = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], dists);
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private int[] BestTrail(int[][] ants, int[][] dists)
        {
            // best trail has shortest total length
            double bestLength = Length(ants[0], dists);
            int idxBestLength = 0;
            for (int k = 1; k <= ants.Length - 1; k++)
            {
                double len = Length(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numCities = ants[0].Length;
            //INSTANT VB NOTE: The local variable bestTrail was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            int[] bestTrail_Renamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }

        // --------------------------------------------------------------------------------------------

        private double[][] InitPheromones(int numCities)
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                int start = random.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

        private int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i <= numCities - 2; i++)
            {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

        private int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cuml.Length-1] to 1.00
            }

            double p = random.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            // inclues cityX and visited cities
            double sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (int i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double length = Length(ants[k], dists);
                        // length of ant k trail
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = (Q / length);
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }
            else if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            else if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == cityY)
            {
                return true;
            }
            else if (trail[idx + 1] == cityY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------

        private int[][] MakeGraphDistances(int numCities)
        {
            int[][] dists = new int[numCities][];
            for (int i = 0; i <= dists.Length - 1; i++)
            {
                dists[i] = new int[numCities];
            }
            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    int d = Mathf.RoundToInt((Gears[i].position - Gears[j].position).magnitude);
                    // [1,8]
                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }
            return dists;
        }

        private double Distance(int cityX, int cityY, int[][] dists)
        {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------

        private void Display(int[] trail, double length)
        {
            Vector3[] positions = new Vector3[trail.Length];
            for (int i = 0; i < trail.Length; i++)
            {
                positions[i] = Gears[trail[i]].position;
                positions[i].y = 1.3f;
            }
            line.positionCount = trail.Length;
            line.SetPositions(positions);
            bestPathText.text = "Path: " + Mathf.RoundToInt((float)length) + " cm";
            Log("");
        }


        private void ShowAnts(int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    //Log(ants[i][j] + " ");
                }

                for (int j = ants[i].Length - 4; j <= ants[i].Length - 1; j++)
                {
                    //Log(ants[i][j] + " ");
                }
                //double len = Length(ants[i], dists);
            }
        }

        private void Display(double[][] pheromones)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                //Log(i + ": ");
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    //Log(pheromones[i][j].ToString("F4").PadLeft(8) + " ");
                }
                //Log("");
            }

        }

    }
    // class AntColonyProgram

}
// ns
