using System;

namespace SpeechRecognition.HMM
{
    public class HiddenMarkovModel
    {

        /**
	 * minimum probability
	 */
        // final double MIN_PROBABILITY = 0.00000000001;
        private double MIN_PROBABILITY = 0.0001;
        private Random _randomGenerator = new Random();
        /**
         * length of observation sequence
         */
        private int _len_obSeq;
        /**
         * number of state in the model example: number of urns
         */
        private int _num_states;
        /**
         * number of observation symbols per state example: how many different
         * colour balls there are
         */
        private int _num_symbols;
        /**
         * number of states the model is allowed to jump
         */
        private int _delta = 2;
        /**
         * discrete set of observation symbols example: sequence of colour of balls
         */
        private int[][] _obSeq;
        /**
         * current observation sequence
         */
        protected int[] _currentSeq;
        /**
         * number of observation sequence
         */
        protected int _num_obSeq;
        /**
         * state transition probability example: probability from one state to
         * another state
         */
        protected double[,] _transition;
        /**
         * discrete output probability example: probability of a specific output
         * from a state
         */
        protected double[][] _output;
        /**
         * initial state distribution example: which state is the starting state
         */
        protected double[] _pi;
        /**
         * forward variable alpha
         */
        protected double[,] _alpha;
        /**
         * backward variable beta
         */
        protected double[,] _beta;
        /**
         * Scale Coefficient
         */
        protected double[] _scaleFactor;
        /**
         * variable for viterbi algorithm
         */
        private int[,] _psi;
        /**
         * best state sequence
         */
        public int[] _q;


        /**
	 * viterbi algorithm used to get best state sequence and probability<br>
	 * calls: none<br>
	 * called by: volume
	 * 
	 * @param testSeq
	 *            test sequence
	 * @return probability
	 */
        public double Viterbi(int[] testSeq)
        {
            SetObSeq(testSeq);
            double[,] phi = new double[_len_obSeq, _num_states];
            _psi = new int[_len_obSeq, _num_states];
            _q = new int[_len_obSeq];
            double temp = 0, max;
            int index = 0;

            for (int i = 0; i < _num_states; i++)
            {
                temp = _pi[i];
                if (temp == 0)
                {
                    temp = MIN_PROBABILITY;
                }

                phi[0, i] = Math.Log(temp) + Math.Log(_output[i][_currentSeq[0]]);
                _psi[0, i] = 0;
            }


            for (int t = 1; t < _len_obSeq; t++)
            {
                for (int j = 0; j < _num_states; j++)
                {
                    max = phi[t - 1, 0] + Math.Log(_transition[0, j]);
                    temp = 0;
                    index = 0;

                    for (int i = 1; i < _num_states; i++)
                    {

                        temp = phi[t - 1, i] + Math.Log(_transition[i, j]);
                        if (temp > max)
                        {
                            max = temp;
                            index = i;
                        }

                    }

                    phi[t, j] = max + Math.Log(_output[j][_currentSeq[t]]);
                    _psi[t, j] = index;
                }
            }

            max = phi[_len_obSeq - 1, 0];
            temp = 0;
            index = 0;
            for (int i = 1; i < _num_states; i++)
            {
                temp = phi[_len_obSeq - 1, i];

                if (temp > max)
                {
                    max = temp;
                    index = i;
                }
            }

            _q[_len_obSeq - 1] = index;

            for (int t = _len_obSeq - 2; t >= 0; t--)
            {
                _q[t] = _psi[t + 1, _q[t + 1]];
            }

            return max;
        }



        /**
         * rescales backward variable beta to prevent underflow<br>
         * calls: none<br>
         * called by: HiddenMarkov
         * 
         * @param t
         *            index number of backward variable beta
         */
        private void RescaleBeta(int t)
        {
            for (int i = 0; i < _num_states; i++)
            {
                _beta[t, i] *= _scaleFactor[t];
            }
        }


        /**
	 * rescales forward variable alpha to prevent underflow<br>
	 * calls: none<br>
	 * called by: HiddenMarkov
	 * 
	 * @param t
	 *            index number of forward variable alpha
	 */
        private void RescaleAlpha(int t)
        {
            // calculate scale coefficients
            for (int i = 0; i < _num_states; i++)
            {
                _scaleFactor[t] += _alpha[t, i];
            }

            _scaleFactor[t] = 1 / _scaleFactor[t];

            // apply scale coefficients
            for (int i = 0; i < _num_states; i++)
            {
                _alpha[t, i] *= _scaleFactor[t];
            }
        }



        /**
	 * returns the probability calculated from the testing sequence<br>
	 * calls: none<br>
	 * called by: volume
	 * 
	 * @param testSeq
	 *            testing sequence
	 * @return probability of observation sequence given the model
	 */
        public double GetProbability(int[] testSeq)
        {
            SetObSeq(testSeq);
            double temp = ComputeAlpha();

            return temp;
        }



        /**
	 * calculate forward variable alpha<br>
	 * calls: none<br>
	 * called by: HiddenMarkov
	 * 
	 * @return probability
	 */
        protected double ComputeAlpha()
        {
            /**
             * Pr(obSeq | model); Probability of the observation sequence given the
             * hmm model
             */
            double probability = 0;

            // reset scaleFactor[]
            for (int t = 0; t < _len_obSeq; t++)
            {
                _scaleFactor[t] = 0;
            }

            /**
             * Initialization: Calculating all alpha variables at time = 0
             */
            for (int i = 0; i < _num_states; i++)
            {
                // System.out.println("current  "+i+" crr  "+currentSeq[0]);

                _alpha[0, i] = _pi[i] * _output[i][_currentSeq[0]];
            }
            RescaleAlpha(0);

            /**
             * Induction:
             */
            for (int t = 0; t < _len_obSeq - 1; t++)
            {
                for (int j = 0; j < _num_states; j++)
                {

                    /**
                     * Sum of all alpha[t][i] * transition[i][j]
                     */
                    double sum = 0;

                    /**
                     * Calculate sum of all alpha[t][i] * transition[i][j], 0 <= i <
                     * num_states
                     */
                    for (int i = 0; i < _num_states; i++)
                    {
                        sum += _alpha[t, i] * _transition[i, j];
                    }

                    _alpha[t + 1, j] = sum * _output[j][_currentSeq[t + 1]];
                }
                RescaleAlpha(t + 1);
            }

            /**
             * Termination: Calculate Pr(obSeq | model)
             */
            for (int i = 0; i < _num_states; i++)
            {
                probability += _alpha[_len_obSeq - 1, i];
            }

            probability = 0;
            // double totalScaleFactor = 1;
            for (int t = 0; t < _len_obSeq; t++)
            {
                // System.out.println("s: " + Math.log(scaleFactor[t]));

                probability += Math.Log(_scaleFactor[t]);

                // totalScaleFactor *= scaleFactor[t];
            }

            return -probability;
            // return porbability / totalScaleFactor;
        }


        /**
	 * calculate backward variable beta for later use with Re-Estimation method<br>
	 * calls: none<br>
	 * called by: HiddenMarkov
	 */
        protected void ComputeBeta()
        {
            /**
             * Initialization: Set all beta variables to 1 at time = len_obSeq - 1
             */
            for (int i = 0; i < _num_states; i++)
            {
                _beta[_len_obSeq - 1, i] = 1;
            }
            RescaleBeta(_len_obSeq - 1);

            /**
             * Induction:
             */
            for (int t = _len_obSeq - 2; t >= 0; t--)
            {
                for (int i = 0; i < _num_states; i++)
                {
                    for (int j = 0; j < _num_states; j++)
                    {
                        _beta[t, i] += _transition[i, j] * _output[j][_currentSeq[t + 1]] * _beta[t + 1, j];
                    }
                }
                RescaleBeta(t);
            }
        }


        /**
	 * set the number of training sequences<br>
	 * calls: none<br>
	 * called by: trainHMM
	 * 
	 * @param k
	 *            number of training sequences
	 */
        public void SetNumObSeq(int k)
        {
            _num_obSeq = k;
            _obSeq = new int[k][];
        }


        /**
	 * set a training sequence for re-estimation step<br>
	 * calls: none<br>
	 * called by: trainHMM
	 * 
	 * @param k
	 *            index representing kth training sequence
	 * @param trainSeq
	 *            training sequence
	 */
        public void SetTrainSeq(int k, int[] trainSeq)
        {
            _obSeq[k] = trainSeq;
        }


        /**
	 * set training sequences for re-estimation step<br>
	 * calls: none<br>
	 * called by: trainHMM
	 * 
	 * @param trainSeq
	 *            training sequences
	 */
        public void SetTrainSeq(int[][] trainSeq)
        {
            _num_obSeq = trainSeq.Length;
            _obSeq = new int[_num_obSeq][];// /ADDED
            // System.out.println("num obSeq << setTrainSeq()    "+num_obSeq);
            for (int k = 0; k < _num_obSeq; k++)
            {
                _obSeq[k] = trainSeq[k];
            }
        }


        /**
	 * train the hmm model until no more improvement<br>
	 * calls: none<br>
	 * called by: trainHMM
	 */
        public void Train()
        {
            // re-estimate 25 times
            // NOTE: should be changed to re-estimate until no more improvement
            for (int i = 0; i < 20; i++)
            {
                Reestimate();
                //System.out.println("reestimating.....");
            }
            //
            // oldm=
        }



        /**
	 * Baum-Welch Algorithm - Re-estimate (iterative udpate and improvement) of
	 * HMM parameters<br>
	 * calls: none<br>
	 * called by: trainHMM
	 */
        private void Reestimate()
        {
            // new probabilities that will be the optimized and replace the older
            // version
            double[,] newTransition = new double[_num_states, _num_states];
            double[][] newOutput = new double[_num_states][];
            double[] numerator = new double[_num_obSeq];
            double[] denominator = new double[_num_obSeq];

            // calculate new transition probability matrix
            double sumP = 0;

            for (int i = 0; i < _num_states; i++)
            {
                newOutput[i] = new double[_num_symbols];
                for (int j = 0; j < _num_states; j++)
                {

                    if (j < i || j > i + _delta)
                    {
                        newTransition[i, j] = 0;
                    }
                    else
                    {
                        for (int k = 0; k < _num_obSeq; k++)
                        {
                            numerator[k] = denominator[k] = 0;
                            SetObSeq(_obSeq[k]);

                            sumP += ComputeAlpha();
                            ComputeBeta();
                            for (int t = 0; t < _len_obSeq - 1; t++)
                            {
                                numerator[k] += _alpha[t, i] * _transition[i, j] * _output[j][_currentSeq[t + 1]] * _beta[t + 1, j];
                                denominator[k] += _alpha[t, i] * _beta[t, i];
                            }
                        }
                        double denom = 0;
                        for (int k = 0; k < _num_obSeq; k++)
                        {
                            newTransition[i, j] += (1 / sumP) * numerator[k];
                            denom += (1 / sumP) * denominator[k];
                        }
                        newTransition[i, j] /= denom;
                        newTransition[i, j] += MIN_PROBABILITY;
                    }
                }
            }

            // calculate new output probability matrix
            sumP = 0;
            for (int i = 0; i < _num_states; i++)
            {
                for (int j = 0; j < _num_symbols; j++)
                {
                    for (int k = 0; k < _num_obSeq; k++)
                    {
                        numerator[k] = denominator[k] = 0;
                        SetObSeq(_obSeq[k]);

                        sumP += ComputeAlpha();
                        ComputeBeta();

                        for (int t = 0; t < _len_obSeq - 1; t++)
                        {
                            if (_currentSeq[t] == j)
                            {
                                numerator[k] += _alpha[t, i] * _beta[t, i];
                            }
                            denominator[k] += _alpha[t, i] * _beta[t, i];
                        }
                    }

                    double denom = 0;
                    for (int k = 0; k < _num_obSeq; k++)
                    {
                        newOutput[i][j] += (1 / sumP) * numerator[k];
                        denom += (1 / sumP) * denominator[k];
                    }

                    newOutput[i][j] /= denom;
                    newOutput[i][j] += MIN_PROBABILITY;
                }
            }

            // replace old matrices after re-estimate
            _transition = newTransition;
            _output = newOutput;
        }


        /**
	 * set observation sequence<br>
	 * calls: none<br>
	 * called by: trainHMM
	 * 
	 * @param observationSeq
	 *            observation sequence
	 */
        public void SetObSeq(int[] observationSeq)
        {
            _currentSeq = observationSeq;
            _len_obSeq = observationSeq.Length;
            // System.out.println("len_obSeq<<setObSeq()   "+len_obSeq);

            _alpha = new double[_len_obSeq, _num_states];
            _beta = new double[_len_obSeq, _num_states];
            _scaleFactor = new double[_len_obSeq];
        }


        public HiddenMarkovModel(int num_states, int num_symbols)
        {
            this._num_states = num_states;
            this._num_symbols = num_symbols;
            _transition = new double[num_states, num_states];
            _output = new double[num_states][];
            for (int index = 0; index < num_states; index++)
            {
                _output[index] = new double[num_symbols];
            }
            _pi = new double[num_states];

            /**
             * in a left-to-right HMM model, the first state is always the initial
             * state. e.g. probability = 1
             */
            _pi[0] = 1;
            for (int i = 1; i < num_states; i++)
            {
                _pi[i] = 0;
            }

            // generate random probability for all the other probability matrices
            RandomProb();
        }


        private void RandomProb()
        {
            for (int i = 0; i < _num_states; i++)
            {
                for (int j = 0; j < _num_states; j++)
                {
                    if (j < i || j > i + _delta)
                    {
                        _transition[i, j] = 0;// R-L prob=0 for L-R HMM, and with
                        // Delta
                    }
                    else
                    {
                        double randNum = _randomGenerator.NextDouble();
                        _transition[i, j] = randNum;
                        // System.out.println("transition init: "+transition[i][j]);
                    }
                }
                for (int j = 0; j < _num_symbols; j++)
                {
                    double randNum = _randomGenerator.NextDouble();
                    _output[i][j] = randNum;
                    // System.out.println("outputInit: "+output[i][j]);
                }

            }
        }

    }
}
