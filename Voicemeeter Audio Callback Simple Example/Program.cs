using System;
using static System.Threading.Thread;
using AtgDev.Voicemeeter;
using AtgDev.Voicemeeter.Types.AudioCallback;
using System.Text;

namespace VoicemeeterAudioCallbackExample
{
    class Program
    {
        public static RemoteApiExtender remote;

        unsafe public static Int32 AudioPassthrough64(void* customDataP, Command command, void* callbackDataP, Int32 addData)
        {
            // API's Audio Callback system is quite reliable.
            // If this code throw exception or executed too slow (IO operations, or even debugging)
            // Voicemeeter just skip it and stream sound without any processing from callback.
            switch (command)
            {
                // only BufferIn case because of Mode.Inputs
                case Command.BufferIn:
                    var audioBufferP = (AudioBuffer64*)callbackDataP;
                    var samplesNumber = audioBufferP->samplesPerFrame;
                    var outputsNumber = audioBufferP->outputsNumber;
                    var inBufferP = audioBufferP->inBufferP;
                    var outBufferP = audioBufferP->outBufferP;
                    Single* inFrame, outFrame;
                    // number of input and output channels match in this case
                    for (int channel = 0; channel < outputsNumber; channel++)
                    {
                        inFrame = (Single*)inBufferP[channel];
                        outFrame = (Single*)outBufferP[channel];
                        for (int i = 0; i < samplesNumber; i++)
                        {
                            // without assignment there will be no audio
                            outFrame[i] = inFrame[i];
                        }
                    }
                    break;
            }
            return 0;
        }

        unsafe public static Int32 AudioPassthrough32(void* customDataP, Command command, void* callbackDataP, Int32 addData)
        {
            // API's Audio Callback system is quite reliable.
            // If this code throw exception or executed too slow (IO operations, or even debugging)
            // Voicemeeter just skip it and stream sound without any processing from callback.
            switch (command)
            {
                // only BufferIn case because of Mode.Inputs
                case Command.BufferIn:
                    var audioBufferP = (AudioBuffer32*)callbackDataP;
                    var samplesNumber = audioBufferP->samplesPerFrame;
                    var outputsNumber = audioBufferP->outputsNumber;
                    var inBufferP = audioBufferP->inBufferP;
                    var outBufferP = audioBufferP->outBufferP;
                    Single* inFrame, outFrame;
                    // number of input and output channels match in this case
                    for (int channel = 0; channel < outputsNumber; channel++)
                    {
                        inFrame = (Single*)inBufferP[channel];
                        outFrame = (Single*)outBufferP[channel];
                        for (int i = 0; i < samplesNumber; i++)
                        {
                            // without assignment there will be no audio
                            outFrame[i] = inFrame[i];
                        }
                    }
                    break;
            }
            return 0;
        }

        static unsafe void TestAudioCallback()
        {
            string clientName = "Simple Audio Callback Test";
            int resp;
            // Registering callback to process inputs only
            if (Environment.Is64BitProcess)
            {
                resp = remote.AudioCallbackRegister(Mode.Inputs, AudioPassthrough64, null, ref clientName);
            } else
            {
                resp = remote.AudioCallbackRegister(Mode.Inputs, AudioPassthrough32, null, ref clientName);
            }
            Console.WriteLine($"REGISTER AUDIO CALLBACK: response {resp}, NAME: {clientName}");
            if (resp != 0) return;
            // Without Sleep() between AudioCallbackRegister() and AudioCallbackStart()
            // there is small chance that the audio might become corrupted.
            Sleep(20);
            resp = remote.AudioCallbackStart();
            Console.WriteLine($"START AUDIO CALLBACK: response {resp}");
            // Process audio for N ms
            Sleep(5000);
            resp = remote.AudioCallbackStop();
            Console.WriteLine($"STOP AUDIO CALLBACK: response {resp}");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                var path = AtgDev.Voicemeeter.Utils.PathHelper.GetDllPath();
                remote = new RemoteApiExtender(path);
            }
            catch (Exception e)
            {
                // if cannot load dll or procedures
                Console.WriteLine($"{e.GetType()}\n{e.Message}");
                Console.ReadKey();
                Environment.Exit(1);
            }
            var resp = remote.Login();
            Console.WriteLine($"Login: {resp}");
            if (resp != 0)
            {
                Console.WriteLine("Can't login");
                Console.ReadKey();
                Environment.Exit(1);
            }
            try
            {
                resp = remote.WaitForNewParams(1000);
                Console.WriteLine($"WaitForUpdate: {resp}");
                // Do stuff with Remote API
                TestAudioCallback();
            }
            finally
            {
                // If exception is thrown program supposed to successfully Logout
                remote.AudioCallbackUnregister();
                Console.WriteLine($"AUDIO CALLBACK UNREGISTER: {resp}");
                resp = remote.Logout();
                Console.WriteLine($"Logout {resp}");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}
