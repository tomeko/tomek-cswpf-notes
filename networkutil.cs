using System;
using System.Diagnostics;
using System.Threading;

namespace tomek_cswpf_notes
{
    public static class networkutil
    {


        public static class AppNetworkMonitor
        {
            static int _timeo_s, _update_ms, _tot_tx_sz_kb;
            static bool bcontinue;
            static Action<int> _update_cb;

            // updates the callback with percent done with init file size specified
            public static void StartMonitorTX(Action<int> update_cb, int total_tx_sz_KB, int update_ms, int timeo_s = 120)
            {
                _update_cb = update_cb;
                _update_ms = update_ms;
                _timeo_s = timeo_s;
                _tot_tx_sz_kb = total_tx_sz_KB;
                bcontinue = true;

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    
                    try
                    {
                        runtxmon();
                    }
                    catch (Exception ex) 
                    {
                        App.Log(ex);
                    }

                }).Start();

            }
            
            public static void CancelTXMonitor()
            {
                bcontinue = false;
            }

            private static void runtxmon()
            {
                PerformanceCounterCategory pcat = new PerformanceCounterCategory("Network Interface");
                string inst = pcat.GetInstanceNames()[0];   // first NIC 
                PerformanceCounter p_tx = new PerformanceCounter("Network Interface", "Bytes Sent/sec", inst);
                PerformanceCounter p_rx = new PerformanceCounter("Network Interface", "Bytes Received/sec", inst);
                int tx_kb = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                TimeSpan timeo = new TimeSpan(0, 0, _timeo_s);

                while (bcontinue)
                {
                    float nxtval = p_tx.NextValue();

                    tx_kb += (int)(nxtval / 1024.0f);

                    int pct_done = tx_kb / _tot_tx_sz_kb;
                    _update_cb(pct_done);

#if DEBUG
                    Console.WriteLine($"TXMON: pct_kb: {pct_done}   nextval: {nxtval} nextval1024: {nxtval / 1024.0f}  totsz_kb:{_tot_tx_sz_kb} inst: {inst}");
#endif 

                    if (tx_kb >= _tot_tx_sz_kb)     // todo: maybe put a threshold in here?
                        bcontinue = false;

                    if (TimeSpan.Compare(sw.Elapsed, timeo) == 1)
                        bcontinue = false;

                    Thread.Sleep(_update_ms);
                }
                sw.Reset();
            }
        }
    }
}
