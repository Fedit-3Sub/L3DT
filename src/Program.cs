using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VIZToHMF
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //==================================================================================================
            //실행 파라미터
            //==================================================================================================
            if (args.Length > 0 && String.IsNullOrEmpty(args[0]) == false)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new HMFForm(args));
            }
        }
    }
}
