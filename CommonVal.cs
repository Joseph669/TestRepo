using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MainInterface
{
    public static class CommonVal  // static 不是必须
    {
        public static List<short> m_shECGList = new List<short>();
        public static string Workspace;
        private static string testerType;
        private static string testerID;
        private static string dataType;

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

        public static string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }
    }
}
