using UnityEngine;
using System.Collections.Generic;

public class AIConfig : MonoBehaviour {

    
    [System.Serializable]
    public class Layer {
        public int Cnt;

        public int Ni1 = -1;
    }
    public List<Layer> Layers;


    public int StartLayer = 1, InputMask = 1, OutputMask = 1;


    NeuralNetwork.Synapsis[] extendArray(NeuralNetwork.Network nn, int i, int nc )  {
        var old = nn.Synapsis[i];
        nn.Synapsis[i] = new NeuralNetwork.Synapsis[nc];
        old.CopyTo(nn.Synapsis[i], 0);
        return nn.Synapsis[i];
    }
    public int In_Neurons = -1, In_Synapsis = -1;
    public int Out_Neurons = -1, Out_Synapsis = -1;

    delegate void Input_Dlg(int startOff, int i);
    void forEach_Input(Input_Dlg sub) {
        if((InputMask & 1) != 0) sub(0,2); //velocity
        if((InputMask & 2) != 0) sub(2, 2);  //turretDir
        if((InputMask & 4) != 0) sub(4, ScanRc); //scanner
        if((InputMask & 8) != 0) sub(4+ ScanRc,ScanRc);//scan for opponent
    }
    delegate void Output_Dlg(int startOff, int i);
    void forEach_Output(Output_Dlg sub) {
        if((OutputMask & 1) != 0) sub(0, 2); //move
        if((OutputMask & 2) != 0) sub(2, 2);  //turret
    }

    int ScanRc;
    public void init( NeuralNetwork.Network nn, int scannerRC ) {
        ScanRc = scannerRC;
        In_Neurons = In_Synapsis = 0;

        int sn0c = nn.Synapsis[StartLayer-1].Length, sn0i = sn0c;
      
        forEach_Input((int startOff, int cnt) => { In_Neurons += cnt; });

        sn0c += (In_Synapsis = In_Neurons * Layers[0].Cnt);
        var synA = extendArray(nn, StartLayer - 1, sn0c);
        forEach_Input((int startOff, int cnt) => {
            for( int i = cnt; i-- > 0; )
                for(int j = Layers[0].Cnt; j-- > 0; )
                    synA[sn0i++] = nn.setSynapsis(0, startOff+i, StartLayer, Layers[0].Ni1 + j  );
        });



        int snOc = nn.Synapsis[nn.Synapsis.Length - 1].Length, snOi = snOc;
        Out_Neurons = Out_Synapsis = 0;
        forEach_Output((int startOff, int cnt) => { Out_Neurons += cnt; });

        snOc += (Out_Synapsis = Out_Neurons * Layers[Layers.Count - 1].Cnt);
        var synO = extendArray(nn, nn.Synapsis.Length-1, snOc);
        forEach_Output((int startOff, int cnt) => {
            for(int i = cnt; i-- > 0;)
                for(int j = Layers[Layers.Count - 1].Cnt; j-- > 0;)
                    synO[snOi++] = nn.setSynapsis(StartLayer + Layers.Count - 1, Layers[Layers.Count - 1].Ni1 + j,  nn.Synapsis.Length, startOff + i);
        });


    }

}
