using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MainInterface
{
    public static class CommonVal  // static 不是必须
    {
        public static List<short> m_shECGList = new List<short>();
        public static string DataWorkSpace;
        public static bool isPlaying = false;
        private static string testerType;
        private static string testerID;
        private static string videoNum;

        public static string TesterType
        {
            get { return testerType; }
            set { testerType = value; }
        }

        public static string TesterID
        {
            get { return testerID; }
            set { testerID = value; }
        }

        public static string VideoNum
        {
            get { return videoNum; }
            set { videoNum = value; }
        }
    }
}
