using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Mrgada
{
    public partial class c_Mrp6 : S7Collector
    {
        public class udtSCADAAnalogSensor
        {
            public S7Var<float> ValueEgu;
            public S7Var<bool> Failure;
            public S7Var<bool> Manual;
            public S7Var<bool> WarningEnabled;
            public S7Var<bool> WarningActive;
            public S7Var<bool> WarningValueLow;
            public S7Var<bool> WarningValueHigh;
            public S7Var<bool> WarningTimeOut;
            public S7Var<bool> Spare_Bool_0;
            public S7Var<bool> InitWarnings;
            public S7Var<bool> ToggleWarnings;
            public S7Var<bool> WarningEnabledW;
            public S7Var<bool> WarningValueLowW;
            public S7Var<bool> WarningValueHighW;
            public S7Var<bool> ToggleWarningsW;
            public S7Var<bool> Spare_Bool_1;
            public S7Var<bool> Spare_Bool_2;
            public S7Var<float> ManValueEGU;

            private List<S7Var<object>> _S7Vars;

            public udtSCADAAnalogSensor(int dbNum)
            {
                ValueEgu = new(dbNum);
                Failure = new(dbNum);
                Manual = new(dbNum);
                WarningEnabled = new(dbNum);
                WarningActive = new(dbNum);
                WarningValueLow = new(dbNum);
                WarningValueHigh = new(dbNum);
                WarningTimeOut = new(dbNum);
                Spare_Bool_0 = new(dbNum);
                InitWarnings = new(dbNum);
                ToggleWarnings = new(dbNum);
                WarningEnabledW = new(dbNum);
                WarningValueLowW = new(dbNum);
                WarningValueHighW = new(dbNum);
                ToggleWarningsW = new(dbNum);
                Spare_Bool_1 = new(dbNum);
                Spare_Bool_2 = new(dbNum);
                ManValueEGU = new(dbNum);
            }
            public int AlignAndIncrement(int bitOffset)
            {
                bitOffset = ValueEgu.AlignAndIncrement(bitOffset);
                bitOffset = Failure.AlignAndIncrement(bitOffset);
                bitOffset = Manual.AlignAndIncrement(bitOffset);
                bitOffset = WarningEnabled.AlignAndIncrement(bitOffset);
                bitOffset = WarningActive.AlignAndIncrement(bitOffset);
                bitOffset = WarningValueLow.AlignAndIncrement(bitOffset);
                bitOffset = WarningValueHigh.AlignAndIncrement(bitOffset);
                bitOffset = WarningTimeOut.AlignAndIncrement(bitOffset);
                bitOffset = Spare_Bool_0.AlignAndIncrement(bitOffset);
                bitOffset = InitWarnings.AlignAndIncrement(bitOffset);
                bitOffset = ToggleWarnings.AlignAndIncrement(bitOffset);
                bitOffset = WarningEnabledW.AlignAndIncrement(bitOffset);
                bitOffset = WarningValueLowW.AlignAndIncrement(bitOffset);
                bitOffset = WarningValueHighW.AlignAndIncrement(bitOffset);
                bitOffset = ToggleWarningsW.AlignAndIncrement(bitOffset);
                bitOffset = Spare_Bool_1.AlignAndIncrement(bitOffset);
                bitOffset = Spare_Bool_2.AlignAndIncrement(bitOffset);
                bitOffset = ManValueEGU.AlignAndIncrement(bitOffset);

                return bitOffset;
            }
            public void ParseCVs(byte[] bytes)
            {
                ValueEgu.ParseCVs(bytes);
                Failure.ParseCVs(bytes);
                Manual.ParseCVs(bytes);
                WarningEnabled.ParseCVs(bytes);
                WarningActive.ParseCVs(bytes);
                WarningValueLow.ParseCVs(bytes);
                WarningValueHigh.ParseCVs(bytes);
                WarningTimeOut.ParseCVs(bytes);
                Spare_Bool_0.ParseCVs(bytes);
                InitWarnings.ParseCVs(bytes);
                ToggleWarnings.ParseCVs(bytes);
                WarningEnabledW.ParseCVs(bytes);
                WarningValueLowW.ParseCVs(bytes);
                WarningValueHighW.ParseCVs(bytes);
                ToggleWarningsW.ParseCVs(bytes);
                Spare_Bool_1.ParseCVs(bytes);
                Spare_Bool_2.ParseCVs(bytes);
                ManValueEGU.ParseCVs(bytes);
            }
        }
    }
}
