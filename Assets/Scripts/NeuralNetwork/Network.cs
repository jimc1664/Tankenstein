using System;

namespace NeuralNetwork
{
    public class Network
    {
        #region Getters & Setters

        public Neuron[] Neurons
        {
            get
            {
                return neurons;
            }
            protected set
            {
                neurons = value;
            }
        }

        public Synapsis[][] Synapsis
        {
            get
            {
                return synapsis;
            }
            protected set
            {
                synapsis = value;
            }
        }

        public float[] Weights
        {
            get
            {
                float[] rtn = new float[amountOfFloats];

                int k = 0;
                for (int i = 0; i < synapsis.Length; i++)
                {
                    for (int j = 0; j < synapsis[i].Length; j++)
                    {
                        rtn[k] = synapsis[i][j].weight;
                        k++;
                    }
                }

                for (int i = 0; i < neurons.Length; i++)
                {
                   // for (int j = 0; j < neurons[i].Length; j++)
                   // {
                        rtn[k] = neurons[i].bias;
                        k++;
                   // }
                }

                return rtn;
            }
            set
            {
                if (value.Length != amountOfFloats)
                    throw new Exception("Weights length does not match synapsis & neurons");

                int k = 0;
                for (int i = 0; i < synapsis.Length; i++)
                {
                    for (int j = 0; j < synapsis[i].Length; j++)
                    {
                        synapsis[i][j].weight = value[k];
                        k++;
                    }
                }

                for (int i = 0; i < neurons.Length; i++)
                {
                   // for (int j = 0; j < neurons[i].Length; j++)
                   // {
                        neurons[i].bias = value[k];
                        k++;
                   // }
                }
            }
        }

        #endregion

        #region Variables

        private Neuron[] neurons;
        private Synapsis[][] synapsis;
        private int amountOfFloats = 0;
        protected int[] layers;
        protected int[] layersNi;
        protected Random random;

        #endregion

        #region Constructors

        void baseInit(int[] lyrs, int seed ) {

            if(lyrs.Length < 3)
                throw new Exception("There needs to be atleast 2 layers");
            random = seed == -1 ? new Random() : new Random(seed);
            layers = lyrs;

            layersNi = new int[layers.Length];

            int tn = 0;
            for(int l = 0; l < layers.Length; l++) {
                layersNi[l] = tn;
                tn += layers[l];
            }

            neurons = new Neuron[tn];
            synapsis = new Synapsis[layers.Length - 1][];


            //Setup Neurons
            for(int i = 0; i < tn; i++) {
                //neurons[l] = new Neuron[layers[l]];
                //for(int i = 0; i < layers[l]; i++) {
                amountOfFloats++;
                neurons[i] = new Neuron(ActivationType.HyperbolicTangent, 0, 0);// Convert.ToSingle(random.NextDouble() * 2.0f - 1.0f));
                //}
            }
        }

        public Network(int[] layers, bool connectAllNodes, int seed = -1) {
            baseInit(layers, seed);



            //Setup Synapsis
            for(int i = 0; i < layers.Length - 1; i++) {
                AddConnection(i, i + 1, connectAllNodes);
            }
        }

        public Network(int[] layers, int seed) {
            baseInit(layers, seed);

            for(int i = 0; i < layers.Length - 1; i++) {
                synapsis[i] = new Synapsis[0];
            }
        }

        /*public Network(int[] layers, ActivationType[] activationMethods, bool connectAllNodes)
        {
            if (layers.Length < 3)
                throw new Exception("There needs to be atleast 2 layers");

            if (layers.Length != activationMethods.Length)
                throw new Exception("Activation Methods length doesn't equal layers length");

            baseInit(layers, -1);


            //Setup Neurons
            for(int i = 0; i < tn; i++) {
                //neurons[l] = new Neuron[layers[l]];
                //for(int i = 0; i < layers[l]; i++) {
                amountOfFloats++;
                neurons[i] = new Neuron(ActivationType.HyperbolicTangent, 0, Convert.ToSingle(random.NextDouble() * 2.0f - 1.0f));
                //}
            } 

            //Setup Synapsis
            for (int i = 0; i < layers.Length - 1; i++)
            {
                AddConnection(i, i + 1, connectAllNodes);
            }
        } */


        public Network(Network o) {
            neurons = (Neuron[])o.neurons.Clone();
            synapsis = (Synapsis[][])o.synapsis.Clone();
            layers = (int[])o.layers.Clone();
            random = new Random();
        }

        
        #endregion

        #region Public Methods
        

        public virtual float[] Compute(float[] xValues)
        {
            if (xValues.Length != layers[0])
                throw new Exception("X Values length doesn't match amount of input nodes");

            //Setup Input Layer
            for (int i = 0; i < xValues.Length; i++)
            {
                neurons[i].charge = xValues[i];
            }


            for(int i = layers[0]; i <neurons.Length; i++) 
                neurons[i].charge = Neurons[i].bias;

            for(int i = 0; i < layers.Length - 1; i++) {
                for(int j = synapsis[i].Length; j-- > 0;) {
                    Neurons[synapsis[i][j].end].charge += synapsis[i][j].weight * Neurons[synapsis[i][j].start].charge;
                }

                for(int j = layers[i+1]; j-- >0; ) {
                    int ni = layersNi[i + 1] + j;
                    Neurons[ni].charge = ActivationMethods.HyperbolidTangent(Neurons[ni].charge );
                }
            }
            /*
            //Go Through Each Layer
            for (int i = 0; i < layers.Length - 1; i++)
            {
                float[] charges = new float[layers[i + 1]];
                int synCounter = 0;
                //Compute Weights
                for (int j = 0; j < layers[i]; j++)
                {
                    for (int k = 0; k < neurons[i + 1].Length; k++)
                    {
                        charges[k] += synapsis[i][synCounter].weight * neurons[i][j].charge;
                        synCounter++;
                    }
                }
                //Add Bias, Activation Function & Apply results to neuron
                for (int j = 0; j < neurons[i + 1].Length; j++)
                {
                    charges[j] += neurons[i + 1][j].bias;
                    charges = ActivationMethods.Activation(charges, neurons[i + 1][j].activation);
                    neurons[i + 1][j].charge = charges[j];
                }
            } */

            //Get Output Layer
            float[] yValues = new float[layers[layers.Length - 1]];
            for (int i = 0; i < yValues.Length; i++)
            {
                yValues[i] = neurons[ layersNi[layers.Length-1] + i].charge;
            }

            return yValues;
        }
        public void copyTo(Network n1) {

            for(int layer = Synapsis.Length; layer-- > 0;) {

                Synapsis[layer].CopyTo(n1.Synapsis[layer], 0);
            }
            Neurons.CopyTo(n1.Neurons, 0);
        }
        #endregion

        #region Private Methods

        private void AddConnection(int startLayer, int endLayer, bool connectAllNodes)
        {
            synapsis[startLayer] = new Synapsis[layers[startLayer] * layers[endLayer]];
            int synCounter = 0;
            for (int i = 0; i < layers[startLayer]; i++)
            {
                for (int j = 0; j < layers[endLayer]; j++)
                {
                    synapsis[startLayer][synCounter] = new Synapsis( layersNi[startLayer]+ i, layersNi[endLayer] + j, Convert.ToSingle(random.NextDouble() * 2.0f - 1.0f));
                    if (!connectAllNodes)
                    {
                        //Set weights to 0 for some synapsis
                    }
                    amountOfFloats++;
                    synCounter++;
                }
            }
        }


        #endregion
        public Synapsis setSynapsis( int nl1, int ni1, int nl2, int ni2 ) {
            return new Synapsis(layersNi[nl1] + ni1, layersNi[nl2] + ni2, Convert.ToSingle(random.NextDouble() * 2.0f - 1.0f));
        }
    }
}